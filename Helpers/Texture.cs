using MelonLoader;
using MelonLoader.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Playables;
using static CreativeMode.ManageFiles;
using static MapLoader;

namespace CreativeMode.Helpers
{
    internal class Texture
    {
        public static readonly Dictionary<string, Material> s_materialCache = new();

        public static Material[] GetTextureForBlock(string blockName, bool isSlab, string properties)
        {
            var Mat = new Material[6];
            var ModelPath = ModelFolder();
            string jsonFilePath = Path.Combine(ModelPath, blockName + ".json");

            if (!File.Exists(jsonFilePath))
            {
                MelonLogger.Warning($"Block model JSON not found: {jsonFilePath}");
                return new Material[6]; // Return empty materials array
            }

            try
            {
                string json = File.ReadAllText(jsonFilePath);
                BlockModel model = JsonConvert.DeserializeObject<BlockModel>(json);

                if (model == null || model.textures == null)
                {
                    MelonLogger.Warning($"Failed to deserialize block model: {jsonFilePath}");
                    return new Material[6];
                }

                Dictionary<Face, string> faces = GetFaceTextures(model, properties);

                foreach (KeyValuePair<Face, string> entry in faces)
                {


                    string texturePath = entry.Value;
                    if (string.IsNullOrEmpty(texturePath))
                    {
                        MelonLogger.Warning($"Texture path is null or empty for face {entry.Key} of block {blockName}");
                        Mat[(int)entry.Key] = null;
                        continue;
                    }

                    if (isSlab && !(entry.Key == Face.Top || entry.Key == Face.Bottom))
                    {
                        Mat[(int)entry.Key] = GetOrCreateMaterial(texturePath, new Vector2(1f,0.5f),entry.Key);
                    }
                    else
                        Mat[(int)entry.Key] = GetOrCreateMaterial(texturePath, Vector2.one, entry.Key);
                }
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"Error loading block textures for {blockName}: {ex.Message}");
            }

            return Mat;
        }

        private static Dictionary<Face, string> GetFaceTextures(BlockModel model, string properties)
        {
            Dictionary<Face, string> result = new Dictionary<Face, string>();

            if (model.parent == null)
            {
                MelonLogger.Warning("Block model parent is null");
                return result;
            }

            if (model.parent.Contains("cube_all"))
            {
                foreach (Face i in Enum.GetValues(typeof(Face)))
                    result[i] = model.textures.all ?? "";
            }
            else if (model.parent.Contains("cube_column"))
            {
                if (properties.Contains("'axis': y"))
                {
                    result[Face.Front] = model.textures.side ?? "";
                    result[Face.Back] = model.textures.side ?? "";
                    result[Face.Left] = model.textures.side ?? "";
                    result[Face.Right] = model.textures.side ?? "";
                    result[Face.Top] = model.textures.end ?? "";
                    result[Face.Bottom] = model.textures.end ?? "";
                }
                else if (properties.Contains("'axis': z"))
                {
                    result[Face.Front] = model.textures.end ?? "";
                    result[Face.Back] = model.textures.end ?? "";
                    result[Face.Left] = model.textures.side ?? "";
                    result[Face.Right] = model.textures.side ?? "";
                    result[Face.Top] = model.textures.side ?? "";
                    result[Face.Bottom] = model.textures.side ?? "";
                }
                else if (properties.Contains("'axis': x"))
                {
                    result[Face.Front] = model.textures.side ?? "";
                    result[Face.Back] = model.textures.side ?? "";
                    result[Face.Left] = model.textures.end ?? "";
                    result[Face.Right] = model.textures.end ?? "";
                    result[Face.Top] = model.textures.side ?? "";
                    result[Face.Bottom] = model.textures.side ?? "";
                }

            }
            else if (model.parent.Contains("orientable"))
            {
                result[Face.Front] = model.textures.front ?? "";
                result[Face.Back] = model.textures.side ?? "";
                result[Face.Left] = model.textures.side ?? "";
                result[Face.Right] = model.textures.side ?? "";
                result[Face.Top] = model.textures.top ?? "";
                result[Face.Bottom] = model.textures.side ?? "";
            }
            else if (model.parent.Contains("block/block"))
            {
                result[Face.Front] = model.textures.side ?? "";
                result[Face.Back] = model.textures.side ?? "";
                result[Face.Left] = model.textures.side ?? "";
                result[Face.Right] = model.textures.side ?? "";
                result[Face.Top] = model.textures.top ?? model.textures.side ?? model.textures.all ?? "";
                result[Face.Bottom] = model.textures.bottom ?? "";
            }
            else if (model.parent.Contains("block/slab"))
            {
                result[Face.Front] = model.textures.side ?? "";
                result[Face.Back] = model.textures.side ?? "";
                result[Face.Left] = model.textures.side ?? "";
                result[Face.Right] = model.textures.side ?? "";
                result[Face.Top] = model.textures.top ?? "";
                result[Face.Bottom] = model.textures.bottom ?? "";
            }
            else
            {
                MelonLogger.Warning($"Unknown block parent type: {model.parent}");
                // Fallback: try to use all available textures
                result[Face.Front] = model.textures.side ?? model.textures.all ?? "";
                result[Face.Back] = model.textures.side ?? model.textures.all ?? "";
                result[Face.Left] = model.textures.top ?? model.textures.all ?? "";
                result[Face.Right] = model.textures.top ?? model.textures.all ?? "";
                result[Face.Top] = model.textures.front ?? model.textures.side ?? model.textures.all ?? "";
                result[Face.Bottom] = model.textures.side ?? model.textures.all ?? "";
            }

            return result;
        }

        private static Material GetOrCreateMaterial(string path, Vector2 scale, Face face)
        {
            Regex regex = new Regex(@"block/(.*)"); // Extract filename without extension block/grass_block_side

            path = Path.Combine(BlockFolder(), regex.Match(path).Groups[1].Value); // Convert to full path without extension

            if (string.IsNullOrEmpty(path))
                return null;

            bool slab = scale != Vector2.one;
            if (s_materialCache.TryGetValue(path + slab + face, out var mat))
                return mat;

            var loaded = LoadTexture(path, scale,face);
            s_materialCache[path + slab + face] = loaded;
            return loaded;
        }

        public static Material LoadTexture(string path, Vector2 scale,Face face)
        {
            if (string.IsNullOrEmpty(path))
            {
                MelonLogger.Warning("Texture path is null or empty");
                return null;
            }

            string Modpath = Path.Combine(BlockFolder(), path + ".png");
            if (!File.Exists(Modpath))
            {
                MelonLogger.Warning($"Texture file not found: {Modpath}");
                return null;
            }

            try
            {
                byte[] fileData = File.ReadAllBytes(Modpath);
                Texture2D texture = new Texture2D(2, 2);
                texture.LoadImage(fileData);
                texture.filterMode = FilterMode.Point;
                texture.wrapMode = TextureWrapMode.Repeat;

                Material mat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));

                float brightness = 1.0f;
                switch (face)
                {
                    case Face.Front:
                        brightness = 0.9f;
                        break;
                    case Face.Back:
                        brightness = 0.7f;
                        break;
                    case Face.Left:
                        brightness = 0.8f;
                        break;
                    case Face.Right:
                        brightness = 0.85f;
                        break;
                    case Face.Top:
                        brightness = 1.0f;
                        break;
                    case Face.Bottom:
                        brightness = 0.7f;
                        break;
                }


                mat.SetColor("_BaseColor", new Color(brightness, brightness, brightness, 1f));
                mat.enableInstancing = true;
                mat.mainTexture = texture;
                mat.mainTextureScale = scale;

                return mat;
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"Error loading texture {Modpath}: {ex.Message}");
                return null;
            }
        }
    }

    [System.Serializable]
    public class BlockModel
    {
        public string parent;
        public TextureData textures;
    }

    [System.Serializable]
    public class TextureData
    {
        public string all;
        public string side;
        public string top;
        public string end;
        public string front;
        public string bottom;
    }
    public enum Face
    {
        Front = 0,
        Back = 1,
        Left = 2,
        Right = 3,
        Top = 4,
        Bottom = 5
    }
}
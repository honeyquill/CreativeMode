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

                Dictionary<string, string> faces = GetFaceTextures(model, properties);

                for (int i = 0; i < 6; i++)
                {
                    // Ensure face texture exists in dictionary
                    if (!faces.ContainsKey(i.ToString()))
                    {
                        MelonLogger.Warning($"Face {i} texture not found for block {blockName}");
                        Mat[i] = null;
                        continue;
                    }

                    string texturePath = faces[i.ToString()];
                    if (string.IsNullOrEmpty(texturePath))
                    {
                        MelonLogger.Warning($"Texture path is null or empty for face {i} of block {blockName}");
                        Mat[i] = null;
                        continue;
                    }

                    if (isSlab && (i == 2 || i == 3)) // top and bottom faces of a slab should use the side texture
                    {
                        Mat[i] = GetOrCreateMaterial(texturePath, new Vector2(1, 0.5f), i);
                    }
                    else
                        Mat[i] = GetOrCreateMaterial(texturePath, Vector2.one, i);
                }
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"Error loading block textures for {blockName}: {ex.Message}");
            }

            return Mat;
        }

        private static Dictionary<string, string> GetFaceTextures(BlockModel model,string properties)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();

            if (model.parent == null)
            {
                MelonLogger.Warning("Block model parent is null");
                return result;
            }

            if (model.parent.Contains("cube_all"))
            {
                for (int i = 0; i < 6; i++)
                    result[i.ToString()] = model.textures.all ?? "";
            }
            else if (model.parent.Contains("cube_column"))
            {
                if (properties.Contains("'axis': y"))
                {
                    result["0"] = model.textures.side ?? "";
                    result["1"] = model.textures.side ?? "";
                    result["2"] = model.textures.side ?? "";
                    result["3"] = model.textures.side ?? "";
                    result["4"] = model.textures.end ?? "";
                    result["5"] = model.textures.end ?? "";
                }
                else if (properties.Contains("'axis': z"))
                {
                    result["0"] = model.textures.end ?? "";
                    result["1"] = model.textures.end ?? "";
                    result["2"] = model.textures.side ?? "";
                    result["3"] = model.textures.side ?? "";
                    result["4"] = model.textures.side ?? "";
                    result["5"] = model.textures.side ?? "";
                }
                else if (properties.Contains("'axis': x"))
                {
                    result["0"] = model.textures.side ?? "";
                    result["1"] = model.textures.side ?? "";
                    result["2"] = model.textures.end ?? "";
                    result["3"] = model.textures.end ?? "";
                    result["4"] = model.textures.side ?? "";
                    result["5"] = model.textures.side ?? "";
                }

            }
            else if (model.parent.Contains("orientable"))
            {
                result["4"] = model.textures.front ?? ""; // front
                result["5"] = model.textures.side ?? "";  // back

                result["0"] = model.textures.side ?? "";
                result["1"] = model.textures.side ?? "";

                result["2"] = model.textures.top ?? "";
                result["3"] = model.textures.top ?? "";
            }
            else if (model.parent.Contains("block/block"))
            {
                result["0"] = model.textures.side ?? "";
                result["1"] = model.textures.side ?? "";
                result["2"] = model.textures.side  ?? "";
                result["3"] = model.textures.side  ?? "";
                result["4"] = model.textures.top ?? model.textures.side ?? model.textures.all ?? "";
                result["5"] = model.textures.bottom ?? "";
            }
            else if (model.parent.Contains("block/slab"))
            {
                result["0"] = model.textures.side ?? "";
                result["1"] = model.textures.side ?? "";
                result["2"] = model.textures.side ?? "";
                result["3"] = model.textures.side ?? "";
                result["4"] = model.textures.top ?? "";
                result["5"] = model.textures.bottom ?? "";
            }
            else
            {
                MelonLogger.Warning($"Unknown block parent type: {model.parent}");
                // Fallback: try to use all available textures
                result["0"] = model.textures.side ?? model.textures.all ?? "";
                result["1"] = model.textures.side ?? model.textures.all ?? "";
                result["2"] = model.textures.top ?? model.textures.all ?? "";
                result["3"] = model.textures.top ?? model.textures.all ?? "";
                result["4"] = model.textures.front ?? model.textures.side ?? model.textures.all ?? "";
                result["5"] = model.textures.side ?? model.textures.all ?? "";
            }

            return result;
        }

        private static Material GetOrCreateMaterial(string path, Vector2 scale, int side)
        {
            Regex regex = new Regex(@"block/(.*)"); // Extract filename without extension block/grass_block_side

            path = Path.Combine(BlockFolder(), regex.Match(path).Groups[1].Value); // Convert to full path without extension

            if (string.IsNullOrEmpty(path))
                return null;

            bool slab = scale != Vector2.one;
            if (s_materialCache.TryGetValue(path + slab + side, out var mat))
                return mat;

            var loaded = LoadTexture(path, scale, side);
            s_materialCache[path + slab + side] = loaded;
            return loaded;
        }

        public static Material LoadTexture(string path, Vector2 scale, int side)
        {
            if (string.IsNullOrEmpty(path))
            {
                MelonLogger.Warning("Texture path is null or empty");
                return null;
            }

            string Modpath = Path.Combine(BlockFolder(), path+".png");
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
                mat.SetColor("_BaseColor", new Color(0.8f, 0.8f, 0.8f, 1f));
                mat.enableInstancing = true;
                mat.mainTexture = texture;
                mat.mainTextureScale = scale;
                
                switch (side)
                {
                    case 4: // Top
                        mat.SetColor("_BaseColor", new Color(1.2f, 1.2f, 1.2f, 1f));
                        break;
                    case 5: // Bottom
                        mat.SetColor("_BaseColor", new Color(0.6f, 0.6f, 0.6f, 1f));
                        break;
                    default:
                        break;
                }

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
}

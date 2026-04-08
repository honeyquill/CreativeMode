using Il2Cpp;
using MelonLoader.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CreativeMode.Helpers
{
    internal class BlockPlacer
    {
        public static Material LoadTexture(string path)
        {
            string Modpath = Path.Combine(MelonEnvironment.ModsDirectory, "Blocks", path);
            if (!File.Exists(Modpath)) 
            { 
                BeetleUtils.SendChatMessage($"File not found: {Modpath}");
                return null;
            }
            byte[] fileData = File.ReadAllBytes(Modpath); 
            Texture2D texture = new Texture2D(2, 2); // size gets replaced
            texture.LoadImage(fileData); // loads PNG/JPG 
            Material mat = new Material(Shader.Find("Shader Graphs/EgyptTilesShader"));
            texture.filterMode = FilterMode.Point; // no blur
            texture.wrapMode = TextureWrapMode.Clamp;

            mat.mainTexture = texture; 
            return mat; 
        }

        public static Vector3 Grid(Vector3 original, float gridSize)
        {
            return new Vector3(
                Mathf.Round(original.x / gridSize) * gridSize,
                Mathf.Round(original.y / gridSize) * gridSize,
                Mathf.Round(original.z / gridSize) * gridSize
            );
        }

        public static void PlaceBlock(string path, float size, Vector3 pos, bool? grid = true)
        {
            Material mat = LoadTexture(path);

            // Snap the position to the nearest grid point
            Vector3 p1 = Grid(pos, size);
            Vector3 p2 = Grid(pos, size);

            Vector3 gridP1 = p1 + new Vector3(size / 2, size / 2, size / 2);
            Vector3 gridP2 = p2 - new Vector3(size / 2, size / 2, size / 2);

            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.GetComponent<Renderer>().material = mat;

            // Center = midpoint between gridP1 and gridP2
            cube.transform.position = (gridP1 + gridP2) / 2f;

            // Size = difference
            cube.transform.localScale = new Vector3(
                Mathf.Abs(gridP2.x - gridP1.x),
                Mathf.Abs(gridP2.y - gridP1.y),
                Mathf.Abs(gridP2.z - gridP1.z)
            );
        }
    }
}

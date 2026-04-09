using Il2Cpp;
using MelonLoader.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;

namespace CreativeMode.Helpers
{
    internal class BlockPlacer
    {
        // Caches and pool to avoid expensive allocations at spawn time
        private static readonly Dictionary<string, Material> s_materialCache = new();
        private static readonly Stack<GameObject> s_pool = new();
        private static readonly List<GameObject> s_active = new();
        private static Mesh s_sharedCubeMesh;

        public static Material LoadTexture(string path)
        {
            string Modpath = Path.Combine(MelonEnvironment.ModsDirectory, "CreativeMode/Blocks", path);
            if (!File.Exists(Modpath))
            {
                return null;
            }
            byte[] fileData = File.ReadAllBytes(Modpath);
            Texture2D texture = new Texture2D(2, 2); 
            texture.LoadImage(fileData); 
            texture.filterMode = FilterMode.Point; 
            texture.wrapMode = TextureWrapMode.Clamp;

            Material mat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            mat.SetColor("_BaseColor", new Color(0.8f, 0.8f, 0.8f, 1f));
            mat.enableInstancing = true;
            mat.mainTexture = texture;
            return mat;
        }

        // Returns a material for the exact path or null if not found.
        private static Material GetMaterialIfExists(string path)
        {
            if (s_materialCache.TryGetValue(path, out var mat))
                return mat;

            var loaded = LoadTexture(path);
            if (loaded != null)
                s_materialCache[path] = loaded;

            return loaded;
        }

        // Returns a material, falling back to a magenta fallback when path is missing or invalid.
        private static Material GetOrCreateMaterial(string path)
        {

            if (s_materialCache.TryGetValue(path, out var mat))
                return mat;

            var loaded = LoadTexture(path);
            s_materialCache[path] = loaded;
            return loaded;
        }

        public static Vector3 Grid(Vector3 original, float gridSize)
        {
            return new Vector3(
                Mathf.Round(original.x / gridSize) * gridSize,
                Mathf.Round(original.y / gridSize) * gridSize,
                Mathf.Round(original.z / gridSize) * gridSize
            );
        }

        // Builds a single shared subdivided cube mesh (UVs, normals, submeshes) once.
        private static Mesh GetSharedCubeMesh()
        {
            if (s_sharedCubeMesh != null) return s_sharedCubeMesh;

            Mesh mesh = new Mesh();
            mesh.name = "Shared_3_Side_Cube";

            Vector3[] vertices = {
        // Front
        new Vector3(-0.5f,-0.5f,0.5f),
        new Vector3(0.5f,-0.5f,0.5f),
        new Vector3(0.5f,0.5f,0.5f),
        new Vector3(-0.5f,0.5f,0.5f),
        // Back
        new Vector3(0.5f,-0.5f,-0.5f),
        new Vector3(-0.5f,-0.5f,-0.5f),
        new Vector3(-0.5f,0.5f,-0.5f),
        new Vector3(0.5f,0.5f,-0.5f),
        // Left
        new Vector3(-0.5f,-0.5f,-0.5f),
        new Vector3(-0.5f,-0.5f,0.5f),
        new Vector3(-0.5f,0.5f,0.5f),
        new Vector3(-0.5f,0.5f,-0.5f),
        // Right
        new Vector3(0.5f,-0.5f,0.5f),
        new Vector3(0.5f,-0.5f,-0.5f),
        new Vector3(0.5f,0.5f,-0.5f),
        new Vector3(0.5f,0.5f,0.5f),
        // Top
        new Vector3(-0.5f,0.5f,0.5f),
        new Vector3(0.5f,0.5f,0.5f),
        new Vector3(0.5f,0.5f,-0.5f),
        new Vector3(-0.5f,0.5f,-0.5f),
        // Bottom
        new Vector3(-0.5f,-0.5f,-0.5f),
        new Vector3(0.5f,-0.5f,-0.5f),
        new Vector3(0.5f,-0.5f,0.5f),
        new Vector3(-0.5f,-0.5f,0.5f)
    };

            mesh.vertices = vertices;

            // 3 submeshes: sides, top, bottom
            mesh.subMeshCount = 3;

            int[] sideTriangles = {
        0,1,2, 0,2,3,      // Front
        4,5,6, 4,6,7,      // Back
        8,9,10, 8,10,11,   // Left
        12,13,14, 12,14,15 // Right
    };
            int[] topTriangles = { 16, 17, 18, 16, 18, 19 };
            int[] bottomTriangles = { 20, 21, 22, 20, 22, 23 };

            mesh.SetTriangles(sideTriangles, 0);
            mesh.SetTriangles(topTriangles, 1);
            mesh.SetTriangles(bottomTriangles, 2);

            // UVs
            Vector2[] uvs = new Vector2[24];
            for (int i = 0; i < 6; i++)
            {
                uvs[i * 4 + 0] = new Vector2(0f, 0f);
                uvs[i * 4 + 1] = new Vector2(1f, 0f);
                uvs[i * 4 + 2] = new Vector2(1f, 1f);
                uvs[i * 4 + 3] = new Vector2(0f, 1f);
            }
            mesh.uv = uvs;

            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            s_sharedCubeMesh = mesh;
            return s_sharedCubeMesh;
        }

        // Pooling helper
        private static GameObject RentBlockObject()
        {
            if (s_pool.Count > 0)
            {
                var go = s_pool.Pop();
                go.SetActive(true);

                // Ensure collider exists and reset it
                var bc = go.GetComponent<BoxCollider>();
                if (bc == null) bc = go.AddComponent<BoxCollider>();
                bc.center = Vector3.zero;
                bc.size = Vector3.one;
                bc.isTrigger = false;

                return go;
            }

            var obj = new GameObject("Block");
            obj.AddComponent<MeshFilter>();
            obj.AddComponent<MeshRenderer>();
            obj.GetComponent<MeshRenderer>().shadowCastingMode = ShadowCastingMode.Off; //disable shadows by default for performance
            var collider = obj.AddComponent<BoxCollider>();
            collider.center = Vector3.zero;
            collider.size = Vector3.one;
            collider.isTrigger = false;
            return obj;
        }

        private static void ReturnBlockObject(GameObject go)
        {
            go.SetActive(false);
            s_pool.Push(go);
        }

        // Main spawn path uses cached materials + shared mesh + pooling.
        public static void PlaceBlock(string path, float size, Vector3 pos, bool? grid = true)
        {
            // Acquire materials (cached). Side material must exist or fallback.
            Material sideMat = GetOrCreateMaterial(path);

            // Top/bottom: prefer explicit textures, but fall back to side material if they don't exist.
            Material topMat = GetMaterialIfExists(path?.Replace(".png", "_top.png")) ?? sideMat;
            Material bottomMat = GetMaterialIfExists(path?.Replace(".png", "_bottom.png")) ?? topMat ?? sideMat;

            // Snap the position to the nearest grid point
            Vector3 gridCenter = Grid(pos, size);

            Vector3 gridP1 = gridCenter + new Vector3(size / 2, size / 2, size / 2);
            Vector3 gridP2 = gridCenter - new Vector3(size / 2, size / 2, size / 2);

            GameObject cube = RentBlockObject();

            // Assign shared mesh (no mesh allocations)
            var mf = cube.GetComponent<MeshFilter>();
            mf.sharedMesh = GetSharedCubeMesh();

            var mr = cube.GetComponent<MeshRenderer>();
            // Use sharedMaterials to avoid instantiating new material copies for renderer
            mr.sharedMaterials = new Material[]
            {
        sideMat,  // sides
        topMat,   // top
        bottomMat // bottom
            };

            // Reduce renderer overhead
            mr.shadowCastingMode = ShadowCastingMode.Off;
            mr.receiveShadows = false;

            // Set transform
            cube.transform.position = (gridP1 + gridP2) / 2f;
            cube.transform.rotation = Quaternion.identity;
            cube.transform.localScale = new Vector3(
                Mathf.Abs(gridP2.x - gridP1.x),
                Mathf.Abs(gridP2.y - gridP1.y),
                Mathf.Abs(gridP2.z - gridP1.z)
            );

            // Ensure collider fits: keep collider size at (1,1,1) and rely on transform scale to size it.
            var box = cube.GetComponent<BoxCollider>();
            if (box == null) box = cube.AddComponent<BoxCollider>();
            box.center = Vector3.zero;
            box.size = Vector3.one;
            box.isTrigger = false;

            // Track for removal
            s_active.Add(cube);
        }

        // Finds nearest active block at approximate location and returns it to the pool.
        public static void RemoveBlock(Vector3 pos)
        {
            const float eps = 0.01f;
            GameObject found = null;
            for (int i = 0; i < s_active.Count; i++)
            {
                var go = s_active[i];
                if (Vector3.Distance(go.transform.position, pos) <= eps)
                {
                    found = go;
                    s_active.RemoveAt(i);
                    break;
                }
            }

            if (found != null)
            {
                ReturnBlockObject(found);
            }
            else
            {
                // If nothing matched exactly, try to remove the closest within a larger radius
                float bestDist = float.MaxValue;
                int bestIdx = -1;
                for (int i = 0; i < s_active.Count; i++)
                {
                    var d = Vector3.Distance(s_active[i].transform.position, pos);
                    if (d < bestDist)
                    {
                        bestDist = d;
                        bestIdx = i;
                    }
                }

                if (bestIdx >= 0 && bestDist <= 1.0f)
                {
                    var go = s_active[bestIdx];
                    s_active.RemoveAt(bestIdx);
                    ReturnBlockObject(go);
                }
            }
        }
    }
}

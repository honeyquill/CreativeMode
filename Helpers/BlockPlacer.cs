using Il2Cpp;
using MelonLoader.Utils;
using Newtonsoft.Json.Serialization;
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
        public static readonly Dictionary<string, Material> s_materialCache = new();
        private static readonly Stack<GameObject> s_pool = new();
        private static readonly List<GameObject> s_active = new();
        public static readonly Dictionary<int, Mesh> s_meshCache = new();

        public static Material LoadTexture(string path,Vector2 scale,int side)
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
            texture.wrapMode = TextureWrapMode.Repeat;
            

            Material mat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            mat.SetColor("_BaseColor", new Color(0.8f, 0.8f, 0.8f, 1f));
            mat.enableInstancing = true;
            mat.mainTexture = texture;
            mat.mainTextureScale = scale;
            switch (side)
            {
                case 1: // Top
                    mat.SetColor("_BaseColor", new Color(1.2f, 1.2f, 1.2f, 1f));
                    break;
                case 2: // Bottom
                    mat.SetColor("_BaseColor", new Color(0.6f, 0.6f, 0.6f, 1f));
                    break;
                default:
                    break;
            } 

            return mat;
        }
        
        // Returns a material, falling back to the fallback when path is missing or invalid.
        private static Material GetOrCreateMaterial(string path, Vector2 scale, int side)
        {
            bool slab = scale != Vector2.one;
            if (s_materialCache.TryGetValue(path + slab + side, out var mat))
                return mat;

            var loaded = LoadTexture(path, scale,side);
            if (loaded == null)
            {
                s_materialCache[path + slab + side] = null;
                return null;
            }

            s_materialCache[path + slab + side] = loaded;
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

        // Prebuilt canonical vertices used for all mesh variants (24 verts)
        private static readonly Vector3[] s_cubeVertices = {
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

        // Map for canonical triangle sets
        private static readonly int[] s_sideTriangles = {
            0,1,2, 0,2,3,      // Front
            4,5,6, 4,6,7,      // Back
            8,9,10, 8,10,11,   // Left
            12,13,14, 12,14,15 // Right
        };
        private static readonly int[] s_topTriangles = { 16, 17, 18, 16, 18, 19 };
        private static readonly int[] s_bottomTriangles = { 20, 21, 22, 20, 22, 23 };

        // Creates or returns a cached mesh that contains only the enabled submeshes.
        // mask bits: 1 = sides, 2 = top, 4 = bottom
        private static Mesh GetSharedMeshForMask(int mask)
        {
            if (s_meshCache.TryGetValue(mask, out var cached))
                return cached;

            Mesh mesh = new Mesh();
            mesh.name = "Shared_Mesh_mask_" + mask;
            mesh.vertices = s_cubeVertices;

            // Count enabled submeshes
            int subCount = 0;
            if ((mask & 1) != 0) subCount++;
            if ((mask & 2) != 0) subCount++;
            if ((mask & 4) != 0) subCount++;

            mesh.subMeshCount = Math.Max(1, subCount); // ensure at least 1 to avoid errors

            int subIndex = 0;
            if ((mask & 1) != 0)
            {
                mesh.SetTriangles(s_sideTriangles, subIndex++);
            }
            if ((mask & 2) != 0)
            {
                mesh.SetTriangles(s_topTriangles, subIndex++);
            }
            if ((mask & 4) != 0)
            {
                mesh.SetTriangles(s_bottomTriangles, subIndex++);
            }

            // UVs (same canonical mapping)
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

            s_meshCache[mask] = mesh;
            return mesh;
        }

        // Pooling helper
        private static GameObject RentBlockObject(Vector3 size)
        {
            if (s_pool.Count > 0)
            {
                var go = s_pool.Pop();
                go.SetActive(true);

                // Ensure collider exists and reset it
                var bc = go.GetComponent<BoxCollider>();
                if (bc == null) bc = go.AddComponent<BoxCollider>();
                bc.center = Vector3.zero;
                bc.size = size;
                bc.isTrigger = false;

                return go;
            }

            var obj = new GameObject("Block");
            obj.AddComponent<MeshFilter>();
            obj.AddComponent<MeshRenderer>();
            obj.GetComponent<MeshRenderer>().shadowCastingMode = ShadowCastingMode.Off; //disable shadows by default for performance
            var collider = obj.AddComponent<BoxCollider>();
            collider.center = Vector3.zero;
            collider.size = Vector3.one*1.01f;
            collider.isTrigger = false;
            return obj;
        }

        private static void ReturnBlockObject(GameObject go)
        {
            go.SetActive(false);
            s_pool.Push(go);
        }

        public static Material GetMaterialByType(string path, Vector2 size, int side)
        {
            switch (side)
            {
                case 0: // Side
                    return GetOrCreateMaterial(path, size, side) ?? GetOrCreateMaterial(path.Replace(".png", "s.png"), size, side);
                case 1: // Top
                    return GetOrCreateMaterial(path?.Replace(".png", "_top.png"), Vector2.one, side) ?? GetOrCreateMaterial(path.Replace(".png", "s.png").Replace(".png", "_top.png"), Vector2.one, side) ?? GetOrCreateMaterial(path, Vector2.one, side) ?? GetOrCreateMaterial(path.Replace(".png", "s.png"), Vector2.one, side);
                case 2: // Bottom
                    return GetOrCreateMaterial(path?.Replace(".png", "_bottom.png"), Vector2.one, side) ?? GetOrCreateMaterial(path.Replace(".png", "s.png").Replace(".png", "_bottom.png"), Vector2.one, side) ?? GetOrCreateMaterial(path, Vector2.one, side) ?? GetOrCreateMaterial(path.Replace(".png", "s.png"), Vector2.one, side) ?? GetMaterialByType(path, Vector2.one, 2);
                default:
                    return null;
            }
        }



        public static void PlaceBlock(string path, float size, Vector3 pos, bool renderSides = true, bool renderTop = true, bool renderBottom = true, string properties = "")
        {
            bool slab;
            string realpath = path.Replace("_slab", "");
            if (realpath != path) slab = true; else slab = false;
            if(properties.Contains("double")) slab = false;
            Material sideMat;
            if (slab)
            {
                sideMat = GetMaterialByType(realpath, new Vector2(1, 0.5f),0);
            }
            else
            {
                sideMat = GetMaterialByType(realpath, Vector2.one, 0);
            }


            Material topMat = GetMaterialByType(realpath, new Vector2(1, 1f), 1);
            Material bottomMat = GetMaterialByType(realpath, new Vector2(1, 1f), 2);

            // Snap the position to the nearest grid point
            Vector3 gridCenter = Grid(pos, size);

            Vector3 gridP1 = gridCenter + new Vector3(size / 2, size / 2, size / 2);
            Vector3 gridP2 = gridCenter - new Vector3(size / 2, size / 2, size / 2);

            GameObject cube = RentBlockObject(new Vector3(1,0.5f,1));

            // Determine mesh mask and materials order. Submesh order is: sides (if present), top (if present), bottom (if present)
            int mask = 0;
            if (renderSides) mask |= 1;
            if (renderTop) mask |= 2;
            if (renderBottom) mask |= 4;

            // Ensure at least one face is rendered; if none are requested, render sides as fallback for visibility/perf predictability
            if (mask == 0) mask = 1;

            var mf = cube.GetComponent<MeshFilter>();
            mf.sharedMesh = GetSharedMeshForMask(mask);

            var mr = cube.GetComponent<MeshRenderer>();

            var materials = new List<Material>(3);
            if ((mask & 1) != 0) materials.Add(sideMat);
            if ((mask & 2) != 0) materials.Add(topMat);
            if ((mask & 4) != 0) materials.Add(bottomMat);

            mr.sharedMaterials = materials.ToArray();

            // Reduce renderer overhead
            mr.shadowCastingMode = ShadowCastingMode.Off;
            mr.receiveShadows = false;

            // Set transform
            cube.transform.position = (gridP1 + gridP2) / 2f;
            if (slab && properties.Contains("bottom")) cube.transform.position += new Vector3(0, -size * 0.5f, 0); // adjust for slab height
            cube.transform.rotation = Quaternion.identity;
            if (slab)
            {
                gridP1.y *= 0.5f;
                gridP2.y *= 0.5f;
            }
            cube.transform.localScale = new Vector3(
                Mathf.Abs(gridP2.x - gridP1.x),
                Mathf.Abs(gridP2.y - gridP1.y),
                Mathf.Abs(gridP2.z - gridP1.z)
            );


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

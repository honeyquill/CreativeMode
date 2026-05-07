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
using static MapLoader;
using static CreativeMode.SpecialBlocks.SpecialBlocks;
using static CreativeMode.Helpers.Texture;

namespace CreativeMode.Helpers
{
    internal class BlockPlacer
    {
        // Caches and pool to avoid expensive allocations at spawn time
        public static readonly Stack<GameObject> s_pool = new();
        public static readonly List<GameObject> s_active = new();
        public static readonly Dictionary<int, Mesh> s_meshCache = new();

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

        // Map for canonical triangle sets (6 faces)
        private static readonly int[] s_frontTriangles = { 0, 1, 2, 0, 2, 3 };
        private static readonly int[] s_backTriangles = { 4, 5, 6, 4, 6, 7 };
        private static readonly int[] s_leftTriangles = { 8, 9, 10, 8, 10, 11 };
        private static readonly int[] s_rightTriangles = { 12, 13, 14, 12, 14, 15 };
        private static readonly int[] s_topTriangles = { 16, 17, 18, 16, 18, 19 };
        private static readonly int[] s_bottomTriangles = { 20, 21, 22, 20, 22, 23 };

        // Creates or returns a cached mesh that contains only the enabled submeshes.
        // mask bits: 1 = front, 2 = back, 4 = left, 8 = right, 16 = top, 32 = bottom
        private static Mesh GetSharedMeshForMask(int mask)
        {
            if (s_meshCache.TryGetValue(mask, out var cached))
                return cached;

            Mesh mesh = new Mesh();
            mesh.name = "Shared_Mesh_mask_" + mask;
            mesh.vertices = s_cubeVertices;

            // Count enabled submeshes
            int subCount = 0;
            if ((mask & 1) != 0) subCount++;   // front
            if ((mask & 2) != 0) subCount++;   // back
            if ((mask & 4) != 0) subCount++;   // left
            if ((mask & 8) != 0) subCount++;   // right
            if ((mask & 16) != 0) subCount++;  // top
            if ((mask & 32) != 0) subCount++;  // bottom

            mesh.subMeshCount = Math.Max(1, subCount); // ensure at least 1 to avoid errors

            int subIndex = 0;
            if ((mask & 1) != 0) mesh.SetTriangles(s_frontTriangles, subIndex++);
            if ((mask & 2) != 0) mesh.SetTriangles(s_backTriangles, subIndex++);
            if ((mask & 4) != 0) mesh.SetTriangles(s_leftTriangles, subIndex++);
            if ((mask & 8) != 0) mesh.SetTriangles(s_rightTriangles, subIndex++);
            if ((mask & 16) != 0) mesh.SetTriangles(s_topTriangles, subIndex++);
            if ((mask & 32) != 0) mesh.SetTriangles(s_bottomTriangles, subIndex++);

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
        private static GameObject RentBlockObject(Vector3 size)
        {
            if (s_pool.Count > 0)
            {
                var go = s_pool.Pop();
                go.SetActive(true);

                // Get all components we need in ONE pass (not multiple GetComponent calls)
                var mf = go.GetComponent<MeshFilter>();
                var mr = go.GetComponent<MeshRenderer>();
                var bc = go.GetComponent<BoxCollider>();

                // CRITICAL FIX: Collider already exists, just reset its size
                // Don't call AddComponent again - that creates a NEW collider and leaks the old one
                if (bc != null)
                {
                    bc.center = Vector3.zero;
                    bc.size = size;
                    bc.isTrigger = false;
                }

                return go;
            }

            // First time creation - set up the GameObject with all components it will ever need
            var obj = new GameObject("Block");

            // Add components once - they will be reused forever
            var meshFilter = obj.AddComponent<MeshFilter>();
            var meshRenderer = obj.AddComponent<MeshRenderer>();
            var boxCollider = obj.AddComponent<BoxCollider>();

            // Configure renderer for performance
            meshRenderer.shadowCastingMode = ShadowCastingMode.Off; // disable shadows by default
            meshRenderer.receiveShadows = false; // blocks don't receive shadows

            // Configure collider
            boxCollider.center = Vector3.zero;
            boxCollider.size = size;
            boxCollider.isTrigger = false;

            return obj;
        }
        public static void ReturnBlockToPool(GameObject block)
        {
            if (block == null)
                return;

            // Disable the object
            block.SetActive(false);

            // Clear references to prevent memory leaks
            var mf = block.GetComponent<MeshFilter>();
            if (mf != null)
                mf.sharedMesh = null;

            var mr = block.GetComponent<MeshRenderer>();
            if (mr != null)
                mr.sharedMaterials = new Material[0];

            // Return to pool for reuse
            s_pool.Push(block);
        }

 
        public static void ClearAllBlocks()
        {
            for (int i = s_active.Count - 1; i >= 0; i--)
            {
                ReturnBlockToPool(s_active[i]);
            }
            s_active.Clear();
        }

        public static void PlaceBlock(BlockData block, GameObject parent)
        {
            float size = 5f;

            bool slab;
            if (block.path.Contains("slab")) slab = true; else slab = false;
            if (block.properties.Contains("double")) slab = false;

            Material[] materials = GetTextureForBlock(block.path, slab, block.properties);

            Vector3 pos = BlockDataToVector3(block);

            // Snap the position to the nearest grid point
            Vector3 gridCenter = Grid(pos, size);

            Vector3 gridP1 = gridCenter + new Vector3(size / 2, size / 2, size / 2);
            Vector3 gridP2 = gridCenter - new Vector3(size / 2, size / 2, size / 2);

            // Rent from pool instead of creating new
            GameObject cube = RentBlockObject(new Vector3(1, 0.5f, 1));
            cube.transform.SetParent(parent.transform);

            // Set mesh (cached)
            var mf = cube.GetComponent<MeshFilter>();
            mf.sharedMesh = GetSharedMeshForMask(63);

            // Set materials
            var mr = cube.GetComponent<MeshRenderer>();
            mr.sharedMaterials = materials.ToArray();

            // Reduce renderer overhead
            mr.shadowCastingMode = ShadowCastingMode.Off;
            mr.receiveShadows = false;

            // Set transform
            cube.transform.position = (gridP1 + gridP2) / 2f;
            if (slab)
                if (block.properties.Contains("bottom"))
                    cube.transform.position += new Vector3(0, -size * 0.25f, 0);
                else
                    cube.transform.position += new Vector3(0, size * 0.25f, 0);
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

            // Collider is already configured by RentBlockObject
            var box = cube.GetComponent<BoxCollider>();
            if (box != null)
            {
                box.center = Vector3.zero;
                box.size = Vector3.one;
                box.isTrigger = false;
            }

            // Track for removal
            s_active.Add(cube);
        }
    }
}

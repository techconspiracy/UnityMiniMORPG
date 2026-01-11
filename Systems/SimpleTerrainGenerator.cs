using UnityEngine;
using System.Collections.Generic;

namespace Game.Core.Systems
{
    /// <summary>
    /// Simple procedural terrain generator using Perlin noise.
    /// Generates rolling hills with biome-specific features.
    /// Optimized for low-end hardware (40fps target).
    /// </summary>
    public class SimpleTerrainGenerator : MonoBehaviour
    {
        [Header("Terrain Settings")]
        [SerializeField, Tooltip("Resolution of terrain mesh (vertices per side)")]
        private int _terrainResolution = 50;
        
        [SerializeField, Tooltip("Height multiplier for terrain")]
        private float _heightScale = 10f;
        
        [SerializeField, Tooltip("Frequency of Perlin noise")]
        private float _noiseScale = 0.1f;
        
        [Header("Biome Colors")]
        [SerializeField] private Color _grasslandColor = new(0.3f, 0.6f, 0.2f);
        [SerializeField] private Color _desertColor = new(0.8f, 0.7f, 0.4f);
        [SerializeField] private Color _snowColor = new(0.9f, 0.9f, 1f);
        [SerializeField] private Color _lavaColor = new(0.8f, 0.2f, 0.1f);
        [SerializeField] private Color _corruptedColor = new(0.4f, 0.1f, 0.5f);
        
        // Cached material for terrain
        private Material _terrainMaterial;
        
        #region Terrain Generation
        
        /// <summary>
        /// Generate terrain for a zone with specified configuration.
        /// </summary>
        public async Awaitable GenerateTerrainForZone(Transform zoneRoot, ZoneConfig config)
        {
            Debug.Log($"[SimpleTerrainGenerator] Generating terrain for zone '{config.zoneName}'...");
            
            float startTime = Time.realtimeSinceStartup;
            
            // Create terrain container
            GameObject terrainObj = new("Terrain");
            terrainObj.transform.SetParent(zoneRoot);
            
            // Generate mesh
            Mesh terrainMesh = GenerateTerrainMesh(config);
            
            // Setup mesh components
            MeshFilter meshFilter = terrainObj.AddComponent<MeshFilter>();
            meshFilter.mesh = terrainMesh;
            
            MeshRenderer meshRenderer = terrainObj.AddComponent<MeshRenderer>();
            meshRenderer.material = GetBiomeMaterial(config.biomeType);
            
            // Add collider for walkability
            MeshCollider meshCollider = terrainObj.AddComponent<MeshCollider>();
            meshCollider.sharedMesh = terrainMesh;
            
            float elapsed = Time.realtimeSinceStartup - startTime;
            Debug.Log($"[SimpleTerrainGenerator] Terrain generated in {elapsed:F3}s");
            
            await Awaitable.NextFrameAsync();
        }
        
        private Mesh GenerateTerrainMesh(ZoneConfig config)
        {
            Mesh mesh = new();
            mesh.name = $"TerrainMesh_{config.zoneName}";
            
            int resolution = _terrainResolution;
            Vector3 size = config.zoneSize;
            
            // Calculate vertex count
            int vertexCount = resolution * resolution;
            Vector3[] vertices = new Vector3[vertexCount];
            Vector2[] uvs = new Vector2[vertexCount];
            Color[] colors = new Color[vertexCount];
            
            // Generate vertices with Perlin noise
            float stepX = size.x / (resolution - 1);
            float stepZ = size.z / (resolution - 1);
            
            for (int z = 0; z < resolution; z++)
            {
                for (int x = 0; x < resolution; x++)
                {
                    int index = z * resolution + x;
                    
                    // Position
                    float xPos = x * stepX - size.x / 2f;
                    float zPos = z * stepZ - size.z / 2f;
                    
                    // Height from Perlin noise
                    float noiseX = (xPos + config.seed) * _noiseScale;
                    float noiseZ = (zPos + config.seed) * _noiseScale;
                    float height = Mathf.PerlinNoise(noiseX, noiseZ) * _heightScale;
                    
                    // Apply biome-specific height modifications
                    height = ApplyBiomeModifier(height, config.biomeType);
                    
                    vertices[index] = new Vector3(xPos, height, zPos);
                    
                    // UVs
                    uvs[index] = new Vector2((float)x / (resolution - 1), (float)z / (resolution - 1));
                    
                    // Vertex colors (for visual variety)
                    colors[index] = GetBiomeColor(config.biomeType, height);
                }
            }
            
            // Generate triangles
            int triangleCount = (resolution - 1) * (resolution - 1) * 6;
            int[] triangles = new int[triangleCount];
            int triIndex = 0;
            
            for (int z = 0; z < resolution - 1; z++)
            {
                for (int x = 0; x < resolution - 1; x++)
                {
                    int topLeft = z * resolution + x;
                    int topRight = topLeft + 1;
                    int bottomLeft = (z + 1) * resolution + x;
                    int bottomRight = bottomLeft + 1;
                    
                    // First triangle
                    triangles[triIndex++] = topLeft;
                    triangles[triIndex++] = bottomLeft;
                    triangles[triIndex++] = topRight;
                    
                    // Second triangle
                    triangles[triIndex++] = topRight;
                    triangles[triIndex++] = bottomLeft;
                    triangles[triIndex++] = bottomRight;
                }
            }
            
            // Assign to mesh
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.uv = uvs;
            mesh.colors = colors;
            
            // Recalculate normals and bounds
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            
            return mesh;
        }
        
        #endregion
        
        #region Biome Modifiers
        
        private float ApplyBiomeModifier(float height, BiomeType biome)
        {
            return biome switch
            {
                BiomeType.Grassland => height, // Gentle rolling hills
                BiomeType.Desert => height * 0.5f, // Flatter terrain
                BiomeType.Snow => height * 1.5f, // More dramatic peaks
                BiomeType.Lava => height * 0.3f + Mathf.Abs(Mathf.Sin(height * 5f)) * 2f, // Rocky plateaus
                BiomeType.Corrupted => height + Mathf.PerlinNoise(height * 10f, height * 10f) * 3f, // Chaotic terrain
                _ => height
            };
        }
        
        private Color GetBiomeColor(BiomeType biome, float height)
        {
            Color baseColor = biome switch
            {
                BiomeType.Grassland => _grasslandColor,
                BiomeType.Desert => _desertColor,
                BiomeType.Snow => _snowColor,
                BiomeType.Lava => _lavaColor,
                BiomeType.Corrupted => _corruptedColor,
                _ => Color.gray
            };
            
            // Add height-based variation
            float variation = height / _heightScale;
            return Color.Lerp(baseColor * 0.7f, baseColor, variation);
        }
        
        private Material GetBiomeMaterial(BiomeType biome)
        {
            if (_terrainMaterial == null)
            {
                _terrainMaterial = new Material(Shader.Find("Standard"));
                _terrainMaterial.name = "TerrainMaterial";
            }
            
            _terrainMaterial.color = biome switch
            {
                BiomeType.Grassland => _grasslandColor,
                BiomeType.Desert => _desertColor,
                BiomeType.Snow => _snowColor,
                BiomeType.Lava => _lavaColor,
                BiomeType.Corrupted => _corruptedColor,
                _ => Color.gray
            };
            
            return _terrainMaterial;
        }
        
        #endregion
    }
}
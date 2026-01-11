using UnityEngine;
using System.Collections.Generic;

namespace Game.Core.Systems
{
    /// <summary>
    /// Procedurally generates character models based on creation choices.
    /// Supports multiple species (human, elf, dwarf, orc, etc.) with male/female variants.
    /// </summary>
    public class ProceduralCharacterBuilder : MonoBehaviour
    {
        [Header("Character Generation Settings")]
        [SerializeField] private float _baseHeight = 1.8f;
        [SerializeField] private float _baseWidth = 0.5f;
        
        /// <summary>
        /// Generate a complete character based on creation data.
        /// </summary>
        public GameObject GenerateCharacter(CharacterCreationData data)
        {
            Debug.Log($"[ProceduralCharacterBuilder] Generating {data.gender} {data.species}...");
            
            GameObject character = new($"Character_{data.species}_{data.gender}");
            
            // Generate body parts
            Mesh bodyMesh = GenerateBodyMesh(data);
            Mesh headMesh = GenerateHeadMesh(data);
            
            // Combine meshes
            Mesh finalMesh = CombineMeshes(bodyMesh, headMesh);
            
            // Setup mesh components
            MeshFilter meshFilter = character.AddComponent<MeshFilter>();
            meshFilter.mesh = finalMesh;
            
            MeshRenderer meshRenderer = character.AddComponent<MeshRenderer>();
            meshRenderer.material = GenerateSkinMaterial(data);
            
            // Add character controller
            CharacterController controller = character.AddComponent<CharacterController>();
            controller.height = GetSpeciesHeight(data.species, data.bodyType);
            controller.radius = 0.3f;
            
            // Add animator
            Animator animator = character.AddComponent<Animator>();
            // TODO: Assign runtime controller
            
            return character;
        }
        
        private Mesh GenerateBodyMesh(CharacterCreationData data)
        {
            Mesh mesh = new();
            mesh.name = "BodyMesh";
            
            float height = GetSpeciesHeight(data.species, data.bodyType);
            float width = _baseWidth * GetBodyTypeScale(data.bodyType);
            
            // Simple capsule-like body
            Vector3[] vertices = new Vector3[]
            {
                // Torso vertices (simplified box)
                new(-width, 0, -width), new(width, 0, -width), new(width, 0, width), new(-width, 0, width), // Bottom
                new(-width, height * 0.6f, -width), new(width, height * 0.6f, -width), new(width, height * 0.6f, width), new(-width, height * 0.6f, width), // Top
            };
            
            int[] triangles = new int[]
            {
                // Bottom face
                0, 2, 1, 0, 3, 2,
                // Top face
                4, 5, 6, 4, 6, 7,
                // Front face
                0, 1, 5, 0, 5, 4,
                // Back face
                3, 7, 6, 3, 6, 2,
                // Left face
                0, 4, 7, 0, 7, 3,
                // Right face
                1, 2, 6, 1, 6, 5
            };
            
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            
            return mesh;
        }
        
        private Mesh GenerateHeadMesh(CharacterCreationData data)
        {
            Mesh mesh = new();
            mesh.name = "HeadMesh";
            
            float height = GetSpeciesHeight(data.species, data.bodyType);
            float headSize = 0.15f;
            float neckHeight = height * 0.6f;
            
            // Simple sphere-like head
            Vector3 headCenter = new(0, neckHeight + headSize, 0);
            
            // Create sphere vertices (simplified)
            List<Vector3> vertices = new();
            List<int> triangles = new();
            
            int segments = 8;
            for (int lat = 0; lat <= segments; lat++)
            {
                float theta = lat * Mathf.PI / segments;
                float sinTheta = Mathf.Sin(theta);
                float cosTheta = Mathf.Cos(theta);
                
                for (int lon = 0; lon <= segments; lon++)
                {
                    float phi = lon * 2 * Mathf.PI / segments;
                    float sinPhi = Mathf.Sin(phi);
                    float cosPhi = Mathf.Cos(phi);
                    
                    Vector3 vertex = new(
                        headSize * sinTheta * cosPhi,
                        headSize * cosTheta,
                        headSize * sinTheta * sinPhi
                    );
                    
                    vertices.Add(headCenter + vertex);
                }
            }
            
            // Generate triangles
            for (int lat = 0; lat < segments; lat++)
            {
                for (int lon = 0; lon < segments; lon++)
                {
                    int first = lat * (segments + 1) + lon;
                    int second = first + segments + 1;
                    
                    triangles.Add(first);
                    triangles.Add(second);
                    triangles.Add(first + 1);
                    
                    triangles.Add(second);
                    triangles.Add(second + 1);
                    triangles.Add(first + 1);
                }
            }
            
            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles.ToArray();
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            
            return mesh;
        }
        
        private Mesh CombineMeshes(params Mesh[] meshes)
        {
            CombineInstance[] combine = new CombineInstance[meshes.Length];
            
            for (int i = 0; i < meshes.Length; i++)
            {
                combine[i].mesh = meshes[i];
                combine[i].transform = Matrix4x4.identity;
            }
            
            Mesh finalMesh = new();
            finalMesh.CombineMeshes(combine, true, false);
            finalMesh.RecalculateNormals();
            finalMesh.RecalculateBounds();
            
            return finalMesh;
        }
        
        private Material GenerateSkinMaterial(CharacterCreationData data)
        {
            Material material = new(Shader.Find("Standard"));
            material.name = $"CharacterMaterial_{data.species}";
            material.color = GetSkinColor(data.species, data.skinTone);
            
            return material;
        }
        
        private float GetSpeciesHeight(Species species, BodyType bodyType)
        {
            float baseHeight = species switch
            {
                Species.Human => 1.8f,
                Species.Elf => 1.9f,
                Species.Dwarf => 1.3f,
                Species.Orc => 2.1f,
                Species.Lizardfolk => 1.85f,
                Species.Android => 1.8f,
                Species.Cyborg => 1.85f,
                Species.Alien => 1.7f,
                Species.Demon => 2.0f,
                Species.Angel => 1.95f,
                _ => 1.8f
            };
            
            return baseHeight;
        }
        
        private float GetBodyTypeScale(BodyType bodyType)
        {
            return bodyType switch
            {
                BodyType.Slim => 0.8f,
                BodyType.Average => 1.0f,
                BodyType.Muscular => 1.2f,
                _ => 1.0f
            };
        }
        
        private Color GetSkinColor(Species species, int skinTone)
        {
            Color[] palette = species switch
            {
                Species.Human => new[] { new Color(1f, 0.8f, 0.6f), new Color(0.8f, 0.6f, 0.4f), new Color(0.4f, 0.3f, 0.2f) },
                Species.Elf => new[] { new Color(1f, 0.95f, 0.9f), new Color(0.9f, 0.85f, 0.8f) },
                Species.Dwarf => new[] { new Color(0.9f, 0.7f, 0.5f), new Color(0.8f, 0.6f, 0.4f) },
                Species.Orc => new[] { new Color(0.4f, 0.6f, 0.3f), new Color(0.3f, 0.5f, 0.2f) },
                Species.Lizardfolk => new[] { new Color(0.3f, 0.7f, 0.3f), new Color(0.4f, 0.6f, 0.5f) },
                Species.Android => new[] { new Color(0.7f, 0.7f, 0.8f), new Color(0.6f, 0.6f, 0.7f) },
                Species.Demon => new[] { new Color(0.8f, 0.2f, 0.2f), new Color(0.6f, 0.1f, 0.1f) },
                Species.Angel => new[] { new Color(1f, 1f, 0.95f), new Color(0.95f, 0.95f, 0.9f) },
                _ => new[] { Color.gray }
            };
            
            return palette[Mathf.Clamp(skinTone, 0, palette.Length - 1)];
        }
    }
    
    #region Character Creation Data
    
    [System.Serializable]
    public class CharacterCreationData
    {
        public Species species = Species.Human;
        public Gender gender = Gender.Male;
        public BodyType bodyType = BodyType.Average;
        public int skinTone = 0;
        public int faceShape = 0;
        public int hairStyle = 0;
        
        // Attributes
        public int strength = 10;
        public int dexterity = 10;
        public int intelligence = 10;
        public int vitality = 10;
        public int endurance = 10;
        public int luck = 10;
        
        // Species ability
        public SpeciesAbility selectedAbility;
        
        // Heroic start bonus
        public HeroicBonus heroicBonus;
    }
    
    public enum Species
    {
        Human, Elf, Dwarf, Orc, Lizardfolk,
        Android, Cyborg, Alien, Demon, Angel
    }
    
    public enum Gender { Male, Female }
    
    public enum BodyType { Slim, Average, Muscular }
    
    public enum SpeciesAbility
    {
        // Human
        Adaptable, SecondWind,
        // Elf
        KeenSenses, NaturesStep,
        // Add more as needed
    }
    
    public enum HeroicBonus
    {
        LegendaryWeapon,
        AncientArmor,
        Spellbook,
        Companion,
        GoldHoard
    }
    
    #endregion
}
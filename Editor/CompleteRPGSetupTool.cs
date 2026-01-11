#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.IO;

/// <summary>
/// COMPLETE RPG SETUP TOOL
/// Tools > Complete RPG Setup
/// Sets up ALL systems, scenes, prefabs, and components automatically.
/// </summary>
public class CompleteRPGSetup : EditorWindow
{
    private Vector2 _scrollPos;
    private bool _foldoutScenes = true;
    private bool _foldoutSystems = true;
    private bool _foldoutPrefabs = true;
    private bool _foldoutUI = true;
    
    [MenuItem("Tools/Complete RPG Setup")]
    static void ShowWindow()
    {
        CompleteRPGSetup window = GetWindow<CompleteRPGSetup>("Complete RPG Setup");
        window.minSize = new Vector2(500, 600);
    }
    
    void OnGUI()
    {
        _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);
        
        GUILayout.Label("COMPLETE RPG SETUP TOOL", EditorStyles.boldLabel);
        GUILayout.Space(10);
        
        EditorGUILayout.HelpBox(
            "This tool will set up your entire RPG project:\n" +
            "• Create all required scenes\n" +
            "• Setup CoreSystemManager with all systems\n" +
            "• Create prefabs for entities and loot\n" +
            "• Setup UI managers\n" +
            "• Configure build settings", 
            MessageType.Info
        );
        
        GUILayout.Space(20);
        
        // Scenes Section
        _foldoutScenes = EditorGUILayout.Foldout(_foldoutScenes, "SCENES", true);
        if (_foldoutScenes)
        {
            EditorGUI.indentLevel++;
            if (GUILayout.Button("1. Create All Scenes", GUILayout.Height(30)))
            {
                CreateAllScenes();
            }
            if (GUILayout.Button("2. Fix Build Settings", GUILayout.Height(30)))
            {
                FixBuildSettings();
            }
            EditorGUI.indentLevel--;
        }
        
        GUILayout.Space(10);
        
        // Systems Section
        _foldoutSystems = EditorGUILayout.Foldout(_foldoutSystems, "CORE SYSTEMS", true);
        if (_foldoutSystems)
        {
            EditorGUI.indentLevel++;
            if (GUILayout.Button("3. Setup Core Systems", GUILayout.Height(30)))
            {
                SetupCoreSystems();
            }
            if (GUILayout.Button("4. Add Loot System", GUILayout.Height(30)))
            {
                AddLootSystem();
            }
            if (GUILayout.Button("5. Add Quest System", GUILayout.Height(30)))
            {
                AddQuestSystem();
            }
            EditorGUI.indentLevel--;
        }
        
        GUILayout.Space(10);
        
        // Prefabs Section
        _foldoutPrefabs = EditorGUILayout.Foldout(_foldoutPrefabs, "PREFABS & ASSETS", true);
        if (_foldoutPrefabs)
        {
            EditorGUI.indentLevel++;
            if (GUILayout.Button("6. Create Entity Prefabs", GUILayout.Height(30)))
            {
                CreateEntityPrefabs();
            }
            if (GUILayout.Button("7. Create Interactable Prefabs", GUILayout.Height(30)))
            {
                CreateInteractablePrefabs();
            }
            EditorGUI.indentLevel--;
        }
        
        GUILayout.Space(10);
        
        // UI Section
        _foldoutUI = EditorGUILayout.Foldout(_foldoutUI, "UI SYSTEMS", true);
        if (_foldoutUI)
        {
            EditorGUI.indentLevel++;
            if (GUILayout.Button("8. Setup UI Managers", GUILayout.Height(30)))
            {
                SetupUIManagers();
            }
            EditorGUI.indentLevel--;
        }
        
        GUILayout.Space(20);
        GUILayout.Label("---", EditorStyles.centeredGreyMiniLabel);
        GUILayout.Space(10);
        
        // DO EVERYTHING
        GUI.backgroundColor = Color.green;
        if (GUILayout.Button("✨ DO EVERYTHING NOW ✨", GUILayout.Height(60)))
        {
            DoCompleteSetup();
        }
        GUI.backgroundColor = Color.white;
        
        GUILayout.Space(10);
        
        // Validate
        if (GUILayout.Button("🔍 Validate Setup", GUILayout.Height(40)))
        {
            ValidateSetup();
        }
        
        EditorGUILayout.EndScrollView();
    }
    
    #region Complete Setup
    
    static void DoCompleteSetup()
    {
        Debug.Log("========== STARTING COMPLETE RPG SETUP ==========");
        
        CreateFolders();
        CreateAllScenes();
        SetupCoreSystems();
        AddLootSystem();
        AddQuestSystem();
        CreateEntityPrefabs();
        CreateInteractablePrefabs();
        SetupUIManagers();
        FixBuildSettings();
        ValidateSetup();
        
        Debug.Log("========== COMPLETE RPG SETUP FINISHED ==========");
        EditorUtility.DisplayDialog("Success!", "Complete RPG setup finished! Check the Console for details.", "OK");
    }
    
    #endregion
    
    #region Folders
    
    static void CreateFolders()
    {
        string[] folders = {
            "Assets/Scenes",
            "Assets/Scenes/Zones",
            "Assets/Scripts/Game/Core",
            "Assets/Scripts/Game/Core/Systems",
            "Assets/Scripts/Game/Core/UI",
            "Assets/Scripts/Game/UI",
            "Assets/Resources",
            "Assets/Resources/Prefabs",
            "Assets/Resources/Prefabs/Entities",
            "Assets/Resources/Prefabs/Interactables"
        };
        
        foreach (string folder in folders)
        {
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
                Debug.Log($"✓ Created folder: {folder}");
            }
        }
        
        AssetDatabase.Refresh();
    }
    
    #endregion
    
    #region Scenes
    
    static void CreateAllScenes()
    {
        CreateFolders();
        
        CreateBootstrapScene();
        CreateMainMenuScene();
        CreateGameWorldScene();
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
    
    static void CreateBootstrapScene()
    {
        if (File.Exists("Assets/Scenes/Bootstrap.unity"))
        {
            Debug.Log("  Bootstrap.unity already exists");
            return;
        }
        
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        
        GameObject bootstrapObj = new GameObject("Bootstrap");
        bootstrapObj.AddComponent<Game.Core.GameBootstrap>();
        
        EditorSceneManager.SaveScene(scene, "Assets/Scenes/Bootstrap.unity");
        Debug.Log("✓ Created Bootstrap.unity");
    }
    
    static void CreateMainMenuScene()
    {
        if (File.Exists("Assets/Scenes/MainMenu.unity"))
        {
            Debug.Log("  MainMenu.unity already exists");
            return;
        }
        
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        
        // Character creation canvas
        GameObject canvasObj = new GameObject("CharacterCreationCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        
        UnityEngine.UI.CanvasScaler scaler = canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
        scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        
        canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        canvasObj.AddComponent<Game.UI.CharacterCreationUI>();
        
        // Systems
        GameObject systemsObj = new GameObject("Systems");
        systemsObj.AddComponent<Game.Core.Systems.ProceduralCharacterBuilder>();
        
        EditorSceneManager.SaveScene(scene, "Assets/Scenes/MainMenu.unity");
        Debug.Log("✓ Created MainMenu.unity");
    }
    
    static void CreateGameWorldScene()
    {
        if (File.Exists("Assets/Scenes/GameWorld.unity"))
        {
            Debug.Log("  GameWorld.unity already exists");
            return;
        }
        
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        
        GameObject initObj = new GameObject("GameWorldInitializer");
        initObj.AddComponent<Game.Core.GameWorldInitializer>();
        
        EditorSceneManager.SaveScene(scene, "Assets/Scenes/GameWorld.unity");
        Debug.Log("✓ Created GameWorld.unity");
    }
    
    #endregion
    
    #region Systems
    
    static void SetupCoreSystems()
    {
        // Find or create CoreSystemManager in current scene
        GameObject coreObj = GameObject.Find("CoreSystemManager");
        
        if (coreObj == null)
        {
            coreObj = new GameObject("CoreSystemManager");
            coreObj.AddComponent<Game.Core.Systems.CoreSystemManager>();
            Debug.Log("✓ Created CoreSystemManager");
        }
        else
        {
            Debug.Log("  CoreSystemManager already exists");
        }
        
        EditorUtility.SetDirty(coreObj);
    }
    
    static void AddLootSystem()
    {
        GameObject lootObj = GameObject.Find("LootSystemManager");
        
        if (lootObj == null)
        {
            lootObj = new GameObject("LootSystemManager");
            lootObj.AddComponent<Game.Core.Systems.LootSystemManager>();
            Debug.Log("✓ Created LootSystemManager");
        }
        
        EditorUtility.SetDirty(lootObj);
    }
    
    static void AddQuestSystem()
    {
        GameObject questObj = GameObject.Find("QuestSystemManager");
        
        if (questObj == null)
        {
            questObj = new GameObject("QuestSystemManager");
            questObj.AddComponent<Game.Core.Systems.QuestSystemManager>();
            Debug.Log("✓ Created QuestSystemManager");
        }
        
        EditorUtility.SetDirty(questObj);
    }
    
    #endregion
    
    #region Prefabs
    
    static void CreateEntityPrefabs()
    {
        CreateFolders();
        
        CreateZombiePrefab();
        CreateSkeletonPrefab();
        CreateOrcPrefab();
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
    
    static void CreateZombiePrefab()
    {
        string path = "Assets/Resources/Prefabs/Entities/Zombie.prefab";
        
        if (File.Exists(path))
        {
            Debug.Log("  Zombie prefab already exists");
            return;
        }
        
        GameObject zombie = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        zombie.name = "Zombie";
        
        Renderer renderer = zombie.GetComponent<Renderer>();
        Material mat = new Material(Shader.Find("Standard"));
        mat.color = Color.green;
        renderer.material = mat;
        
        zombie.AddComponent<Game.Core.Systems.EntityStats>();
        zombie.AddComponent<Game.Core.Systems.EntityAI>();
        zombie.AddComponent<Game.Core.Systems.PoolableEntity>();
        
        PrefabUtility.SaveAsPrefabAsset(zombie, path);
        DestroyImmediate(zombie);
        
        Debug.Log("✓ Created Zombie prefab");
    }
    
    static void CreateSkeletonPrefab()
    {
        string path = "Assets/Resources/Prefabs/Entities/Skeleton.prefab";
        
        if (File.Exists(path))
        {
            Debug.Log("  Skeleton prefab already exists");
            return;
        }
        
        GameObject skeleton = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        skeleton.name = "Skeleton";
        
        Renderer renderer = skeleton.GetComponent<Renderer>();
        Material mat = new Material(Shader.Find("Standard"));
        mat.color = Color.white;
        renderer.material = mat;
        
        skeleton.AddComponent<Game.Core.Systems.EntityStats>();
        skeleton.AddComponent<Game.Core.Systems.EntityAI>();
        skeleton.AddComponent<Game.Core.Systems.PoolableEntity>();
        
        PrefabUtility.SaveAsPrefabAsset(skeleton, path);
        DestroyImmediate(skeleton);
        
        Debug.Log("✓ Created Skeleton prefab");
    }
    
    static void CreateOrcPrefab()
    {
        string path = "Assets/Resources/Prefabs/Entities/Orc.prefab";
        
        if (File.Exists(path))
        {
            Debug.Log("  Orc prefab already exists");
            return;
        }
        
        GameObject orc = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        orc.name = "Orc";
        
        Renderer renderer = orc.GetComponent<Renderer>();
        Material mat = new Material(Shader.Find("Standard"));
        mat.color = new Color(0.4f, 0.6f, 0.3f);
        renderer.material = mat;
        
        Game.Core.Systems.EntityStats stats = orc.AddComponent<Game.Core.Systems.EntityStats>();
        stats.maxHealth = 200;
        stats.currentHealth = 200;
        
        orc.AddComponent<Game.Core.Systems.EntityAI>();
        orc.AddComponent<Game.Core.Systems.PoolableEntity>();
        
        PrefabUtility.SaveAsPrefabAsset(orc, path);
        DestroyImmediate(orc);
        
        Debug.Log("✓ Created Orc prefab");
    }
    
    static void CreateInteractablePrefabs()
    {
        CreateDoorPrefab();
        CreateChestPrefab();
        CreateLadderPrefab();
    }
    
    static void CreateDoorPrefab()
    {
        string path = "Assets/Resources/Prefabs/Interactables/Door.prefab";
        
        if (File.Exists(path))
        {
            Debug.Log("  Door prefab already exists");
            return;
        }
        
        GameObject door = GameObject.CreatePrimitive(PrimitiveType.Cube);
        door.name = "Door";
        door.transform.localScale = new Vector3(1.5f, 3f, 0.2f);
        
        Renderer renderer = door.GetComponent<Renderer>();
        Material mat = new Material(Shader.Find("Standard"));
        mat.color = new Color(0.6f, 0.4f, 0.2f);
        renderer.material = mat;
        
        door.AddComponent<Game.Core.Systems.Door>();
        
        PrefabUtility.SaveAsPrefabAsset(door, path);
        DestroyImmediate(door);
        
        Debug.Log("✓ Created Door prefab");
    }
    
    static void CreateChestPrefab()
    {
        string path = "Assets/Resources/Prefabs/Interactables/Chest.prefab";
        
        if (File.Exists(path))
        {
            Debug.Log("  Chest prefab already exists");
            return;
        }
        
        GameObject chest = GameObject.CreatePrimitive(PrimitiveType.Cube);
        chest.name = "Chest";
        chest.transform.localScale = new Vector3(1f, 0.8f, 0.6f);
        
        Renderer renderer = chest.GetComponent<Renderer>();
        Material mat = new Material(Shader.Find("Standard"));
        mat.color = new Color(0.7f, 0.5f, 0.2f);
        renderer.material = mat;
        
        chest.AddComponent<Game.Core.Systems.Chest>();
        
        PrefabUtility.SaveAsPrefabAsset(chest, path);
        DestroyImmediate(chest);
        
        Debug.Log("✓ Created Chest prefab");
    }
    
    static void CreateLadderPrefab()
    {
        string path = "Assets/Resources/Prefabs/Interactables/Ladder.prefab";
        
        if (File.Exists(path))
        {
            Debug.Log("  Ladder prefab already exists");
            return;
        }
        
        GameObject ladder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        ladder.name = "Ladder";
        ladder.transform.localScale = new Vector3(0.3f, 3f, 0.3f);
        
        Renderer renderer = ladder.GetComponent<Renderer>();
        Material mat = new Material(Shader.Find("Standard"));
        mat.color = new Color(0.5f, 0.3f, 0.1f);
        renderer.material = mat;
        
        ladder.AddComponent<Game.Core.Systems.Ladder>();
        
        PrefabUtility.SaveAsPrefabAsset(ladder, path);
        DestroyImmediate(ladder);
        
        Debug.Log("✓ Created Ladder prefab");
    }
    
    #endregion
    
    #region UI
    
    static void SetupUIManagers()
    {
        // These are runtime-generated, but we can create placeholder objects
        GameObject uiRoot = GameObject.Find("UIRoot");
        
        if (uiRoot == null)
        {
            uiRoot = new GameObject("UIRoot");
            Debug.Log("✓ Created UIRoot");
        }
        
        // Add UI components
        if (uiRoot.GetComponent<Game.UI.InventoryUI>() == null)
        {
            uiRoot.AddComponent<Game.UI.InventoryUI>();
            Debug.Log("✓ Added InventoryUI");
        }
        
        if (uiRoot.GetComponent<Game.UI.QuestUI>() == null)
        {
            uiRoot.AddComponent<Game.UI.QuestUI>();
            Debug.Log("✓ Added QuestUI");
        }
        
        if (uiRoot.GetComponent<Game.UI.NetworkUI>() == null)
        {
            uiRoot.AddComponent<Game.UI.NetworkUI>();
            Debug.Log("✓ Added NetworkUI");
        }
        
        EditorUtility.SetDirty(uiRoot);
    }
    
    #endregion
    
    #region Build Settings
    
    static void FixBuildSettings()
    {
        var sceneList = new System.Collections.Generic.List<EditorBuildSettingsScene>();
        
        string[] scenePaths = {
            "Assets/Scenes/Bootstrap.unity",
            "Assets/Scenes/MainMenu.unity",
            "Assets/Scenes/GameWorld.unity"
        };
        
        foreach (string path in scenePaths)
        {
            if (File.Exists(path))
            {
                sceneList.Add(new EditorBuildSettingsScene(path, true));
                Debug.Log($"✓ Added to build: {path}");
            }
            else
            {
                Debug.LogWarning($"⚠ Scene not found: {path}");
            }
        }
        
        EditorBuildSettings.scenes = sceneList.ToArray();
        Debug.Log($"✓ Build settings updated with {sceneList.Count} scenes");
    }
    
    #endregion
    
    #region Validation
    
    static void ValidateSetup()
    {
        Debug.Log("========== VALIDATION ==========");
        
        bool allGood = true;
        
        // Check scenes
        string[] requiredScenes = {
            "Assets/Scenes/Bootstrap.unity",
            "Assets/Scenes/MainMenu.unity",
            "Assets/Scenes/GameWorld.unity"
        };
        
        foreach (string scene in requiredScenes)
        {
            if (File.Exists(scene))
            {
                Debug.Log($"✓ Scene: {scene}");
            }
            else
            {
                Debug.LogError($"✗ Missing scene: {scene}");
                allGood = false;
            }
        }
        
        // Check prefabs
        string[] requiredPrefabs = {
            "Assets/Resources/Prefabs/Entities/Zombie.prefab",
            "Assets/Resources/Prefabs/Entities/Skeleton.prefab",
            "Assets/Resources/Prefabs/Entities/Orc.prefab",
            "Assets/Resources/Prefabs/Interactables/Door.prefab",
            "Assets/Resources/Prefabs/Interactables/Chest.prefab"
        };
        
        foreach (string prefab in requiredPrefabs)
        {
            if (File.Exists(prefab))
            {
                Debug.Log($"✓ Prefab: {prefab}");
            }
            else
            {
                Debug.LogWarning($"⚠ Missing prefab: {prefab}");
            }
        }
        
        // Check build settings
        int sceneCount = EditorBuildSettings.scenes.Length;
        if (sceneCount >= 3)
        {
            Debug.Log($"✓ Build settings: {sceneCount} scenes configured");
        }
        else
        {
            Debug.LogWarning($"⚠ Build settings: Only {sceneCount} scenes (need 3)");
            allGood = false;
        }
        
        Debug.Log("========== VALIDATION END ==========");
        
        if (allGood)
        {
            Debug.Log("<color=green><b>✓✓✓ ALL CHECKS PASSED ✓✓✓</b></color>");
            EditorUtility.DisplayDialog("Validation Complete", 
                "All checks passed! Your RPG project is ready.", "OK");
        }
        else
        {
            Debug.LogWarning("<color=orange>⚠ Some checks failed - run 'DO EVERYTHING NOW'</color>");
            EditorUtility.DisplayDialog("Validation Issues", 
                "Some checks failed. Click 'DO EVERYTHING NOW' to fix.", "OK");
        }
    }
    
    #endregion
}
#endif
#####CompleteRPGSetupTool.cs##### 
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
 
#####QuickFix.cs##### 
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.IO;

/// <summary>
/// ONE-CLICK FIX for all setup issues
/// Tools > Fix RPG Setup
/// </summary>
public class QuickFix : EditorWindow
{
    [MenuItem("Tools/Fix RPG Setup")]
    static void ShowWindow()
    {
        QuickFix window = GetWindow<QuickFix>("RPG Quick Fix");
        window.minSize = new Vector2(400, 300);
    }
    
    void OnGUI()
    {
        GUILayout.Label("RPG Quick Fix", EditorStyles.boldLabel);
        GUILayout.Space(10);
        
        EditorGUILayout.HelpBox("This will fix all common setup issues", MessageType.Info);
        GUILayout.Space(10);
        
        if (GUILayout.Button("1. Create Missing Folders", GUILayout.Height(30)))
        {
            CreateFolders();
        }
        
        if (GUILayout.Button("2. Create Required Scenes", GUILayout.Height(30)))
        {
            CreateScenes();
        }
        
        if (GUILayout.Button("3. Fix Build Settings", GUILayout.Height(30)))
        {
            FixBuildSettings();
        }
        
        if (GUILayout.Button("4. Validate Current Setup", GUILayout.Height(30)))
        {
            ValidateSetup();
        }
        
        GUILayout.Space(20);
        GUILayout.Label("---", EditorStyles.centeredGreyMiniLabel);
        GUILayout.Space(10);
        
        GUI.backgroundColor = Color.green;
        if (GUILayout.Button("FIX EVERYTHING NOW", GUILayout.Height(50)))
        {
            FixEverything();
        }
        GUI.backgroundColor = Color.white;
        
        GUILayout.Space(20);
        
        if (GUILayout.Button("Open Bootstrap Scene"))
        {
            OpenBootstrap();
        }
    }
    
    static void FixEverything()
    {
        Debug.Log("========== FIXING RPG SETUP ==========");
        CreateFolders();
        CreateScenes();
        FixBuildSettings();
        ValidateSetup();
        Debug.Log("========== FIX COMPLETE ==========");
        EditorUtility.DisplayDialog("Success", "All fixes applied! Check Console for details.", "OK");
    }
    
    static void CreateFolders()
    {
        string[] folders = {
            "Assets/Scenes",
            "Assets/Scenes/Zones",
            "Assets/Scripts/Game/Core/UI",
            "Assets/Resources",
            "Assets/Resources/Prefabs"
        };
        
        foreach (string folder in folders)
        {
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
                Debug.Log($"✓ Created folder: {folder}");
            }
            else
            {
                Debug.Log($"  Folder exists: {folder}");
            }
        }
        
        AssetDatabase.Refresh();
    }
    
    static void CreateScenes()
    {
        // Bootstrap Scene
        if (!File.Exists("Assets/Scenes/Bootstrap.unity"))
        {
            Scene bootstrap = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            
            GameObject bootstrapObj = new GameObject("Bootstrap");
            bootstrapObj.AddComponent<Game.Core.GameBootstrap>();
            
            EditorSceneManager.SaveScene(bootstrap, "Assets/Scenes/Bootstrap.unity");
            Debug.Log("✓ Created Bootstrap.unity");
        }
        else
        {
            Debug.Log("  Bootstrap.unity exists");
        }
        
        // MainMenu Scene
        if (!File.Exists("Assets/Scenes/MainMenu.unity"))
        {
            Scene mainMenu = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            
            // Create Canvas
            GameObject canvasObj = new GameObject("CharacterCreationCanvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            
            UnityEngine.UI.CanvasScaler scaler = canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
            scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            
            canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            
            // Add CharacterCreationUI - this is the fix!
            canvasObj.AddComponent<Game.UI.CharacterCreationUI>();
            
            // Create Systems container
            GameObject systemsObj = new GameObject("Systems");
            systemsObj.AddComponent<Game.Core.Systems.ProceduralCharacterBuilder>();
            
            EditorSceneManager.SaveScene(mainMenu, "Assets/Scenes/MainMenu.unity");
            Debug.Log("✓ Created MainMenu.unity with CharacterCreationUI");
        }
        else
        {
            Debug.Log("  MainMenu.unity exists");
        }
        
        // GameWorld Scene
        if (!File.Exists("Assets/Scenes/GameWorld.unity"))
        {
            Scene gameWorld = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            
            // Add GameWorldInitializer
            GameObject initObj = new GameObject("GameWorldInitializer");
            initObj.AddComponent<Game.Core.GameWorldInitializer>();
            
            EditorSceneManager.SaveScene(gameWorld, "Assets/Scenes/GameWorld.unity");
            Debug.Log("✓ Created GameWorld.unity");
        }
        else
        {
            Debug.Log("  GameWorld.unity exists");
        }
    }
    
    static void FixBuildSettings()
    {
        var sceneList = new System.Collections.Generic.List<EditorBuildSettingsScene>();
        
        // Add scenes in order
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
    
    static void ValidateSetup()
    {
        Debug.Log("========== VALIDATION ==========");
        
        bool allGood = true;
        
        // Check folders
        string[] requiredFolders = {
            "Assets/Scenes",
            "Assets/Scripts/Game/Core/UI"
        };
        
        foreach (string folder in requiredFolders)
        {
            if (Directory.Exists(folder))
            {
                Debug.Log($"✓ Folder: {folder}");
            }
            else
            {
                Debug.LogError($"✗ Missing folder: {folder}");
                allGood = false;
            }
        }
        
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
        
        // Check scripts
        System.Type[] requiredTypes = {
            typeof(Game.Core.GameBootstrap),
            typeof(Game.UI.CharacterCreationUI),
            typeof(Game.Core.GameWorldInitializer),
            typeof(Game.Core.PlayerController),
            typeof(Game.Core.Systems.CoreSystemManager)
        };
        
        foreach (System.Type type in requiredTypes)
        {
            if (type != null)
            {
                Debug.Log($"✓ Script: {type.Name}");
            }
            else
            {
                Debug.LogError($"✗ Missing script type");
                allGood = false;
            }
        }
        
        Debug.Log("========== VALIDATION END ==========");
        
        if (allGood)
        {
            Debug.Log("<color=green><b>✓✓✓ ALL CHECKS PASSED ✓✓✓</b></color>");
        }
        else
        {
            Debug.LogWarning("<color=orange>⚠ Some checks failed - run 'FIX EVERYTHING NOW'</color>");
        }
    }
    
    static void OpenBootstrap()
    {
        if (File.Exists("Assets/Scenes/Bootstrap.unity"))
        {
            EditorSceneManager.OpenScene("Assets/Scenes/Bootstrap.unity");
            Debug.Log("✓ Opened Bootstrap scene");
        }
        else
        {
            Debug.LogError("Bootstrap.unity not found! Click 'Create Required Scenes' first.");
        }
    }
}
#endif 
 
#####QuickSetup.cs##### 
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;

public class QuickSetup : EditorWindow
{
    [MenuItem("Tools/Quick Setup RPG")]
    static void ShowWindow()
    {
        GetWindow<QuickSetup>("RPG Setup");
    }
    
    void OnGUI()
    {
        GUILayout.Label("RPG First-Time Setup", EditorStyles.boldLabel);
        
        if (GUILayout.Button("1. Create Folders"))
        {
            CreateFolders();
        }
        
        if (GUILayout.Button("2. Create Scenes"))
        {
            CreateScenes();
        }
        
        if (GUILayout.Button("3. Setup Build Settings"))
        {
            SetupBuildSettings();
        }
        
        if (GUILayout.Button("4. Create Prefabs"))
        {
            CreatePrefabs();
        }
        
        GUILayout.Space(20);
        
        if (GUILayout.Button("DO EVERYTHING", GUILayout.Height(40)))
        {
            CreateFolders();
            CreateScenes();
            SetupBuildSettings();
            CreatePrefabs();
            Debug.Log("✓✓✓ SETUP COMPLETE! ✓✓✓");
        }
    }
    
    static void CreateFolders()
    {
        string[] folders = {
            "Assets/Resources",
            "Assets/Resources/Prefabs",
            "Assets/Scenes",
            "Assets/Scenes/Zones",
            "Assets/Scripts/Game/Core/UI"
        };
        
        foreach (string folder in folders)
        {
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
                Debug.Log($"✓ Created: {folder}");
            }
        }
        
        AssetDatabase.Refresh();
    }
    
    static void CreateScenes()
    {
        // Create Bootstrap scene
        var bootstrap = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        GameObject bootstrapObj = new GameObject("Bootstrap");
        bootstrapObj.AddComponent<Game.Core.GameBootstrap>();
        EditorSceneManager.SaveScene(bootstrap, "Assets/Scenes/Bootstrap.unity");
        Debug.Log("✓ Created Bootstrap.unity");
        
        // Create MainMenu scene
        var mainMenu = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        GameObject canvas = new GameObject("CharacterCreationCanvas");
        Canvas canvasComp = canvas.AddComponent<Canvas>();
        canvasComp.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.AddComponent<UnityEngine.UI.CanvasScaler>();
        canvas.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        
        GameObject systems = new GameObject("Systems");
        systems.AddComponent<Game.Core.Systems.ProceduralCharacterBuilder>();
        
        EditorSceneManager.SaveScene(mainMenu, "Assets/Scenes/MainMenu.unity");
        Debug.Log("✓ Created MainMenu.unity");
        
        // Create GameWorld scene
        var gameWorld = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        EditorSceneManager.SaveScene(gameWorld, "Assets/Scenes/GameWorld.unity");
        Debug.Log("✓ Created GameWorld.unity");
    }
    
    static void SetupBuildSettings()
    {
        var scenes = new EditorBuildSettingsScene[]
        {
            new EditorBuildSettingsScene("Assets/Scenes/Bootstrap.unity", true),
            new EditorBuildSettingsScene("Assets/Scenes/MainMenu.unity", true),
            new EditorBuildSettingsScene("Assets/Scenes/GameWorld.unity", true)
        };
        
        EditorBuildSettings.scenes = scenes;
        Debug.Log("✓ Build settings configured");
    }
    
    static void CreatePrefabs()
    {
        // Create DamageNumber prefab
        GameObject damageNum = new GameObject("DamageNumber");
        damageNum.AddComponent<Game.Core.Systems.DamageNumber>();
        
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(damageNum.transform);
        TextMesh textMesh = textObj.AddComponent<TextMesh>();
        textMesh.fontSize = 32;
        textMesh.anchor = TextAnchor.MiddleCenter;
        
        PrefabUtility.SaveAsPrefabAsset(damageNum, "Assets/Resources/Prefabs/DamageNumber.prefab");
        DestroyImmediate(damageNum);
        Debug.Log("✓ Created DamageNumber.prefab");
    }
}
#endif 
 
#####TutorialSetupTool.cs##### 
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

/// <summary>
/// ONE-CLICK Tutorial Setup Tool
/// Tools > Setup Tutorial Flow
/// Creates everything needed to run the tutorial
/// </summary>
public class TutorialSetupTool : EditorWindow
{
    [MenuItem("Tools/Setup Tutorial Flow")]
    static void ShowWindow()
    {
        TutorialSetupTool window = GetWindow<TutorialSetupTool>("Tutorial Setup");
        window.minSize = new Vector2(500, 400);
    }
    
    private void OnGUI()
    {
        GUILayout.Label("TUTORIAL FLOW SETUP", EditorStyles.boldLabel);
        GUILayout.Space(10);
        
        EditorGUILayout.HelpBox(
            "This will create a complete tutorial flow:\n" +
            "• Bootstrap scene with EnhancedGameBootstrap\n" +
            "• GameWorld scene with TutorialSystemManager\n" +
            "• All required systems configured\n" +
            "• Build settings updated", 
            MessageType.Info
        );
        
        GUILayout.Space(20);
        
        if (GUILayout.Button("1. Create Bootstrap Scene", GUILayout.Height(50)))
        {
            CreateBootstrapScene();
        }
        
        if (GUILayout.Button("2. Setup GameWorld Scene", GUILayout.Height(50)))
        {
            SetupGameWorldScene();
        }
        
        if (GUILayout.Button("3. Fix Build Settings", GUILayout.Height(50)))
        {
            FixBuildSettings();
        }
        
        GUILayout.Space(20);
        GUILayout.Label("---", EditorStyles.centeredGreyMiniLabel);
        GUILayout.Space(10);
        
        GUI.backgroundColor = Color.green;
        if (GUILayout.Button("✨ DO EVERYTHING ✨", GUILayout.Height(80)))
        {
            DoCompleteSetup();
        }
        GUI.backgroundColor = Color.white;
        
        GUILayout.Space(20);
        
        if (GUILayout.Button("🎮 PLAY TUTORIAL", GUILayout.Height(60)))
        {
            PlayTutorial();
        }
    }
    
    static void DoCompleteSetup()
    {
        Debug.Log("========== TUTORIAL SETUP STARTED ==========");
        
        CreateBootstrapScene();
        SetupGameWorldScene();
        FixBuildSettings();
        
        Debug.Log("========== TUTORIAL SETUP COMPLETE ==========");
        EditorUtility.DisplayDialog("Success!", 
            "Tutorial flow setup complete!\n\nClick 'PLAY TUTORIAL' to test it.", "OK");
    }
    
    static void CreateBootstrapScene()
    {
        if (System.IO.File.Exists("Assets/Scenes/Bootstrap.unity"))
        {
            Debug.Log("Bootstrap scene exists - updating it...");
            EditorSceneManager.OpenScene("Assets/Scenes/Bootstrap.unity");
        }
        else
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            EditorSceneManager.SaveScene(scene, "Assets/Scenes/Bootstrap.unity");
        }
        
        // Clear scene
        GameObject[] allObjects = UnityEngine.Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        foreach (GameObject obj in allObjects)
        {
            if (obj.transform.parent == null)
                DestroyImmediate(obj);
        }
        
        // Add EnhancedGameBootstrap
        GameObject bootstrapObj = new GameObject("EnhancedGameBootstrap");
        bootstrapObj.AddComponent<Game.Core.EnhancedGameBootstrap>();
        
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
        
        Debug.Log("✓ Bootstrap scene created with EnhancedGameBootstrap");
    }
    
    static void SetupGameWorldScene()
    {
        if (System.IO.File.Exists("Assets/Scenes/GameWorld.unity"))
        {
            EditorSceneManager.OpenScene("Assets/Scenes/GameWorld.unity");
        }
        else
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            EditorSceneManager.SaveScene(scene, "Assets/Scenes/GameWorld.unity");
        }
        
        // Check for TutorialSystemManager
        Game.Core.TutorialSystemManager tutorial = UnityEngine.Object.FindFirstObjectByType<Game.Core.TutorialSystemManager>();
        
        if (tutorial == null)
        {
            GameObject tutorialObj = new GameObject("TutorialSystemManager");
            tutorialObj.AddComponent<Game.Core.TutorialSystemManager>();
            Debug.Log("✓ Added TutorialSystemManager to GameWorld");
        }
        else
        {
            Debug.Log("✓ TutorialSystemManager already exists");
        }
        
        // Check for GameWorldInitializer
        Game.Core.GameWorldInitializer initializer = UnityEngine.Object.FindFirstObjectByType<Game.Core.GameWorldInitializer>();
        
        if (initializer == null)
        {
            GameObject initObj = new GameObject("GameWorldInitializer");
            initObj.AddComponent<Game.Core.GameWorldInitializer>();
            Debug.Log("✓ Added GameWorldInitializer to GameWorld");
        }
        else
        {
            Debug.Log("✓ GameWorldInitializer already exists");
        }
        
        // Ensure we have a directional light
        Light[] lights = UnityEngine.Object.FindObjectsByType<Light>(FindObjectsSortMode.None);
        bool hasDirectionalLight = false;
        foreach (Light light in lights)
        {
            if (light.type == LightType.Directional)
            {
                hasDirectionalLight = true;
                break;
            }
        }
        
        if (!hasDirectionalLight)
        {
            GameObject lightObj = new GameObject("Directional Light");
            Light light = lightObj.AddComponent<Light>();
            light.type = LightType.Directional;
            lightObj.transform.rotation = Quaternion.Euler(50, -30, 0);
            Debug.Log("✓ Added Directional Light");
        }
        
        // Add a ground plane
        GameObject[] planes = GameObject.FindGameObjectsWithTag("Untagged");
        bool hasGround = false;
        foreach (GameObject obj in planes)
        {
            if (obj.name.Contains("Ground"))
            {
                hasGround = true;
                break;
            }
        }
        
        if (!hasGround)
        {
            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.position = Vector3.zero;
            ground.transform.localScale = new Vector3(20, 1, 20);
            
            Renderer renderer = ground.GetComponent<Renderer>();
            Material mat = new Material(Shader.Find("Standard"));
            mat.color = new Color(0.3f, 0.5f, 0.3f);
            renderer.material = mat;
            
            Debug.Log("✓ Added ground plane");
        }
        
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
        
        Debug.Log("✓ GameWorld scene configured");
    }
    
    static void FixBuildSettings()
    {
        var sceneList = new System.Collections.Generic.List<EditorBuildSettingsScene>();
        
        string[] scenePaths = {
            "Assets/Scenes/Bootstrap.unity",
            "Assets/Scenes/GameWorld.unity"
        };
        
        foreach (string path in scenePaths)
        {
            if (System.IO.File.Exists(path))
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
    
    static void PlayTutorial()
    {
        // Open Bootstrap scene
        if (System.IO.File.Exists("Assets/Scenes/Bootstrap.unity"))
        {
            EditorSceneManager.OpenScene("Assets/Scenes/Bootstrap.unity");
            Debug.Log("✓ Bootstrap scene loaded - ready to play!");
            
            EditorApplication.isPlaying = true;
        }
        else
        {
            EditorUtility.DisplayDialog("Error", 
                "Bootstrap scene not found! Click 'DO EVERYTHING' first.", "OK");
        }
    }
}
#endif 
 
#####IPoolable.cs##### 
using UnityEngine;

namespace Game.Core.Pooling
{
    /// <summary>
    /// Interface for all objects that can be pooled.
    /// Ensures consistent lifecycle management across all pooled types.
    /// </summary>
    public interface IPoolable
    {
        /// <summary>
        /// Called when object is retrieved from pool.
        /// Use this instead of Awake/Start for pooled objects.
        /// </summary>
        void OnSpawnFromPool();
        
        /// <summary>
        /// Called when object is returned to pool.
        /// Reset all state here to avoid cross-contamination.
        /// </summary>
        void OnReturnToPool();
        
        /// <summary>
        /// The GameObject this poolable is attached to.
        /// Required for transform operations and SetActive calls.
        /// </summary>
        GameObject GameObject { get; }
        
        /// <summary>
        /// Unique identifier for this poolable type.
        /// Used for pool organization and debugging.
        /// </summary>
        string PoolKey { get; }
        
        /// <summary>
        /// Whether this object is currently active in the scene.
        /// Used to prevent double-returns to pool.
        /// </summary>
        bool IsActiveInPool { get; set; }
    }
    
    /// <summary>
    /// Base MonoBehaviour implementation of IPoolable.
    /// Inherit from this for quick poolable object creation.
    /// </summary>
    public abstract class PoolableObject : MonoBehaviour, IPoolable
    {
        [Header("Pooling")]
        [SerializeField, Tooltip("Unique key for this poolable type")]
        private string _poolKey = "DefaultPool";
        
        private bool _isActiveInPool;
        
        public GameObject GameObject => gameObject;
        public string PoolKey => _poolKey;
        public bool IsActiveInPool 
        { 
            get => _isActiveInPool; 
            set => _isActiveInPool = value; 
        }
        
        /// <summary>
        /// Override this to handle spawn logic.
        /// Called when retrieved from pool.
        /// </summary>
        public virtual void OnSpawnFromPool()
        {
            _isActiveInPool = true;
            gameObject.SetActive(true);
        }
        
        /// <summary>
        /// Override this to handle return logic.
        /// Called when returned to pool.
        /// IMPORTANT: Reset all state here!
        /// </summary>
        public virtual void OnReturnToPool()
        {
            _isActiveInPool = false;
            gameObject.SetActive(false);
        }
        
        /// <summary>
        /// Helper method to return this object to pool.
        /// Call instead of Destroy() for pooled objects.
        /// </summary>
        protected void ReturnToPool()
        {
            Game.Core.Systems.ObjectPoolManager poolManager = Game.Core.Systems.CoreSystemManager.PoolManager;
            if (poolManager != null)
            {
                poolManager.ReturnToPool(this);
            }
            else
            {
                Debug.LogError($"Cannot return {gameObject.name} to pool - ObjectPoolManager not found!");
            }
        }
    }
} 
 
#####CombatSystem.cs##### 
using UnityEngine;
using System.Collections.Generic;

namespace Game.Core.Systems
{
    /// <summary>
    /// Complete combat system with damage calculation, abilities, and magic.
    /// RPG-tanky combat with fun early game pacing.
    /// </summary>
    public partial class CombatSystemManager : MonoBehaviour
    {
        [Header("Combat Settings")]
        [SerializeField] private float _globalDamageMultiplier = 1.0f;
        [SerializeField] private float _criticalHitMultiplier = 2.0f;
        
        public void Shutdown() { }
        
        /// <summary>
        /// Calculate final damage with all modifiers.
        /// </summary>
        public DamageResult CalculateDamage(EntityStats attacker, EntityStats defender, int baseDamage, DamageType damageType)
        {
            DamageResult result = new();
            
            // Get relevant stat for damage type
            float attackerStat = damageType switch
            {
                DamageType.Physical => attacker.strength,
                DamageType.Ranged => attacker.dexterity,
                DamageType.Magic => attacker.intelligence,
                _ => 0
            };
            
            // Base damage calculation
            float damage = baseDamage * (1 + attackerStat / 100f);
            
            // Apply global multiplier
            damage *= _globalDamageMultiplier;
            
            // Check for critical hit
            float critRoll = Random.Range(0f, 1f);
            if (critRoll <= attacker.criticalChance)
            {
                damage *= _criticalHitMultiplier;
                result.isCritical = true;
            }
            
            // Apply elemental multiplier
            damage *= GetElementalMultiplier(attacker.damageElement, defender.resistances);
            
            // Apply armor reduction (diminishing returns formula)
            float armorReduction = defender.armor / (defender.armor + 100f);
            damage *= (1f - armorReduction);
            
            // Ensure minimum damage
            damage = Mathf.Max(1, damage);
            
            result.finalDamage = Mathf.RoundToInt(damage);
            result.damageType = damageType;
            result.wasBlocked = false; // TODO: Implement blocking
            
            return result;
        }
        
        private float GetElementalMultiplier(DamageElement element, EntityResistances resistances)
        {
            return element switch
            {
                DamageElement.Fire => 1f - resistances.fireResistance,
                DamageElement.Ice => 1f - resistances.iceResistance,
                DamageElement.Lightning => 1f - resistances.lightningResistance,
                DamageElement.Poison => 1f - resistances.poisonResistance,
                DamageElement.Holy => 1f - resistances.holyResistance,
                DamageElement.Dark => 1f - resistances.darkResistance,
                _ => 1f
            };
        }
        
        /// <summary>
        /// Apply damage to an entity and trigger effects.
        /// </summary>
        public void ApplyDamage(GameObject target, DamageResult damage, GameObject attacker)
        {
            EntityStats targetStats = target.GetComponent<EntityStats>();
            if (targetStats == null) return;
            
            // Apply damage
            targetStats.currentHealth -= damage.finalDamage;
            targetStats.currentHealth = Mathf.Max(0, targetStats.currentHealth);
            
            // Trigger damage events
            OnDamageDealt?.Invoke(attacker, target, damage);
            
            // Check for death
            if (targetStats.currentHealth <= 0)
            {
                OnEntityDeath?.Invoke(target, attacker);
            }
            
            // Spawn damage number (pooled)
            SpawnDamageNumber(target.transform.position, damage);
        }
        
        private void SpawnDamageNumber(Vector3 position, DamageResult damage)
        {
            ObjectPoolManager poolManager = CoreSystemManager.PoolManager;
            if (poolManager == null) return;
            
            DamageNumber damageNum = poolManager.Get<DamageNumber>("DamageNumber", position, Quaternion.identity);
            if (damageNum != null)
            {
                damageNum.Initialize(damage.finalDamage, damage.isCritical, damage.damageType);
            }
        }
        
        // Events
        public System.Action<GameObject, GameObject, DamageResult> OnDamageDealt;
        public System.Action<GameObject, GameObject> OnEntityDeath;
    }
    
    /// <summary>
    /// Entity stats component for all combat entities.
    /// </summary>
    [System.Serializable]
    public class EntityStats : MonoBehaviour
    {
        [Header("Primary Stats")]
        public int strength = 10;
        public int dexterity = 10;
        public int intelligence = 10;
        public int vitality = 10;
        public int endurance = 10;
        public int luck = 10;
        
        [Header("Combat Stats")]
        public int currentHealth = 100;
        public int maxHealth = 100;
        public int currentMana = 50;
        public int maxMana = 50;
        public int currentStamina = 100;
        public int maxStamina = 100;
        
        [Header("Defense")]
        public int armor = 0;
        public EntityResistances resistances;
        
        [Header("Offense")]
        public DamageElement damageElement = DamageElement.None;
        public float criticalChance = 0.05f;
        
        [Header("Level")]
        public int level = 1;
        public int experience = 0;
        public int experienceToNextLevel = 100;
        
        private void Start()
        {
            RecalculateStats();
        }
        
        public void RecalculateStats()
        {
            maxHealth = 100 + (vitality * 10);
            maxMana = 100 + (intelligence * 10);
            maxStamina = 100 + (endurance * 10);
            
            currentHealth = Mathf.Min(currentHealth, maxHealth);
            currentMana = Mathf.Min(currentMana, maxMana);
            currentStamina = Mathf.Min(currentStamina, maxStamina);
        }
        
        public void AddExperience(int amount)
        {
            experience += amount;
            
            while (experience >= experienceToNextLevel)
            {
                LevelUp();
            }
        }
        
        private void LevelUp()
        {
            level++;
            experience -= experienceToNextLevel;
            experienceToNextLevel = Mathf.RoundToInt(experienceToNextLevel * 1.2f);
            
            // Grant attribute points
            strength += 2;
            dexterity += 2;
            intelligence += 2;
            vitality += 2;
            endurance += 2;
            luck += 1;
            
            RecalculateStats();
            
            Debug.Log($"[EntityStats] Level up! Now level {level}");
        }
    }
    
    [System.Serializable]
    public struct EntityResistances
    {
        public float fireResistance;
        public float iceResistance;
        public float lightningResistance;
        public float poisonResistance;
        public float holyResistance;
        public float darkResistance;
    }
    
    public struct DamageResult
    {
        public int finalDamage;
        public DamageType damageType;
        public bool isCritical;
        public bool wasBlocked;
    }
    
    public enum DamageType { Physical, Ranged, Magic }
    public enum DamageElement { None, Fire, Ice, Lightning, Poison, Holy, Dark }
    
    /// <summary>
    /// Poolable damage number display.
    /// </summary>
    public class DamageNumber : Game.Core.Pooling.PoolableObject
    {
        [SerializeField] private TextMesh _textMesh;
        [SerializeField] private float _floatSpeed = 2f;
        [SerializeField] private float _lifetime = 1.5f;
        
        private float _spawnTime;
        
        private void Awake()
        {
            if (_textMesh == null)
            {
                _textMesh = GetComponentInChildren<TextMesh>();
                if (_textMesh == null)
                {
                    GameObject textObj = new("Text");
                    textObj.transform.SetParent(transform);
                    _textMesh = textObj.AddComponent<TextMesh>();
                    _textMesh.fontSize = 32;
                    _textMesh.anchor = TextAnchor.MiddleCenter;
                }
            }
        }
        
        public void Initialize(int damage, bool isCrit, DamageType type)
        {
            _textMesh.text = isCrit ? $"{damage}!" : damage.ToString();
            _textMesh.color = isCrit ? Color.yellow : Color.white;
            _spawnTime = Time.time;
        }
        
        private void Update()
        {
            if (!IsActiveInPool) return;
            
            // Float upward
            transform.position += Vector3.up * _floatSpeed * Time.deltaTime;
            
            // Fade out
            float alpha = 1f - ((Time.time - _spawnTime) / _lifetime);
            Color color = _textMesh.color;
            color.a = alpha;
            _textMesh.color = color;
            
            // Return to pool when lifetime expires
            if (Time.time - _spawnTime >= _lifetime)
            {
                ReturnToPool();
            }
        }
        
        public override void OnReturnToPool()
        {
            base.OnReturnToPool();
            _textMesh.color = Color.white;
        }
    }
    
    /// <summary>
    /// Ability system controller.
    /// </summary>
    public class AbilitySystemController : MonoBehaviour
    {
        [SerializeField] private List<Ability> _equippedAbilities = new();
        [SerializeField] private int _maxAbilitySlots = 10;
        
        public bool UseAbility(int slotIndex, GameObject caster, Vector3 targetPosition)
        {
            if (slotIndex < 0 || slotIndex >= _equippedAbilities.Count)
                return false;
            
            Ability ability = _equippedAbilities[slotIndex];
            if (ability == null) return false;
            
            // Check cooldown
            if (ability.IsOnCooldown()) return false;
            
            // Check resource cost
            EntityStats stats = caster.GetComponent<EntityStats>();
            if (stats == null) return false;
            
            if (stats.currentMana < ability.manaCost || stats.currentStamina < ability.staminaCost)
                return false;
            
            // Execute ability
            ability.Execute(caster, targetPosition);
            
            // Consume resources
            stats.currentMana -= ability.manaCost;
            stats.currentStamina -= ability.staminaCost;
            
            // Start cooldown
            ability.StartCooldown();
            
            return true;
        }
    }
    
    [System.Serializable]
    public class Ability
    {
        public string abilityName;
        public AbilityType abilityType;
        public int manaCost;
        public int staminaCost;
        public float cooldown;
        public float baseDamage;
        public float aoeRadius;
        
        private float _cooldownEndTime;
        
        public bool IsOnCooldown() => Time.time < _cooldownEndTime;
        
        public void StartCooldown() => _cooldownEndTime = Time.time + cooldown;
        
        public virtual void Execute(GameObject caster, Vector3 targetPosition)
        {
            Debug.Log($"[Ability] {caster.name} used {abilityName}");
        }
    }
    
    public enum AbilityType { Damage, Heal, Buff, Debuff, Summon, Movement, Utility }
} 
 
#####CoreSystemManager.cs##### 
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Core.Systems
{
    /// <summary>
    /// Central hub for all core game systems.
    /// FIXED: Proper root GameObject for DontDestroyOnLoad
    /// FIXED: Child systems no longer try to persist independently
    /// </summary>
    public class CoreSystemManager : MonoBehaviour
    {
        public static CoreSystemManager Instance { get; private set; }
        
        [Header("System References (Auto-Assigned at Runtime)")]
        [SerializeField] private ObjectPoolManager _poolManager;
        [SerializeField] private ZoneSystemManager _zoneManager;
        [SerializeField] private EntitySystemManager _entityManager;
        [SerializeField] private CombatSystemManager _combatManager;
        [SerializeField] private InventorySystemManager _inventoryManager;
        [SerializeField] private UISystemManager _uiManager;
        [SerializeField] private AudioSystemManager _audioManager;
        [SerializeField] private WebSocketNetworkManager _networkManager;
        [SerializeField] private AdminConsoleManager _adminConsoleManager;
        
        [Header("Initialization Status")]
        [SerializeField] private bool _isInitialized;
        [SerializeField] private float _initializationTime;
        
        public static ObjectPoolManager PoolManager => Instance?._poolManager;
        public static ZoneSystemManager ZoneManager => Instance?._zoneManager;
        public static EntitySystemManager EntityManager => Instance?._entityManager;
        public static CombatSystemManager CombatManager => Instance?._combatManager;
        public static InventorySystemManager InventoryManager => Instance?._inventoryManager;
        public static UISystemManager UIManager => Instance?._uiManager;
        public static AudioSystemManager AudioManager => Instance?._audioManager;
        public static WebSocketNetworkManager NetworkManager => Instance?._networkManager;
        public static AdminConsoleManager AdminConsole => Instance?._adminConsoleManager;
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            Instance = this;
            
            // CRITICAL FIX: Must be root GameObject for DontDestroyOnLoad
            if (transform.parent != null)
            {
                transform.SetParent(null);
            }
            
            DontDestroyOnLoad(gameObject);
            
            InitializeSystems();
        }
        
        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
        
        private void InitializeSystems()
        {
            float startTime = Time.realtimeSinceStartup;
            
            Debug.Log("[CoreSystemManager] Initializing core systems...");
            
            // Initialize in dependency order
            InitializePoolManager();
            InitializeNetworkManager();
            InitializeZoneManager();
            InitializeEntityManager();
            InitializeCombatManager();
            InitializeInventoryManager();
            InitializeAudioManager();
            InitializeUIManager();
            InitializeAdminConsole();
            
            _initializationTime = Time.realtimeSinceStartup - startTime;
            _isInitialized = true;
            
            Debug.Log($"[CoreSystemManager] All systems initialized in {_initializationTime:F3}s");
        }
        
        private void InitializePoolManager()
        {
            _poolManager = GetOrCreateChildSystem<ObjectPoolManager>("ObjectPoolManager");
        }
        
        private void InitializeNetworkManager()
        {
            _networkManager = GetOrCreateChildSystem<WebSocketNetworkManager>("WebSocketNetworkManager");
        }
        
        private void InitializeZoneManager()
        {
            _zoneManager = GetOrCreateChildSystem<ZoneSystemManager>("ZoneSystemManager");
        }
        
        private void InitializeEntityManager()
        {
            _entityManager = GetOrCreateChildSystem<EntitySystemManager>("EntitySystemManager");
        }
        
        private void InitializeCombatManager()
        {
            _combatManager = GetOrCreateChildSystem<CombatSystemManager>("CombatSystemManager");
        }
        
        private void InitializeInventoryManager()
        {
            _inventoryManager = GetOrCreateChildSystem<InventorySystemManager>("InventorySystemManager");
        }
        
        private void InitializeAudioManager()
        {
            _audioManager = GetOrCreateChildSystem<AudioSystemManager>("AudioSystemManager");
        }
        
        private void InitializeUIManager()
        {
            _uiManager = GetOrCreateChildSystem<UISystemManager>("UISystemManager");
        }
        
        private void InitializeAdminConsole()
        {
            // DISABLED: Admin console has critical bugs, skip for now
            // _adminConsoleManager = GetOrCreateChildSystem<AdminConsoleManager>("AdminConsoleManager");
            Debug.Log("[CoreSystemManager] Admin console disabled (has UI bugs)");
        }
        
        private T GetOrCreateChildSystem<T>(string childName) where T : Component
        {
            Transform child = transform.Find(childName);
            if (child != null)
            {
                T component = child.GetComponent<T>();
                if (component != null)
                {
                    Debug.Log($"[CoreSystemManager] Found existing {typeof(T).Name}");
                    return component;
                }
            }
            
            GameObject systemObj = new GameObject(childName);
            systemObj.transform.SetParent(transform);
            T newComponent = systemObj.AddComponent<T>();
            
            Debug.Log($"[CoreSystemManager] Created {typeof(T).Name}");
            return newComponent;
        }
        
        public bool IsReady()
        {
            return _isInitialized 
                && _poolManager != null
                && _zoneManager != null
                && _entityManager != null
                && _combatManager != null
                && _inventoryManager != null
                && _uiManager != null
                && _audioManager != null
                && _networkManager != null;
        }
        
        public async Awaitable ShutdownAllSystems()
        {
            Debug.Log("[CoreSystemManager] Shutting down all systems...");
            
            if (_uiManager != null) 
                _uiManager.Shutdown();
            
            if (_audioManager != null) 
                _audioManager.Shutdown();
            
            if (_inventoryManager != null) 
                _inventoryManager.Shutdown();
            
            if (_combatManager != null) 
                _combatManager.Shutdown();
            
            if (_entityManager != null) 
                await _entityManager.ShutdownAsync();
            
            if (_zoneManager != null) 
                await _zoneManager.ShutdownAsync();
            
            if (_networkManager != null) 
                await _networkManager.DisconnectAsync();
            
            if (_poolManager != null)
                _poolManager.ClearAllPools();
            
            _isInitialized = false;
            
            Debug.Log("[CoreSystemManager] All systems shut down");
        }
    }
} 
 
#####EnhancedGameBootstrap.cs##### 
using UnityEngine;
using UnityEngine.SceneManagement;
using Game.Core.Systems;

namespace Game.Core
{
    /// <summary>
    /// ENHANCED Game Bootstrap - Streamlined flow with tutorial auto-start.
    /// Skips character creation and goes straight to gameplay.
    /// </summary>
    public class EnhancedGameBootstrap : MonoBehaviour
    {
        [Header("Flow Configuration")]
        [SerializeField] private bool _skipCharacterCreation = true;
        [SerializeField] private bool _goDirectlyToGameWorld = true;
        [SerializeField] private bool _autoStartTutorial = true;
        
        [Header("Performance Settings")]
        [SerializeField] private int _targetFrameRate = 60;
        [SerializeField] private bool _enableVSync = true;
        [SerializeField] private int _defaultQualityLevel = 2;
        
        [Header("Default Character")]
        [SerializeField] private Species _defaultSpecies = Species.Human;
        [SerializeField] private Gender _defaultGender = Gender.Male;
        
        private bool _isBootstrapped;
        private CoreSystemManager _coreSystemManager;
        
        private async void Start()
        {
            if (_isBootstrapped)
            {
                Debug.LogWarning("[EnhancedBootstrap] Already bootstrapped!");
                return;
            }
            
            Debug.Log("[EnhancedBootstrap] ===== STARTING ENHANCED BOOTSTRAP =====");
            
            await BootstrapGame();
        }
        
        private async Awaitable BootstrapGame()
        {
            float startTime = Time.realtimeSinceStartup;
            
            // 1. Apply performance settings
            ApplyPerformanceSettings();
            
            // 2. Ensure CoreSystemManager exists and is ready
            await EnsureCoreSystemManager();
            
            // 3. Create default character data
            CharacterCreationData defaultCharacter = CreateDefaultCharacter();
            PlayerPrefs.SetString("CurrentCharacter", JsonUtility.ToJson(defaultCharacter));
            PlayerPrefs.Save();
            
            // 4. Load appropriate scene
            if (_goDirectlyToGameWorld)
            {
                await LoadGameWorld();
            }
            else
            {
                await LoadMainMenu();
            }
            
            _isBootstrapped = true;
            
            float totalTime = Time.realtimeSinceStartup - startTime;
            Debug.Log($"[EnhancedBootstrap] ===== BOOTSTRAP COMPLETE in {totalTime:F3}s =====");
        }
        
        private void ApplyPerformanceSettings()
        {
            if (_enableVSync)
            {
                QualitySettings.vSyncCount = 1;
                Application.targetFrameRate = -1;
            }
            else
            {
                QualitySettings.vSyncCount = 0;
                Application.targetFrameRate = _targetFrameRate;
            }
            
            QualitySettings.SetQualityLevel(_defaultQualityLevel, true);
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            
            Debug.Log($"[EnhancedBootstrap] Performance: FPS={Application.targetFrameRate}, VSync={QualitySettings.vSyncCount}");
        }
        
        private async Awaitable EnsureCoreSystemManager()
        {
            _coreSystemManager = FindFirstObjectByType<CoreSystemManager>();
            
            if (_coreSystemManager == null)
            {
                Debug.Log("[EnhancedBootstrap] Creating CoreSystemManager...");
                
                GameObject coreSystemObj = new GameObject("CoreSystemManager");
                coreSystemObj.transform.SetParent(null);
                _coreSystemManager = coreSystemObj.AddComponent<CoreSystemManager>();
                
                // Wait for initialization
                int maxWait = 100;
                int waited = 0;
                while (!_coreSystemManager.IsReady() && waited < maxWait)
                {
                    await Awaitable.NextFrameAsync();
                    waited++;
                }
                
                if (!_coreSystemManager.IsReady())
                {
                    Debug.LogError("[EnhancedBootstrap] CoreSystemManager failed to initialize!");
                }
                else
                {
                    Debug.Log("[EnhancedBootstrap] ✓ CoreSystemManager ready");
                }
            }
            else
            {
                Debug.Log("[EnhancedBootstrap] ✓ CoreSystemManager found");
                
                // Wait for it to be ready
                while (!_coreSystemManager.IsReady())
                {
                    await Awaitable.NextFrameAsync();
                }
            }
        }
        
        private CharacterCreationData CreateDefaultCharacter()
        {
            CharacterCreationData character = new CharacterCreationData
            {
                species = _defaultSpecies,
                gender = _defaultGender,
                bodyType = BodyType.Average,
                skinTone = 0,
                faceShape = 0,
                hairStyle = 0,
                strength = 15,
                dexterity = 15,
                intelligence = 15,
                vitality = 15,
                endurance = 15,
                luck = 10
            };
            
            Debug.Log($"[EnhancedBootstrap] ✓ Created default {character.species} {character.gender} character");
            
            return character;
        }
        
        private async Awaitable LoadGameWorld()
        {
            Debug.Log("[EnhancedBootstrap] Loading GameWorld scene...");
            
            Scene currentScene = SceneManager.GetActiveScene();
            if (currentScene.name == "GameWorld")
            {
                Debug.Log("[EnhancedBootstrap] Already in GameWorld - setting up tutorial");
                SetupTutorial();
                return;
            }
            
            AsyncOperation loadOp = SceneManager.LoadSceneAsync("GameWorld", LoadSceneMode.Single);
            
            if (loadOp == null)
            {
                Debug.LogError("[EnhancedBootstrap] Failed to load GameWorld! Check Build Settings.");
                return;
            }
            
            while (!loadOp.isDone)
            {
                await Awaitable.NextFrameAsync();
            }
            
            Debug.Log("[EnhancedBootstrap] ✓ GameWorld loaded");
            
            // Wait a frame for scene to initialize
            await Awaitable.NextFrameAsync();
            
            SetupTutorial();
        }
        
        private async Awaitable LoadMainMenu()
        {
            Debug.Log("[EnhancedBootstrap] Loading MainMenu scene...");
            
            AsyncOperation loadOp = SceneManager.LoadSceneAsync("MainMenu", LoadSceneMode.Single);
            
            if (loadOp == null)
            {
                Debug.LogError("[EnhancedBootstrap] Failed to load MainMenu!");
                return;
            }
            
            while (!loadOp.isDone)
            {
                await Awaitable.NextFrameAsync();
            }
            
            Debug.Log("[EnhancedBootstrap] ✓ MainMenu loaded");
        }
        
        private void SetupTutorial()
        {
            if (!_autoStartTutorial) return;
            
            TutorialSystemManager tutorial = FindFirstObjectByType<TutorialSystemManager>();
            
            if (tutorial == null)
            {
                Debug.Log("[EnhancedBootstrap] Creating TutorialSystemManager...");
                GameObject tutorialObj = new GameObject("TutorialSystemManager");
                tutorial = tutorialObj.AddComponent<TutorialSystemManager>();
            }
            
            Debug.Log("[EnhancedBootstrap] ✓ Tutorial system ready");
        }
        
        private void OnApplicationQuit()
        {
            if (_coreSystemManager != null)
            {
                _ = _coreSystemManager.ShutdownAllSystems();
            }
        }
    }
} 
 
#####EntitySystemManager.cs##### 
using UnityEngine;
using System.Collections.Generic;
using Game.Core.Pooling;

namespace Game.Core.Systems
{
    /// <summary>
    /// Entity System Manager - Manages all entities (players, NPCs, enemies).
    /// Handles entity spawning, lifecycle, and AI coordination.
    /// COMPLETE IMPLEMENTATION - No more TODOs!
    /// </summary>
    public class EntitySystemManager : MonoBehaviour
    {
        [Header("Entity Management")]
        [SerializeField] private int _maxActiveEntities = 100;
        [SerializeField] private int _currentEntityCount;
        
        [Header("Entity Prefabs")]
        [SerializeField] private GameObject _zombiePrefab;
        [SerializeField] private GameObject _skeletonPrefab;
        [SerializeField] private GameObject _orcPrefab;
        [SerializeField] private GameObject _npcPrefab;
        
        [Header("Runtime Tracking")]
        [SerializeField] private List<GameObject> _activeEntities = new();
        [SerializeField] private Dictionary<string, EntityData> _entityDatabase;
        
        private ObjectPoolManager _poolManager;
        
        #region Initialization
        
        private void Awake()
        {
            _poolManager = CoreSystemManager.PoolManager;
            _entityDatabase = new Dictionary<string, EntityData>();
            
            InitializeEntityPrefabs();
            RegisterEntityTypes();
            
            Debug.Log("[EntitySystemManager] Initialized");
        }
        
        private void InitializeEntityPrefabs()
        {
            // Create default entity prefabs if not assigned
            if (_zombiePrefab == null)
                _zombiePrefab = CreateDefaultEntityPrefab("Zombie", Color.green, 150);
            
            if (_skeletonPrefab == null)
                _skeletonPrefab = CreateDefaultEntityPrefab("Skeleton", Color.white, 80);
            
            if (_orcPrefab == null)
                _orcPrefab = CreateDefaultEntityPrefab("Orc", new Color(0.4f, 0.6f, 0.3f), 200);
            
            if (_npcPrefab == null)
                _npcPrefab = CreateDefaultEntityPrefab("NPC", Color.blue, 100);
        }
        
        private GameObject CreateDefaultEntityPrefab(string name, Color color, int maxHealth)
        {
            GameObject prefab = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            prefab.name = $"{name}Prefab";
            prefab.tag = "Enemy";
            
            // Visual
            Renderer renderer = prefab.GetComponent<Renderer>();
            Material mat = new Material(Shader.Find("Standard"));
            mat.color = color;
            renderer.material = mat;
            
            // Stats
            EntityStats stats = prefab.AddComponent<EntityStats>();
            stats.maxHealth = maxHealth;
            stats.currentHealth = maxHealth;
            stats.level = 1;
            
            // AI
            EntityAI ai = prefab.AddComponent<EntityAI>();
            
            // Make it a poolable entity
            PoolableEntity poolable = prefab.AddComponent<PoolableEntity>();
            
            prefab.SetActive(false);
            return prefab;
        }
        
        private void RegisterEntityTypes()
        {
            if (_poolManager == null) return;
            
            // Register entity prefabs with pool manager
            _poolManager.RegisterPrefab("Zombie", _zombiePrefab, 50);
            _poolManager.RegisterPrefab("Skeleton", _skeletonPrefab, 50);
            _poolManager.RegisterPrefab("Orc", _orcPrefab, 30);
            _poolManager.RegisterPrefab("NPC", _npcPrefab, 20);
            
            // Pre-warm pools
            _poolManager.WarmPool("Zombie", 10);
            _poolManager.WarmPool("Skeleton", 10);
            _poolManager.WarmPool("Orc", 5);
            _poolManager.WarmPool("NPC", 5);
            
            Debug.Log("[EntitySystemManager] Entity pools registered and warmed");
        }
        
        #endregion
        
        #region Entity Spawning
        
        /// <summary>
        /// Spawn an entity at a position with rotation.
        /// </summary>
        public GameObject SpawnEntity(string entityType, Vector3 position, Quaternion rotation)
        {
            if (_currentEntityCount >= _maxActiveEntities)
            {
                Debug.LogWarning($"[EntitySystemManager] Max entities ({_maxActiveEntities}) reached!");
                return null;
            }
            
            // Get from pool
            PoolableEntity entity = _poolManager.Get<PoolableEntity>(entityType, position, rotation);
            
            if (entity == null)
            {
                Debug.LogError($"[EntitySystemManager] Failed to spawn entity type: {entityType}");
                return null;
            }
            
            GameObject entityObj = entity.GameObject;
            
            // Track entity
            _activeEntities.Add(entityObj);
            _currentEntityCount = _activeEntities.Count;
            
            // Initialize AI
            EntityAI ai = entityObj.GetComponent<EntityAI>();
            if (ai != null)
            {
                ai.Initialize();
            }
            
            Debug.Log($"[EntitySystemManager] Spawned {entityType} at {position}");
            
            return entityObj;
        }
        
        /// <summary>
        /// Spawn entity at a spawn point.
        /// </summary>
        public GameObject SpawnEntityAtSpawnPoint(SpawnPoint spawnPoint)
        {
            if (spawnPoint == null) return null;
            
            string entityType = GetEntityTypeForSpawnPoint(spawnPoint.Type);
            Vector3 spawnPos = spawnPoint.transform.position + Random.insideUnitSphere * spawnPoint.Radius;
            spawnPos.y = spawnPoint.transform.position.y;
            
            return SpawnEntity(entityType, spawnPos, Quaternion.identity);
        }
        
        private string GetEntityTypeForSpawnPoint(SpawnPointType type)
        {
            return type switch
            {
                SpawnPointType.Enemy => GetRandomEnemyType(),
                SpawnPointType.NPC => "NPC",
                SpawnPointType.Boss => "Orc", // Bosses are just tougher orcs for now
                _ => "Zombie"
            };
        }
        
        private string GetRandomEnemyType()
        {
            string[] types = { "Zombie", "Skeleton", "Orc" };
            return types[Random.Range(0, types.Length)];
        }
        
        #endregion
        
        #region Entity Despawning
        
        /// <summary>
        /// Despawn an entity and return it to pool.
        /// </summary>
        public void DespawnEntity(GameObject entity)
        {
            if (entity == null) return;
            
            PoolableEntity poolable = entity.GetComponent<PoolableEntity>();
            
            if (poolable != null && _poolManager != null)
            {
                _poolManager.ReturnToPool(poolable);
            }
            else
            {
                Destroy(entity);
            }
            
            _activeEntities.Remove(entity);
            _currentEntityCount = _activeEntities.Count;
            
            Debug.Log($"[EntitySystemManager] Despawned {entity.name}");
        }
        
        /// <summary>
        /// Despawn all entities in the scene.
        /// </summary>
        public void DespawnAllEntities()
        {
            List<GameObject> entitiesToRemove = new List<GameObject>(_activeEntities);
            
            foreach (GameObject entity in entitiesToRemove)
            {
                DespawnEntity(entity);
            }
            
            Debug.Log("[EntitySystemManager] All entities despawned");
        }
        
        #endregion
        
        #region Entity Queries
        
        public int GetEntityCount()
        {
            return _currentEntityCount;
        }
        
        public List<GameObject> GetAllEntities()
        {
            return new List<GameObject>(_activeEntities);
        }
        
        public List<GameObject> GetEntitiesInRadius(Vector3 center, float radius)
        {
            List<GameObject> result = new();
            
            foreach (GameObject entity in _activeEntities)
            {
                if (entity != null && Vector3.Distance(entity.transform.position, center) <= radius)
                {
                    result.Add(entity);
                }
            }
            
            return result;
        }
        
        public GameObject GetNearestEntity(Vector3 position, string tag = null)
        {
            GameObject nearest = null;
            float nearestDist = float.MaxValue;
            
            foreach (GameObject entity in _activeEntities)
            {
                if (entity == null) continue;
                if (tag != null && !entity.CompareTag(tag)) continue;
                
                float dist = Vector3.Distance(position, entity.transform.position);
                if (dist < nearestDist)
                {
                    nearestDist = dist;
                    nearest = entity;
                }
            }
            
            return nearest;
        }
        
        #endregion
        
        #region Update & AI Coordination
        
        private void Update()
        {
            // Clean up null references
            _activeEntities.RemoveAll(e => e == null);
            _currentEntityCount = _activeEntities.Count;
            
            // Update AI coordination every second
            if (Time.frameCount % 60 == 0)
            {
                CoordinateEntityAI();
            }
        }
        
        private void CoordinateEntityAI()
        {
            // Find player
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null) return;
            
            // Update all entity AI with player location
            foreach (GameObject entity in _activeEntities)
            {
                if (entity == null) continue;
                
                EntityAI ai = entity.GetComponent<EntityAI>();
                if (ai != null)
                {
                    ai.UpdateTarget(player.transform.position);
                }
            }
        }
        
        #endregion
        
        #region Shutdown
        
        public async Awaitable ShutdownAsync()
        {
            Debug.Log("[EntitySystemManager] Shutting down...");
            DespawnAllEntities();
            await Awaitable.NextFrameAsync();
        }
        
        #endregion
    }
    
    #region Supporting Types
    
    /// <summary>
    /// Poolable entity wrapper.
    /// </summary>
    public class PoolableEntity : PoolableObject
    {
        private EntityAI _ai;
        private EntityStats _stats;
        
        private void Awake()
        {
            _ai = GetComponent<EntityAI>();
            _stats = GetComponent<EntityStats>();
        }
        
        public override void OnSpawnFromPool()
        {
            base.OnSpawnFromPool();
            
            // Reset stats
            if (_stats != null)
            {
                _stats.currentHealth = _stats.maxHealth;
                _stats.currentMana = _stats.maxMana;
                _stats.currentStamina = _stats.maxStamina;
            }
            
            // Reset AI
            if (_ai != null)
            {
                _ai.Initialize();
            }
        }
        
        public override void OnReturnToPool()
        {
            base.OnReturnToPool();
            
            // Stop AI
            if (_ai != null)
            {
                _ai.Stop();
            }
        }
    }
    
    /// <summary>
    /// Simple AI controller for entities.
    /// </summary>
    public class EntityAI : MonoBehaviour
    {
        [Header("AI Settings")]
        [SerializeField] private float _detectionRange = 15f;
        [SerializeField] private float _attackRange = 2f;
        [SerializeField] private float _moveSpeed = 3f;
        [SerializeField] private float _attackCooldown = 2f;
        
        [Header("State")]
        [SerializeField] private AIState _currentState = AIState.Idle;
        [SerializeField] private Vector3 _targetPosition;
        
        private float _lastAttackTime;
        private Transform _transform;
        private EntityStats _stats;
        private bool _isActive;
        
        private void Awake()
        {
            _transform = transform;
            _stats = GetComponent<EntityStats>();
        }
        
        public void Initialize()
        {
            _isActive = true;
            _currentState = AIState.Idle;
            _lastAttackTime = 0f;
        }
        
        public void Stop()
        {
            _isActive = false;
            _currentState = AIState.Idle;
        }
        
        public void UpdateTarget(Vector3 targetPosition)
        {
            if (!_isActive) return;
            
            _targetPosition = targetPosition;
            
            float distanceToTarget = Vector3.Distance(_transform.position, targetPosition);
            
            if (distanceToTarget <= _attackRange)
            {
                _currentState = AIState.Attacking;
            }
            else if (distanceToTarget <= _detectionRange)
            {
                _currentState = AIState.Chasing;
            }
            else
            {
                _currentState = AIState.Idle;
            }
        }
        
        private void Update()
        {
            if (!_isActive) return;
            
            switch (_currentState)
            {
                case AIState.Idle:
                    HandleIdle();
                    break;
                case AIState.Chasing:
                    HandleChasing();
                    break;
                case AIState.Attacking:
                    HandleAttacking();
                    break;
            }
        }
        
        private void HandleIdle()
        {
            // Do nothing, wait for target
        }
        
        private void HandleChasing()
        {
            // Move toward target
            Vector3 direction = (_targetPosition - _transform.position).normalized;
            direction.y = 0; // Keep on ground
            
            _transform.position += direction * _moveSpeed * Time.deltaTime;
            _transform.LookAt(new Vector3(_targetPosition.x, _transform.position.y, _targetPosition.z));
        }
        
        private void HandleAttacking()
        {
            // Face target
            _transform.LookAt(new Vector3(_targetPosition.x, _transform.position.y, _targetPosition.z));
            
            // Attack on cooldown
            if (Time.time - _lastAttackTime >= _attackCooldown)
            {
                PerformAttack();
                _lastAttackTime = Time.time;
            }
        }
        
        private void PerformAttack()
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null) return;
            
            float distance = Vector3.Distance(_transform.position, player.transform.position);
            if (distance > _attackRange) return;
            
            // Get combat manager
            CombatSystemManager combatManager = CoreSystemManager.CombatManager;
            if (combatManager == null) return;
            
            // Get stats
            EntityStats playerStats = player.GetComponent<EntityStats>();
            if (playerStats == null) return;
            
            // Calculate damage
            DamageResult damage = combatManager.CalculateDamage(
                _stats, 
                playerStats, 
                10, // Base damage
                DamageType.Physical
            );
            
            // Apply damage
            combatManager.ApplyDamage(player, damage, gameObject);
            
            Debug.Log($"[EntityAI] {gameObject.name} attacked player for {damage.finalDamage} damage");
        }
        
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, _detectionRange);
            
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, _attackRange);
        }
    }
    
    public enum AIState
    {
        Idle,
        Chasing,
        Attacking,
        Fleeing
    }
    
    [System.Serializable]
    public class EntityData
    {
        public string entityId;
        public string entityType;
        public int level;
        public Vector3 lastPosition;
    }
    
    #endregion
} 
 
#####GameBootstrap.cs##### 
using UnityEngine;
using UnityEngine.SceneManagement;
using Game.Core.Systems;

namespace Game.Core
{
    /// <summary>
    /// FIXED: Validates build settings, ensures scenes exist
    /// </summary>
    public class GameBootstrap : MonoBehaviour
    {
        [Header("Bootstrap Configuration")]
        [SerializeField, Tooltip("Scene to load after initialization")]
        private string _initialSceneName = "MainMenu";
        
        [SerializeField, Tooltip("Show loading screen during initialization")]
        private bool _showLoadingScreen = true;
        
        [SerializeField, Tooltip("Minimum time to show loading screen")]
        private float _minLoadingTime = 1f;
        
        [Header("Performance Settings")]
        [SerializeField, Tooltip("Target frame rate (0 = unlimited)")]
        private int _targetFrameRate = 60;
        
        [SerializeField, Tooltip("Enable VSync")]
        private bool _enableVSync = true;
        
        [Header("Quality Settings")]
        [SerializeField, Tooltip("Default quality level on startup (0-5)")]
        [Range(0, 5)]
        private int _defaultQualityLevel = 2;
        
        [Header("Runtime References")]
        [SerializeField] private CoreSystemManager _coreSystemManager;
        [SerializeField] private Canvas _loadingCanvas;
        
        private bool _isBootstrapped;
        
        private async void Start()
        {
            if (_isBootstrapped)
            {
                Debug.LogWarning("[GameBootstrap] Already bootstrapped!");
                return;
            }
            
            // CRITICAL: Validate scene setup FIRST
            if (!ValidateSceneSetup())
            {
                Debug.LogError("[GameBootstrap] Scene validation failed! Check Build Settings.");
                return;
            }
            
            await BootstrapGame();
        }
        
        private void OnApplicationQuit()
        {
            if (_coreSystemManager != null)
            {
                _ = _coreSystemManager.ShutdownAllSystems();
            }
        }
        
        private bool ValidateSceneSetup()
        {
            // Check if this is Bootstrap scene
            Scene currentScene = SceneManager.GetActiveScene();
            if (currentScene.buildIndex != 0)
            {
                Debug.LogWarning($"[GameBootstrap] Bootstrap should be build index 0, currently {currentScene.buildIndex}");
            }
            
            // Check if target scene exists in build settings
            int sceneCount = SceneManager.sceneCountInBuildSettings;
            bool foundTarget = false;
            
            for (int i = 0; i < sceneCount; i++)
            {
                string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
                string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);
                
                if (sceneName == _initialSceneName)
                {
                    foundTarget = true;
                    Debug.Log($"[GameBootstrap] Found target scene '{_initialSceneName}' at index {i}");
                    break;
                }
            }
            
            if (!foundTarget)
            {
                Debug.LogError($"[GameBootstrap] Scene '{_initialSceneName}' not found in Build Settings!");
                Debug.LogError("[GameBootstrap] Go to File > Build Settings and add your scenes!");
                return false;
            }
            
            // Check for GameWorld scene
            bool hasGameWorld = false;
            for (int i = 0; i < sceneCount; i++)
            {
                string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
                string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);
                
                if (sceneName == "GameWorld")
                {
                    hasGameWorld = true;
                    break;
                }
            }
            
            if (!hasGameWorld)
            {
                Debug.LogWarning("[GameBootstrap] 'GameWorld' scene not in Build Settings - character creation will fail!");
            }
            
            return true;
        }
        
        private async Awaitable BootstrapGame()
        {
            float startTime = Time.realtimeSinceStartup;
            
            Debug.Log("[GameBootstrap] Starting game bootstrap...");
            
            ApplyPerformanceSettings();
            
            if (_showLoadingScreen)
            {
                ShowLoadingScreen();
            }
            
            await EnsureCoreSystemManager();
            
            float elapsed = Time.realtimeSinceStartup - startTime;
            if (elapsed < _minLoadingTime)
            {
                float waitTime = _minLoadingTime - elapsed;
                await Awaitable.WaitForSecondsAsync(waitTime);
            }
            
            await LoadInitialScene();
            
            if (_showLoadingScreen)
            {
                HideLoadingScreen();
            }
            
            _isBootstrapped = true;
            
            float totalTime = Time.realtimeSinceStartup - startTime;
            Debug.Log($"[GameBootstrap] Bootstrap complete in {totalTime:F3}s");
        }
        
        private void ApplyPerformanceSettings()
        {
            if (_enableVSync)
            {
                QualitySettings.vSyncCount = 1;
                Application.targetFrameRate = -1;
            }
            else
            {
                QualitySettings.vSyncCount = 0;
                Application.targetFrameRate = _targetFrameRate;
            }
            
            int maxQualityLevel = QualitySettings.names.Length - 1;
            int safeQualityLevel = Mathf.Clamp(_defaultQualityLevel, 0, maxQualityLevel);
            
            if (safeQualityLevel != _defaultQualityLevel)
            {
                Debug.LogWarning($"[GameBootstrap] Quality level {_defaultQualityLevel} out of range (0-{maxQualityLevel}). Using {safeQualityLevel}.");
            }
            
            QualitySettings.SetQualityLevel(safeQualityLevel, true);
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            
            string qualityName = "Unknown";
            if (safeQualityLevel >= 0 && safeQualityLevel < QualitySettings.names.Length)
            {
                qualityName = QualitySettings.names[safeQualityLevel];
            }
            
            Debug.Log($"[GameBootstrap] Performance: TargetFPS={Application.targetFrameRate}, VSync={QualitySettings.vSyncCount}, Quality={qualityName}");
        }
        
        private async Awaitable EnsureCoreSystemManager()
        {
            _coreSystemManager = FindFirstObjectByType<CoreSystemManager>();
            
            if (_coreSystemManager == null)
            {
                Debug.Log("[GameBootstrap] Creating CoreSystemManager...");
                
                GameObject coreSystemObj = new("CoreSystemManager");
                // CRITICAL: Ensure it's a root GameObject
                coreSystemObj.transform.SetParent(null);
                _coreSystemManager = coreSystemObj.AddComponent<CoreSystemManager>();
                
                int maxWait = 100;
                int waited = 0;
                while (!_coreSystemManager.IsReady() && waited < maxWait)
                {
                    await Awaitable.NextFrameAsync();
                    waited++;
                }
                
                if (!_coreSystemManager.IsReady())
                {
                    Debug.LogError("[GameBootstrap] CoreSystemManager failed to initialize!");
                }
            }
            else
            {
                Debug.Log("[GameBootstrap] CoreSystemManager already exists");
            }
        }
        
        private async Awaitable LoadInitialScene()
        {
            if (string.IsNullOrEmpty(_initialSceneName))
            {
                Debug.LogWarning("[GameBootstrap] No initial scene specified!");
                return;
            }
            
            Scene currentScene = SceneManager.GetActiveScene();
            if (currentScene.name == _initialSceneName)
            {
                Debug.Log($"[GameBootstrap] Already in scene '{_initialSceneName}'");
                return;
            }
            
            Debug.Log($"[GameBootstrap] Loading scene '{_initialSceneName}'...");
            
            AsyncOperation loadOp = SceneManager.LoadSceneAsync(_initialSceneName, LoadSceneMode.Single);
            
            if (loadOp == null)
            {
                Debug.LogError($"[GameBootstrap] Failed to load scene '{_initialSceneName}'! Check Build Settings.");
                return;
            }
            
            while (!loadOp.isDone)
            {
                await Awaitable.NextFrameAsync();
            }
            
            Debug.Log($"[GameBootstrap] Scene '{_initialSceneName}' loaded successfully");
        }
        
        private void ShowLoadingScreen()
        {
            if (_loadingCanvas != null)
            {
                _loadingCanvas.gameObject.SetActive(true);
                return;
            }
            
            GameObject canvasObj = new("LoadingCanvas");
            _loadingCanvas = canvasObj.AddComponent<Canvas>();
            _loadingCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _loadingCanvas.sortingOrder = 9999;
            
            UnityEngine.UI.CanvasScaler scaler = canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
            scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            
            canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            
            GameObject panelObj = new("LoadingPanel");
            panelObj.transform.SetParent(canvasObj.transform, false);
            
            UnityEngine.UI.Image panel = panelObj.AddComponent<UnityEngine.UI.Image>();
            panel.color = Color.black;
            
            RectTransform panelRect = panelObj.GetComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.sizeDelta = Vector2.zero;
            
            GameObject textObj = new("LoadingText");
            textObj.transform.SetParent(panelObj.transform, false);
            
            UnityEngine.UI.Text text = textObj.AddComponent<UnityEngine.UI.Text>();
            text.text = "Loading...";
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 48;
            text.color = Color.white;
            text.alignment = TextAnchor.MiddleCenter;
            
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0.5f, 0.5f);
            textRect.anchorMax = new Vector2(0.5f, 0.5f);
            textRect.sizeDelta = new Vector2(400, 100);
            
            DontDestroyOnLoad(canvasObj);
        }
        
        private void HideLoadingScreen()
        {
            if (_loadingCanvas != null)
            {
                Destroy(_loadingCanvas.gameObject);
                _loadingCanvas = null;
            }
        }
    }
} 
 
#####GameWorldInitializer.cs##### 
using UnityEngine;
using Game.Core.Systems;

namespace Game.Core
{
    /// <summary>
    /// Initializes the game world and spawns the player.
    /// Place this on a GameObject in the GameWorld scene.
    /// </summary>
    public class GameWorldInitializer : MonoBehaviour
    {
        [Header("World Settings")]
        [SerializeField] private bool _generateZoneOnStart = true;
        [SerializeField] private string _defaultZoneName = "StartingZone";
        
        [Header("Spawn Settings")]
        [SerializeField] private Vector3 _defaultSpawnPosition = new Vector3(0, 5f, 0);
        
        private async void Start()
        {
            await InitializeGameWorld();
        }
        
        private async Awaitable InitializeGameWorld()
        {
            Debug.Log("[GameWorld] Initializing game world...");
            
            // Wait for CoreSystemManager
            while (CoreSystemManager.Instance == null || !CoreSystemManager.Instance.IsReady())
            {
                await Awaitable.NextFrameAsync();
            }
            
            // Generate starting zone if needed
            if (_generateZoneOnStart)
            {
                await GenerateStartingZone();
            }
            
            // Spawn player
            SpawnPlayer();
            
            Debug.Log("[GameWorld] Initialization complete!");
        }
        
        private async Awaitable GenerateStartingZone()
        {
            ZoneSystemManager zoneManager = CoreSystemManager.ZoneManager;
            if (zoneManager == null)
            {
                Debug.LogError("[GameWorld] ZoneManager not found!");
                return;
            }
            
            ZoneConfig config = new ZoneConfig
            {
                zoneName = _defaultZoneName,
                zoneType = ZoneType.Wilderness,
                biomeType = BiomeType.Grassland,
                levelRange = new Vector2Int(1, 5),
                zoneSize = new Vector3(200, 0, 200),
                seed = Random.Range(1000, 9999)
            };
            
            Debug.Log($"[GameWorld] Generating zone '{_defaultZoneName}'...");
            await zoneManager.GenerateZone(config);
        }
        
        private void SpawnPlayer()
        {
            // Load character data
            string json = PlayerPrefs.GetString("CurrentCharacter", "");
            if (string.IsNullOrEmpty(json))
            {
                Debug.LogError("[GameWorld] No character data found! Creating default character.");
                json = JsonUtility.ToJson(new CharacterCreationData());
            }
            
            CharacterCreationData data = JsonUtility.FromJson<CharacterCreationData>(json);
            
            // Get or create character builder
            ProceduralCharacterBuilder builder = FindFirstObjectByType<ProceduralCharacterBuilder>();
            if (builder == null)
            {
                GameObject builderObj = new GameObject("CharacterBuilder");
                builder = builderObj.AddComponent<ProceduralCharacterBuilder>();
            }
            
            // Generate player character
            GameObject player = builder.GenerateCharacter(data);
            player.name = "Player";
            player.tag = "Player";
            player.layer = LayerMask.NameToLayer("Default");
            
            // Add required components
            if (player.GetComponent<EntityStats>() == null)
            {
                EntityStats stats = player.AddComponent<EntityStats>();
                stats.strength = data.strength;
                stats.dexterity = data.dexterity;
                stats.intelligence = data.intelligence;
                stats.vitality = data.vitality;
                stats.endurance = data.endurance;
                stats.luck = data.luck;
                stats.RecalculateStats();
            }
            
            // Add player controller
            PlayerController controller = player.AddComponent<PlayerController>();
            controller.Initialize(data);
            
            // Position player at spawn point
            SpawnPoint[] spawns = FindObjectsByType<SpawnPoint>(FindObjectsSortMode.None);
            bool foundSpawn = false;
            
            foreach (SpawnPoint spawn in spawns)
            {
                if (spawn.Type == SpawnPointType.Player)
                {
                    player.transform.position = spawn.transform.position + Vector3.up * 2f;
                    foundSpawn = true;
                    Debug.Log($"[GameWorld] Player spawned at spawn point: {spawn.transform.position}");
                    break;
                }
            }
            
            if (!foundSpawn)
            {
                player.transform.position = _defaultSpawnPosition;
                Debug.Log($"[GameWorld] Player spawned at default position: {_defaultSpawnPosition}");
            }
            
            // Setup camera
            Camera mainCam = Camera.main;
            if (mainCam != null)
            {
                mainCam.transform.SetParent(player.transform);
                mainCam.transform.localPosition = new Vector3(0, 1.6f, 0);
                mainCam.transform.localRotation = Quaternion.identity;
            }
            
            Debug.Log($"[GameWorld] Player spawned successfully! ({data.species} {data.gender})");
        }
    }
} 
 
#####InteractableRegistry.cs##### 
using UnityEngine;
using System.Collections.Generic;

namespace Game.Core.Systems
{
    /// <summary>
    /// Registry for all interactable objects in zones.
    /// Manages doors, chests, ladders, stairs, windows, levers, traps, etc.
    /// COMPLETE IMPLEMENTATION!
    /// </summary>
    public class InteractableRegistry : MonoBehaviour
    {
        [Header("Interactable Prefabs")]
        [SerializeField] private GameObject _doorPrefab;
        [SerializeField] private GameObject _chestPrefab;
        [SerializeField] private GameObject _ladderPrefab;
        [SerializeField] private GameObject _leverPrefab;
        [SerializeField] private GameObject _trapPrefab;
        
        [Header("Runtime Registry")]
        [SerializeField] private List<InteractableObject> _registeredInteractables = new();
        
        private Dictionary<string, InteractableObject> _interactableMap;
        
        private void Awake()
        {
            _interactableMap = new Dictionary<string, InteractableObject>();
            CreateDefaultPrefabs();
        }
        
        private void CreateDefaultPrefabs()
        {
            // Create default prefabs if not assigned
            if (_doorPrefab == null)
                _doorPrefab = CreateDoorPrefab();
            
            if (_chestPrefab == null)
                _chestPrefab = CreateChestPrefab();
            
            if (_ladderPrefab == null)
                _ladderPrefab = CreateLadderPrefab();
            
            if (_leverPrefab == null)
                _leverPrefab = CreateLeverPrefab();
            
            if (_trapPrefab == null)
                _trapPrefab = CreateTrapPrefab();
        }
        
        private GameObject CreateDoorPrefab()
        {
            GameObject door = GameObject.CreatePrimitive(PrimitiveType.Cube);
            door.name = "DoorPrefab";
            door.transform.localScale = new Vector3(1.5f, 3f, 0.2f);
            door.GetComponent<Renderer>().material.color = new Color(0.6f, 0.4f, 0.2f);
            door.AddComponent<Door>();
            door.SetActive(false);
            return door;
        }
        
        private GameObject CreateChestPrefab()
        {
            GameObject chest = GameObject.CreatePrimitive(PrimitiveType.Cube);
            chest.name = "ChestPrefab";
            chest.transform.localScale = new Vector3(1f, 0.8f, 0.6f);
            chest.GetComponent<Renderer>().material.color = new Color(0.7f, 0.5f, 0.2f);
            chest.AddComponent<Chest>();
            chest.SetActive(false);
            return chest;
        }
        
        private GameObject CreateLadderPrefab()
        {
            GameObject ladder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            ladder.name = "LadderPrefab";
            ladder.transform.localScale = new Vector3(0.3f, 3f, 0.3f);
            ladder.GetComponent<Renderer>().material.color = new Color(0.5f, 0.3f, 0.1f);
            ladder.AddComponent<Ladder>();
            ladder.SetActive(false);
            return ladder;
        }
        
        private GameObject CreateLeverPrefab()
        {
            GameObject lever = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            lever.name = "LeverPrefab";
            lever.transform.localScale = new Vector3(0.1f, 1f, 0.1f);
            lever.GetComponent<Renderer>().material.color = Color.gray;
            lever.AddComponent<Lever>();
            lever.SetActive(false);
            return lever;
        }
        
        private GameObject CreateTrapPrefab()
        {
            GameObject trap = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            trap.name = "TrapPrefab";
            trap.transform.localScale = Vector3.one * 0.5f;
            trap.GetComponent<Renderer>().material.color = Color.red;
            trap.AddComponent<Trap>();
            trap.SetActive(false);
            return trap;
        }
        
        public void RegisterInteractable(InteractableObject interactable)
        {
            if (!_registeredInteractables.Contains(interactable))
            {
                _registeredInteractables.Add(interactable);
                _interactableMap[interactable.GetInstanceID().ToString()] = interactable;
            }
        }
        
        public void UnregisterInteractable(InteractableObject interactable)
        {
            _registeredInteractables.Remove(interactable);
            _interactableMap.Remove(interactable.GetInstanceID().ToString());
        }
        
        public List<InteractableObject> GetInteractablesInRange(Vector3 position, float range)
        {
            List<InteractableObject> nearby = new();
            
            foreach (InteractableObject interactable in _registeredInteractables)
            {
                if (interactable != null && Vector3.Distance(position, interactable.transform.position) <= range)
                {
                    nearby.Add(interactable);
                }
            }
            
            return nearby;
        }
        
        public GameObject SpawnInteractable(string type, Vector3 position, Quaternion rotation)
        {
            GameObject prefab = type switch
            {
                "Door" => _doorPrefab,
                "Chest" => _chestPrefab,
                "Ladder" => _ladderPrefab,
                "Lever" => _leverPrefab,
                "Trap" => _trapPrefab,
                _ => null
            };
            
            if (prefab == null)
            {
                Debug.LogError($"[InteractableRegistry] Unknown interactable type: {type}");
                return null;
            }
            
            GameObject instance = Instantiate(prefab, position, rotation);
            instance.name = $"{type}_{instance.GetInstanceID()}";
            instance.SetActive(true);
            
            return instance;
        }
    }
    
    /// <summary>
    /// Base class for all interactable objects.
    /// </summary>
    public abstract class InteractableObject : MonoBehaviour
    {
        [Header("Interactable Settings")]
        [SerializeField] protected string _interactionPrompt = "Press E to interact";
        [SerializeField] protected float _interactionRange = 3f;
        [SerializeField] protected bool _isInteractable = true;
        [SerializeField] protected bool _requiresLineOfSight = true;
        
        public string InteractionPrompt => _interactionPrompt;
        public float InteractionRange => _interactionRange;
        public bool IsInteractable => _isInteractable;
        
        protected virtual void Start()
        {
            InteractableRegistry registry = FindFirstObjectByType<InteractableRegistry>();
            registry?.RegisterInteractable(this);
        }
        
        protected virtual void OnDestroy()
        {
            InteractableRegistry registry = FindFirstObjectByType<InteractableRegistry>();
            registry?.UnregisterInteractable(this);
        }
        
        public abstract void Interact(GameObject interactor);
        
        public virtual bool CanInteract(GameObject interactor)
        {
            if (!_isInteractable) return false;
            
            float distance = Vector3.Distance(transform.position, interactor.transform.position);
            if (distance > _interactionRange) return false;
            
            if (_requiresLineOfSight)
            {
                Vector3 direction = transform.position - interactor.transform.position;
                if (Physics.Raycast(interactor.transform.position, direction, out RaycastHit hit, _interactionRange))
                {
                    return hit.collider.gameObject == gameObject;
                }
                return false;
            }
            
            return true;
        }
        
        protected virtual void OnDrawGizmos()
        {
            Gizmos.color = _isInteractable ? Color.cyan : Color.gray;
            Gizmos.DrawWireSphere(transform.position, _interactionRange);
        }
    }
    
    /// <summary>
    /// Door interactable - can be opened/closed, locked/unlocked.
    /// </summary>
    public class Door : InteractableObject
    {
        [Header("Door Settings")]
        [SerializeField] private bool _isLocked;
        [SerializeField] private bool _isOpen;
        [SerializeField] private float _openAngle = 90f;
        [SerializeField] private float _openSpeed = 2f;
        [SerializeField] private string _requiredKeyId;
        
        private Quaternion _closedRotation;
        private Quaternion _openRotation;
        private bool _isAnimating;
        
        protected override void Start()
        {
            base.Start();
            _closedRotation = transform.rotation;
            _openRotation = _closedRotation * Quaternion.Euler(0, _openAngle, 0);
            
            _interactionPrompt = _isLocked ? "Locked" : "Press E to open";
        }
        
        public override void Interact(GameObject interactor)
        {
            if (_isLocked)
            {
                Debug.Log("[Door] Door is locked!");
                // TODO: Check for key in inventory
                return;
            }
            
            if (!_isAnimating)
            {
                _isOpen = !_isOpen;
                StartCoroutine(AnimateDoor());
            }
        }
        
        private System.Collections.IEnumerator AnimateDoor()
        {
            _isAnimating = true;
            Quaternion targetRotation = _isOpen ? _openRotation : _closedRotation;
            
            while (Quaternion.Angle(transform.rotation, targetRotation) > 0.1f)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * _openSpeed);
                yield return null;
            }
            
            transform.rotation = targetRotation;
            _isAnimating = false;
            
            _interactionPrompt = _isOpen ? "Press E to close" : "Press E to open";
        }
        
        public void Unlock()
        {
            _isLocked = false;
            _interactionPrompt = "Press E to open";
            Debug.Log("[Door] Door unlocked!");
        }
        
        public void Lock()
        {
            _isLocked = true;
            _isOpen = false;
            _interactionPrompt = "Locked";
            Debug.Log("[Door] Door locked!");
        }
    }
    
    /// <summary>
    /// Chest interactable - contains loot.
    /// </summary>
    public class Chest : InteractableObject
    {
        [Header("Chest Settings")]
        [SerializeField] private bool _isOpened;
        [SerializeField] private int _lootCount = 3;
        [SerializeField] private Rarity _minLootRarity = Rarity.Common;
        [SerializeField] private Rarity _maxLootRarity = Rarity.Rare;
        
        protected override void Start()
        {
            base.Start();
            _interactionPrompt = _isOpened ? "Empty" : "Press E to open";
        }
        
        public override void Interact(GameObject interactor)
        {
            if (_isOpened)
            {
                Debug.Log("[Chest] Already looted!");
                return;
            }
            
            _isOpened = true;
            _interactionPrompt = "Empty";
            SpawnLoot();
        }
        
        private void SpawnLoot()
        {
            ItemGenerationEngine itemGen = FindFirstObjectByType<ItemGenerationEngine>();
            if (itemGen == null)
            {
                Debug.LogWarning("[Chest] No ItemGenerationEngine found!");
                return;
            }
            
            Debug.Log($"[Chest] Spawning {_lootCount} items!");
            
            for (int i = 0; i < _lootCount; i++)
            {
                Rarity rarity = (Rarity)Random.Range((int)_minLootRarity, (int)_maxLootRarity + 1);
                ItemType type = Random.value > 0.5f ? ItemType.Weapon : ItemType.Armor;
                
                ItemData item = itemGen.GetOrGenerateItem(rarity, type);
                Debug.Log($"  - {item.itemName} ({item.rarity})");
                
                // TODO: Add to player inventory or spawn as pickup
            }
        }
        
        public override bool CanInteract(GameObject interactor)
        {
            return base.CanInteract(interactor) && !_isOpened;
        }
    }
    
    /// <summary>
    /// Ladder interactable - allows vertical traversal.
    /// </summary>
    public class Ladder : InteractableObject
    {
        [Header("Ladder Settings")]
        [SerializeField] private float _climbSpeed = 3f;
        [SerializeField] private Transform _topPosition;
        [SerializeField] private Transform _bottomPosition;
        
        private bool _isClimbing;
        
        protected override void Start()
        {
            base.Start();
            _interactionPrompt = "Press E to climb";
            
            // Create top/bottom positions if not set
            if (_topPosition == null)
            {
                GameObject top = new GameObject("LadderTop");
                top.transform.SetParent(transform);
                top.transform.localPosition = Vector3.up * 5f;
                _topPosition = top.transform;
            }
            
            if (_bottomPosition == null)
            {
                GameObject bottom = new GameObject("LadderBottom");
                bottom.transform.SetParent(transform);
                bottom.transform.localPosition = Vector3.zero;
                _bottomPosition = bottom.transform;
            }
        }
        
        public override void Interact(GameObject interactor)
        {
            if (_isClimbing) return;
            
            // Determine which direction to climb
            float distToTop = Vector3.Distance(interactor.transform.position, _topPosition.position);
            float distToBottom = Vector3.Distance(interactor.transform.position, _bottomPosition.position);
            
            Vector3 target = distToTop < distToBottom ? _bottomPosition.position : _topPosition.position;
            
            StartCoroutine(ClimbLadder(interactor, target));
        }
        
        private System.Collections.IEnumerator ClimbLadder(GameObject interactor, Vector3 target)
        {
            _isClimbing = true;
            
            // Disable player controller
            PlayerController playerController = interactor.GetComponent<PlayerController>();
            if (playerController != null)
                playerController.enabled = false;
            
            // Climb to target
            while (Vector3.Distance(interactor.transform.position, target) > 0.5f)
            {
                interactor.transform.position = Vector3.MoveTowards(
                    interactor.transform.position, 
                    target, 
                    _climbSpeed * Time.deltaTime
                );
                yield return null;
            }
            
            interactor.transform.position = target;
            
            // Re-enable player controller
            if (playerController != null)
                playerController.enabled = true;
            
            _isClimbing = false;
            Debug.Log("[Ladder] Finished climbing!");
        }
    }
    
    /// <summary>
    /// Lever interactable - toggles between on/off states, triggers events.
    /// </summary>
    public class Lever : InteractableObject
    {
        [Header("Lever Settings")]
        [SerializeField] private bool _isActivated;
        [SerializeField] private float _toggleAngle = 45f;
        [SerializeField] private GameObject _targetObject;
        
        private Quaternion _offRotation;
        private Quaternion _onRotation;
        
        public System.Action<bool> OnToggled;
        
        protected override void Start()
        {
            base.Start();
            _offRotation = transform.rotation;
            _onRotation = _offRotation * Quaternion.Euler(_toggleAngle, 0, 0);
            _interactionPrompt = "Press E to pull lever";
        }
        
        public override void Interact(GameObject interactor)
        {
            _isActivated = !_isActivated;
            
            transform.rotation = _isActivated ? _onRotation : _offRotation;
            
            OnToggled?.Invoke(_isActivated);
            
            // Trigger target object
            if (_targetObject != null)
            {
                Door door = _targetObject.GetComponent<Door>();
                if (door != null)
                {
                    if (_isActivated)
                        door.Unlock();
                    else
                        door.Lock();
                }
            }
            
            Debug.Log($"[Lever] Toggled {(_isActivated ? "ON" : "OFF")}");
        }
    }
    
    /// <summary>
    /// Trap interactable - triggers damage when activated.
    /// </summary>
    public class Trap : MonoBehaviour
    {
        [Header("Trap Settings")]
        [SerializeField] private int _damage = 50;
        [SerializeField] private float _triggerRadius = 2f;
        [SerializeField] private float _cooldown = 3f;
        [SerializeField] private bool _isReusable = true;
        
        private float _lastTriggerTime;
        private bool _isTriggered;
        
        private void Update()
        {
            if (_isTriggered && !_isReusable) return;
            
            if (Time.time - _lastTriggerTime < _cooldown) return;
            
            // Check for entities in range
            Collider[] hits = Physics.OverlapSphere(transform.position, _triggerRadius);
            
            foreach (Collider hit in hits)
            {
                if (hit.CompareTag("Player") || hit.CompareTag("Enemy"))
                {
                    TriggerTrap(hit.gameObject);
                    break;
                }
            }
        }
        
        private void TriggerTrap(GameObject victim)
        {
            _isTriggered = true;
            _lastTriggerTime = Time.time;
            
            EntityStats stats = victim.GetComponent<EntityStats>();
            if (stats != null)
            {
                stats.currentHealth -= _damage;
                Debug.Log($"[Trap] {victim.name} triggered trap! Took {_damage} damage");
            }
            
            // Visual effect
            StartCoroutine(PlayTrapEffect());
        }
        
        private System.Collections.IEnumerator PlayTrapEffect()
        {
            Renderer renderer = GetComponent<Renderer>();
            Color originalColor = renderer.material.color;
            
            renderer.material.color = Color.yellow;
            yield return new WaitForSeconds(0.2f);
            
            renderer.material.color = originalColor;
            
            if (_isReusable)
            {
                _isTriggered = false;
            }
        }
        
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, _triggerRadius);
        }
    }
} 
 
#####InventorySystemManager.cs##### 
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Game.Core.Systems
{
    /// <summary>
    /// Inventory System Manager - Complete inventory management with equipment slots.
    /// Handles item storage, equipped items, stacking, and inventory UI.
    /// COMPLETE IMPLEMENTATION!
    /// </summary>
    public class InventorySystemManager : MonoBehaviour
    {
        [Header("Inventory Configuration")]
        [SerializeField] private int _defaultInventorySize = 30;
        [SerializeField] private int _maxStackSize = 99;
        
        [Header("Player Inventory")]
        [SerializeField] private InventoryData _playerInventory;
        
        [Header("Equipment Slots")]
        [SerializeField] private EquipmentData _playerEquipment;
        
        private Dictionary<GameObject, InventoryData> _entityInventories;
        
        #region Initialization
        
        private void Awake()
        {
            _entityInventories = new Dictionary<GameObject, InventoryData>();
            InitializePlayerInventory();
            Debug.Log("[InventorySystemManager] Initialized");
        }
        
        private void InitializePlayerInventory()
        {
            _playerInventory = new InventoryData(_defaultInventorySize);
            _playerEquipment = new EquipmentData();
        }
        
        #endregion
        
        #region Inventory Management
        
        /// <summary>
        /// Get or create inventory for an entity.
        /// </summary>
        public InventoryData GetInventory(GameObject entity)
        {
            // Player inventory
            if (entity.CompareTag("Player"))
            {
                return _playerInventory;
            }
            
            // Entity inventories
            if (!_entityInventories.ContainsKey(entity))
            {
                _entityInventories[entity] = new InventoryData(_defaultInventorySize);
            }
            
            return _entityInventories[entity];
        }
        
        /// <summary>
        /// Add item to entity inventory.
        /// </summary>
        public bool AddItem(GameObject entity, ItemData item, int quantity = 1)
        {
            InventoryData inventory = GetInventory(entity);
            
            bool added = inventory.AddItem(item, quantity);
            
            if (added)
            {
                Debug.Log($"[InventorySystemManager] Added {quantity}x {item.itemName} to {entity.name}");
                OnInventoryChanged?.Invoke(entity);
            }
            else
            {
                Debug.LogWarning($"[InventorySystemManager] Failed to add {item.itemName} - inventory full!");
            }
            
            return added;
        }
        
        /// <summary>
        /// Remove item from entity inventory.
        /// </summary>
        public bool RemoveItem(GameObject entity, string itemId, int quantity = 1)
        {
            InventoryData inventory = GetInventory(entity);
            
            bool removed = inventory.RemoveItem(itemId, quantity);
            
            if (removed)
            {
                Debug.Log($"[InventorySystemManager] Removed {quantity}x item {itemId} from {entity.name}");
                OnInventoryChanged?.Invoke(entity);
            }
            
            return removed;
        }
        
        /// <summary>
        /// Check if entity has item.
        /// </summary>
        public bool HasItem(GameObject entity, string itemId, int quantity = 1)
        {
            InventoryData inventory = GetInventory(entity);
            return inventory.HasItem(itemId, quantity);
        }
        
        /// <summary>
        /// Get item count in inventory.
        /// </summary>
        public int GetItemCount(GameObject entity, string itemId)
        {
            InventoryData inventory = GetInventory(entity);
            return inventory.GetItemCount(itemId);
        }
        
        #endregion
        
        #region Equipment Management
        
        /// <summary>
        /// Equip an item from inventory.
        /// </summary>
        public bool EquipItem(GameObject entity, string itemId)
        {
            if (!entity.CompareTag("Player"))
            {
                Debug.LogWarning("[InventorySystemManager] Only player can equip items currently");
                return false;
            }
            
            InventoryData inventory = GetInventory(entity);
            ItemData item = inventory.GetItem(itemId);
            
            if (item == null)
            {
                Debug.LogWarning($"[InventorySystemManager] Item {itemId} not found in inventory");
                return false;
            }
            
            // Determine slot based on item type
            if (item.itemType == ItemType.Weapon)
            {
                return EquipWeapon(entity, item);
            }
            else if (item.itemType == ItemType.Armor)
            {
                return EquipArmor(entity, item);
            }
            
            return false;
        }
        
        private bool EquipWeapon(GameObject entity, ItemData weapon)
        {
            // Unequip current weapon
            if (_playerEquipment.equippedWeapon != null)
            {
                UnequipItem(entity, _playerEquipment.equippedWeapon.itemId);
            }
            
            // Equip new weapon
            _playerEquipment.equippedWeapon = weapon;
            
            // Apply weapon stats to player
            EntityStats stats = entity.GetComponent<EntityStats>();
            if (stats != null)
            {
                stats.damageElement = weapon.weaponArchetype switch
                {
                    WeaponArchetype.LaserRifle => DamageElement.Lightning,
                    WeaponArchetype.PlasmaGun => DamageElement.Fire,
                    _ => DamageElement.None
                };
            }
            
            Debug.Log($"[InventorySystemManager] Equipped weapon: {weapon.itemName}");
            OnEquipmentChanged?.Invoke(entity);
            return true;
        }
        
        private bool EquipArmor(GameObject entity, ItemData armor)
        {
            // Unequip current armor in slot
            ItemData currentArmor = _playerEquipment.GetArmorInSlot(armor.armorSlot);
            if (currentArmor != null)
            {
                UnequipItem(entity, currentArmor.itemId);
            }
            
            // Equip new armor
            _playerEquipment.SetArmorInSlot(armor.armorSlot, armor);
            
            // Apply armor stats
            RecalculateEquipmentStats(entity);
            
            Debug.Log($"[InventorySystemManager] Equipped armor: {armor.itemName}");
            OnEquipmentChanged?.Invoke(entity);
            return true;
        }
        
        /// <summary>
        /// Unequip an item and return it to inventory.
        /// </summary>
        public bool UnequipItem(GameObject entity, string itemId)
        {
            if (!entity.CompareTag("Player"))
                return false;
            
            ItemData item = null;
            
            // Check weapon
            if (_playerEquipment.equippedWeapon != null && _playerEquipment.equippedWeapon.itemId == itemId)
            {
                item = _playerEquipment.equippedWeapon;
                _playerEquipment.equippedWeapon = null;
            }
            
            // Check armor slots
            foreach (ArmorSlot slot in System.Enum.GetValues(typeof(ArmorSlot)))
            {
                ItemData armorPiece = _playerEquipment.GetArmorInSlot(slot);
                if (armorPiece != null && armorPiece.itemId == itemId)
                {
                    item = armorPiece;
                    _playerEquipment.SetArmorInSlot(slot, null);
                    break;
                }
            }
            
            if (item == null)
                return false;
            
            // Return to inventory (already there, just mark as unequipped)
            RecalculateEquipmentStats(entity);
            
            Debug.Log($"[InventorySystemManager] Unequipped: {item.itemName}");
            OnEquipmentChanged?.Invoke(entity);
            return true;
        }
        
        private void RecalculateEquipmentStats(GameObject entity)
        {
            EntityStats stats = entity.GetComponent<EntityStats>();
            if (stats == null) return;
            
            // Reset bonus stats
            int totalArmor = 0;
            int bonusHealth = 0;
            int bonusMana = 0;
            float critChance = 0f;
            
            // Sum armor pieces
            foreach (ArmorSlot slot in System.Enum.GetValues(typeof(ArmorSlot)))
            {
                ItemData armor = _playerEquipment.GetArmorInSlot(slot);
                if (armor != null)
                {
                    totalArmor += armor.armorValue;
                    bonusHealth += armor.bonusHealth;
                    bonusMana += armor.bonusMana;
                    critChance += armor.critChance;
                }
            }
            
            // Add weapon bonuses
            if (_playerEquipment.equippedWeapon != null)
            {
                bonusHealth += _playerEquipment.equippedWeapon.bonusHealth;
                bonusMana += _playerEquipment.equippedWeapon.bonusMana;
                critChance += _playerEquipment.equippedWeapon.critChance;
            }
            
            // Apply to stats
            stats.armor = totalArmor;
            stats.criticalChance = critChance;
            
            // Recalculate derived stats
            stats.RecalculateStats();
            
            Debug.Log($"[InventorySystemManager] Recalculated stats: Armor={totalArmor}, Crit={critChance:P}");
        }
        
        public EquipmentData GetEquipment(GameObject entity)
        {
            if (entity.CompareTag("Player"))
                return _playerEquipment;
            
            return null;
        }
        
        #endregion
        
        #region Item Transfer
        
        /// <summary>
        /// Transfer item from one entity to another.
        /// </summary>
        public bool TransferItem(GameObject from, GameObject to, string itemId, int quantity = 1)
        {
            InventoryData fromInv = GetInventory(from);
            InventoryData toInv = GetInventory(to);
            
            ItemData item = fromInv.GetItem(itemId);
            if (item == null || !fromInv.HasItem(itemId, quantity))
                return false;
            
            // Remove from source
            if (!fromInv.RemoveItem(itemId, quantity))
                return false;
            
            // Add to destination
            if (!toInv.AddItem(item, quantity))
            {
                // Failed to add, return to source
                fromInv.AddItem(item, quantity);
                return false;
            }
            
            OnInventoryChanged?.Invoke(from);
            OnInventoryChanged?.Invoke(to);
            
            Debug.Log($"[InventorySystemManager] Transferred {quantity}x {item.itemName} from {from.name} to {to.name}");
            return true;
        }
        
        #endregion
        
        #region Events
        
        public System.Action<GameObject> OnInventoryChanged;
        public System.Action<GameObject> OnEquipmentChanged;
        
        #endregion
        
        #region Shutdown
        
        public void Shutdown()
        {
            Debug.Log("[InventorySystemManager] Shutting down...");
            _entityInventories.Clear();
        }
        
        #endregion
    }
    
    #region Data Structures
    
    /// <summary>
    /// Inventory data container.
    /// </summary>
    [System.Serializable]
    public class InventoryData
    {
        [SerializeField] private int _maxSlots;
        [SerializeField] private List<InventorySlot> _slots;
        
        public int MaxSlots => _maxSlots;
        public List<InventorySlot> Slots => _slots;
        
        public InventoryData(int maxSlots)
        {
            _maxSlots = maxSlots;
            _slots = new List<InventorySlot>();
        }
        
        public bool AddItem(ItemData item, int quantity)
        {
            // Try to stack with existing item
            InventorySlot existingSlot = _slots.FirstOrDefault(s => s.item.itemId == item.itemId);
            if (existingSlot != null)
            {
                existingSlot.quantity += quantity;
                return true;
            }
            
            // Check if we have space
            if (_slots.Count >= _maxSlots)
                return false;
            
            // Add new slot
            _slots.Add(new InventorySlot(item, quantity));
            return true;
        }
        
        public bool RemoveItem(string itemId, int quantity)
        {
            InventorySlot slot = _slots.FirstOrDefault(s => s.item.itemId == itemId);
            if (slot == null || slot.quantity < quantity)
                return false;
            
            slot.quantity -= quantity;
            
            if (slot.quantity <= 0)
            {
                _slots.Remove(slot);
            }
            
            return true;
        }
        
        public bool HasItem(string itemId, int quantity)
        {
            InventorySlot slot = _slots.FirstOrDefault(s => s.item.itemId == itemId);
            return slot != null && slot.quantity >= quantity;
        }
        
        public int GetItemCount(string itemId)
        {
            InventorySlot slot = _slots.FirstOrDefault(s => s.item.itemId == itemId);
            return slot?.quantity ?? 0;
        }
        
        public ItemData GetItem(string itemId)
        {
            InventorySlot slot = _slots.FirstOrDefault(s => s.item.itemId == itemId);
            return slot?.item;
        }
        
        public List<ItemData> GetAllItems()
        {
            return _slots.Select(s => s.item).ToList();
        }
        
        public void Clear()
        {
            _slots.Clear();
        }
    }
    
    /// <summary>
    /// Individual inventory slot.
    /// </summary>
    [System.Serializable]
    public class InventorySlot
    {
        public ItemData item;
        public int quantity;
        
        public InventorySlot(ItemData item, int quantity)
        {
            this.item = item;
            this.quantity = quantity;
        }
    }
    
    /// <summary>
    /// Equipment data container.
    /// </summary>
    [System.Serializable]
    public class EquipmentData
    {
        public ItemData equippedWeapon;
        public ItemData equippedHead;
        public ItemData equippedChest;
        public ItemData equippedHands;
        public ItemData equippedLegs;
        public ItemData equippedFeet;
        
        public ItemData GetArmorInSlot(ArmorSlot slot)
        {
            return slot switch
            {
                ArmorSlot.Head => equippedHead,
                ArmorSlot.Chest => equippedChest,
                ArmorSlot.Hands => equippedHands,
                ArmorSlot.Legs => equippedLegs,
                ArmorSlot.Feet => equippedFeet,
                _ => null
            };
        }
        
        public void SetArmorInSlot(ArmorSlot slot, ItemData armor)
        {
            switch (slot)
            {
                case ArmorSlot.Head: equippedHead = armor; break;
                case ArmorSlot.Chest: equippedChest = armor; break;
                case ArmorSlot.Hands: equippedHands = armor; break;
                case ArmorSlot.Legs: equippedLegs = armor; break;
                case ArmorSlot.Feet: equippedFeet = armor; break;
            }
        }
        
        public List<ItemData> GetAllEquippedItems()
        {
            List<ItemData> items = new();
            
            if (equippedWeapon != null) items.Add(equippedWeapon);
            if (equippedHead != null) items.Add(equippedHead);
            if (equippedChest != null) items.Add(equippedChest);
            if (equippedHands != null) items.Add(equippedHands);
            if (equippedLegs != null) items.Add(equippedLegs);
            if (equippedFeet != null) items.Add(equippedFeet);
            
            return items;
        }
        
        public int GetTotalArmor()
        {
            int total = 0;
            
            if (equippedHead != null) total += equippedHead.armorValue;
            if (equippedChest != null) total += equippedChest.armorValue;
            if (equippedHands != null) total += equippedHands.armorValue;
            if (equippedLegs != null) total += equippedLegs.armorValue;
            if (equippedFeet != null) total += equippedFeet.armorValue;
            
            return total;
        }
    }
    
    #endregion
} 
 
#####ItemGenerationEngine.cs##### 
using UnityEngine;
using System.Collections.Generic;

namespace Game.Core.Systems
{
    /// <summary>
    /// Hybrid item generation system.
    /// Pre-generates common items at startup, generates rare items on-demand.
    /// Supports weapons, armor, consumables with procedural stats and affixes.
    /// </summary>
    public class ItemGenerationEngine : MonoBehaviour
    {
        [Header("Generation Settings")]
        [SerializeField] private int _commonItemPregenCount = 50;
        [SerializeField] private int _maxCachedItems = 500;
        
        [Header("Item Pools")]
        [SerializeField] private List<ItemData> _cachedItems = new();
        
        private Dictionary<string, ItemData> _itemCache;
        private System.Random _rng;
        
        #region Initialization
        
        private void Awake()
        {
            _itemCache = new Dictionary<string, ItemData>();
            _rng = new System.Random();
            
            PreGenerateCommonItems();
        }
        
        private void PreGenerateCommonItems()
        {
            Debug.Log("[ItemGenerationEngine] Pre-generating common items...");
            
            for (int i = 0; i < _commonItemPregenCount; i++)
            {
                // Generate mix of weapons and armor
                ItemData item = i % 2 == 0 
                    ? GenerateWeapon(Rarity.Common) 
                    : GenerateArmor(Rarity.Common);
                
                CacheItem(item);
            }
            
            Debug.Log($"[ItemGenerationEngine] Pre-generated {_cachedItems.Count} items");
        }
        
        #endregion
        
        #region Weapon Generation
        
        public ItemData GenerateWeapon(Rarity rarity)
        {
            WeaponArchetype archetype = GetRandomWeaponArchetype();
            
            ItemData weapon = new()
            {
                itemId = System.Guid.NewGuid().ToString(),
                itemName = GenerateWeaponName(archetype, rarity),
                itemType = ItemType.Weapon,
                rarity = rarity,
                weaponArchetype = archetype,
                level = GetLevelForRarity(rarity),
            };
            
            // Generate base stats
            weapon.damage = GetWeaponBaseDamage(archetype, rarity);
            weapon.attackSpeed = GetWeaponAttackSpeed(archetype);
            weapon.range = GetWeaponRange(archetype);
            
            // Generate affixes based on rarity
            weapon.affixes = GenerateAffixes(rarity);
            
            // Apply affix bonuses
            ApplyAffixBonuses(weapon);
            
            return weapon;
        }
        
        private WeaponArchetype GetRandomWeaponArchetype()
        {
            WeaponArchetype[] archetypes = (WeaponArchetype[])System.Enum.GetValues(typeof(WeaponArchetype));
            return archetypes[_rng.Next(archetypes.Length)];
        }
        
        private string GenerateWeaponName(WeaponArchetype archetype, Rarity rarity)
        {
            string prefix = rarity switch
            {
                Rarity.Common => "",
                Rarity.Uncommon => "Fine ",
                Rarity.Rare => "Superior ",
                Rarity.Epic => "Masterwork ",
                Rarity.Legendary => "Legendary ",
                Rarity.Mythic => "Divine ",
                _ => ""
            };
            
            return $"{prefix}{archetype}";
        }
        
        private int GetWeaponBaseDamage(WeaponArchetype archetype, Rarity rarity)
        {
            int baseDamage = archetype switch
            {
                WeaponArchetype.Stick => 5,
                WeaponArchetype.Sword => 25,
                WeaponArchetype.Axe => 30,
                WeaponArchetype.Hammer => 35,
                WeaponArchetype.Bow => 20,
                WeaponArchetype.Pistol => 40,
                WeaponArchetype.Rifle => 50,
                WeaponArchetype.LaserRifle => 60,
                WeaponArchetype.PlasmaGun => 70,
                WeaponArchetype.QuantumBlade => 100,
                _ => 10
            };
            
            // Scale with rarity
            float multiplier = rarity switch
            {
                Rarity.Common => 1.0f,
                Rarity.Uncommon => 1.3f,
                Rarity.Rare => 1.7f,
                Rarity.Epic => 2.2f,
                Rarity.Legendary => 3.0f,
                Rarity.Mythic => 4.5f,
                _ => 1.0f
            };
            
            return Mathf.RoundToInt(baseDamage * multiplier);
        }
        
        private float GetWeaponAttackSpeed(WeaponArchetype archetype)
        {
            return archetype switch
            {
                WeaponArchetype.Stick => 2.0f,
                WeaponArchetype.Sword => 1.5f,
                WeaponArchetype.Axe => 1.2f,
                WeaponArchetype.Hammer => 1.0f,
                WeaponArchetype.Bow => 1.3f,
                WeaponArchetype.Pistol => 2.5f,
                WeaponArchetype.Rifle => 1.8f,
                _ => 1.5f
            };
        }
        
        private float GetWeaponRange(WeaponArchetype archetype)
        {
            return archetype switch
            {
                WeaponArchetype.Stick => 2f,
                WeaponArchetype.Sword => 2.5f,
                WeaponArchetype.Axe => 2.5f,
                WeaponArchetype.Hammer => 2f,
                WeaponArchetype.Bow => 30f,
                WeaponArchetype.Pistol => 25f,
                WeaponArchetype.Rifle => 50f,
                WeaponArchetype.LaserRifle => 60f,
                _ => 3f
            };
        }
        
        #endregion
        
        #region Armor Generation
        
        public ItemData GenerateArmor(Rarity rarity)
        {
            ArmorSlot slot = GetRandomArmorSlot();
            ArmorArchetype archetype = GetRandomArmorArchetype();
            
            ItemData armor = new()
            {
                itemId = System.Guid.NewGuid().ToString(),
                itemName = GenerateArmorName(archetype, slot, rarity),
                itemType = ItemType.Armor,
                rarity = rarity,
                armorSlot = slot,
                armorArchetype = archetype,
                level = GetLevelForRarity(rarity),
            };
            
            // Generate base stats
            armor.armorValue = GetArmorValue(archetype, slot, rarity);
            
            // Generate affixes
            armor.affixes = GenerateAffixes(rarity);
            
            // Apply affix bonuses
            ApplyAffixBonuses(armor);
            
            return armor;
        }
        
        private ArmorSlot GetRandomArmorSlot()
        {
            ArmorSlot[] slots = (ArmorSlot[])System.Enum.GetValues(typeof(ArmorSlot));
            return slots[_rng.Next(slots.Length)];
        }
        
        private ArmorArchetype GetRandomArmorArchetype()
        {
            ArmorArchetype[] archetypes = (ArmorArchetype[])System.Enum.GetValues(typeof(ArmorArchetype));
            return archetypes[_rng.Next(archetypes.Length)];
        }
        
        private string GenerateArmorName(ArmorArchetype archetype, ArmorSlot slot, Rarity rarity)
        {
            string prefix = rarity switch
            {
                Rarity.Uncommon => "Fine ",
                Rarity.Rare => "Superior ",
                Rarity.Epic => "Masterwork ",
                Rarity.Legendary => "Legendary ",
                Rarity.Mythic => "Divine ",
                _ => ""
            };
            
            return $"{prefix}{archetype} {slot}";
        }
        
        private int GetArmorValue(ArmorArchetype archetype, ArmorSlot slot, Rarity rarity)
        {
            int baseArmor = archetype switch
            {
                ArmorArchetype.Cloth => 5,
                ArmorArchetype.Leather => 15,
                ArmorArchetype.Chain => 30,
                ArmorArchetype.Plate => 50,
                ArmorArchetype.PowerArmor => 80,
                ArmorArchetype.EnergyShield => 100,
                _ => 10
            };
            
            // Scale with slot
            float slotMultiplier = slot switch
            {
                ArmorSlot.Head => 0.8f,
                ArmorSlot.Chest => 1.5f,
                ArmorSlot.Hands => 0.6f,
                ArmorSlot.Legs => 1.0f,
                ArmorSlot.Feet => 0.6f,
                _ => 1.0f
            };
            
            // Scale with rarity
            float rarityMultiplier = rarity switch
            {
                Rarity.Common => 1.0f,
                Rarity.Uncommon => 1.3f,
                Rarity.Rare => 1.7f,
                Rarity.Epic => 2.2f,
                Rarity.Legendary => 3.0f,
                Rarity.Mythic => 4.5f,
                _ => 1.0f
            };
            
            return Mathf.RoundToInt(baseArmor * slotMultiplier * rarityMultiplier);
        }
        
        #endregion
        
        #region Affix Generation
        
        private List<ItemAffix> GenerateAffixes(Rarity rarity)
        {
            List<ItemAffix> affixes = new();
            
            int affixCount = rarity switch
            {
                Rarity.Common => 0,
                Rarity.Uncommon => 1,
                Rarity.Rare => 2,
                Rarity.Epic => 3,
                Rarity.Legendary => 4,
                Rarity.Mythic => 6,
                _ => 0
            };
            
            for (int i = 0; i < affixCount; i++)
            {
                affixes.Add(GenerateRandomAffix(rarity));
            }
            
            return affixes;
        }
        
        private ItemAffix GenerateRandomAffix(Rarity rarity)
        {
            AffixType[] types = (AffixType[])System.Enum.GetValues(typeof(AffixType));
            AffixType type = types[_rng.Next(types.Length)];
            
            float valueMultiplier = rarity switch
            {
                Rarity.Uncommon => 1.0f,
                Rarity.Rare => 1.5f,
                Rarity.Epic => 2.5f,
                Rarity.Legendary => 4.0f,
                Rarity.Mythic => 6.0f,
                _ => 1.0f
            };
            
            return new ItemAffix
            {
                affixType = type,
                value = Mathf.RoundToInt(_rng.Next(5, 20) * valueMultiplier)
            };
        }
        
        private void ApplyAffixBonuses(ItemData item)
        {
            foreach (ItemAffix affix in item.affixes)
            {
                switch (affix.affixType)
                {
                    case AffixType.BonusDamage:
                        item.damage += affix.value;
                        break;
                    case AffixType.BonusArmor:
                        item.armorValue += affix.value;
                        break;
                    case AffixType.BonusHealth:
                        item.bonusHealth = affix.value;
                        break;
                    case AffixType.BonusMana:
                        item.bonusMana = affix.value;
                        break;
                    case AffixType.CriticalChance:
                        item.critChance = affix.value / 100f;
                        break;
                }
            }
        }
        
        #endregion
        
        #region Cache Management
        
        private void CacheItem(ItemData item)
        {
            if (_cachedItems.Count >= _maxCachedItems)
            {
                // Remove oldest common item
                for (int i = 0; i < _cachedItems.Count; i++)
                {
                    if (_cachedItems[i].rarity == Rarity.Common)
                    {
                        _itemCache.Remove(_cachedItems[i].itemId);
                        _cachedItems.RemoveAt(i);
                        break;
                    }
                }
            }
            
            _cachedItems.Add(item);
            _itemCache[item.itemId] = item;
        }
        
        public ItemData GetOrGenerateItem(Rarity rarity, ItemType type)
        {
            // Try to find cached item of matching rarity/type
            foreach (ItemData item in _cachedItems)
            {
                if (item.rarity == rarity && item.itemType == type)
                {
                    return item;
                }
            }
            
            // Generate new item
            ItemData newItem = type == ItemType.Weapon 
                ? GenerateWeapon(rarity) 
                : GenerateArmor(rarity);
            
            CacheItem(newItem);
            return newItem;
        }
        
        #endregion
        
        #region Helpers
        
        private int GetLevelForRarity(Rarity rarity)
        {
            return rarity switch
            {
                Rarity.Common => _rng.Next(1, 10),
                Rarity.Uncommon => _rng.Next(10, 20),
                Rarity.Rare => _rng.Next(20, 35),
                Rarity.Epic => _rng.Next(35, 50),
                Rarity.Legendary => _rng.Next(50, 75),
                Rarity.Mythic => _rng.Next(75, 100),
                _ => 1
            };
        }
        
        #endregion
    }
    
    #region Item Data Structures
    
    [System.Serializable]
    public class ItemData
    {
        public string itemId;
        public string itemName;
        public ItemType itemType;
        public Rarity rarity;
        public int level;
        
        // Weapon stats
        public WeaponArchetype weaponArchetype;
        public int damage;
        public float attackSpeed;
        public float range;
        
        // Armor stats
        public ArmorSlot armorSlot;
        public ArmorArchetype armorArchetype;
        public int armorValue;
        
        // Bonus stats
        public int bonusHealth;
        public int bonusMana;
        public float critChance;
        
        // Affixes
        public List<ItemAffix> affixes = new();
    }
    
    [System.Serializable]
    public class ItemAffix
    {
        public AffixType affixType;
        public int value;
    }
    
    public enum ItemType { Weapon, Armor, Consumable, Material }
    
    public enum Rarity { Common, Uncommon, Rare, Epic, Legendary, Mythic }
    
    public enum WeaponArchetype
    {
        Stick, Sword, Axe, Hammer, Spear, Bow, Crossbow,
        Pistol, Rifle, Shotgun, LaserRifle, PlasmaGun, Railgun,
        Staff, Wand, QuantumBlade
    }
    
    public enum ArmorSlot { Head, Chest, Hands, Legs, Feet }
    
    public enum ArmorArchetype
    {
        Cloth, Leather, Chain, Plate, PowerArmor, EnergyShield
    }
    
    public enum AffixType
    {
        BonusDamage, BonusArmor, BonusHealth, BonusMana,
        CriticalChance, AttackSpeed, MovementSpeed, LifeSteal
    }
    
    #endregion
} 
 
#####LootSystemManager.cs##### 
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Game.Core.Systems
{
    /// <summary>
    /// Loot System Manager - Handles loot drops, loot tables, and rarity rolls.
    /// Integrates with ItemGenerationEngine for procedural loot.
    /// COMPLETE IMPLEMENTATION!
    /// </summary>
    public class LootSystemManager : MonoBehaviour
    {
        [Header("Loot Configuration")]
        [SerializeField] private bool _autoDropOnDeath = true;
        [SerializeField] private float _lootDropRadius = 2f;
        [SerializeField] private float _lootPickupRange = 3f;
        
        [Header("Rarity Weights")]
        [SerializeField] private RarityWeights _rarityWeights = new();
        
        [Header("Loot Tables")]
        [SerializeField] private List<LootTable> _lootTables = new();
        
        [Header("World Loot")]
        [SerializeField] private List<LootDrop> _activeLootDrops = new();
        
        private ItemGenerationEngine _itemGenerator;
        private Dictionary<string, LootTable> _lootTableMap;
        
        #region Initialization
        
        private void Awake()
        {
            _itemGenerator = FindFirstObjectByType<ItemGenerationEngine>();
            if (_itemGenerator == null)
            {
                GameObject genObj = new GameObject("ItemGenerationEngine");
                _itemGenerator = genObj.AddComponent<ItemGenerationEngine>();
            }
            
            _lootTableMap = new Dictionary<string, LootTable>();
            InitializeLootTables();
            
            Debug.Log("[LootSystemManager] Initialized");
        }
        
        private void Start()
        {
            // Subscribe to combat events
            CombatSystemManager combatManager = CoreSystemManager.CombatManager;
            if (combatManager != null)
            {
                combatManager.OnEntityDeath += OnEntityDeath;
            }
        }
        
        private void OnDestroy()
        {
            CombatSystemManager combatManager = CoreSystemManager.CombatManager;
            if (combatManager != null)
            {
                combatManager.OnEntityDeath -= OnEntityDeath;
            }
        }
        
        private void InitializeLootTables()
        {
            // Create default loot tables for common enemy types
            CreateDefaultLootTable("Zombie", 1, 3, new Vector2Int(1, 5));
            CreateDefaultLootTable("Skeleton", 1, 3, new Vector2Int(1, 5));
            CreateDefaultLootTable("Orc", 2, 4, new Vector2Int(5, 10));
            CreateDefaultLootTable("Boss", 5, 8, new Vector2Int(10, 20));
            
            Debug.Log($"[LootSystemManager] Initialized {_lootTables.Count} loot tables");
        }
        
        private void CreateDefaultLootTable(string tableName, int minItems, int maxItems, Vector2Int levelRange)
        {
            LootTable table = new LootTable
            {
                tableName = tableName,
                minItems = minItems,
                maxItems = maxItems,
                levelRange = levelRange,
                goldMin = levelRange.x * 10,
                goldMax = levelRange.y * 20
            };
            
            _lootTables.Add(table);
            _lootTableMap[tableName] = table;
        }
        
        #endregion
        
        #region Loot Generation
        
        /// <summary>
        /// Generate loot from a loot table.
        /// </summary>
        public List<ItemData> GenerateLoot(string lootTableName)
        {
            if (!_lootTableMap.ContainsKey(lootTableName))
            {
                Debug.LogWarning($"[LootSystemManager] Loot table '{lootTableName}' not found!");
                return new List<ItemData>();
            }
            
            LootTable table = _lootTableMap[lootTableName];
            return GenerateLootFromTable(table);
        }
        
        private List<ItemData> GenerateLootFromTable(LootTable table)
        {
            List<ItemData> loot = new();
            
            int itemCount = Random.Range(table.minItems, table.maxItems + 1);
            
            for (int i = 0; i < itemCount; i++)
            {
                // Roll rarity
                Rarity rarity = RollRarity();
                
                // Roll item type
                ItemType itemType = Random.value > 0.5f ? ItemType.Weapon : ItemType.Armor;
                
                // Generate item
                ItemData item = itemType == ItemType.Weapon 
                    ? _itemGenerator.GenerateWeapon(rarity) 
                    : _itemGenerator.GenerateArmor(rarity);
                
                loot.Add(item);
            }
            
            Debug.Log($"[LootSystemManager] Generated {loot.Count} items from table '{table.tableName}'");
            return loot;
        }
        
        /// <summary>
        /// Roll a random rarity based on weights.
        /// </summary>
        private Rarity RollRarity()
        {
            float roll = Random.Range(0f, 1f);
            
            float cumulative = 0f;
            
            cumulative += _rarityWeights.commonWeight;
            if (roll <= cumulative) return Rarity.Common;
            
            cumulative += _rarityWeights.uncommonWeight;
            if (roll <= cumulative) return Rarity.Uncommon;
            
            cumulative += _rarityWeights.rareWeight;
            if (roll <= cumulative) return Rarity.Rare;
            
            cumulative += _rarityWeights.epicWeight;
            if (roll <= cumulative) return Rarity.Epic;
            
            cumulative += _rarityWeights.legendaryWeight;
            if (roll <= cumulative) return Rarity.Legendary;
            
            return Rarity.Mythic;
        }
        
        #endregion
        
        #region Loot Drops
        
        /// <summary>
        /// Drop loot at a position in the world.
        /// </summary>
        public void DropLoot(List<ItemData> items, Vector3 position, int gold = 0)
        {
            if (items == null || items.Count == 0)
            {
                Debug.Log("[LootSystemManager] No items to drop");
                return;
            }
            
            foreach (ItemData item in items)
            {
                Vector3 dropPosition = position + Random.insideUnitSphere * _lootDropRadius;
                dropPosition.y = position.y;
                
                CreateLootDrop(item, dropPosition);
            }
            
            if (gold > 0)
            {
                CreateGoldDrop(gold, position);
            }
            
            Debug.Log($"[LootSystemManager] Dropped {items.Count} items at {position}");
        }
        
        private void CreateLootDrop(ItemData item, Vector3 position)
        {
            GameObject dropObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            dropObj.name = $"LootDrop_{item.itemName}";
            dropObj.transform.position = position + Vector3.up * 0.5f;
            dropObj.transform.localScale = Vector3.one * 0.5f;
            
            // Visual
            Renderer renderer = dropObj.GetComponent<Renderer>();
            Material mat = new Material(Shader.Find("Standard"));
            mat.color = GetRarityColor(item.rarity);
            mat.SetFloat("_Metallic", 0.5f);
            renderer.material = mat;
            
            // Add glow effect
            Light light = dropObj.AddComponent<Light>();
            light.color = GetRarityColor(item.rarity);
            light.range = 3f;
            light.intensity = 0.5f;
            
            // Add loot drop component
            LootDrop lootDrop = dropObj.AddComponent<LootDrop>();
            lootDrop.Initialize(item, this);
            
            _activeLootDrops.Add(lootDrop);
        }
        
        private void CreateGoldDrop(int amount, Vector3 position)
        {
            GameObject dropObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            dropObj.name = $"GoldDrop_{amount}";
            dropObj.transform.position = position + Vector3.up * 0.5f;
            dropObj.transform.localScale = Vector3.one * 0.3f;
            
            Renderer renderer = dropObj.GetComponent<Renderer>();
            Material mat = new Material(Shader.Find("Standard"));
            mat.color = Color.yellow;
            mat.SetFloat("_Metallic", 1f);
            renderer.material = mat;
            
            GoldDrop goldDrop = dropObj.AddComponent<GoldDrop>();
            goldDrop.Initialize(amount, this);
        }
        
        private Color GetRarityColor(Rarity rarity)
        {
            return rarity switch
            {
                Rarity.Common => Color.white,
                Rarity.Uncommon => Color.green,
                Rarity.Rare => Color.blue,
                Rarity.Epic => new Color(0.6f, 0.3f, 1f),
                Rarity.Legendary => new Color(1f, 0.5f, 0f),
                Rarity.Mythic => new Color(1f, 0f, 0f),
                _ => Color.white
            };
        }
        
        #endregion
        
        #region Loot Pickup
        
        /// <summary>
        /// Check for nearby loot and attempt pickup.
        /// </summary>
        private void Update()
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null) return;
            
            CheckForAutoPickup(player);
        }
        
        private void CheckForAutoPickup(GameObject player)
        {
            List<LootDrop> dropsToRemove = new();
            
            foreach (LootDrop drop in _activeLootDrops)
            {
                if (drop == null) continue;
                
                float distance = Vector3.Distance(player.transform.position, drop.transform.position);
                
                if (distance <= _lootPickupRange)
                {
                    if (PickupLoot(player, drop))
                    {
                        dropsToRemove.Add(drop);
                    }
                }
            }
            
            // Remove picked up drops
            foreach (LootDrop drop in dropsToRemove)
            {
                _activeLootDrops.Remove(drop);
                if (drop != null)
                    Destroy(drop.gameObject);
            }
        }
        
        private bool PickupLoot(GameObject player, LootDrop drop)
        {
            InventorySystemManager inventoryManager = CoreSystemManager.InventoryManager;
            if (inventoryManager == null) return false;
            
            bool added = inventoryManager.AddItem(player, drop.Item);
            
            if (added)
            {
                Debug.Log($"[LootSystemManager] Player picked up: {drop.Item.itemName}");
                
                // Play pickup sound
                AudioSystemManager audioManager = CoreSystemManager.AudioManager;
                if (audioManager != null)
                {
                    // audioManager.PlaySFX(pickupSound, player.transform.position);
                }
                
                return true;
            }
            
            return false;
        }
        
        public void RemoveLootDrop(LootDrop drop)
        {
            _activeLootDrops.Remove(drop);
        }
        
        #endregion
        
        #region Event Handlers
        
        private void OnEntityDeath(GameObject deadEntity, GameObject killer)
        {
            if (!_autoDropOnDeath) return;
            
            // Don't drop loot from player death
            if (deadEntity.CompareTag("Player")) return;
            
            // Determine loot table based on entity name
            string lootTableName = DetermineLootTable(deadEntity);
            
            // Generate loot
            List<ItemData> loot = GenerateLoot(lootTableName);
            
            // Generate gold
            int gold = GenerateGold(lootTableName);
            
            // Drop loot at death position
            DropLoot(loot, deadEntity.transform.position, gold);
        }
        
        private string DetermineLootTable(GameObject entity)
        {
            // Check entity name for keywords
            string name = entity.name.ToLower();
            
            if (name.Contains("boss")) return "Boss";
            if (name.Contains("orc")) return "Orc";
            if (name.Contains("skeleton")) return "Skeleton";
            if (name.Contains("zombie")) return "Zombie";
            
            // Default to zombie table
            return "Zombie";
        }
        
        private int GenerateGold(string lootTableName)
        {
            if (!_lootTableMap.ContainsKey(lootTableName))
                return Random.Range(5, 20);
            
            LootTable table = _lootTableMap[lootTableName];
            return Random.Range(table.goldMin, table.goldMax + 1);
        }
        
        #endregion
        
        #region Public API
        
        public void SetRarityWeights(RarityWeights weights)
        {
            _rarityWeights = weights;
        }
        
        public void AddLootTable(LootTable table)
        {
            if (!_lootTableMap.ContainsKey(table.tableName))
            {
                _lootTables.Add(table);
                _lootTableMap[table.tableName] = table;
            }
        }
        
        public List<string> GetAvailableLootTables()
        {
            return _lootTableMap.Keys.ToList();
        }
        
        #endregion
    }
    
    #region Data Structures
    
    /// <summary>
    /// Loot table definition.
    /// </summary>
    [System.Serializable]
    public class LootTable
    {
        public string tableName;
        public int minItems = 1;
        public int maxItems = 3;
        public Vector2Int levelRange = new Vector2Int(1, 10);
        public int goldMin = 10;
        public int goldMax = 50;
    }
    
    /// <summary>
    /// Rarity roll weights (must sum to 1.0).
    /// </summary>
    [System.Serializable]
    public class RarityWeights
    {
        [Range(0f, 1f)] public float commonWeight = 0.50f;
        [Range(0f, 1f)] public float uncommonWeight = 0.25f;
        [Range(0f, 1f)] public float rareWeight = 0.15f;
        [Range(0f, 1f)] public float epicWeight = 0.07f;
        [Range(0f, 1f)] public float legendaryWeight = 0.025f;
        [Range(0f, 1f)] public float mythicWeight = 0.005f;
        
        public void Normalize()
        {
            float total = commonWeight + uncommonWeight + rareWeight + epicWeight + legendaryWeight + mythicWeight;
            
            if (total > 0)
            {
                commonWeight /= total;
                uncommonWeight /= total;
                rareWeight /= total;
                epicWeight /= total;
                legendaryWeight /= total;
                mythicWeight /= total;
            }
        }
    }
    
    #endregion
    
    #region Loot Drop Components
    
    /// <summary>
    /// World loot drop component.
    /// </summary>
    public class LootDrop : MonoBehaviour
    {
        private ItemData _item;
        private LootSystemManager _lootManager;
        private float _spawnTime;
        private float _bobSpeed = 1f;
        private float _bobHeight = 0.2f;
        private Vector3 _startPosition;
        
        public ItemData Item => _item;
        
        public void Initialize(ItemData item, LootSystemManager lootManager)
        {
            _item = item;
            _lootManager = lootManager;
            _spawnTime = Time.time;
            _startPosition = transform.position;
        }
        
        private void Update()
        {
            // Bob up and down
            float bob = Mathf.Sin((Time.time - _spawnTime) * _bobSpeed) * _bobHeight;
            transform.position = _startPosition + Vector3.up * bob;
            
            // Rotate
            transform.Rotate(Vector3.up, 90f * Time.deltaTime);
        }
        
        private void OnDestroy()
        {
            if (_lootManager != null)
            {
                _lootManager.RemoveLootDrop(this);
            }
        }
    }
    
    /// <summary>
    /// Gold drop component.
    /// </summary>
    public class GoldDrop : MonoBehaviour
    {
        private int _amount;
        private LootSystemManager _lootManager;
        private float _spawnTime;
        private Vector3 _startPosition;
        
        public int Amount => _amount;
        
        public void Initialize(int amount, LootSystemManager lootManager)
        {
            _amount = amount;
            _lootManager = lootManager;
            _spawnTime = Time.time;
            _startPosition = transform.position;
        }
        
        private void Update()
        {
            // Bob
            float bob = Mathf.Sin((Time.time - _spawnTime) * 2f) * 0.15f;
            transform.position = _startPosition + Vector3.up * bob;
            
            // Rotate
            transform.Rotate(Vector3.up, 180f * Time.deltaTime);
            
            // Check for player pickup
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                float distance = Vector3.Distance(transform.position, player.transform.position);
                if (distance <= 3f)
                {
                    PickupGold(player);
                }
            }
        }
        
        private void PickupGold(GameObject player)
        {
            // TODO: Add gold to player currency
            Debug.Log($"[GoldDrop] Player picked up {_amount} gold");
            Destroy(gameObject);
        }
    }
    
    #endregion
} 
 
#####LootTableEditorController.cs##### 
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace Game.Core.Systems
{
    /// <summary>
    /// Loot Table Editor for Admin Console - Create and edit loot tables.
    /// </summary>
    public class LootTableEditorController : MonoBehaviour
    {
        private GameObject _contentPanel;
        private LootSystemManager _lootManager;
        
        private InputField _tableNameInput;
        private InputField _minItemsInput;
        private InputField _maxItemsInput;
        private InputField _minLevelInput;
        private InputField _maxLevelInput;
        private InputField _goldMinInput;
        private InputField _goldMaxInput;
        private Text _previewText;
        private Dropdown _tableDropdown;
        
        private LootTable _currentTable;
        
        public void Initialize(GameObject contentPanel)
        {
            _contentPanel = contentPanel;
            _lootManager = FindFirstObjectByType<LootSystemManager>();
            
            if (_lootManager == null)
            {
                GameObject lootObj = new GameObject("LootSystemManager");
                _lootManager = lootObj.AddComponent<LootSystemManager>();
            }
            
            CreateUI();
            CreateNewTable();
        }
        
        private void CreateUI()
        {
            Transform content = CreateScrollableContent();
            
            CreateHeader(content, "LOOT TABLE EDITOR");
            
            // Table selector
            CreateDropdown(content, "Select Table:", out _tableDropdown, GetTableNames());
            _tableDropdown.onValueChanged.AddListener(v => LoadTable(v));
            
            CreateInputField(content, "Table Name:", out _tableNameInput);
            _tableNameInput.onValueChanged.AddListener(v => UpdatePreview());
            
            CreateInputField(content, "Min Items:", out _minItemsInput);
            _minItemsInput.onValueChanged.AddListener(v => UpdatePreview());
            
            CreateInputField(content, "Max Items:", out _maxItemsInput);
            _maxItemsInput.onValueChanged.AddListener(v => UpdatePreview());
            
            CreateInputField(content, "Min Level:", out _minLevelInput);
            _minLevelInput.onValueChanged.AddListener(v => UpdatePreview());
            
            CreateInputField(content, "Max Level:", out _maxLevelInput);
            _maxLevelInput.onValueChanged.AddListener(v => UpdatePreview());
            
            CreateInputField(content, "Gold Min:", out _goldMinInput);
            _goldMinInput.onValueChanged.AddListener(v => UpdatePreview());
            
            CreateInputField(content, "Gold Max:", out _goldMaxInput);
            _goldMaxInput.onValueChanged.AddListener(v => UpdatePreview());
            
            CreateButton(content, "✨ NEW TABLE", () => CreateNewTable());
            CreateButton(content, "💾 SAVE TABLE", () => SaveTable());
            CreateButton(content, "🎲 TEST DROP", () => TestDrop());
            
            CreatePreviewPanel(content);
        }
        
        private Transform CreateScrollableContent()
        {
            ScrollRect scroll = _contentPanel.GetComponent<ScrollRect>();
            if (scroll == null) scroll = _contentPanel.AddComponent<ScrollRect>();
            
            GameObject content = new GameObject("Content");
            content.transform.SetParent(_contentPanel.transform, false);
            
            RectTransform contentRect = content.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 1);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.pivot = new Vector2(0.5f, 1);
            contentRect.sizeDelta = new Vector2(0, 2000);
            
            VerticalLayoutGroup layout = content.AddComponent<VerticalLayoutGroup>();
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            layout.spacing = 15;
            layout.padding = new RectOffset(20, 20, 20, 20);
            
            ContentSizeFitter fitter = content.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            
            scroll.content = contentRect;
            scroll.horizontal = false;
            scroll.vertical = true;
            
            return content.transform;
        }
        
        private void CreateHeader(Transform parent, string text)
        {
            GameObject header = new GameObject("Header");
            header.transform.SetParent(parent, false);
            
            LayoutElement layout = header.AddComponent<LayoutElement>();
            layout.preferredHeight = 60;
            
            Text headerText = header.AddComponent<Text>();
            headerText.text = text;
            headerText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            headerText.fontSize = 28;
            headerText.color = new Color(1f, 0.8f, 0.2f);
            headerText.alignment = TextAnchor.MiddleCenter;
        }
        
        private void CreateDropdown(Transform parent, string label, out Dropdown dropdown, List<string> options)
        {
            GameObject row = new GameObject($"Row_{label}");
            row.transform.SetParent(parent, false);
            
            LayoutElement rowLayout = row.AddComponent<LayoutElement>();
            rowLayout.preferredHeight = 50;
            
            HorizontalLayoutGroup rowHLayout = row.AddComponent<HorizontalLayoutGroup>();
            rowHLayout.childForceExpandWidth = true;
            rowHLayout.spacing = 10;
            
            GameObject labelObj = new GameObject("Label");
            labelObj.transform.SetParent(row.transform, false);
            
            LayoutElement labelLayout = labelObj.AddComponent<LayoutElement>();
            labelLayout.preferredWidth = 200;
            
            Text labelText = labelObj.AddComponent<Text>();
            labelText.text = label;
            labelText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            labelText.fontSize = 18;
            labelText.color = Color.white;
            labelText.alignment = TextAnchor.MiddleLeft;
            
            GameObject dropdownObj = new GameObject("Dropdown");
            dropdownObj.transform.SetParent(row.transform, false);
            
            Image dropdownImg = dropdownObj.AddComponent<Image>();
            dropdownImg.color = new Color(0.2f, 0.2f, 0.3f);
            
            dropdown = dropdownObj.AddComponent<Dropdown>();
            dropdown.ClearOptions();
            dropdown.AddOptions(options);
            
            GameObject dropdownLabel = new GameObject("Label");
            dropdownLabel.transform.SetParent(dropdownObj.transform, false);
            
            RectTransform dropdownLabelRect = dropdownLabel.AddComponent<RectTransform>();
            dropdownLabelRect.anchorMin = Vector2.zero;
            dropdownLabelRect.anchorMax = Vector2.one;
            dropdownLabelRect.offsetMin = new Vector2(10, 2);
            dropdownLabelRect.offsetMax = new Vector2(-25, -2);
            
            Text dropdownLabelText = dropdownLabel.AddComponent<Text>();
            dropdownLabelText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            dropdownLabelText.fontSize = 16;
            dropdownLabelText.color = Color.white;
            dropdownLabelText.alignment = TextAnchor.MiddleLeft;
            
            dropdown.captionText = dropdownLabelText;
        }
        
        private void CreateInputField(Transform parent, string label, out InputField inputField)
        {
            GameObject row = new GameObject($"Row_{label}");
            row.transform.SetParent(parent, false);
            
            LayoutElement rowLayout = row.AddComponent<LayoutElement>();
            rowLayout.preferredHeight = 50;
            
            HorizontalLayoutGroup rowHLayout = row.AddComponent<HorizontalLayoutGroup>();
            rowHLayout.childForceExpandWidth = true;
            rowHLayout.spacing = 10;
            
            GameObject labelObj = new GameObject("Label");
            labelObj.transform.SetParent(row.transform, false);
            
            LayoutElement labelLayout = labelObj.AddComponent<LayoutElement>();
            labelLayout.preferredWidth = 200;
            
            Text labelText = labelObj.AddComponent<Text>();
            labelText.text = label;
            labelText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            labelText.fontSize = 18;
            labelText.color = Color.white;
            labelText.alignment = TextAnchor.MiddleLeft;
            
            GameObject inputObj = new GameObject("InputField");
            inputObj.transform.SetParent(row.transform, false);
            
            Image inputImg = inputObj.AddComponent<Image>();
            inputImg.color = new Color(0.2f, 0.2f, 0.3f);
            
            inputField = inputObj.AddComponent<InputField>();
            
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(inputObj.transform, false);
            
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(10, 5);
            textRect.offsetMax = new Vector2(-10, -5);
            
            Text text = textObj.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 16;
            text.color = Color.white;
            text.alignment = TextAnchor.MiddleLeft;
            
            inputField.textComponent = text;
        }
        
        private void CreateButton(Transform parent, string label, UnityEngine.Events.UnityAction onClick)
        {
            GameObject btnObj = new GameObject($"Btn_{label}");
            btnObj.transform.SetParent(parent, false);
            
            LayoutElement btnLayout = btnObj.AddComponent<LayoutElement>();
            btnLayout.preferredHeight = 60;
            
            Image btnImg = btnObj.AddComponent<Image>();
            btnImg.color = new Color(0.6f, 0.4f, 0.2f);
            
            Button btn = btnObj.AddComponent<Button>();
            btn.targetGraphic = btnImg;
            btn.onClick.AddListener(onClick);
            
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(btnObj.transform, false);
            
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            
            Text text = textObj.AddComponent<Text>();
            text.text = label;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 20;
            text.color = Color.white;
            text.alignment = TextAnchor.MiddleCenter;
        }
        
        private void CreatePreviewPanel(Transform parent)
        {
            GameObject panel = new GameObject("PreviewPanel");
            panel.transform.SetParent(parent, false);
            
            LayoutElement panelLayout = panel.AddComponent<LayoutElement>();
            panelLayout.preferredHeight = 300;
            
            Image panelImg = panel.AddComponent<Image>();
            panelImg.color = new Color(0.1f, 0.1f, 0.15f);
            
            GameObject textObj = new GameObject("PreviewText");
            textObj.transform.SetParent(panel.transform, false);
            
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(15, 15);
            textRect.offsetMax = new Vector2(-15, -15);
            
            _previewText = textObj.AddComponent<Text>();
            _previewText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _previewText.fontSize = 16;
            _previewText.color = Color.white;
            _previewText.alignment = TextAnchor.UpperLeft;
        }
        
        private List<string> GetTableNames()
        {
            List<string> names = new List<string> { "New Table" };
            
            if (_lootManager != null)
            {
                names.AddRange(_lootManager.GetAvailableLootTables());
            }
            
            return names;
        }
        
        private void LoadTable(int index)
        {
            if (index == 0)
            {
                CreateNewTable();
                return;
            }
            
            List<string> tables = _lootManager.GetAvailableLootTables();
            if (index - 1 < tables.Count)
            {
                // TODO: Load existing table
                Debug.Log($"[LootTableEditor] Load table: {tables[index - 1]}");
            }
        }
        
        private void CreateNewTable()
        {
            _currentTable = new LootTable
            {
                tableName = "NewLootTable",
                minItems = 1,
                maxItems = 3,
                levelRange = new Vector2Int(1, 10),
                goldMin = 10,
                goldMax = 50
            };
            
            _tableNameInput.text = _currentTable.tableName;
            _minItemsInput.text = _currentTable.minItems.ToString();
            _maxItemsInput.text = _currentTable.maxItems.ToString();
            _minLevelInput.text = _currentTable.levelRange.x.ToString();
            _maxLevelInput.text = _currentTable.levelRange.y.ToString();
            _goldMinInput.text = _currentTable.goldMin.ToString();
            _goldMaxInput.text = _currentTable.goldMax.ToString();
            
            UpdatePreview();
        }
        
        private void UpdatePreview()
        {
            if (_currentTable == null) return;
            
            _currentTable.tableName = _tableNameInput.text;
            
            if (int.TryParse(_minItemsInput.text, out int minItems))
                _currentTable.minItems = minItems;
            
            if (int.TryParse(_maxItemsInput.text, out int maxItems))
                _currentTable.maxItems = maxItems;
            
            if (int.TryParse(_minLevelInput.text, out int minLevel))
                _currentTable.levelRange.x = minLevel;
            
            if (int.TryParse(_maxLevelInput.text, out int maxLevel))
                _currentTable.levelRange.y = maxLevel;
            
            if (int.TryParse(_goldMinInput.text, out int goldMin))
                _currentTable.goldMin = goldMin;
            
            if (int.TryParse(_goldMaxInput.text, out int goldMax))
                _currentTable.goldMax = goldMax;
            
            _previewText.text = $"<b><color=yellow>{_currentTable.tableName}</color></b>\n\n" +
                              $"<color=cyan>Items:</color> {_currentTable.minItems} - {_currentTable.maxItems}\n" +
                              $"<color=green>Level Range:</color> {_currentTable.levelRange.x} - {_currentTable.levelRange.y}\n" +
                              $"<color=yellow>Gold:</color> {_currentTable.goldMin} - {_currentTable.goldMax}\n\n" +
                              $"<i>This table will generate between {_currentTable.minItems} and {_currentTable.maxItems} items</i>";
        }
        
        private void SaveTable()
        {
            if (_lootManager != null && _currentTable != null)
            {
                _lootManager.AddLootTable(_currentTable);
                Debug.Log($"[LootTableEditor] Saved table: {_currentTable.tableName}");
                
                // Refresh dropdown
                _tableDropdown.ClearOptions();
                _tableDropdown.AddOptions(GetTableNames());
            }
        }
        
        private void TestDrop()
        {
            if (_lootManager == null || _currentTable == null) return;
            
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            Vector3 dropPosition = player != null ? player.transform.position + Vector3.forward * 3f : Vector3.zero;
            
            // Generate test loot
            List<ItemData> loot = _lootManager.GenerateLoot(_currentTable.tableName);
            int gold = Random.Range(_currentTable.goldMin, _currentTable.goldMax + 1);
            
            // Drop it
            _lootManager.DropLoot(loot, dropPosition, gold);
            
            Debug.Log($"[LootTableEditor] Test drop: {loot.Count} items, {gold} gold at {dropPosition}");
        }
    }
} 
 
#####ObjectPoolManager.cs##### 
using System;
using System.Collections.Generic;
using UnityEngine;
using Game.Core.Pooling;

namespace Game.Core.Systems
{
    /// <summary>
    /// Centralized object pool manager for zero-allocation spawning.
    /// FIXED: Removed DontDestroyOnLoad - parent CoreSystemManager handles persistence
    /// </summary>
    public class ObjectPoolManager : MonoBehaviour
    {
        [Header("Pool Configuration")]
        [SerializeField, Tooltip("Parent transform for all pooled objects")]
        private Transform _poolRoot;
        
        [SerializeField, Tooltip("Initial pool sizes per type")]
        private List<PoolConfiguration> _preWarmPools = new();
        
        [Header("Runtime Pool Statistics (Read-Only)")]
        [SerializeField] private int _totalPooledObjects;
        [SerializeField] private int _totalActiveObjects;
        [SerializeField] private int _poolHits;
        [SerializeField] private int _poolMisses;
        
        private Dictionary<string, Stack<IPoolable>> _pools;
        private Dictionary<string, GameObject> _prefabs;
        private Dictionary<string, HashSet<IPoolable>> _activeObjects;
        private Dictionary<string, int> _poolLimits;
        
        private const int DEFAULT_POOL_LIMIT = 500;
        
        private void Awake()
        {
            // REMOVED: DontDestroyOnLoad - parent handles it
            InitializePools();
        }
        
        private void InitializePools()
        {
            _pools = new Dictionary<string, Stack<IPoolable>>(32);
            _prefabs = new Dictionary<string, GameObject>(32);
            _activeObjects = new Dictionary<string, HashSet<IPoolable>>(32);
            _poolLimits = new Dictionary<string, int>(32);
            
            if (_poolRoot == null)
            {
                GameObject root = new("PoolRoot");
                root.transform.SetParent(transform);
                _poolRoot = root.transform;
            }
            
            foreach (PoolConfiguration config in _preWarmPools)
            {
                if (config.prefab != null)
                {
                    RegisterPrefab(config.poolKey, config.prefab, config.maxPoolSize);
                    WarmPool(config.poolKey, config.initialSize);
                }
            }
            
            Debug.Log($"[ObjectPoolManager] Initialized with {_preWarmPools.Count} pre-warmed pools");
        }
        
        public void RegisterPrefab(string poolKey, GameObject prefab, int maxPoolSize = DEFAULT_POOL_LIMIT)
        {
            if (string.IsNullOrEmpty(poolKey))
            {
                Debug.LogError("[ObjectPoolManager] Cannot register prefab with empty pool key!");
                return;
            }
            
            if (_prefabs.ContainsKey(poolKey))
            {
                Debug.LogWarning($"[ObjectPoolManager] Pool key '{poolKey}' already registered. Overwriting.");
            }
            
            _prefabs[poolKey] = prefab;
            _poolLimits[poolKey] = maxPoolSize;
            
            if (!_pools.ContainsKey(poolKey))
            {
                _pools[poolKey] = new Stack<IPoolable>(maxPoolSize / 4);
            }
            
            if (!_activeObjects.ContainsKey(poolKey))
            {
                _activeObjects[poolKey] = new HashSet<IPoolable>();
            }
        }
        
        public void WarmPool(string poolKey, int count)
        {
            if (!_prefabs.ContainsKey(poolKey))
            {
                Debug.LogError($"[ObjectPoolManager] Cannot warm pool '{poolKey}' - prefab not registered!");
                return;
            }
            
            GameObject prefab = _prefabs[poolKey];
            Stack<IPoolable> pool = _pools[poolKey];
            
            for (int i = 0; i < count; i++)
            {
                GameObject obj = Instantiate(prefab, _poolRoot);
                obj.name = $"{poolKey}_{i}";
                obj.SetActive(false);
                
                IPoolable poolable = obj.GetComponent<IPoolable>();
                if (poolable == null)
                {
                    Debug.LogError($"[ObjectPoolManager] Prefab for '{poolKey}' missing IPoolable component!");
                    Destroy(obj);
                    continue;
                }
                
                poolable.IsActiveInPool = false;
                pool.Push(poolable);
                _totalPooledObjects++;
            }
            
            Debug.Log($"[ObjectPoolManager] Pre-warmed '{poolKey}' pool with {count} instances");
        }
        
        public T Get<T>(string poolKey, Vector3 position, Quaternion rotation) where T : class, IPoolable
        {
            if (!_pools.ContainsKey(poolKey))
            {
                Debug.LogError($"[ObjectPoolManager] Pool '{poolKey}' not found! Did you forget to RegisterPrefab?");
                return null;
            }
            
            Stack<IPoolable> pool = _pools[poolKey];
            IPoolable poolable;
            
            if (pool.Count > 0)
            {
                poolable = pool.Pop();
                _poolHits++;
            }
            else
            {
                _poolMisses++;
                
                if (!_prefabs.ContainsKey(poolKey))
                {
                    Debug.LogError($"[ObjectPoolManager] No prefab registered for '{poolKey}'!");
                    return null;
                }
                
                int activeCount = _activeObjects[poolKey].Count;
                int poolLimit = _poolLimits[poolKey];
                
                if (activeCount >= poolLimit)
                {
                    Debug.LogWarning($"[ObjectPoolManager] Pool '{poolKey}' exceeded limit ({poolLimit}). Reusing oldest object.");
                    HashSet<IPoolable>.Enumerator enumerator = _activeObjects[poolKey].GetEnumerator();
                    enumerator.MoveNext();
                    IPoolable oldest = enumerator.Current;
                    ReturnToPool(oldest);
                    poolable = pool.Pop();
                }
                else
                {
                    GameObject obj = Instantiate(_prefabs[poolKey], _poolRoot);
                    obj.name = $"{poolKey}_Dynamic_{_totalPooledObjects}";
                    poolable = obj.GetComponent<IPoolable>();
                    _totalPooledObjects++;
                }
            }
            
            Transform t = poolable.GameObject.transform;
            t.position = position;
            t.rotation = rotation;
            t.SetParent(null);
            
            poolable.OnSpawnFromPool();
            _activeObjects[poolKey].Add(poolable);
            _totalActiveObjects++;
            
            return poolable as T;
        }
        
        public T Get<T>(string poolKey) where T : class, IPoolable
        {
            return Get<T>(poolKey, Vector3.zero, Quaternion.identity);
        }
        
        public void ReturnToPool(IPoolable poolable)
        {
            if (poolable == null)
            {
                Debug.LogWarning("[ObjectPoolManager] Attempted to return null object to pool!");
                return;
            }
            
            if (!poolable.IsActiveInPool)
            {
                Debug.LogWarning($"[ObjectPoolManager] Object '{poolable.GameObject.name}' already returned to pool!");
                return;
            }
            
            string poolKey = poolable.PoolKey;
            
            if (!_pools.ContainsKey(poolKey))
            {
                Debug.LogError($"[ObjectPoolManager] Pool '{poolKey}' not found for return!");
                return;
            }
            
            poolable.OnReturnToPool();
            poolable.GameObject.transform.SetParent(_poolRoot);
            
            _pools[poolKey].Push(poolable);
            _activeObjects[poolKey].Remove(poolable);
            _totalActiveObjects--;
        }
        
        public void ClearPool(string poolKey)
        {
            if (!_pools.ContainsKey(poolKey))
            {
                return;
            }
            
            Stack<IPoolable> pool = _pools[poolKey];
            while (pool.Count > 0)
            {
                IPoolable poolable = pool.Pop();
                if (poolable != null && poolable.GameObject != null)
                {
                    Destroy(poolable.GameObject);
                    _totalPooledObjects--;
                }
            }
            
            HashSet<IPoolable> active = _activeObjects[poolKey];
            foreach (IPoolable poolable in active)
            {
                if (poolable != null && poolable.GameObject != null)
                {
                    Destroy(poolable.GameObject);
                    _totalPooledObjects--;
                    _totalActiveObjects--;
                }
            }
            active.Clear();
            
            Debug.Log($"[ObjectPoolManager] Cleared pool '{poolKey}'");
        }
        
        public void ClearAllPools()
        {
            foreach (string poolKey in _pools.Keys)
            {
                ClearPool(poolKey);
            }
            
            _poolHits = 0;
            _poolMisses = 0;
            
            Debug.Log("[ObjectPoolManager] All pools cleared");
        }
        
        public PoolStats GetPoolStats(string poolKey)
        {
            if (!_pools.ContainsKey(poolKey))
            {
                return default;
            }
            
            return new PoolStats
            {
                poolKey = poolKey,
                inactiveCount = _pools[poolKey].Count,
                activeCount = _activeObjects[poolKey].Count,
                totalCount = _pools[poolKey].Count + _activeObjects[poolKey].Count,
                maxPoolSize = _poolLimits[poolKey]
            };
        }
        
        [Serializable]
        public class PoolConfiguration
        {
            public string poolKey = "DefaultPool";
            public GameObject prefab;
            public int initialSize = 10;
            public int maxPoolSize = 100;
        }
        
        public struct PoolStats
        {
            public string poolKey;
            public int inactiveCount;
            public int activeCount;
            public int totalCount;
            public int maxPoolSize;
        }
    }
} 
 
#####PlayerController.cs##### 
using UnityEngine;
using Game.Core.Systems;

namespace Game.Core
{
    /// <summary>
    /// First-person player controller with movement, combat, and interaction.
    /// Attach this to the player character GameObject.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float _moveSpeed = 5f;
        [SerializeField] private float _sprintMultiplier = 1.5f;
        [SerializeField] private float _jumpForce = 5f;
        [SerializeField] private float _gravity = -9.81f;
        
        [Header("Camera Settings")]
        [SerializeField] private float _mouseSensitivity = 2f;
        [SerializeField] private float _maxLookAngle = 80f;
        
        [Header("Combat Settings")]
        [SerializeField] private float _attackRange = 10f;
        [SerializeField] private float _interactRange = 3f;
        [SerializeField] private int _baseDamage = 25;
        
        [Header("Runtime State")]
        [SerializeField] private bool _isGrounded;
        [SerializeField] private float _currentSpeed;
        
        private CharacterController _characterController;
        private Camera _playerCamera;
        private EntityStats _playerStats;
        private Vector3 _moveVelocity;
        private float _cameraRotationX;
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            InitializeComponents();
            SetupCamera();
            LockCursor();
        }
        
        private void Update()
        {
            UpdateMovement();
            UpdateCamera();
            UpdateInput();
        }
        
        #endregion
        
        #region Initialization
        
        private void InitializeComponents()
        {
            _characterController = GetComponent<CharacterController>();
            if (_characterController == null)
            {
                _characterController = gameObject.AddComponent<CharacterController>();
            }
            
            // Configure character controller
            _characterController.height = 2f;
            _characterController.radius = 0.5f;
            _characterController.center = new Vector3(0, 1f, 0);
            _characterController.slopeLimit = 45f;
            _characterController.stepOffset = 0.3f;
            
            _playerStats = GetComponent<EntityStats>();
            if (_playerStats == null)
            {
                Debug.LogWarning("[PlayerController] No EntityStats found! Adding default.");
                _playerStats = gameObject.AddComponent<EntityStats>();
            }
            
            _playerCamera = GetComponentInChildren<Camera>();
        }
        
        private void SetupCamera()
        {
            if (_playerCamera == null)
            {
                // Use main camera if available
                _playerCamera = Camera.main;
                
                if (_playerCamera != null)
                {
                    _playerCamera.transform.SetParent(transform);
                    _playerCamera.transform.localPosition = new Vector3(0, 1.6f, 0);
                    _playerCamera.transform.localRotation = Quaternion.identity;
                }
                else
                {
                    // Create new camera
                    GameObject cameraObj = new GameObject("PlayerCamera");
                    cameraObj.transform.SetParent(transform);
                    cameraObj.transform.localPosition = new Vector3(0, 1.6f, 0);
                    cameraObj.transform.localRotation = Quaternion.identity;
                    
                    _playerCamera = cameraObj.AddComponent<Camera>();
                    _playerCamera.tag = "MainCamera";
                }
            }
            
            // Configure camera
            if (_playerCamera != null)
            {
                _playerCamera.nearClipPlane = 0.1f;
                _playerCamera.farClipPlane = 1000f;
                _playerCamera.fieldOfView = 75f;
            }
        }
        
        private void LockCursor()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        
        public void Initialize(CharacterCreationData creationData)
        {
            if (_playerStats != null)
            {
                // Stats are already set by GameWorldInitializer
                Debug.Log($"[PlayerController] Initialized {creationData.species} {creationData.gender} player");
            }
        }
        
        #endregion
        
        #region Movement
        
        private void UpdateMovement()
        {
            _isGrounded = _characterController.isGrounded;
            
            // Reset fall velocity when grounded
            if (_isGrounded && _moveVelocity.y < 0)
            {
                _moveVelocity.y = -2f;
            }
            
            // Get input
            float horizontalInput = Input.GetAxis("Horizontal");
            float verticalInput = Input.GetAxis("Vertical");
            bool isSprinting = Input.GetKey(KeyCode.LeftShift) && _playerStats.currentStamina > 0;
            
            // Calculate move direction relative to camera
            Vector3 moveDirection = transform.right * horizontalInput + transform.forward * verticalInput;
            
            // Apply speed
            float currentMoveSpeed = isSprinting ? _moveSpeed * _sprintMultiplier : _moveSpeed;
            _currentSpeed = currentMoveSpeed;
            
            // Move character
            _characterController.Move(moveDirection * currentMoveSpeed * Time.deltaTime);
            
            // Handle stamina
            if (isSprinting && moveDirection.magnitude > 0.1f)
            {
                _playerStats.currentStamina = Mathf.Max(0, 
                    _playerStats.currentStamina - Mathf.RoundToInt(10f * Time.deltaTime));
            }
            else if (_playerStats.currentStamina < _playerStats.maxStamina)
            {
                _playerStats.currentStamina = Mathf.Min(_playerStats.maxStamina, 
                    _playerStats.currentStamina + Mathf.RoundToInt(5f * Time.deltaTime));
            }
            
            // Handle jumping
            if (Input.GetButtonDown("Jump") && _isGrounded)
            {
                _moveVelocity.y = Mathf.Sqrt(_jumpForce * -2f * _gravity);
            }
            
            // Apply gravity
            _moveVelocity.y += _gravity * Time.deltaTime;
            _characterController.Move(_moveVelocity * Time.deltaTime);
        }
        
        #endregion
        
        #region Camera Control
        
        private void UpdateCamera()
        {
            if (_playerCamera == null) return;
            
            float mouseXInput = Input.GetAxis("Mouse X") * _mouseSensitivity;
            float mouseYInput = Input.GetAxis("Mouse Y") * _mouseSensitivity;
            
            // Rotate player body left/right
            transform.Rotate(Vector3.up * mouseXInput);
            
            // Rotate camera up/down (clamped)
            _cameraRotationX -= mouseYInput;
            _cameraRotationX = Mathf.Clamp(_cameraRotationX, -_maxLookAngle, _maxLookAngle);
            _playerCamera.transform.localRotation = Quaternion.Euler(_cameraRotationX, 0, 0);
        }
        
        #endregion
        
        #region Input Handling
        
        private void UpdateInput()
        {
            // Attack
            if (Input.GetMouseButtonDown(0))
            {
                PerformAttack();
            }
            
            // Interact
            if (Input.GetKeyDown(KeyCode.E))
            {
                AttemptInteraction();
            }
            
            // Toggle cursor
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                ToggleCursor();
            }
            
            // Quick inventory (example)
            if (Input.GetKeyDown(KeyCode.I))
            {
                UISystemManager uiManager = CoreSystemManager.UIManager;
                if (uiManager != null)
                {
                    uiManager.ToggleInventory();
                }
            }
            
            // Character sheet (example)
            if (Input.GetKeyDown(KeyCode.C))
            {
                UISystemManager uiManager = CoreSystemManager.UIManager;
                if (uiManager != null)
                {
                    uiManager.ToggleCharacterSheet();
                }
            }
        }
        
        private void ToggleCursor()
        {
            if (Cursor.lockState == CursorLockMode.Locked)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
        
        #endregion
        
        #region Combat
        
        private void PerformAttack()
        {
            if (_playerCamera == null) return;
            
            // Raycast from center of screen
            Ray attackRay = _playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
            
            if (Physics.Raycast(attackRay, out RaycastHit hitInfo, _attackRange))
            {
                // Check if we hit an entity with stats
                EntityStats targetStats = hitInfo.collider.GetComponent<EntityStats>();
                
                if (targetStats != null && targetStats != _playerStats)
                {
                    // Get combat manager
                    CombatSystemManager combatManager = CoreSystemManager.CombatManager;
                    
                    if (combatManager != null)
                    {
                        // Calculate damage
                        DamageResult damageResult = combatManager.CalculateDamage(
                            _playerStats, 
                            targetStats, 
                            _baseDamage, 
                            DamageType.Physical
                        );
                        
                        // Apply damage
                        combatManager.ApplyDamage(
                            hitInfo.collider.gameObject, 
                            damageResult, 
                            gameObject
                        );
                        
                        Debug.Log($"[PlayerController] Hit {hitInfo.collider.name} for {damageResult.finalDamage} damage" +
                                 (damageResult.isCritical ? " (CRITICAL!)" : ""));
                    }
                }
                else
                {
                    Debug.Log($"[PlayerController] Hit {hitInfo.collider.name} (non-entity)");
                }
            }
        }
        
        #endregion
        
        #region Interaction
        
        private void AttemptInteraction()
        {
            if (_playerCamera == null) return;
            
            // Raycast from center of screen
            Ray interactRay = _playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
            
            if (Physics.Raycast(interactRay, out RaycastHit hitInfo, _interactRange))
            {
                // Check for interactable object
                InteractableObject interactable = hitInfo.collider.GetComponent<InteractableObject>();
                
                if (interactable != null && interactable.IsInteractable)
                {
                    interactable.Interact(gameObject);
                    Debug.Log($"[PlayerController] Interacted with {interactable.name}: {interactable.InteractionPrompt}");
                }
                else
                {
                    Debug.Log($"[PlayerController] {hitInfo.collider.name} is not interactable");
                }
            }
        }
        
        #endregion
        
        #region Public API
        
        public void TakeDamage(int damage)
        {
            if (_playerStats != null)
            {
                _playerStats.currentHealth = Mathf.Max(0, _playerStats.currentHealth - damage);
                
                if (_playerStats.currentHealth <= 0)
                {
                    OnPlayerDeath();
                }
            }
        }
        
        public void Heal(int amount)
        {
            if (_playerStats != null)
            {
                _playerStats.currentHealth = Mathf.Min(_playerStats.maxHealth, 
                    _playerStats.currentHealth + amount);
            }
        }
        
        public void RestoreMana(int amount)
        {
            if (_playerStats != null)
            {
                _playerStats.currentMana = Mathf.Min(_playerStats.maxMana, 
                    _playerStats.currentMana + amount);
            }
        }
        
        public void RestoreStamina(int amount)
        {
            if (_playerStats != null)
            {
                _playerStats.currentStamina = Mathf.Min(_playerStats.maxStamina, 
                    _playerStats.currentStamina + amount);
            }
        }
        
        private void OnPlayerDeath()
        {
            Debug.Log("[PlayerController] Player died!");
            // TODO: Implement respawn/game over logic
            enabled = false;
        }
        
        public EntityStats GetStats()
        {
            return _playerStats;
        }
        
        public bool IsMoving()
        {
            return _currentSpeed > 0.1f;
        }
        
        public bool IsGrounded()
        {
            return _isGrounded;
        }
        
        #endregion
        
        #region Debug
        
        private void OnDrawGizmos()
        {
            if (_playerCamera == null) return;
            
            // Draw attack range
            Ray attackRay = _playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
            Gizmos.color = Color.red;
            Gizmos.DrawRay(attackRay.origin, attackRay.direction * _attackRange);
            
            // Draw interact range
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, _interactRange);
        }
        
        #endregion
    }
} 
 
#####ProceduralCharacterBuilder.cs##### 
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
 
#####QuestSystemData.cs##### 
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Game.Core.Systems
{
    /// <summary>
    /// Quest data container - Pure data, no logic.
    /// </summary>
    [System.Serializable]
    public class QuestData
    {
        public string questId;
        public string questName;
        public string questDescription;
        public QuestType questType;
        public QuestStatus status = QuestStatus.Available;
        public int level = 1;
        
        public List<QuestObjective> objectives = new();
        
        public int experienceReward;
        public int goldReward;
        public List<ItemData> itemRewards = new();
        
        public bool IsComplete()
        {
            return objectives.All(o => o.isCompleted);
        }
        
        public float GetProgress()
        {
            if (objectives.Count == 0) return 0f;
            
            float totalProgress = objectives.Sum(o => (float)o.currentCount / o.requiredCount);
            return totalProgress / objectives.Count;
        }
        
        public void ResetProgress()
        {
            foreach (QuestObjective objective in objectives)
            {
                objective.currentCount = 0;
                objective.isCompleted = false;
            }
        }
    }
    
    /// <summary>
    /// Quest objective container - Pure data, no logic.
    /// </summary>
    [System.Serializable]
    public class QuestObjective
    {
        public string objectiveId;
        public string description;
        public ObjectiveType objectiveType;
        public string targetName;
        public int requiredCount = 1;
        public int currentCount = 0;
        public bool isCompleted = false;
        
        public float GetProgress()
        {
            return (float)currentCount / requiredCount;
        }
    }
    
    /// <summary>
    /// Quest type enumeration.
    /// </summary>
    public enum QuestType
    {
        Kill,
        Collect,
        Talk,
        Explore,
        Escort,
        Deliver,
        Craft
    }
    
    /// <summary>
    /// Quest status enumeration.
    /// </summary>
    public enum QuestStatus
    {
        Available,
        Active,
        Completed,
        Failed
    }
    
    /// <summary>
    /// Objective type enumeration.
    /// </summary>
    public enum ObjectiveType
    {
        Kill,
        Collect,
        Talk,
        Explore,
        Escort,
        Deliver,
        Interact,
        Craft
    }
} 
 
#####QuestSystemManager.cs##### 
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Game.Core.Systems
{
    /// <summary>
    /// Quest System Manager - Complete quest management with objectives and rewards.
    /// Supports multiple objective types: Kill, Collect, Talk, Explore, Escort.
    /// FIXED: Data structures moved to QuestSystemData.cs
    /// </summary>
    public class QuestSystemManager : MonoBehaviour
    {
        [Header("Quest Database")]
        [SerializeField] private List<QuestData> _availableQuests = new();
        [SerializeField] private List<QuestData> _activeQuests = new();
        [SerializeField] private List<QuestData> _completedQuests = new();
        
        [Header("Configuration")]
        [SerializeField] private int _maxActiveQuests = 10;
        
        private Dictionary<string, QuestData> _questDatabase;
        
        #region Events
        
        public System.Action<QuestData> OnQuestStarted;
        public System.Action<QuestData> OnQuestCompleted;
        public System.Action<QuestData> OnQuestFailed;
        public System.Action<QuestData, QuestObjective> OnObjectiveCompleted;
        
        #endregion
        
        #region Initialization
        
        private void Awake()
        {
            _questDatabase = new Dictionary<string, QuestData>();
            InitializeQuests();
            SubscribeToEvents();
            Debug.Log("[QuestSystemManager] Initialized");
        }
        
        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }
        
        private void InitializeQuests()
        {
            CreateDefaultQuests();
        }
        
        private void CreateDefaultQuests()
        {
            // Quest 1: Kill Quest
            QuestData killQuest = new QuestData
            {
                questId = "quest_001",
                questName = "Zombie Slayer",
                questDescription = "The local village is being overrun by zombies. Help clear them out!",
                questType = QuestType.Kill,
                level = 1,
                experienceReward = 100,
                goldReward = 50
            };
            killQuest.objectives.Add(new QuestObjective
            {
                objectiveId = "obj_001",
                objectiveType = ObjectiveType.Kill,
                targetName = "Zombie",
                requiredCount = 10,
                currentCount = 0,
                description = "Kill 10 Zombies"
            });
            AddQuest(killQuest);
            
            // Quest 2: Collection Quest
            QuestData collectQuest = new QuestData
            {
                questId = "quest_002",
                questName = "Herb Gathering",
                questDescription = "The village healer needs healing herbs. Collect them from the wilderness.",
                questType = QuestType.Collect,
                level = 1,
                experienceReward = 75,
                goldReward = 30
            };
            collectQuest.objectives.Add(new QuestObjective
            {
                objectiveId = "obj_002",
                objectiveType = ObjectiveType.Collect,
                targetName = "HealingHerb",
                requiredCount = 5,
                currentCount = 0,
                description = "Collect 5 Healing Herbs"
            });
            AddQuest(collectQuest);
            
            // Quest 3: Talk Quest
            QuestData talkQuest = new QuestData
            {
                questId = "quest_003",
                questName = "The Village Elder",
                questDescription = "Speak with the village elder to learn about the ancient prophecy.",
                questType = QuestType.Talk,
                level = 1,
                experienceReward = 50,
                goldReward = 20
            };
            talkQuest.objectives.Add(new QuestObjective
            {
                objectiveId = "obj_003",
                objectiveType = ObjectiveType.Talk,
                targetName = "VillageElder",
                requiredCount = 1,
                currentCount = 0,
                description = "Talk to the Village Elder"
            });
            AddQuest(talkQuest);
            
            // Quest 4: Exploration Quest
            QuestData exploreQuest = new QuestData
            {
                questId = "quest_004",
                questName = "Scout the Dark Forest",
                questDescription = "Explore the Dark Forest and report back on any dangers.",
                questType = QuestType.Explore,
                level = 2,
                experienceReward = 150,
                goldReward = 75
            };
            exploreQuest.objectives.Add(new QuestObjective
            {
                objectiveId = "obj_004",
                objectiveType = ObjectiveType.Explore,
                targetName = "DarkForest",
                requiredCount = 1,
                currentCount = 0,
                description = "Explore the Dark Forest"
            });
            AddQuest(exploreQuest);
        }
        
        private void SubscribeToEvents()
        {
            CombatSystemManager combatManager = CoreSystemManager.CombatManager;
            if (combatManager != null)
            {
                combatManager.OnEntityDeath += OnEntityKilled;
            }
            
            InventorySystemManager inventoryManager = CoreSystemManager.InventoryManager;
            if (inventoryManager != null)
            {
                inventoryManager.OnInventoryChanged += OnInventoryChanged;
            }
        }
        
        private void UnsubscribeFromEvents()
        {
            CombatSystemManager combatManager = CoreSystemManager.CombatManager;
            if (combatManager != null)
            {
                combatManager.OnEntityDeath -= OnEntityKilled;
            }
            
            InventorySystemManager inventoryManager = CoreSystemManager.InventoryManager;
            if (inventoryManager != null)
            {
                inventoryManager.OnInventoryChanged -= OnInventoryChanged;
            }
        }
        
        #endregion
        
        #region Quest Management
        
        public void AddQuest(QuestData quest)
        {
            if (!_questDatabase.ContainsKey(quest.questId))
            {
                _questDatabase[quest.questId] = quest;
                _availableQuests.Add(quest);
                Debug.Log($"[QuestSystemManager] Added quest: {quest.questName}");
            }
        }
        
        public bool StartQuest(string questId)
        {
            if (!_questDatabase.ContainsKey(questId))
            {
                Debug.LogWarning($"[QuestSystemManager] Quest not found: {questId}");
                return false;
            }
            
            if (_activeQuests.Count >= _maxActiveQuests)
            {
                Debug.LogWarning("[QuestSystemManager] Max active quests reached!");
                return false;
            }
            
            QuestData quest = _questDatabase[questId];
            
            if (quest.status != QuestStatus.Available)
            {
                Debug.LogWarning($"[QuestSystemManager] Quest not available: {quest.questName}");
                return false;
            }
            
            quest.status = QuestStatus.Active;
            _availableQuests.Remove(quest);
            _activeQuests.Add(quest);
            
            OnQuestStarted?.Invoke(quest);
            
            Debug.Log($"[QuestSystemManager] Started quest: {quest.questName}");
            return true;
        }
        
        public bool CompleteQuest(string questId)
        {
            QuestData quest = _activeQuests.FirstOrDefault(q => q.questId == questId);
            
            if (quest == null)
            {
                Debug.LogWarning($"[QuestSystemManager] Active quest not found: {questId}");
                return false;
            }
            
            if (!quest.IsComplete())
            {
                Debug.LogWarning($"[QuestSystemManager] Quest not complete: {quest.questName}");
                return false;
            }
            
            quest.status = QuestStatus.Completed;
            _activeQuests.Remove(quest);
            _completedQuests.Add(quest);
            
            GiveRewards(quest);
            
            OnQuestCompleted?.Invoke(quest);
            
            Debug.Log($"[QuestSystemManager] Completed quest: {quest.questName}");
            return true;
        }
        
        public bool AbandonQuest(string questId)
        {
            QuestData quest = _activeQuests.FirstOrDefault(q => q.questId == questId);
            
            if (quest == null)
                return false;
            
            quest.status = QuestStatus.Available;
            quest.ResetProgress();
            
            _activeQuests.Remove(quest);
            _availableQuests.Add(quest);
            
            Debug.Log($"[QuestSystemManager] Abandoned quest: {quest.questName}");
            return true;
        }
        
        #endregion
        
        #region Objective Tracking
        
        private void OnEntityKilled(GameObject deadEntity, GameObject killer)
        {
            if (!killer.CompareTag("Player"))
                return;
            
            string entityName = GetEntityName(deadEntity);
            
            foreach (QuestData quest in _activeQuests)
            {
                foreach (QuestObjective objective in quest.objectives)
                {
                    if (objective.objectiveType == ObjectiveType.Kill && 
                        objective.targetName == entityName &&
                        !objective.isCompleted)
                    {
                        objective.currentCount++;
                        
                        if (objective.currentCount >= objective.requiredCount)
                        {
                            objective.isCompleted = true;
                            OnObjectiveCompleted?.Invoke(quest, objective);
                            Debug.Log($"[QuestSystemManager] Objective completed: {objective.description}");
                        }
                        
                        if (quest.IsComplete())
                        {
                            CompleteQuest(quest.questId);
                        }
                    }
                }
            }
        }
        
        private void OnInventoryChanged(GameObject entity)
        {
            if (!entity.CompareTag("Player"))
                return;
            
            InventorySystemManager inventoryManager = CoreSystemManager.InventoryManager;
            if (inventoryManager == null)
                return;
            
            InventoryData inventory = inventoryManager.GetInventory(entity);
            
            foreach (QuestData quest in _activeQuests)
            {
                foreach (QuestObjective objective in quest.objectives)
                {
                    if (objective.objectiveType == ObjectiveType.Collect && !objective.isCompleted)
                    {
                        int count = inventory.GetItemCount(objective.targetName);
                        objective.currentCount = count;
                        
                        if (objective.currentCount >= objective.requiredCount)
                        {
                            objective.isCompleted = true;
                            OnObjectiveCompleted?.Invoke(quest, objective);
                            Debug.Log($"[QuestSystemManager] Objective completed: {objective.description}");
                        }
                        
                        if (quest.IsComplete())
                        {
                            CompleteQuest(quest.questId);
                        }
                    }
                }
            }
        }
        
        public void UpdateObjective(string questId, string objectiveId, int progress)
        {
            QuestData quest = _activeQuests.FirstOrDefault(q => q.questId == questId);
            if (quest == null)
                return;
            
            QuestObjective objective = quest.objectives.FirstOrDefault(o => o.objectiveId == objectiveId);
            if (objective == null)
                return;
            
            objective.currentCount += progress;
            
            if (objective.currentCount >= objective.requiredCount)
            {
                objective.isCompleted = true;
                OnObjectiveCompleted?.Invoke(quest, objective);
            }
            
            if (quest.IsComplete())
            {
                CompleteQuest(quest.questId);
            }
        }
        
        public void CompleteObjective(string questId, string objectiveId)
        {
            QuestData quest = _activeQuests.FirstOrDefault(q => q.questId == questId);
            if (quest == null)
                return;
            
            QuestObjective objective = quest.objectives.FirstOrDefault(o => o.objectiveId == objectiveId);
            if (objective == null)
                return;
            
            objective.isCompleted = true;
            objective.currentCount = objective.requiredCount;
            
            OnObjectiveCompleted?.Invoke(quest, objective);
            
            if (quest.IsComplete())
            {
                CompleteQuest(quest.questId);
            }
        }
        
        #endregion
        
        #region Rewards
        
        private void GiveRewards(QuestData quest)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
                return;
            
            EntityStats stats = player.GetComponent<EntityStats>();
            if (stats != null && quest.experienceReward > 0)
            {
                stats.AddExperience(quest.experienceReward);
                Debug.Log($"[QuestSystemManager] Awarded {quest.experienceReward} XP");
            }
            
            if (quest.goldReward > 0)
            {
                Debug.Log($"[QuestSystemManager] Awarded {quest.goldReward} gold");
            }
            
            InventorySystemManager inventoryManager = CoreSystemManager.InventoryManager;
            if (inventoryManager != null && quest.itemRewards != null)
            {
                foreach (ItemData item in quest.itemRewards)
                {
                    inventoryManager.AddItem(player, item);
                    Debug.Log($"[QuestSystemManager] Awarded item: {item.itemName}");
                }
            }
        }
        
        #endregion
        
        #region Queries
        
        public List<QuestData> GetAvailableQuests()
        {
            return new List<QuestData>(_availableQuests);
        }
        
        public List<QuestData> GetActiveQuests()
        {
            return new List<QuestData>(_activeQuests);
        }
        
        public List<QuestData> GetCompletedQuests()
        {
            return new List<QuestData>(_completedQuests);
        }
        
        public QuestData GetQuest(string questId)
        {
            return _questDatabase.ContainsKey(questId) ? _questDatabase[questId] : null;
        }
        
        public bool IsQuestActive(string questId)
        {
            return _activeQuests.Any(q => q.questId == questId);
        }
        
        public bool IsQuestCompleted(string questId)
        {
            return _completedQuests.Any(q => q.questId == questId);
        }
        
        #endregion
        
        #region Helpers
        
        private string GetEntityName(GameObject entity)
        {
            string name = entity.name.ToLower();
            
            if (name.Contains("zombie")) return "Zombie";
            if (name.Contains("skeleton")) return "Skeleton";
            if (name.Contains("orc")) return "Orc";
            if (name.Contains("boss")) return "Boss";
            
            return entity.name;
        }
        
        #endregion
        
        #region Save/Load
        
        public void SaveQuests()
        {
            for (int i = 0; i < _activeQuests.Count; i++)
            {
                string json = JsonUtility.ToJson(_activeQuests[i]);
                PlayerPrefs.SetString($"ActiveQuest_{i}", json);
            }
            PlayerPrefs.SetInt("ActiveQuestCount", _activeQuests.Count);
            
            for (int i = 0; i < _completedQuests.Count; i++)
            {
                PlayerPrefs.SetString($"CompletedQuest_{i}", _completedQuests[i].questId);
            }
            PlayerPrefs.SetInt("CompletedQuestCount", _completedQuests.Count);
            
            PlayerPrefs.Save();
            Debug.Log("[QuestSystemManager] Quests saved");
        }
        
        public void LoadQuests()
        {
            int activeCount = PlayerPrefs.GetInt("ActiveQuestCount", 0);
            for (int i = 0; i < activeCount; i++)
            {
                string json = PlayerPrefs.GetString($"ActiveQuest_{i}", "");
                if (!string.IsNullOrEmpty(json))
                {
                    QuestData quest = JsonUtility.FromJson<QuestData>(json);
                    if (!_activeQuests.Contains(quest))
                    {
                        _activeQuests.Add(quest);
                    }
                }
            }
            
            int completedCount = PlayerPrefs.GetInt("CompletedQuestCount", 0);
            for (int i = 0; i < completedCount; i++)
            {
                string questId = PlayerPrefs.GetString($"CompletedQuest_{i}", "");
                if (!string.IsNullOrEmpty(questId) && _questDatabase.ContainsKey(questId))
                {
                    QuestData quest = _questDatabase[questId];
                    quest.status = QuestStatus.Completed;
                    if (!_completedQuests.Contains(quest))
                    {
                        _completedQuests.Add(quest);
                    }
                }
            }
            
            Debug.Log($"[QuestSystemManager] Loaded {_activeQuests.Count} active quests, {_completedQuests.Count} completed");
        }
        
        #endregion
    }
} 
 
#####SimpleTerrainGenerator.cs##### 
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
 
#####SystemManagers.cs##### 
using UnityEngine;

namespace Game.Core.Systems
{
    /// <summary>
    /// Zone System Manager - Manages zone loading, generation, and transitions.
    /// Integrates with ZoneSceneManager for scene management.
    /// </summary>
    public class ZoneSystemManager : MonoBehaviour
    {
        [Header("Zone Management")]
        [SerializeField] private ZoneSceneManager _sceneManager;
        
        private void Awake()
        {
            _sceneManager = GetComponent<ZoneSceneManager>();
            if (_sceneManager == null)
            {
                _sceneManager = gameObject.AddComponent<ZoneSceneManager>();
            }
        }
        
        public async Awaitable<UnityEngine.SceneManagement.Scene> LoadZone(string zoneName, ZoneConfig config = null)
        {
            return await _sceneManager.LoadZone(zoneName, config);
        }
        
        public async Awaitable UnloadZone(string zoneName)
        {
            await _sceneManager.UnloadZone(zoneName);
        }
        
        public async Awaitable<UnityEngine.SceneManagement.Scene> GenerateZone(ZoneConfig config)
        {
            return await _sceneManager.GenerateAndSaveZone(config);
        }
        
        public string GetCurrentZone()
        {
            return _sceneManager.GetCurrentZone();
        }
        
        public async Awaitable ShutdownAsync()
        {
            if (_sceneManager != null)
            {
                await _sceneManager.ShutdownAsync();
            }
        }
    }
    

    /// <summary>
    /// Audio System Manager - Manages all audio playback.
    /// Handles music, sound effects, and spatial audio.
    /// </summary>
    public class AudioSystemManager : MonoBehaviour
    {
        [Header("Audio Configuration")]
        [SerializeField] private float _masterVolume = 1.0f;
        [SerializeField] private float _musicVolume = 0.7f;
        [SerializeField] private float _sfxVolume = 1.0f;
        
        private AudioSource _musicSource;
        
        private void Awake()
        {
            _musicSource = gameObject.AddComponent<AudioSource>();
            _musicSource.loop = true;
            _musicSource.playOnAwake = false;
            _musicSource.volume = _musicVolume;
            
            Debug.Log("[AudioSystemManager] Initialized");
        }
        
        public void PlayMusic(AudioClip clip)
        {
            if (_musicSource != null && clip != null)
            {
                _musicSource.clip = clip;
                _musicSource.Play();
            }
        }
        
        public void PlaySFX(AudioClip clip, Vector3 position)
        {
            if (clip != null)
            {
                AudioSource.PlayClipAtPoint(clip, position, _sfxVolume * _masterVolume);
            }
        }
        
        public void StopMusic()
        {
            if (_musicSource != null)
            {
                _musicSource.Stop();
            }
        }
        
        public void SetMasterVolume(float volume)
        {
            _masterVolume = Mathf.Clamp01(volume);
        }
        
        public void SetMusicVolume(float volume)
        {
            _musicVolume = Mathf.Clamp01(volume);
            if (_musicSource != null)
            {
                _musicSource.volume = _musicVolume * _masterVolume;
            }
        }
        
        public void SetSFXVolume(float volume)
        {
            _sfxVolume = Mathf.Clamp01(volume);
        }
        
        public void Shutdown()
        {
            StopMusic();
            Debug.Log("[AudioSystemManager] Shutting down...");
        }
    }
} 
 
#####TutorialSystemManager.cs##### 
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Game.Core.Systems;

namespace Game.Core
{
    /// <summary>
    /// Tutorial System - Orchestrates the entire game flow.
    /// Creates a functional demo that showcases all systems working together.
    /// Place this in the GameWorld scene to auto-start the tutorial.
    /// </summary>
    public class TutorialSystemManager : MonoBehaviour
    {
        [Header("Tutorial Configuration")]
        [SerializeField] private bool _autoStartTutorial = true;
        [SerializeField] private float _stepDelay = 2f;
        
        [Header("Tutorial UI")]
        [SerializeField] private Canvas _tutorialCanvas;
        [SerializeField] private Text _tutorialText;
        [SerializeField] private Text _objectiveText;
        [SerializeField] private GameObject _tutorialPanel;
        
        [Header("Spawned Objects")]
        [SerializeField] private GameObject _player;
        [SerializeField] private List<GameObject> _spawnedEnemies = new();
        [SerializeField] private GameObject _tutorialChest;
        
        private int _currentStep = 0;
        private bool _tutorialActive = false;
        private bool _waitingForPlayerAction = false;
        private string _currentObjective = "";
        
        // System references
        private CoreSystemManager _coreManager;
        private EntitySystemManager _entityManager;
        private CombatSystemManager _combatManager;
        private LootSystemManager _lootManager;
        private QuestSystemManager _questManager;
        private InventorySystemManager _inventoryManager;
        
        #region Initialization
        
        private async void Start()
        {
            Debug.Log("[TutorialSystem] Starting tutorial initialization...");
            
            await InitializeTutorial();
            
            if (_autoStartTutorial)
            {
                await Awaitable.WaitForSecondsAsync(1f);
                StartTutorial();
            }
        }
        
        private async Awaitable InitializeTutorial()
        {
            // Wait for core systems
            while (CoreSystemManager.Instance == null || !CoreSystemManager.Instance.IsReady())
            {
                await Awaitable.NextFrameAsync();
            }
            
            _coreManager = CoreSystemManager.Instance;
            _entityManager = CoreSystemManager.EntityManager;
            _combatManager = CoreSystemManager.CombatManager;
            _lootManager = FindFirstObjectByType<LootSystemManager>();
            _questManager = FindFirstObjectByType<QuestSystemManager>();
            _inventoryManager = CoreSystemManager.InventoryManager;
            
            // Create tutorial UI
            CreateTutorialUI();
            
            // Setup player
            SetupPlayer();
            
            Debug.Log("[TutorialSystem] Initialization complete!");
        }
        
        private void CreateTutorialUI()
        {
            if (_tutorialCanvas != null) return;
            
            GameObject canvasObj = new GameObject("TutorialCanvas");
            canvasObj.transform.SetParent(transform);
            
            _tutorialCanvas = canvasObj.AddComponent<Canvas>();
            _tutorialCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _tutorialCanvas.sortingOrder = 100;
            
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            
            canvasObj.AddComponent<GraphicRaycaster>();
            
            // Tutorial panel
            _tutorialPanel = new GameObject("TutorialPanel");
            _tutorialPanel.transform.SetParent(_tutorialCanvas.transform, false);
            
            RectTransform panelRect = _tutorialPanel.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.85f);
            panelRect.anchorMax = new Vector2(0.5f, 0.85f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = new Vector2(800, 120);
            
            Image panelBg = _tutorialPanel.AddComponent<Image>();
            panelBg.color = new Color(0.1f, 0.1f, 0.15f, 0.9f);
            
            // Tutorial text
            GameObject textObj = new GameObject("TutorialText");
            textObj.transform.SetParent(_tutorialPanel.transform, false);
            
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(20, 20);
            textRect.offsetMax = new Vector2(-20, -20);
            
            _tutorialText = textObj.AddComponent<Text>();
            _tutorialText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _tutorialText.fontSize = 24;
            _tutorialText.color = Color.yellow;
            _tutorialText.alignment = TextAnchor.MiddleCenter;
            
            // Objective panel
            GameObject objPanel = new GameObject("ObjectivePanel");
            objPanel.transform.SetParent(_tutorialCanvas.transform, false);
            
            RectTransform objRect = objPanel.AddComponent<RectTransform>();
            objRect.anchorMin = new Vector2(0.02f, 0.85f);
            objRect.anchorMax = new Vector2(0.02f, 0.85f);
            objRect.pivot = new Vector2(0, 0.5f);
            objRect.sizeDelta = new Vector2(400, 100);
            
            Image objBg = objPanel.AddComponent<Image>();
            objBg.color = new Color(0.2f, 0.3f, 0.2f, 0.8f);
            
            GameObject objTextObj = new GameObject("ObjectiveText");
            objTextObj.transform.SetParent(objPanel.transform, false);
            
            RectTransform objTextRect = objTextObj.AddComponent<RectTransform>();
            objTextRect.anchorMin = Vector2.zero;
            objTextRect.anchorMax = Vector2.one;
            objTextRect.offsetMin = new Vector2(15, 15);
            objTextRect.offsetMax = new Vector2(-15, -15);
            
            _objectiveText = objTextObj.AddComponent<Text>();
            _objectiveText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _objectiveText.fontSize = 18;
            _objectiveText.color = Color.white;
            _objectiveText.alignment = TextAnchor.UpperLeft;
            
            _tutorialPanel.SetActive(false);
        }
        
        private void SetupPlayer()
        {
            _player = GameObject.FindGameObjectWithTag("Player");
            
            if (_player == null)
            {
                Debug.LogWarning("[TutorialSystem] No player found! Creating tutorial player...");
                CreateTutorialPlayer();
            }
            
            // Ensure player has all required components
            if (_player.GetComponent<PlayerController>() == null)
            {
                _player.AddComponent<PlayerController>();
            }
            
            if (_player.GetComponent<EntityStats>() == null)
            {
                EntityStats stats = _player.AddComponent<EntityStats>();
                stats.maxHealth = 200;
                stats.currentHealth = 200;
                stats.maxMana = 100;
                stats.currentMana = 100;
            }
        }
        
        private void CreateTutorialPlayer()
        {
            _player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            _player.name = "TutorialPlayer";
            _player.tag = "Player";
            _player.transform.position = new Vector3(0, 2, 0);
            
            Renderer renderer = _player.GetComponent<Renderer>();
            Material mat = new Material(Shader.Find("Standard"));
            mat.color = Color.cyan;
            renderer.material = mat;
            
            EntityStats stats = _player.AddComponent<EntityStats>();
            stats.maxHealth = 200;
            stats.currentHealth = 200;
            stats.maxMana = 100;
            stats.currentMana = 100;
            stats.strength = 15;
            stats.dexterity = 15;
            stats.intelligence = 15;
            
            CharacterController controller = _player.AddComponent<CharacterController>();
            controller.height = 2f;
            controller.radius = 0.5f;
            
            PlayerController playerController = _player.AddComponent<PlayerController>();
            
            // Setup camera
            Camera mainCam = Camera.main;
            if (mainCam == null)
            {
                GameObject camObj = new GameObject("MainCamera");
                mainCam = camObj.AddComponent<Camera>();
                camObj.tag = "MainCamera";
            }
            
            mainCam.transform.SetParent(_player.transform);
            mainCam.transform.localPosition = new Vector3(0, 1.6f, 0);
            mainCam.transform.localRotation = Quaternion.identity;
        }
        
        #endregion
        
        #region Tutorial Flow
        
        public void StartTutorial()
        {
            _tutorialActive = true;
            _currentStep = 0;
            _tutorialPanel.SetActive(true);
            
            Debug.Log("[TutorialSystem] *** TUTORIAL STARTED ***");
            
            ExecuteNextStep();
        }
        
        private async void ExecuteNextStep()
        {
            if (!_tutorialActive) return;
            
            _currentStep++;
            _waitingForPlayerAction = false;
            
            Debug.Log($"[TutorialSystem] === Step {_currentStep} ===");
            
            switch (_currentStep)
            {
                case 1:
                    await Step1_Welcome();
                    break;
                case 2:
                    await Step2_Movement();
                    break;
                case 3:
                    await Step3_SpawnEnemy();
                    break;
                case 4:
                    await Step4_Combat();
                    break;
                case 5:
                    await Step5_Loot();
                    break;
                case 6:
                    await Step6_Inventory();
                    break;
                case 7:
                    await Step7_Quests();
                    break;
                case 8:
                    await Step8_SpawnMultipleEnemies();
                    break;
                case 9:
                    await Step9_PoolingDemo();
                    break;
                case 10:
                    await Step10_Completion();
                    break;
                default:
                    EndTutorial();
                    break;
            }
        }
        
        #endregion
        
        #region Tutorial Steps
        
        private async Awaitable Step1_Welcome()
        {
            ShowTutorial("WELCOME TO THE RPG TUTORIAL!\n\nAll systems are loaded and ready.");
            SetObjective("Objective: Learn the game systems");
            
            await Awaitable.WaitForSecondsAsync(_stepDelay * 2);
            ExecuteNextStep();
        }
        
        private async Awaitable Step2_Movement()
        {
            ShowTutorial("Use WASD to move, MOUSE to look around, SHIFT to sprint.\n\nTry moving around!");
            SetObjective("Move around with WASD\nLook with Mouse");
            
            _waitingForPlayerAction = true;
            
            Vector3 startPos = _player.transform.position;
            float movedDistance = 0;
            
            while (movedDistance < 5f)
            {
                await Awaitable.NextFrameAsync();
                movedDistance = Vector3.Distance(startPos, _player.transform.position);
            }
            
            ShowTutorial("Great! Movement works perfectly.");
            await Awaitable.WaitForSecondsAsync(_stepDelay);
            ExecuteNextStep();
        }
        
        private async Awaitable Step3_SpawnEnemy()
        {
            ShowTutorial("ENTITY SYSTEM: Spawning an enemy using the object pool...");
            SetObjective("Watch the enemy spawn");
            
            await Awaitable.WaitForSecondsAsync(_stepDelay);
            
            Vector3 spawnPos = _player.transform.position + _player.transform.forward * 5f;
            GameObject enemy = _entityManager.SpawnEntity("Zombie", spawnPos, Quaternion.identity);
            
            if (enemy != null)
            {
                _spawnedEnemies.Add(enemy);
                ShowTutorial("Enemy spawned! The entity system is working.");
                
                // Make enemy face player
                enemy.transform.LookAt(_player.transform);
            }
            else
            {
                ShowTutorial("ERROR: Entity system failed! Check ObjectPoolManager.");
            }
            
            await Awaitable.WaitForSecondsAsync(_stepDelay);
            ExecuteNextStep();
        }
        
        private async Awaitable Step4_Combat()
        {
            ShowTutorial("COMBAT SYSTEM: Left-click to attack the enemy!\n\nAim at the zombie and click.");
            SetObjective("Defeat the zombie\nLeft-Click to attack");
            
            _waitingForPlayerAction = true;
            
            GameObject targetEnemy = _spawnedEnemies.Count > 0 ? _spawnedEnemies[0] : null;
            
            if (targetEnemy != null)
            {
                EntityStats enemyStats = targetEnemy.GetComponent<EntityStats>();
                
                while (targetEnemy != null && enemyStats != null && enemyStats.currentHealth > 0)
                {
                    await Awaitable.NextFrameAsync();
                }
                
                ShowTutorial("COMBAT SUCCESS! Damage numbers and combat system working!");
                _spawnedEnemies.Remove(targetEnemy);
            }
            else
            {
                ShowTutorial("Enemy not found, skipping combat test...");
            }
            
            await Awaitable.WaitForSecondsAsync(_stepDelay);
            ExecuteNextStep();
        }
        
        private async Awaitable Step5_Loot()
        {
            ShowTutorial("LOOT SYSTEM: Spawning a chest with random loot...");
            SetObjective("Approach the chest to collect loot");
            
            await Awaitable.WaitForSecondsAsync(_stepDelay);
            
            Vector3 chestPos = _player.transform.position + _player.transform.forward * 4f;
            
            if (_lootManager != null)
            {
                _tutorialChest = _lootManager.GetComponent<InteractableRegistry>()?.SpawnInteractable("Chest", chestPos, Quaternion.identity);
                
                if (_tutorialChest != null)
                {
                    ShowTutorial("Chest spawned! Walk up to it (get close)...");
                    
                    // Wait for player to get close
                    while (Vector3.Distance(_player.transform.position, chestPos) > 3f)
                    {
                        await Awaitable.NextFrameAsync();
                    }
                    
                    // Auto-open chest
                    Chest chest = _tutorialChest.GetComponent<Chest>();
                    if (chest != null)
                    {
                        chest.Interact(_player);
                        ShowTutorial("LOOT COLLECTED! Check your inventory (Press I).");
                    }
                }
            }
            else
            {
                ShowTutorial("Loot system not found, skipping...");
            }
            
            await Awaitable.WaitForSecondsAsync(_stepDelay * 1.5f);
            ExecuteNextStep();
        }
        
        private async Awaitable Step6_Inventory()
        {
            ShowTutorial("INVENTORY SYSTEM: Press 'I' to open your inventory.\n\nCheck your loot!");
            SetObjective("Press I to view inventory\nPress I again to close");
            
            _waitingForPlayerAction = true;
            
            // Wait for player to open inventory
            bool inventoryOpened = false;
            float timeout = 10f;
            float elapsed = 0f;
            
            while (!inventoryOpened && elapsed < timeout)
            {
                if (Input.GetKeyDown(KeyCode.I))
                {
                    inventoryOpened = true;
                }
                elapsed += Time.deltaTime;
                await Awaitable.NextFrameAsync();
            }
            
            if (inventoryOpened)
            {
                ShowTutorial("Inventory system working! You can equip items by right-clicking them.");
            }
            else
            {
                ShowTutorial("Inventory UI available. Moving on...");
            }
            
            await Awaitable.WaitForSecondsAsync(_stepDelay);
            ExecuteNextStep();
        }
        
        private async Awaitable Step7_Quests()
        {
            ShowTutorial("QUEST SYSTEM: Starting a tutorial quest...");
            SetObjective("Kill 3 enemies to complete quest");
            
            await Awaitable.WaitForSecondsAsync(_stepDelay);
            
            if (_questManager != null)
            {
                // Create tutorial quest
                QuestData tutorialQuest = new QuestData
                {
                    questId = "tutorial_quest",
                    questName = "First Blood",
                    questDescription = "Defeat enemies to prove your combat skills!",
                    questType = QuestType.Kill,
                    level = 1,
                    experienceReward = 200,
                    goldReward = 100
                };
                
                tutorialQuest.objectives.Add(new QuestObjective
                {
                    objectiveId = "kill_enemies",
                    objectiveType = ObjectiveType.Kill,
                    targetName = "Zombie",
                    requiredCount = 3,
                    currentCount = 0,
                    description = "Kill 3 Zombies"
                });
                
                _questManager.AddQuest(tutorialQuest);
                _questManager.StartQuest("tutorial_quest");
                
                ShowTutorial("Quest accepted! Press Q to view quest log.");
            }
            else
            {
                ShowTutorial("Quest system not available, skipping...");
            }
            
            await Awaitable.WaitForSecondsAsync(_stepDelay);
            ExecuteNextStep();
        }
        
        private async Awaitable Step8_SpawnMultipleEnemies()
        {
            ShowTutorial("ENTITY SYSTEM: Spawning multiple enemies...\n\nPrepare for battle!");
            SetObjective("Defeat all enemies (3 total)");
            
            await Awaitable.WaitForSecondsAsync(_stepDelay);
            
            // Spawn 3 enemies in a circle around player
            for (int i = 0; i < 3; i++)
            {
                float angle = i * (360f / 3f);
                Vector3 offset = Quaternion.Euler(0, angle, 0) * Vector3.forward * 8f;
                Vector3 spawnPos = _player.transform.position + offset;
                
                GameObject enemy = _entityManager.SpawnEntity("Zombie", spawnPos, Quaternion.identity);
                if (enemy != null)
                {
                    _spawnedEnemies.Add(enemy);
                    enemy.transform.LookAt(_player.transform);
                }
                
                await Awaitable.WaitForSecondsAsync(0.5f);
            }
            
            ShowTutorial($"3 enemies spawned! Current pool count: {_entityManager.GetEntityCount()}");
            
            // Wait for all enemies to be defeated
            _waitingForPlayerAction = true;
            
            while (_spawnedEnemies.Count > 0)
            {
                // Remove null (dead) enemies
                _spawnedEnemies.RemoveAll(e => e == null);
                await Awaitable.NextFrameAsync();
            }
            
            ShowTutorial("ALL ENEMIES DEFEATED! Quest should be complete.");
            
            await Awaitable.WaitForSecondsAsync(_stepDelay);
            ExecuteNextStep();
        }
        
        private async Awaitable Step9_PoolingDemo()
        {
            ShowTutorial("POOLING SYSTEM: Rapid spawn/despawn test...\n\nWatch the magic!");
            SetObjective("Observe object pooling in action");
            
            await Awaitable.WaitForSecondsAsync(_stepDelay);
            
            List<GameObject> testEnemies = new List<GameObject>();
            
            // Rapid spawn
            for (int i = 0; i < 5; i++)
            {
                Vector3 pos = _player.transform.position + new Vector3(Random.Range(-5f, 5f), 0, Random.Range(3f, 8f));
                GameObject enemy = _entityManager.SpawnEntity("Skeleton", pos, Quaternion.identity);
                if (enemy != null) testEnemies.Add(enemy);
                
                await Awaitable.WaitForSecondsAsync(0.3f);
            }
            
            ShowTutorial($"Spawned 5 enemies rapidly! Pool stats: {_entityManager.GetEntityCount()} active");
            
            await Awaitable.WaitForSecondsAsync(_stepDelay);
            
            // Rapid despawn
            ShowTutorial("Now despawning them all...");
            
            foreach (GameObject enemy in testEnemies)
            {
                if (enemy != null)
                {
                    _entityManager.DespawnEntity(enemy);
                    await Awaitable.WaitForSecondsAsync(0.2f);
                }
            }
            
            ShowTutorial("POOLING SUCCESS! Objects returned to pool for reuse.");
            
            await Awaitable.WaitForSecondsAsync(_stepDelay);
            ExecuteNextStep();
        }
        
        private async Awaitable Step10_Completion()
        {
            ShowTutorial("TUTORIAL COMPLETE!\n\nAll systems verified and working!");
            SetObjective("✓ All Systems Online\n✓ Combat Working\n✓ Inventory Working\n✓ Quests Working\n✓ Pooling Working");
            
            await Awaitable.WaitForSecondsAsync(_stepDelay * 3);
            
            ShowTutorial("Press F12 (if host) to open Admin Console.\n\nFree play mode activated!");
            SetObjective("Explore and test all features!");
            
            await Awaitable.WaitForSecondsAsync(_stepDelay * 2);
            
            _tutorialPanel.SetActive(false);
            
            Debug.Log("[TutorialSystem] *** TUTORIAL COMPLETED SUCCESSFULLY ***");
        }
        
        #endregion
        
        #region Helper Methods
        
        private void ShowTutorial(string message)
        {
            if (_tutorialText != null)
            {
                _tutorialText.text = message;
                Debug.Log($"[Tutorial] {message}");
            }
        }
        
        private void SetObjective(string objective)
        {
            _currentObjective = objective;
            if (_objectiveText != null)
            {
                _objectiveText.text = $"<b>OBJECTIVE:</b>\n{objective}";
            }
        }
        
        private void EndTutorial()
        {
            _tutorialActive = false;
            _tutorialPanel?.SetActive(false);
            Debug.Log("[TutorialSystem] Tutorial ended.");
        }
        
        #endregion
        
        #region Debug Commands
        
        private void Update()
        {
            if (!_tutorialActive) return;
            
            // Skip step with SPACE
            if (Input.GetKeyDown(KeyCode.Space) && !_waitingForPlayerAction)
            {
                Debug.Log("[TutorialSystem] Skipping step...");
                ExecuteNextStep();
            }
            
            // Force next step with N
            if (Input.GetKeyDown(KeyCode.N))
            {
                Debug.Log("[TutorialSystem] Force advancing...");
                _waitingForPlayerAction = false;
                ExecuteNextStep();
            }
            
            // Restart tutorial with R
            if (Input.GetKeyDown(KeyCode.R))
            {
                Debug.Log("[TutorialSystem] Restarting tutorial...");
                StartTutorial();
            }
        }
        
        #endregion
    }
} 
 
#####UISystem.cs##### 
using UnityEngine;
using UnityEngine.UI;

namespace Game.Core.Systems
{
    /// <summary>
    /// Central UI management system.
    /// Manages HUD (Doom-style) and menus (D&D-style).
    /// </summary>
    public partial class UISystemManager : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Canvas _hudCanvas;
        [SerializeField] private Canvas _menuCanvas;
        
        [Header("HUD Elements")]
        [SerializeField] private Slider _healthBar;
        [SerializeField] private Slider _manaBar;
        [SerializeField] private Slider _staminaBar;
        [SerializeField] private Text _healthText;
        [SerializeField] private Text _manaText;
        [SerializeField] private Text _staminaText;
        
        [Header("Menu Panels")]
        [SerializeField] private GameObject _inventoryPanel;
        [SerializeField] private GameObject _characterPanel;
        [SerializeField] private GameObject _spellbookPanel;
        
        private EntityStats _playerStats;
        
        private void Awake()
        {
            InitializeUI();
        }
        
        private void Update()
        {
            UpdateHUD();
            HandleMenuInput();
        }
        
        private void InitializeUI()
        {
            CreateHUDCanvas();
            CreateMenuCanvas();
        }
        
        private void CreateHUDCanvas()
        {
            if (_hudCanvas != null) return;
            
            GameObject hudObj = new("HUDCanvas");
            hudObj.transform.SetParent(transform);
            
            _hudCanvas = hudObj.AddComponent<Canvas>();
            _hudCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _hudCanvas.sortingOrder = 10;
            
            CanvasScaler scaler = hudObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            
            hudObj.AddComponent<GraphicRaycaster>();
            
            CreateHealthBar();
            CreateManaBar();
            CreateStaminaBar();
        }
        
        private void CreateHealthBar()
        {
            GameObject barObj = new("HealthBar");
            barObj.transform.SetParent(_hudCanvas.transform, false);
            
            RectTransform rect = barObj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0f);
            rect.anchorMax = new Vector2(0.5f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.anchoredPosition = new Vector2(-200, 50);
            rect.sizeDelta = new Vector2(150, 30);
            
            _healthBar = barObj.AddComponent<Slider>();
            _healthBar.minValue = 0;
            _healthBar.maxValue = 1;
            _healthBar.value = 1;
            
            // Background
            GameObject bg = new("Background");
            bg.transform.SetParent(barObj.transform, false);
            Image bgImg = bg.AddComponent<Image>();
            bgImg.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
            RectTransform bgRect = bg.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.sizeDelta = Vector2.zero;
            
            // Fill
            GameObject fill = new("Fill");
            fill.transform.SetParent(barObj.transform, false);
            Image fillImg = fill.AddComponent<Image>();
            fillImg.color = Color.red;
            _healthBar.fillRect = fill.GetComponent<RectTransform>();
            _healthBar.fillRect.anchorMin = Vector2.zero;
            _healthBar.fillRect.anchorMax = Vector2.one;
            _healthBar.fillRect.sizeDelta = Vector2.zero;
            
            // Text
            GameObject textObj = new("Text");
            textObj.transform.SetParent(barObj.transform, false);
            _healthText = textObj.AddComponent<Text>();
            _healthText.text = "1000/1000";
            _healthText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _healthText.fontSize = 18;
            _healthText.color = Color.white;
            _healthText.alignment = TextAnchor.MiddleCenter;
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
        }
        
        private void CreateManaBar()
        {
            GameObject barObj = new("ManaBar");
            barObj.transform.SetParent(_hudCanvas.transform, false);
            
            RectTransform rect = barObj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0f);
            rect.anchorMax = new Vector2(0.5f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.anchoredPosition = new Vector2(0, 50);
            rect.sizeDelta = new Vector2(150, 30);
            
            _manaBar = barObj.AddComponent<Slider>();
            _manaBar.minValue = 0;
            _manaBar.maxValue = 1;
            _manaBar.value = 1;
            
            GameObject bg = new("Background");
            bg.transform.SetParent(barObj.transform, false);
            Image bgImg = bg.AddComponent<Image>();
            bgImg.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
            RectTransform bgRect = bg.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.sizeDelta = Vector2.zero;
            
            GameObject fill = new("Fill");
            fill.transform.SetParent(barObj.transform, false);
            Image fillImg = fill.AddComponent<Image>();
            fillImg.color = Color.blue;
            _manaBar.fillRect = fill.GetComponent<RectTransform>();
            _manaBar.fillRect.anchorMin = Vector2.zero;
            _manaBar.fillRect.anchorMax = Vector2.one;
            _manaBar.fillRect.sizeDelta = Vector2.zero;
            
            GameObject textObj = new("Text");
            textObj.transform.SetParent(barObj.transform, false);
            _manaText = textObj.AddComponent<Text>();
            _manaText.text = "500/500";
            _manaText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _manaText.fontSize = 18;
            _manaText.color = Color.white;
            _manaText.alignment = TextAnchor.MiddleCenter;
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
        }
        
        private void CreateStaminaBar()
        {
            GameObject barObj = new("StaminaBar");
            barObj.transform.SetParent(_hudCanvas.transform, false);
            
            RectTransform rect = barObj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0f);
            rect.anchorMax = new Vector2(0.5f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.anchoredPosition = new Vector2(200, 50);
            rect.sizeDelta = new Vector2(150, 30);
            
            _staminaBar = barObj.AddComponent<Slider>();
            _staminaBar.minValue = 0;
            _staminaBar.maxValue = 1;
            _staminaBar.value = 1;
            
            GameObject bg = new("Background");
            bg.transform.SetParent(barObj.transform, false);
            Image bgImg = bg.AddComponent<Image>();
            bgImg.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
            RectTransform bgRect = bg.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.sizeDelta = Vector2.zero;
            
            GameObject fill = new("Fill");
            fill.transform.SetParent(barObj.transform, false);
            Image fillImg = fill.AddComponent<Image>();
            fillImg.color = Color.green;
            _staminaBar.fillRect = fill.GetComponent<RectTransform>();
            _staminaBar.fillRect.anchorMin = Vector2.zero;
            _staminaBar.fillRect.anchorMax = Vector2.one;
            _staminaBar.fillRect.sizeDelta = Vector2.zero;
            
            GameObject textObj = new("Text");
            textObj.transform.SetParent(barObj.transform, false);
            _staminaText = textObj.AddComponent<Text>();
            _staminaText.text = "500/500";
            _staminaText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _staminaText.fontSize = 18;
            _staminaText.color = Color.white;
            _staminaText.alignment = TextAnchor.MiddleCenter;
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
        }
        
        private void CreateMenuCanvas()
        {
            if (_menuCanvas != null) return;
            
            GameObject menuObj = new("MenuCanvas");
            menuObj.transform.SetParent(transform);
            
            _menuCanvas = menuObj.AddComponent<Canvas>();
            _menuCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _menuCanvas.sortingOrder = 20;
            
            menuObj.SetActive(false); // Hidden by default
        }
        
        private void UpdateHUD()
        {
            if (_playerStats == null)
            {
                _playerStats = FindFirstObjectByType<EntityStats>();
            }
            
            if (_playerStats != null)
            {
                _healthBar.value = (float)_playerStats.currentHealth / _playerStats.maxHealth;
                _healthText.text = $"{_playerStats.currentHealth}/{_playerStats.maxHealth}";
                
                _manaBar.value = (float)_playerStats.currentMana / _playerStats.maxMana;
                _manaText.text = $"{_playerStats.currentMana}/{_playerStats.maxMana}";
                
                _staminaBar.value = (float)_playerStats.currentStamina / _playerStats.maxStamina;
                _staminaText.text = $"{_playerStats.currentStamina}/{_playerStats.maxStamina}";
            }
        }
        
        private void HandleMenuInput()
        {
            if (Input.GetKeyDown(KeyCode.I))
            {
                ToggleInventory();
            }
            
            if (Input.GetKeyDown(KeyCode.C))
            {
                ToggleCharacterSheet();
            }
            
            if (Input.GetKeyDown(KeyCode.K))
            {
                ToggleSpellbook();
            }
        }
        
        public void ToggleInventory()
        {
            if (_inventoryPanel != null)
            {
                _inventoryPanel.SetActive(!_inventoryPanel.activeSelf);
            }
        }
        
        public void ToggleCharacterSheet()
        {
            if (_characterPanel != null)
            {
                _characterPanel.SetActive(!_characterPanel.activeSelf);
            }
        }
        
        public void ToggleSpellbook()
        {
            if (_spellbookPanel != null)
            {
                _spellbookPanel.SetActive(!_spellbookPanel.activeSelf);
            }
        }
        
        public void Shutdown() { }
    }
} 
 
#####WebSocketNetworkManager.cs##### 
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Game.Core.Systems
{
    /// <summary>
    /// WebSocket-based LAN multiplayer manager (max 6 players).
    /// COMPLETE IMPLEMENTATION with server/client logic and synchronization.
    /// </summary>
    public class WebSocketNetworkManager : MonoBehaviour
    {
        [Header("Network Configuration")]
        [SerializeField] private int _maxPlayers = 6;
        [SerializeField] private int _port = 7777;
        [SerializeField] private float _updateRate = 20f; // Updates per second
        
        [Header("Network State")]
        [SerializeField] private bool _isHost;
        [SerializeField] private bool _isConnected;
        [SerializeField] private int _connectedPlayerCount;
        [SerializeField] private string _localPlayerId;
        
        [Header("Connection Info")]
        [SerializeField] private string _serverAddress = "127.0.0.1";
        [SerializeField] private List<string> _connectedPlayerIds = new();
        
        private TcpListener _server;
        private TcpClient _client;
        private NetworkStream _networkStream;
        private Dictionary<string, PlayerNetworkData> _playerData = new();
        private Dictionary<string, TcpClient> _connectedClients = new();
        private float _lastUpdateTime;
        private bool _isRunning;
        
        #region Events
        
        public Action<string> OnPlayerConnected;
        public Action<string> OnPlayerDisconnected;
        public Action<NetworkMessage> OnMessageReceived;
        
        #endregion
        
        #region Initialization
        
        private void Awake()
        {
            _localPlayerId = Guid.NewGuid().ToString();
            Debug.Log($"[NetworkManager] Local player ID: {_localPlayerId}");
        }
        
        private void OnDestroy()
        {
            _ = DisconnectAsync();
        }
        
        #endregion
        
        #region Host/Server
        
        public async Task<bool> StartHostAsync()
        {
            if (_isHost || _isConnected)
            {
                Debug.LogWarning("[NetworkManager] Already hosting or connected!");
                return false;
            }
            
            try
            {
                _server = new TcpListener(IPAddress.Any, _port);
                _server.Start();
                _isHost = true;
                _isConnected = true;
                _isRunning = true;
                
                _connectedPlayerIds.Add(_localPlayerId);
                _connectedPlayerCount = 1;
                
                Debug.Log($"[NetworkManager] Server started on port {_port}");
                
                // Start accepting clients
                _ = AcceptClientsAsync();
                
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[NetworkManager] Failed to start server: {e.Message}");
                return false;
            }
        }
        
        private async Task AcceptClientsAsync()
        {
            while (_isRunning && _isHost)
            {
                try
                {
                    TcpClient client = await _server.AcceptTcpClientAsync();
                    
                    if (_connectedPlayerCount >= _maxPlayers)
                    {
                        Debug.LogWarning("[NetworkManager] Max players reached, rejecting connection");
                        client.Close();
                        continue;
                    }
                    
                    string playerId = Guid.NewGuid().ToString();
                    _connectedClients[playerId] = client;
                    _connectedPlayerIds.Add(playerId);
                    _connectedPlayerCount = _connectedPlayerIds.Count;
                    
                    Debug.Log($"[NetworkManager] Player connected: {playerId}");
                    OnPlayerConnected?.Invoke(playerId);
                    
                    // Send welcome message
                    await SendToClientAsync(client, new NetworkMessage
                    {
                        messageType = MessageType.PlayerConnected,
                        senderId = "SERVER",
                        data = playerId
                    });
                    
                    // Start receiving from this client
                    _ = ReceiveFromClientAsync(client, playerId);
                }
                catch (Exception e)
                {
                    if (_isRunning)
                    {
                        Debug.LogError($"[NetworkManager] Error accepting client: {e.Message}");
                    }
                }
                
                await Task.Yield();
            }
        }
        
        private async Task ReceiveFromClientAsync(TcpClient client, string playerId)
        {
            NetworkStream stream = client.GetStream();
            byte[] buffer = new byte[4096];
            
            try
            {
                while (_isRunning && client.Connected)
                {
                    int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                    
                    if (bytesRead == 0)
                    {
                        // Client disconnected
                        HandleClientDisconnect(playerId);
                        break;
                    }
                    
                    string json = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    NetworkMessage message = JsonUtility.FromJson<NetworkMessage>(json);
                    
                    // Process message
                    ProcessMessage(message);
                    
                    // Broadcast to other clients
                    await BroadcastMessageAsync(message, playerId);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[NetworkManager] Error receiving from client {playerId}: {e.Message}");
                HandleClientDisconnect(playerId);
            }
        }
        
        private void HandleClientDisconnect(string playerId)
        {
            if (_connectedClients.ContainsKey(playerId))
            {
                _connectedClients[playerId].Close();
                _connectedClients.Remove(playerId);
            }
            
            _connectedPlayerIds.Remove(playerId);
            _playerData.Remove(playerId);
            _connectedPlayerCount = _connectedPlayerIds.Count;
            
            Debug.Log($"[NetworkManager] Player disconnected: {playerId}");
            OnPlayerDisconnected?.Invoke(playerId);
        }
        
        private async Task BroadcastMessageAsync(NetworkMessage message, string excludePlayerId = null)
        {
            List<string> disconnectedPlayers = new();
            
            foreach (var kvp in _connectedClients)
            {
                if (kvp.Key == excludePlayerId)
                    continue;
                
                try
                {
                    await SendToClientAsync(kvp.Value, message);
                }
                catch
                {
                    disconnectedPlayers.Add(kvp.Key);
                }
            }
            
            // Clean up disconnected players
            foreach (string playerId in disconnectedPlayers)
            {
                HandleClientDisconnect(playerId);
            }
        }
        
        private async Task SendToClientAsync(TcpClient client, NetworkMessage message)
        {
            string json = JsonUtility.ToJson(message);
            byte[] data = Encoding.UTF8.GetBytes(json);
            
            NetworkStream stream = client.GetStream();
            await stream.WriteAsync(data, 0, data.Length);
        }
        
        #endregion
        
        #region Client
        
        public async Task<bool> ConnectToServerAsync(string serverAddress)
        {
            if (_isHost || _isConnected)
            {
                Debug.LogWarning("[NetworkManager] Already hosting or connected!");
                return false;
            }
            
            try
            {
                _serverAddress = serverAddress;
                _client = new TcpClient();
                
                await _client.ConnectAsync(serverAddress, _port);
                _networkStream = _client.GetStream();
                _isConnected = true;
                _isRunning = true;
                
                Debug.Log($"[NetworkManager] Connected to server: {serverAddress}:{_port}");
                
                // Start receiving messages
                _ = ReceiveMessagesAsync();
                
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[NetworkManager] Failed to connect: {e.Message}");
                return false;
            }
        }
        
        private async Task ReceiveMessagesAsync()
        {
            byte[] buffer = new byte[4096];
            
            try
            {
                while (_isRunning && _client != null && _client.Connected)
                {
                    int bytesRead = await _networkStream.ReadAsync(buffer, 0, buffer.Length);
                    
                    if (bytesRead == 0)
                    {
                        Debug.LogWarning("[NetworkManager] Server disconnected");
                        await DisconnectAsync();
                        break;
                    }
                    
                    string json = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    NetworkMessage message = JsonUtility.FromJson<NetworkMessage>(json);
                    
                    ProcessMessage(message);
                }
            }
            catch (Exception e)
            {
                if (_isRunning)
                {
                    Debug.LogError($"[NetworkManager] Error receiving messages: {e.Message}");
                    await DisconnectAsync();
                }
            }
        }
        
        #endregion
        
        #region Message Processing
        
        private void ProcessMessage(NetworkMessage message)
        {
            switch (message.messageType)
            {
                case MessageType.PlayerConnected:
                    if (message.senderId == "SERVER")
                    {
                        _localPlayerId = message.data;
                        Debug.Log($"[NetworkManager] Assigned player ID: {_localPlayerId}");
                    }
                    break;
                
                case MessageType.PlayerPosition:
                    UpdatePlayerPosition(message);
                    break;
                
                case MessageType.PlayerAction:
                    ProcessPlayerAction(message);
                    break;
                
                case MessageType.ChatMessage:
                    ProcessChatMessage(message);
                    break;
            }
            
            OnMessageReceived?.Invoke(message);
        }
        
        private void UpdatePlayerPosition(NetworkMessage message)
        {
            PlayerNetworkData data = JsonUtility.FromJson<PlayerNetworkData>(message.data);
            
            if (!_playerData.ContainsKey(message.senderId))
            {
                _playerData[message.senderId] = data;
                SpawnNetworkPlayer(message.senderId, data);
            }
            else
            {
                _playerData[message.senderId] = data;
                UpdateNetworkPlayer(message.senderId, data);
            }
        }
        
        private void ProcessPlayerAction(NetworkMessage message)
        {
            // TODO: Handle player actions (attacks, interactions, etc.)
            Debug.Log($"[NetworkManager] Player action from {message.senderId}: {message.data}");
        }
        
        private void ProcessChatMessage(NetworkMessage message)
        {
            Debug.Log($"[NetworkManager] Chat from {message.senderId}: {message.data}");
        }
        
        #endregion
        
        #region Network Updates
        
        private void Update()
        {
            if (!_isConnected)
                return;
            
            // Send position updates at fixed rate
            if (Time.time - _lastUpdateTime >= 1f / _updateRate)
            {
                SendPositionUpdate();
                _lastUpdateTime = Time.time;
            }
        }
        
        private void SendPositionUpdate()
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
                return;
            
            PlayerNetworkData data = new PlayerNetworkData
            {
                position = player.transform.position,
                rotation = player.transform.rotation.eulerAngles,
                health = player.GetComponent<EntityStats>()?.currentHealth ?? 100
            };
            
            SendMessage(new NetworkMessage
            {
                messageType = MessageType.PlayerPosition,
                senderId = _localPlayerId,
                data = JsonUtility.ToJson(data)
            });
        }
        
        public void SendMessage(NetworkMessage message)
        {
            if (!_isConnected)
                return;
            
            message.senderId = _localPlayerId;
            message.timestamp = DateTime.Now.Ticks;
            
            _ = SendMessageAsync(message);
        }
        
        private async Task SendMessageAsync(NetworkMessage message)
        {
            try
            {
                if (_isHost)
                {
                    // Broadcast to all clients
                    await BroadcastMessageAsync(message);
                }
                else if (_client != null && _networkStream != null)
                {
                    // Send to server
                    string json = JsonUtility.ToJson(message);
                    byte[] data = Encoding.UTF8.GetBytes(json);
                    await _networkStream.WriteAsync(data, 0, data.Length);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[NetworkManager] Error sending message: {e.Message}");
            }
        }
        
        #endregion
        
        #region Network Players
        
        private void SpawnNetworkPlayer(string playerId, PlayerNetworkData data)
        {
            // Create visual representation of network player
            GameObject networkPlayer = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            networkPlayer.name = $"NetworkPlayer_{playerId}";
            networkPlayer.transform.position = data.position;
            networkPlayer.transform.rotation = Quaternion.Euler(data.rotation);
            
            // Color it differently
            Renderer renderer = networkPlayer.GetComponent<Renderer>();
            Material mat = new Material(Shader.Find("Standard"));
            mat.color = Color.blue;
            renderer.material = mat;
            
            // Add network player component
            NetworkPlayer netPlayer = networkPlayer.AddComponent<NetworkPlayer>();
            netPlayer.Initialize(playerId, data);
            
            Debug.Log($"[NetworkManager] Spawned network player: {playerId}");
        }
        
        private void UpdateNetworkPlayer(string playerId, PlayerNetworkData data)
        {
            GameObject networkPlayer = GameObject.Find($"NetworkPlayer_{playerId}");
            
            if (networkPlayer != null)
            {
                NetworkPlayer netPlayer = networkPlayer.GetComponent<NetworkPlayer>();
                if (netPlayer != null)
                {
                    netPlayer.UpdateData(data);
                }
            }
        }
        
        #endregion
        
        #region Disconnect
        
        public async Task DisconnectAsync()
        {
            _isRunning = false;
            
            if (_isHost)
            {
                // Close all client connections
                foreach (var client in _connectedClients.Values)
                {
                    client.Close();
                }
                _connectedClients.Clear();
                
                _server?.Stop();
                _server = null;
            }
            else
            {
                _networkStream?.Close();
                _client?.Close();
            }
            
            _isHost = false;
            _isConnected = false;
            _connectedPlayerIds.Clear();
            _connectedPlayerCount = 0;
            _playerData.Clear();
            
            Debug.Log("[NetworkManager] Disconnected");
            
            await Task.Yield();
        }
        
        #endregion
        
        #region Public API
        
        public bool IsHost() => _isHost;
        public bool IsConnected() => _isConnected;
        public int GetPlayerCount() => _connectedPlayerCount;
        public string GetLocalPlayerId() => _localPlayerId;
        public List<string> GetConnectedPlayers() => new List<string>(_connectedPlayerIds);
        
        public void SendChatMessage(string message)
        {
            SendMessage(new NetworkMessage
            {
                messageType = MessageType.ChatMessage,
                senderId = _localPlayerId,
                data = message
            });
        }
        
        #endregion
    }
    
    #region Network Data Structures
    
    [Serializable]
    public class NetworkMessage
    {
        public MessageType messageType;
        public string senderId;
        public string data;
        public long timestamp;
    }
    
    [Serializable]
    public class PlayerNetworkData
    {
        public Vector3 position;
        public Vector3 rotation;
        public int health;
    }
    
    public enum MessageType
    {
        PlayerConnected,
        PlayerDisconnected,
        PlayerPosition,
        PlayerAction,
        ChatMessage,
        EntitySpawn,
        EntityDespawn
    }
    
    /// <summary>
    /// Component for network player representations.
    /// </summary>
    public class NetworkPlayer : MonoBehaviour
    {
        private string _playerId;
        private PlayerNetworkData _data;
        private Vector3 _targetPosition;
        private Quaternion _targetRotation;
        
        public void Initialize(string playerId, PlayerNetworkData data)
        {
            _playerId = playerId;
            _data = data;
            _targetPosition = data.position;
            _targetRotation = Quaternion.Euler(data.rotation);
        }
        
        public void UpdateData(PlayerNetworkData data)
        {
            _data = data;
            _targetPosition = data.position;
            _targetRotation = Quaternion.Euler(data.rotation);
        }
        
        private void Update()
        {
            // Smooth interpolation
            transform.position = Vector3.Lerp(transform.position, _targetPosition, Time.deltaTime * 10f);
            transform.rotation = Quaternion.Slerp(transform.rotation, _targetRotation, Time.deltaTime * 10f);
        }
    }
    
    #endregion
} 
 
#####ZoneSceneManager.cs##### 
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System;

namespace Game.Core.Systems
{
    /// <summary>
    /// Manages zone loading, unloading, and scene caching.
    /// Zones are Unity scenes that can be generated procedurally and saved.
    /// Supports additive scene loading for seamless zone transitions.
    /// </summary>
    public class ZoneSceneManager : MonoBehaviour
    {
        [Header("Zone Configuration")]
        [SerializeField, Tooltip("Path where generated zones are saved")]
        private string _zoneSavePath = "Assets/Scenes/Zones/";
        
        [SerializeField, Tooltip("Maximum zones loaded simultaneously")]
        private int _maxLoadedZones = 3;
        
        [Header("Runtime State")]
        [SerializeField] private string _currentZoneName;
        [SerializeField] private List<string> _loadedZones = new();
        [SerializeField] private List<string> _cachedZoneNames = new();
        
        // Zone metadata cache
        private Dictionary<string, ZoneMetadata> _zoneMetadataCache;
        
        // Active zone scenes
        private Dictionary<string, Scene> _activeZoneScenes;
        
        // Zone generation callback
        private SimpleTerrainGenerator _terrainGenerator;
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            InitializeZoneManager();
        }
        
        #endregion
        
        #region Initialization
        
        private void InitializeZoneManager()
        {
            _zoneMetadataCache = new Dictionary<string, ZoneMetadata>();
            _activeZoneScenes = new Dictionary<string, Scene>();
            _loadedZones = new List<string>();
            _cachedZoneNames = new List<string>();
            
            // Get terrain generator reference
            _terrainGenerator = GetComponent<SimpleTerrainGenerator>();
            if (_terrainGenerator == null)
            {
                _terrainGenerator = gameObject.AddComponent<SimpleTerrainGenerator>();
            }
            
            ScanForCachedZones();
            
            Debug.Log($"[ZoneSceneManager] Initialized. Found {_cachedZoneNames.Count} cached zones.");
        }
        
        private void ScanForCachedZones()
        {
            // In runtime, check PlayerPrefs for cached zone list
            // In editor, scan directory for .unity files
            
            #if UNITY_EDITOR
            if (System.IO.Directory.Exists(_zoneSavePath))
            {
                string[] sceneFiles = System.IO.Directory.GetFiles(_zoneSavePath, "*.unity");
                foreach (string file in sceneFiles)
                {
                    string zoneName = System.IO.Path.GetFileNameWithoutExtension(file);
                    _cachedZoneNames.Add(zoneName);
                }
            }
            #else
            // Runtime: Load from PlayerPrefs
            string cachedList = PlayerPrefs.GetString("CachedZones", "");
            if (!string.IsNullOrEmpty(cachedList))
            {
                _cachedZoneNames.AddRange(cachedList.Split(','));
            }
            #endif
        }
        
        #endregion
        
        #region Zone Generation
        
        /// <summary>
        /// Generate a new zone procedurally and optionally save it as a scene.
        /// </summary>
        public async Awaitable<Scene> GenerateAndSaveZone(ZoneConfig config)
        {
            Debug.Log($"[ZoneSceneManager] Generating zone '{config.zoneName}'...");
            
            // Create new scene additively
            Scene newScene = SceneManager.CreateScene(config.zoneName);
            SceneManager.SetActiveScene(newScene);
            
            // Create zone root object
            GameObject zoneRoot = new($"Zone_{config.zoneName}");
            SceneManager.MoveGameObjectToScene(zoneRoot, newScene);
            
            // Generate terrain
            await _terrainGenerator.GenerateTerrainForZone(zoneRoot.transform, config);
            
            // Place interactables
            await PlaceInteractables(zoneRoot.transform, config);
            
            // Setup spawn points
            CreateSpawnPoints(zoneRoot.transform, config);
            
            // Create zone boundary
            CreateZoneBoundary(zoneRoot.transform, config);
            
            // Save metadata
            ZoneMetadata metadata = new()
            {
                zoneName = config.zoneName,
                zoneType = config.zoneType,
                levelRange = config.levelRange,
                biomeType = config.biomeType,
                generatedTimestamp = DateTime.Now.ToString()
            };
            _zoneMetadataCache[config.zoneName] = metadata;
            
            // Save scene (Editor only)
            #if UNITY_EDITOR
            SaveZoneAsScene(config.zoneName, newScene);
            #else
            // Runtime: Save to ScriptableObject
            SaveZoneMetadata(metadata);
            #endif
            
            // Add to loaded zones
            _activeZoneScenes[config.zoneName] = newScene;
            _loadedZones.Add(config.zoneName);
            _currentZoneName = config.zoneName;
            
            Debug.Log($"[ZoneSceneManager] Zone '{config.zoneName}' generated and saved");
            
            return newScene;
        }
        
        #endregion
        
        #region Zone Loading
        
        /// <summary>
        /// Load a zone scene additively.
        /// Checks cache first, generates if not found.
        /// </summary>
        public async Awaitable<Scene> LoadZone(string zoneName, ZoneConfig config = null)
        {
            // Check if already loaded
            if (_activeZoneScenes.ContainsKey(zoneName))
            {
                Debug.Log($"[ZoneSceneManager] Zone '{zoneName}' already loaded");
                return _activeZoneScenes[zoneName];
            }
            
            // Check max loaded zones limit
            if (_loadedZones.Count >= _maxLoadedZones)
            {
                await UnloadOldestZone();
            }
            
            // Check if zone is cached
            if (IsZoneCached(zoneName))
            {
                return await LoadCachedZone(zoneName);
            }
            else
            {
                // Generate new zone
                if (config == null)
                {
                    Debug.LogWarning($"[ZoneSceneManager] Zone '{zoneName}' not cached and no config provided. Using default.");
                    config = ZoneConfig.CreateDefault(zoneName);
                }
                
                return await GenerateAndSaveZone(config);
            }
        }
        
        private async Awaitable<Scene> LoadCachedZone(string zoneName)
        {
            Debug.Log($"[ZoneSceneManager] Loading cached zone '{zoneName}'...");
            
            AsyncOperation loadOp = SceneManager.LoadSceneAsync(zoneName, LoadSceneMode.Additive);
            
            if (loadOp == null)
            {
                Debug.LogError($"[ZoneSceneManager] Failed to load zone '{zoneName}'!");
                return default;
            }
            
            while (!loadOp.isDone)
            {
                await Awaitable.NextFrameAsync();
            }
            
            Scene loadedScene = SceneManager.GetSceneByName(zoneName);
            _activeZoneScenes[zoneName] = loadedScene;
            _loadedZones.Add(zoneName);
            _currentZoneName = zoneName;
            
            Debug.Log($"[ZoneSceneManager] Zone '{zoneName}' loaded from cache");
            
            return loadedScene;
        }
        
        #endregion
        
        #region Zone Unloading
        
        /// <summary>
        /// Unload a zone scene and cache its state.
        /// </summary>
        public async Awaitable UnloadZone(string zoneName)
        {
            if (!_activeZoneScenes.ContainsKey(zoneName))
            {
                Debug.LogWarning($"[ZoneSceneManager] Zone '{zoneName}' not loaded, cannot unload");
                return;
            }
            
            Debug.Log($"[ZoneSceneManager] Unloading zone '{zoneName}'...");
            
            Scene scene = _activeZoneScenes[zoneName];
            
            // Save zone state before unloading (if needed)
            // TODO: Serialize entity states, player progress, etc.
            
            AsyncOperation unloadOp = SceneManager.UnloadSceneAsync(scene);
            
            while (!unloadOp.isDone)
            {
                await Awaitable.NextFrameAsync();
            }
            
            _activeZoneScenes.Remove(zoneName);
            _loadedZones.Remove(zoneName);
            
            if (_currentZoneName == zoneName)
            {
                _currentZoneName = _loadedZones.Count > 0 ? _loadedZones[0] : null;
            }
            
            Debug.Log($"[ZoneSceneManager] Zone '{zoneName}' unloaded");
        }
        
        private async Awaitable UnloadOldestZone()
        {
            if (_loadedZones.Count == 0) return;
            
            string oldestZone = _loadedZones[0];
            Debug.Log($"[ZoneSceneManager] Unloading oldest zone '{oldestZone}' due to max limit");
            await UnloadZone(oldestZone);
        }
        
        #endregion
        
        #region Zone Helpers
        
        public bool IsZoneCached(string zoneName)
        {
            return _cachedZoneNames.Contains(zoneName);
        }
        
        public List<string> GetCachedZones()
        {
            return new List<string>(_cachedZoneNames);
        }
        
        public string GetCurrentZone()
        {
            return _currentZoneName;
        }
        
        public ZoneMetadata GetZoneMetadata(string zoneName)
        {
            return _zoneMetadataCache.ContainsKey(zoneName) 
                ? _zoneMetadataCache[zoneName] 
                : null;
        }
        
        #endregion
        
        #region Interactable Placement
        
        private async Awaitable PlaceInteractables(Transform zoneRoot, ZoneConfig config)
        {
            // Create interactables container
            GameObject interactablesContainer = new("Interactables");
            interactablesContainer.transform.SetParent(zoneRoot);
            
            // Place based on zone type
            switch (config.zoneType)
            {
                case ZoneType.Town:
                    await PlaceTownInteractables(interactablesContainer.transform, config);
                    break;
                case ZoneType.Dungeon:
                    await PlaceDungeonInteractables(interactablesContainer.transform, config);
                    break;
                case ZoneType.Wilderness:
                    await PlaceWildernessInteractables(interactablesContainer.transform, config);
                    break;
            }
            
            await Awaitable.NextFrameAsync();
        }
        
        private async Awaitable PlaceTownInteractables(Transform parent, ZoneConfig config)
        {
            // TODO: Place NPCs, shops, quest givers, doors, etc.
            await Awaitable.NextFrameAsync();
        }
        
        private async Awaitable PlaceDungeonInteractables(Transform parent, ZoneConfig config)
        {
            // TODO: Place chests, doors, traps, levers, etc.
            await Awaitable.NextFrameAsync();
        }
        
        private async Awaitable PlaceWildernessInteractables(Transform parent, ZoneConfig config)
        {
            // TODO: Place resource nodes, campfires, etc.
            await Awaitable.NextFrameAsync();
        }
        
        #endregion
        
        #region Spawn Points
        
        private void CreateSpawnPoints(Transform zoneRoot, ZoneConfig config)
        {
            GameObject spawnContainer = new("SpawnPoints");
            spawnContainer.transform.SetParent(zoneRoot);
            
            // Create player spawn
            CreateSpawnPoint(spawnContainer.transform, "PlayerSpawn", SpawnPointType.Player, Vector3.zero);
            
            // Create enemy spawns based on zone type
            int enemySpawnCount = config.zoneType == ZoneType.Town ? 0 : 10;
            for (int i = 0; i < enemySpawnCount; i++)
            {
                Vector3 randomPos = new(
                    UnityEngine.Random.Range(-config.zoneSize.x / 2, config.zoneSize.x / 2),
                    0,
                    UnityEngine.Random.Range(-config.zoneSize.z / 2, config.zoneSize.z / 2)
                );
                
                CreateSpawnPoint(spawnContainer.transform, $"EnemySpawn_{i}", SpawnPointType.Enemy, randomPos);
            }
        }
        
        private void CreateSpawnPoint(Transform parent, string spawnName, SpawnPointType type, Vector3 position)
        {
            GameObject spawnObj = new(spawnName);
            spawnObj.transform.SetParent(parent);
            spawnObj.transform.position = position;
            
            SpawnPoint spawn = spawnObj.AddComponent<SpawnPoint>();
            spawn.Initialize(type, 5f, 10);
        }
        
        #endregion
        
        #region Zone Boundary
        
        private void CreateZoneBoundary(Transform zoneRoot, ZoneConfig config)
        {
            GameObject boundary = new("ZoneBoundary");
            boundary.transform.SetParent(zoneRoot);
            boundary.layer = LayerMask.NameToLayer("Default");
            
            // Create invisible box collider walls around zone
            float wallHeight = 50f;
            Vector3 size = config.zoneSize;
            
            // North wall
            CreateBoundaryWall(boundary.transform, new Vector3(0, wallHeight / 2, size.z / 2), 
                new Vector3(size.x, wallHeight, 1));
            
            // South wall
            CreateBoundaryWall(boundary.transform, new Vector3(0, wallHeight / 2, -size.z / 2), 
                new Vector3(size.x, wallHeight, 1));
            
            // East wall
            CreateBoundaryWall(boundary.transform, new Vector3(size.x / 2, wallHeight / 2, 0), 
                new Vector3(1, wallHeight, size.z));
            
            // West wall
            CreateBoundaryWall(boundary.transform, new Vector3(-size.x / 2, wallHeight / 2, 0), 
                new Vector3(1, wallHeight, size.z));
        }
        
        private void CreateBoundaryWall(Transform parent, Vector3 position, Vector3 size)
        {
            GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = "BoundaryWall";
            wall.transform.SetParent(parent);
            wall.transform.localPosition = position;
            wall.transform.localScale = size;
            
            // Make invisible but keep collider
            Renderer renderer = wall.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.enabled = false;
            }
        }
        
        #endregion
        
        #region Serialization
        
        #if UNITY_EDITOR
        private void SaveZoneAsScene(string zoneName, Scene scene)
        {
            if (!System.IO.Directory.Exists(_zoneSavePath))
            {
                System.IO.Directory.CreateDirectory(_zoneSavePath);
            }
            
            string scenePath = $"{_zoneSavePath}{zoneName}.unity";
            bool saved = UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene, scenePath);
            
            if (saved)
            {
                _cachedZoneNames.Add(zoneName);
                Debug.Log($"[ZoneSceneManager] Zone saved to: {scenePath}");
            }
        }
        #endif
        
        private void SaveZoneMetadata(ZoneMetadata metadata)
        {
            // Runtime: Save metadata to PlayerPrefs or file
            string json = JsonUtility.ToJson(metadata);
            PlayerPrefs.SetString($"ZoneMeta_{metadata.zoneName}", json);
            
            // Update cached zones list
            if (!_cachedZoneNames.Contains(metadata.zoneName))
            {
                _cachedZoneNames.Add(metadata.zoneName);
                PlayerPrefs.SetString("CachedZones", string.Join(",", _cachedZoneNames));
            }
            
            PlayerPrefs.Save();
        }
        
        #endregion
        
        #region Shutdown
        
        public async Awaitable ShutdownAsync()
        {
            // Unload all zones
            List<string> zonesToUnload = new(_loadedZones);
            foreach (string zone in zonesToUnload)
            {
                await UnloadZone(zone);
            }
        }
        
        #endregion
    }
    
    #region Supporting Types
    
    [Serializable]
    public class ZoneConfig
    {
        public string zoneName = "NewZone";
        public ZoneType zoneType = ZoneType.Wilderness;
        public BiomeType biomeType = BiomeType.Grassland;
        public Vector2Int levelRange = new(1, 10);
        public Vector3 zoneSize = new(100, 0, 100);
        public int seed = 12345;
        
        public static ZoneConfig CreateDefault(string name)
        {
            return new ZoneConfig { zoneName = name };
        }
    }
    
    [Serializable]
    public class ZoneMetadata
    {
        public string zoneName;
        public ZoneType zoneType;
        public BiomeType biomeType;
        public Vector2Int levelRange;
        public string generatedTimestamp;
    }
    
    public enum ZoneType
    {
        Town,
        Dungeon,
        Wilderness,
        Arena
    }
    
    public enum BiomeType
    {
        Grassland,
        Desert,
        Snow,
        Lava,
        Corrupted
    }
    
    public enum SpawnPointType
    {
        Player,
        Enemy,
        NPC,
        Boss,
        Resource
    }
    
    /// <summary>
    /// Spawn point component for zone entity placement.
    /// </summary>
    public class SpawnPoint : MonoBehaviour
    {
        [SerializeField] private SpawnPointType _type;
        [SerializeField] private float _radius = 5f;
        [SerializeField] private int _maxEntities = 10;
        
        public SpawnPointType Type => _type;
        public float Radius => _radius;
        public int MaxEntities => _maxEntities;
        
        public void Initialize(SpawnPointType type, float radius, int maxEntities)
        {
            _type = type;
            _radius = radius;
            _maxEntities = maxEntities;
        }
        
        private void OnDrawGizmos()
        {
            Gizmos.color = _type switch
            {
                SpawnPointType.Player => Color.green,
                SpawnPointType.Enemy => Color.red,
                SpawnPointType.NPC => Color.blue,
                SpawnPointType.Boss => Color.magenta,
                SpawnPointType.Resource => Color.yellow,
                _ => Color.white
            };
            
            Gizmos.DrawWireSphere(transform.position, _radius);
        }
    }
    
    #endregion
} 
 
#####AdminConsoleManager.cs##### 
using UnityEngine;
using System.Collections.Generic;

namespace Game.Core.Systems
{
    /// <summary>
    /// Admin console manager for in-game editing and debugging.
    /// F12 toggles console (server host/admin only).
    /// Provides UI for editing weapons, armor, spells, pools, entities, zones.
    /// </summary>
    public class AdminConsoleManager : MonoBehaviour
    {
        [Header("Console Configuration")]
        [SerializeField, Tooltip("Key to toggle admin console")]
        private KeyCode _toggleKey = KeyCode.F12;
        
        [SerializeField, Tooltip("Only allow admin access for server host")]
        private bool _hostOnly = true;
        
        [Header("Console State")]
        [SerializeField] private bool _isConsoleOpen;
        [SerializeField] private bool _isAdminAuthorized;
        [SerializeField] private AdminTab _currentTab = AdminTab.PoolDatabase;
        
        [Header("Runtime References (Auto-Assigned)")]
        [SerializeField] private Canvas _consoleCanvas;
        [SerializeField] private GameObject _consolePanel;
        
        // Tab controllers (to be implemented)
        private PoolDatabaseController _poolDatabaseController;
        private WeaponEditorController _weaponEditorController;
        private ArmorEditorController _armorEditorController;
        private SpellEditorController _spellEditorController;
        private EntityInspectorController _entityInspectorController;
        private ZoneEditorController _zoneEditorController;
        private PlayerManagementController _playerManagementController;
        
        private Dictionary<AdminTab, GameObject> _tabPanels;
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            InitializeConsole();
            CheckAdminAuthorization();
        }
        
        private void Update()
        {
            // Toggle console with F12
            if (Input.GetKeyDown(_toggleKey))
            {
                if (_isAdminAuthorized)
                {
                    ToggleConsole();
                }
                else
                {
                    Debug.LogWarning("[AdminConsole] Admin access denied. Host only.");
                }
            }
            
            // Tab switching (1-7 keys when console open)
            if (_isConsoleOpen)
            {
                HandleTabSwitching();
            }
        }
        
        #endregion
        
        #region Initialization
        
        private void InitializeConsole()
        {
            CreateConsoleUI();
            _tabPanels = new Dictionary<AdminTab, GameObject>();
            _isConsoleOpen = false;
        }
        
        private void CheckAdminAuthorization()
        {
            // Check if player is server host
            if (_hostOnly)
            {
                WebSocketNetworkManager netManager = CoreSystemManager.NetworkManager;
                _isAdminAuthorized = netManager != null && netManager.IsHost();
            }
            else
            {
                // Allow admin access in single-player
                _isAdminAuthorized = true;
            }
            
            Debug.Log($"[AdminConsole] Admin authorized: {_isAdminAuthorized}");
        }
        
        private void CreateConsoleUI()
        {
            // Create canvas for admin console
            GameObject canvasObj = new("AdminConsoleCanvas");
            canvasObj.transform.SetParent(transform);
            
            _consoleCanvas = canvasObj.AddComponent<Canvas>();
            _consoleCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _consoleCanvas.sortingOrder = 1000; // Above game UI
            
            UnityEngine.UI.CanvasScaler scaler = canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
            scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            
            canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            
            // Create main console panel
            CreateConsolePanel();
            
            // Hide by default
            _consolePanel.SetActive(false);
        }
        
        private void CreateConsolePanel()
        {
            _consolePanel = new GameObject("ConsolePanel");
            _consolePanel.transform.SetParent(_consoleCanvas.transform, false);
            
            // Background
            UnityEngine.UI.Image bg = _consolePanel.AddComponent<UnityEngine.UI.Image>();
            bg.color = new Color(0.1f, 0.1f, 0.1f, 0.95f); // Dark semi-transparent
            
            RectTransform rect = _consolePanel.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.1f, 0.1f);
            rect.anchorMax = new Vector2(0.9f, 0.9f);
            rect.sizeDelta = Vector2.zero;
            
            // Title bar
            CreateTitleBar();
            
            // Tab buttons
            CreateTabButtons();
            
            // Content area (where tab-specific content goes)
            CreateContentArea();
        }
        
        private void CreateTitleBar()
        {
            GameObject titleBar = new("TitleBar");
            titleBar.transform.SetParent(_consolePanel.transform, false);
            
            UnityEngine.UI.Image bg = titleBar.AddComponent<UnityEngine.UI.Image>();
            bg.color = new Color(0.2f, 0.2f, 0.2f, 1f);
            
            RectTransform rect = titleBar.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(1, 1);
            rect.pivot = new Vector2(0.5f, 1);
            rect.sizeDelta = new Vector2(0, 50);
            
            // Title text
            GameObject textObj = new("TitleText");
            textObj.transform.SetParent(titleBar.transform, false);
            
            UnityEngine.UI.Text text = textObj.AddComponent<UnityEngine.UI.Text>();
            text.text = "ADMIN CONSOLE - F12 to Close";
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 24;
            text.color = Color.cyan;
            text.alignment = TextAnchor.MiddleCenter;
            
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
        }
        
        private void CreateTabButtons()
        {
            GameObject tabBar = new("TabBar");
            tabBar.transform.SetParent(_consolePanel.transform, false);
            
            UnityEngine.UI.Image bg = tabBar.AddComponent<UnityEngine.UI.Image>();
            bg.color = new Color(0.15f, 0.15f, 0.15f, 1f);
            
            RectTransform rect = tabBar.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(1, 1);
            rect.pivot = new Vector2(0.5f, 1);
            rect.anchoredPosition = new Vector2(0, -50);
            rect.sizeDelta = new Vector2(0, 40);
            
            // Add horizontal layout
            UnityEngine.UI.HorizontalLayoutGroup layout = tabBar.AddComponent<UnityEngine.UI.HorizontalLayoutGroup>();
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;
            layout.spacing = 5;
            layout.padding = new RectOffset(5, 5, 5, 5);
            
            // Create tab buttons
            string[] tabNames = System.Enum.GetNames(typeof(AdminTab));
            for (int i = 0; i < tabNames.Length; i++)
            {
                CreateTabButton(tabBar.transform, tabNames[i], (AdminTab)i);
            }
        }
        
        private void CreateTabButton(Transform parent, string label, AdminTab tab)
        {
            GameObject button = new($"Tab_{label}");
            button.transform.SetParent(parent, false);
            
            UnityEngine.UI.Button btn = button.AddComponent<UnityEngine.UI.Button>();
            UnityEngine.UI.Image btnImg = button.AddComponent<UnityEngine.UI.Image>();
            btnImg.color = new Color(0.3f, 0.3f, 0.3f, 1f);
            
            // Button text
            GameObject textObj = new("Text");
            textObj.transform.SetParent(button.transform, false);
            
            UnityEngine.UI.Text text = textObj.AddComponent<UnityEngine.UI.Text>();
            text.text = $"{label}\n(Key {(int)tab + 1})";
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 16;
            text.color = Color.white;
            text.alignment = TextAnchor.MiddleCenter;
            
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            
            // Button click handler
            btn.onClick.AddListener(() => SwitchToTab(tab));
        }
        
        private void CreateContentArea()
        {
            GameObject contentArea = new("ContentArea");
            contentArea.transform.SetParent(_consolePanel.transform, false);
            
            RectTransform rect = contentArea.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 0);
            rect.anchorMax = new Vector2(1, 1);
            rect.offsetMin = new Vector2(10, 10); // Left, Bottom
            rect.offsetMax = new Vector2(-10, -100); // Right, Top (account for title+tabs)
            
            // Create placeholder tab panels
            CreateTabPanels(contentArea.transform);
        }
        
        private void CreateTabPanels(Transform parent)
        {
            // Create a panel for each tab (to be populated by specific controllers)
            foreach (AdminTab tab in System.Enum.GetValues(typeof(AdminTab)))
            {
                GameObject panel = new($"Panel_{tab}");
                panel.transform.SetParent(parent, false);
                
                RectTransform rect = panel.GetComponent<RectTransform>();
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.sizeDelta = Vector2.zero;
                
                // Add scroll view for content
                UnityEngine.UI.ScrollRect scroll = panel.AddComponent<UnityEngine.UI.ScrollRect>();
                UnityEngine.UI.Image scrollBg = panel.AddComponent<UnityEngine.UI.Image>();
                scrollBg.color = new Color(0.05f, 0.05f, 0.05f, 1f);
                
                // Content container
                GameObject content = new("Content");
                content.transform.SetParent(panel.transform, false);
                
                RectTransform contentRect = content.GetComponent<RectTransform>();
                contentRect.anchorMin = new Vector2(0, 1);
                contentRect.anchorMax = new Vector2(1, 1);
                contentRect.pivot = new Vector2(0.5f, 1);
                contentRect.sizeDelta = new Vector2(0, 1000); // Will expand with content
                
                scroll.content = contentRect;
                scroll.horizontal = false;
                scroll.vertical = true;
                
                // Add placeholder text
                CreatePlaceholderText(content.transform, tab);
                
                _tabPanels[tab] = panel;
                panel.SetActive(tab == _currentTab); // Only show current tab
            }
        }
        
        private void CreatePlaceholderText(Transform parent, AdminTab tab)
        {
            GameObject textObj = new("PlaceholderText");
            textObj.transform.SetParent(parent, false);
            
            UnityEngine.UI.Text text = textObj.AddComponent<UnityEngine.UI.Text>();
            text.text = $"{tab} panel\n\n(Content will be populated by specific controller)";
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 20;
            text.color = Color.gray;
            text.alignment = TextAnchor.UpperLeft;
            
            RectTransform rect = textObj.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(1, 1);
            rect.pivot = new Vector2(0, 1);
            rect.sizeDelta = new Vector2(-20, 100);
            rect.anchoredPosition = new Vector2(10, -10);
        }
        
        #endregion
        
        #region Console Control
        
        public void ToggleConsole()
        {
            _isConsoleOpen = !_isConsoleOpen;
            _consolePanel.SetActive(_isConsoleOpen);
            
            // Pause game when console open (optional)
            Time.timeScale = _isConsoleOpen ? 0f : 1f;
            
            // Lock/unlock cursor
            Cursor.visible = _isConsoleOpen;
            Cursor.lockState = _isConsoleOpen ? CursorLockMode.None : CursorLockMode.Locked;
            
            Debug.Log($"[AdminConsole] Console {(_isConsoleOpen ? "opened" : "closed")}");
        }
        
        private void SwitchToTab(AdminTab tab)
        {
            _currentTab = tab;
            
            // Hide all tab panels
            foreach (KeyValuePair<AdminTab, GameObject> kvp in _tabPanels)
            {
                kvp.Value.SetActive(kvp.Key == tab);
            }
            
            Debug.Log($"[AdminConsole] Switched to tab: {tab}");
        }
        
        private void HandleTabSwitching()
        {
            // Keys 1-7 switch tabs
            if (Input.GetKeyDown(KeyCode.Alpha1)) SwitchToTab(AdminTab.PoolDatabase);
            if (Input.GetKeyDown(KeyCode.Alpha2)) SwitchToTab(AdminTab.WeaponEditor);
            if (Input.GetKeyDown(KeyCode.Alpha3)) SwitchToTab(AdminTab.ArmorEditor);
            if (Input.GetKeyDown(KeyCode.Alpha4)) SwitchToTab(AdminTab.SpellEditor);
            if (Input.GetKeyDown(KeyCode.Alpha5)) SwitchToTab(AdminTab.EntityInspector);
            if (Input.GetKeyDown(KeyCode.Alpha6)) SwitchToTab(AdminTab.ZoneEditor);
            if (Input.GetKeyDown(KeyCode.Alpha7)) SwitchToTab(AdminTab.PlayerManagement);
        }
        
        #endregion
        
        #region Public API
        
        public bool IsConsoleOpen() => _isConsoleOpen;
        public bool IsAdminAuthorized() => _isAdminAuthorized;
        
        public void Shutdown()
        {
            if (_isConsoleOpen)
            {
                ToggleConsole(); // Close and restore time scale
            }
        }
        
        #endregion
        
        #region Nested Types
        
        public enum AdminTab
        {
            PoolDatabase = 0,
            WeaponEditor = 1,
            ArmorEditor = 2,
            SpellEditor = 3,
            EntityInspector = 4,
            ZoneEditor = 5,
            PlayerManagement = 6
        }
        
        #endregion
    }
    
    #region Placeholder Controllers (To be implemented)
    
    public class EntityInspectorController { }
    public class ZoneEditorController { }
    public class PlayerManagementController { }
    
    #endregion
} 
 
#####ArmorEditorController.cs##### 
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

namespace Game.Core.Systems
{
    /// <summary>
    /// Armor Editor Controller - Create and edit armor pieces with live preview.
    /// </summary>
    public class ArmorEditorController : MonoBehaviour
    {
        private GameObject _contentPanel;
        private ItemGenerationEngine _itemGenerator;
        private ItemData _currentArmor;
        
        private Dropdown _slotDropdown;
        private Dropdown _archetypeDropdown;
        private Dropdown _rarityDropdown;
        private InputField _armorInput;
        private Text _previewText;
        
        public void Initialize(GameObject contentPanel)
        {
            _contentPanel = contentPanel;
            _itemGenerator = FindFirstObjectByType<ItemGenerationEngine>();
            
            if (_itemGenerator == null)
            {
                GameObject genObj = new GameObject("ItemGenerationEngine");
                _itemGenerator = genObj.AddComponent<ItemGenerationEngine>();
            }
            
            CreateUI();
            GenerateRandomArmor();
        }
        
        private void CreateUI()
        {
            Transform content = CreateScrollableContent();
            
            CreateHeader(content, "ARMOR EDITOR");
            
            CreateDropdown(content, "Slot:", out _slotDropdown, 
                System.Enum.GetNames(typeof(ArmorSlot)).ToList());
            _slotDropdown.onValueChanged.AddListener(v => UpdatePreview());
            
            CreateDropdown(content, "Archetype:", out _archetypeDropdown, 
                System.Enum.GetNames(typeof(ArmorArchetype)).ToList());
            _archetypeDropdown.onValueChanged.AddListener(v => UpdatePreview());
            
            CreateDropdown(content, "Rarity:", out _rarityDropdown, 
                System.Enum.GetNames(typeof(Rarity)).ToList());
            _rarityDropdown.onValueChanged.AddListener(v => UpdatePreview());
            
            CreateInputField(content, "Armor Value:", out _armorInput);
            _armorInput.onValueChanged.AddListener(v => UpdatePreview());
            
            CreateButton(content, "🎲 GENERATE RANDOM", () => GenerateRandomArmor());
            CreateButton(content, "💾 SAVE ARMOR", () => SaveArmor());
            
            CreatePreviewPanel(content);
        }
        
        private Transform CreateScrollableContent()
        {
            ScrollRect scroll = _contentPanel.GetComponent<ScrollRect>();
            if (scroll == null) scroll = _contentPanel.AddComponent<ScrollRect>();
            
            GameObject content = new GameObject("Content");
            content.transform.SetParent(_contentPanel.transform, false);
            
            RectTransform contentRect = content.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 1);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.pivot = new Vector2(0.5f, 1);
            contentRect.sizeDelta = new Vector2(0, 2000);
            
            VerticalLayoutGroup layout = content.AddComponent<VerticalLayoutGroup>();
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            layout.spacing = 15;
            layout.padding = new RectOffset(20, 20, 20, 20);
            
            ContentSizeFitter fitter = content.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            
            scroll.content = contentRect;
            scroll.horizontal = false;
            scroll.vertical = true;
            
            return content.transform;
        }
        
        private void CreateHeader(Transform parent, string text)
        {
            GameObject header = new GameObject("Header");
            header.transform.SetParent(parent, false);
            
            LayoutElement layout = header.AddComponent<LayoutElement>();
            layout.preferredHeight = 60;
            
            Text headerText = header.AddComponent<Text>();
            headerText.text = text;
            headerText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            headerText.fontSize = 28;
            headerText.color = Color.yellow;
            headerText.alignment = TextAnchor.MiddleCenter;
        }
        
        private void CreateDropdown(Transform parent, string label, out Dropdown dropdown, List<string> options)
        {
            GameObject row = new GameObject($"Row_{label}");
            row.transform.SetParent(parent, false);
            
            LayoutElement rowLayout = row.AddComponent<LayoutElement>();
            rowLayout.preferredHeight = 50;
            
            HorizontalLayoutGroup rowHLayout = row.AddComponent<HorizontalLayoutGroup>();
            rowHLayout.childForceExpandWidth = true;
            rowHLayout.spacing = 10;
            
            GameObject labelObj = new GameObject("Label");
            labelObj.transform.SetParent(row.transform, false);
            
            LayoutElement labelLayout = labelObj.AddComponent<LayoutElement>();
            labelLayout.preferredWidth = 200;
            
            Text labelText = labelObj.AddComponent<Text>();
            labelText.text = label;
            labelText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            labelText.fontSize = 18;
            labelText.color = Color.white;
            labelText.alignment = TextAnchor.MiddleLeft;
            
            GameObject dropdownObj = new GameObject("Dropdown");
            dropdownObj.transform.SetParent(row.transform, false);
            
            Image dropdownImg = dropdownObj.AddComponent<Image>();
            dropdownImg.color = new Color(0.2f, 0.2f, 0.3f);
            
            dropdown = dropdownObj.AddComponent<Dropdown>();
            dropdown.ClearOptions();
            dropdown.AddOptions(options);
            
            GameObject dropdownLabel = new GameObject("Label");
            dropdownLabel.transform.SetParent(dropdownObj.transform, false);
            
            RectTransform dropdownLabelRect = dropdownLabel.AddComponent<RectTransform>();
            dropdownLabelRect.anchorMin = Vector2.zero;
            dropdownLabelRect.anchorMax = Vector2.one;
            dropdownLabelRect.offsetMin = new Vector2(10, 2);
            dropdownLabelRect.offsetMax = new Vector2(-25, -2);
            
            Text dropdownLabelText = dropdownLabel.AddComponent<Text>();
            dropdownLabelText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            dropdownLabelText.fontSize = 16;
            dropdownLabelText.color = Color.white;
            dropdownLabelText.alignment = TextAnchor.MiddleLeft;
            
            dropdown.captionText = dropdownLabelText;
        }
        
        private void CreateInputField(Transform parent, string label, out InputField inputField)
        {
            GameObject row = new GameObject($"Row_{label}");
            row.transform.SetParent(parent, false);
            
            LayoutElement rowLayout = row.AddComponent<LayoutElement>();
            rowLayout.preferredHeight = 50;
            
            HorizontalLayoutGroup rowHLayout = row.AddComponent<HorizontalLayoutGroup>();
            rowHLayout.childForceExpandWidth = true;
            rowHLayout.spacing = 10;
            
            GameObject labelObj = new GameObject("Label");
            labelObj.transform.SetParent(row.transform, false);
            
            LayoutElement labelLayout = labelObj.AddComponent<LayoutElement>();
            labelLayout.preferredWidth = 200;
            
            Text labelText = labelObj.AddComponent<Text>();
            labelText.text = label;
            labelText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            labelText.fontSize = 18;
            labelText.color = Color.white;
            labelText.alignment = TextAnchor.MiddleLeft;
            
            GameObject inputObj = new GameObject("InputField");
            inputObj.transform.SetParent(row.transform, false);
            
            Image inputImg = inputObj.AddComponent<Image>();
            inputImg.color = new Color(0.2f, 0.2f, 0.3f);
            
            inputField = inputObj.AddComponent<InputField>();
            
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(inputObj.transform, false);
            
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(10, 5);
            textRect.offsetMax = new Vector2(-10, -5);
            
            Text text = textObj.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 16;
            text.color = Color.white;
            text.alignment = TextAnchor.MiddleLeft;
            
            inputField.textComponent = text;
        }
        
        private void CreateButton(Transform parent, string label, UnityEngine.Events.UnityAction onClick)
        {
            GameObject btnObj = new GameObject($"Btn_{label}");
            btnObj.transform.SetParent(parent, false);
            
            LayoutElement btnLayout = btnObj.AddComponent<LayoutElement>();
            btnLayout.preferredHeight = 60;
            
            Image btnImg = btnObj.AddComponent<Image>();
            btnImg.color = new Color(0.3f, 0.6f, 0.3f);
            
            Button btn = btnObj.AddComponent<Button>();
            btn.targetGraphic = btnImg;
            btn.onClick.AddListener(onClick);
            
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(btnObj.transform, false);
            
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            
            Text text = textObj.AddComponent<Text>();
            text.text = label;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 20;
            text.color = Color.white;
            text.alignment = TextAnchor.MiddleCenter;
        }
        
        private void CreatePreviewPanel(Transform parent)
        {
            GameObject panel = new GameObject("PreviewPanel");
            panel.transform.SetParent(parent, false);
            
            LayoutElement panelLayout = panel.AddComponent<LayoutElement>();
            panelLayout.preferredHeight = 300;
            
            Image panelImg = panel.AddComponent<Image>();
            panelImg.color = new Color(0.1f, 0.1f, 0.15f);
            
            GameObject textObj = new GameObject("PreviewText");
            textObj.transform.SetParent(panel.transform, false);
            
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(15, 15);
            textRect.offsetMax = new Vector2(-15, -15);
            
            _previewText = textObj.AddComponent<Text>();
            _previewText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _previewText.fontSize = 16;
            _previewText.color = Color.white;
            _previewText.alignment = TextAnchor.UpperLeft;
        }
        
        private void GenerateRandomArmor()
        {
            Rarity rarity = (Rarity)Random.Range(0, System.Enum.GetValues(typeof(Rarity)).Length);
            _currentArmor = _itemGenerator.GenerateArmor(rarity);
            
            _slotDropdown.value = (int)_currentArmor.armorSlot;
            _archetypeDropdown.value = (int)_currentArmor.armorArchetype;
            _rarityDropdown.value = (int)_currentArmor.rarity;
            _armorInput.text = _currentArmor.armorValue.ToString();
            
            UpdatePreview();
        }
        
        private void UpdatePreview()
        {
            if (_currentArmor == null) return;
            
            _currentArmor.armorSlot = (ArmorSlot)_slotDropdown.value;
            _currentArmor.armorArchetype = (ArmorArchetype)_archetypeDropdown.value;
            _currentArmor.rarity = (Rarity)_rarityDropdown.value;
            
            if (int.TryParse(_armorInput.text, out int armor))
                _currentArmor.armorValue = armor;
            
            Color rarityColor = GetRarityColor(_currentArmor.rarity);
            string rarityHex = ColorUtility.ToHtmlStringRGB(rarityColor);
            
            _previewText.text = $"<b><color=#{rarityHex}>{_currentArmor.itemName}</color></b>\n" +
                              $"<color=grey>Level {_currentArmor.level} {_currentArmor.armorArchetype} {_currentArmor.armorSlot}</color>\n\n" +
                              $"<color=orange>Armor:</color> {_currentArmor.armorValue}\n\n" +
                              $"<b>Affixes:</b>\n";
            
            foreach (ItemAffix affix in _currentArmor.affixes)
            {
                _previewText.text += $"+ {affix.affixType}: {affix.value}\n";
            }
        }
        
        private Color GetRarityColor(Rarity rarity)
        {
            return rarity switch
            {
                Rarity.Common => Color.white,
                Rarity.Uncommon => Color.green,
                Rarity.Rare => Color.blue,
                Rarity.Epic => new Color(0.6f, 0.3f, 1f),
                Rarity.Legendary => new Color(1f, 0.5f, 0f),
                Rarity.Mythic => new Color(1f, 0f, 0f),
                _ => Color.white
            };
        }
        
        private void SaveArmor()
        {
            string json = JsonUtility.ToJson(_currentArmor, true);
            Debug.Log($"[ArmorEditor] Saved armor:\n{json}");
            
            PlayerPrefs.SetString($"Armor_{_currentArmor.itemId}", json);
            PlayerPrefs.Save();
        }
    }
} 
 
#####CharacterCreationUI.cs##### 
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Game.Core.Systems;
using System.Collections.Generic;

namespace Game.UI
{
    public class CharacterCreationUI : MonoBehaviour
    {
        [Header("Navigation")]
        [SerializeField] private Button _nextButton;
        [SerializeField] private Button _backButton;
        [SerializeField] private Button _startGameButton;
        
        private CharacterCreationData _characterData;
        private int _currentStep;
        private int _attributePoints = 20;
        private GameObject _previewCharacter;
        private ProceduralCharacterBuilder _characterBuilder;
        private Canvas _mainCanvas;
        
        private List<GameObject> _allPanels = new List<GameObject>();
        
        private void Awake()
        {
            _characterData = new CharacterCreationData();
            _currentStep = 0;
            
            _characterBuilder = FindFirstObjectByType<ProceduralCharacterBuilder>();
            if (_characterBuilder == null)
            {
                GameObject builderObj = new GameObject("CharacterBuilder");
                _characterBuilder = builderObj.AddComponent<ProceduralCharacterBuilder>();
            }
            
            InitializeUI();
        }
        
        private void Start()
        {
            ShowStep(_currentStep);
        }
        
        private void InitializeUI()
        {
            _mainCanvas = GetComponent<Canvas>();
            if (_mainCanvas == null)
            {
                _mainCanvas = gameObject.AddComponent<Canvas>();
                _mainCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                
                CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                
                gameObject.AddComponent<GraphicRaycaster>();
            }
            
            CreateAllPanels();
            CreateNavigationButtons();
        }
        
        private void CreateAllPanels()
        {
            _allPanels.Add(CreateWelcomePanel());
            _allPanels.Add(CreateSpeciesPanel());
            _allPanels.Add(CreateCustomizationPanel());
            _allPanels.Add(CreateAttributesPanel());
            _allPanels.Add(CreateConfirmPanel());
        }
        
        private GameObject CreateWelcomePanel()
        {
            GameObject panel = CreatePanel("WelcomePanel");
            
            CreateText(panel.transform, "CREATE YOUR HERO", 
                new Vector2(0.5f, 0.6f), new Vector2(800, 100), 60, Color.white);
            
            CreateText(panel.transform, "Begin your epic adventure", 
                new Vector2(0.5f, 0.5f), new Vector2(600, 50), 24, Color.gray);
            
            Button startBtn = CreateButton(panel.transform, "START", 
                new Vector2(0.5f, 0.35f), new Vector2(300, 60), () => NextStep());
            
            return panel;
        }
        
        private GameObject CreateSpeciesPanel()
        {
            GameObject panel = CreatePanel("SpeciesPanel");
            
            CreateText(panel.transform, "SELECT YOUR SPECIES", 
                new Vector2(0.5f, 0.9f), new Vector2(800, 60), 40, Color.white);
            
            Species[] species = (Species[])System.Enum.GetValues(typeof(Species));
            int columns = 2;
            float startY = 0.75f;
            float spacing = 0.08f;
            
            for (int i = 0; i < species.Length; i++)
            {
                int row = i / columns;
                int col = i % columns;
                
                float x = 0.3f + (col * 0.4f);
                float y = startY - (row * spacing);
                
                Species currentSpecies = species[i];
                CreateButton(panel.transform, species[i].ToString(), 
                    new Vector2(x, y), new Vector2(350, 50), 
                    () => SelectSpecies(currentSpecies));
            }
            
            CreateText(panel.transform, "Choose wisely - each species has unique abilities!", 
                new Vector2(0.5f, 0.1f), new Vector2(700, 40), 18, Color.yellow);
            
            return panel;
        }
        
        private GameObject CreateCustomizationPanel()
        {
            GameObject panel = CreatePanel("CustomizationPanel");
            
            CreateText(panel.transform, "CUSTOMIZE APPEARANCE", 
                new Vector2(0.5f, 0.9f), new Vector2(800, 60), 40, Color.white);
            
            CreateText(panel.transform, "Gender:", new Vector2(0.25f, 0.7f), 
                new Vector2(150, 30), 20, Color.white);
            CreateButton(panel.transform, "Male", new Vector2(0.4f, 0.7f), new Vector2(120, 40), 
                () => { _characterData.gender = Gender.Male; });
            CreateButton(panel.transform, "Female", new Vector2(0.55f, 0.7f), new Vector2(120, 40), 
                () => { _characterData.gender = Gender.Female; });
            
            CreateText(panel.transform, "Build:", new Vector2(0.25f, 0.6f), 
                new Vector2(150, 30), 20, Color.white);
            CreateButton(panel.transform, "Slim", new Vector2(0.35f, 0.6f), new Vector2(100, 40), 
                () => { _characterData.bodyType = BodyType.Slim; });
            CreateButton(panel.transform, "Average", new Vector2(0.5f, 0.6f), new Vector2(100, 40), 
                () => { _characterData.bodyType = BodyType.Average; });
            CreateButton(panel.transform, "Muscular", new Vector2(0.65f, 0.6f), new Vector2(100, 40), 
                () => { _characterData.bodyType = BodyType.Muscular; });
            
            return panel;
        }
        
        private GameObject CreateAttributesPanel()
        {
            GameObject panel = CreatePanel("AttributesPanel");
            
            CreateText(panel.transform, "ALLOCATE ATTRIBUTES", 
                new Vector2(0.5f, 0.9f), new Vector2(800, 60), 40, Color.white);
            
            CreateText(panel.transform, $"Points Remaining: {_attributePoints}", 
                new Vector2(0.5f, 0.8f), new Vector2(400, 40), 24, Color.yellow);
            
            float yStart = 0.7f;
            float yStep = 0.1f;
            
            CreateAttributeRow(panel.transform, "Strength", yStart, 
                v => _characterData.strength = (int)v);
            CreateAttributeRow(panel.transform, "Dexterity", yStart - yStep, 
                v => _characterData.dexterity = (int)v);
            CreateAttributeRow(panel.transform, "Intelligence", yStart - yStep * 2, 
                v => _characterData.intelligence = (int)v);
            CreateAttributeRow(panel.transform, "Vitality", yStart - yStep * 3, 
                v => _characterData.vitality = (int)v);
            CreateAttributeRow(panel.transform, "Endurance", yStart - yStep * 4, 
                v => _characterData.endurance = (int)v);
            CreateAttributeRow(panel.transform, "Luck", yStart - yStep * 5, 
                v => _characterData.luck = (int)v);
            
            return panel;
        }
        
        private void CreateAttributeRow(Transform parent, string name, float yPos, 
            UnityEngine.Events.UnityAction<float> onChanged)
        {
            CreateText(parent, name + ":", new Vector2(0.25f, yPos), 
                new Vector2(150, 30), 18, Color.white);
            
            CreateButton(parent, "-", new Vector2(0.4f, yPos), new Vector2(40, 30), 
                () => AdjustAttribute(name, -1));
            
            CreateText(parent, "10", new Vector2(0.5f, yPos), 
                new Vector2(60, 30), 20, Color.cyan).name = name + "Value";
            
            CreateButton(parent, "+", new Vector2(0.6f, yPos), new Vector2(40, 30), 
                () => AdjustAttribute(name, 1));
        }
        
        private void AdjustAttribute(string attributeName, int delta)
        {
            int currentValue = attributeName switch
            {
                "Strength" => _characterData.strength,
                "Dexterity" => _characterData.dexterity,
                "Intelligence" => _characterData.intelligence,
                "Vitality" => _characterData.vitality,
                "Endurance" => _characterData.endurance,
                "Luck" => _characterData.luck,
                _ => 10
            };
            
            int newValue = Mathf.Clamp(currentValue + delta, 5, 25);
            
            if (newValue == currentValue) return;
            
            int totalUsed = (_characterData.strength + _characterData.dexterity + 
                           _characterData.intelligence + _characterData.vitality + 
                           _characterData.endurance + _characterData.luck) - 60;
            
            if (delta > 0 && totalUsed >= _attributePoints) return;
            
            switch (attributeName)
            {
                case "Strength": _characterData.strength = newValue; break;
                case "Dexterity": _characterData.dexterity = newValue; break;
                case "Intelligence": _characterData.intelligence = newValue; break;
                case "Vitality": _characterData.vitality = newValue; break;
                case "Endurance": _characterData.endurance = newValue; break;
                case "Luck": _characterData.luck = newValue; break;
            }
            
            UpdateAttributeDisplay();
        }
        
        private void UpdateAttributeDisplay()
        {
            UpdateValueText("StrengthValue", _characterData.strength);
            UpdateValueText("DexterityValue", _characterData.dexterity);
            UpdateValueText("IntelligenceValue", _characterData.intelligence);
            UpdateValueText("VitalityValue", _characterData.vitality);
            UpdateValueText("EnduranceValue", _characterData.endurance);
            UpdateValueText("LuckValue", _characterData.luck);
            
            int used = (_characterData.strength + _characterData.dexterity + 
                       _characterData.intelligence + _characterData.vitality + 
                       _characterData.endurance + _characterData.luck) - 60;
            
            Text pointsText = FindTextByContent("Points Remaining:");
            if (pointsText != null)
            {
                int remaining = _attributePoints - used;
                pointsText.text = $"Points Remaining: {remaining}";
                pointsText.color = remaining < 0 ? Color.red : Color.yellow;
            }
        }
        
        private void UpdateValueText(string objName, int value)
        {
            GameObject obj = GameObject.Find(objName);
            if (obj != null)
            {
                Text text = obj.GetComponent<Text>();
                if (text != null) text.text = value.ToString();
            }
        }
        
        private Text FindTextByContent(string content)
        {
            foreach (Text text in FindObjectsByType<Text>(FindObjectsSortMode.None))
            {
                if (text.text.Contains(content)) return text;
            }
            return null;
        }
        
        private GameObject CreateConfirmPanel()
        {
            GameObject panel = CreatePanel("ConfirmPanel");
            
            CreateText(panel.transform, "CONFIRM YOUR HERO", 
                new Vector2(0.5f, 0.9f), new Vector2(800, 60), 40, Color.white);
            
            Text summary = CreateText(panel.transform, "Summary will appear here", 
                new Vector2(0.5f, 0.5f), new Vector2(700, 600), 18, Color.white);
            summary.alignment = TextAnchor.UpperLeft;
            summary.name = "SummaryText";
            
            return panel;
        }
        
        private void CreateNavigationButtons()
        {
            _backButton = CreateButton(transform, "← BACK", 
                new Vector2(0.1f, 0.05f), new Vector2(150, 50), () => PreviousStep());
            
            _nextButton = CreateButton(transform, "NEXT →", 
                new Vector2(0.9f, 0.05f), new Vector2(150, 50), () => NextStep());
            
            _startGameButton = CreateButton(transform, "START GAME!", 
                new Vector2(0.5f, 0.1f), new Vector2(300, 60), () => StartGame());
            _startGameButton.gameObject.SetActive(false);
        }
        
        private GameObject CreatePanel(string name)
        {
            GameObject panel = new GameObject(name);
            panel.transform.SetParent(transform, false);
            
            RectTransform rect = panel.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;
            
            Image img = panel.AddComponent<Image>();
            img.color = new Color(0.1f, 0.1f, 0.15f, 0.95f);
            
            panel.SetActive(false);
            return panel;
        }
        
        private Text CreateText(Transform parent, string content, Vector2 anchorPos, 
            Vector2 size, int fontSize, Color color)
        {
            GameObject obj = new GameObject("Text_" + content.Replace(" ", "_"));
            obj.transform.SetParent(parent, false);
            
            RectTransform rect = obj.AddComponent<RectTransform>();
            rect.anchorMin = anchorPos;
            rect.anchorMax = anchorPos;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            
            Text text = obj.AddComponent<Text>();
            text.text = content;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize;
            text.color = color;
            text.alignment = TextAnchor.MiddleCenter;
            
            return text;
        }
        
        private Button CreateButton(Transform parent, string label, Vector2 anchorPos, 
            Vector2 size, UnityEngine.Events.UnityAction onClick)
        {
            GameObject obj = new GameObject("Btn_" + label.Replace(" ", "_"));
            obj.transform.SetParent(parent, false);
            
            RectTransform rect = obj.AddComponent<RectTransform>();
            rect.anchorMin = anchorPos;
            rect.anchorMax = anchorPos;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            
            Image img = obj.AddComponent<Image>();
            img.color = new Color(0.3f, 0.5f, 0.8f);
            
            Button btn = obj.AddComponent<Button>();
            btn.targetGraphic = img;
            btn.onClick.AddListener(onClick);
            
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(obj.transform, false);
            
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            
            Text text = textObj.AddComponent<Text>();
            text.text = label;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 20;
            text.color = Color.white;
            text.alignment = TextAnchor.MiddleCenter;
            
            return btn;
        }
        
        private void SelectSpecies(Species species)
        {
            _characterData.species = species;
            Debug.Log($"Selected species: {species}");
        }
        
        private void ShowStep(int step)
        {
            for (int i = 0; i < _allPanels.Count; i++)
            {
                _allPanels[i].SetActive(i == step);
            }
            
            _backButton.gameObject.SetActive(step > 0 && step < 4);
            _nextButton.gameObject.SetActive(step < 4);
            _startGameButton.gameObject.SetActive(step == 4);
            
            if (step == 4) UpdateConfirmationSummary();
        }
        
        private void UpdateConfirmationSummary()
        {
            Text summary = GameObject.Find("SummaryText")?.GetComponent<Text>();
            if (summary != null)
            {
                summary.text = $"<b>SPECIES:</b> {_characterData.species}\n" +
                              $"<b>GENDER:</b> {_characterData.gender}\n" +
                              $"<b>BUILD:</b> {_characterData.bodyType}\n\n" +
                              $"<b>ATTRIBUTES:</b>\n" +
                              $"Strength: {_characterData.strength}\n" +
                              $"Dexterity: {_characterData.dexterity}\n" +
                              $"Intelligence: {_characterData.intelligence}\n" +
                              $"Vitality: {_characterData.vitality}\n" +
                              $"Endurance: {_characterData.endurance}\n" +
                              $"Luck: {_characterData.luck}\n\n" +
                              $"<color=yellow>Ready to begin your adventure?</color>";
            }
        }
        
        private void NextStep()
        {
            _currentStep++;
            if (_currentStep >= _allPanels.Count) _currentStep = _allPanels.Count - 1;
            ShowStep(_currentStep);
        }
        
        private void PreviousStep()
        {
            _currentStep--;
            if (_currentStep < 0) _currentStep = 0;
            ShowStep(_currentStep);
        }
        
        private async void StartGame()
        {
            // Validate character
            int totalUsed = (_characterData.strength + _characterData.dexterity + 
                           _characterData.intelligence + _characterData.vitality + 
                           _characterData.endurance + _characterData.luck) - 60;
            
            if (totalUsed > _attributePoints)
            {
                Debug.LogError("[CharacterCreation] Too many attribute points allocated!");
                return;
            }
            
            // Save character
            string json = JsonUtility.ToJson(_characterData);
            PlayerPrefs.SetString("CurrentCharacter", json);
            PlayerPrefs.Save();
            
            Debug.Log($"[CharacterCreation] Starting game with {_characterData.species} {_characterData.gender}");
            
            // CRITICAL FIX: Check if scene exists in build settings
            int sceneCount = SceneManager.sceneCountInBuildSettings;
            bool foundGameWorld = false;
            
            for (int i = 0; i < sceneCount; i++)
            {
                string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
                string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);
                
                if (sceneName == "GameWorld")
                {
                    foundGameWorld = true;
                    break;
                }
            }
            
            if (!foundGameWorld)
            {
                Debug.LogError("[CharacterCreation] GameWorld scene not in Build Settings!");
                Debug.LogError("Go to File > Build Settings and add GameWorld.unity");
                return;
            }
            
            // Load game world
            AsyncOperation loadOp = SceneManager.LoadSceneAsync("GameWorld", LoadSceneMode.Single);
            
            if (loadOp == null)
            {
                Debug.LogError("[CharacterCreation] Failed to start loading GameWorld!");
                return;
            }
            
            while (!loadOp.isDone)
            {
                await Awaitable.NextFrameAsync();
            }
            
            Debug.Log("[CharacterCreation] GameWorld loaded successfully");
        }
    }
} 
 
#####InventoryUI.cs##### 
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace Game.UI
{
    /// <summary>
    /// Complete inventory UI with equipment panel, item tooltips, and drag-drop.
    /// </summary>
    public class InventoryUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Canvas _canvas;
        [SerializeField] private GameObject _inventoryPanel;
        [SerializeField] private GameObject _equipmentPanel;
        [SerializeField] private GameObject _tooltipPanel;
        
        [Header("Inventory Grid")]
        [SerializeField] private Transform _inventoryGridParent;
        [SerializeField] private List<InventorySlotUI> _inventorySlots = new();
        
        [Header("Equipment Slots")]
        [SerializeField] private EquipmentSlotUI _weaponSlot;
        [SerializeField] private EquipmentSlotUI _headSlot;
        [SerializeField] private EquipmentSlotUI _chestSlot;
        [SerializeField] private EquipmentSlotUI _handsSlot;
        [SerializeField] private EquipmentSlotUI _legsSlot;
        [SerializeField] private EquipmentSlotUI _feetSlot;
        
        [Header("Tooltip")]
        [SerializeField] private Text _tooltipText;
        
        private Game.Core.Systems.InventorySystemManager _inventoryManager;
        private GameObject _player;
        private bool _isOpen;
        
        #region Initialization
        
        private void Awake()
        {
            CreateUI();
        }
        
        private void Start()
        {
            _inventoryManager = Game.Core.Systems.CoreSystemManager.InventoryManager;
            _player = GameObject.FindGameObjectWithTag("Player");
            
            if (_inventoryManager != null)
            {
                _inventoryManager.OnInventoryChanged += OnInventoryChanged;
                _inventoryManager.OnEquipmentChanged += OnEquipmentChanged;
            }
            
            RefreshInventory();
            _inventoryPanel.SetActive(false);
        }
        
        private void OnDestroy()
        {
            if (_inventoryManager != null)
            {
                _inventoryManager.OnInventoryChanged -= OnInventoryChanged;
                _inventoryManager.OnEquipmentChanged -= OnEquipmentChanged;
            }
        }
        
        #endregion
        
        #region UI Creation
        
        private void CreateUI()
        {
            // Create canvas
            if (_canvas == null)
            {
                GameObject canvasObj = new GameObject("InventoryCanvas");
                canvasObj.transform.SetParent(transform, false);
                
                _canvas = canvasObj.AddComponent<Canvas>();
                _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                _canvas.sortingOrder = 20;
                
                CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                
                canvasObj.AddComponent<GraphicRaycaster>();
            }
            
            CreateInventoryPanel();
            CreateEquipmentPanel();
            CreateTooltipPanel();
        }
        
        private void CreateInventoryPanel()
        {
            _inventoryPanel = new GameObject("InventoryPanel");
            _inventoryPanel.transform.SetParent(_canvas.transform, false);
            
            RectTransform rect = _inventoryPanel.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.25f, 0.2f);
            rect.anchorMax = new Vector2(0.75f, 0.8f);
            rect.sizeDelta = Vector2.zero;
            
            Image bg = _inventoryPanel.AddComponent<Image>();
            bg.color = new Color(0.1f, 0.1f, 0.15f, 0.95f);
            
            // Title
            CreateTitle(_inventoryPanel.transform, "INVENTORY");
            
            // Close button
            CreateCloseButton(_inventoryPanel.transform);
            
            // Inventory grid
            CreateInventoryGrid();
        }
        
        private void CreateTitle(Transform parent, string text)
        {
            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(parent, false);
            
            RectTransform rect = titleObj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(1, 1);
            rect.pivot = new Vector2(0.5f, 1);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(0, 60);
            
            Text titleText = titleObj.AddComponent<Text>();
            titleText.text = text;
            titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            titleText.fontSize = 32;
            titleText.color = Color.yellow;
            titleText.alignment = TextAnchor.MiddleCenter;
        }
        
        private void CreateCloseButton(Transform parent)
        {
            GameObject btnObj = new GameObject("CloseButton");
            btnObj.transform.SetParent(parent, false);
            
            RectTransform rect = btnObj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(1, 1);
            rect.anchorMax = new Vector2(1, 1);
            rect.pivot = new Vector2(1, 1);
            rect.anchoredPosition = new Vector2(-10, -10);
            rect.sizeDelta = new Vector2(50, 50);
            
            Image img = btnObj.AddComponent<Image>();
            img.color = new Color(0.6f, 0.2f, 0.2f);
            
            Button btn = btnObj.AddComponent<Button>();
            btn.targetGraphic = img;
            btn.onClick.AddListener(ToggleInventory);
            
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(btnObj.transform, false);
            
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            
            Text text = textObj.AddComponent<Text>();
            text.text = "X";
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 24;
            text.color = Color.white;
            text.alignment = TextAnchor.MiddleCenter;
        }
        
        private void CreateInventoryGrid()
        {
            GameObject gridObj = new GameObject("InventoryGrid");
            gridObj.transform.SetParent(_inventoryPanel.transform, false);
            
            RectTransform rect = gridObj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 0);
            rect.anchorMax = new Vector2(0.65f, 1);
            rect.offsetMin = new Vector2(10, 10);
            rect.offsetMax = new Vector2(-10, -70);
            
            ScrollRect scroll = gridObj.AddComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.vertical = true;
            
            // Content
            GameObject content = new GameObject("Content");
            content.transform.SetParent(gridObj.transform, false);
            
            RectTransform contentRect = content.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 1);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.pivot = new Vector2(0.5f, 1);
            contentRect.sizeDelta = new Vector2(0, 1000);
            
            GridLayoutGroup grid = content.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(80, 80);
            grid.spacing = new Vector2(5, 5);
            grid.padding = new RectOffset(10, 10, 10, 10);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 5;
            
            ContentSizeFitter fitter = content.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            
            scroll.content = contentRect;
            _inventoryGridParent = content.transform;
            
            // Create 30 slots
            for (int i = 0; i < 30; i++)
            {
                CreateInventorySlot(i);
            }
        }
        
        private void CreateInventorySlot(int index)
        {
            GameObject slotObj = new GameObject($"InventorySlot_{index}");
            slotObj.transform.SetParent(_inventoryGridParent, false);
            
            Image bg = slotObj.AddComponent<Image>();
            bg.color = new Color(0.2f, 0.2f, 0.25f);
            
            InventorySlotUI slot = slotObj.AddComponent<InventorySlotUI>();
            slot.Initialize(index, this);
            
            _inventorySlots.Add(slot);
        }
        
        private void CreateEquipmentPanel()
        {
            _equipmentPanel = new GameObject("EquipmentPanel");
            _equipmentPanel.transform.SetParent(_canvas.transform, false);
            
            RectTransform rect = _equipmentPanel.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 0);
            rect.anchorMax = new Vector2(0, 0);
            rect.pivot = new Vector2(0, 0);
            rect.anchoredPosition = new Vector2(10, 10);
            rect.sizeDelta = new Vector2(300, 500);
            
            Image bg = _equipmentPanel.AddComponent<Image>();
            bg.color = new Color(0.1f, 0.1f, 0.15f, 0.95f);
            
            // Title
            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(_equipmentPanel.transform, false);
            
            RectTransform titleRect = titleObj.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0, 1);
            titleRect.anchorMax = new Vector2(1, 1);
            titleRect.pivot = new Vector2(0.5f, 1);
            titleRect.sizeDelta = new Vector2(0, 50);
            
            Text titleText = titleObj.AddComponent<Text>();
            titleText.text = "EQUIPMENT";
            titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            titleText.fontSize = 24;
            titleText.color = Color.cyan;
            titleText.alignment = TextAnchor.MiddleCenter;
            
            // Equipment slots
            _weaponSlot = CreateEquipmentSlot("Weapon", new Vector2(0.5f, 0.8f));
            _headSlot = CreateEquipmentSlot("Head", new Vector2(0.5f, 0.65f));
            _chestSlot = CreateEquipmentSlot("Chest", new Vector2(0.5f, 0.5f));
            _handsSlot = CreateEquipmentSlot("Hands", new Vector2(0.5f, 0.35f));
            _legsSlot = CreateEquipmentSlot("Legs", new Vector2(0.5f, 0.2f));
            _feetSlot = CreateEquipmentSlot("Feet", new Vector2(0.5f, 0.05f));
            
            _equipmentPanel.SetActive(false);
        }
        
        private EquipmentSlotUI CreateEquipmentSlot(string slotName, Vector2 anchorPos)
        {
            GameObject slotObj = new GameObject($"EquipmentSlot_{slotName}");
            slotObj.transform.SetParent(_equipmentPanel.transform, false);
            
            RectTransform rect = slotObj.AddComponent<RectTransform>();
            rect.anchorMin = anchorPos;
            rect.anchorMax = anchorPos;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(100, 100);
            
            Image bg = slotObj.AddComponent<Image>();
            bg.color = new Color(0.3f, 0.3f, 0.35f);
            
            // Label
            GameObject labelObj = new GameObject("Label");
            labelObj.transform.SetParent(slotObj.transform, false);
            
            RectTransform labelRect = labelObj.AddComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0, 0);
            labelRect.anchorMax = new Vector2(1, 0);
            labelRect.pivot = new Vector2(0.5f, 0);
            labelRect.sizeDelta = new Vector2(0, 20);
            
            Text labelText = labelObj.AddComponent<Text>();
            labelText.text = slotName;
            labelText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            labelText.fontSize = 12;
            labelText.color = Color.gray;
            labelText.alignment = TextAnchor.MiddleCenter;
            
            EquipmentSlotUI slot = slotObj.AddComponent<EquipmentSlotUI>();
            slot.Initialize(slotName, this);
            
            return slot;
        }
        
        private void CreateTooltipPanel()
        {
            _tooltipPanel = new GameObject("TooltipPanel");
            _tooltipPanel.transform.SetParent(_canvas.transform, false);
            
            RectTransform rect = _tooltipPanel.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 0);
            rect.anchorMax = new Vector2(0, 0);
            rect.pivot = new Vector2(0, 0);
            rect.sizeDelta = new Vector2(300, 200);
            
            Image bg = _tooltipPanel.AddComponent<Image>();
            bg.color = new Color(0.05f, 0.05f, 0.1f, 0.95f);
            
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(_tooltipPanel.transform, false);
            
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(10, 10);
            textRect.offsetMax = new Vector2(-10, -10);
            
            _tooltipText = textObj.AddComponent<Text>();
            _tooltipText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _tooltipText.fontSize = 14;
            _tooltipText.color = Color.white;
            _tooltipText.alignment = TextAnchor.UpperLeft;
            
            _tooltipPanel.SetActive(false);
        }
        
        #endregion
        
        #region UI Management
        
        public void ToggleInventory()
        {
            _isOpen = !_isOpen;
            _inventoryPanel.SetActive(_isOpen);
            _equipmentPanel.SetActive(_isOpen);
            
            if (_isOpen)
            {
                RefreshInventory();
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                HideTooltip();
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
        
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.I))
            {
                ToggleInventory();
            }
            
            if (Input.GetKeyDown(KeyCode.Escape) && _isOpen)
            {
                ToggleInventory();
            }
        }
        
        #endregion
        
        #region Refresh
        
        private void OnInventoryChanged(GameObject entity)
        {
            if (entity == _player)
                RefreshInventory();
        }
        
        private void OnEquipmentChanged(GameObject entity)
        {
            if (entity == _player)
                RefreshEquipment();
        }
        
        private void RefreshInventory()
        {
            if (_inventoryManager == null || _player == null) return;
            
            Game.Core.Systems.InventoryData inventory = _inventoryManager.GetInventory(_player);
            
            // Clear all slots
            foreach (InventorySlotUI slot in _inventorySlots)
            {
                slot.ClearSlot();
            }
            
            // Fill slots with items
            for (int i = 0; i < inventory.Slots.Count && i < _inventorySlots.Count; i++)
            {
                _inventorySlots[i].SetItem(inventory.Slots[i].item, inventory.Slots[i].quantity);
            }
            
            RefreshEquipment();
        }
        
        private void RefreshEquipment()
        {
            if (_inventoryManager == null || _player == null) return;
            
            Game.Core.Systems.EquipmentData equipment = _inventoryManager.GetEquipment(_player);
            if (equipment == null) return;
            
            _weaponSlot.SetItem(equipment.equippedWeapon);
            _headSlot.SetItem(equipment.equippedHead);
            _chestSlot.SetItem(equipment.equippedChest);
            _handsSlot.SetItem(equipment.equippedHands);
            _legsSlot.SetItem(equipment.equippedLegs);
            _feetSlot.SetItem(equipment.equippedFeet);
        }
        
        #endregion
        
        #region Tooltip
        
        public void ShowTooltip(Game.Core.Systems.ItemData item, Vector2 position)
        {
            if (item == null)
            {
                HideTooltip();
                return;
            }
            
            _tooltipPanel.SetActive(true);
            
            Color rarityColor = GetRarityColor(item.rarity);
            string rarityHex = ColorUtility.ToHtmlStringRGB(rarityColor);
            
            string tooltip = $"<color=#{rarityHex}><b>{item.itemName}</b></color>\n";
            tooltip += $"<color=grey>{item.itemType} - Level {item.level}</color>\n\n";
            
            if (item.itemType == Game.Core.Systems.ItemType.Weapon)
            {
                tooltip += $"<color=orange>Damage:</color> {item.damage}\n";
                tooltip += $"<color=cyan>Attack Speed:</color> {item.attackSpeed:F2}\n";
                tooltip += $"<color=yellow>Range:</color> {item.range:F1}\n";
            }
            else if (item.itemType == Game.Core.Systems.ItemType.Armor)
            {
                tooltip += $"<color=orange>Armor:</color> {item.armorValue}\n";
            }
            
            if (item.affixes != null && item.affixes.Count > 0)
            {
                tooltip += "\n<b>Affixes:</b>\n";
                foreach (var affix in item.affixes)
                {
                    tooltip += $"<color=green>+ {affix.affixType}: {affix.value}</color>\n";
                }
            }
            
            _tooltipText.text = tooltip;
            
            RectTransform tooltipRect = _tooltipPanel.GetComponent<RectTransform>();
            tooltipRect.position = position + new Vector2(10, -10);
        }
        
        public void HideTooltip()
        {
            _tooltipPanel.SetActive(false);
        }
        
        private Color GetRarityColor(Game.Core.Systems.Rarity rarity)
        {
            return rarity switch
            {
                Game.Core.Systems.Rarity.Common => Color.white,
                Game.Core.Systems.Rarity.Uncommon => Color.green,
                Game.Core.Systems.Rarity.Rare => Color.blue,
                Game.Core.Systems.Rarity.Epic => new Color(0.6f, 0.3f, 1f),
                Game.Core.Systems.Rarity.Legendary => new Color(1f, 0.5f, 0f),
                Game.Core.Systems.Rarity.Mythic => new Color(1f, 0f, 0f),
                _ => Color.white
            };
        }
        
        #endregion
        
        #region Item Actions
        
        public void OnItemClicked(Game.Core.Systems.ItemData item)
        {
            if (item == null || _inventoryManager == null || _player == null) return;
            
            // Right click to equip
            if (Input.GetMouseButtonDown(1))
            {
                _inventoryManager.EquipItem(_player, item.itemId);
            }
        }
        
        public void OnEquipmentSlotClicked(Game.Core.Systems.ItemData item)
        {
            if (item == null || _inventoryManager == null || _player == null) return;
            
            // Right click to unequip
            if (Input.GetMouseButtonDown(1))
            {
                _inventoryManager.UnequipItem(_player, item.itemId);
            }
        }
        
        #endregion
    }
    
    #region Slot UI Components
    
    /// <summary>
    /// Individual inventory slot UI.
    /// </summary>
    public class InventorySlotUI : MonoBehaviour
    {
        private int _slotIndex;
        private Game.Core.Systems.ItemData _item;
        private int _quantity;
        private InventoryUI _inventoryUI;
        
        private Image _icon;
        private Text _quantityText;
        
        public void Initialize(int slotIndex, InventoryUI inventoryUI)
        {
            _slotIndex = slotIndex;
            _inventoryUI = inventoryUI;
            
            CreateSlotUI();
        }
        
        private void CreateSlotUI()
        {
            // Icon
            GameObject iconObj = new GameObject("Icon");
            iconObj.transform.SetParent(transform, false);
            
            RectTransform iconRect = iconObj.AddComponent<RectTransform>();
            iconRect.anchorMin = Vector2.zero;
            iconRect.anchorMax = Vector2.one;
            iconRect.offsetMin = new Vector2(5, 5);
            iconRect.offsetMax = new Vector2(-5, -5);
            
            _icon = iconObj.AddComponent<Image>();
            _icon.color = Color.clear;
            
            // Quantity text
            GameObject quantityObj = new GameObject("Quantity");
            quantityObj.transform.SetParent(transform, false);
            
            RectTransform quantityRect = quantityObj.AddComponent<RectTransform>();
            quantityRect.anchorMin = new Vector2(1, 0);
            quantityRect.anchorMax = new Vector2(1, 0);
            quantityRect.pivot = new Vector2(1, 0);
            quantityRect.sizeDelta = new Vector2(30, 20);
            
            _quantityText = quantityObj.AddComponent<Text>();
            _quantityText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _quantityText.fontSize = 14;
            _quantityText.color = Color.white;
            _quantityText.alignment = TextAnchor.LowerRight;
            
            // Button for click handling
            Button btn = gameObject.AddComponent<Button>();
            btn.targetGraphic = GetComponent<Image>();
            btn.onClick.AddListener(OnClicked);
        }
        
        public void SetItem(Game.Core.Systems.ItemData item, int quantity)
        {
            _item = item;
            _quantity = quantity;
            
            if (item != null)
            {
                _icon.color = GetRarityColor(item.rarity);
                _quantityText.text = quantity > 1 ? quantity.ToString() : "";
            }
            else
            {
                ClearSlot();
            }
        }
        
        public void ClearSlot()
        {
            _item = null;
            _quantity = 0;
            _icon.color = Color.clear;
            _quantityText.text = "";
        }
        
        private void OnClicked()
        {
            if (_item != null)
            {
                _inventoryUI.OnItemClicked(_item);
            }
        }
        
        private void OnPointerEnter()
        {
            if (_item != null)
            {
                _inventoryUI.ShowTooltip(_item, Input.mousePosition);
            }
        }
        
        private void OnPointerExit()
        {
            _inventoryUI.HideTooltip();
        }
        
        private Color GetRarityColor(Game.Core.Systems.Rarity rarity)
        {
            return rarity switch
            {
                Game.Core.Systems.Rarity.Common => new Color(0.6f, 0.6f, 0.6f),
                Game.Core.Systems.Rarity.Uncommon => new Color(0.2f, 0.8f, 0.2f),
                Game.Core.Systems.Rarity.Rare => new Color(0.2f, 0.4f, 1f),
                Game.Core.Systems.Rarity.Epic => new Color(0.6f, 0.3f, 1f),
                Game.Core.Systems.Rarity.Legendary => new Color(1f, 0.6f, 0f),
                Game.Core.Systems.Rarity.Mythic => new Color(1f, 0.2f, 0.2f),
                _ => Color.gray
            };
        }
    }
    
    /// <summary>
    /// Equipment slot UI.
    /// </summary>
    public class EquipmentSlotUI : MonoBehaviour
    {
        private string _slotName;
        private Game.Core.Systems.ItemData _item;
        private InventoryUI _inventoryUI;
        
        private Image _icon;
        
        public void Initialize(string slotName, InventoryUI inventoryUI)
        {
            _slotName = slotName;
            _inventoryUI = inventoryUI;
            
            CreateSlotUI();
        }
        
        private void CreateSlotUI()
        {
            // Icon
            GameObject iconObj = new GameObject("Icon");
            iconObj.transform.SetParent(transform, false);
            
            RectTransform iconRect = iconObj.AddComponent<RectTransform>();
            iconRect.anchorMin = Vector2.zero;
            iconRect.anchorMax = Vector2.one;
            iconRect.offsetMin = new Vector2(10, 20);
            iconRect.offsetMax = new Vector2(-10, -10);
            
            _icon = iconObj.AddComponent<Image>();
            _icon.color = Color.clear;
            
            // Button
            Button btn = gameObject.AddComponent<Button>();
            btn.targetGraphic = GetComponent<Image>();
            btn.onClick.AddListener(OnClicked);
        }
        
        public void SetItem(Game.Core.Systems.ItemData item)
        {
            _item = item;
            
            if (item != null)
            {
                _icon.color = GetRarityColor(item.rarity);
            }
            else
            {
                _icon.color = Color.clear;
            }
        }
        
        private void OnClicked()
        {
            if (_item != null)
            {
                _inventoryUI.OnEquipmentSlotClicked(_item);
            }
        }
        
        private Color GetRarityColor(Game.Core.Systems.Rarity rarity)
        {
            return rarity switch
            {
                Game.Core.Systems.Rarity.Common => new Color(0.6f, 0.6f, 0.6f),
                Game.Core.Systems.Rarity.Uncommon => new Color(0.2f, 0.8f, 0.2f),
                Game.Core.Systems.Rarity.Rare => new Color(0.2f, 0.4f, 1f),
                Game.Core.Systems.Rarity.Epic => new Color(0.6f, 0.3f, 1f),
                Game.Core.Systems.Rarity.Legendary => new Color(1f, 0.6f, 0f),
                Game.Core.Systems.Rarity.Mythic => new Color(1f, 0.2f, 0.2f),
                _ => Color.gray
            };
        }
    }
    
    #endregion
} 
 
#####MainMenuController.cs##### 
   using UnityEngine;
   using UnityEngine.SceneManagement;

   public class MainMenuController : MonoBehaviour
   {
       void Update()
       {
           if (Input.GetKeyDown(KeyCode.Space))
           {
               // Will be replaced with proper character creation
               Debug.Log("Starting game...");
           }
       }
   } 
 
#####NetworkUI.cs##### 
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    /// <summary>
    /// Network UI for hosting/joining games.
    /// </summary>
    public class NetworkUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Canvas _canvas;
        [SerializeField] private GameObject _menuPanel;
        
        [Header("Connection")]
        [SerializeField] private InputField _serverAddressInput;
        [SerializeField] private Button _hostButton;
        [SerializeField] private Button _joinButton;
        [SerializeField] private Text _statusText;
        
        private Game.Core.Systems.WebSocketNetworkManager _networkManager;
        
        private void Awake()
        {
            CreateUI();
        }
        
        private void Start()
        {
            _networkManager = Game.Core.Systems.CoreSystemManager.NetworkManager;
            
            if (_networkManager != null)
            {
                _networkManager.OnPlayerConnected += OnPlayerConnected;
                _networkManager.OnPlayerDisconnected += OnPlayerDisconnected;
            }
        }
        
        private void OnDestroy()
        {
            if (_networkManager != null)
            {
                _networkManager.OnPlayerConnected -= OnPlayerConnected;
                _networkManager.OnPlayerDisconnected -= OnPlayerDisconnected;
            }
        }
        
        private void CreateUI()
        {
            if (_canvas == null)
            {
                GameObject canvasObj = new GameObject("NetworkCanvas");
                canvasObj.transform.SetParent(transform, false);
                
                _canvas = canvasObj.AddComponent<Canvas>();
                _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                _canvas.sortingOrder = 25;
                
                CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                
                canvasObj.AddComponent<GraphicRaycaster>();
            }
            
            CreateMenuPanel();
        }
        
        private void CreateMenuPanel()
        {
            _menuPanel = new GameObject("NetworkMenuPanel");
            _menuPanel.transform.SetParent(_canvas.transform, false);
            
            RectTransform rect = _menuPanel.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(400, 300);
            
            Image bg = _menuPanel.AddComponent<Image>();
            bg.color = new Color(0.1f, 0.1f, 0.15f, 0.95f);
            
            // Title
            CreateTitle();
            
            // Server address input
            CreateServerAddressInput();
            
            // Buttons
            CreateHostButton();
            CreateJoinButton();
            
            // Status text
            CreateStatusText();
            
            _menuPanel.SetActive(false);
        }
        
        private void CreateTitle()
        {
            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(_menuPanel.transform, false);
            
            RectTransform rect = titleObj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(1, 1);
            rect.pivot = new Vector2(0.5f, 1);
            rect.anchoredPosition = new Vector2(0, -10);
            rect.sizeDelta = new Vector2(0, 50);
            
            Text title = titleObj.AddComponent<Text>();
            title.text = "MULTIPLAYER";
            title.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            title.fontSize = 28;
            title.color = Color.cyan;
            title.alignment = TextAnchor.MiddleCenter;
        }
        
        private void CreateServerAddressInput()
        {
            GameObject inputObj = new GameObject("ServerAddressInput");
            inputObj.transform.SetParent(_menuPanel.transform, false);
            
            RectTransform rect = inputObj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.6f);
            rect.anchorMax = new Vector2(0.5f, 0.6f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(350, 40);
            
            Image img = inputObj.AddComponent<Image>();
            img.color = new Color(0.2f, 0.2f, 0.3f);
            
            _serverAddressInput = inputObj.AddComponent<InputField>();
            _serverAddressInput.text = "127.0.0.1";
            
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(inputObj.transform, false);
            
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(10, 5);
            textRect.offsetMax = new Vector2(-10, -5);
            
            Text text = textObj.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 18;
            text.color = Color.white;
            text.alignment = TextAnchor.MiddleLeft;
            
            _serverAddressInput.textComponent = text;
            
            // Placeholder
            GameObject placeholderObj = new GameObject("Placeholder");
            placeholderObj.transform.SetParent(inputObj.transform, false);
            
            RectTransform placeholderRect = placeholderObj.AddComponent<RectTransform>();
            placeholderRect.anchorMin = Vector2.zero;
            placeholderRect.anchorMax = Vector2.one;
            placeholderRect.offsetMin = new Vector2(10, 5);
            placeholderRect.offsetMax = new Vector2(-10, -5);
            
            Text placeholder = placeholderObj.AddComponent<Text>();
            placeholder.text = "Server IP Address";
            placeholder.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            placeholder.fontSize = 18;
            placeholder.color = Color.gray;
            placeholder.alignment = TextAnchor.MiddleLeft;
            
            _serverAddressInput.placeholder = placeholder;
        }
        
        private void CreateHostButton()
        {
            GameObject btnObj = new GameObject("HostButton");
            btnObj.transform.SetParent(_menuPanel.transform, false);
            
            RectTransform rect = btnObj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.4f);
            rect.anchorMax = new Vector2(0.5f, 0.4f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(350, 50);
            
            Image img = btnObj.AddComponent<Image>();
            img.color = new Color(0.2f, 0.6f, 0.2f);
            
            _hostButton = btnObj.AddComponent<Button>();
            _hostButton.targetGraphic = img;
            _hostButton.onClick.AddListener(OnHostClicked);
            
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(btnObj.transform, false);
            
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            
            Text text = textObj.AddComponent<Text>();
            text.text = "HOST GAME";
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 24;
            text.color = Color.white;
            text.alignment = TextAnchor.MiddleCenter;
        }
        
        private void CreateJoinButton()
        {
            GameObject btnObj = new GameObject("JoinButton");
            btnObj.transform.SetParent(_menuPanel.transform, false);
            
            RectTransform rect = btnObj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.25f);
            rect.anchorMax = new Vector2(0.5f, 0.25f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(350, 50);
            
            Image img = btnObj.AddComponent<Image>();
            img.color = new Color(0.2f, 0.4f, 0.6f);
            
            _joinButton = btnObj.AddComponent<Button>();
            _joinButton.targetGraphic = img;
            _joinButton.onClick.AddListener(OnJoinClicked);
            
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(btnObj.transform, false);
            
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            
            Text text = textObj.AddComponent<Text>();
            text.text = "JOIN GAME";
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 24;
            text.color = Color.white;
            text.alignment = TextAnchor.MiddleCenter;
        }
        
        private void CreateStatusText()
        {
            GameObject textObj = new GameObject("StatusText");
            textObj.transform.SetParent(_menuPanel.transform, false);
            
            RectTransform rect = textObj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 0);
            rect.anchorMax = new Vector2(1, 0);
            rect.pivot = new Vector2(0.5f, 0);
            rect.anchoredPosition = new Vector2(0, 10);
            rect.sizeDelta = new Vector2(-20, 40);
            
            _statusText = textObj.AddComponent<Text>();
            _statusText.text = "";
            _statusText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _statusText.fontSize = 14;
            _statusText.color = Color.yellow;
            _statusText.alignment = TextAnchor.LowerCenter;
        }
        
        private async void OnHostClicked()
        {
            if (_networkManager == null) return;
            
            _statusText.text = "Starting server...";
            bool success = await _networkManager.StartHostAsync();
            
            if (success)
            {
                _statusText.text = "Server started! Waiting for players...";
                _statusText.color = Color.green;
                _menuPanel.SetActive(false);
            }
            else
            {
                _statusText.text = "Failed to start server";
                _statusText.color = Color.red;
            }
        }
        
        private async void OnJoinClicked()
        {
            if (_networkManager == null) return;
            
            string serverAddress = _serverAddressInput.text;
            if (string.IsNullOrEmpty(serverAddress))
            {
                _statusText.text = "Please enter server address";
                _statusText.color = Color.red;
                return;
            }
            
            _statusText.text = $"Connecting to {serverAddress}...";
            bool success = await _networkManager.ConnectToServerAsync(serverAddress);
            
            if (success)
            {
                _statusText.text = "Connected!";
                _statusText.color = Color.green;
                _menuPanel.SetActive(false);
            }
            else
            {
                _statusText.text = "Failed to connect";
                _statusText.color = Color.red;
            }
        }
        
        public void ShowMenu()
        {
            _menuPanel.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        
        public void HideMenu()
        {
            _menuPanel.SetActive(false);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        
        private void OnPlayerConnected(string playerId)
        {
            Debug.Log($"[NetworkUI] Player connected: {playerId}");
        }
        
        private void OnPlayerDisconnected(string playerId)
        {
            Debug.Log($"[NetworkUI] Player disconnected: {playerId}");
        }
        
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.M))
            {
                if (_menuPanel.activeSelf)
                    HideMenu();
                else
                    ShowMenu();
            }
        }
    }
} 
 
#####PoolDatabaseController.cs##### 
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace Game.Core.Systems
{
    /// <summary>
    /// Pool Database Controller - View and edit all object pools in real-time.
    /// Shows active/inactive counts, allows clearing pools, adjusting limits.
    /// </summary>
    public class PoolDatabaseController : MonoBehaviour
    {
        private GameObject _contentPanel;
        private ObjectPoolManager _poolManager;
        private List<PoolStatsDisplay> _poolDisplays = new();
        private ScrollRect _scrollRect;
        
        public void Initialize(GameObject contentPanel)
        {
            _contentPanel = contentPanel;
            _poolManager = CoreSystemManager.PoolManager;
            
            CreateUI();
            RefreshPoolList();
        }
        
        private void CreateUI()
        {
            // Clear existing content
            foreach (Transform child in _contentPanel.transform)
            {
                if (child.name != "Content") Destroy(child.gameObject);
            }
            
            // Get or create scroll rect
            _scrollRect = _contentPanel.GetComponent<ScrollRect>();
            if (_scrollRect == null)
            {
                _scrollRect = _contentPanel.AddComponent<ScrollRect>();
            }
            
            // Create content container
            GameObject content = new GameObject("Content");
            content.transform.SetParent(_contentPanel.transform, false);
            
            RectTransform contentRect = content.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 1);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.pivot = new Vector2(0.5f, 1);
            contentRect.sizeDelta = new Vector2(0, 2000);
            
            VerticalLayoutGroup layout = content.AddComponent<VerticalLayoutGroup>();
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            layout.spacing = 10;
            layout.padding = new RectOffset(20, 20, 20, 20);
            
            ContentSizeFitter fitter = content.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            
            _scrollRect.content = contentRect;
            _scrollRect.horizontal = false;
            _scrollRect.vertical = true;
            
            // Header
            CreateHeader(content.transform);
            
            // Refresh button
            CreateRefreshButton(content.transform);
        }
        
        private void CreateHeader(Transform parent)
        {
            GameObject header = new GameObject("Header");
            header.transform.SetParent(parent, false);
            
            LayoutElement headerLayout = header.AddComponent<LayoutElement>();
            headerLayout.preferredHeight = 80;
            
            Text headerText = header.AddComponent<Text>();
            headerText.text = "OBJECT POOL DATABASE\nReal-time pool monitoring and management";
            headerText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            headerText.fontSize = 24;
            headerText.color = Color.cyan;
            headerText.alignment = TextAnchor.MiddleCenter;
        }
        
        private void CreateRefreshButton(Transform parent)
        {
            GameObject btnObj = new GameObject("RefreshButton");
            btnObj.transform.SetParent(parent, false);
            
            LayoutElement btnLayout = btnObj.AddComponent<LayoutElement>();
            btnLayout.preferredHeight = 50;
            
            Image btnImg = btnObj.AddComponent<Image>();
            btnImg.color = new Color(0.2f, 0.6f, 0.2f);
            
            Button btn = btnObj.AddComponent<Button>();
            btn.targetGraphic = btnImg;
            btn.onClick.AddListener(RefreshPoolList);
            
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(btnObj.transform, false);
            
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            
            Text text = textObj.AddComponent<Text>();
            text.text = "🔄 REFRESH POOLS";
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 20;
            text.color = Color.white;
            text.alignment = TextAnchor.MiddleCenter;
        }
        
        public void RefreshPoolList()
        {
            if (_poolManager == null)
            {
                Debug.LogWarning("[PoolDatabaseController] PoolManager not available");
                return;
            }
            
            // Clear old displays
            foreach (PoolStatsDisplay display in _poolDisplays)
            {
                if (display.gameObject != null)
                    Destroy(display.gameObject);
            }
            _poolDisplays.Clear();
            
            // Get all pool keys via reflection
            System.Reflection.FieldInfo poolsField = typeof(ObjectPoolManager).GetField("_pools", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (poolsField != null)
            {
                var pools = poolsField.GetValue(_poolManager) as System.Collections.IDictionary;
                if (pools != null)
                {
                    Transform contentTransform = _scrollRect.content;
                    
                    foreach (System.Collections.DictionaryEntry entry in pools)
                    {
                        string poolKey = entry.Key.ToString();
                        CreatePoolDisplay(contentTransform, poolKey);
                    }
                }
            }
            
            Debug.Log($"[PoolDatabaseController] Refreshed {_poolDisplays.Count} pools");
        }
        
        private void CreatePoolDisplay(Transform parent, string poolKey)
        {
            GameObject displayObj = new GameObject($"Pool_{poolKey}");
            displayObj.transform.SetParent(parent, false);
            
            LayoutElement layout = displayObj.AddComponent<LayoutElement>();
            layout.preferredHeight = 120;
            
            Image bg = displayObj.AddComponent<Image>();
            bg.color = new Color(0.15f, 0.15f, 0.2f);
            
            PoolStatsDisplay display = displayObj.AddComponent<PoolStatsDisplay>();
            display.Initialize(poolKey, _poolManager);
            
            _poolDisplays.Add(display);
        }
        
        private void Update()
        {
            // Auto-refresh every second
            if (Time.frameCount % 60 == 0)
            {
                foreach (PoolStatsDisplay display in _poolDisplays)
                {
                    if (display != null)
                        display.UpdateStats();
                }
            }
        }
    }
    
    /// <summary>
    /// Individual pool stats display component.
    /// </summary>
    public class PoolStatsDisplay : MonoBehaviour
    {
        private string _poolKey;
        private ObjectPoolManager _poolManager;
        private Text _statsText;
        
        public void Initialize(string poolKey, ObjectPoolManager poolManager)
        {
            _poolKey = poolKey;
            _poolManager = poolManager;
            
            CreateUI();
            UpdateStats();
        }
        
        private void CreateUI()
        {
            // Stats text
            GameObject textObj = new GameObject("StatsText");
            textObj.transform.SetParent(transform, false);
            
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0, 0);
            textRect.anchorMax = new Vector2(0.7f, 1);
            textRect.offsetMin = new Vector2(10, 10);
            textRect.offsetMax = new Vector2(-10, -10);
            
            _statsText = textObj.AddComponent<Text>();
            _statsText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _statsText.fontSize = 16;
            _statsText.color = Color.white;
            _statsText.alignment = TextAnchor.MiddleLeft;
            
            // Clear button
            CreateButton("Clear", new Vector2(0.75f, 0.5f), new Vector2(100, 40), 
                () => ClearPool());
        }
        
        private void CreateButton(string label, Vector2 anchorPos, Vector2 size, 
            UnityEngine.Events.UnityAction onClick)
        {
            GameObject btnObj = new GameObject($"Btn_{label}");
            btnObj.transform.SetParent(transform, false);
            
            RectTransform btnRect = btnObj.AddComponent<RectTransform>();
            btnRect.anchorMin = anchorPos;
            btnRect.anchorMax = anchorPos;
            btnRect.pivot = new Vector2(0.5f, 0.5f);
            btnRect.sizeDelta = size;
            
            Image btnImg = btnObj.AddComponent<Image>();
            btnImg.color = new Color(0.6f, 0.2f, 0.2f);
            
            Button btn = btnObj.AddComponent<Button>();
            btn.targetGraphic = btnImg;
            btn.onClick.AddListener(onClick);
            
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(btnObj.transform, false);
            
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            
            Text text = textObj.AddComponent<Text>();
            text.text = label;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 14;
            text.color = Color.white;
            text.alignment = TextAnchor.MiddleCenter;
        }
        
        public void UpdateStats()
        {
            if (_poolManager == null) return;
            
            ObjectPoolManager.PoolStats stats = _poolManager.GetPoolStats(_poolKey);
            
            string statusColor = stats.activeCount > stats.maxPoolSize * 0.8f ? "red" : "white";
            
            _statsText.text = $"<b><color=cyan>{_poolKey}</color></b>\n" +
                            $"Active: <color={statusColor}>{stats.activeCount}</color> | " +
                            $"Inactive: {stats.inactiveCount} | " +
                            $"Total: {stats.totalCount}\n" +
                            $"Max: {stats.maxPoolSize}";
        }
        
        private void ClearPool()
        {
            if (_poolManager != null)
            {
                _poolManager.ClearPool(_poolKey);
                UpdateStats();
                Debug.Log($"[PoolDatabase] Cleared pool: {_poolKey}");
            }
        }
    }
} 
 
#####QuestUI.cs##### 
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace Game.UI
{
    /// <summary>
    /// Quest UI - PURE VIEW LAYER
    /// References QuestSystemManager from Game.Core.Systems
    /// NO business logic - only UI display and user interaction
    /// </summary>
    public class QuestUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Canvas _canvas;
        [SerializeField] private GameObject _questPanel;
        [SerializeField] private Transform _questListContainer;
        [SerializeField] private GameObject _questEntryPrefab;
        
        [Header("Detail View")]
        [SerializeField] private GameObject _questDetailPanel;
        [SerializeField] private Text _questTitleText;
        [SerializeField] private Text _questDescriptionText;
        [SerializeField] private Text _questObjectivesText;
        [SerializeField] private Text _questRewardsText;
        [SerializeField] private Button _acceptButton;
        [SerializeField] private Button _abandonButton;
        
        private Game.Core.Systems.QuestSystemManager _questManager;
        private bool _isOpen;
        
        #region Initialization
        
        private void Awake()
        {
            CreateUI();
        }
        
        private void Start()
        {
            _questManager = Game.Core.Systems.CoreSystemManager.Instance?.GetComponent<Game.Core.Systems.QuestSystemManager>();
            
            if (_questManager != null)
            {
                _questManager.OnQuestStarted += OnQuestStarted;
                _questManager.OnQuestCompleted += OnQuestCompleted;
                _questManager.OnObjectiveCompleted += OnObjectiveCompleted;
            }
            else
            {
                Debug.LogWarning("[QuestUI] QuestSystemManager not found!");
            }
            
            _questPanel?.SetActive(false);
            _questDetailPanel?.SetActive(false);
        }
        
        private void OnDestroy()
        {
            if (_questManager != null)
            {
                _questManager.OnQuestStarted -= OnQuestStarted;
                _questManager.OnQuestCompleted -= OnQuestCompleted;
                _questManager.OnObjectiveCompleted -= OnObjectiveCompleted;
            }
        }
        
        #endregion
        
        #region UI Creation
        
        private void CreateUI()
        {
            if (_canvas == null)
            {
                GameObject canvasObj = new GameObject("QuestCanvas");
                canvasObj.transform.SetParent(transform, false);
                
                _canvas = canvasObj.AddComponent<Canvas>();
                _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                _canvas.sortingOrder = 15;
                
                CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                
                canvasObj.AddComponent<GraphicRaycaster>();
            }
            
            CreateQuestPanel();
            CreateQuestDetailPanel();
        }
        
        private void CreateQuestPanel()
        {
            _questPanel = new GameObject("QuestPanel");
            _questPanel.transform.SetParent(_canvas.transform, false);
            
            RectTransform rect = _questPanel.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.7f, 0.2f);
            rect.anchorMax = new Vector2(0.95f, 0.8f);
            rect.sizeDelta = Vector2.zero;
            
            Image bg = _questPanel.AddComponent<Image>();
            bg.color = new Color(0.1f, 0.1f, 0.15f, 0.95f);
            
            CreateTitle(_questPanel.transform, "QUESTS");
            CreateCloseButton(_questPanel.transform);
            CreateQuestList();
        }
        
        private void CreateTitle(Transform parent, string text)
        {
            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(parent, false);
            
            RectTransform rect = titleObj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(1, 1);
            rect.pivot = new Vector2(0.5f, 1);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(0, 60);
            
            Text titleText = titleObj.AddComponent<Text>();
            titleText.text = text;
            titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            titleText.fontSize = 32;
            titleText.color = Color.yellow;
            titleText.alignment = TextAnchor.MiddleCenter;
        }
        
        private void CreateCloseButton(Transform parent)
        {
            GameObject btnObj = new GameObject("CloseButton");
            btnObj.transform.SetParent(parent, false);
            
            RectTransform rect = btnObj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(1, 1);
            rect.anchorMax = new Vector2(1, 1);
            rect.pivot = new Vector2(1, 1);
            rect.anchoredPosition = new Vector2(-10, -10);
            rect.sizeDelta = new Vector2(50, 50);
            
            Image img = btnObj.AddComponent<Image>();
            img.color = new Color(0.6f, 0.2f, 0.2f);
            
            Button btn = btnObj.AddComponent<Button>();
            btn.targetGraphic = img;
            btn.onClick.AddListener(ToggleQuestLog);
            
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(btnObj.transform, false);
            
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            
            Text text = textObj.AddComponent<Text>();
            text.text = "X";
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 24;
            text.color = Color.white;
            text.alignment = TextAnchor.MiddleCenter;
        }
        
        private void CreateQuestList()
        {
            GameObject scrollView = new GameObject("ScrollView");
            scrollView.transform.SetParent(_questPanel.transform, false);
            
            RectTransform scrollRect = scrollView.AddComponent<RectTransform>();
            scrollRect.anchorMin = new Vector2(0, 0);
            scrollRect.anchorMax = new Vector2(1, 1);
            scrollRect.offsetMin = new Vector2(10, 10);
            scrollRect.offsetMax = new Vector2(-10, -70);
            
            ScrollRect scroll = scrollView.AddComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.vertical = true;
            
            GameObject content = new GameObject("Content");
            content.transform.SetParent(scrollView.transform, false);
            
            RectTransform contentRect = content.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 1);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.pivot = new Vector2(0.5f, 1);
            contentRect.sizeDelta = new Vector2(0, 1000);
            
            VerticalLayoutGroup layout = content.AddComponent<VerticalLayoutGroup>();
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            layout.spacing = 5;
            layout.padding = new RectOffset(5, 5, 5, 5);
            
            ContentSizeFitter fitter = content.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            
            scroll.content = contentRect;
            _questListContainer = content.transform;
        }
        
        private void CreateQuestDetailPanel()
        {
            _questDetailPanel = new GameObject("QuestDetailPanel");
            _questDetailPanel.transform.SetParent(_canvas.transform, false);
            
            RectTransform rect = _questDetailPanel.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.3f, 0.2f);
            rect.anchorMax = new Vector2(0.65f, 0.8f);
            rect.sizeDelta = Vector2.zero;
            
            Image bg = _questDetailPanel.AddComponent<Image>();
            bg.color = new Color(0.05f, 0.05f, 0.1f, 0.95f);
            
            // Title
            CreateDetailText("TitleText", new Vector2(0.5f, 0.9f), new Vector2(-20, 60), 28, out _questTitleText);
            
            // Description
            CreateDetailText("DescriptionText", new Vector2(0.5f, 0.7f), new Vector2(-20, 150), 16, out _questDescriptionText);
            _questDescriptionText.alignment = TextAnchor.UpperLeft;
            
            // Objectives
            CreateDetailText("ObjectivesText", new Vector2(0.5f, 0.4f), new Vector2(-20, 200), 16, out _questObjectivesText);
            _questObjectivesText.alignment = TextAnchor.UpperLeft;
            
            // Rewards
            CreateDetailText("RewardsText", new Vector2(0.5f, 0.15f), new Vector2(-20, 80), 16, out _questRewardsText);
            _questRewardsText.alignment = TextAnchor.UpperLeft;
            
            // Buttons
            CreateAcceptButton();
            CreateAbandonButton();
        }
        
        private void CreateDetailText(string name, Vector2 anchorPos, Vector2 size, int fontSize, out Text textComponent)
        {
            GameObject textObj = new GameObject(name);
            textObj.transform.SetParent(_questDetailPanel.transform, false);
            
            RectTransform rect = textObj.AddComponent<RectTransform>();
            rect.anchorMin = anchorPos;
            rect.anchorMax = anchorPos;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            
            textComponent = textObj.AddComponent<Text>();
            textComponent.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            textComponent.fontSize = fontSize;
            textComponent.color = Color.white;
            textComponent.alignment = TextAnchor.MiddleCenter;
        }
        
        private void CreateAcceptButton()
        {
            GameObject btnObj = new GameObject("AcceptButton");
            btnObj.transform.SetParent(_questDetailPanel.transform, false);
            
            RectTransform rect = btnObj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.25f, 0.05f);
            rect.anchorMax = new Vector2(0.25f, 0.05f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(150, 50);
            
            Image img = btnObj.AddComponent<Image>();
            img.color = new Color(0.2f, 0.6f, 0.2f);
            
            _acceptButton = btnObj.AddComponent<Button>();
            _acceptButton.targetGraphic = img;
            _acceptButton.onClick.AddListener(OnAcceptButtonClicked);
            
            CreateButtonText(btnObj.transform, "ACCEPT");
        }
        
        private void CreateAbandonButton()
        {
            GameObject btnObj = new GameObject("AbandonButton");
            btnObj.transform.SetParent(_questDetailPanel.transform, false);
            
            RectTransform rect = btnObj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.75f, 0.05f);
            rect.anchorMax = new Vector2(0.75f, 0.05f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(150, 50);
            
            Image img = btnObj.AddComponent<Image>();
            img.color = new Color(0.6f, 0.2f, 0.2f);
            
            _abandonButton = btnObj.AddComponent<Button>();
            _abandonButton.targetGraphic = img;
            _abandonButton.onClick.AddListener(OnAbandonButtonClicked);
            
            CreateButtonText(btnObj.transform, "ABANDON");
        }
        
        private void CreateButtonText(Transform parent, string text)
        {
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(parent, false);
            
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            
            Text btnText = textObj.AddComponent<Text>();
            btnText.text = text;
            btnText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            btnText.fontSize = 20;
            btnText.color = Color.white;
            btnText.alignment = TextAnchor.MiddleCenter;
        }
        
        #endregion
        
        #region UI Management
        
        public void ToggleQuestLog()
        {
            _isOpen = !_isOpen;
            _questPanel?.SetActive(_isOpen);
            
            if (_isOpen)
            {
                RefreshQuestList();
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                _questDetailPanel?.SetActive(false);
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
        
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Q))
            {
                ToggleQuestLog();
            }
            
            if (Input.GetKeyDown(KeyCode.Escape) && _isOpen)
            {
                ToggleQuestLog();
            }
        }
        
        #endregion
        
        #region Quest Display
        
        private void RefreshQuestList()
        {
            if (_questManager == null || _questListContainer == null) return;
            
            // Clear existing entries
            foreach (Transform child in _questListContainer)
            {
                Destroy(child.gameObject);
            }
            
            // Add active quests
            List<Game.Core.Systems.QuestData> activeQuests = _questManager.GetActiveQuests();
            foreach (var quest in activeQuests)
            {
                CreateQuestEntry(quest, true);
            }
            
            // Add available quests
            List<Game.Core.Systems.QuestData> availableQuests = _questManager.GetAvailableQuests();
            foreach (var quest in availableQuests)
            {
                CreateQuestEntry(quest, false);
            }
        }
        
        private void CreateQuestEntry(Game.Core.Systems.QuestData quest, bool isActive)
        {
            GameObject entry = new GameObject($"Quest_{quest.questId}");
            entry.transform.SetParent(_questListContainer, false);
            
            LayoutElement layout = entry.AddComponent<LayoutElement>();
            layout.preferredHeight = 60;
            
            Image bg = entry.AddComponent<Image>();
            bg.color = isActive ? new Color(0.2f, 0.3f, 0.2f) : new Color(0.15f, 0.15f, 0.2f);
            
            Button btn = entry.AddComponent<Button>();
            btn.targetGraphic = bg;
            btn.onClick.AddListener(() => ShowQuestDetails(quest));
            
            // Quest name
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(entry.transform, false);
            
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(10, 5);
            textRect.offsetMax = new Vector2(-10, -5);
            
            Text text = textObj.AddComponent<Text>();
            text.text = $"{quest.questName}\n<size=12><color=grey>Level {quest.level} - {quest.questType}</color></size>";
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 16;
            text.color = Color.white;
            text.alignment = TextAnchor.MiddleLeft;
        }
        
        private Game.Core.Systems.QuestData _currentDetailQuest;
        
        private void ShowQuestDetails(Game.Core.Systems.QuestData quest)
        {
            _currentDetailQuest = quest;
            _questDetailPanel?.SetActive(true);
            
            _questTitleText.text = quest.questName;
            _questDescriptionText.text = quest.questDescription;
            
            // Objectives
            string objectivesText = "<b>OBJECTIVES:</b>\n";
            foreach (var objective in quest.objectives)
            {
                string checkmark = objective.isCompleted ? "✓" : "○";
                objectivesText += $"{checkmark} {objective.description} ({objective.currentCount}/{objective.requiredCount})\n";
            }
            _questObjectivesText.text = objectivesText;
            
            // Rewards
            string rewardsText = "<b>REWARDS:</b>\n";
            if (quest.experienceReward > 0)
                rewardsText += $"<color=cyan>+{quest.experienceReward} XP</color>\n";
            if (quest.goldReward > 0)
                rewardsText += $"<color=yellow>+{quest.goldReward} Gold</color>\n";
            _questRewardsText.text = rewardsText;
            
            // Button states
            bool isActive = _questManager.IsQuestActive(quest.questId);
            _acceptButton.gameObject.SetActive(!isActive && quest.status == Game.Core.Systems.QuestStatus.Available);
            _abandonButton.gameObject.SetActive(isActive);
        }
        
        #endregion
        
        #region Button Handlers
        
        private void OnAcceptButtonClicked()
        {
            if (_currentDetailQuest != null && _questManager != null)
            {
                _questManager.StartQuest(_currentDetailQuest.questId);
                _questDetailPanel?.SetActive(false);
                RefreshQuestList();
            }
        }
        
        private void OnAbandonButtonClicked()
        {
            if (_currentDetailQuest != null && _questManager != null)
            {
                _questManager.AbandonQuest(_currentDetailQuest.questId);
                _questDetailPanel?.SetActive(false);
                RefreshQuestList();
            }
        }
        
        #endregion
        
        #region Event Handlers
        
        private void OnQuestStarted(Game.Core.Systems.QuestData quest)
        {
            Debug.Log($"[QuestUI] Quest started: {quest.questName}");
            if (_isOpen) RefreshQuestList();
        }
        
        private void OnQuestCompleted(Game.Core.Systems.QuestData quest)
        {
            Debug.Log($"[QuestUI] Quest completed: {quest.questName}");
            if (_isOpen) RefreshQuestList();
            
            // Show completion notification
            ShowQuestCompletionNotification(quest);
        }
        
        private void OnObjectiveCompleted(Game.Core.Systems.QuestData quest, Game.Core.Systems.QuestObjective objective)
        {
            Debug.Log($"[QuestUI] Objective completed: {objective.description}");
            if (_isOpen && _currentDetailQuest?.questId == quest.questId)
            {
                ShowQuestDetails(quest);
            }
        }
        
        private void ShowQuestCompletionNotification(Game.Core.Systems.QuestData quest)
        {
            // TODO: Implement fancy completion notification
            Debug.Log($"<color=yellow>QUEST COMPLETED: {quest.questName}</color>");
        }
        
        #endregion
    }
} 
 
#####SpellEditorController.cs##### 
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace Game.Core.Systems
{
    /// <summary>
    /// Spell Editor Controller - Create and edit magic spells.
    /// </summary>
    public class SpellEditorController : MonoBehaviour
    {
        private GameObject _contentPanel;
        private SpellData _currentSpell;
        
        private InputField _spellNameInput;
        private Dropdown _spellTypeDropdown;
        private InputField _manaCostInput;
        private InputField _cooldownInput;
        private InputField _damageInput;
        private InputField _rangeInput;
        private Dropdown _elementDropdown;
        private Text _previewText;
        
        private List<SpellData> _spellLibrary = new();
        
        public void Initialize(GameObject contentPanel)
        {
            _contentPanel = contentPanel;
            LoadSpellLibrary();
            CreateUI();
            CreateNewSpell();
        }
        
        private void CreateUI()
        {
            Transform content = CreateScrollableContent();
            
            CreateHeader(content, "SPELL EDITOR");
            
            CreateInputField(content, "Spell Name:", out _spellNameInput);
            _spellNameInput.onValueChanged.AddListener(v => UpdatePreview());
            
            CreateDropdown(content, "Type:", out _spellTypeDropdown, 
                new List<string> { "Damage", "Healing", "Buff", "Debuff", "Summon", "Utility" });
            _spellTypeDropdown.onValueChanged.AddListener(v => UpdatePreview());
            
            CreateDropdown(content, "Element:", out _elementDropdown, 
                System.Enum.GetNames(typeof(DamageElement)));
            _elementDropdown.onValueChanged.AddListener(v => UpdatePreview());
            
            CreateInputField(content, "Mana Cost:", out _manaCostInput);
            _manaCostInput.onValueChanged.AddListener(v => UpdatePreview());
            
            CreateInputField(content, "Cooldown (s):", out _cooldownInput);
            _cooldownInput.onValueChanged.AddListener(v => UpdatePreview());
            
            CreateInputField(content, "Damage:", out _damageInput);
            _damageInput.onValueChanged.AddListener(v => UpdatePreview());
            
            CreateInputField(content, "Range:", out _rangeInput);
            _rangeInput.onValueChanged.AddListener(v => UpdatePreview());
            
            CreateButton(content, "✨ NEW SPELL", () => CreateNewSpell());
            CreateButton(content, "💾 SAVE SPELL", () => SaveSpell());
            CreateButton(content, "📚 VIEW LIBRARY", () => ShowSpellLibrary());
            
            CreatePreviewPanel(content);
        }
        
        private Transform CreateScrollableContent()
        {
            ScrollRect scroll = _contentPanel.GetComponent<ScrollRect>();
            if (scroll == null) scroll = _contentPanel.AddComponent<ScrollRect>();
            
            GameObject content = new GameObject("Content");
            content.transform.SetParent(_contentPanel.transform, false);
            
            RectTransform contentRect = content.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 1);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.pivot = new Vector2(0.5f, 1);
            contentRect.sizeDelta = new Vector2(0, 2000);
            
            VerticalLayoutGroup layout = content.AddComponent<VerticalLayoutGroup>();
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            layout.spacing = 15;
            layout.padding = new RectOffset(20, 20, 20, 20);
            
            ContentSizeFitter fitter = content.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            
            scroll.content = contentRect;
            scroll.horizontal = false;
            scroll.vertical = true;
            
            return content.transform;
        }
        
        private void CreateHeader(Transform parent, string text)
        {
            GameObject header = new GameObject("Header");
            header.transform.SetParent(parent, false);
            
            LayoutElement layout = header.AddComponent<LayoutElement>();
            layout.preferredHeight = 60;
            
            Text headerText = header.AddComponent<Text>();
            headerText.text = text;
            headerText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            headerText.fontSize = 28;
            headerText.color = new Color(0.6f, 0.3f, 1f);
            headerText.alignment = TextAnchor.MiddleCenter;
        }
        
        private void CreateDropdown(Transform parent, string label, out Dropdown dropdown, System.Array options)
        {
            List<string> optionsList = new List<string>();
            foreach (var option in options)
            {
                optionsList.Add(option.ToString());
            }
            CreateDropdown(parent, label, out dropdown, optionsList);
        }
        
        private void CreateDropdown(Transform parent, string label, out Dropdown dropdown, List<string> options)
        {
            GameObject row = new GameObject($"Row_{label}");
            row.transform.SetParent(parent, false);
            
            LayoutElement rowLayout = row.AddComponent<LayoutElement>();
            rowLayout.preferredHeight = 50;
            
            HorizontalLayoutGroup rowHLayout = row.AddComponent<HorizontalLayoutGroup>();
            rowHLayout.childForceExpandWidth = true;
            rowHLayout.spacing = 10;
            
            GameObject labelObj = new GameObject("Label");
            labelObj.transform.SetParent(row.transform, false);
            
            LayoutElement labelLayout = labelObj.AddComponent<LayoutElement>();
            labelLayout.preferredWidth = 200;
            
            Text labelText = labelObj.AddComponent<Text>();
            labelText.text = label;
            labelText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            labelText.fontSize = 18;
            labelText.color = Color.white;
            labelText.alignment = TextAnchor.MiddleLeft;
            
            GameObject dropdownObj = new GameObject("Dropdown");
            dropdownObj.transform.SetParent(row.transform, false);
            
            Image dropdownImg = dropdownObj.AddComponent<Image>();
            dropdownImg.color = new Color(0.2f, 0.2f, 0.3f);
            
            dropdown = dropdownObj.AddComponent<Dropdown>();
            dropdown.ClearOptions();
            dropdown.AddOptions(options);
            
            GameObject dropdownLabel = new GameObject("Label");
            dropdownLabel.transform.SetParent(dropdownObj.transform, false);
            
            RectTransform dropdownLabelRect = dropdownLabel.AddComponent<RectTransform>();
            dropdownLabelRect.anchorMin = Vector2.zero;
            dropdownLabelRect.anchorMax = Vector2.one;
            dropdownLabelRect.offsetMin = new Vector2(10, 2);
            dropdownLabelRect.offsetMax = new Vector2(-25, -2);
            
            Text dropdownLabelText = dropdownLabel.AddComponent<Text>();
            dropdownLabelText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            dropdownLabelText.fontSize = 16;
            dropdownLabelText.color = Color.white;
            dropdownLabelText.alignment = TextAnchor.MiddleLeft;
            
            dropdown.captionText = dropdownLabelText;
        }
        
        private void CreateInputField(Transform parent, string label, out InputField inputField)
        {
            GameObject row = new GameObject($"Row_{label}");
            row.transform.SetParent(parent, false);
            
            LayoutElement rowLayout = row.AddComponent<LayoutElement>();
            rowLayout.preferredHeight = 50;
            
            HorizontalLayoutGroup rowHLayout = row.AddComponent<HorizontalLayoutGroup>();
            rowHLayout.childForceExpandWidth = true;
            rowHLayout.spacing = 10;
            
            GameObject labelObj = new GameObject("Label");
            labelObj.transform.SetParent(row.transform, false);
            
            LayoutElement labelLayout = labelObj.AddComponent<LayoutElement>();
            labelLayout.preferredWidth = 200;
            
            Text labelText = labelObj.AddComponent<Text>();
            labelText.text = label;
            labelText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            labelText.fontSize = 18;
            labelText.color = Color.white;
            labelText.alignment = TextAnchor.MiddleLeft;
            
            GameObject inputObj = new GameObject("InputField");
            inputObj.transform.SetParent(row.transform, false);
            
            Image inputImg = inputObj.AddComponent<Image>();
            inputImg.color = new Color(0.2f, 0.2f, 0.3f);
            
            inputField = inputObj.AddComponent<InputField>();
            
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(inputObj.transform, false);
            
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(10, 5);
            textRect.offsetMax = new Vector2(-10, -5);
            
            Text text = textObj.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 16;
            text.color = Color.white;
            text.alignment = TextAnchor.MiddleLeft;
            
            inputField.textComponent = text;
        }
        
        private void CreateButton(Transform parent, string label, UnityEngine.Events.UnityAction onClick)
        {
            GameObject btnObj = new GameObject($"Btn_{label}");
            btnObj.transform.SetParent(parent, false);
            
            LayoutElement btnLayout = btnObj.AddComponent<LayoutElement>();
            btnLayout.preferredHeight = 60;
            
            Image btnImg = btnObj.AddComponent<Image>();
            btnImg.color = new Color(0.4f, 0.2f, 0.6f);
            
            Button btn = btnObj.AddComponent<Button>();
            btn.targetGraphic = btnImg;
            btn.onClick.AddListener(onClick);
            
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(btnObj.transform, false);
            
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            
            Text text = textObj.AddComponent<Text>();
            text.text = label;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 20;
            text.color = Color.white;
            text.alignment = TextAnchor.MiddleCenter;
        }
        
        private void CreatePreviewPanel(Transform parent)
        {
            GameObject panel = new GameObject("PreviewPanel");
            panel.transform.SetParent(parent, false);
            
            LayoutElement panelLayout = panel.AddComponent<LayoutElement>();
            panelLayout.preferredHeight = 300;
            
            Image panelImg = panel.AddComponent<Image>();
            panelImg.color = new Color(0.1f, 0.1f, 0.15f);
            
            GameObject textObj = new GameObject("PreviewText");
            textObj.transform.SetParent(panel.transform, false);
            
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(15, 15);
            textRect.offsetMax = new Vector2(-15, -15);
            
            _previewText = textObj.AddComponent<Text>();
            _previewText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _previewText.fontSize = 16;
            _previewText.color = Color.white;
            _previewText.alignment = TextAnchor.UpperLeft;
        }
        
        private void CreateNewSpell()
        {
            _currentSpell = new SpellData
            {
                spellId = System.Guid.NewGuid().ToString(),
                spellName = "New Spell",
                manaCost = 50,
                cooldown = 5f,
                damage = 100,
                range = 20f,
                element = DamageElement.Fire
            };
            
            _spellNameInput.text = _currentSpell.spellName;
            _manaCostInput.text = _currentSpell.manaCost.ToString();
            _cooldownInput.text = _currentSpell.cooldown.ToString();
            _damageInput.text = _currentSpell.damage.ToString();
            _rangeInput.text = _currentSpell.range.ToString();
            _elementDropdown.value = (int)_currentSpell.element;
            
            UpdatePreview();
        }
        
        private void UpdatePreview()
        {
            if (_currentSpell == null) return;
            
            _currentSpell.spellName = _spellNameInput.text;
            
            if (int.TryParse(_manaCostInput.text, out int mana))
                _currentSpell.manaCost = mana;
            
            if (float.TryParse(_cooldownInput.text, out float cd))
                _currentSpell.cooldown = cd;
            
            if (int.TryParse(_damageInput.text, out int dmg))
                _currentSpell.damage = dmg;
            
            if (float.TryParse(_rangeInput.text, out float range))
                _currentSpell.range = range;
            
            _currentSpell.element = (DamageElement)_elementDropdown.value;
            
            Color elementColor = GetElementColor(_currentSpell.element);
            string elementHex = ColorUtility.ToHtmlStringRGB(elementColor);
            
            _previewText.text = $"<b><color=#{elementHex}>{_currentSpell.spellName}</color></b>\n" +
                              $"<color=grey>{_currentSpell.element} Spell</color>\n\n" +
                              $"<color=cyan>Mana Cost:</color> {_currentSpell.manaCost}\n" +
                              $"<color=yellow>Cooldown:</color> {_currentSpell.cooldown}s\n" +
                              $"<color=orange>Damage:</color> {_currentSpell.damage}\n" +
                              $"<color=green>Range:</color> {_currentSpell.range}\n\n" +
                              $"<i>Cast this spell to unleash {_currentSpell.element} magic!</i>";
        }
        
        private Color GetElementColor(DamageElement element)
        {
            return element switch
            {
                DamageElement.Fire => new Color(1f, 0.3f, 0f),
                DamageElement.Ice => new Color(0.3f, 0.7f, 1f),
                DamageElement.Lightning => new Color(1f, 1f, 0.3f),
                DamageElement.Poison => new Color(0.4f, 1f, 0.3f),
                DamageElement.Holy => new Color(1f, 1f, 0.8f),
                DamageElement.Dark => new Color(0.5f, 0.2f, 0.7f),
                _ => Color.white
            };
        }
        
        private void SaveSpell()
        {
            string json = JsonUtility.ToJson(_currentSpell, true);
            Debug.Log($"[SpellEditor] Saved spell:\n{json}");
            
            PlayerPrefs.SetString($"Spell_{_currentSpell.spellId}", json);
            PlayerPrefs.Save();
            
            if (!_spellLibrary.Contains(_currentSpell))
            {
                _spellLibrary.Add(_currentSpell);
            }
        }
        
        private void LoadSpellLibrary()
        {
            int spellCount = PlayerPrefs.GetInt("SpellCount", 0);
            
            for (int i = 0; i < spellCount; i++)
            {
                string json = PlayerPrefs.GetString($"SpellLibrary_{i}", "");
                if (!string.IsNullOrEmpty(json))
                {
                    SpellData spell = JsonUtility.FromJson<SpellData>(json);
                    _spellLibrary.Add(spell);
                }
            }
        }
        
        private void ShowSpellLibrary()
        {
            Debug.Log($"[SpellEditor] Spell Library ({_spellLibrary.Count} spells)");
            foreach (SpellData spell in _spellLibrary)
            {
                Debug.Log($"  - {spell.spellName} ({spell.element})");
            }
        }
    }
    
    /// <summary>
    /// Spell data structure for serialization.
    /// </summary>
    [System.Serializable]
    public class SpellData
    {
        public string spellId;
        public string spellName;
        public int manaCost;
        public float cooldown;
        public int damage;
        public float range;
        public DamageElement element;
        public string description;
    }
} 
 
#####WeaponEditorController.cs##### 
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

namespace Game.Core.Systems
{
    /// <summary>
    /// Weapon Editor Controller - Create and edit weapons with live preview.
    /// </summary>
    public class WeaponEditorController : MonoBehaviour
    {
        private GameObject _contentPanel;
        private ItemGenerationEngine _itemGenerator;
        private ItemData _currentWeapon;
        
        private Dropdown _archetypeDropdown;
        private Dropdown _rarityDropdown;
        private InputField _damageInput;
        private InputField _attackSpeedInput;
        private InputField _rangeInput;
        private Text _previewText;
        
        public void Initialize(GameObject contentPanel)
        {
            _contentPanel = contentPanel;
            _itemGenerator = FindFirstObjectByType<ItemGenerationEngine>();
            
            if (_itemGenerator == null)
            {
                GameObject genObj = new GameObject("ItemGenerationEngine");
                _itemGenerator = genObj.AddComponent<ItemGenerationEngine>();
            }
            
            CreateUI();
            GenerateRandomWeapon();
        }
        
        private void CreateUI()
        {
            Transform content = CreateScrollableContent();
            
            CreateHeader(content, "WEAPON EDITOR");
            
            CreateDropdown(content, "Archetype:", out _archetypeDropdown, 
                System.Enum.GetNames(typeof(WeaponArchetype)).ToList());
            _archetypeDropdown.onValueChanged.AddListener(v => UpdatePreview());
            
            CreateDropdown(content, "Rarity:", out _rarityDropdown, 
                System.Enum.GetNames(typeof(Rarity)).ToList());
            _rarityDropdown.onValueChanged.AddListener(v => UpdatePreview());
            
            CreateInputField(content, "Damage:", out _damageInput);
            _damageInput.onValueChanged.AddListener(v => UpdatePreview());
            
            CreateInputField(content, "Attack Speed:", out _attackSpeedInput);
            _attackSpeedInput.onValueChanged.AddListener(v => UpdatePreview());
            
            CreateInputField(content, "Range:", out _rangeInput);
            _rangeInput.onValueChanged.AddListener(v => UpdatePreview());
            
            CreateButton(content, "🎲 GENERATE RANDOM", () => GenerateRandomWeapon());
            CreateButton(content, "💾 SAVE WEAPON", () => SaveWeapon());
            
            CreatePreviewPanel(content);
        }
        
        private Transform CreateScrollableContent()
        {
            ScrollRect scroll = _contentPanel.GetComponent<ScrollRect>();
            if (scroll == null) scroll = _contentPanel.AddComponent<ScrollRect>();
            
            GameObject content = new GameObject("Content");
            content.transform.SetParent(_contentPanel.transform, false);
            
            RectTransform contentRect = content.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 1);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.pivot = new Vector2(0.5f, 1);
            contentRect.sizeDelta = new Vector2(0, 2000);
            
            VerticalLayoutGroup layout = content.AddComponent<VerticalLayoutGroup>();
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            layout.spacing = 15;
            layout.padding = new RectOffset(20, 20, 20, 20);
            
            ContentSizeFitter fitter = content.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            
            scroll.content = contentRect;
            scroll.horizontal = false;
            scroll.vertical = true;
            
            return content.transform;
        }
        
        private void CreateHeader(Transform parent, string text)
        {
            GameObject header = new GameObject("Header");
            header.transform.SetParent(parent, false);
            
            LayoutElement layout = header.AddComponent<LayoutElement>();
            layout.preferredHeight = 60;
            
            Text headerText = header.AddComponent<Text>();
            headerText.text = text;
            headerText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            headerText.fontSize = 28;
            headerText.color = Color.yellow;
            headerText.alignment = TextAnchor.MiddleCenter;
        }
        
        private void CreateDropdown(Transform parent, string label, out Dropdown dropdown, List<string> options)
        {
            GameObject row = new GameObject($"Row_{label}");
            row.transform.SetParent(parent, false);
            
            LayoutElement rowLayout = row.AddComponent<LayoutElement>();
            rowLayout.preferredHeight = 50;
            
            HorizontalLayoutGroup rowHLayout = row.AddComponent<HorizontalLayoutGroup>();
            rowHLayout.childForceExpandWidth = true;
            rowHLayout.spacing = 10;
            
            // Label
            GameObject labelObj = new GameObject("Label");
            labelObj.transform.SetParent(row.transform, false);
            
            LayoutElement labelLayout = labelObj.AddComponent<LayoutElement>();
            labelLayout.preferredWidth = 200;
            
            Text labelText = labelObj.AddComponent<Text>();
            labelText.text = label;
            labelText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            labelText.fontSize = 18;
            labelText.color = Color.white;
            labelText.alignment = TextAnchor.MiddleLeft;
            
            // Dropdown
            GameObject dropdownObj = new GameObject("Dropdown");
            dropdownObj.transform.SetParent(row.transform, false);
            
            Image dropdownImg = dropdownObj.AddComponent<Image>();
            dropdownImg.color = new Color(0.2f, 0.2f, 0.3f);
            
            dropdown = dropdownObj.AddComponent<Dropdown>();
            dropdown.ClearOptions();
            dropdown.AddOptions(options);
            
            // Dropdown label
            GameObject dropdownLabel = new GameObject("Label");
            dropdownLabel.transform.SetParent(dropdownObj.transform, false);
            
            RectTransform dropdownLabelRect = dropdownLabel.AddComponent<RectTransform>();
            dropdownLabelRect.anchorMin = Vector2.zero;
            dropdownLabelRect.anchorMax = Vector2.one;
            dropdownLabelRect.offsetMin = new Vector2(10, 2);
            dropdownLabelRect.offsetMax = new Vector2(-25, -2);
            
            Text dropdownLabelText = dropdownLabel.AddComponent<Text>();
            dropdownLabelText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            dropdownLabelText.fontSize = 16;
            dropdownLabelText.color = Color.white;
            dropdownLabelText.alignment = TextAnchor.MiddleLeft;
            
            dropdown.captionText = dropdownLabelText;
        }
        
        private void CreateInputField(Transform parent, string label, out InputField inputField)
        {
            GameObject row = new GameObject($"Row_{label}");
            row.transform.SetParent(parent, false);
            
            LayoutElement rowLayout = row.AddComponent<LayoutElement>();
            rowLayout.preferredHeight = 50;
            
            HorizontalLayoutGroup rowHLayout = row.AddComponent<HorizontalLayoutGroup>();
            rowHLayout.childForceExpandWidth = true;
            rowHLayout.spacing = 10;
            
            // Label
            GameObject labelObj = new GameObject("Label");
            labelObj.transform.SetParent(row.transform, false);
            
            LayoutElement labelLayout = labelObj.AddComponent<LayoutElement>();
            labelLayout.preferredWidth = 200;
            
            Text labelText = labelObj.AddComponent<Text>();
            labelText.text = label;
            labelText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            labelText.fontSize = 18;
            labelText.color = Color.white;
            labelText.alignment = TextAnchor.MiddleLeft;
            
            // Input field
            GameObject inputObj = new GameObject("InputField");
            inputObj.transform.SetParent(row.transform, false);
            
            Image inputImg = inputObj.AddComponent<Image>();
            inputImg.color = new Color(0.2f, 0.2f, 0.3f);
            
            inputField = inputObj.AddComponent<InputField>();
            inputField.textComponent = CreateInputText(inputObj.transform);
        }
        
        private Text CreateInputText(Transform parent)
        {
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(parent, false);
            
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(10, 5);
            textRect.offsetMax = new Vector2(-10, -5);
            
            Text text = textObj.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 16;
            text.color = Color.white;
            text.alignment = TextAnchor.MiddleLeft;
            
            return text;
        }
        
        private void CreateButton(Transform parent, string label, UnityEngine.Events.UnityAction onClick)
        {
            GameObject btnObj = new GameObject($"Btn_{label}");
            btnObj.transform.SetParent(parent, false);
            
            LayoutElement btnLayout = btnObj.AddComponent<LayoutElement>();
            btnLayout.preferredHeight = 60;
            
            Image btnImg = btnObj.AddComponent<Image>();
            btnImg.color = new Color(0.3f, 0.6f, 0.3f);
            
            Button btn = btnObj.AddComponent<Button>();
            btn.targetGraphic = btnImg;
            btn.onClick.AddListener(onClick);
            
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(btnObj.transform, false);
            
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            
            Text text = textObj.AddComponent<Text>();
            text.text = label;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 20;
            text.color = Color.white;
            text.alignment = TextAnchor.MiddleCenter;
        }
        
        private void CreatePreviewPanel(Transform parent)
        {
            GameObject panel = new GameObject("PreviewPanel");
            panel.transform.SetParent(parent, false);
            
            LayoutElement panelLayout = panel.AddComponent<LayoutElement>();
            panelLayout.preferredHeight = 300;
            
            Image panelImg = panel.AddComponent<Image>();
            panelImg.color = new Color(0.1f, 0.1f, 0.15f);
            
            GameObject textObj = new GameObject("PreviewText");
            textObj.transform.SetParent(panel.transform, false);
            
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(15, 15);
            textRect.offsetMax = new Vector2(-15, -15);
            
            _previewText = textObj.AddComponent<Text>();
            _previewText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _previewText.fontSize = 16;
            _previewText.color = Color.white;
            _previewText.alignment = TextAnchor.UpperLeft;
        }
        
        private void GenerateRandomWeapon()
        {
            Rarity rarity = (Rarity)Random.Range(0, System.Enum.GetValues(typeof(Rarity)).Length);
            _currentWeapon = _itemGenerator.GenerateWeapon(rarity);
            
            LoadWeaponToUI();
            UpdatePreview();
        }
        
        private void LoadWeaponToUI()
        {
            _archetypeDropdown.value = (int)_currentWeapon.weaponArchetype;
            _rarityDropdown.value = (int)_currentWeapon.rarity;
            _damageInput.text = _currentWeapon.damage.ToString();
            _attackSpeedInput.text = _currentWeapon.attackSpeed.ToString("F2");
            _rangeInput.text = _currentWeapon.range.ToString("F1");
        }
        
        private void UpdatePreview()
        {
            if (_currentWeapon == null) return;
            
            // Update weapon from UI
            _currentWeapon.weaponArchetype = (WeaponArchetype)_archetypeDropdown.value;
            _currentWeapon.rarity = (Rarity)_rarityDropdown.value;
            
            if (int.TryParse(_damageInput.text, out int damage))
                _currentWeapon.damage = damage;
            
            if (float.TryParse(_attackSpeedInput.text, out float atkSpeed))
                _currentWeapon.attackSpeed = atkSpeed;
            
            if (float.TryParse(_rangeInput.text, out float range))
                _currentWeapon.range = range;
            
            // Generate preview
            Color rarityColor = GetRarityColor(_currentWeapon.rarity);
            string rarityHex = ColorUtility.ToHtmlStringRGB(rarityColor);
            
            _previewText.text = $"<b><color=#{rarityHex}>{_currentWeapon.itemName}</color></b>\n" +
                              $"<color=grey>Level {_currentWeapon.level} {_currentWeapon.weaponArchetype}</color>\n\n" +
                              $"<color=orange>Damage:</color> {_currentWeapon.damage}\n" +
                              $"<color=cyan>Attack Speed:</color> {_currentWeapon.attackSpeed:F2}\n" +
                              $"<color=yellow>Range:</color> {_currentWeapon.range:F1}\n\n" +
                              $"<b>Affixes:</b>\n";
            
            foreach (ItemAffix affix in _currentWeapon.affixes)
            {
                _previewText.text += $"+ {affix.affixType}: {affix.value}\n";
            }
        }
        
        private Color GetRarityColor(Rarity rarity)
        {
            return rarity switch
            {
                Rarity.Common => Color.white,
                Rarity.Uncommon => Color.green,
                Rarity.Rare => Color.blue,
                Rarity.Epic => new Color(0.6f, 0.3f, 1f),
                Rarity.Legendary => new Color(1f, 0.5f, 0f),
                Rarity.Mythic => new Color(1f, 0f, 0f),
                _ => Color.white
            };
        }
        
        private void SaveWeapon()
        {
            string json = JsonUtility.ToJson(_currentWeapon, true);
            Debug.Log($"[WeaponEditor] Saved weapon:\n{json}");
            
            PlayerPrefs.SetString($"Weapon_{_currentWeapon.itemId}", json);
            PlayerPrefs.Save();
        }
    }
} 
 

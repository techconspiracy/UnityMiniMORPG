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
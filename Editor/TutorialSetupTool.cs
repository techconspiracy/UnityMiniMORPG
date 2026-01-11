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
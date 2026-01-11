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
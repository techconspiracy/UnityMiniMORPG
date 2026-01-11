# UAsset MMORPG System - CORRECTED Installation Guide

## Critical Fix: Object Pool Configuration

### The Issue
The original guide stated "It will automatically create CoreSystemManager as a child" - this is misleading. The CoreSystemManager and its child systems are created at **runtime** during `GameBootstrap.Start()`, not in the editor.

### Corrected Setup Process

#### Step 1: Create Bootstrap Scene (Exact Steps)

1. **File → New Scene**
2. Save as `Assets/Scenes/Bootstrap.unity`
3. **Right-click Hierarchy → Create Empty**
4. Name it: `GameBootstrap` (NOT CoreSystemManager)
5. **Select GameBootstrap GameObject**
6. **Add Component → Game Bootstrap** (the script)
7. Configure in Inspector:
   ```
   Initial Scene Name: MainMenu
   Show Loading Screen: ✓
   Min Loading Time: 1
   Target Frame Rate: 60
   Enable VSync: ✓
   Default Quality Level: 2
   ```
8. **Save scene** (Ctrl+S)

**IMPORTANT**: You will NOT see CoreSystemManager in the hierarchy yet. It creates itself at runtime.

#### Step 2: First Test - Verify Auto-Creation

1. **Press Play** ▶️
2. **Look at Hierarchy** while in Play mode
3. **You should now see**:
   ```
   Hierarchy (Play Mode):
   ├── GameBootstrap
   ├── CoreSystemManager (DontDestroyOnLoad)
   │   ├── ObjectPoolManager
   │   ├── WebSocketNetworkManager
   │   ├── ZoneSystemManager
   │   ├── EntitySystemManager
   │   ├── CombatSystemManager
   │   ├── InventorySystemManager
   │   ├── AudioSystemManager
   │   ├── UISystemManager
   │   └── AdminConsoleManager
   └── LoadingCanvas (temporary, then destroyed)
   ```

4. **Check Console**:
   ```
   [GameBootstrap] Starting game bootstrap...
   [GameBootstrap] Creating CoreSystemManager...
   [CoreSystemManager] Initializing core systems...
   [CoreSystemManager] Created ObjectPoolManager
   [CoreSystemManager] Created WebSocketNetworkManager
   [CoreSystemManager] Created ZoneSystemManager
   ... (9 systems total)
   [ObjectPoolManager] Initialized with 0 pre-warmed pools
   [CoreSystemManager] All systems initialized in 0.XXXs
   [GameBootstrap] Bootstrap complete in X.XXXs
   [GameBootstrap] Scene 'MainMenu' loaded
   ```

5. **Stop Play Mode**
6. **Hierarchy returns to normal** - CoreSystemManager disappears (it's DontDestroyOnLoad)

### Object Pool Configuration (Corrected)

Since you can't configure pools before runtime, here are your options:

#### Option A: Configure Pools in Code (Recommended for Testing)

Create a test script that runs after systems initialize:

```csharp
using UnityEngine;
using Game.Core.Systems;
using Game.Core.Pooling;

public class PoolSetup : MonoBehaviour
{
    [Header("Prefab References")]
    [SerializeField] private GameObject _damageNumberPrefab;
    [SerializeField] private GameObject _projectilePrefab;
    
    async void Start()
    {
        // Wait for CoreSystemManager
        while (CoreSystemManager.Instance == null || !CoreSystemManager.Instance.IsReady())
        {
            await Awaitable.NextFrameAsync();
        }
        
        ObjectPoolManager poolMgr = CoreSystemManager.PoolManager;
        
        // Register and warm pools
        if (_damageNumberPrefab != null)
        {
            poolMgr.RegisterPrefab("DamageNumber", _damageNumberPrefab, maxPoolSize: 100);
            poolMgr.WarmPool("DamageNumber", count: 50);
        }
        
        if (_projectilePrefab != null)
        {
            poolMgr.RegisterPrefab("Projectile", _projectilePrefab, maxPoolSize: 500);
            poolMgr.WarmPool("Projectile", count: 100);
        }
        
        Debug.Log("[PoolSetup] Pools configured!");
    }
}
```

**Usage**:
1. Create empty GameObject in MainMenu scene: "PoolSetup"
2. Add `PoolSetup` script
3. Assign your prefabs in Inspector
4. Press Play - pools configure automatically

#### Option B: Use CoreSystemManager's Pre-Warm List (Advanced)

Modify `ObjectPoolManager.cs` to expose configuration in Inspector:

**In ObjectPoolManager.Awake()**, the `_preWarmPools` list CAN be configured, but you need to:

1. **Create a ScriptableObject** for pool configurations:

```csharp
using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "PoolConfig", menuName = "Game/Pool Configuration")]
public class PoolConfigAsset : ScriptableObject
{
    public List<PoolEntry> pools = new();
    
    [System.Serializable]
    public class PoolEntry
    {
        public string poolKey;
        public GameObject prefab;
        public int initialSize = 10;
        public int maxPoolSize = 100;
    }
}
```

2. **Modify ObjectPoolManager** to load from ScriptableObject:

```csharp
// Add to ObjectPoolManager class
[Header("Pool Configuration Asset")]
[SerializeField] private PoolConfigAsset _poolConfigAsset;

private void InitializePools()
{
    // ... existing code ...
    
    // Load from ScriptableObject if assigned
    if (_poolConfigAsset != null)
    {
        foreach (var entry in _poolConfigAsset.pools)
        {
            if (entry.prefab != null)
            {
                RegisterPrefab(entry.poolKey, entry.prefab, entry.maxPoolSize);
                WarmPool(entry.poolKey, entry.initialSize);
            }
        }
    }
    
    // ... rest of existing code ...
}
```

3. **Create the asset**:
   - Right-click in Project → Create → Game → Pool Configuration
   - Name it "DefaultPoolConfig"
   - Add your pool entries

4. **Assign in GameBootstrap**:
   - You'd need to modify GameBootstrap to pass this to ObjectPoolManager
   - Or better: Make ObjectPoolManager find it via Resources.Load

### The Real Fix: Updated GameBootstrap

Here's the corrected `GameBootstrap.cs` that properly exposes pool configuration:

```csharp
using UnityEngine;
using UnityEngine.SceneManagement;
using Game.Core.Systems;

namespace Game.Core
{
    public class GameBootstrap : MonoBehaviour
    {
        [Header("Bootstrap Configuration")]
        [SerializeField] private string _initialSceneName = "MainMenu";
        [SerializeField] private bool _showLoadingScreen = true;
        [SerializeField] private float _minLoadingTime = 1f;
        
        [Header("Performance Settings")]
        [SerializeField] private int _targetFrameRate = 60;
        [SerializeField] private bool _enableVSync = true;
        
        [Header("Quality Settings")]
        [SerializeField] private int _defaultQualityLevel = 2;
        
        [Header("Pool Configuration (Optional)")]
        [SerializeField] private PoolConfigAsset _poolConfig;
        
        [Header("Runtime References (Auto-Assigned)")]
        [SerializeField] private CoreSystemManager _coreSystemManager;
        [SerializeField] private Canvas _loadingCanvas;
        
        private bool _isBootstrapped;
        
        private async void Start()
        {
            if (_isBootstrapped) return;
            await BootstrapGame();
        }
        
        private async Awaitable BootstrapGame()
        {
            float startTime = Time.realtimeSinceStartup;
            Debug.Log("[GameBootstrap] Starting game bootstrap...");
            
            ApplyPerformanceSettings();
            
            if (_showLoadingScreen)
                ShowLoadingScreen();
            
            await EnsureCoreSystemManager();
            
            // Configure pools AFTER CoreSystemManager exists
            if (_poolConfig != null)
            {
                ConfigurePools();
            }
            
            float elapsed = Time.realtimeSinceStartup - startTime;
            if (elapsed < _minLoadingTime)
            {
                await Awaitable.WaitForSecondsAsync(_minLoadingTime - elapsed);
            }
            
            await LoadInitialScene();
            
            if (_showLoadingScreen)
                HideLoadingScreen();
            
            _isBootstrapped = true;
            Debug.Log($"[GameBootstrap] Bootstrap complete in {Time.realtimeSinceStartup - startTime:F3}s");
        }
        
        private void ConfigurePools()
        {
            ObjectPoolManager poolMgr = CoreSystemManager.PoolManager;
            if (poolMgr == null)
            {
                Debug.LogError("[GameBootstrap] Cannot configure pools - ObjectPoolManager not found!");
                return;
            }
            
            foreach (var entry in _poolConfig.pools)
            {
                if (entry.prefab != null)
                {
                    poolMgr.RegisterPrefab(entry.poolKey, entry.prefab, entry.maxPoolSize);
                    poolMgr.WarmPool(entry.poolKey, entry.initialSize);
                }
            }
            
            Debug.Log($"[GameBootstrap] Configured {_poolConfig.pools.Count} pools");
        }
        
        // ... rest of existing methods (ApplyPerformanceSettings, EnsureCoreSystemManager, etc.)
    }
}
```

## Updated Verification Steps

### Test 1: Systems Auto-Create

1. Open Bootstrap scene
2. Press Play
3. **While in Play mode**, expand CoreSystemManager in Hierarchy
4. Verify all 9 child systems exist
5. Check Console for initialization logs
6. Stop Play mode
7. CoreSystemManager disappears (correct behavior)

### Test 2: Pool Configuration Works

**If using Option A (Code-based)**:
1. Add PoolSetup script to MainMenu scene
2. Assign prefabs
3. Press Play
4. Check Console: "[PoolSetup] Pools configured!"
5. Press F12 → Tab 1 (Pool Database)
6. Should see your registered pools

**If using Option B (ScriptableObject)**:
1. Create PoolConfigAsset
2. Assign to GameBootstrap._poolConfig
3. Press Play
4. Check Console: "[GameBootstrap] Configured X pools"

### Test 3: Pools Actually Work

```csharp
// Add to any script in MainMenu scene
public class PoolTest : MonoBehaviour
{
    async void Start()
    {
        while (!CoreSystemManager.Instance.IsReady())
            await Awaitable.NextFrameAsync();
        
        ObjectPoolManager pools = CoreSystemManager.PoolManager;
        
        // Get pool stats
        var stats = pools.GetPoolStats("DamageNumber");
        Debug.Log($"DamageNumber pool: {stats.totalCount} total, {stats.activeCount} active");
        
        // Try to get an object
        var dmgNum = pools.Get<DamageNumber>("DamageNumber", Vector3.zero, Quaternion.identity);
        if (dmgNum != null)
        {
            Debug.Log("✓ Successfully got object from pool!");
            pools.ReturnToPool(dmgNum);
        }
    }
}
```

## Common Issues - FIXED

### Issue: "CoreSystemManager not visible in Bootstrap scene"
**Fix**: This is correct! It only appears during Play mode. Don't try to manually add it.

### Issue: "Cannot configure ObjectPoolManager before Play"
**Fix**: Use Option A (PoolSetup script) or Option B (ScriptableObject asset).

### Issue: "ObjectPoolManager.Instance is null"
**Fix**: Always wait for `CoreSystemManager.IsReady()` before accessing any manager:
```csharp
while (CoreSystemManager.Instance == null || !CoreSystemManager.Instance.IsReady())
{
    await Awaitable.NextFrameAsync();
}

// NOW safe to use
ObjectPoolManager poolMgr = CoreSystemManager.PoolManager;
```

## Recommended Workflow

1. **Setup Bootstrap scene** (GameBootstrap only)
2. **Create MainMenu scene** (empty for now)
3. **Press Play** to verify auto-creation works
4. **Stop Play**
5. **Create PoolSetup script** in MainMenu scene
6. **Assign prefabs** to PoolSetup
7. **Press Play** again
8. **Verify pools** via F12 → Pool Database tab

This workflow avoids the confusion of trying to configure things that don't exist yet in Edit mode.
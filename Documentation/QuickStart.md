# UAsset MMORPG System - Quick Start Guide

**Get up and running in 10 minutes!**

## Prerequisites
- Unity 6.1.0f1 or later
- Basic Unity knowledge
- 15 minutes of your time

---

## Step 1: Create Project (2 minutes)

1. Open **Unity Hub**
2. Click **New Project**
3. Select **3D Core** template
4. Name: `UAssetMMORPG`
5. Click **Create Project**

---

## Step 2: Install Scripts (3 minutes)

### Copy All Scripts

Create this folder structure:
```
Assets/
└── Scripts/
    └── Game/
        └── Core/
            ├── Pooling/
            └── Systems/
```

**Copy these scripts into respective folders:**

**Pooling/** folder:
- `IPoolable.cs`

**Systems/** folder:
- `GameBootstrap.cs`
- `CoreSystemManager.cs`
- `ObjectPoolManager.cs`
- `ZoneSceneManager.cs`
- `SimpleTerrainGenerator.cs`
- `InteractableRegistry.cs`
- `ProceduralCharacterBuilder.cs`
- `ItemGenerationEngine.cs`
- `CombatSystem.cs`
- `UISystem.cs`
- `AdminConsoleManager.cs`
- `WebSocketNetworkManager.cs`

**Wait for compilation** (check bottom-right status bar)

---

## Step 3: Create Bootstrap Scene (2 minutes)

1. **File → New Scene**
2. Save as `Assets/Scenes/Bootstrap.unity`
3. **Right-click Hierarchy → Create Empty**
4. Name it: `GameBootstrap`
5. **Add Component → GameBootstrap**
6. Configure in Inspector:
   ```
   Initial Scene Name: MainMenu
   Show Loading Screen: ✓
   Target Frame Rate: 60
   Enable VSync: ✓
   Default Quality Level: 2
   ```
7. **Save scene** (Ctrl+S)

---

## Step 4: Create Main Menu Scene (1 minute)

1. **File → New Scene**
2. Save as `Assets/Scenes/MainMenu.unity`
3. **Right-click Hierarchy → UI → Canvas**
4. **Right-click Canvas → UI → Text**
5. Set text: `"PRESS SPACE TO START"`
6. **Save scene**

---

## Step 5: Configure Build Settings (1 minute)

1. **File → Build Settings**
2. **Add Open Scenes** (with Bootstrap open)
3. Switch to MainMenu, **Add Open Scenes**
4. **Drag Bootstrap** to top (index 0)
5. Click **Close**

---

## Step 6: First Test (1 minute)

1. **Open Bootstrap scene**
2. **Press Play** ▶️

**Expected Result:**
```
[GameBootstrap] Starting game bootstrap...
[CoreSystemManager] Initializing core systems...
[ObjectPoolManager] Initialized with 0 pre-warmed pools
[GameBootstrap] Bootstrap complete in 0.123s
```

**✓ If you see this, installation successful!**

---

## Next Steps

### Test Core Systems

**Press F12** while in Play mode:
- Admin Console should open
- Time pauses
- Seven tabs visible

**If this works**, you're ready to:

1. **Read USER_MANUAL.md** - Learn gameplay features
2. **Read API_DOCUMENTATION.md** - Extend the system
3. **Run EXAMPLES.md** - Test all systems

---

## Common Quick Start Issues

### Issue: Scripts won't compile

**Fix:**
```
1. Check namespace: All scripts should use "Game.Core.Systems"
2. Verify Unity version: Must be 6.1+
3. Check Package Manager: Install Burst, Collections, UI packages
```

### Issue: Bootstrap scene doesn't transition

**Fix:**
```
1. Verify Initial Scene Name: "MainMenu" (exact match)
2. Check Build Settings: Both scenes added, Bootstrap is index 0
3. Look at Console: Check for error messages
```

### Issue: Admin console doesn't open

**Fix:**
```
1. Wait 2-3 seconds after scene loads
2. Make sure you're in Play mode
3. Press F12 (not Fn+F12)
4. Check Console for initialization errors
```

---

## Verification Checklist

- [ ] Unity 6.1+ installed
- [ ] All 12 scripts copied and compiled
- [ ] Bootstrap scene created with GameBootstrap component
- [ ] MainMenu scene created
- [ ] Build Settings configured (Bootstrap at index 0)
- [ ] Test Play: No console errors
- [ ] F12 opens admin console

**If all checkboxes are checked, you're ready to build!**

---

## What You Have Now

✓ **Core Systems**: Object pooling, zone management, networking  
✓ **Content Generation**: Procedural items, terrain, characters  
✓ **Combat System**: Damage calculations, abilities, stats  
✓ **UI Framework**: HUD, menus, admin console  
✓ **Admin Tools**: F12 console for live editing

---

## First Meaningful Test

Want to see something cool? Run this test:

1. **Create new scene**: `FirstTest.unity`
2. **Create empty GameObject**: "TestController"
3. **Add this script**:

```csharp
using UnityEngine;
using Game.Core.Systems;

public class FirstTest : MonoBehaviour
{
    async void Start()
    {
        // Wait for systems
        while (CoreSystemManager.Instance == null || !CoreSystemManager.Instance.IsReady())
        {
            await Awaitable.NextFrameAsync();
        }
        
        // Generate a zone
        ZoneConfig config = new()
        {
            zoneName = "MyFirstZone",
            zoneType = ZoneType.Wilderness,
            biomeType = BiomeType.Grassland,
            zoneSize = new Vector3(50, 0, 50),
            seed = 12345
        };
        
        await CoreSystemManager.ZoneManager.GenerateAndSaveZone(config);
        
        Debug.Log("Your first zone is ready! Look around!");
    }
}
```

4. **Press Play**
5. **You'll see**: Rolling green hills appear!

---

## Learning Path

1. ✓ **You are here** - Quick Start (Done!)
2. **Next**: Run Example 1 from EXAMPLES.md
3. **Then**: Read full USER_MANUAL.md
4. **Finally**: Build your game with API_DOCUMENTATION.md

---

## Get Help

**Problems?** Check these in order:

1. **Console Window** (Ctrl+Shift+C) - Error messages?
2. **Inspector** - Component properly attached?
3. **Build Settings** - Scenes in correct order?
4. **INSTALLATION_GUIDE.md** - Full troubleshooting steps

---

## Time Investment Summary

```
Quick Start:        10 minutes (you just did this!)
Full Installation:  30 minutes (INSTALLATION_GUIDE.md)
Learn System:       2 hours (USER_MANUAL.md)
Master API:         4 hours (API_DOCUMENTATION.md)
Build Game:         ∞ hours (your creativity!)
```

---

**Congratulations! Your MMORPG framework is ready. Now go build something amazing! 🚀**
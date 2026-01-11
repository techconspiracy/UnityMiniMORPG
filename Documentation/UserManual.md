# UAsset MMORPG System - User Manual

## Table of Contents
1. [Getting Started](#getting-started)
2. [Character Creation](#character-creation)
3. [Core Gameplay](#core-gameplay)
4. [Combat System](#combat-system)
5. [Inventory & Equipment](#inventory--equipment)
6. [Abilities & Magic](#abilities--magic)
7. [Admin Console](#admin-console)
8. [Multiplayer](#multiplayer)

---

## Getting Started

### First Launch

1. **Start the game** from Bootstrap scene
2. **Main Menu loads** automatically
3. **Press SPACE** to begin (temporary - full character creation coming)
4. **Tutorial prompt** (if implemented) will guide you

### Controls Overview

#### Movement (CoD-style)
```
W/A/S/D     - Move forward/left/back/right
Mouse       - Look around
Space       - Jump
Shift       - Sprint
Ctrl        - Crouch
```

#### Combat
```
Left Click  - Primary attack
Right Click - Secondary attack / ADS
R           - Reload (ranged weapons)
1-9, 0      - Ability hotbar
```

#### UI & Menus
```
I           - Toggle Inventory
C           - Toggle Character Sheet
K           - Toggle Spellbook
M           - Toggle Map
Esc         - Open Menu
F12         - Admin Console (host only)
```

#### Interaction
```
E           - Interact with objects
    - Open doors
    - Open chests
    - Talk to NPCs
    - Climb ladders
    - Pick up items
```

---

## Character Creation

### Step 1: Species Selection

Choose from 10 species, each with unique traits:

#### Human
- **Passive**: Adaptable (+10% XP gain)
- **Active**: Second Wind (Heal 25% HP, 5min cooldown)
- **Stats**: Balanced across all attributes

#### Elf
- **Passive**: Keen Senses (+20% vision range)
- **Active**: Nature's Step (Invisible in forests, 3min cooldown)
- **Stats**: High DEX, high INT, low STR

#### Dwarf
- **Passive**: Stone Skin (+15% armor)
- **Active**: War Cry (Taunt enemies, 2min cooldown)
- **Stats**: High VIT, high STR, low DEX

#### Orc
- **Passive**: Berserker Rage (+20% damage below 30% HP)
- **Active**: Blood Fury (+50% damage, 10sec, 5min cooldown)
- **Stats**: Very high STR, low INT

#### Lizardfolk
- **Passive**: Poison Resistance (50% poison resist, +10 natural armor)
- **Active**: Tail Sweep (360° knockback, 30sec cooldown)
- **Stats**: High VIT, balanced

#### Android
- **Passive**: Overcharge (+30% tech weapon damage)
- **Active**: System Reboot (Cleanse all debuffs, 10min cooldown)
- **Stats**: High INT, no natural armor

#### Cyborg
- **Passive**: Mechanical Enhancement (+10% all stats)
- **Active**: Emergency Repair (Heal 50% HP over 10sec)
- **Stats**: Balanced hybrid

#### Alien
- **Passive**: Psionic Attunement (+20% magic damage)
- **Active**: Telepathy (Read NPC disposition, no cooldown)
- **Stats**: Very high INT, low STR

#### Demon
- **Passive**: Dark Pact (Heal 5% of damage dealt)
- **Active**: Hellfire (AOE fire damage, 1min cooldown)
- **Stats**: High INT, high STR, negative holy resist

#### Angel
- **Passive**: Divine Blessing (+10% heal effectiveness)
- **Active**: Holy Smite (Single target holy damage, 45sec cooldown)
- **Stats**: High INT, high VIT, negative dark resist

### Step 2: Gender & Appearance

- **Gender**: Male / Female
- **Body Type**: Slim (-10% HP, +10% movement) / Average / Muscular (+10% HP, -5% movement)
- **Skin Tone**: 3-5 options per species
- **Face Shape**: 5 presets
- **Hair Style**: 10+ options

### Step 3: Attribute Distribution

Start with **20 points** to distribute:
```
Strength (STR)     - Melee damage, carry weight
Dexterity (DEX)    - Ranged damage, dodge, attack speed
Intelligence (INT) - Magic power, mana pool
Vitality (VIT)     - Health pool, health regen
Endurance (END)    - Stamina pool, stamina regen
Luck (LUCK)        - Critical chance, loot quality
```

**Minimum per stat**: 5  
**Maximum per stat**: 15 (at creation)

**Recommended Builds**:
- **Warrior**: STR 15, VIT 15, END 10
- **Archer**: DEX 15, STR 10, END 10, LUCK 5
- **Mage**: INT 15, VIT 10, END 10
- **Tank**: VIT 15, END 15, STR 10
- **Hybrid**: All stats at 10 (balanced)

### Step 4: Heroic Start Bonus

Choose **one**:

1. **Legendary Weapon**: Rare starting weapon (+50% damage, special ability)
2. **Ancient Armor**: Full rare armor set (+100 armor, set bonus)
3. **Spellbook**: 3 starting spells (instead of 0-1)
4. **Companion**: Pet/summon that fights alongside you
5. **Gold Hoard**: 5000 starting gold (normal start is 100)

---

## Core Gameplay

### Exploration

#### Zones
- **Towns**: Safe areas with NPCs, shops, quests
- **Dungeons**: Combat-focused, loot-heavy, boss encounters
- **Wilderness**: Open exploration, resource gathering, wandering enemies
- **Arenas**: PvP instances (multiplayer)

#### Zone Transitions
- Walk to **zone edges** to see portal prompt
- Press **E** to confirm transition
- Loading screen (brief)
- Spawn at designated **Player Spawn Point**

#### Interactables

**Doors**:
- Press **E** to open/close
- Some require keys (check inventory)
- Locked doors show red prompt

**Chests**:
- Press **E** to loot
- Contains 2-5 random items
- Loot quality scales with zone level
- One-time loot (per respawn)

**Ladders**:
- Approach ladder
- Press **E** to start climbing
- W/S to climb up/down
- Space to jump off

**NPCs**:
- Press **E** to talk
- Dialogue options appear
- Quest givers have **!** icon
- Shopkeepers have **$** icon

### Quests & Objectives

#### Quest Types
1. **Kill Quests**: Defeat X enemies
2. **Collection Quests**: Gather X items
3. **Delivery Quests**: Bring item to NPC
4. **Exploration Quests**: Discover new zones
5. **Boss Quests**: Defeat named enemy

#### Quest UI
- Press **M** to open quest log
- Active quest tracked on HUD (top-center)
- Objectives update in real-time
- Turn in at quest giver for rewards

### Experience & Leveling

#### Gaining XP
- **Kill enemies**: XP scales with enemy level
- **Complete quests**: Flat XP reward
- **Discover zones**: One-time XP bonus
- **Loot rare items**: Small XP bonus

#### Level Up Benefits
- **+2 to all primary stats** (STR, DEX, INT, VIT, END)
- **+1 to LUCK**
- **Full HP/Mana/Stamina restore**
- **Skill point** (for ability unlocks)

#### Level Scaling
```
Level 1-10:   Early game (100-300 HP, 20-50 damage)
Level 11-25:  Mid game (500-1000 HP, 80-150 damage)
Level 26-50:  Late game (2000-5000 HP, 300-800 damage)
Level 51-100: End game (10000+ HP, 2000+ damage)
```

---

## Combat System

### Damage Types

**Physical**: Melee weapons, scaled by STR  
**Ranged**: Bows, guns, scaled by DEX  
**Magic**: Spells, scaled by INT

### Damage Calculation

```
Final Damage = Base Damage 
             × (1 + Attacker Stat / 100)
             × Crit Multiplier (if crit)
             × Elemental Multiplier
             × (1 - Armor Reduction)
```

**Armor Reduction Formula**:
```
Reduction % = Armor / (Armor + 100)
```
- 50 armor = 33% reduction
- 100 armor = 50% reduction
- 200 armor = 67% reduction
- 500 armor = 83% reduction (diminishing returns)

### Elemental System

**Elements**: Fire, Ice, Lightning, Poison, Holy, Dark

**Resistances**:
- Each entity has 0-100% resistance per element
- 0% = full damage
- 50% = half damage
- 100% = immune
- Negative values = vulnerable (takes extra damage)

**Examples**:
- Fire Demon: 80% fire resist, -50% ice resist
- Ice Elemental: 100% ice resist, -80% fire resist
- Undead: -50% holy resist, 50% dark resist

### Critical Hits

- **Base Crit Chance**: 5% (LUCK affects this)
- **Crit Multiplier**: 2x damage (default)
- **Crit displayed**: Yellow damage numbers with "!"

### Status Effects

**Burning**: 5% max HP per second, 5 seconds  
**Frozen**: Movement speed -50%, 3 seconds  
**Shocked**: Cannot use abilities, 2 seconds  
**Poisoned**: 3% max HP per second, 10 seconds  
**Blessed**: +20% healing received, 30 seconds  
**Cursed**: -20% all damage dealt, 30 seconds

---

## Inventory & Equipment

### Inventory System

Press **I** to open inventory (D&D-style grid)

#### Inventory Grid
- **10×10 slots** (100 total)
- Items stack (up to 99)
- Drag to move items
- Right-click for context menu:
  - Equip
  - Use
  - Drop
  - Destroy

#### Filters
- **All Items** (default)
- **Weapons**
- **Armor**
- **Consumables**
- **Materials**
- **Quest Items**

#### Sorting
- **By Name** (alphabetical)
- **By Rarity** (mythic → common)
- **By Level** (high → low)
- **By Recent** (newest first)

### Equipment Slots

#### Weapon Slots
- **Main Hand**: Primary weapon
- **Off Hand**: Shield, secondary weapon, or spell focus

#### Armor Slots
- **Head**: Helmets, hats, crowns
- **Chest**: Body armor, robes, vests
- **Hands**: Gloves, gauntlets, bracers
- **Legs**: Pants, greaves, robes (legs)
- **Feet**: Boots, shoes, greaves (feet)

#### Accessory Slots
- **Ring 1**: Stat bonuses, abilities
- **Ring 2**: Stat bonuses, abilities
- **Amulet**: Powerful bonuses, set pieces

### Item Rarity

**Visual Indicators**:
```
Common     - White text, white beam (5m)
Uncommon   - Green text, green beam (7m)
Rare       - Blue text, blue beam (10m)
Epic       - Purple text, purple beam (15m), glow
Legendary  - Orange text, orange beam (20m), sparkles
Mythic     - Red text, red beam (30m), lightning
```

**Stat Scaling**:
- Common: 1.0x base stats
- Uncommon: 1.3x base stats
- Rare: 1.7x base stats
- Epic: 2.2x base stats
- Legendary: 3.0x base stats
- Mythic: 4.5x base stats

### Item Affixes

Items can have **prefixes** and **suffixes**:

**Prefixes** (damage-focused):
- **+Damage**: Flat damage bonus
- **Flaming**: Adds fire damage
- **Frozen**: Adds ice damage
- **Shocking**: Adds lightning damage

**Suffixes** (defensive/utility):
- **of Health**: +HP bonus
- **of Mana**: +Mana bonus
- **of Resistance**: +elemental resist
- **of Speed**: +movement/attack speed

**Affix Count by Rarity**:
- Common: 0 affixes
- Uncommon: 1 affix
- Rare: 2 affixes
- Epic: 3 affixes
- Legendary: 4 affixes
- Mythic: 6 affixes

### Loot System

#### Drop Sources

**Enemy Kills**:
- 80% drop rate
- 1-3 items per kill
- Rarity based on enemy level/type

**Chests**:
- 100% drop rate
- 2-5 items per chest
- Rarity +1 tier vs enemy drops

**Bosses**:
- 100% drop rate
- 5-10 items
- Guaranteed 1+ epic/legendary
- Rarity +2 tiers

#### Auto-Pickup
- Press **E** near item to pick up
- Items go directly to inventory
- If inventory full, item stays on ground
- Items disappear after 5 minutes

---

## Abilities & Magic

### Ability System

Press **K** to open Spellbook

#### Ability Sources

1. **Equipment Abilities**: Granted by equipped items
2. **Learned Spells**: From spellbooks, trainers, quests
3. **Species Abilities**: Racial passives/actives
4. **Class Abilities**: Unlocked via skill tree (future)

#### Hotbar Slots

**10 slots** (keys 1-9, 0):
- Drag abilities from Spellbook
- Cooldowns displayed on icons
- Mana/stamina costs shown
- Can swap anytime (out of combat)

### Resource Management

**Mana** (blue bar):
- Used for **magic abilities**
- Base: 100 + (INT × 10)
- Regen: 5% per second (out of combat)

**Stamina** (green bar):
- Used for **physical abilities**
- Base: 100 + (END × 10)
- Regen: 5% per second (out of combat)

**Cooldowns**:
- Per-ability timers
- Global cooldown: 0.5sec (all abilities)
- Can't spam abilities

### Spell Types

#### Direct Damage
- **Fireball**: 100 fire damage, 3sec cooldown, 20 mana
- **Lightning Bolt**: 80 lightning damage, 2sec cooldown, 15 mana
- **Ice Shard**: 60 ice damage + slow, 4sec cooldown, 18 mana

#### Area of Effect (AOE)
- **Flame Wave**: 150 fire damage, 5m radius, 10sec cooldown, 40 mana
- **Blizzard**: 200 ice damage over time, 10m radius, 20sec cooldown, 60 mana

#### Buffs
- **Strength Potion**: +30% STR, 60sec, 5sec cooldown, 25 mana
- **Haste**: +50% attack speed, 30sec, 30sec cooldown, 30 mana
- **Shield**: +100 armor, 20sec, 15sec cooldown, 35 mana

#### Heals
- **Heal**: Restore 100 HP, 5sec cooldown, 30 mana
- **Regeneration**: Restore 50 HP/sec over 10sec, 15sec cooldown, 50 mana

#### Movement
- **Dash**: Teleport 10m forward, 8sec cooldown, 20 stamina
- **Blink**: Teleport to cursor, 15sec cooldown, 40 mana

### Ability Combos

**Example Combo**:
1. Cast **Shield** (+100 armor)
2. **Dash** into enemy pack
3. Cast **Flame Wave** (AOE damage)
4. Finish stragglers with **Fireball**

---

## Admin Console

**Access**: Press **F12** (host/single-player only)

### Console Tabs

Press **1-7** to switch tabs:

#### 1. Pool Database
- View all object pools
- See active/inactive counts
- Performance metrics (hits/misses)
- Force clear pools
- Warm/grow specific pools

**Use Case**: Monitor memory usage, debug pooling issues

#### 2. Weapon Editor
- Search existing weapons
- Create new weapons
- Edit weapon stats:
  - Damage
  - Attack speed
  - Range
  - Element
  - Affixes
- Real-time mesh preview
- Spawn weapon in world
- Save to pool

**Use Case**: Create custom weapons, balance testing

#### 3. Armor Editor
- Browse armor pieces
- Edit armor stats:
  - Armor value
  - Resistances
  - Bonus stats
  - Abilities
- Preview on character model
- Spawn in world
- Save to pool

**Use Case**: Create custom armor sets, testing builds

#### 4. Spell Editor
- Create new spells
- Edit spell properties:
  - Damage/heal amount
  - AOE radius
  - Cooldown
  - Mana/stamina cost
  - Visual effects
- Test spell immediately
- Add to spellbook
- Grant to player

**Use Case**: Create custom spells, ability balancing

#### 5. Entity Inspector
- List all entities in zone
- Select to inspect
- Edit entity stats:
  - HP/Mana/Stamina
  - Damage
  - Level
  - AI behavior
- Teleport entity
- Delete entity
- Spawn new entity

**Use Case**: Debug AI, balance encounters, testing

#### 6. Zone Editor
- Current zone info
- Regenerate terrain
- Place/remove interactables:
  - Doors
  - Chests
  - NPCs
  - Spawn points
- Edit spawn point settings
- Adjust lighting
- Save zone changes

**Use Case**: Level design, zone balancing

#### 7. Player Management (Multiplayer)
- View all connected players
- Select player
- Grant items/gold/XP
- Teleport player
- Edit player stats
- Kick/ban player

**Use Case**: GM tools, server administration

### Admin Commands

Console also supports text commands (future):
```
/give [item] [amount]       - Give item to self
/spawn [enemy] [count]      - Spawn enemies
/tp [x] [y] [z]             - Teleport to coordinates
/setlevel [level]           - Set player level
/godmode                    - Toggle invincibility
```

---

## Multiplayer

### LAN Hosting

#### Starting a Server (Host)

1. **Start game** normally
2. **Character creation** completes
3. **Press F1** to open network menu (future feature)
4. **Click "Host Server"**
5. **Configure**:
   - Port: 7777 (default)
   - Max Players: 6 (max)
   - Password: (optional)
6. **Click "Start"**
7. **Share your local IP** with friends

**Finding your IP**:
- Windows: `ipconfig` in cmd
- Mac/Linux: `ifconfig` in terminal
- Look for IPv4 address (e.g., 192.168.1.100)

#### Joining a Server (Client)

1. **Start game**
2. **Character creation**
3. **Press F1**
4. **Click "Join Server"**
5. **Enter host IP**: 192.168.1.100:7777
6. **Enter password** (if required)
7. **Click "Connect"**

### Multiplayer Features

#### Instanced Loot
- Each player sees their own loot
- No loot stealing
- Loot quality per-player (based on their stats/luck)

#### Client-Authoritative Movement
- Smooth, responsive movement (CoD-style)
- No input lag
- Predictive client-side

#### Server-Authoritative Combat
- Damage calculated on server
- Prevents cheating
- Synced to all clients

#### Shared World State
- Enemies visible to all players
- Cooperative combat
- Boss encounters scale with player count

### Multiplayer Limitations

- **Max 6 players** per instance
- **LAN only** (WebSocket)
- **Host must stay online** (no dedicated server)
- **Progress saves to host** only

---

## Tips & Strategies

### Early Game (Level 1-10)

1. **Prioritize VIT** for survivability
2. **Complete all town quests** for easy XP
3. **Loot everything** - even commons sell for gold
4. **Avoid boss zones** until level 10+
5. **Use consumables liberally** - they're cheap and common

### Mid Game (Level 11-25)

1. **Focus on one damage stat** (STR, DEX, or INT)
2. **Farm dungeons** for rare+ loot
3. **Join multiplayer** groups for tough content
4. **Save legendary items** - don't sell them
5. **Experiment with builds** - respecs are possible

### Late Game (Level 26-50)

1. **Optimize gear** for set bonuses
2. **Farm specific bosses** for targeted loot
3. **Master ability combos** for max DPS
4. **Help low-level players** for karma (XP bonus)
5. **Save gold** for crafting (future feature)

### End Game (Level 51-100)

1. **Min-max stats** via gear
2. **Hunt mythic items** (0.1% drop rate)
3. **Compete in arena** (PvP)
4. **Challenge yourself** - solo bosses
5. **Help community** - host servers, share strategies

---

## Hotkeys Reference

```
Movement:
W/A/S/D     - Move
Mouse       - Look
Space       - Jump
Shift       - Sprint
Ctrl        - Crouch

Combat:
LMB         - Attack
RMB         - Alt attack / ADS
R           - Reload
1-9, 0      - Abilities

UI:
I           - Inventory
C           - Character
K           - Spellbook
M           - Map
Esc         - Menu

Admin:
F12         - Console
1-7         - Console tabs

Interaction:
E           - Interact
F           - Quick loot (hold)
Tab         - Toggle HUD

Multiplayer:
F1          - Network menu
T           - Chat (type)
Enter       - Send chat
```

---

## FAQ

**Q: How do I save my progress?**  
A: Auto-save occurs every 5 minutes and on zone transitions. Manual save via Esc → Save Game.

**Q: Can I respec my attributes?**  
A: Yes, find a "Trainer" NPC in towns (costs gold, scales with level).

**Q: How do I craft items?**  
A: Crafting system coming in future update. For now, loot is the primary source.

**Q: What's the level cap?**  
A: Level 100 (current). May increase in future.

**Q: Can I play offline?**  
A: Yes, single-player mode works entirely offline.

**Q: How do I report bugs?**  
A: Use Admin Console → Press F12 → Bug Report tab (future feature).

**Q: Where's the best farming spot?**  
A: Dungeons level 20-30 have highest legendary drop rates.

**Q: Can I mod the game?**  
A: Yes! See API_DOCUMENTATION.md for extending systems.

---

**For technical details, see API_DOCUMENTATION.md**  
**For examples, see EXAMPLES.md**
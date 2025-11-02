# TurboTrip - World & Level Selection System

## 📋 Overview

This system allows players to choose from multiple worlds, each containing multiple levels. Progress is automatically saved using PlayerPrefs.

## 🏗️ Architecture

### Data Layer
- **WorldData** (ScriptableObject): Defines a world with name, description, icon, color, and array of levels
- **LevelData** (ScriptableObject): Defines a level with name, scene name, target time, icon, unlock requirements

### Management Layer
- **GameProgressManager** (Singleton): Tracks unlocked worlds/levels, completion status, best times. Persists across scenes.

### UI Layer
- **WorldSelectionUI**: Manages world selection screen, generates world buttons dynamically
- **LevelSelectionUI**: Manages level selection screen for chosen world
- **WorldButton**: Individual world button component
- **LevelButton**: Individual level button component

### Integration
- **LevelTimer**: Tracks time in gameplay levels
- **LoadingScreen/FinalScene**: Shows completion screen and saves progress

---

## 🚀 Setup Instructions

### Step 1: Create World and Level Data Assets

1. **Create Your First World:**
   - Right-click in Project → `Create > TurboTrip > World Data`
   - Name it `World_1`
   - Set properties:
     - World Name: "Neon City"
     - Description: "The cyberpunk streets..."
     - World Number: 1
     - Unlocked By Default: ✓ (checked)
     - World Icon: (assign a sprite)
     - World Color: (pick a color)

2. **Create Levels for This World:**
   - Right-click in Project → `Create > TurboTrip > Level Data`
   - Name it `Level_1_1` (World 1, Level 1)
   - Set properties:
     - Level Name: "Tutorial"
     - Level Number: 1
     - Scene Name: "Level_1_1" (the scene to load)
     - Unlocked By Default: ✓ (first level should be unlocked)
     - Target Time: 60 (seconds)
     - Level Icon: (assign a sprite)
   
3. **Assign Levels to World:**
   - Select `World_1` asset
   - In Inspector, expand "Levels" array
   - Set Size to the number of levels (e.g., 3)
   - Drag each `Level_1_1`, `Level_1_2`, etc. into the array slots

4. **Repeat for Additional Worlds:**
   - Create `World_2`, `World_3`, etc.
   - Only the first world should have "Unlocked By Default" checked
   - Create levels for each world

### Step 2: Setup Game Progress Manager

1. **Create Manager GameObject:**
   - In your first scene (e.g., Main Menu or World Selection)
   - Create Empty GameObject, name it "GameProgressManager"
   - Add Component → `GameProgressManager` script
   
2. **Assign All Worlds:**
   - In Inspector, expand "All Worlds" array
   - Set Size to number of worlds (e.g., 3)
   - Drag each WorldData asset into the slots in order

3. **This GameObject persists across scenes** via DontDestroyOnLoad

### Step 3: Create World Selection Scene

1. **Create New Scene:**
   - File → New Scene
   - Save as `WorldSelection`

2. **Add UI Canvas:**
   - Create UI → Canvas
   - Set Canvas Scaler to "Scale With Screen Size" (1920x1080 reference)

3. **Create World Selection UI:**
   - Create Empty GameObject under Canvas: "WorldSelectionUI"
   - Add Component → `WorldSelectionUI` script
   - Configure:
     - World Button Container: Create UI → Scroll View → Viewport → Content (assign this)
     - World Button Prefab: (create this in Step 4)
     - Title Text: Create UI → Text - TextMeshPro, assign here
     - Level Selection Scene Name: "LevelSelection"

4. **Create World Button Prefab:**
   - Create UI → Button - TextMeshPro
   - Rename to "WorldButton"
   - Add Component → `WorldButton` script
   - Structure (add these as children):
     - Background (Image) - already exists
     - WorldIcon (Image)
     - WorldNameText (TextMeshPro)
     - WorldDescriptionText (TextMeshPro)
     - LockedOverlay (Image) - semi-transparent black panel
     - LockedText (TextMeshPro) - "LOCKED" text
   - Assign all references in WorldButton component
   - Drag to Project to create prefab
   - Delete from scene
   - Assign prefab to WorldSelectionUI's "World Button Prefab" field

5. **Add Back Button (Optional):**
   - Create UI → Button
   - Text: "Back"
   - OnClick → WorldSelectionUI → OnBackButton()

### Step 4: Create Level Selection Scene

1. **Create New Scene:**
   - File → New Scene
   - Save as `LevelSelection`

2. **Add UI Canvas** (same as Step 3.2)

3. **Create Level Selection UI:**
   - Create Empty GameObject under Canvas: "LevelSelectionUI"
   - Add Component → `LevelSelectionUI` script
   - Configure:
     - Level Button Container: Create Scroll View → Viewport → Content
     - Level Button Prefab: (create this next)
     - World Title Text: Create TextMeshPro text
     - World Description Text: Create TextMeshPro text
     - World Selection Scene Name: "WorldSelection"

4. **Create Level Button Prefab:**
   - Create UI → Button - TextMeshPro
   - Rename to "LevelButton"
   - Add Component → `LevelButton` script
   - Structure:
     - Background (Image)
     - LevelIcon (Image)
     - LevelNameText (TextMeshPro)
     - LevelNumberText (TextMeshPro)
     - BestTimeText (TextMeshPro)
     - LockedOverlay (Image)
     - CompletedIndicator (Image) - checkmark or star
   - Assign references in LevelButton component
   - Create prefab
   - Assign to LevelSelectionUI

5. **Add Back Button:**
   - OnClick → LevelSelectionUI → OnBackButton()

### Step 5: Add to Build Settings

1. **File → Build Settings**
2. **Add Open Scenes:**
   - Add "WorldSelection"
   - Add "LevelSelection"
   - Add "LoadingScene" (if you have it)
   - Add all level scenes (e.g., "Level_1_1", "Level_1_2", etc.)

### Step 6: Setup Level Completion (In Gameplay Scenes)

1. **In Each Level Scene:**
   - Create Empty GameObject: "LevelTimer"
   - Add Component → `LevelTimer` script
   - This tracks time automatically

2. **Create Finish Trigger:**
   - Create GameObject with Collider2D (Trigger)
   - Add script to detect player:
   ```csharp
   void OnTriggerEnter2D(Collider2D other) {
       if (other.CompareTag("Player")) {
           LevelTimer.Instance?.FinishLevel();
           SceneManager.LoadScene("LoadingScene");
       }
   }
   ```

3. **Setup LoadingScene/FinalScene:**
   - Ensure your completion scene has the `LoadingScreen` component
   - Add continue/retry/menu buttons and assign to the script

---

## 🎮 How It Works

### Player Flow:
1. **Start** → World Selection Screen
2. **Choose World** → Level Selection Screen (shows that world's levels)
3. **Choose Level** → Gameplay Scene (level map)
4. **Complete Level** → Completion Screen
5. **Continue** → Back to Level Selection (next level unlocked)

### Unlock Logic:
- First world and first level of first world start unlocked
- Completing a level unlocks the next level in that world
- Completing the last level of a world unlocks the next world
- Progress auto-saves via PlayerPrefs

### Data Flow:
```
WorldData (ScriptableObject)
  ↓
GameProgressManager (tracks unlocks/completion)
  ↓
WorldSelectionUI / LevelSelectionUI (generates buttons)
  ↓
Player selects → Loads scene
  ↓
LevelTimer tracks time
  ↓
On finish → LoadingScreen saves progress
```

---

## 📝 TODO: Create Your Level Maps

Now that the selection system is complete, you need to:

1. **Create Level Scenes:**
   - Create a new scene for each level (e.g., `Level_1_1.unity`)
   - Build your level geometry/platforms/obstacles
   - Add player spawn point
   - Add checkpoints
   - Add finish trigger
   - Add LevelTimer GameObject

2. **Name Scenes to Match LevelData:**
   - The "Scene Name" field in each LevelData must match the actual scene name
   - Example: LevelData asset `Level_1_1` should have sceneName = "Level_1_1"

3. **Test Flow:**
   - Start from WorldSelection scene
   - Select world → Select level → Play level → Complete → See completion screen

---

## 🛠️ Optional Enhancements

- **Star Rating System:** Extend LevelButton to show stars based on time
- **Collectibles:** Track coins/collectibles in GameProgressManager
- **Animations:** Add button hover/click animations
- **Sound Effects:** Play sounds on world/level selection
- **Loading Screen:** Add async scene loading with progress bar
- **Localization:** Store text in separate localization system
- **Cloud Save:** Replace PlayerPrefs with cloud save service

---

## 🐛 Troubleshooting

**Problem: "GameProgressManager not found"**
- Ensure GameProgressManager GameObject exists in the first scene
- Check that it has DontDestroyOnLoad (automatic in script)

**Problem: "No world selected" error in LevelSelection**
- Make sure you navigate from WorldSelection → LevelSelection
- Don't directly open LevelSelection scene in editor

**Problem: Levels not unlocking**
- Check that LoadingScreen/FinalScene is calling `CompleteLevel()`
- Verify level scene names match LevelData sceneName fields
- Test by calling `GameProgressManager.Instance.ResetProgress()` to clear saves

**Problem: Buttons not appearing**
- Check that prefabs are assigned in UI scripts
- Verify WorldData has levels assigned in array
- Check console for errors

---

## 📁 File Structure

```
Assets/Scripts/
├── WorldData.cs              (ScriptableObject)
├── LevelData.cs              (ScriptableObject)
├── GameProgressManager.cs    (Singleton, persists)
├── WorldSelectionUI.cs       (Menu controller)
├── LevelSelectionUI.cs       (Menu controller)
├── WorldButton.cs            (Button component)
├── LevelButton.cs            (Button component)
├── LevelTimer.cs             (Gameplay timer)
└── FinalScene.cs             (Completion screen)

Assets/Data/
├── Worlds/
│   ├── World_1.asset
│   ├── World_2.asset
│   └── World_3.asset
└── Levels/
    ├── Level_1_1.asset
    ├── Level_1_2.asset
    ├── Level_2_1.asset
    └── ...

Assets/Scenes/
├── WorldSelection.unity
├── LevelSelection.unity
├── LoadingScene.unity
├── Level_1_1.unity          (TODO: Create your maps)
├── Level_1_2.unity          (TODO: Create your maps)
└── ...
```

---

## ✅ Quick Start Checklist

- [ ] Create at least 1 WorldData asset
- [ ] Create at least 2 LevelData assets
- [ ] Assign levels to world
- [ ] Create GameProgressManager GameObject with all worlds assigned
- [ ] Create WorldSelection scene with UI
- [ ] Create LevelSelection scene with UI
- [ ] Create WorldButton prefab
- [ ] Create LevelButton prefab
- [ ] Add all scenes to Build Settings
- [ ] Create at least 1 level scene (TODO)
- [ ] Test full flow: World → Level → Play → Complete

Good luck building your game! 🎮🚀

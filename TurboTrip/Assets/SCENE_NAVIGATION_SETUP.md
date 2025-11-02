# TurboTrip - Quick Scene Navigation Setup

## ✅ Scripts Created

All navigation scripts are now in place and connected!

---

## 🎮 Complete Scene Flow

```
Home (Start Here)
  ↓ [Play Button]
WorldSelection
  ↓ [Select World]
LevelSelection
  ↓ [Select Level]
Level_1_1 (or any level scene)
  ↓ [Reach Finish Line]
LoadingScene (Completion)
  ↓ [Continue] → LevelSelection
  ↓ [Retry] → Level_1_1
  ↓ [Menu] → Home
```

---

## 🔧 Scene Setup Instructions

### 1. Home Scene Setup

**What you need:**
- A "Play" button
- An "Exit" button

**Steps:**
1. Open `Home.unity`
2. Create an empty GameObject named "HomeManager"
3. Add Component → `HomeManager` script
4. Assign your Play button to the `playButton` field
5. Assign your Exit button to the `exitButton` field
6. Leave `worldSelectionSceneName` as "WorldSelection"

**Optional:** Add GameProgressManager GameObject if not already there:
- Create Empty GameObject: "GameProgressManager"
- Add Component → `GameProgressManager`
- Assign your WorldData assets to the `All Worlds` array

---

### 2. WorldSelection Scene Setup

**Already configured if you followed the main guide!**

Just verify:
- WorldSelectionUI has `levelSelectionSceneName = "LevelSelection"`
- WorldSelectionUI has `homeSceneName = "Home"`
- Add a Back button and wire OnClick → WorldSelectionUI → OnBackButton()

---

### 3. LevelSelection Scene Setup

**Already configured if you followed the main guide!**

Verify:
- LevelSelectionUI has `worldSelectionSceneName = "WorldSelection"`
- Back button calls OnBackButton()

---

### 4. Level_1_1 Scene Setup

**Add Finish Trigger:**

1. Open `Level_1_1.unity`
2. Create a GameObject at the end of your level (e.g., "FinishLine")
3. Add a Collider2D (Box Collider 2D or similar)
4. Check "Is Trigger" ✓
5. Add Component → `FinishLineTrigger` script
6. Set `completionSceneName = "LoadingScene"`

**Ensure LevelTimer exists:**
- Create Empty GameObject: "LevelTimer"
- Add Component → `LevelTimer` script

---

### 5. LoadingScene Setup

**Already enhanced!**

Just add the UI buttons if not present:
- Continue button → OnClick → LoadingScreen → OnContinue()
- Retry button → OnClick → LoadingScreen → OnRetry()
- Menu button → OnClick → LoadingScreen → OnMenu()

Buttons are optional but enhance the completion screen navigation.

---

## 📋 Build Settings

Add all scenes to Build Settings in this order:

1. Home
2. WorldSelection
3. LevelSelection
4. LoadingScene
5. Level_1_1 (and all other level scenes)

**Steps:**
- File → Build Settings
- Drag scenes from Project window into "Scenes In Build" list
- Home should be index 0 (first scene loaded)

---

## 🧪 Testing the Flow

1. **Start from Home scene** (press Play in Unity)
2. Click "Play" → Should load WorldSelection
3. Click a world button → Should load LevelSelection
4. Click a level button → Should load Level_1_1
5. Reach the finish line → Should load LoadingScene
6. Click "Continue" → Back to LevelSelection
7. Click "Menu" → Back to Home

---

## 🎯 Key Points

### Navigation Chain:
- **Home** → WorldSelection (Play button)
- **WorldSelection** → LevelSelection (World button) OR Home (Back button)
- **LevelSelection** → Level scene (Level button) OR WorldSelection (Back button)
- **Level scene** → LoadingScene (FinishLineTrigger)
- **LoadingScene** → LevelSelection (Continue) OR Level (Retry) OR Home (Menu)

### GameProgressManager:
- Created in Home scene
- Persists through all scenes (DontDestroyOnLoad)
- Tracks unlocked worlds/levels automatically
- Saves on level completion

### Simple Scripts:
- **HomeManager**: Play → WorldSelection, Exit → Quit
- **FinishLineTrigger**: Finish line detector that loads LoadingScene

---

## 🐛 Troubleshooting

**"Scene 'X' couldn't be loaded"**
→ Add the scene to Build Settings (File → Build Settings → Add Open Scenes)

**"GameProgressManager not found"**
→ Add GameProgressManager GameObject to your Home scene

**Finish line not working**
→ Make sure your player is tagged "Player" and FinishLineTrigger collider is marked as Trigger

**Level not unlocking**
→ Check that first level has `unlockedByDefault = true` in LevelData asset

---

## ✨ You're All Set!

The navigation is now complete and simple:
- Home → World → Level → Play → Finish → Continue/Retry/Home

No complex UI needed - just wire up the buttons and you're good to go! 🚀

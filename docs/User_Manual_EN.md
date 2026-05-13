# Educational Mini-Game Framework — User Manual

> A complete, step-by-step guide for **developers**, **game designers**, and **educators**
> using the Unity Educational Mini-Game Framework (a Hub + 6 mini-games skeleton
> designed for children aged 4–6).

---

## Table of contents

1. [Introduction](#1-introduction)
2. [Getting started](#2-getting-started)
3. [Core concepts (in plain language)](#3-core-concepts-in-plain-language)
4. [Project structure explained](#4-project-structure-explained)
5. [How to use the framework (main section)](#5-how-to-use-the-framework-main-section)
   - [5.1 Create a new mini-game with NO CODE](#51-create-a-new-mini-game-with-no-code)
   - [5.2 Edit an existing mini-game](#52-edit-an-existing-mini-game)
6. [UI customization (very important)](#6-ui-customization-very-important)
7. [Audio system usage](#7-audio-system-usage)
8. [Adding educational content](#8-adding-educational-content)
9. [Testing the game](#9-testing-the-game)
10. [Debugging guide](#10-debugging-guide)
11. [Extending the framework (advanced)](#11-extending-the-framework-advanced)
12. [Best practices for educational design](#12-best-practices-for-educational-design)
13. [Checklist for publishing a new mini-game](#13-checklist-for-publishing-a-new-mini-game)
14. [Appendix: glossary & menu reference](#14-appendix-glossary--menu-reference)

---

## 1. Introduction

### What is this framework

This is a **Unity (C#) framework for building hub-driven educational mini-games**
for early-literacy and early-numeracy lessons. Out of the box it ships:

- A persistent **Bootstrap** scene that boots core services (audio, input, save, language).
- A **Hub** scene with an interactive map of nodes (one per mini-game).
- **Six mini-game templates** loaded additively from the Hub:
  1. **Consonants** — phonetic multiple choice
  2. **Counting** — quantity discrimination
  3. **Memory** — sound/image pair matching
  4. **Story** — interactive story with multiple-choice gaps
  5. **Classify** — drag tokens into the correct bins
  6. **Syllables** — assemble a word by tapping syllables in order
- **ScriptableObject data assets** for challenges, difficulty, audio cues, the catalog,
  and unlock rules — so authors can build content without writing code.

### Who it is for

| Reader | What they get |
|--------|---------------|
| **Educators / authors** | A drag-and-drop workflow: duplicate a template, drop in art and audio, click Play. |
| **Game designers (non-programmers)** | A data-driven content pipeline using ScriptableObjects and prefabs. |
| **Unity developers** | Clear extension points (`MiniGameBase`, event channels, service interfaces) to add new mini-game types and systems. |

### Problems it solves

- **Each new mini-game is just data + a scene** — no per-game C# unless behavior is truly new.
- **One Hub** drives navigation, unlocks, and additive loading; mini-games stay isolated.
- **Audio comes first**: narration has its own priority/queue/interrupt logic so early
  learners don't depend on reading.
- **Teacher analytics** are built in: every answer is recorded per concept key and can be
  exported to CSV.

---

## 2. Getting started

### 2.1 Required software

- **Unity 6 (6000.4.5f1)** — exact version pinned in `ProjectSettings/ProjectVersion.txt`. Open this project with **Unity Hub** → *Open* → select the project folder. Unity Hub will prompt to install the matching editor if missing.
- **Unity Input System** package (already included in `Packages/`).
- **Git** (optional, recommended for tracking content changes).

> ⚠️ **Warning:** opening the project with an older Unity version can corrupt scenes and `*.asset` files. Always use the pinned version.

### 2.2 Opening the project

1. Launch **Unity Hub**.
2. Click **Open** → navigate to the project root (the folder that contains `Assets/`, `Packages/`, `ProjectSettings/`).
3. Wait for the first import (5–15 minutes on a fresh machine — Unity builds the asset database).
4. When the editor opens, look for the menu **`Edu Framework`** in the top menu bar. If it is there, the framework code compiled correctly.

### 2.3 Generate the demo content (first run)

Before pressing Play, generate the demo data and scenes:

1. In the menu bar, click **`Edu Framework → Generate Default Content (SOs + Scenes + Hub Wire)`**.
2. Watch the Console for the message *"Edu Framework: default content generated."*
3. This creates demo ScriptableObjects, the six `MG_*` mini-game scenes, the shell prefabs,
   adds the mini-game scenes to **Build Settings**, and wires the **`HubConfigurationSO`** into the Hub scene.

### 2.4 Press Play

1. Open **`Assets/Scenes/Bootstrap.unity`** (this is the entry scene).
2. Make sure the Bootstrap scene's root GameObject has all of: `BootstrapScope`, `AudioDirector`, `InputRouter`, `ProgressService`, `LocalizationService`, **and** `AppSessionController` on it (see `BootstrapScope.cs`). All required for boot.
3. Press **Play**.
4. The Bootstrap scene loads, registers services into `AppContext`, then `AppSessionController` loads the **Hub** scene.
5. You see six green node buttons. Click one → that mini-game loads additively → play it → it returns to the Hub when done.

> 💡 **Tip:** if pressing Play from `Hub.unity` directly throws *"AppContext not ready"*, always start from `Bootstrap.unity`. The Bootstrap scene is the only place services are initialized.

### 2.5 Folder overview (one-line each)

| Folder | What lives here |
|--------|----------------|
| `Assets/Scenes/` | The two persistent scenes you start from: `Bootstrap.unity` and `Hub.unity`. |
| `Assets/Scripts/` | All C# source code, grouped by system (Core, Audio, Input, UI, Hub, MiniGames, Progress, Data, Editor). |
| `Assets/EduFramework/Docs/` | This manual and the PT-BR tutorials. |
| `Assets/EduFramework/Prefabs/` | Reusable prefabs: UI shells (multiple-choice, drag-drop) and the mini-game template. |
| `Assets/EduFramework/Scenes/MiniGames/` | The six additive mini-game scenes (`MG_*.unity`). |
| `Assets/EduFramework/ScriptableObjects/Generated/` | Demo content assets created by *Generate Default Content*. |
| `Packages/` | Unity packages (Input System, etc.). Do not edit by hand. |
| `Library/`, `Temp/`, `Logs/` | Generated by Unity. **Never commit, never edit, safe to delete.** |

---

## 3. Core concepts (in plain language)

### What is a "Mini-Game"?

A **mini-game** is one short activity for the child (≈1–3 minutes). Technically it is:

- A **scene** whose name begins with `MG_` (e.g. `MG_Consonants.unity`).
- A **root GameObject** with a component that inherits from **`MiniGameBase`** (e.g. `MiniGameMultipleChoice`).
- A **`MiniGameConfigSO`** asset dropped into the `_configAsset` field of that component.

The `MiniGameBase` lifecycle runs automatically: on `Start`, it reads the config, builds the UI shell, asks the player questions, records answers, and tells the Hub when it's done.

### What is the "Hub"?

The **Hub** is the world map shown between mini-games. It is one Unity scene
(`Hub.unity`) with a single component, **`HubWorldController`**, that:

1. Reads a `HubConfigurationSO` asset.
2. Spawns one **node button** per `HubMapNode` entry.
3. When a node is clicked, **additively** loads the matching `MG_*` scene from `MiniGameCatalogSO`.
4. When the mini-game finishes, it unloads every scene whose name starts with `MG_` and reveals the Hub canvas again.

### What are ScriptableObjects?

A **ScriptableObject (SO)** is a Unity asset that stores **data**, not behavior. Think of it as a typed JSON file that lives in the Project window.

- You create them via **`Assets → Create → Edu → …`** (or right-click in the Project window).
- You **double-click** the asset to edit fields in the Inspector.
- You **drag** them into MonoBehaviour fields to reference them.
- They are **shared**: change one `MiniGameConfigSO` and every scene that uses it sees the new values.

Examples in this project:
`AudioCueSO`, `MultipleChoiceChallengeSO`, `ChallengeSetSO`, `DifficultyProfileSO`,
`MiniGameConfigSO`, `MiniGameCatalogSO`, `HubConfigurationSO`, `UnlockRuleSO`,
`LocalizedTableSO`, `BnccTagSO`, `StoryBookSO`, `StoryPageSO`.

### How data drives the game

This is the chain of references at runtime:

```
HubConfigurationSO
   ├── MiniGameCatalogSO   ─── entry: { gameId, displayNameKey, additiveSceneName }
   └── HubMapNode[]        ─── per node: { nodeId, linkedGameId, uiPosition, unlockRule }

MG_<game>.unity scene
   └── MiniGameRoot GameObject
        ├── MiniGameMultipleChoice (script) ─── _configAsset → MiniGameConfigSO
        └── MultipleChoiceShellView (script)

MiniGameConfigSO
   ├── _challengeSet → ChallengeSetSO ─── List<ChallengeSO>
   ├── _difficulty   → DifficultyProfileSO
   ├── _successCue   → AudioCueSO
   └── _neutralRetryCue → AudioCueSO
```

The same `gameId` string ties everything together: it appears in `MiniGameConfigSO`, in `MiniGameCatalogEntry`, and in `HubMapNode.LinkedGameId`. **Mismatched IDs are the #1 cause of "the node does nothing".**

---

## 4. Project structure explained

### 4.1 Scripts (`Assets/Scripts/`)

Beginner-friendly summary of what each folder contains:

| Folder | What it does |
|--------|-------------|
| `Core/` | The boot system. `AppContext` is the global service locator. `BootstrapScope` wires up services on `Awake`. `AppSessionController` loads the Hub after bootstrap and tracks the lab clock. |
| `Core/Events/` | ScriptableObject event channels (`AnswerEvaluatedEventChannelSO`, `MiniGameSessionEventChannelSO`, `VoidEventChannelSO`) — optional buses to decouple Hub/UI/analytics. |
| `Audio/` | `AudioDirector` (facade), `SfxPlayer`, `NarrationController` (priority queue), `MusicController` (ducking), `AudioCueSO`, `AudioPriority` enum. |
| `Input/` | `InputRouter` — wraps the Unity Input System and exposes semantic properties like `PrimaryClickPressedThisFrame`. |
| `UI/` | Shared UI: `MultipleChoiceShellView`, `DragDropSlotsShellView`, `DraggableUI`, `DropBinView`, `FeedbackKit`, `GameplayUiUtility`. |
| `Hub/` | `HubWorldController` (spawns nodes, loads/unloads `MG_*` scenes) and `MiniGameSessionHub` (the `RequestExitToHub` delegate). |
| `MiniGames/` | `MiniGameBase` (lifecycle), `MiniGameContext`, `EvaluationResult`, `ChallengePicker`, and one concrete class per game (`MiniGameMultipleChoice`, `MiniGameCounting`, `MiniGameMemory`, `MiniGameStoryGaps`, `MiniGameClassification`, `MiniGameSyllableBuilder`). |
| `Progress/` | `ProgressService` (local JSON save under `Application.persistentDataPath`) and `ProgressExportUtility` (teacher CSV). |
| `Localization/` | `LocalizationService` with a hard-coded pt-BR fallback table and support for `LocalizedTableSO` assets. |
| `Data/` | All ScriptableObject **types** (Challenge, ChallengeSet, MiniGameConfig, MiniGameCatalog, HubConfiguration, UnlockRule, LocalizedTable, BnccTag, StoryBook). |
| `Editor/` | Editor-only tools: `EduDefaultContentBuilder` (the *Generate Default Content* menu) and `TeacherExportWindow`. |

### 4.2 Prefabs (`Assets/EduFramework/Prefabs/`)

- **`UI/MultipleChoiceShellView.prefab`** — A blank prefab carrying the `MultipleChoiceShellView` component, used as a parent for runtime-built choice tiles.
- **`UI/DragDropSlotsShellView.prefab`** — Same idea for drag/drop games.
- **`MiniGames/MiniGame_Template_MultipleChoice.prefab`** — A ready-to-use root for multiple-choice games. It carries both `MultipleChoiceShellView` and `MiniGameMultipleChoice` on a single root object — drop it into a new `MG_*` scene, assign your config, done.

### 4.3 ScriptableObjects (`Assets/EduFramework/ScriptableObjects/Generated/`)

The **`Generate Default Content`** menu fills this folder with one of each:
`Ch_*.asset` (challenges), `Set_*.asset` (challenge sets), `Cfg_*.asset` (mini-game configs),
`Difficulty_Default.asset`, `Unlock_Always.asset`, `MiniGameCatalog_Default.asset`,
`HubConfiguration_Default.asset`, `StoryPage_01.asset`, `StoryBook_Demo.asset`.

You can keep authoring inside this folder, or create a new sibling folder for your own content (e.g. `ScriptableObjects/Curso2025/`). The framework finds them via direct references, not by folder name.

### 4.4 Scenes (`Assets/Scenes/` and `Assets/EduFramework/Scenes/MiniGames/`)

- **`Bootstrap.unity`** — the always-on services scene. Loaded first.
- **`Hub.unity`** — the world map.
- **`MG_*.unity`** — six additive mini-game scenes. Each contains a single `MiniGameRoot` GameObject with a `MiniGame*` component on it.

> ⚠️ **Naming rule:** the scene's **file name must start with `MG_`** (see `HubWorldController.UnloadMiniGamesRoutine`). Anything else won't be unloaded when the player returns to the Hub.

### 4.5 Audio

Audio assets (clips) belong in any folder under `Assets/` (we recommend `Assets/EduFramework/Audio/Voice/`, `Sfx/`, `Music/`). The clips are referenced indirectly via **`AudioCueSO`** assets — never wire raw `AudioClip` into a game object; always wrap them in a cue so priority/cooldown/volume metadata stays consistent.

### 4.6 UI

There is **no premade UI canvas asset**. UI in this framework is built **at runtime** by the shell components (`MultipleChoiceShellView`, `DragDropSlotsShellView`, etc.) and the `GameplayUiUtility.CreateOverlayCanvas` helper. To customize UI you either:

- replace the runtime-built canvas with your own prefab and assign it to the shell's `_choicesParent`/`_binsParent` field, or
- subclass the shell and override layout (see [§ 6](#6-ui-customization-very-important)).

---

## 5. How to use the framework (main section)

### 5.1 Create a new mini-game with NO CODE

Use this path when your new mini-game fits the **multiple-choice pattern** ("listen / look → pick the right tile").

#### Step 1 — Make sure the template exists

In the menu, click **`Edu Framework → Create Template Prefab (Multiple Choice Mini-Game)`**.

This creates (if missing) **`Assets/EduFramework/Prefabs/MiniGames/MiniGame_Template_MultipleChoice.prefab`** and pings it in the Project window. It contains a single GameObject with both `MultipleChoiceShellView` and `MiniGameMultipleChoice` on it.

#### Step 2 — Author the audio cues

You'll typically need three cues per challenge:

1. **Prompt** — "Which one starts with B?"
2. **Success** — short positive ding.
3. **Neutral retry** — calm "let's try again" (never harsh).

For each:
- Drag your `.wav` / `.mp3` clip into `Assets/EduFramework/Audio/Voice/` or similar.
- **Right-click → `Create → Edu → Audio → Audio Cue`**.
- Assign the clip to **Clip**, pick a **Priority** (`Tutorial` for prompts, `Celebration` for success, `Correction` for retries) and tweak **Volume Scale** if needed.

#### Step 3 — Author the challenges

For each question:
- **Right-click → `Create → Edu → Data → Challenge → Multiple Choice`**.
- In the Inspector:
  - **Id**: a short stable string like `mc_letter_b`.
  - **Concept keys**: one or more analytics keys, e.g. `phoneme:/b/`. These show up in the teacher CSV per child.
  - **Stimulus Image**: optional big picture/center sprite.
  - **Option Images**: ordered list of tile sprites.
  - **Option Ids**: parallel list of labels (used by the renderer as accessible text).
  - **Correct Index**: the index (0-based) of the right answer in the option list.
  - **Prompt Narration**: drag the prompt `AudioCueSO` here.
  - **BNCC Tags**: optional `BnccTagSO[]` for curriculum reports.

#### Step 4 — Group them into a Challenge Set

- **Right-click → `Create → Edu → Data → Challenge Set`**.
- Drag every `MultipleChoiceChallengeSO` you authored into the **Challenges** list.

#### Step 5 — Pick a difficulty profile

You can reuse `Difficulty_Default.asset` from the generated content. To make a new one:

- **Right-click → `Create → Edu → Data → Difficulty Profile`**.
- Set **Rounds** (how many questions per session), **Wrong Streak Before Simplify** (after this many wrong answers in a row, drop one distractor), **Min/Max Choice Count**.

#### Step 6 — Create the MiniGameConfig

- **Right-click → `Create → Edu → Data → Mini Game Config`**.
- Name it e.g. `Cfg_MyNewGame.asset`.
- Fill in:
  - **Game Id**: a unique string, e.g. `letters_b`. **Write it down — you'll reuse it twice.**
  - **Challenge Set**: drag the set from Step 4.
  - **Difficulty**: drag the profile from Step 5.
  - **Success Cue / Neutral Retry Cue**: drag the cues from Step 2.

#### Step 7 — Create the additive scene

1. **File → New Scene → Empty**.
2. Save as **`Assets/EduFramework/Scenes/MiniGames/MG_MyNewGame.unity`** (the **`MG_` prefix is mandatory** so the Hub knows how to unload it).
3. Drag **`MiniGame_Template_MultipleChoice.prefab`** from the Project window into the Hierarchy.
4. Select the prefab instance, find the **Mini Game Multiple Choice** component in the Inspector, and drag your `Cfg_MyNewGame.asset` into the **Config Asset** field.
5. Save the scene (**Ctrl/Cmd+S**).
6. **File → Build Settings → Add Open Scenes** (or drag the scene file into the list).

#### Step 8 — Wire the Hub

1. Open `MiniGameCatalog_Default.asset` (or your own catalog SO).
2. Add a new entry:
   - **Game Id**: `letters_b` (same as in your config).
   - **Display Name Key**: `mg.letters_b` (used for localized name; can be the same as the id if you don't localize yet).
   - **Additive Scene Name**: `MG_MyNewGame` (no `.unity` extension).
   - **Recommended Age Min/Max**: e.g. 4 / 6.
3. Open `HubConfiguration_Default.asset`.
4. Add a new `HubMapNode`:
   - **Node Id**: `node_letters_b`.
   - **Linked Game Id**: `letters_b` (must match).
   - **Anchored UI Position**: e.g. `(0, -300)` to place it below existing nodes.
   - **Unlock Rule**: drag `Unlock_Always.asset` (or a `RequireMiniGamesCompletedRuleSO` if you want to gate it).

#### Step 9 — Test

- Open `Assets/Scenes/Bootstrap.unity`.
- Press Play.
- Your new node should appear on the map. Click it → your scene loads → questions play → exit returns to the Hub.

> 💡 **Tip:** the **Display Name Key** ("mg.letters_b") falls back to the `linkedGameId` if no localization row matches. So you can ship a new game without touching the localization table — just expect the on-screen label to read your raw id until you author a translation row.

### 5.2 Edit an existing mini-game

You almost never need to open the scene to change behavior — most of it lives in the SO.

#### Change the difficulty

1. Open the **`Cfg_*.asset`** for that game (e.g. `Cfg_Consonants.asset`).
2. Open the linked **`DifficultyProfileSO`** (e.g. `Difficulty_Default.asset`).
3. Adjust:
   - **Rounds** — more = longer session.
   - **Wrong Streak Before Simplify** — lower = mercy kicks in faster.
   - **Max Choice Count** — fewer distractors = easier.
4. Press Play.

> ⚠️ `Difficulty_Default.asset` is **shared** by all six demo games. To change difficulty for **one** game without affecting the others, duplicate the asset (**Ctrl/Cmd+D**), rename it `Difficulty_Consonants.asset`, and assign that to the consonants config only.

#### Replace assets (sprites, audio)

- Drop new files into the `Assets/EduFramework/Art/` or `Audio/` folders.
- Open the **challenge SO** that referenced the old asset.
- Drag the new sprite/cue into the appropriate field.
- No code, no recompile.

#### Change the wording shown on the node

- Open `MiniGameCatalog_Default.asset`.
- Edit **Display Name Key** for the entry — or, if you have a `LocalizedTableSO`, edit the row matching that key.

#### Adjust a single question

- Find the `Ch_*.asset` (e.g. `Ch_MC_A.asset`).
- Change the **Correct Index**, swap an **Option Image**, or update the **Prompt Narration** cue.

#### Tweak feedback animation strength

- Select the `MiniGameRoot` in the `MG_*` scene and find the **`FeedbackKit`** (auto-added by `MiniGameBase`).
- Edit **Pulse Seconds**, **Correct Scale**, **Wrong Scale** in the Inspector.

---

## 6. UI customization (very important)

This framework currently **builds most UI at runtime** through small C# helpers. That keeps the demo light, but it means UI customization is a two-step process: **(a)** decide whether you want to override pieces via Inspector or by replacing prefabs, and **(b)** keep accessibility-friendly defaults.

### 6.1 Where UI lives

| Where | What it does | How to customize |
|-------|--------------|------------------|
| `GameplayUiUtility.CreateOverlayCanvas(name, parent)` | Creates a 1920×1080 reference Screen-Space-Overlay canvas with a `GraphicRaycaster`. | Replace with your own canvas prefab and parent the shell to it instead of letting it auto-create. |
| `GameplayUiUtility.CreateChoiceButton(parent, label, image)` | Builds a 220×220 button with image + label, used by multiple-choice and counting games. | Subclass the shell and provide your own button factory, **or** replace at runtime via `OnInitialized()` in a subclass. |
| `MultipleChoiceShellView` | Lays out choice buttons horizontally inside `_choicesParent` (40 px side padding, 200 px top/bottom, 24 px spacing). | Assign your own pre-styled `_choicesParent` `RectTransform` in the Inspector to skip the auto-built version. |
| `DragDropSlotsShellView` | Two horizontal layout groups: top row of bins, bottom row of tokens. | Same — assign `_binsParent` and `_tokensParent` to your own layout. |
| `HubWorldController` | Builds the hub nodes (180×180 green Image + Button + Text label). | Replace with a prefab-driven version (see § 11.1). |
| `FeedbackKit` | Plays a non-punitive scale pulse on correct/wrong. | Inspector fields: `_pulseSeconds`, `_correctScale`, `_wrongScale`. |

### 6.2 Changing visual style (colors, fonts, sprites, button styles)

#### Colors

The runtime-built UI uses these hard-coded colors:

| Element | Color | Source |
|---------|-------|--------|
| Hub node (unlocked) | RGB `(0.35, 0.75, 0.45)` — green | `HubWorldController.BuildMap()` |
| Hub node (locked) | RGB `(0.55, 0.55, 0.55)` — grey | `HubWorldController.BuildMap()` |
| Choice button (no sprite) | RGB `(0.85, 0.9, 1)` — pale blue | `GameplayUiUtility.CreateChoiceButton` |
| Drop bin | RGB `(0.95, 0.95, 1)` — almost white | `DragDropSlotsShellView.BuildBins` |
| Memory card | RGB `(0.2, 0.45, 0.85)` — blue | `MiniGameMemory.MakeCard` |

To change them **without subclassing**, the cleanest path is to assign a **sprite** to the relevant button/image. The factories all skip the fallback color when an image is present.

To change them **by code** (designer-controlled), copy the relevant helper into your own utility class (e.g. `BrandedUiUtility`) and route your subclassed shell through it. See § 11 for the pattern.

#### Fonts

Runtime text uses Unity's built-in **`LegacyRuntime.ttf`** (see `GameplayUiUtility.BuiltinRuntimeFont`). To use a brand font:

1. Drop your `.ttf` or `.otf` into `Assets/EduFramework/UI/Fonts/`.
2. In `GameplayUiUtility.cs`, replace the `BuiltinRuntimeFont` getter to load your font instead:

   ```csharp
   public static Font BuiltinRuntimeFont =>
       _builtinRuntimeFont ??= Resources.Load<Font>("Fonts/ChildFriendly");
   ```

   (Put the font in `Assets/Resources/Fonts/` so `Resources.Load` finds it.)
3. Rebuild — every label across hub, choice buttons, prompt text, and memory cards picks it up.

> 💡 **Tip:** if you'd rather move to **TextMeshPro**, replace `Text` with `TMP_Text` in `GameplayUiUtility.CreateChoiceButton` and `HubWorldController.BuildMap`. Test in `MG_Consonants` first.

#### Sprites

- **Hub node icon**: extend `HubWorldController` to read a sprite from each `HubMapNode` and assign it to the auto-built `Image`. The cleanest way is to add a `_icon` field to `HubMapNode` (`HubConfigurationSO.cs`) and apply it inside `BuildMap()`.
- **Choice tile sprite**: just fill the **Option Images** array on the `MultipleChoiceChallengeSO` — `CreateChoiceButton` uses it and clears the fallback color automatically.
- **Memory cards**: today they show a `?` text. To use sprite-flipping, add face A/B sprite fields on `MemoryPairChallengeSO` and edit `MiniGameMemory.Flip()` to swap `Image.sprite` instead of changing `Text.text`.

#### Button styles

The simplest way to restyle every `CreateChoiceButton`:

1. Open `Assets/Scripts/UI/GameplayUiUtility.cs`.
2. In `CreateChoiceButton`, change `rt.sizeDelta` (default `220, 220`), add an outline (`go.AddComponent<Outline>()`), or set `btn.transition = Selectable.Transition.SpriteSwap` and assign branded sprites.

### 6.3 Adapting for children (the most important section)

This framework targets **4–6-year-olds**, so:

- **Touch targets ≥ 64 px on screen**. Default choice buttons are 220×220 — keep them at least 180×180 even on smaller layouts.
- **Big readable labels**: `resizeTextForBestFit` is enabled with `resizeTextMaxSize: 32` on choice buttons. Don't lower this below 18.
- **Audio first, text second**: every challenge should set **Prompt Narration**. The on-screen label is treated as accessible alt-text, not the primary cue.
- **Non-punitive feedback**: wrong answers play a `Correction`-tier *neutral retry* cue and a **shrink** (scale 0.96) — never a "buzzer" sound, never red flashes. Keep `FeedbackKit._wrongScale` close to 1.0.
- **Slow pacing**: each round waits 0.45 s after feedback (`yield return new WaitForSecondsRealtime(0.45f)`). Increase this in `MiniGameMultipleChoice`/`MiniGameCounting` etc. for younger groups.
- **No timer pressure**: rounds are bounded by `DifficultyProfileSO.Rounds`, never by a clock.

### 6.4 Creating new UI elements

#### A new button style

Make a **prefab** named e.g. `Button_Choice_Branded.prefab` in `Assets/EduFramework/Prefabs/UI/` with your styling. Then create a subclass of `MultipleChoiceShellView` that instantiates this prefab instead of calling `GameplayUiUtility.CreateChoiceButton`. Swap the script on the template prefab to your subclass.

#### A new panel (e.g. a pause menu, settings, hub header)

1. Create a prefab `Prefabs/UI/PauseOverlay.prefab` with your panel hierarchy.
2. In the **Hub** scene, drop the prefab as a sibling of `HubCanvas` (or assign it via a new field on `HubWorldController`).
3. Toggle it with a Hub-level button or a keyboard shortcut from `AppSessionController`.

#### A new feedback popup

Today `FeedbackKit.Play(correct, target)` only does a scale pulse. To add a popup:

1. Author a `Popup_Correct.prefab` and `Popup_Wrong.prefab` in `Prefabs/UI/`.
2. Add a `[SerializeField] GameObject _correctPopupPrefab;` / `_wrongPopupPrefab;` on `FeedbackKit`.
3. In `FeedbackKit.Play()`, instantiate the appropriate prefab under the canvas, then `Destroy` it after the pulse.

### 6.5 UI best practices

- **Audio + visual redundancy**: every interactable should have both an icon and a narration cue. Never rely on color alone (color-blind safe).
- **Accessibility**: keep the `EventSystem` (the framework auto-spawns one in `GameplayUiUtility.EnsureEventSystem`) so keyboard, mouse, and touch all work.
- **Avoid text dependency**: in challenge SOs, treat `OptionIds` as alt-text. If your audience can't read, fill `OptionImages` and leave `OptionIds` as machine-readable IDs (`fruit_apple`, `fruit_pear`).
- **Layouts that breathe**: stick to `HorizontalLayoutGroup`/`VerticalLayoutGroup` with at least 24 px spacing and 40 px side padding (this matches the framework defaults).
- **Don't fight the canvas scaler**: reference resolution is `1920×1080`, match-mode `0.5`. If you need a different aspect, update it in `GameplayUiUtility.CreateOverlayCanvas` once — never per-prefab.

---

## 7. Audio system usage

### 7.1 Architecture (one screen)

```
        AppContext.Audio  (IAudioDirector)
                │
       ┌────────┴────────────────┐
       │                         │
  SfxPlayer            NarrationController
  (PlayOneShot)        (priority queue, single voice)
                                  │
                          AudioCueSO[] queued
                                  │
                          MusicController.SetDucked(true)
                          while narration is playing
```

`AudioDirector` is the only audio entry point you should call from gameplay (`Context.Audio` inside `MiniGameBase`). It owns the volume sliders (master, sfx, narration) and exposes:

- `PlaySfxCue(AudioCueSO)` — non-blocking, no ducking.
- `EnqueueNarration(AudioCueSO)` — adds to the narration queue with priority logic.
- `StopNarration()` — clears the queue + active voice.

### 7.2 Audio priority (interrupt rules)

From `AudioPriority.cs`:

| Tier | Value | Use for |
|------|-------|---------|
| Ambient | 0 | Background loops, never interrupted but never interrupts. |
| Celebration | 10 | Success jingles. |
| Correction | 40 | "Try again" calm voice. |
| Tutorial | 60 | The teacher voice giving the prompt. **Always wins.** |

Rules (see `NarrationController.Enqueue`):

- A higher-priority cue **clears the queue and stops the current voice**.
- A same-priority cue **queues** if `SameTierQueues = true` (default), otherwise **replaces** the current voice.
- A lower-priority cue **always queues** behind the current one.
- A cue with `CooldownSeconds > 0` is **silently dropped** if it has been played too recently.

### 7.3 Adding a narration line (no code)

1. Drop your `.wav` into the project (e.g. `Audio/Voice/pt_BR/letters/`).
2. Right-click → **`Create → Edu → Audio → Audio Cue`**, name it `Cue_PromptLetterB`.
3. In its Inspector:
   - **Clip** → drag the wav.
   - **Priority** → `Tutorial`.
   - **Volume Scale** → 1.0 unless the recording is hot.
   - **Cooldown Seconds** → 0 (use 2–3 for repetitive cues like hover sounds).
   - **Subtitle Localization Key** → optional, paired with `LocalizedTableSO` for on-screen subtitles.
4. Drag the cue into a `MultipleChoiceChallengeSO._promptNarration` field — done.

### 7.4 Linking audio to actions

- **Per-challenge prompt**: `ChallengeSO._promptNarration` (auto-played each round by `MiniGameMultipleChoice`/`Counting`/etc.).
- **Per-game success / retry**: `MiniGameConfigSO._successCue` (SFX tier) and `_neutralRetryCue` (Correction tier) — used by `MiniGameBase.PlayFeedback`.
- **Per-option hover audio** (multiple choice): `MultipleChoiceChallengeSO._optionHoverAudio[]` — wire one per option index.
- **Memory card flip**: `MemoryPairChallengeSO._cardFaceA / _cardFaceB` — not auto-played yet but accessible from a subclass.
- **Syllable target word**: `SyllableChallengeSO._targetWordAudio` — played once per round before tiles light up.

### 7.5 Best practices for educational audio

- **Prefer a single voice actor across the curriculum.** Children pattern-match the timbre.
- **Keep prompts under 4 seconds.** Long prompts get tuned out.
- **Always pair audio with motion** (`FeedbackKit` pulse), so deaf and hard-of-hearing players still see something happen.
- **Never play more than 2 narration cues per challenge** (prompt + retry). Use `SameTierQueues = true` so multiple wrong answers don't pile up.
- **Use `Correction` tier for retries**, not `Tutorial`, so a fresh `Tutorial` (re-asked prompt) still interrupts a retry cue.
- **Master volume defaults to 1.0** — give teachers a slider on the hub if classroom acoustics vary.

---

## 8. Adding educational content

The general rule: **make the SO first, drop it into a `ChallengeSetSO`, point the `MiniGameConfigSO` at the set**. All six demo games follow this pattern.

### 8.1 New multiple-choice question

`Create → Edu → Data → Challenge → Multiple Choice` →
fill `Id`, `ConceptKeys`, `OptionImages`, `OptionIds`, `CorrectIndex`, `PromptNarration` → drag into a `ChallengeSetSO`. See § 5.1 step 3.

### 8.2 New counting / quantity exercise

`Create → Edu → Data → Challenge → Quantity` →
- **Target Count** (1–6).
- **Token Sprite** (the thing being counted; future use).
- **Number Clips**: AudioClip array, index 0 = "one", index 1 = "two", etc.
- **Concept Keys**: `count:5`, `count:quantity`.

### 8.3 New memory pair

`Create → Edu → Data → Challenge → Memory Pair` →
- **Pair Id** is the matching key. **Two memory challenges sharing the same `PairId` form a pair.** (See `MiniGameMemory.RunSessionRoutine`.)
- **Card Face A / B**: the audio that should play when the card is flipped.

> 💡 **Tip:** for one matching pair (A↔B), author **one** `MemoryPairChallengeSO` with a unique `PairId` — the memory game spawns two cards per challenge automatically.

### 8.4 New classification round

`Create → Edu → Data → Challenge → Classification` →
- **Items**: list of `{ sprite, categoryId }`.
- **Bin Labels**: list of category IDs (these become the bin display names and the matching keys).

The drag/drop check (`MiniGameClassification.RunSessionRoutine`) compares each token's `categoryId` to the bin's `CategoryId`.

### 8.5 New syllable challenge

`Create → Edu → Data → Challenge → Syllable Builder` →
- **Syllable Parts In Order**: e.g. `["CA", "SA"]` to spell *casa*.
- **Syllable Sprites**: parallel array for visual syllable tiles.
- **Target Word Audio**: cue with the whole word; played before tiles appear.

The tiles are shuffled on screen; the child must tap them **in the original order**. A wrong tap resets the progress to zero and plays the retry cue.

### 8.6 New story page / book

1. `Create → Edu → Data → Challenge → Multiple Choice` for the gap (the chosen image fills the blank).
2. `Create → Edu → Data → Story Page` → set `Illustration` (page art) and `GapChallenge` (the MC from step 1).
3. `Create → Edu → Data → Story Book` → drag pages in order.
4. Drag the book into `MiniGameConfigSO._storyBook`.
5. Add a catalog entry with **Additive Scene Name** pointing at an `MG_*` scene whose root is `MiniGameStoryGaps`.

### 8.7 New audio clip

Drop the file into `Assets/EduFramework/Audio/Voice|Sfx|Music/`. Unity imports it automatically. Then wrap it in an `AudioCueSO` (see § 7.3) — never reference raw `AudioClip` from gameplay code.

### 8.8 New sprite / image

Drop the PNG into `Assets/EduFramework/Art/`. In the import inspector, set **Texture Type: Sprite (2D and UI)**. Drag into the relevant SO field.

### 8.9 New BNCC tag

`Create → Edu → Data → BNCC Tag` → fill `Official Code` (e.g. `EI03EF03`) and `Display Name`. Reference from any `ChallengeSO._bnccTags` or `MiniGameCatalogEntry._bnccTags`. Tags don't gate gameplay; they enrich the teacher CSV.

### 8.10 New localized string

1. `Create → Edu → Data → Localized Table` (one per project is enough).
2. Add rows: `{ key: "mg.letters_b", ptBR: "Letra B", secondary: "" }`.
3. In the **Bootstrap** scene, find the `LocalizationService` and drag the table into the `_tables` array.
4. The framework looks up `pt-BR` first, then falls back to `secondary` for other languages. Set **Default Language Id** on the service if you want non-pt-BR builds.

---

## 9. Testing the game

### 9.1 Test a single mini-game (fast iteration loop)

Mini-games **require services from Bootstrap**, so opening `MG_Consonants.unity` alone and pressing Play will log *"AppContext not ready"*.

The proper loop:

1. Keep **`Bootstrap.unity`** as the **only** scene in your hierarchy at edit time (the loaded `MG_*` scene will arrive at runtime).
2. In **File → Build Settings**, ensure `Bootstrap` is index 0 and the relevant `MG_*` scene is included.
3. To skip the Hub during testing, temporarily set `AppSessionController._hubSceneName` to your mini-game scene (`MG_Consonants`) — this routes the boot loader straight there. Remember to revert before shipping.

### 9.2 Test the Hub

- Open `Hub.unity` **with** `Bootstrap.unity` already loaded (drag both into the Hierarchy or, more reliably, always start from `Bootstrap` and let `AppSessionController` load the Hub).
- Verify each node loads its scene and unloads correctly when the session completes.
- Use **Window → Analysis → Profiler** during a session to confirm memory is released between mini-games (additive unload should drop ~30–80 MB depending on assets).

### 9.3 Test unlocks

- Author a `RequireMiniGamesCompletedRuleSO` with one of your demo `gameId`s in `_requiredGameIds`.
- Drag it into a hub node's **Unlock Rule**.
- First boot: node is grey and not clickable. Play and finish the required game → quit → relaunch (progress is persisted to `Application.persistentDataPath/profiles/slot_01/progress_v1.json`) → node is now green.

### 9.4 Reset progress for a clean test

Delete (or rename) `Application.persistentDataPath/profiles/slot_01/progress_v1.json`.

On Windows the path is typically
`C:\Users\<you>\AppData\LocalLow\<CompanyName>\<ProductName>\profiles\slot_01\progress_v1.json`.

### 9.5 Common test scenarios

| Scenario | Expected result |
|---------|-----------------|
| Click any hub node | Hub canvas hides, additive scene loads, mini-game starts after Bootstrap services initialize. |
| Click the wrong answer | `FeedbackKit` shrink, neutral retry cue from `MiniGameConfigSO._neutralRetryCue` plays (if assigned). |
| Click the right answer | `FeedbackKit` enlarge, success cue plays (if assigned). |
| Complete N rounds | Mini-game emits `Completed` event on `MiniGameSessionEventChannelSO`, progress increments, control returns to hub. |
| Force focus loss (Alt-Tab) | `AppSessionController.IsPausedForFocus` becomes `true` and the lab clock stops accumulating. |

---

## 10. Debugging guide

### Missing references / "Node doesn't load anything"

Symptoms: click a hub node, nothing happens (no scene loads).

Checks (in order):
1. Open `MiniGameCatalog_Default.asset`. Find the entry. **Additive Scene Name** must match the actual scene file name **without** `.unity`.
2. `File → Build Settings` → the scene must be in the list **and ticked**.
3. The `HubMapNode._linkedGameId` (in `HubConfigurationSO`) must equal the entry's `_gameId`.

### "AppContext not ready" log

You started a scene that requires Bootstrap services without running Bootstrap first.

Fix: always press Play in `Bootstrap.unity`. Or, in code, add `if (!AppContext.IsInitialized) return;` guards (`MiniGameBase` already does this).

### "Missing MiniGameConfigSO" log

The `MG_*` scene's root GameObject has a `MiniGame*` component but no config dragged into `_configAsset`.

Fix: open the scene, select the root, drag the right `Cfg_*.asset` into the Inspector field.

### Audio not playing

In order:
1. Is the `AudioClip` set on the `AudioCueSO`? `_clip == null` ⇒ the cue is silently dropped.
2. Is the cue's **Volume Scale** > 0? Is the `AudioDirector`'s **Master/Narration/Sfx Volume** > 0?
3. Is a **higher-priority** narration already playing? Lower-tier cues queue behind it.
4. Is **Cooldown Seconds** rejecting the cue? Replays inside the cooldown are dropped.
5. Confirm Unity's audio is not globally muted (toolbar speaker icon in Editor).
6. Open the `BootstrapScope` GameObject — it must have an `AudioDirector` component (and `SfxPlayer`/`NarrationController`/`MusicController` are auto-added on `Awake`).

### UI not updating / no buttons appear

- `MultipleChoiceShellView.Bind()` is called with `challenge == null`? Check that the `ChallengeSetSO` has at least one matching `MultipleChoiceChallengeSO` for that game.
- The challenge has zero `OptionImages` **and** zero `OptionIds`. `BuildSubsetIndices` defaults to 2 anyway, but the buttons will be blank.
- The `EventSystem` was lost between scenes. The framework auto-creates one in `GameplayUiUtility.EnsureEventSystem`, but if you removed that call, add it to your custom shell's `Awake`.
- Multiple Canvases stacked with the same `sortingOrder`. The hub canvas uses `20`; the mini-game's auto-canvas uses `100`. If your custom canvas hides the mini-game, raise its sorting order.

### Event channels silent

`AnswerEvaluatedEventChannelSO` and `MiniGameSessionEventChannelSO` are **optional** references on `MiniGameBase`. If you forget to assign them, the event simply doesn't fire — progress recording still works because `RaiseAnswerEvaluated` always calls `ProgressService.RecordAnswer`.

### Progress not saving

- Open `BootstrapScope` GameObject; confirm `ProgressService` is on it (it's auto-resolved on `Awake`).
- `ProgressService.NotifySessionCompleted` is called in `MiniGameBase.SessionWrapper` after the routine ends. If your custom game ends without finishing the routine (e.g. you throw or return early), call `Context.Progress.NotifySessionCompleted(gameId)` yourself.

---

## 11. Extending the framework (advanced)

For developers.

### 11.1 Create a brand-new mini-game type (custom rules)

1. **C# class** in `Assets/Scripts/MiniGames/` extending `MiniGameBase`:

   ```csharp
   public sealed class MiniGameRhythm : MiniGameBase
   {
       protected override void OnInitialized()
       {
           GameplayUiUtility.EnsureEventSystem();
       }

       protected override IEnumerator RunSessionRoutine()
       {
           var diff = Config.Difficulty;
           var rounds = diff != null ? diff.Rounds : 3;
           for (var r = 0; r < rounds; r++)
           {
               yield return RunOneRound();
           }
       }

       IEnumerator RunOneRound()
       {
           var sw = Stopwatch.StartNew();
           bool? ok = null;
           // ...your UI + input loop here...
           while (!ok.HasValue) yield return null;
           sw.Stop();
           var result = new EvaluationResult(ok.Value, new[] { "rhythm:beat" }, (float)sw.Elapsed.TotalSeconds);
           RaiseAnswerEvaluated(result);
           PlayFeedback(result);
       }
   }
   ```

2. **Optionally** create a new ChallengeSO subclass for this gameplay (e.g. `RhythmChallengeSO`). Put it in `Assets/Scripts/Data/` with a `[CreateAssetMenu(menuName = "Edu/Data/Challenge/Rhythm")]` attribute.

3. **Scene**: create `MG_Rhythm.unity` with one root `GameObject` carrying your component. Assign a `MiniGameConfigSO` via the `_configAsset` field.

4. **Catalog + Hub**: add a `MiniGameCatalogEntry` and a `HubMapNode` for it (see § 5.1 step 8).

### 11.2 Add a new service (e.g. analytics sink)

1. Define an `IAnalyticsSink` interface in `Assets/Scripts/Core/ServiceContracts.cs`.
2. Implement a `MonoBehaviour` on the Bootstrap GameObject (e.g. `MyHttpAnalytics : MonoBehaviour, IAnalyticsSink`).
3. Add a field on `AppServices` (`Assets/Scripts/Core/AppContext.cs`) and a getter on `AppContext` so callers can reach it.
4. Wire it up in `BootstrapScope.Awake()` alongside `audio`/`input`/`progress`/`localization`.
5. Forward `AnswerEvaluatedEvent` to it by subscribing to your `AnswerEvaluatedEventChannelSO` from a separate `MonoBehaviour` in the Bootstrap scene.

### 11.3 Replace the runtime-built hub with a prefab map

1. Build your own hub layout as a prefab: a Canvas, a `RectTransform` map root, and one button prefab per node.
2. Strip the auto-build code in `HubWorldController` (`EnsureCanvas`, `BuildMap`) and instead expose a `[SerializeField] HubNodeView _nodePrefab;` and instantiate **your** prefab under your map root, passing it the `HubMapNode` data.
3. Keep the `LoadMiniGameRoutine`/`UnloadMiniGamesRoutine` flow — that's the contract with the rest of the framework.

### 11.4 Use the event channels for cross-system glue

`AnswerEvaluatedEventChannelSO` and `MiniGameSessionEventChannelSO` are SO-based event buses. You can:

- Create one shared asset (`Events_AnswerEvaluated.asset`).
- Assign it on every `MiniGameBase._answerEvents` field.
- Add `MonoBehaviour` listeners (e.g. a `HudScoreView` in the Hub) that subscribe to its C# event in `OnEnable`.

This keeps the hub/UI from referencing concrete `MiniGame*` classes.

### 11.5 Maintaining architecture consistency

- **Don't read `AppContext` outside the `MiniGameBase.OnInitialized()` boundary** — that's where the `MiniGameContext` snapshot is taken. Always go through `Context.Audio`, `Context.Progress`, etc.
- **Don't reference `MiniGame*` types from the Hub.** The Hub only knows `gameId` strings and scene names.
- **Never call `SceneManager.LoadScene` from gameplay.** Use `MiniGameSessionHub.RequestExitToHub?.Invoke()` to return; the Hub owns scene management.
- **Keep the `MG_` prefix** on every additive mini-game scene; `HubWorldController.UnloadMiniGamesRoutine` matches that prefix.
- **Use the SO `[CreateAssetMenu]` path `Edu/...`**. Authors will look for content under one menu root.

---

## 12. Best practices for educational design

### 12.1 Feedback loops

- **Within 200 ms of an action, visual feedback fires** (`FeedbackKit.Play` is instant).
- **Audio feedback follows within 600 ms** (the 0.45 s wait after each round + the cue's own duration).
- **Progress feedback is delayed**: don't show "level up" mid-session; show it on the hub map after the session.

### 12.2 Encouragement systems

- Treat the **success cue** as celebration, not validation. Use varied takes (use the same `AudioCueSO` with several clips rotated via a small subclass) to avoid auditory fatigue.
- After three correct answers in a row, escalate to a **stronger** celebration cue. Easy to add via a subclass of `MiniGameBase` that tracks streaks and picks a different cue.
- **Never** display a numeric score during a 4–6-year-old session. Keep counters hidden in `ProgressService` and surface them only in the teacher CSV.

### 12.3 Error tolerance

- The default `DifficultyProfileSO` reduces choices by 1 after 2 wrong answers in a row (`WrongStreakBeforeSimplify = 2`, `ChoiceCountForStreak`). Keep this on — it's silent scaffolding.
- The **neutral retry** cue is non-punitive language by design ("vamos tentar de novo", never "errado"). Audit every retry recording.
- **Allow infinite tries per question.** No mini-game in this framework counts wrong answers against rounds completed.

### 12.4 Cultural adaptation (very important)

This framework was built for use in **Brazilian schools**, with explicit space for **indigenous-language** audio:

- `LocalizedRow.secondary` is reserved for a second language alongside `ptBR`.
- `LocalizedTableSO.TryGet` returns `secondary` when `CurrentLanguageId` doesn't start with `pt`.
- **You can ship indigenous-language voice** without writing code: author a second `AudioCueSO` whose clip is the indigenous take, and swap which cue you assign as `PromptNarration` per content build. (For runtime language switching, extend `AudioCueSO` with a `LocalizedAudioClipSet` and have `NarrationController` resolve the clip via `LocalizationService.CurrentLanguageId`.)
- **Imagery matters as much as words.** When picking sprites for `MultipleChoiceChallengeSO.OptionImages`, prefer culturally familiar objects (regional fruits, local animals) over generic stock art.
- **Avoid embedded text on sprites** — text inside an image can't be localized. Keep text in the `OptionIds` array so labels can pass through `LocalizationService.Get`.
- **Reading direction**: this framework assumes left-to-right. If you target Arabic-language content, flip the `HorizontalLayoutGroup` child order in `MultipleChoiceShellView`.

---

## 13. Checklist for publishing a new mini-game

Use this list before merging a new mini-game into the curriculum.

**Content & data**
- [ ] `MiniGameConfigSO` created and named `Cfg_<game>.asset`.
- [ ] `_gameId` set to a unique, stable string (snake_case).
- [ ] `ChallengeSetSO` linked, with at least **3 challenges** (enough for one shuffle).
- [ ] Each challenge has at least **one Concept Key** (e.g. `phoneme:/b/`).
- [ ] `DifficultyProfileSO` assigned (`Rounds ≥ 3`, `MaxChoiceCount` appropriate to age).

**Audio**
- [ ] **Prompt narration** filled on every challenge (`Tutorial` priority).
- [ ] **Success cue** and **Neutral retry cue** filled on the `MiniGameConfigSO`.
- [ ] All cues use the correct `AudioPriority`.
- [ ] Recorded by a consistent voice; peaks below clipping.

**UI**
- [ ] All choice tiles have either a sprite or non-empty `OptionIds`.
- [ ] Tested at **1920×1080** and **1366×768** (canvas scaler default).
- [ ] No raw colors used as the only feedback signal (every action has motion + audio).
- [ ] Tested with a **touch screen** if the lab uses one.

**Hub wiring**
- [ ] `MiniGameCatalogEntry` added: matching `gameId`, correct `additiveSceneName` (no `.unity`).
- [ ] Scene in **File → Build Settings**, ticked.
- [ ] Scene name starts with `MG_`.
- [ ] `HubMapNode` added with the same `linkedGameId`, a sensible `anchoredUiPosition`, and an `UnlockRule`.

**Behavior & progress**
- [ ] Mini-game returns to the Hub on completion (auto-unload works because the scene name starts with `MG_`).
- [ ] Progress increments after a session: open `progress_v1.json` and verify `sessionsCompleted` ticks up.
- [ ] Wrong answer triggers the neutral retry; no harsh sounds.
- [ ] Window focus loss pauses the lab clock (`AppSessionController._pausedForFocus`).

**Educational review**
- [ ] At least one **BNCC tag** on every challenge (`_bnccTags`).
- [ ] Language reviewed by a teacher (vocabulary appropriate for 4–6).
- [ ] Cultural review done if releasing to a new community.

**Final test**
- [ ] Cold-start from `Bootstrap.unity` runs the full Hub → mini-game → Hub loop without warnings.
- [ ] No red errors in the Console during a full session.
- [ ] **`Edu Framework → Teacher Export…`** for the same slot writes a CSV with rows for the new game's concept keys.

---

## 14. Appendix: glossary & menu reference

### Glossary

| Term | Meaning |
|------|---------|
| **AppContext** | Static service locator initialized in Bootstrap. Provides `Audio`, `Input`, `Progress`, `Localization`. |
| **AudioCueSO** | A ScriptableObject wrapping an `AudioClip` with priority, volume, cooldown, subtitle key. |
| **BNCC** | *Base Nacional Comum Curricular* — Brazil's national curriculum. `BnccTagSO` lets you tag challenges to official skill codes. |
| **ChallengeSO** | The data for one question/round. Subclasses: Multiple Choice, Quantity, Memory Pair, Classification, Syllable. |
| **ChallengeSetSO** | An ordered list of challenges. Referenced by `MiniGameConfigSO`. |
| **Concept key** | A short string (`phoneme:/b/`, `count:3`) used to slice analytics in the teacher CSV. |
| **DifficultyProfileSO** | Rounds count and adaptive-choice rules. |
| **EvaluationResult** | The struct (`Correct`, `ConceptKeys`, `LatencySeconds`) passed from a mini-game to `RaiseAnswerEvaluated`. |
| **FeedbackKit** | Component that plays the scale pulse on correct/wrong. |
| **Hub** | The world-map scene (`Hub.unity`) with one `HubWorldController`. |
| **MiniGameBase** | Abstract MonoBehaviour with the shared mini-game lifecycle and `Context` accessor. |
| **MiniGameConfigSO** | Per-game config: `gameId`, challenge set, difficulty, success/retry cues, optional story book. |
| **MiniGameCatalogSO** | The registry: entries map `gameId` → scene name + display key. |
| **MiniGameSessionHub** | Static `RequestExitToHub` action that mini-games invoke when they finish. |
| **ScriptableObject (SO)** | Unity data asset, edited in the Inspector, referenced by drag-and-drop. |
| **Shell** | A UI controller that builds gameplay UI at runtime (`MultipleChoiceShellView`, `DragDropSlotsShellView`). |

### Editor menus

| Menu | What it does |
|------|--------------|
| `Edu Framework → Generate Default Content (SOs + Scenes + Hub Wire)` | Creates all demo SOs, the six `MG_*` scenes, the shell prefabs, the template prefab, adds scenes to Build Settings, and wires the hub config into the `Hub.unity` scene. |
| `Edu Framework → Create Template Prefab (Multiple Choice Mini-Game)` | (Re)creates `MiniGame_Template_MultipleChoice.prefab` only. |
| `Edu Framework → Teacher Export…` | Opens a window to export the JSON progress at a chosen profile slot to a teacher-friendly CSV. |

### `Assets → Create → Edu` reference

| Path | Type | Notes |
|------|------|-------|
| `Edu / Audio / Audio Cue` | `AudioCueSO` | Wrap every clip. |
| `Edu / Data / Challenge Set` | `ChallengeSetSO` | List of `ChallengeSO`. |
| `Edu / Data / Challenge / Multiple Choice` | `MultipleChoiceChallengeSO` | Most flexible challenge type. |
| `Edu / Data / Challenge / Quantity` | `QuantityChallengeSO` | Counting game. |
| `Edu / Data / Challenge / Memory Pair` | `MemoryPairChallengeSO` | Two challenges with same `PairId` form a pair. |
| `Edu / Data / Challenge / Classification` | `ClassificationChallengeSO` | Drag-drop categorization. |
| `Edu / Data / Challenge / Syllable Builder` | `SyllableChallengeSO` | Ordered syllable assembly. |
| `Edu / Data / Difficulty Profile` | `DifficultyProfileSO` | Shared by all six games by default. |
| `Edu / Data / Mini Game Config` | `MiniGameConfigSO` | One per game. |
| `Edu / Data / Mini Game Catalog` | `MiniGameCatalogSO` | One project-wide. |
| `Edu / Data / Hub Configuration` | `HubConfigurationSO` | One project-wide; hub reads it. |
| `Edu / Data / Story Page` / `Story Book` | `StoryPageSO`/`StoryBookSO` | For `MiniGameStoryGaps`. |
| `Edu / Data / Unlock / Always Unlocked` | `AlwaysUnlockedRuleSO` | Default open node. |
| `Edu / Data / Unlock / Require Completed Games` | `RequireMiniGamesCompletedRuleSO` | Gate by `gameId`s. |
| `Edu / Data / Localized Table` | `LocalizedTableSO` | Localization rows (pt-BR + secondary). |
| `Edu / Data / BNCC Tag` | `BnccTagSO` | Curriculum metadata. |
| `Edu / Events / Answer Evaluated` | `AnswerEvaluatedEventChannelSO` | Optional event bus. |
| `Edu / Events / MiniGame Session Event` | `MiniGameSessionEventChannelSO` | Optional event bus. |
| `Edu / Events / Void Event` | `VoidEventChannelSO` | Optional generic event bus. |

---

*See also:*
- `Assets/EduFramework/Docs/Tutorial_Framework_PTBR.md` — concise Portuguese tutorial.
- `Assets/EduFramework/Docs/Guia_ScriptableObjects_PTBR.md` — full ScriptableObject reference in Portuguese.

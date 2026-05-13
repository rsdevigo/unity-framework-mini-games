#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityFramework.MiniGames.Data;
using UnityFramework.MiniGames.Gameplay;
using UnityFramework.MiniGames.Hub;
using UnityFramework.MiniGames.UI;

namespace UnityFramework.MiniGames.EditorTools
{
    public static class EduDefaultContentBuilder
    {
        const string Root = "Assets/EduFramework";
        const string Gen = Root + "/ScriptableObjects/Generated";
        const string MiniScenes = Root + "/Scenes/MiniGames";

        [MenuItem("Edu Framework/Create Template Prefab (Multiple Choice Mini-Game)")]
        public static void CreateMultipleChoiceMiniGameTemplatePrefab() =>
            EnsureMiniGameMultipleChoiceTemplatePrefab(pingWhenExists: true);

        [MenuItem("Edu Framework/Generate Default Content (SOs + Scenes + Hub Wire)")]
        public static void GenerateAll()
        {
            Directory.CreateDirectory(Gen);
            Directory.CreateDirectory(MiniScenes);

            var unlock = GetOrCreate<AlwaysUnlockedRuleSO>($"{Gen}/Unlock_Always.asset");
            var difficulty = GetOrCreate<DifficultyProfileSO>($"{Gen}/Difficulty_Default.asset");
            ApplyDifficultyDefaults(difficulty);

            var mc1 = CreateMultipleChoice($"{Gen}/Ch_MC_A.asset", "mc_a", new[] { "A", "B", "C" }, 0, new[] { "phoneme:/b/" });
            var mc2 = CreateMultipleChoice($"{Gen}/Ch_MC_B.asset", "mc_b", new[] { "1", "2", "3" }, 1, new[] { "count:3" });
            var q1 = CreateQuantity($"{Gen}/Ch_Q_3.asset", 3, new[] { "count:quantity" });
            var memA = CreateMemoryPair($"{Gen}/Ch_Mem_A.asset", "pairA");
            var memB = CreateMemoryPair($"{Gen}/Ch_Mem_B.asset", "pairB");
            var cls = CreateClassification($"{Gen}/Ch_Classify.asset");
            var syl = CreateSyllable($"{Gen}/Ch_Syllable.asset", new[] { "CA", "SA" });

            var setMc = CreateChallengeSet($"{Gen}/Set_Consonants.asset", new ChallengeSO[] { mc1, mc2 });
            var setCount = CreateChallengeSet($"{Gen}/Set_Counting.asset", new ChallengeSO[] { q1 });
            var setMem = CreateChallengeSet($"{Gen}/Set_Memory.asset", new ChallengeSO[] { memA, memB });
            var setCls = CreateChallengeSet($"{Gen}/Set_Classify.asset", new ChallengeSO[] { cls });
            var setSyl = CreateChallengeSet($"{Gen}/Set_Syllable.asset", new ChallengeSO[] { syl });

            var gap = CreateMultipleChoice($"{Gen}/Ch_StoryGap.asset", "gap", new[] { "sol", "chuva", "vento" }, 0, new[] { "story:gap1" });
            var page = CreateStoryPage($"{Gen}/StoryPage_01.asset", gap);
            var book = CreateStoryBook($"{Gen}/StoryBook_Demo.asset", new[] { page });

            var setStory = CreateChallengeSet($"{Gen}/Set_Story.asset", new ChallengeSO[] { mc1 });

            var cfgConsonants = CreateMiniGameConfig($"{Gen}/Cfg_Consonants.asset", "consonants", setMc, difficulty, null);
            var cfgCounting = CreateMiniGameConfig($"{Gen}/Cfg_Counting.asset", "counting", setCount, difficulty, null);
            var cfgMemory = CreateMiniGameConfig($"{Gen}/Cfg_Memory.asset", "memory", setMem, difficulty, null);
            var cfgStory = CreateMiniGameConfig($"{Gen}/Cfg_Story.asset", "story", setStory, difficulty, book);
            var cfgClassify = CreateMiniGameConfig($"{Gen}/Cfg_Classify.asset", "classify", setCls, difficulty, null);
            var cfgSyllable = CreateMiniGameConfig($"{Gen}/Cfg_Syllable.asset", "syllables", setSyl, difficulty, null);

            var catalog = GetOrCreate<MiniGameCatalogSO>($"{Gen}/MiniGameCatalog_Default.asset");
            ApplyCatalog(catalog);

            var hubCfg = GetOrCreate<HubConfigurationSO>($"{Gen}/HubConfiguration_Default.asset");
            ApplyHubConfiguration(hubCfg, catalog, unlock);

            BuildMiniGameScene("MG_Consonants", typeof(MiniGameMultipleChoice), cfgConsonants);
            BuildMiniGameScene("MG_Counting", typeof(MiniGameCounting), cfgCounting);
            BuildMiniGameScene("MG_Memory", typeof(MiniGameMemory), cfgMemory);
            BuildMiniGameScene("MG_Story", typeof(MiniGameStoryGaps), cfgStory);
            BuildMiniGameScene("MG_Classify", typeof(MiniGameClassification), cfgClassify);
            BuildMiniGameScene("MG_Syllables", typeof(MiniGameSyllableBuilder), cfgSyllable);

            AppendBuildSettings();
            WireHubScene(hubCfg);
            SaveShellPrefab($"{Root}/Prefabs/UI/MultipleChoiceShellView.prefab", typeof(MultipleChoiceShellView));
            SaveShellPrefab($"{Root}/Prefabs/UI/DragDropSlotsShellView.prefab", typeof(DragDropSlotsShellView));
            EnsureMiniGameMultipleChoiceTemplatePrefab(pingWhenExists: false);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Edu Framework: default content generated. Open the Hub scene and press Play.");
        }

        static T GetOrCreate<T>(string path) where T : ScriptableObject
        {
            var existing = AssetDatabase.LoadAssetAtPath<T>(path);
            if (existing != null)
                return existing;
            var so = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(so, path);
            return so;
        }

        static void ApplyDifficultyDefaults(DifficultyProfileSO d)
        {
            var so = new SerializedObject(d);
            so.FindProperty("_rounds").intValue = 4;
            so.FindProperty("_wrongStreakBeforeSimplify").intValue = 2;
            so.FindProperty("_maxChoiceCount").intValue = 4;
            so.FindProperty("_minChoiceCount").intValue = 2;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static MultipleChoiceChallengeSO CreateMultipleChoice(string path, string id, string[] optionLabels, int correctIndex, string[] concepts)
        {
            var so = GetOrCreate<MultipleChoiceChallengeSO>(path);
            var ser = new SerializedObject(so);
            ser.FindProperty("_id").stringValue = id;
            WriteStringArray(ser.FindProperty("_conceptKeys"), concepts);
            var ids = ser.FindProperty("_optionIds");
            ids.ClearArray();
            foreach (var label in optionLabels)
            {
                ids.InsertArrayElementAtIndex(ids.arraySize);
                ids.GetArrayElementAtIndex(ids.arraySize - 1).stringValue = label;
            }

            ser.FindProperty("_correctIndex").intValue = correctIndex;
            ser.ApplyModifiedPropertiesWithoutUndo();
            return so;
        }

        static QuantityChallengeSO CreateQuantity(string path, int target, string[] concepts)
        {
            var so = GetOrCreate<QuantityChallengeSO>(path);
            var ser = new SerializedObject(so);
            ser.FindProperty("_id").stringValue = "qty_demo";
            WriteStringArray(ser.FindProperty("_conceptKeys"), concepts);
            ser.FindProperty("_targetCount").intValue = target;
            ser.ApplyModifiedPropertiesWithoutUndo();
            return so;
        }

        static MemoryPairChallengeSO CreateMemoryPair(string path, string pairId)
        {
            var so = GetOrCreate<MemoryPairChallengeSO>(path);
            var ser = new SerializedObject(so);
            ser.FindProperty("_id").stringValue = pairId;
            WriteStringArray(ser.FindProperty("_conceptKeys"), new[] { $"memory:{pairId}" });
            ser.FindProperty("_pairId").stringValue = pairId;
            ser.ApplyModifiedPropertiesWithoutUndo();
            return so;
        }

        static ClassificationChallengeSO CreateClassification(string path)
        {
            var so = GetOrCreate<ClassificationChallengeSO>(path);
            var ser = new SerializedObject(so);
            ser.FindProperty("_id").stringValue = "cls_demo";
            WriteStringArray(ser.FindProperty("_conceptKeys"), new[] { "classify:demo" });
            var items = ser.FindProperty("_items");
            items.ClearArray();
            for (var i = 0; i < 2; i++)
            {
                items.InsertArrayElementAtIndex(items.arraySize);
                var el = items.GetArrayElementAtIndex(items.arraySize - 1);
                el.FindPropertyRelative("categoryId").stringValue = i == 0 ? "fruit" : "animal";
            }

            WriteStringArray(ser.FindProperty("_binLabels"), new[] { "fruit", "animal" });
            ser.ApplyModifiedPropertiesWithoutUndo();
            return so;
        }

        static SyllableChallengeSO CreateSyllable(string path, string[] parts)
        {
            var so = GetOrCreate<SyllableChallengeSO>(path);
            var ser = new SerializedObject(so);
            ser.FindProperty("_id").stringValue = "syl_demo";
            WriteStringArray(ser.FindProperty("_conceptKeys"), new[] { "syllable:demo" });
            WriteStringArray(ser.FindProperty("_syllablePartsInOrder"), parts);
            ser.ApplyModifiedPropertiesWithoutUndo();
            return so;
        }

        static StoryPageSO CreateStoryPage(string path, MultipleChoiceChallengeSO gap)
        {
            var so = GetOrCreate<StoryPageSO>(path);
            var ser = new SerializedObject(so);
            ser.FindProperty("_gapChallenge").objectReferenceValue = gap;
            ser.ApplyModifiedPropertiesWithoutUndo();
            return so;
        }

        static StoryBookSO CreateStoryBook(string path, StoryPageSO[] pages)
        {
            var so = GetOrCreate<StoryBookSO>(path);
            var ser = new SerializedObject(so);
            var p = ser.FindProperty("_pages");
            p.ClearArray();
            foreach (var page in pages)
            {
                p.InsertArrayElementAtIndex(p.arraySize);
                p.GetArrayElementAtIndex(p.arraySize - 1).objectReferenceValue = page;
            }

            ser.ApplyModifiedPropertiesWithoutUndo();
            return so;
        }

        static ChallengeSetSO CreateChallengeSet(string path, ChallengeSO[] challenges)
        {
            var so = GetOrCreate<ChallengeSetSO>(path);
            var ser = new SerializedObject(so);
            var list = ser.FindProperty("_challenges");
            list.ClearArray();
            foreach (var c in challenges)
            {
                if (c == null)
                    continue;
                list.InsertArrayElementAtIndex(list.arraySize);
                list.GetArrayElementAtIndex(list.arraySize - 1).objectReferenceValue = c;
            }

            ser.ApplyModifiedPropertiesWithoutUndo();
            return so;
        }

        static MiniGameConfigSO CreateMiniGameConfig(string path, string gameId, ChallengeSetSO set, DifficultyProfileSO diff, StoryBookSO book)
        {
            var so = GetOrCreate<MiniGameConfigSO>(path);
            var ser = new SerializedObject(so);
            ser.FindProperty("_gameId").stringValue = gameId;
            ser.FindProperty("_challengeSet").objectReferenceValue = set;
            ser.FindProperty("_difficulty").objectReferenceValue = diff;
            ser.FindProperty("_storyBook").objectReferenceValue = book;
            ser.ApplyModifiedPropertiesWithoutUndo();
            return so;
        }

        static void ApplyCatalog(MiniGameCatalogSO catalog)
        {
            var ser = new SerializedObject(catalog);
            var e = ser.FindProperty("_entries");
            e.ClearArray();
            void Add(string id, string key, string scene)
            {
                e.InsertArrayElementAtIndex(e.arraySize);
                var el = e.GetArrayElementAtIndex(e.arraySize - 1);
                el.FindPropertyRelative("_gameId").stringValue = id;
                el.FindPropertyRelative("_displayNameKey").stringValue = key;
                el.FindPropertyRelative("_additiveSceneName").stringValue = scene;
                el.FindPropertyRelative("_bnccTags").ClearArray();
                el.FindPropertyRelative("_recommendedAgeMin").intValue = 4;
                el.FindPropertyRelative("_recommendedAgeMax").intValue = 6;
            }

            Add("consonants", "mg.consonants", "MG_Consonants");
            Add("counting", "mg.counting", "MG_Counting");
            Add("memory", "mg.memory", "MG_Memory");
            Add("story", "mg.story", "MG_Story");
            Add("classify", "mg.classify", "MG_Classify");
            Add("syllables", "mg.syllables", "MG_Syllables");
            ser.ApplyModifiedPropertiesWithoutUndo();
        }

        static void ApplyHubConfiguration(HubConfigurationSO hub, MiniGameCatalogSO catalog, AlwaysUnlockedRuleSO unlock)
        {
            var ser = new SerializedObject(hub);
            ser.FindProperty("_catalog").objectReferenceValue = catalog;
            var nodes = ser.FindProperty("_nodes");
            nodes.ClearArray();
            var positions = new[]
            {
                ("node_consonants", "consonants", new Vector2(-420, 80)),
                ("node_counting", "counting", new Vector2(-220, -40)),
                ("node_memory", "memory", new Vector2(0, 120)),
                ("node_story", "story", new Vector2(220, -40)),
                ("node_classify", "classify", new Vector2(420, 80)),
                ("node_syllables", "syllables", new Vector2(0, -200)),
            };
            foreach (var (nid, gid, pos) in positions)
            {
                nodes.InsertArrayElementAtIndex(nodes.arraySize);
                var n = nodes.GetArrayElementAtIndex(nodes.arraySize - 1);
                n.FindPropertyRelative("_nodeId").stringValue = nid;
                n.FindPropertyRelative("_linkedGameId").stringValue = gid;
                n.FindPropertyRelative("_anchoredUiPosition").vector2Value = pos;
                n.FindPropertyRelative("_unlockRule").objectReferenceValue = unlock;
            }

            ser.ApplyModifiedPropertiesWithoutUndo();
        }

        static void WriteStringArray(SerializedProperty prop, string[] values)
        {
            prop.ClearArray();
            if (values == null)
                return;
            foreach (var v in values)
            {
                prop.InsertArrayElementAtIndex(prop.arraySize);
                prop.GetArrayElementAtIndex(prop.arraySize - 1).stringValue = v;
            }
        }

        static void BuildMiniGameScene(string sceneName, System.Type miniGameType, MiniGameConfigSO cfg)
        {
            var sc = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var root = new GameObject("MiniGameRoot");
            var mb = (MiniGameBase)root.AddComponent(miniGameType);
            var so = new SerializedObject(mb);
            so.FindProperty("_configAsset").objectReferenceValue = cfg;
            so.ApplyModifiedPropertiesWithoutUndo();

            var scenePath = $"{MiniScenes}/{sceneName}.unity";
            EditorSceneManager.SaveScene(sc, scenePath);
        }

        static void AppendBuildSettings()
        {
            var scenes = EditorBuildSettings.scenes.ToList();
            void Add(string path)
            {
                if (scenes.Any(s => s.path == path))
                    return;
                scenes.Add(new EditorBuildSettingsScene(path, true));
            }

            Add($"{MiniScenes}/MG_Consonants.unity");
            Add($"{MiniScenes}/MG_Counting.unity");
            Add($"{MiniScenes}/MG_Memory.unity");
            Add($"{MiniScenes}/MG_Story.unity");
            Add($"{MiniScenes}/MG_Classify.unity");
            Add($"{MiniScenes}/MG_Syllables.unity");
            EditorBuildSettings.scenes = scenes.ToArray();
        }

        static void SaveShellPrefab(string path, System.Type componentType)
        {
            if (File.Exists(path))
                return;
            var dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir))
                Directory.CreateDirectory(dir);
            var go = new GameObject(Path.GetFileNameWithoutExtension(path));
            go.AddComponent(componentType);
            PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
        }

        /// <summary>
        /// Root prefab for additive mini-game scenes: <see cref="MiniGameMultipleChoice"/> + <see cref="MultipleChoiceShellView"/> on one object (matches runtime shell lookup).
        /// </summary>
        static void EnsureMiniGameMultipleChoiceTemplatePrefab(bool pingWhenExists)
        {
            const string path = Root + "/Prefabs/MiniGames/MiniGame_Template_MultipleChoice.prefab";
            var dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir))
                Directory.CreateDirectory(dir);
            if (File.Exists(path))
            {
                if (pingWhenExists)
                    EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<Object>(path));
                return;
            }

            var go = new GameObject("MiniGame_Template_MultipleChoice");
            go.AddComponent<MultipleChoiceShellView>();
            go.AddComponent<MiniGameMultipleChoice>();
            PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            AssetDatabase.SaveAssets();
            if (pingWhenExists)
                EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<Object>(path));
        }

        static void WireHubScene(HubConfigurationSO hubCfg)
        {
            if (!TryGetHubSceneAssetPath(out var hubPath))
            {
                Debug.LogWarning(
                    "Edu Framework: no Hub scene found under Assets (e.g. Assets/Scenes/Hub.unity). " +
                    "Skip wiring HubWorldController; assign HubConfigurationSO manually in your Hub scene.");
                return;
            }

            try
            {
                var sc = EditorSceneManager.OpenScene(hubPath, OpenSceneMode.Single);
                var hub = Object.FindObjectsByType<HubWorldController>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                    .FirstOrDefault();
                if (hub == null)
                {
                    var go = new GameObject("HubSystems");
                    hub = go.AddComponent<HubWorldController>();
                }

                var so = new SerializedObject(hub);
                so.FindProperty("_configuration").objectReferenceValue = hubCfg;
                so.ApplyModifiedPropertiesWithoutUndo();
                EditorSceneManager.SaveScene(sc);
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"Edu Framework: could not open or save hub scene '{hubPath}': {ex.Message}\nAssign HubConfigurationSO manually on HubWorldController.");
            }
        }

        /// <summary>
        /// Resolves a project Hub scene path via the asset database (do not use <see cref="File.Exists"/> on "Assets/..." — CWD may not be the project root).
        /// </summary>
        static bool TryGetHubSceneAssetPath(out string path)
        {
            path = null;
            foreach (var candidate in new[] { "Assets/Scenes/Hub.unity", "Assets/EduFramework/Scenes/Hub.unity" })
            {
                if (AssetDatabase.LoadAssetAtPath<SceneAsset>(candidate) != null)
                {
                    path = candidate;
                    return true;
                }
            }

            foreach (var guid in AssetDatabase.FindAssets("t:Scene"))
            {
                var p = AssetDatabase.GUIDToAssetPath(guid);
                if (string.Equals(Path.GetFileName(p), "Hub.unity", System.StringComparison.OrdinalIgnoreCase))
                {
                    path = p;
                    return true;
                }
            }

            return false;
        }
    }
}
#endif

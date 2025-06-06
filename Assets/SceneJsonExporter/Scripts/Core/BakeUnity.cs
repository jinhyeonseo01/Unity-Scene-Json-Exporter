// BakeUnityRefactored.cs
// ------------------------------------------------------------
// One‑file refactor of the original BakeUnity class.
//  • Groups related fields in regions.
//  • Extracts helper methods.
//  • Removes duplicated logic (Scene/Selection export share core path).
//  • Uses explicit names & early returns for clarity.
//  • Keeps public API identical: SceneExport, SelectExport, CopyResources.
// ------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Clrain.SceneToJson;

namespace Clrain.SceneToJson
{
    [InitializeOnLoad]
    public static class BakeUnity
    {
        #region === Constants & Paths ===

        public static string DefinePathResources = "Assets/";           // Project root for in‑game resources
        public static string DefinePathConfigResources = "Assets/";           // Config‑specific root

        public static string ExportJsonPath = "./Assets/Exports/"; // Final JSON save path
        public static string ExportResourcePath = "./Assets/Exports/"; // Copied resource path

        #endregion


        #region === Working Collections ===

        public static Queue<BakeObject> PreprocessQueue = new Queue<BakeObject>(8192);
        public static Queue<BakeObject> BakeQueue = new Queue<BakeObject>(8192);
        public static HashSet<string> GuidCache = new HashSet<string>(8192);
        public static HashSet<string> ResourcePaths = new HashSet<string>(8192);

        private static readonly HashSet<Type> BakeTypeTable = new()
    {
        typeof(GameObject),
        typeof(Material),
        typeof(Component)
    };

        #endregion

        #region === Public Entry Points ===

        public static void SceneExport() => ExportInternal(UnityEngine.Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.InstanceID).ToList(), SceneManager.GetActiveScene().name);

        public static void SelectExport()
        {
            var selectionRoots = Selection.gameObjects;
            if (selectionRoots.Length == 0)
            {
                Debug.LogWarning("Nothing selected.");
                return;
            }

            var allSelected = selectionRoots
                .SelectMany(g => g.GetComponentsInChildren<Transform>(true))
                .Select(t => t.gameObject)
                .ToList();

            string exportName = $"{selectionRoots[0].name}_with_{selectionRoots.Length}";
            ExportInternal(allSelected, exportName);
        }

        public static void CopyResources()
        {
            foreach (string src in ResourcePaths.Where(File.Exists))
            {
                // Strip leading "Assets/" → relative path
                string relative = src.StartsWith("Assets/") ? src[7..] : src;
                string dst = Path.Combine(ExportResourcePath, relative);
                Directory.CreateDirectory(Path.GetDirectoryName(dst)!);
                File.Copy(src, dst, true);
            }

            AssetDatabase.Refresh();
            Debug.Log($"Resource copy finished. Count = {ResourcePaths.Count}");
        }

        #endregion

        #region === Core Export Pipeline ===


        private static void PrepareForBake()
        {
            PreprocessQueue.Clear();
            BakeQueue.Clear();
            GuidCache.Clear();
            ResourcePaths.Clear();
            BakeObject.Init();

            UnityEngine.Object.FindAnyObjectByType<ExportSetting>()?.PathUpdate();
        }


        private static void ExportInternal(List<GameObject> sourceObjects, string exportName)
        {
            PrepareForBake();

            // Queue preprocessing
            foreach (GameObject go in GameObjectFilter(sourceObjects))
                EnqueuePreprocess(go);

            // Run bake pipeline
            JObject rootJson = new();
            rootJson["references"] = new JObject();

            RunPreprocessPhase();
            RunBakePhase(rootJson);

            // Save
            string jsonText = rootJson.ToString();
            SaveJsonToDisk(jsonText, exportName);
        }

        private static void RunPreprocessPhase()
        {
            while (PreprocessQueue.TryDequeue(out var obj))
            {
                obj.Preprocess();
                BakeQueue.Enqueue(obj);
            }
        }

        private static void RunBakePhase(JObject root)
        {
            var refs = (JObject)root["references"]!;

            while (BakeQueue.TryDequeue(out var obj))
            {
                Type concrete = FindMostDerivedMatchType(obj.target.GetType());
                string key = $"{concrete.Name}s";

                refs[key] ??= new JArray();
                ((JArray)refs[key]!).Add(obj.Bake(root));
            }
        }

        #endregion

        #region === Queue & Table Helpers ===

        public static void EnqueuePreprocess<T>(T target) where T : class
        {
            if (target == null)
                return;
            string guid = BakeGuid.GetGuid(target);
            if (GuidCache.Contains(guid)) return;

            BakeObject bakeObj = BakeObject.CreateProperty(target);
            if (bakeObj == null) return;

            GuidCache.Add(guid);
            PreprocessQueue.Enqueue(bakeObj);
        }


        public static void AddResourcePath(string assetPath) => ResourcePaths.Add(assetPath);

        #endregion

        #region === Utility ===

        private static Type FindMostDerivedMatchType(Type concrete)
        {
            var candidates = BakeTypeTable.Where(t => t.IsAssignableFrom(concrete)).ToList();
            return candidates.First(t => !candidates.Any(o => o != t && t.IsAssignableFrom(o)));
        }

        private static void SaveJsonToDisk(string json, string fileName)
        {
            string dir = ExportJsonPath.Trim();
            Directory.CreateDirectory(dir);
            string full = Path.Combine(dir, fileName + ".json");
            File.WriteAllText(full, json);
            AssetDatabase.Refresh();

            Debug.Log($"Bake Done → lines: {json.Count(c => c == '\n')}\nExport File : {fileName}.json\nExport Path : {full}\n\n");
        }

        public static List<GameObject> GameObjectFilter(List<GameObject> origin)
        {
            List<GameObject> filterList = origin.Where(e => !e.name.Contains("#")).ToList();
            List<GameObject> removeList = origin.Where(e => e.name.Contains("##")).ToList();
            removeList.SelectMany(x => x.GetComponentsInChildren<Transform>(true).Select(e => e.gameObject)).ToList().ForEach(e => filterList.Remove(e));
            filterList = filterList.Distinct().ToList();
            return filterList;
        }

        #endregion
    }
}
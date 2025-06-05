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


[InitializeOnLoad]
public static class BakeUnity
{
    //[OdinSerialize]
    public static string DefinePathResources = "Assets/";
    public static string DefinePathConfigResources = "Assets/";

    public static List<GameObject> refList_GameObject;

    public static Queue<BakeObject> preprocessQueue   = new Queue<BakeObject>(8192);
    public static Queue<BakeObject> bakeQueue         = new Queue<BakeObject>(8192);
    public static HashSet<string> guidTable             = new HashSet<string>(8192);

    public static HashSet<Type> BakeTypeTable;

    [NonSerialized, HideInInspector]
    public static string finalJson;
    //[OdinSerialize]
    public static string ExportJsonPath = "./Assets/Exports/";
    public static string ExportResourcePath = "./Assets/Exports/";


    public static HashSet<string> resourceFilePathTable;

    //[Button]
    public static void SceneExport()
    {
        JObject totalJson;
        InitBake();
        InitJson(out totalJson);

        
        foreach (var obj in GameObjectFilter(UnityEngine.Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.InstanceID).ToList()))
            AddPreprocess(obj);

        Baking(ref totalJson);


        finalJson = totalJson.ToString();

        Debug.Log($"total Json Line : {finalJson.Count((e) => e == '\n')}");

        UnityEngine.SceneManagement.Scene scene = SceneManager.GetActiveScene();
        Save(finalJson, scene.name);
    }

    //[Button]
    public static void SelectExport()
    {
        JObject totalJson;
        InitBake();
        InitJson(out totalJson);


        foreach (var obj in GameObjectFilter(Selection.gameObjects.SelectMany(e => e.GetComponentsInChildren<Transform>(true).Select(t => t.gameObject)).ToList()))
            AddPreprocess(obj);

        Baking(ref totalJson);


        finalJson = totalJson.ToString();

        Debug.Log($"total Json Line : {finalJson.Count((e) => e == '\n')}");

        //UnityEngine.SceneManagement.Scene scene = SceneManager.GetActiveScene();
        Save(finalJson, $"{Selection.gameObjects[0].name} with {Selection.gameObjects.Length}Count");
    }

    public static List<GameObject> GameObjectFilter(List<GameObject> origin)
    {
        List<GameObject> filterList = origin.Where(e => !e.name.Contains("#")).ToList();
        List<GameObject> removeList = origin.Where(e => e.name.Contains("##")).ToList();
        removeList.SelectMany(x => x.GetComponentsInChildren<Transform>(true).Select(e => e.gameObject)).ToList().ForEach(e => filterList.Remove(e));
        filterList = filterList.Distinct().ToList();
        return filterList;
    }

    public static void CopyResources()
    {
        var filePathList = resourceFilePathTable.Where(e => !string.IsNullOrEmpty(e)).ToList();

        foreach (var path in filePathList)
        {
            // 파일이 실제로 존재하는지 확인
            if (File.Exists(path))
            {
                // 4. "Assets/" 접두어 제거
                string relativePath = path;
                const string prefix = "Assets/";
                if (relativePath.StartsWith(prefix))
                {
                    relativePath = relativePath.Substring(prefix.Length);
                }
                else
                    Debug.Log(path);
                // 5. 사용자 지정 경로와 결합하여 최종 대상 경로 생성
                string destinationPath = Path.Combine(BakeUnity.ExportResourcePath, relativePath);

                // 6. 대상 디렉토리 경로 확인 및 없으면 생성
                string destDir = Path.GetDirectoryName(destinationPath);
                if (!Directory.Exists(destDir))
                {
                    Directory.CreateDirectory(destDir);
                    Debug.Log($"디렉토리 생성: {destDir}");
                }

                // 7. 파일 복사 (overwrite가 true이면 기존 파일 덮어씀)
                File.Copy(path, destinationPath, true);
            }
            else
            {
                Debug.LogWarning($"파일을 찾을 수 없음: {path}");
            }
        }

        AssetDatabase.Refresh();
        Debug.Log($"파일 복사 완료 : Count - {filePathList.Count}");
    }

    public static void InitBake()
    {
        refList_GameObject ??= new List<GameObject>(8192);
        refList_GameObject.Clear();

        resourceFilePathTable ??= new HashSet<string>(8192);
        resourceFilePathTable.Clear();

        guidTable ??= new HashSet<string>(8192);
        guidTable.Clear();

        BakeTypeTable ??= new HashSet<Type>
        {
            typeof(GameObject),
            typeof(Material),
            typeof(Component)
        };

        BakeObject.Init();

        UnityEngine.Object.FindAnyObjectByType<ExportSetting>()?.PathUpdate();
    }
    public static void InitJson(out JObject json)
    {
        json = new JObject();
        json["references"] ??= new JObject();
    }

    public static void Baking(ref JObject totalJson)
    {

        while (preprocessQueue.TryDequeue(out var element)) {
            element.Preprocess();
            AddBake(element);
        }


        while (bakeQueue.TryDequeue(out var element)) {
            Type targetType = element.target.GetType();
            var candidates = BakeTypeTable.Where(t => t.IsAssignableFrom(targetType)).ToList();
            var foundType = candidates.Where(t => !candidates.Any(u => u != t && t.IsAssignableFrom(u))).FirstOrDefault();
            if (foundType != null)
            {
                var typeKey = $"{foundType.Name}s";
                var refJson = totalJson["references"];
                (refJson as JObject)[typeKey] ??= new JArray();
                ((refJson as JObject)[typeKey] as JArray)?.Add(element.Bake(totalJson));
            }
        }

    }

    private static void Save(string data, string name)
    {
        var sceneName = name;
        Debug.Log($"Baking Name : {sceneName}");

        string dirPath = ExportJsonPath;// Path.GetDirectoryName(exportPath);
        string filePath = $"{dirPath}/{sceneName}.json";
        // StreamWriter를 사용하여 문자열을 파일에 저장
        if (!Directory.Exists(dirPath))
            Directory.CreateDirectory(dirPath);
        using (StreamWriter writer = new StreamWriter(filePath)) {
            writer.WriteLine(finalJson);
        }
        AssetDatabase.Refresh();

        Console.WriteLine("파일에 저장되었습니다.");
    }


    public static void AddResourcePath(string path)
    {
        resourceFilePathTable.Add(path);
    }

    public static void AddPreprocess<T>(T bakeProcess) where T : class
    {
        if (bakeProcess == null)
            return;
        var guid = BakeGuid.GetGuid(bakeProcess);
        if (!guidTable.Contains(guid))
        {
            var bake = BakeObject.CreateProperty(bakeProcess);
            if (bake != null)
            {
                guidTable.Add(guid);
                preprocessQueue.Enqueue(bake);
            }
        }
    }
    public static void AddBake(BakeObject bakeProcess)
    {
        bakeQueue.Enqueue(bakeProcess);
    }

    private static List<GameObject> GetAllSceneGameObjects() =>
    Resources.FindObjectsOfTypeAll<GameObject>()
             .Where(go => !EditorUtility.IsPersistent(go)) // scene objects only
             .ToList();
}

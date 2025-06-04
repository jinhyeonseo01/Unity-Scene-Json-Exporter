using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;
using Newtonsoft.Json.Linq;
using System;
using UnityEditor;
using System.IO;
<<<<<<< Updated upstream
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
=======

>>>>>>> Stashed changes

[InitializeOnLoad]
public class BakeUnity : MonoBehaviour
{
    //[OdinSerialize]
    public static string definePath_Resources = "Assets/";
    public static string definePath_ConfigResources = "Assets/";
    public static Dictionary<string, string> nameToPathTable = new Dictionary<string, string>(4092);

    public static List<GameObject> refList_GameObject;
    public static List<Material> refList_Material;
    public static List<Component> refList_Component;

    [NonSerialized, HideInInspector]
    protected static string finalJson;
    //[OdinSerialize]
    public static string exportPath = "./Assets/Exports/";
    public static string exportResourcePath = "./Assets/Exports/";

    public static HashSet<string> resourceFilePathTable;

    //[Button]
    public static void Baking()
    {
        InitBake();
        JObject totalJson;
        InitJson(out totalJson);

        var childsAll = FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.InstanceID);
        var childsAllRemoveList = childsAll
        .Where(e => e.name.Contains("##"))
        .ToList();

        refList_GameObject = FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.InstanceID)
            .Where(e => !e.name.Contains("#"))
            .ToList();
        foreach (var gameObject in childsAllRemoveList)
        {
            var list = new List<Transform>();
            gameObject.GetComponentsInChildren(true, list);
            foreach (var child in list)
                refList_GameObject.Remove(child.gameObject);
        }
        if (refList_GameObject.Count == 0)
            return;

        SharedAnimationFBX.models.Clear();

        for (int i = 0; i < refList_GameObject.Count; i++)
            PrevProcessingGameObject(refList_GameObject[i]);
        for (int i = 0; i < refList_Component.Count; i++)
            PrevProcessingComponent(refList_Component[i]);
        for (int i = 0; i < refList_Material.Count; i++)
            PrevProcessingMaterial(refList_Material[i]);

        Debug.Log($"total GameObject : {refList_GameObject.Count}");
        Debug.Log($"total Component : {refList_Component.Count}");
        Debug.Log($"total Material : {refList_Material.Count}");

        for (int i = 0; i < refList_GameObject.Count; i++)
            BakeGameObject(totalJson, refList_GameObject[i]);
        for (int i = 0; i < refList_Component.Count; i++)
            BakeComponent(totalJson, refList_Component[i]);
        for (int i = 0; i < refList_Material.Count; i++)
            BakeMaterial(totalJson, refList_Material[i]);
        for (int i = 0; i < refList_GameObject.Count; i++)
            BakeObject(totalJson, refList_GameObject[i]);

        foreach (var gameObject in childsAll)
            foreach (var sharedAnim in gameObject.GetComponentsInChildren<SharedAnimationFBX>(true))
                sharedAnim.BakeBone();

        totalJson.Add("AnimationBoneMapping", JObject.FromObject(SharedAnimationFBX.allBoneMappings));

        List<string> animationList = new List<string>();
        foreach (var gameObject in childsAll)
        {
            foreach (var sharedAnim in gameObject.GetComponentsInChildren<SharedAnimationFBX>(true))
            {
                var list = sharedAnim.sharedModelsData.models;
                foreach (var obj in list)
                {
                    if (!obj.IsUnityNull() && obj != null)
                    {
                        var path = AssetDatabase.GetAssetPath(obj).Trim();
                        if (!string.IsNullOrEmpty(path))
                        {
                            BakeUnity.filePathSet.Add(path);
                            path = path.Replace("Assets/", BakeUnity.definePath_Resources);

                            var fileName = path.Split("/").Last();
                            var name = string.Join(".", fileName.Split(".")[0..^1]);
                            if (!animationList.Contains(path))
                                animationList.Add(path);
                            //data["path"] = path;
                        }
                    }
                }

                foreach (var path2 in SharedAnimationFBX.models)
                {
                    BakeUnity.filePathSet.Add(path2);
                    var path = path2.Replace("Assets/", BakeUnity.definePath_Resources);
                    var fileName = path.Split("/").Last();
                    var name = string.Join(".", fileName.Split(".")[0..^1]);
                    if (!animationList.Contains(path))
                        animationList.Add(path);
                }
            }
        }
        totalJson.Add("AnimationModels", JArray.FromObject(animationList));


        finalJson = totalJson.ToString();

        Debug.Log($"total Json Line : {finalJson.Count((e) => e == '\n')}");

        Scene scene = SceneManager.GetActiveScene();
        Save(finalJson, scene.name);
    }

    //[Button]
    private static void Save(string data, string name)
    {
        var sceneName = name;
        Debug.Log($"Baking Name : {sceneName}");

        string dirPath = exportPath;// Path.GetDirectoryName(exportPath);
        string filePath = $"{dirPath}/{sceneName}.json";
        // StreamWriter�� ����Ͽ� ���ڿ��� ���Ͽ� ����
        if (!Directory.Exists(dirPath))
            Directory.CreateDirectory(dirPath);
        using (StreamWriter writer = new StreamWriter(filePath)) {
            writer.WriteLine(finalJson);
        }
        AssetDatabase.Refresh();

        Console.WriteLine("���Ͽ� ����Ǿ����ϴ�.");
    }

    //[Button]
    public static void SelectBaking()
    {
        InitBake();
        JObject totalJson;
        InitJson(out totalJson);


        List<GameObject> selectedObjects = new List<GameObject>(8192);
        GameObject[] selecteds = Selection.gameObjects;
        foreach (GameObject go in selecteds)
        {
            Transform[] allTransforms = go.GetComponentsInChildren<Transform>();
            foreach (Transform child in allTransforms) {
                    selectedObjects.Add(child.gameObject);
            }
        }

        var childsAll = selectedObjects;
        var childsAllRemoveList = childsAll
        .Where(e => e.name.Contains("##"))
        .ToList();

        refList_GameObject = selectedObjects
            .Where(e => !e.name.Contains("#"))
            .ToList();
        foreach (var gameObject in childsAllRemoveList)
        {
            var list = new List<Transform>();
            gameObject.GetComponentsInChildren(true, list);
            foreach (var child in list)
                refList_GameObject.Remove(child.gameObject);
        }
        if (refList_GameObject.Count == 0)
            return;

        SharedAnimationFBX.models.Clear();

        for (int i = 0; i < refList_GameObject.Count; i++)
            PrevProcessingGameObject(refList_GameObject[i]);
        for (int i = 0; i < refList_Component.Count; i++)
            PrevProcessingComponent(refList_Component[i]);
        for (int i = 0; i < refList_Material.Count; i++)
            PrevProcessingMaterial(refList_Material[i]);

        Debug.Log($"total GameObject : {refList_GameObject.Count}");
        Debug.Log($"total Component : {refList_Component.Count}");
        Debug.Log($"total Material : {refList_Material.Count}");

        for (int i = 0; i < refList_GameObject.Count; i++)
            BakeGameObject(totalJson, refList_GameObject[i]);
        for (int i = 0; i < refList_Component.Count; i++)
            BakeComponent(totalJson, refList_Component[i]);
        for (int i = 0; i < refList_Material.Count; i++)
            BakeMaterial(totalJson, refList_Material[i]);


        foreach (var gameObject in childsAll)
            foreach (var sharedAnim in gameObject.GetComponentsInChildren<SharedAnimationFBX>(true))
                sharedAnim.BakeBone();

        totalJson.Add("AnimationBoneMapping", JObject.FromObject(SharedAnimationFBX.allBoneMappings));

        List<string> animationList = new List<string>();
        foreach (var gameObject in childsAll)
        {
            foreach (var sharedAnim in gameObject.GetComponentsInChildren<SharedAnimationFBX>(true))
            {
                var list = sharedAnim.sharedModelsData.models;
                foreach (var obj in list)
                {
                    if (!obj.IsUnityNull() && obj != null)
                    {
                        var path = AssetDatabase.GetAssetPath(obj).Trim();
                        if (!string.IsNullOrEmpty(path))
                        {
                            BakeUnity.filePathSet.Add(path);
                            path = path.Replace("Assets/", BakeUnity.definePath_Resources);

                            var fileName = path.Split("/").Last();
                            var name = string.Join(".", fileName.Split(".")[0..^1]);
                            if (!animationList.Contains(path))
                                animationList.Add(path);
                            //data["path"] = path;
                        }
                    }
                }
                foreach (var path2 in SharedAnimationFBX.models)
                {
                    BakeUnity.filePathSet.Add(path2);
                    var path = path2.Replace("Assets/", BakeUnity.definePath_Resources);
                    var fileName = path.Split("/").Last();
                    var name = string.Join(".", fileName.Split(".")[0..^1]);
                    if (!animationList.Contains(path))
                        animationList.Add(path);
                }
            }
        }
        totalJson.Add("AnimationModels", JArray.FromObject(animationList));


        finalJson = totalJson.ToString();

        Debug.Log($"total Json Line : {finalJson.Count((e) => e == '\n')}");

        //UnityEngine.SceneManagement.Scene scene = SceneManager.GetActiveScene();
        Save(finalJson, $"{Selection.gameObjects[0].name} with {Selection.gameObjects.Length}Count");
    }
    public static void CopyResources()
    {
        var filePathList = resourceFilePathTable.Where(e => !string.IsNullOrEmpty(e)).ToList();

        foreach (var path in filePathList)
        {
            // ������ ������ �����ϴ��� Ȯ��
            if (File.Exists(path))
            {
                // 4. "Assets/" ���ξ� ����
                string relativePath = path;
                const string prefix = "Assets/";
                if (relativePath.StartsWith(prefix))
                {
                    relativePath = relativePath.Substring(prefix.Length);
                }
                else
                    Debug.Log(path);
                // 5. ����� ���� ��ο� �����Ͽ� ���� ��� ��� ����
                string destinationPath = Path.Combine(BakeUnity.exportResourcePath, relativePath);

                // 6. ��� ���丮 ��� Ȯ�� �� ������ ����
                string destDir = Path.GetDirectoryName(destinationPath);
                if (!Directory.Exists(destDir))
                {
                    Directory.CreateDirectory(destDir);
                    Debug.Log($"���丮 ����: {destDir}");
                }

                // 7. ���� ���� (overwrite�� true�̸� ���� ���� ���)
                File.Copy(path, destinationPath, true);
            }
            else
            {
                Debug.LogWarning($"������ ã�� �� ����: {path}");
            }
        }
        
        AssetDatabase.Refresh();
        Debug.Log($"���� ���� �Ϸ� : Count - {filePathList.Count}");
    }
    public static void InitBake()
    {
        refList_GameObject ??= new List<GameObject>(8192);
        refList_GameObject.Clear();

        refList_Material ??= new List<Material>(8192);
        refList_Material.Clear();

        refList_Component ??= new List<Component>(8192);
        refList_Component.Clear();

        nameToPathTable = new Dictionary<string, string>(8192);
        nameToPathTable.Clear();

        resourceFilePathTable ??= new HashSet<string>(8192);
        resourceFilePathTable.Clear();

        global::BakeObject.Init();

        var setting = FindAnyObjectByType<BakeSetting>();
        if(setting != null)
            setting.PathUpdate();
    }
    public static void InitJson(out JObject json)
    {
        json = new JObject();
        json["references"] ??= new JObject();
        //json["path"] ??= new JObject();
        //json["path"]["resources"] ??= new JArray();

        var refJson = json["references"];
        (refJson as JObject)["GameObjects"] ??= new JArray();
        (refJson as JObject)["Materials"] ??= new JArray();
        (refJson as JObject)["Components"] ??= new JArray();
        //obj["references"] ??= new JArray();
    }
<<<<<<< Updated upstream
=======

    public static void Baking(ref JObject totalJson)
    {
        for (int i = 0; i < refList_GameObject.Count; i++)
            PrevProcessingGameObject(refList_GameObject[i]);
        for (int i = 0; i < refList_Component.Count; i++)
            PrevProcessingComponent(refList_Component[i]);
        for (int i = 0; i < refList_Material.Count; i++)
            PrevProcessingMaterial(refList_Material[i]);
        for (int i = 0; i < refList_Material.Count; i++)
            PrevProcessingMaterial(refList_Material[i]);

        PrevProcessingModel();

        Debug.Log($"total GameObject : {refList_GameObject.Count}");
        Debug.Log($"total Component : {refList_Component.Count}");
        Debug.Log($"total Material : {refList_Material.Count}");

        for (int i = 0; i < refList_GameObject.Count; i++)
            BakeGameObject(totalJson, refList_GameObject[i]);
        for (int i = 0; i < refList_Component.Count; i++)
            BakeComponent(totalJson, refList_Component[i]);
        for (int i = 0; i < refList_Material.Count; i++)
            BakeMaterial(totalJson, refList_Material[i]);
        //for (int i = 0; i < refList_GameObject.Count; i++)
        //BakeObject(totalJson, refList_GameObject[i]);

        BakeModel(totalJson, refList_GameObject);
    }

    //[Button]
    private static void Save(string data, string name)
    {
        var sceneName = name;
        Debug.Log($"Baking Name : {sceneName}");

        string dirPath = exportPath;// Path.GetDirectoryName(exportPath);
        string filePath = $"{dirPath}/{sceneName}.json";
        // StreamWriter�� ����Ͽ� ���ڿ��� ���Ͽ� ����
        if (!Directory.Exists(dirPath))
            Directory.CreateDirectory(dirPath);
        using (StreamWriter writer = new StreamWriter(filePath))
        {
            writer.WriteLine(finalJson);
        }
        AssetDatabase.Refresh();

        Console.WriteLine("���Ͽ� ����Ǿ����ϴ�.");
    }

    public static void AddResourcePath(string path)
    {
        resourceFilePathTable.Add(path);
    }


>>>>>>> Stashed changes
    public static void PrevProcessingPrefab(GameObject prefab)
    {
        foreach (var trans in prefab.GetComponentsInChildren<Transform>()) {
            var gameObject = trans.gameObject;
            PrevProcessingGameObject(gameObject);
        }
    }
    public static void PrevProcessingGameObject(GameObject gameObject)
    {
        if(!refList_GameObject.Contains(gameObject))
            refList_GameObject.Add(gameObject);

        refList_Component.AddRange(gameObject.GetComponents<Component>()
            .Where(e => e != null)
            .ToList());

    }
    public static void PrevProcessingComponent(Component component)
    {
        //gameObjectToBakeTable.TryGetValue(component.gameObject, out var bakingInfo);
        BakeGuid.SetGuid(component);
        var property = global::BakeObject.CreateProperty(component);
        if(property != null)
            property.Preprocess();

    }
    public static void PrevProcessingMaterial(Material material)
    {
        BakeGuid.SetGuid(material);
        global::BakeObject.CreateProperty(material);
    }

    public static void BakeGameObject(JObject prevJson, GameObject gameObject)
    {
        JObject objJson = new JObject();
        objJson["name"] = gameObject.name;
        objJson["guid"] = BakeGuid.GetGuid(gameObject);
        objJson["active"] = gameObject.activeSelf;
        objJson["static"] = gameObject.isStatic;
        objJson["deactivate"] = gameObject.layer == LayerMask.NameToLayer("Deactivate");


        objJson["components"] ??= new JArray();
        objJson["childs"] ??= new JArray();
        objJson["parent"] = "";

        //----------------------------------------
        objJson["parent"] = BakeGuid.GetGuid(gameObject.transform.parent);

        for (int i = 0; i < gameObject.transform.childCount; i++)
        {
            (objJson["childs"] as JArray).Add(BakeGuid.GetGuid(gameObject.transform.GetChild(i).gameObject));
        }

        foreach (var component in gameObject.GetComponents<Component>())
        {
            (objJson["components"] as JArray).Add(BakeGuid.GetGuid(component));
        }


        var typeKey = "GameObjects";
        var refJson = prevJson["references"];
        (refJson as JObject)[typeKey] ??= new JArray();
        ((refJson as JObject)[typeKey] as JArray)?.Add(objJson);
    }
    public static void BakeComponent(JObject prevJson, Component component)
    {
        var refJson = prevJson["references"];
        (refJson as JObject)["Components"] ??= new JArray();

        var bakeComponent = BakeObject.GetProperty(component);
        if(bakeComponent != null)
            ((refJson as JObject)["Components"] as JArray).Add(bakeComponent.Bake());
    }
    public static void BakeMaterial(JObject prevJson, Material obj)
    {
        var refJson = prevJson["references"];
        (refJson as JObject)["Materials"] ??= new JArray();

        var bake = BakeObject.GetProperty(obj);
        if (bake != null)
            ((refJson as JObject)["Materials"] as JArray).Add(bake.Bake());
    }

<<<<<<< Updated upstream
    public static void BakeObject<T>(JObject prevJson, T t) where T : class
    {
        //prevJson.Add("type", "GameObject");
        //JObject objJson = new JObject();
        //Debug.Log(prevJson.ToString());
    }
    public static bool TryGetGuid<T>(T obj, out string guid) where T : class
    {
        guid = "null";
        if (obj != null && objectToGuidTable.TryGetValue(obj, out guid))
            return true;
        return false;
    }
    public static string TrySetGuid<T>(T obj) where T : class
    {
        if (obj == null)
            return "null";
        if (!objectToGuidTable.TryGetValue(obj, out var value))
            return objectToGuidTable[obj] = NewGuid();
        return value;
    }
    public static string SetGuidAndUpdate<T>(T obj, string guid) where T : class
    {
        return objectToGuidTable[obj] = guid;
    }
    public static string NewGuid()
    {
        return System.Guid.NewGuid().ToString();
=======

    public static void BakeModel(JObject prevJson, List<GameObject> objectList)
    {

        List<string> animationList = new List<string>();
        foreach (var gameObject in objectList)
        {
            foreach (var sharedAnim in gameObject.GetComponentsInChildren<BakeFBXModel>(true))
                sharedAnim.BakeBone();

            foreach (var sharedAnim in gameObject.GetComponentsInChildren<BakeFBXModel>(true))
            {
                foreach (var sharedModelsData in sharedAnim.modelsTableList)
                {
                    var list = sharedModelsData.models;
                    foreach (var obj in list)
                    {
                        if (obj != null)
                        {
                            var path = AssetDatabase.GetAssetPath(obj).Trim();
                            if (!string.IsNullOrEmpty(path))
                            {
                                BakeUnity.resourceFilePathTable.Add(path);
                                path = path.Replace("Assets/", BakeUnity.definePath_Resources);

                                var fileName = path.Split("/").Last();
                                var name = string.Join(".", fileName.Split(".")[0..^1]);
                                if (!animationList.Contains(path))
                                    animationList.Add(path);
                            }
                        }
                    }

                    foreach (var path2 in BakeFBXModel.modelPathList)
                    {
                        BakeUnity.resourceFilePathTable.Add(path2);
                        var path = path2.Replace("Assets/", BakeUnity.definePath_Resources);
                        var fileName = path.Split("/").Last();
                        var name = string.Join(".", fileName.Split(".")[0..^1]);
                        if (!animationList.Contains(path))
                            animationList.Add(path);
                    }
                }
            }
        }
        prevJson.Add("Models", JArray.FromObject(animationList));
        prevJson.Add("AnimationBoneMappingTable", JObject.FromObject(BakeFBXModel.allBoneMappings));
>>>>>>> Stashed changes
    }
}

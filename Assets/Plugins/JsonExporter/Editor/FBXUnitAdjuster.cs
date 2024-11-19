// Assets/Editor/FBXUnitAdjuster.cs
using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEditor.Formats.Fbx.Exporter; // FBX Exporter ��Ű�� ���ӽ����̽�
using UnityEngine.Formats.Fbx.Exporter;
using System.Collections.Generic;

public class FBXUnitAdjuster : EditorWindow
{
    // �⺻ ����
    private string folderPath = "Assets/Models"; // FBX ������ ��ġ�� �⺻ ����

    [MenuItem("Tools/FBX Unit Adjuster")]
    public static void ShowWindow()
    {
        GetWindow<FBXUnitAdjuster>("FBX Unit Adjuster");
    }

    private string ConvertToRelativePath(string absolutePath)
    {
        if (absolutePath.StartsWith(Application.dataPath))
        {
            return "Assets" + absolutePath.Substring(Application.dataPath.Length);
        }
        else
        {
            Debug.LogWarning("���õ� ��ΰ� ������Ʈ ���� ���� �����ϴ�.");
            return "";
        }
    }

    private void OnGUI()
    {
        GUILayout.Label("FBX Unit Adjuster", EditorStyles.boldLabel);
        GUILayout.Space(10);


        EditorGUILayout.BeginHorizontal();
        folderPath = EditorGUILayout.TextField("���� ���", folderPath);

        if (GUILayout.Button("ã�ƺ���", GUILayout.Width(70)))
        {
            string selectedPath = EditorUtility.OpenFolderPanel("���� ����", "", "");
            if (!string.IsNullOrEmpty(selectedPath))
            {
                folderPath = ConvertToRelativePath(selectedPath);
            }
        }
        EditorGUILayout.EndHorizontal();


        GUILayout.Space(20);

        // ���� ��ư
        if (GUILayout.Button("Adjust Units"))
        {
            AdjustUnits();
        }
    }

    private void AdjustUnits()
    {
        // ���� ��� ��ȿ�� �˻�
        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            EditorUtility.DisplayDialog("Error", "Invalid folder path.", "OK");
            return;
        }

        // ���� �� ��� FBX ���� ã��
        string[] fbxFiles = Directory.GetFiles(folderPath, "*.fbx", SearchOption.AllDirectories);
        if (fbxFiles.Length == 0)
        {
            EditorUtility.DisplayDialog("Info", "No FBX files found in the specified folder.", "OK");
            return;
        }

        // ���� ��Ȳ ǥ�ø� ���� ���α׷��� �� ����
        int totalFiles = fbxFiles.Length;
        int currentFile = 0;

        // ���� ������ 1�� �����ϱ� ���� ������ ���� ���
        float targetUnitScale = 1.0f; // ��ǥ ���� ������ (1 ���� = 1m)

        // ó���� ���� ��� ���� (���� �� ����)
        List<string> successFiles = new List<string>();
        List<string> failedFiles = new List<string>();

        foreach (string filePath in fbxFiles)
        {
            currentFile++;
            EditorUtility.DisplayProgressBar("Adjusting FBX Units", $"Processing {Path.GetFileName(filePath)} ({currentFile}/{totalFiles})", (float)currentFile / totalFiles);

            string assetPath = filePath;
            ModelImporter importer = AssetImporter.GetAtPath(assetPath) as ModelImporter;

            if (importer == null)
            {
                Debug.LogWarning($"Failed to get ModelImporter for {assetPath}");
                failedFiles.Add(assetPath);
                continue;
            }


            //GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            //var prefabs = FindPrefabsUsingFBX(assetPath);
            //Debug.Log(assetPath);
            //Debug.Log(prefabs.Count);


            // ���� �۷ι� ������ ��������
            float existingScale = importer.globalScale;
            //importer.globalScale = 1.0f;
            importer.globalScale = 1.0f;
            //importer.useFileUnits = true;

            // ���� ���� ���� �� ������Ʈ
            importer.SaveAndReimport();

            // FBX Exporter�� ����Ͽ� ������ ���� ����Ʈ�� GameObject�κ��� �ٽ� FBX�� �ͽ���Ʈ
            GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);

            if (go == null)
            {
                Debug.LogWarning($"Failed to load GameObject for {assetPath}");
                failedFiles.Add(assetPath);
                continue;
            }

            // �ӽ� ��� ����
            string tempExportPath = Path.Combine(Path.GetDirectoryName(filePath), Path.GetFileNameWithoutExtension(filePath) + ".fbx");


            string metaFilePath = tempExportPath + ".meta";

            // ���� .meta ���� ���
            string backupMetaFilePath = metaFilePath + ".bak";
            if (File.Exists(metaFilePath))
            {
                File.Copy(metaFilePath, backupMetaFilePath, true);
            }


            // �ͽ���Ʈ �õ�
            go.transform.localScale = go.transform.localScale * existingScale;
            OnPostprocessModel(go);
            ExportModelOptions o = new ExportModelOptions();
            o.ExportFormat = ExportFormat.ASCII;
            o.UseMayaCompatibleNames = true;
            o.PreserveImportSettings = true;
            ModelExporter.ExportObject(tempExportPath, go, o);
            successFiles.Add(filePath);


            if (File.Exists(backupMetaFilePath))
            {
                File.Copy(backupMetaFilePath, metaFilePath, true);
                File.Delete(backupMetaFilePath);

                // Unity ���� �����ͺ��̽� ����
                AssetDatabase.ImportAsset(tempExportPath);
            }


            go = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            importer.SaveAndReimport();
            //var objs = FindPrefabs(assetPath);
            //for (int i = 0; i < objs.Count; i++)
            //objs[i].transform.localScale = go.transform.localScale;
            //foreach (var prefab in prefabs)
            //{
            //UpdatePrefabWithFBX(prefab, go);
            //}
        }
        AssetDatabase.Refresh();

        // ���α׷��� �� ����
        EditorUtility.ClearProgressBar();

        // ��� ���
        string summary = $"Unit adjustment completed.\n\nTotal Files: {totalFiles}\nSuccessfully Adjusted: {successFiles.Count}\nFailed: {failedFiles.Count}";
        if (failedFiles.Count > 0)
        {
            summary += "\n\nFailed Files:\n";
            foreach (string fail in failedFiles)
            {
                summary += $"- {fail}\n";
            }
        }

        EditorUtility.DisplayDialog("FBX Unit Adjuster", summary, "OK");
    }

    void OnPostprocessModel(GameObject g)
    {
        // ��� �ڽ� ��ü�� ���� �޽� �̸��� ����
        MeshFilter[] meshFilters = g.GetComponentsInChildren<MeshFilter>();
        foreach (MeshFilter meshFilter in meshFilters)
        {
            if (meshFilter.sharedMesh != null)
            {
                // ���ϴ� �޽� �̸����� ����
                meshFilter.gameObject.name = meshFilter.sharedMesh.name;  // GameObject �̸��� �����ϰ� ����
            }
        }
    }


    // FBX�� �����ϴ� Prefab �˻�
    private List<GameObject> FindPrefabsUsingFBX(string fbxPath)
    {
        var prefabList = new List<GameObject>();
        string[] allPrefabs = AssetDatabase.FindAssets("t:Prefab");

        foreach (string prefabGUID in allPrefabs)
        {
            string prefabPath = AssetDatabase.GUIDToAssetPath(prefabGUID);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

            // FBX�� �����ϴ� Prefab���� Ȯ��
            if (PrefabReferencesFBX(prefab, fbxPath))
            {
                prefabList.Add(prefab);
            }
        }

        return prefabList;
    }

    // Prefab�� Ư�� FBX�� �����ϴ��� Ȯ��
    private bool PrefabReferencesFBX(GameObject prefab, string fbxPath)
    {
        var renderers = prefab.GetComponentsInChildren<MeshFilter>(true);
        foreach (var renderer in renderers)
        {
            //Debug.Log(renderer.sharedMesh);
            //Debug.Log(renderer.mesh);
            if (renderer.sharedMesh != null)
            {
                if (renderer.sharedMesh != null && AssetDatabase.GetAssetPath(renderer.sharedMesh) == AssetDatabase.GetAssetPath(AssetDatabase.LoadAssetAtPath<Object>(fbxPath)))
                {
                    return true;
                }
            }
        }

        return false;
    }


    private void UpdatePrefabWithFBX(GameObject prefab, GameObject importedModel)
    {
        // FBX�� Mesh �����͸� Prefab�� ����
        Debug.Log(prefab);
        Debug.Log(importedModel);
        var prefabInstances = prefab.GetComponentsInChildren<Transform>(true);
        var importedInstances = importedModel.GetComponentsInChildren<Transform>(true);
        foreach (var prefabInstance in prefabInstances)
        {
            foreach (var importedInstance in importedInstances)
            {
                if (prefabInstance.name == importedInstance.name)
                {
                    // MeshRenderer ���� ������Ʈ
                    var prefabRenderer = prefabInstance.GetComponent<MeshFilter>();
                    var importedRenderer = importedInstance.GetComponent<MeshFilter>();
                    if (prefabRenderer != null && importedRenderer != null)
                    {
                        //prefabRenderer.sha
                        prefabRenderer.sharedMesh = importedRenderer.sharedMesh;
                    }
                }
            }
        }

        // Prefab ����
        PrefabUtility.SavePrefabAsset(prefab);
        Debug.Log($"Updated Prefab: {prefab.name} with updated FBX: {importedModel.name}");
    }










    public static List<GameObject> FindPrefabs(string fbxPath)
    {
        // FBX ������ ��� ���� (Project â�� FBX ���� ���)
        Object fbxAsset = AssetDatabase.LoadAssetAtPath<Object>(fbxPath);
        List<GameObject> gameObjects = new List<GameObject>();
        if (fbxAsset == null)
        {
            Debug.LogError($"FBX file not found at path: {fbxPath}");
            return null;
        }

        // ��� Prefab �˻�
        string[] allPrefabs = AssetDatabase.FindAssets("t:Prefab");

        Debug.Log($"Searching for Prefabs using FBX: {fbxPath}");
        foreach (string prefabGUID in allPrefabs)
        {
            string prefabPath = AssetDatabase.GUIDToAssetPath(prefabGUID);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

            if (PrefabUsesFBX(prefab, fbxAsset))
            {
                gameObjects.Add(prefab);
            }
        }
        return gameObjects;
    }

    private static bool PrefabUsesFBX(GameObject prefab, Object fbxAsset)
    {
        if (prefab == null || fbxAsset == null)
            return false;

        // Prefab ������ ��� MeshFilter �� SkinnedMeshRenderer�� �˻�
        var meshFilters = prefab.GetComponentsInChildren<MeshFilter>(true);
        foreach (var meshFilter in meshFilters)
        {
            if (meshFilter.sharedMesh != null && AssetDatabase.GetAssetPath(meshFilter.sharedMesh) == AssetDatabase.GetAssetPath(fbxAsset))
            {
                return true;
            }
        }

        var skinnedMeshRenderers = prefab.GetComponentsInChildren<SkinnedMeshRenderer>(true);
        foreach (var skinnedMeshRenderer in skinnedMeshRenderers)
        {
            if (skinnedMeshRenderer.sharedMesh != null && AssetDatabase.GetAssetPath(skinnedMeshRenderer.sharedMesh) == AssetDatabase.GetAssetPath(fbxAsset))
            {
                return true;
            }
        }

        return false;
    }
}
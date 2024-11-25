// Assets/Editor/FBXUnitAdjuster.cs
using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEditor.Formats.Fbx.Exporter; // FBX Exporter ��Ű�� ���ӽ����̽�
using System.Collections.Generic;
using System;
using System.Linq;

public class FBXUnitAdjuster : EditorWindow
{
    // �⺻ ����
    private string folderPath = "Assets/Models"; // FBX ������ ��ġ�� �⺻ ����

    [MenuItem("Tools/FBX Exporter/FBX Unit Fix Tool")]
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
    public Dictionary<int, int> meshChangeIDTable = new Dictionary<int, int>();
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
        Debug.Log(fbxFiles);

        meshChangeIDTable ??= new Dictionary<int, int>();
        meshChangeIDTable.Clear();

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

            try
            {

                // ���� �۷ι� ������ ��������
                float existingScale = importer.globalScale;
                //importer.globalScale = 1.0f;
                importer.globalScale = 1.0f;
                importer.useFileUnits = true;
                importer.bakeAxisConversion = true;
                importer.useFileScale = true;
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


                Dictionary<MeshFilter, int> meshPrevIDTable = new Dictionary<MeshFilter, int>();
                Dictionary<SkinnedMeshRenderer, int> meshSkinnedPrevIDTable = new Dictionary<SkinnedMeshRenderer, int>();
                MeshFilter[] meshFilters = go.GetComponentsInChildren<MeshFilter>(true);
                foreach (var meshFilter in meshFilters)
                    meshPrevIDTable.Add(meshFilter, meshFilter.sharedMesh.GetInstanceID());

                SkinnedMeshRenderer[] meshSkinneds = go.GetComponentsInChildren<SkinnedMeshRenderer>(true);
                foreach (var meshSkinned in meshSkinneds)
                    meshSkinnedPrevIDTable.Add(meshSkinned, meshSkinned.sharedMesh.GetInstanceID());


                // �ӽ� ��� ����
                string tempExportPath = Path.Combine(Path.GetDirectoryName(filePath), Path.GetFileNameWithoutExtension(filePath) + ".fbx");
                Debug.Log(tempExportPath);

                string metaFilePath = tempExportPath + ".meta";
                string rand = System.Guid.NewGuid().ToString();
                // ���� .meta ���� ���
                string backupMetaFilePath = tempExportPath + rand + ".bak";
                if (File.Exists(metaFilePath))
                {
                    File.Copy(metaFilePath, backupMetaFilePath, true);
                }

                // �ͽ���Ʈ �õ�
                go.transform.localScale = go.transform.localScale * existingScale;
                //OnPostprocessModel(go);
                ExportModelOptions o = new ExportModelOptions();
                o.UseMayaCompatibleNames = true;
                o.PreserveImportSettings = true;
                ModelExporter.ExportObject(tempExportPath, go, o);
                successFiles.Add(filePath);


                if (File.Exists(backupMetaFilePath))
                {
                    File.Copy(backupMetaFilePath, metaFilePath, true);
                    File.Delete(backupMetaFilePath);
                }
                AssetDatabase.ImportAsset(tempExportPath, ImportAssetOptions.ForceUpdate);


                go = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                meshFilters = go.GetComponentsInChildren<MeshFilter>(true);

                foreach (var meshFilter in meshFilters)
                    if (meshPrevIDTable.ContainsKey(meshFilter))
                        meshChangeIDTable.Add(meshPrevIDTable[meshFilter], meshFilter.sharedMesh.GetInstanceID());
                    else
                        Debug.Log($"Not Found : {meshFilter.sharedMesh.GetInstanceID()}");

                meshSkinneds = go.GetComponentsInChildren<SkinnedMeshRenderer>(true);
                foreach (var meshSkinned in meshSkinneds)
                    if (meshSkinnedPrevIDTable.ContainsKey(meshSkinned))
                        meshChangeIDTable.Add(meshSkinnedPrevIDTable[meshSkinned], meshSkinned.sharedMesh.GetInstanceID());
                    else
                        Debug.Log($"Not Found : {meshSkinned.sharedMesh.GetInstanceID()}");


            }
            finally
            {
            }


            //var objs = FindPrefabs(assetPath);
            //for (int i = 0; i < objs.Count; i++)
            //objs[i].transform.localScale = go.transform.localScale;
            //foreach (var prefab in prefabs)
            //{
            //UpdatePrefabWithFBX(prefab, go);
            //}
        }
        AssetDatabase.Refresh();

        foreach (var meshID in meshChangeIDTable)
            Debug.Log($"{meshID.Key} : {meshID.Value}");
        FixMesh();
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

    public void FixMesh()
    {
        var refList_GameObject = FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.InstanceID)
            .Where(e => !e.name.Contains("#"))
            .ToList();

        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets" });

        List<GameObject> prefabs = new List<GameObject>();
        Dictionary<GameObject, string> prefabPaths = new Dictionary<GameObject, string>();

        foreach (string guid in prefabGuids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (prefab != null)
            {
                prefabs.Add(prefab);
                prefabPaths.Add(prefab, assetPath);
            }
        }
        refList_GameObject.AddRange(prefabs);

        for (int i = 0; i < refList_GameObject.Count; i++)
            FixMeshLinks(refList_GameObject[i]);
        foreach (var prefab in prefabPaths)
            PrefabUtility.SaveAsPrefabAsset(prefab.Key, prefab.Value);
    }
    public void FixMeshLinks(GameObject rootObject)
    {
        // ��� MeshFilter�� Ž��
        MeshFilter[] meshFilters = rootObject.GetComponentsInChildren<MeshFilter>();

        foreach (var meshFilter in meshFilters)
        {
            if (meshFilter.sharedMesh == null)
            {
                SerializedObject serializedObject = new SerializedObject(meshFilter);
                SerializedProperty meshProperty = serializedObject.FindProperty("m_Mesh");
                if (meshProperty != null)
                {
                    if (meshChangeIDTable.ContainsKey(meshProperty.objectReferenceInstanceIDValue))
                    {
                        Debug.Log($"������ ���� : {meshProperty.objectReferenceInstanceIDValue}");
                        meshProperty.objectReferenceInstanceIDValue = meshChangeIDTable[meshProperty.objectReferenceInstanceIDValue];
                        serializedObject.ApplyModifiedProperties();
                    }
                }

            }
        }


        SkinnedMeshRenderer[] meshSkinneds = rootObject.GetComponentsInChildren<SkinnedMeshRenderer>();

        foreach (var meshSkinned in meshSkinneds)
        {
            if (meshSkinned.sharedMesh == null)
            {
                SerializedObject serializedObject = new SerializedObject(meshSkinned);
                SerializedProperty meshProperty = serializedObject.FindProperty("m_Mesh");
                if (meshProperty != null)
                {
                    if (meshChangeIDTable.ContainsKey(meshProperty.objectReferenceInstanceIDValue))
                    {
                        Debug.Log($"������ ���� : {meshProperty.objectReferenceInstanceIDValue}");
                        meshProperty.objectReferenceInstanceIDValue = meshChangeIDTable[meshProperty.objectReferenceInstanceIDValue];
                        serializedObject.ApplyModifiedProperties();
                    }
                }

            }
        }
    }

}
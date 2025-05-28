// Assets/Editor/FBXUnitAdjuster.cs
using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEditor.Formats.Fbx.Exporter; // FBX Exporter 패키지 네임스페이스
using System.Collections.Generic;
using System;
using System.Linq;

public class FBXUnitAdjuster : EditorWindow
{
    // 기본 설정
    private string folderPath = "Assets/Models"; // FBX 파일이 위치한 기본 폴더

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
            Debug.LogWarning("선택된 경로가 프로젝트 폴더 내에 없습니다.");
            return "";
        }
    }

    private void OnGUI()
    {
        GUILayout.Label("FBX Unit Adjuster", EditorStyles.boldLabel);
        GUILayout.Space(10);


        EditorGUILayout.BeginHorizontal();
        folderPath = EditorGUILayout.TextField("파일 경로", folderPath);

        if (GUILayout.Button("찾아보기", GUILayout.Width(70)))
        {
            string selectedPath = EditorUtility.OpenFolderPanel("파일 선택", "", "");
            if (!string.IsNullOrEmpty(selectedPath))
            {
                folderPath = ConvertToRelativePath(selectedPath);
            }
        }
        EditorGUILayout.EndHorizontal();


        GUILayout.Space(20);

        // 실행 버튼
        if (GUILayout.Button("Adjust Units"))
        {
            AdjustUnits();
        }
    }
    public Dictionary<int, int> meshChangeIDTable = new Dictionary<int, int>();
    private void AdjustUnits()
    {
        // 폴더 경로 유효성 검사
        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            EditorUtility.DisplayDialog("Error", "Invalid folder path.", "OK");
            return;
        }

        // 폴더 내 모든 FBX 파일 찾기
        string[] fbxFiles = Directory.GetFiles(folderPath, "*.fbx", SearchOption.AllDirectories);
        if (fbxFiles.Length == 0)
        {
            EditorUtility.DisplayDialog("Info", "No FBX files found in the specified folder.", "OK");
            return;
        }

        // 진행 상황 표시를 위한 프로그레스 바 설정
        int totalFiles = fbxFiles.Length;
        int currentFile = 0;


        // 처리된 파일 목록 저장 (성공 및 실패)
        List<string> successFiles = new List<string>();
        List<string> failedFiles = new List<string>();
        Debug.Log(fbxFiles.Length + " Fbx File Found");

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

                // 기존 글로벌 스케일 가져오기
                //float existingScale = importer.globalScale;
                //importer.globalScale = 1.0f;
                //importer.useFileUnits = true;
                //if(importer.bakeAxisConversion)
                //    importer.bakeAxisConversion = false;
                //else
                //    importer.bakeAxisConversion = true;

                //importer.useFileScale = true;
                //// 변경 사항 저장 및 재임포트
                //importer.SaveAndReimport();

                // FBX Exporter를 사용하여 수정된 모델을 임포트한 GameObject로부터 다시 FBX로 익스포트
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


                // 임시 경로 설정
                string tempExportPath = Path.Combine(Path.GetDirectoryName(filePath), Path.GetFileNameWithoutExtension(filePath) + ".fbx");
                //Debug.Log(tempExportPath);

                //string metaFilePath = tempExportPath + ".meta";
                //string rand = System.Guid.NewGuid().ToString();
                //// 기존 .meta 파일 백업
                //string backupMetaFilePath = tempExportPath + rand + ".bak";
                //if (File.Exists(metaFilePath))
                //{
                //    File.Copy(metaFilePath, backupMetaFilePath, true);
                //}

                // 익스포트 시도
                //go.transform.localScale = go.transform.localScale * existingScale;
                ////OnPostprocessModel(go);
                ExportModelOptions o = new ExportModelOptions();
                o.UseMayaCompatibleNames = true;
                o.PreserveImportSettings = true;


                ModelExporter.ExportObject(tempExportPath, go, o);
                successFiles.Add(filePath);


                //if (File.Exists(backupMetaFilePath))
                //{
                //    File.Copy(backupMetaFilePath, metaFilePath, true);
                //    File.Delete(backupMetaFilePath);
                //}
                //AssetDatabase.ImportAsset(tempExportPath, ImportAssetOptions.ForceUpdate);

                //importer = AssetImporter.GetAtPath(assetPath) as ModelImporter;
                //if(!importer.bakeAxisConversion)
                //    importer.bakeAxisConversion = true;

                //importer.SaveAndReimport();


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
            if(meshID.Key != meshID.Value)
                Debug.Log($"{meshID.Key} -> {meshID.Value}");
        FixMesh();
        // 프로그레스 바 해제
        EditorUtility.ClearProgressBar();

        // 결과 요약
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
        // 모든 자식 객체에 대해 메쉬 이름을 수정
        MeshFilter[] meshFilters = g.GetComponentsInChildren<MeshFilter>();
        foreach (MeshFilter meshFilter in meshFilters)
        {
            if (meshFilter.sharedMesh != null)
            {
                // 원하는 메쉬 이름으로 변경
                meshFilter.gameObject.name = meshFilter.sharedMesh.name;  // GameObject 이름과 동일하게 설정
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
        // 모든 MeshFilter를 탐색
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
                        Debug.Log($"재참조 성공 : {meshProperty.objectReferenceInstanceIDValue}");
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
                        Debug.Log($"재참조 성공 : {meshProperty.objectReferenceInstanceIDValue}");
                        meshProperty.objectReferenceInstanceIDValue = meshChangeIDTable[meshProperty.objectReferenceInstanceIDValue];
                        serializedObject.ApplyModifiedProperties();
                    }
                }

            }
        }
    }

}
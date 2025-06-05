// Assets/Editor/FBXUnitAdjuster.cs
using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEditor.Formats.Fbx.Exporter; // FBX Exporter 패키지 네임스페이스
using System.Collections.Generic;
using System;
using System.Linq;
using Autodesk.Fbx;
using Codice.CM.Common;
using Unity.VisualScripting;

public class FBXUnitFix : EditorWindow
{
    // 기본 설정
    private string folderPath = "Assets/Models"; // FBX 파일이 위치한 기본 폴더

    [MenuItem("Tools/Json Exporter/Editor Macro Expansions/FBX Unit Fix Tool")]
    public static void ShowWindow()
    {
        GetWindow<FBXUnitFix>("FBX Unit Fix");
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
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("FBX Unit Fix", EditorStyles.boldLabel);
        GUILayout.Space(3);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        {
            string tooltipText =
                "A tool that automatically corrects FBX unit scale factor and reorders coordinate axes " +
                "so Unity’s imported model, animation, and transform data match exactly how they were authored in the original FBX file.";
            EditorGUILayout.HelpBox(tooltipText, MessageType.Info);
            GUILayout.Space(10);
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        {
            folderPath = EditorGUILayout.TextField("File Folder Path", folderPath);

            if (GUILayout.Button("Browse", GUILayout.Width(70)))
            {
                string selectedPath = EditorUtility.OpenFolderPanel("Select Folder", "", "");
                if (!string.IsNullOrEmpty(selectedPath))
                {
                    folderPath = ConvertToRelativePath(selectedPath);
                }
            }
        }
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(20);

        // Execute button
        if (GUILayout.Button("Fix Models in Folder"))
        {
            FixUnits();
        }
    }
    readonly Dictionary<string, string> meshGuidMap = new(); // old → new
    readonly Dictionary<string, float> scaleRatioMap = new(); // FBX  → 배율

    /* ───────── 메인 루프 ───────── */
    void FixUnits()
    {
        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            EditorUtility.DisplayDialog("Error", "Invalid folder.", "OK");
            return;
        }

        string[] fbxFiles = Directory.GetFiles(folderPath, "*.fbx", SearchOption.AllDirectories);
        if (fbxFiles.Length == 0)
        {
            EditorUtility.DisplayDialog("Info", "No FBX in folder.", "OK");
            return;
        }

        meshGuidMap.Clear();
        scaleRatioMap.Clear();
        List<string> failed = new();

        AssetDatabase.StartAssetEditing();
        try
        {
            for (int i = 0; i < fbxFiles.Length; i++)
            {
                EditorUtility.DisplayProgressBar(
                    "Fixing FBX",
                    $"{i + 1}/{fbxFiles.Length}\n{fbxFiles[i]}",
                    (i + 1f) / fbxFiles.Length);

                if (!FixSingleFbx(fbxFiles[i]))
                    failed.Add(fbxFiles[i]);
            }
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
            EditorUtility.ClearProgressBar();
        }

        AssetDatabase.Refresh();

        RelinkMeshesInSceneAndPrefabs();

        EditorUtility.DisplayDialog(
            "FBX Fixer",
            $"Finished.\nTotal: {fbxFiles.Length}\nFailed: {failed.Count}",
            "OK");
    }

    /* ───────── FBX 하나 처리 ───────── */
    bool FixSingleFbx(string assetPath)
    {
        var orgGO = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
        if (!orgGO) return false;

        var oldInfo = CollectMeshInfo(orgGO);

        var importer = AssetImporter.GetAtPath(assetPath) as ModelImporter;
        if (!importer) return false;

        float prevScale = importer.globalScale;
        bool prevUseFile = importer.useFileScale;
        bool needScale = (prevScale != 1f) || !prevUseFile;

        if (!needScale) return true;

        if (needScale)
        {
            importer.globalScale = 1f;
            importer.useFileScale = true;
        }

        importer.SaveAndReimport();   // 새 Mesh 생성

        /* FBX Exporter로 새 파일(마야 네임) 생성 후 원본 교체 */
        string tempPath = Path.Combine(
            Path.GetDirectoryName(assetPath),
            Path.GetFileNameWithoutExtension(assetPath) + "_fixed.fbx");

        var exportOpt = new ExportModelOptions
        {
            UseMayaCompatibleNames = true,
            PreserveImportSettings = true
        };
        ModelExporter.ExportObject(tempPath, orgGO, exportOpt);
        FileUtil.ReplaceFile(tempPath, assetPath);
        AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);

        AssetDatabase.DeleteAsset(tempPath);

        /* 새 GUID 매핑 */
        var fixedGO = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
        var newInfo = CollectMeshInfo(fixedGO); // 이름 → {guid, vertCount}

        foreach (var kv in oldInfo)          // kv.Key = MeshKey, kv.Value = oldGuid
        {
            if (newInfo.TryGetValue(kv.Key, out string newGuid))
            {
                if (kv.Value != newGuid)     // 실제로 바뀌었을 때만
                    meshGuidMap[kv.Value] = newGuid;
            }
            else
            {
                Debug.LogWarning($"Mesh not found after reimport: {kv.Key.name}", orgGO);
            }
        }

        /* 스케일 역보정 기록 */
        if (needScale && !Mathf.Approximately(prevScale, 1f))
            scaleRatioMap[AssetDatabase.AssetPathToGUID(assetPath)] = prevScale;

        return true;
    }

    /* ───────── 메쉬 GUID 수집 ───────── */
    struct MeshKey
    {
        public string name;
        public int vert;
        public int sub;
        public int tris;
        public int hash;   // 추가 안전장치

        public override int GetHashCode() => hash;
        public override bool Equals(object obj)
            => obj is MeshKey other &&
               name == other.name &&
               vert == other.vert &&
               sub == other.sub &&
                tris == other.tris;
    }

    MeshKey MakeKey(Mesh m)
    {
        int triTotal = 0;
        for (int i = 0; i < m.subMeshCount; i++)
            triTotal += (int)m.GetIndexCount(i) / 3;

        var key = new MeshKey
        {
            name = m.name,
            vert = m.vertexCount,
            sub = m.subMeshCount,
            tris = triTotal,
        };
        key.hash = ComputeHash(key);   // 빠른 32-bit hash
        return key;
    }

    static int ComputeHash(MeshKey k)
    {
        unchecked
        {
            int h = 17;
            h = h * 23 + k.name.GetHashCode();
            h = h * 23 + k.vert;
            h = h * 23 + k.sub;
            h = h * 23 + k.tris;
            return h;
        }
    }

    Dictionary<MeshKey, string> CollectMeshInfo(GameObject root)
    {
        var dict = new Dictionary<MeshKey, string>();
        void Add(Mesh m)
        {
            if (!m) return;
            var key = MakeKey(m);
            string g = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(m));
            dict[key] = g;
        }

        foreach (var mf in root.GetComponentsInChildren<MeshFilter>(true)) Add(mf.sharedMesh);
        foreach (var smr in root.GetComponentsInChildren<SkinnedMeshRenderer>(true)) Add(smr.sharedMesh);
        return dict;
    }

    /* ───────── 씬 + 프리팹 일괄 재연결 ───────── */
    void RelinkMeshesInSceneAndPrefabs()
    {
        /* 현재 열린 씬 */
        foreach (var go in Resources.FindObjectsOfTypeAll<GameObject>())
            if (!EditorUtility.IsPersistent(go))
                FixMeshRefs(go);

        /* 모든 프리팹 */
        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets" });
        for (int i = 0; i < prefabGuids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(prefabGuids[i]);
            EditorUtility.DisplayProgressBar(
                "Relinking Prefabs",
                $"{i + 1}/{prefabGuids.Length}\n{path}",
                (i + 1f) / prefabGuids.Length);

            var pf = PrefabUtility.LoadPrefabContents(path);
            bool changed = FixMeshRefs(pf);
            if (changed) PrefabUtility.SaveAsPrefabAsset(pf, path);
            PrefabUtility.UnloadPrefabContents(pf);
        }
        EditorUtility.ClearProgressBar();
    }

    /* ───────── 루트 1개 처리 ───────── */
    bool FixMeshRefs(GameObject root)
    {
        bool modified = false;

        /* 스케일 역보정 */
        string fbxGuid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(root));
        if (scaleRatioMap.TryGetValue(fbxGuid, out float ratio) &&
            !Mathf.Approximately(ratio, 1f))
        {
            root.transform.localScale *= ratio;
            modified = true;
        }

        foreach (var mf in root.GetComponentsInChildren<MeshFilter>(true))
            modified |= ReplaceMissingMesh(mf);
        foreach (var smr in root.GetComponentsInChildren<SkinnedMeshRenderer>(true))
            modified |= ReplaceMissingMesh(smr);

        return modified;
    }

    bool ReplaceMissingMesh(Component comp)
    {
        using var so = new SerializedObject(comp);
        var meshProp = so.FindProperty("m_Mesh");
        if (meshProp == null) return false;

        string oldGuid = meshProp.objectReferenceValue
            ? AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(meshProp.objectReferenceValue))
            : meshProp.FindPropertyRelative("m_GUID")?.stringValue;


        if (!string.IsNullOrEmpty(oldGuid) &&
            meshGuidMap.TryGetValue(oldGuid, out string newGuid))
        {
            string newPath = AssetDatabase.GUIDToAssetPath(newGuid);
            Mesh newMesh = AssetDatabase.LoadAssetAtPath<Mesh>(newPath);
            if (newMesh != null)
            {
                meshProp.objectReferenceValue = newMesh;
                so.ApplyModifiedProperties();
                return true;
            }
        }
        return false;
    }

}

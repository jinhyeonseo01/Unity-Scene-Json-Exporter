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
            
            // ���� �۷ι� ������ ��������
            float existingScale = importer.globalScale;
            importer.globalScale = 1.0f;

            // ���� ���� ���� �� ������Ʈ
            importer.SaveAndReimport();

            // FBX Exporter�� ����Ͽ� ������ ���� ����Ʈ�� GameObject�κ��� �ٽ� FBX�� �ͽ���Ʈ
            GameObject gos = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);

            GameObject go = gos;
            if (go == null)
            {
                Debug.LogWarning($"Failed to load GameObject for {assetPath}");
                failedFiles.Add(assetPath);
                continue;
            }

            // �ӽ� ��� ����
            string tempExportPath = Path.Combine(Path.GetDirectoryName(filePath), Path.GetFileNameWithoutExtension(filePath) + ".fbx");

            // �ͽ���Ʈ �õ�
            go.transform.localScale = go.transform.localScale * existingScale;
            ModelExporter.ExportObject(tempExportPath, go);
            successFiles.Add(filePath);
        }

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
}
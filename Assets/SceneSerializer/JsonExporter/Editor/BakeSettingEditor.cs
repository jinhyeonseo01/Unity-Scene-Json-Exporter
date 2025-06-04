using UnityEngine;
using UnityEditor;
using System.IO;

[CustomEditor(typeof(BakeSetting))]
public class BakeSettingEditor : Editor
{

    private string ConvertToRelativePath(string absolutePath)
    {
        if (absolutePath.StartsWith(Application.dataPath))
        {
            return "Assets" + absolutePath.Substring(Application.dataPath.Length);
        }
        else
        {
            return absolutePath;
        }
    }

    public override void OnInspectorGUI()
    {
        // �⺻ �ν����� ǥ��
        DrawDefaultInspector();

        var boldStyle = new GUIStyle(EditorStyles.label)
        {
            fontStyle = FontStyle.Bold
        };
        // ��� ��ũ��Ʈ ��������
        BakeSetting myComponent = (BakeSetting)target;


        EditorGUILayout.BeginVertical();
            GUILayout.Space(EditorGUIUtility.singleLineHeight * 1);
            EditorGUILayout.LabelField("Json�� ������ ���", boldStyle);
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical();
            EditorGUILayout.BeginHorizontal();
            myComponent.jsonExportPath = EditorGUILayout.TextField("Export Json Path", myComponent.jsonExportPath);

            if (GUILayout.Button("ã�ƺ���", GUILayout.Width(60)))
            {
                string selectedPath = EditorUtility.OpenFolderPanel("���� ����", myComponent.jsonExportPath, "");
                if (!string.IsNullOrEmpty(selectedPath))
                {
                    myComponent.jsonExportPath = ConvertToRelativePath(selectedPath);
                }
            }
            if (GUILayout.Button("����", GUILayout.Width(40)))
            {
                if (Directory.Exists(myComponent.jsonExportPath))
                {
                    // ������ �����ݴϴ�.
                    EditorUtility.RevealInFinder(myComponent.jsonExportPath + "/");
                    // �Ǵ� Windows�� ���:
                    // System.Diagnostics.Process.Start(myComponent.resourceExportPath);
                }
                else
                {
                    Debug.LogWarning("��ΰ� �������� �ʽ��ϴ�: " + myComponent.jsonExportPath);
                }
            }
            EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();


        EditorGUILayout.BeginVertical();
            GUILayout.Space(EditorGUIUtility.singleLineHeight * 0);
            EditorGUILayout.LabelField("���ҽ��� �����Ͽ� ������ ���", boldStyle);
        EditorGUILayout.EndVertical();


        EditorGUILayout.BeginVertical();
            EditorGUILayout.BeginHorizontal();
            myComponent.resourceExportPath = EditorGUILayout.TextField("Export Resources Path", myComponent.resourceExportPath);

            if (GUILayout.Button("ã�ƺ���", GUILayout.Width(60)))
            {
                string selectedPath = EditorUtility.OpenFolderPanel("���� ����", myComponent.resourceExportPath, "");
                if (!string.IsNullOrEmpty(selectedPath))
                {
                    myComponent.resourceExportPath = ConvertToRelativePath(selectedPath);
                }
            }
            if (GUILayout.Button("����", GUILayout.Width(40)))
            {
                if (Directory.Exists(myComponent.resourceExportPath))
                {
                    // ������ �����ݴϴ�.
                    EditorUtility.RevealInFinder(myComponent.resourceExportPath+"/");
                    // �Ǵ� Windows�� ���:
                    // System.Diagnostics.Process.Start(myComponent.resourceExportPath);
                }
                else
                {
                    Debug.LogWarning("��ΰ� �������� �ʽ��ϴ�: " + myComponent.resourceExportPath);
                }
            }
            EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();


        GUILayout.Space(20);

        // ��ư �߰�
        if (GUILayout.Button("Path Info Update"))
        {
            myComponent.PathUpdate();
        }
    }
}
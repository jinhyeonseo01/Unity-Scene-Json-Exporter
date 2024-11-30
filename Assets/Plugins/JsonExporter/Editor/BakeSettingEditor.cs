using UnityEngine;
using UnityEditor;

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

        // ��� ��ũ��Ʈ ��������
        BakeSetting myComponent = (BakeSetting)target;
        EditorGUILayout.BeginHorizontal();

        myComponent.jsonExportPath = EditorGUILayout.TextField("Export Path", myComponent.jsonExportPath);

        if (GUILayout.Button("ã�ƺ���", GUILayout.Width(70)))
        {
            string selectedPath = EditorUtility.OpenFolderPanel("���� ����", "", "");
            if (!string.IsNullOrEmpty(selectedPath))
            {
                myComponent.jsonExportPath = ConvertToRelativePath(selectedPath);
            }
        }
        EditorGUILayout.EndHorizontal();


        GUILayout.Space(20);

        // ��ư �߰�
        if (GUILayout.Button("Path Info Update"))
        {
            myComponent.PathUpdate();
        }
    }
}
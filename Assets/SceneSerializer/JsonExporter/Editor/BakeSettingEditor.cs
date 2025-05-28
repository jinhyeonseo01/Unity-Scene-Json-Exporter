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
        // 기본 인스펙터 표시
        DrawDefaultInspector();

        var boldStyle = new GUIStyle(EditorStyles.label)
        {
            fontStyle = FontStyle.Bold
        };
        // 대상 스크립트 가져오기
        BakeSetting myComponent = (BakeSetting)target;


        EditorGUILayout.BeginVertical();
            GUILayout.Space(EditorGUIUtility.singleLineHeight * 1);
            EditorGUILayout.LabelField("Json을 내보낼 경로", boldStyle);
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical();
            EditorGUILayout.BeginHorizontal();
            myComponent.jsonExportPath = EditorGUILayout.TextField("Export Json Path", myComponent.jsonExportPath);

            if (GUILayout.Button("찾아보기", GUILayout.Width(60)))
            {
                string selectedPath = EditorUtility.OpenFolderPanel("파일 선택", myComponent.jsonExportPath, "");
                if (!string.IsNullOrEmpty(selectedPath))
                {
                    myComponent.jsonExportPath = ConvertToRelativePath(selectedPath);
                }
            }
            if (GUILayout.Button("열기", GUILayout.Width(40)))
            {
                if (Directory.Exists(myComponent.jsonExportPath))
                {
                    // 폴더를 열어줍니다.
                    EditorUtility.RevealInFinder(myComponent.jsonExportPath + "/");
                    // 또는 Windows의 경우:
                    // System.Diagnostics.Process.Start(myComponent.resourceExportPath);
                }
                else
                {
                    Debug.LogWarning("경로가 존재하지 않습니다: " + myComponent.jsonExportPath);
                }
            }
            EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();


        EditorGUILayout.BeginVertical();
            GUILayout.Space(EditorGUIUtility.singleLineHeight * 0);
            EditorGUILayout.LabelField("리소스를 복사하여 내보낼 경로", boldStyle);
        EditorGUILayout.EndVertical();


        EditorGUILayout.BeginVertical();
            EditorGUILayout.BeginHorizontal();
            myComponent.resourceExportPath = EditorGUILayout.TextField("Export Resources Path", myComponent.resourceExportPath);

            if (GUILayout.Button("찾아보기", GUILayout.Width(60)))
            {
                string selectedPath = EditorUtility.OpenFolderPanel("파일 선택", myComponent.resourceExportPath, "");
                if (!string.IsNullOrEmpty(selectedPath))
                {
                    myComponent.resourceExportPath = ConvertToRelativePath(selectedPath);
                }
            }
            if (GUILayout.Button("열기", GUILayout.Width(40)))
            {
                if (Directory.Exists(myComponent.resourceExportPath))
                {
                    // 폴더를 열어줍니다.
                    EditorUtility.RevealInFinder(myComponent.resourceExportPath+"/");
                    // 또는 Windows의 경우:
                    // System.Diagnostics.Process.Start(myComponent.resourceExportPath);
                }
                else
                {
                    Debug.LogWarning("경로가 존재하지 않습니다: " + myComponent.resourceExportPath);
                }
            }
            EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();


        GUILayout.Space(20);

        // 버튼 추가
        if (GUILayout.Button("Path Info Update"))
        {
            myComponent.PathUpdate();
        }
    }
}
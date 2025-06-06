using UnityEngine;
using UnityEditor;
using System.IO;

namespace Clrain.SceneToJson
{
    [CustomEditor(typeof(ExportSetting))]
    public class ExportSettingEditor : Editor
    {
        private ExportSetting myComponent;
        private string jsonKey;
        private string resourceKey;

        private void OnEnable()
        {
            myComponent = (ExportSetting)target;

            // Use the asset path as part of the key so each ExportSetting instance has its own stored values
            string assetPath = AssetDatabase.GetAssetPath(myComponent);
            jsonKey = $"ExportSetting_{assetPath}_jsonPath";
            resourceKey = $"ExportSetting_{assetPath}_resourcePath";

            // Load saved values from EditorPrefs if they exist
            if (EditorPrefs.HasKey(jsonKey))
            {
                myComponent.jsonExportPath = EditorPrefs.GetString(jsonKey);
            }
            if (EditorPrefs.HasKey(resourceKey))
            {
                myComponent.resourceExportPath = EditorPrefs.GetString(resourceKey);
            }
        }

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
            myComponent = (ExportSetting)target;

            // Draw all other serialized fields normally
            DrawDefaultInspector();

            var boldStyle = new GUIStyle(EditorStyles.label)
            {
                fontStyle = FontStyle.Bold
            };

            GUILayout.Space(EditorGUIUtility.singleLineHeight);
            EditorGUILayout.LabelField("Json Export Path", boldStyle);

            EditorGUILayout.BeginHorizontal();
            {
                // Text field for jsonExportPath
                string newJsonPath = EditorGUILayout.TextField("Export Json Path", myComponent.jsonExportPath);
                if (newJsonPath != myComponent.jsonExportPath)
                {
                    myComponent.jsonExportPath = newJsonPath;
                    // Save immediately to EditorPrefs
                    EditorPrefs.SetString(jsonKey, newJsonPath);
                }

                if (GUILayout.Button("Browse", GUILayout.Width(60)))
                {
                    string selectedPath = EditorUtility.OpenFolderPanel("Select Folder", myComponent.jsonExportPath, "");
                    if (!string.IsNullOrEmpty(selectedPath))
                    {
                        string rel = ConvertToRelativePath(selectedPath);
                        myComponent.jsonExportPath = rel;
                        EditorPrefs.SetString(jsonKey, rel);
                    }
                }

                if (GUILayout.Button("Open", GUILayout.Width(40)))
                {
                    if (Directory.Exists(myComponent.jsonExportPath))
                    {
                        EditorUtility.RevealInFinder(myComponent.jsonExportPath + "/");
                    }
                    else
                    {
                        Debug.LogWarning("Path does not exist: " + myComponent.jsonExportPath);
                    }
                }
            }
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(EditorGUIUtility.singleLineHeight);
            EditorGUILayout.LabelField("Resource Export Path", boldStyle);

            EditorGUILayout.BeginHorizontal();
            {
                // Text field for resourceExportPath
                string newResourcePath = EditorGUILayout.TextField("Export Resources Path", myComponent.resourceExportPath);
                if (newResourcePath != myComponent.resourceExportPath)
                {
                    myComponent.resourceExportPath = newResourcePath;
                    EditorPrefs.SetString(resourceKey, newResourcePath);
                }

                if (GUILayout.Button("Browse", GUILayout.Width(60)))
                {
                    string selectedPath = EditorUtility.OpenFolderPanel("Select Folder", myComponent.resourceExportPath, "");
                    if (!string.IsNullOrEmpty(selectedPath))
                    {
                        string rel = ConvertToRelativePath(selectedPath);
                        myComponent.resourceExportPath = rel;
                        EditorPrefs.SetString(resourceKey, rel);
                    }
                }

                if (GUILayout.Button("Open", GUILayout.Width(40)))
                {
                    if (Directory.Exists(myComponent.resourceExportPath))
                    {
                        EditorUtility.RevealInFinder(myComponent.resourceExportPath + "/");
                    }
                    else
                    {
                        Debug.LogWarning("Path does not exist: " + myComponent.resourceExportPath);
                    }
                }
            }
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(20);

            if (GUILayout.Button("Path Info Update"))
            {
                myComponent.PathUpdate();
            }
        }
    }
}
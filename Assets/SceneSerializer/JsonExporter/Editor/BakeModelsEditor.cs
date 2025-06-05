#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(ModelList))]
public class BakeModelsEditor : Editor
{
    // ScriptableObject(=modelsTableList�� ���) �� �ش� ��ü�� �׸� Editor �ν��Ͻ��� ����
    private Dictionary<ScriptableObject, Editor> cachedEditors
        = new Dictionary<ScriptableObject, Editor>();

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        ModelList myTarget = (ModelList)target;
        if (myTarget.modelsTableList == null)
            return;

        int i = 0;
        foreach (var sharedModelsData in myTarget.modelsTableList)
        {
            if (sharedModelsData == null)
            {
                i++;
                continue;
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField($"Shared Models Data ({i})", EditorStyles.boldLabel);

            // 1) �ش� ScriptableObject�� �̹� ĳ�õ� �����Ͱ� �ִ��� Ȯ��
            if (!cachedEditors.TryGetValue(sharedModelsData, out var tempEditor)
                || tempEditor == null)
            {
                // 2) ������ ���� �����ؼ� ĳ�ÿ� �ִ´�
                CreateCachedEditor(sharedModelsData, null, ref tempEditor);
                cachedEditors[sharedModelsData] = tempEditor;
            }

            // 3) ����� Editor�� ����� Inspector�� �׸�
            if (tempEditor != null)
                tempEditor.OnInspectorGUI();

            i++;
        }
    }

    // Editor�� ��Ȱ��ȭ�ǰų� Inspectorâ�� ���� �� ĳ�õ� ������ ����
    private void OnDisable()
    {
        foreach (var kv in cachedEditors)
        {
            if (kv.Value != null)
                DestroyImmediate(kv.Value);
        }
        cachedEditors.Clear();
    }
}
#endif
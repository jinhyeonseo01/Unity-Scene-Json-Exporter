#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(ModelList))]
public class BakeModelsEditor : Editor
{
    // ScriptableObject(=modelsTableList의 요소) → 해당 객체를 그릴 Editor 인스턴스를 매핑
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

            // 1) 해당 ScriptableObject가 이미 캐시된 에디터가 있는지 확인
            if (!cachedEditors.TryGetValue(sharedModelsData, out var tempEditor)
                || tempEditor == null)
            {
                // 2) 없으면 새로 생성해서 캐시에 넣는다
                CreateCachedEditor(sharedModelsData, null, ref tempEditor);
                cachedEditors[sharedModelsData] = tempEditor;
            }

            // 3) 저장된 Editor를 사용해 Inspector를 그림
            if (tempEditor != null)
                tempEditor.OnInspectorGUI();

            i++;
        }
    }

    // Editor가 비활성화되거나 Inspector창이 닫힐 때 캐시된 에디터 정리
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
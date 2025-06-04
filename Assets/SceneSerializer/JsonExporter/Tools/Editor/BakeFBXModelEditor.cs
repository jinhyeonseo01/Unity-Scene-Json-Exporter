#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(BakeFBXModel))]
public class BakeFBXModelEditor : Editor
{
    // ScriptableObject 에디터를 캐싱할 변수
    private Editor sharedModelsDataEditor;

    public override void OnInspectorGUI()
    {
        // 기본 인스펙터 그리기
        DrawDefaultInspector();

        // 대상 스크립트 가져오기
        BakeFBXModel myTarget = (BakeFBXModel)target;

        if (myTarget.modelsTableList != null)
        {
            int i = 0;
            foreach (var sharedModelsData in myTarget.modelsTableList)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField($"Shared Models Data ({i})", EditorStyles.boldLabel);

                // 할당된 ScriptableObject의 인라인 에디터 생성 (캐싱)
                if (sharedModelsDataEditor == null)
                    CreateCachedEditor(sharedModelsData, null, ref sharedModelsDataEditor);

                // 인라인 에디터 그리기
                if (sharedModelsDataEditor != null)
                    sharedModelsDataEditor.OnInspectorGUI();
                i++;
            }
        }
    }
}
#endif
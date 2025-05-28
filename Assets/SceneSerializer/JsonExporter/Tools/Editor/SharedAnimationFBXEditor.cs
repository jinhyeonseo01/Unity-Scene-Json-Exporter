#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SharedAnimationFBX))]
public class SharedAnimationFBXEditor : Editor
{
    // ScriptableObject 에디터를 캐싱할 변수
    private Editor sharedModelsDataEditor;

    public override void OnInspectorGUI()
    {
        // 기본 인스펙터 그리기
        DrawDefaultInspector();

        // 대상 스크립트 가져오기
        SharedAnimationFBX myTarget = (SharedAnimationFBX)target;

        if (myTarget.sharedModelsData != null)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Shared Models Data", EditorStyles.boldLabel);

            // 할당된 ScriptableObject의 인라인 에디터 생성 (캐싱)
            if (sharedModelsDataEditor == null)
                CreateCachedEditor(myTarget.sharedModelsData, null, ref sharedModelsDataEditor);

            // 인라인 에디터 그리기
            if (sharedModelsDataEditor != null)
                sharedModelsDataEditor.OnInspectorGUI();
        }
    }
}
#endif
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SharedAnimationFBX))]
public class SharedAnimationFBXEditor : Editor
{
    // ScriptableObject �����͸� ĳ���� ����
    private Editor sharedModelsDataEditor;

    public override void OnInspectorGUI()
    {
        // �⺻ �ν����� �׸���
        DrawDefaultInspector();

        // ��� ��ũ��Ʈ ��������
        SharedAnimationFBX myTarget = (SharedAnimationFBX)target;

        if (myTarget.sharedModelsData != null)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Shared Models Data", EditorStyles.boldLabel);

            // �Ҵ�� ScriptableObject�� �ζ��� ������ ���� (ĳ��)
            if (sharedModelsDataEditor == null)
                CreateCachedEditor(myTarget.sharedModelsData, null, ref sharedModelsDataEditor);

            // �ζ��� ������ �׸���
            if (sharedModelsDataEditor != null)
                sharedModelsDataEditor.OnInspectorGUI();
        }
    }
}
#endif
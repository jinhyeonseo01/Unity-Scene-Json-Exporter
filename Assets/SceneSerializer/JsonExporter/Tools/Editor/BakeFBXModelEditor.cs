#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(BakeFBXModel))]
public class BakeFBXModelEditor : Editor
{
    // ScriptableObject �����͸� ĳ���� ����
    private Editor sharedModelsDataEditor;

    public override void OnInspectorGUI()
    {
        // �⺻ �ν����� �׸���
        DrawDefaultInspector();

        // ��� ��ũ��Ʈ ��������
        BakeFBXModel myTarget = (BakeFBXModel)target;

        if (myTarget.modelsTableList != null)
        {
            int i = 0;
            foreach (var sharedModelsData in myTarget.modelsTableList)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField($"Shared Models Data ({i})", EditorStyles.boldLabel);

                // �Ҵ�� ScriptableObject�� �ζ��� ������ ���� (ĳ��)
                if (sharedModelsDataEditor == null)
                    CreateCachedEditor(sharedModelsData, null, ref sharedModelsDataEditor);

                // �ζ��� ������ �׸���
                if (sharedModelsDataEditor != null)
                    sharedModelsDataEditor.OnInspectorGUI();
                i++;
            }
        }
    }
}
#endif
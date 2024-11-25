using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(BakeSetting))]
public class BakeSettingEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // �⺻ �ν����� ǥ��
        DrawDefaultInspector();

        // ��� ��ũ��Ʈ ��������
        BakeSetting myComponent = (BakeSetting)target;

        // ��ư �߰�
        if (GUILayout.Button("Path Info Update"))
        {
            myComponent.PathUpdate();
        }
    }
}
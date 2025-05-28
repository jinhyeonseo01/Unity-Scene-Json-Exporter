using UnityEngine;
using UnityEditor;

public class ClearHashFromObjectNamesRecursive
{
    // Tools �޴��� �׸� �߰�: ������ ������Ʈ �� ��� ���� ������Ʈ�� �̸����� '#' ���ڸ� ��� �����մϴ�.
    [MenuItem("Tools/Clear '#' from Object Names (Recursive)")]
    public static void ClearHashesRecursive()
    {
        int count = 0;
        // ���õ� ��� ��Ʈ ������Ʈ�� ���� ó���մϴ�.
        foreach (GameObject root in Selection.gameObjects)
        {
            // ���� ������Ʈ�� ������ ��� Transform�� ������ (��Ȱ�� ������Ʈ ����)
            Transform[] allTransforms = root.GetComponentsInChildren<Transform>(true);
            foreach (Transform t in allTransforms)
            {
                GameObject obj = t.gameObject;
                string oldName = obj.name;
                string newName = oldName.Replace("#", "");
                if (oldName != newName)
                {
                    // Undo ��� ����
                    Undo.RecordObject(obj, "Clear '#' from Object Name (Recursive)");
                    obj.name = newName;
                    count++;
                }
            }
        }
        Debug.Log($"�� {count}���� ������Ʈ �̸����� '#' ���ڸ� �����߽��ϴ�.");
    }
}
using UnityEngine;
using UnityEditor;

public class RemoveCollidersRecursive
{
    // Tools �޴��� �׸� �߰�: ������ ������Ʈ �� ��� ���� ������Ʈ���� 3D, 2D �ݶ��̴��� ��� �����մϴ�.
    [MenuItem("Tools/Remove All Colliders (Recursive)")]
    public static void RemoveAllColliders()
    {
        int count = 0;
        // ���õ� ��� ��Ʈ ������Ʈ�� ���� ó���մϴ�.
        foreach (GameObject root in Selection.gameObjects)
        {
            // 3D �ݶ��̴� ���� (��Ȱ�� ������Ʈ ����)
            Collider[] colliders3D = root.GetComponentsInChildren<Collider>(true);
            foreach (Collider col in colliders3D)
            {
                Undo.DestroyObjectImmediate(col);
                count++;
            }

            // 2D �ݶ��̴� ���� (��Ȱ�� ������Ʈ ����)
            Collider2D[] colliders2D = root.GetComponentsInChildren<Collider2D>(true);
            foreach (Collider2D col2d in colliders2D)
            {
                Undo.DestroyObjectImmediate(col2d);
                count++;
            }
        }
        Debug.Log($"�� {count}���� �ݶ��̴��� ���ŵǾ����ϴ�.");
    }
}
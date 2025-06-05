using UnityEngine;
using UnityEditor;
using System.Linq;

public class ClearHashFromObjectNamesRecursive
{
    // Tools �޴��� �׸� �߰�: ������ ������Ʈ �� ��� ���� ������Ʈ�� �̸����� '#' ���ڸ� ��� �����մϴ�.
    [MenuItem("Tools/Json Exporter/Exporter Expansions/Remove ��#�� from Selected GameObject Names (Recursive)")]
    public static void ClearHashesRecursive()
    {
        int count = 0;

        var list = Selection.gameObjects.ToList().SelectMany(e => e.GetComponentsInChildren<Transform>(true).Select(e => e.gameObject)).ToList();
        Undo.RecordObjects(list.ToArray(), "Remove ��#�� from Selected GameObject Names (Recursive)");
        list.ForEach(obj => {
                string oldName = obj.name;
                string newName = oldName.Replace("#", "");
                if (oldName != newName)
                {
                    obj.name = newName;
                    count++;
                }
            });
        Debug.Log($"Remove ��#�� from Selected GameObject Names (Recursive). Count({count})");
    }
}
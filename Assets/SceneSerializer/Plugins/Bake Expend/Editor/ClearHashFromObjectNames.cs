using UnityEngine;
using UnityEditor;
using System.Linq;

public class ClearHashFromObjectNamesRecursive
{
    // Tools 메뉴에 항목 추가: 선택한 오브젝트 및 모든 하위 오브젝트의 이름에서 '#' 문자를 모두 제거합니다.
    [MenuItem("Tools/Json Exporter/Exporter Expansions/Remove ‘#’ from Selected GameObject Names (Recursive)")]
    public static void ClearHashesRecursive()
    {
        int count = 0;

        var list = Selection.gameObjects.ToList().SelectMany(e => e.GetComponentsInChildren<Transform>(true).Select(e => e.gameObject)).ToList();
        Undo.RecordObjects(list.ToArray(), "Remove ‘#’ from Selected GameObject Names (Recursive)");
        list.ForEach(obj => {
                string oldName = obj.name;
                string newName = oldName.Replace("#", "");
                if (oldName != newName)
                {
                    obj.name = newName;
                    count++;
                }
            });
        Debug.Log($"Remove ‘#’ from Selected GameObject Names (Recursive). Count({count})");
    }
}
using UnityEngine;
using UnityEditor;

public class ClearHashFromObjectNamesRecursive
{
    // Tools 메뉴에 항목 추가: 선택한 오브젝트 및 모든 하위 오브젝트의 이름에서 '#' 문자를 모두 제거합니다.
    [MenuItem("Tools/Clear '#' from Object Names (Recursive)")]
    public static void ClearHashesRecursive()
    {
        int count = 0;
        // 선택된 모든 루트 오브젝트에 대해 처리합니다.
        foreach (GameObject root in Selection.gameObjects)
        {
            // 하위 오브젝트를 포함한 모든 Transform을 가져옴 (비활성 오브젝트 포함)
            Transform[] allTransforms = root.GetComponentsInChildren<Transform>(true);
            foreach (Transform t in allTransforms)
            {
                GameObject obj = t.gameObject;
                string oldName = obj.name;
                string newName = oldName.Replace("#", "");
                if (oldName != newName)
                {
                    // Undo 기록 남김
                    Undo.RecordObject(obj, "Clear '#' from Object Name (Recursive)");
                    obj.name = newName;
                    count++;
                }
            }
        }
        Debug.Log($"총 {count}개의 오브젝트 이름에서 '#' 문자를 제거했습니다.");
    }
}
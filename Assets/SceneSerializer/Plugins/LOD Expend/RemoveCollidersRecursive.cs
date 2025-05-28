using UnityEngine;
using UnityEditor;

public class RemoveCollidersRecursive
{
    // Tools 메뉴에 항목 추가: 선택한 오브젝트 및 모든 하위 오브젝트에서 3D, 2D 콜라이더를 모두 제거합니다.
    [MenuItem("Tools/Remove All Colliders (Recursive)")]
    public static void RemoveAllColliders()
    {
        int count = 0;
        // 선택된 모든 루트 오브젝트에 대해 처리합니다.
        foreach (GameObject root in Selection.gameObjects)
        {
            // 3D 콜라이더 제거 (비활성 오브젝트 포함)
            Collider[] colliders3D = root.GetComponentsInChildren<Collider>(true);
            foreach (Collider col in colliders3D)
            {
                Undo.DestroyObjectImmediate(col);
                count++;
            }

            // 2D 콜라이더 제거 (비활성 오브젝트 포함)
            Collider2D[] colliders2D = root.GetComponentsInChildren<Collider2D>(true);
            foreach (Collider2D col2d in colliders2D)
            {
                Undo.DestroyObjectImmediate(col2d);
                count++;
            }
        }
        Debug.Log($"총 {count}개의 콜라이더가 제거되었습니다.");
    }
}
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class ApplySelectedPrefabInstancesRecursive
{
    // Adds a menu item under Tools that scans the selected GameObjects and all their children, applying any prefab instances it finds.
    [MenuItem("Tools/Json Exporter/Editor Macro Expansions/Apply Selected Prefab Instances (Recursive)")]
    public static void ApplyPrefabsRecursive()
    {
        int appliedCount = 0;
        HashSet<GameObject> prefabRoots = new HashSet<GameObject>();

        // 선택된 모든 오브젝트 및 그 하위 오브젝트들을 순회
        foreach (GameObject root in Selection.gameObjects)
        {
            Transform[] allTransforms = root.GetComponentsInChildren<Transform>(true);
            foreach (Transform t in allTransforms)
            {
                GameObject go = t.gameObject;
                if (PrefabUtility.IsPartOfPrefabInstance(go))
                {
                    // 최상위 프리팹 인스턴스 루트를 가져와 중복되지 않도록 추가
                    GameObject prefabRoot = PrefabUtility.GetOutermostPrefabInstanceRoot(go);
                    prefabRoots.Add(prefabRoot);
                }
            }
        }

        // 수집된 모든 프리팹 인스턴스에 대해 어플라이 실행
        foreach (GameObject prefab in prefabRoots)
        {
            PrefabUtility.ApplyPrefabInstance(prefab, InteractionMode.AutomatedAction);
            appliedCount++;
        }

        Debug.Log($"총 {appliedCount}개의 프리팹 인스턴스가 어플라이되었습니다.");
    }
}
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

        // ���õ� ��� ������Ʈ �� �� ���� ������Ʈ���� ��ȸ
        foreach (GameObject root in Selection.gameObjects)
        {
            Transform[] allTransforms = root.GetComponentsInChildren<Transform>(true);
            foreach (Transform t in allTransforms)
            {
                GameObject go = t.gameObject;
                if (PrefabUtility.IsPartOfPrefabInstance(go))
                {
                    // �ֻ��� ������ �ν��Ͻ� ��Ʈ�� ������ �ߺ����� �ʵ��� �߰�
                    GameObject prefabRoot = PrefabUtility.GetOutermostPrefabInstanceRoot(go);
                    prefabRoots.Add(prefabRoot);
                }
            }
        }

        // ������ ��� ������ �ν��Ͻ��� ���� ���ö��� ����
        foreach (GameObject prefab in prefabRoots)
        {
            PrefabUtility.ApplyPrefabInstance(prefab, InteractionMode.AutomatedAction);
            appliedCount++;
        }

        Debug.Log($"�� {appliedCount}���� ������ �ν��Ͻ��� ���ö��̵Ǿ����ϴ�.");
    }
}
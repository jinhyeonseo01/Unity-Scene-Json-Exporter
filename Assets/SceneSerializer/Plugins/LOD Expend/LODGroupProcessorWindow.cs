using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Security.Cryptography;

public class LODGroupProcessorWindow : EditorWindow
{
    // 인스턴스 변수로 targetIndex를 저장합니다.
    private int targetIndex = 0;

    // 에디터 창을 열기 위한 메뉴 아이템
    [MenuItem("Tools/LODGroup Processor")]
    public static void ShowWindow()
    {
        GetWindow<LODGroupProcessorWindow>("LOD Group Processor");
    }

    // 에디터 창이 활성화될 때 EditorPrefs에서 targetIndex를 불러옵니다.
    private void OnEnable()
    {
        targetIndex = EditorPrefs.GetInt("LODGroupProcessor_TargetIndex", 0);
    }

    // 단축키 (Ctrl+Shift+L 또는 ⌘+Shift+L)를 이용하여 선택한 오브젝트 처리
    [MenuItem("Tools/Process LOD Groups %#L")]
    public static void ProcessSelectedObjects()
    {
        // EditorPrefs에서 최신 targetIndex 값을 불러옴
        int savedTargetIndex = EditorPrefs.GetInt("LODGroupProcessor_TargetIndex", 0);
        foreach (GameObject root in Selection.gameObjects)
        {
            // 선택한 오브젝트와 모든 하위 오브젝트들을 포함하여 순회 (비활성 오브젝트도 포함)
            Transform[] allTransforms = root.GetComponentsInChildren<Transform>(true);
            foreach (Transform t in allTransforms)
            {
                LODGroup lodGroup = t.GetComponent<LODGroup>();
                if (lodGroup != null)
                {
                    ProcessLODGroup(lodGroup, savedTargetIndex);
                }
            }
        }
        Debug.Log("처리 완료 - 선택한 LOD 인덱스: " + savedTargetIndex);
    }

    // 에디터 창의 GUI: LOD 인덱스를 설정할 수 있는 인풋 필드와 버튼을 제공
    private void OnGUI()
    {
        GUILayout.Label("LODGroup Processor", EditorStyles.boldLabel);

        // LOD 인덱스 입력 필드
        int newIndex = EditorGUILayout.IntField("LOD Index to Keep", targetIndex);
        if (newIndex != targetIndex)
        {
            targetIndex = newIndex;
            EditorPrefs.SetInt("LODGroupProcessor_TargetIndex", targetIndex);
        }

        // 버튼을 눌러서도 처리 가능
        if (GUILayout.Button("Process Selected Objects"))
        {
            ProcessSelectedObjects();
        }
    }

    // LODGroup 처리 메서드
    // 선택한 keepIndex(범위를 벗어나면 클램프 처리)에 해당하는 LOD의 렌더러와 동일한 게임 오브젝트가 다른 LOD에 포함되어 있으면, 해당 게임 오브젝트는 건드리지 않습니다.
    private static void ProcessLODGroup(LODGroup lodGroup, int keepIndex)
    {
        LOD[] lods = lodGroup.GetLODs();
        if (lods == null || lods.Length == 0)
            return;

        // 유효한 범위로 LOD 인덱스 클램프
        int skipIndex = Mathf.Clamp(keepIndex, 0, lods.Length - 1);

        // 선택된 LOD에 포함된 렌더러의 게임 오브젝트를 집합에 저장
        HashSet<GameObject> keepObjects = new HashSet<GameObject>();
        foreach (Renderer renderer in lods[skipIndex].renderers)
        {
            if (renderer != null && renderer.gameObject != null)
            {
                keepObjects.Add(renderer.gameObject);
            }
        }

        // 다른 LOD들을 순회하며, keepObjects에 포함되지 않은 경우에만 이름 앞에 "##"를 붙임
        for (int i = 0; i < lods.Length; i++)
        {
            if (i == skipIndex)
                continue;

            foreach (Renderer renderer in lods[i].renderers)
            {
                if (renderer != null && renderer.gameObject != null)
                {
                    // 만약 선택한 LOD에도 포함된 오브젝트라면 건드리지 않음
                    if (keepObjects.Contains(renderer.gameObject))
                        continue;

                    if (!renderer.gameObject.name.StartsWith("##"))
                    {
                        renderer.gameObject.name = "##" + renderer.gameObject.name;
                    }
                }
            }
        }
    }
}
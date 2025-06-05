#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Overlays;
using UnityEngine;
using UnityEngine.UIElements;

[Overlay(typeof(SceneView), "Snap to Floor", defaultDisplay = true)]
public class TerrainAlignerOverlay : Overlay
{
    public override VisualElement CreatePanelContent()
    {
        // 최상위 컨테이너
        var root = new VisualElement
        {
            style =
            {
                minWidth = 175,
                minHeight = 24,
                paddingTop = 4,
                paddingBottom = 4,
                paddingLeft = 6,
                paddingRight = 6,
                flexDirection = FlexDirection.Column
            }
        };

        // Foldout을 만들어서 “접기/펼치기” 기능을 추가
        var foldout = new Foldout
        {
            text = "Snap to Floor Settings",
            value = TerrainAlignerEditor.IsActive // 펼쳐진 상태 여부 초기값
        };

        // Foldout이 펼쳐졌을 때만 표시할 내부 내용 컨테이너
        var contentContainer = new VisualElement
        {
            style =
            {
                flexDirection = FlexDirection.Column,
                marginLeft = 4,       // 살짝 들여쓰기
                paddingBottom = 4
            }
        };

        // 내부 Label
        var label = new Label("Press 'U' to Snap Selected")
        {
            style =
            {
                unityTextAlign = TextAnchor.MiddleLeft,
                marginBottom = 4
            }
        };
        contentContainer.Add(label);

        // 내부 Toggle
        var toggle = new Toggle("Enable Snap to Floor")
        {
            value = TerrainAlignerEditor.IsActive,
            style =
            {
                unityTextAlign = TextAnchor.MiddleLeft
            }
        };
        toggle.RegisterValueChangedCallback(evt =>
        {
            TerrainAlignerEditor.IsActive = evt.newValue;
            // 메뉴 체크 상태 갱신
            EditorApplication.delayCall += () => Menu.SetChecked("Tools/Snap to Floor", TerrainAlignerEditor.IsActive);
            // 씬뷰 리페인트
            SceneView.RepaintAll();
        });
        contentContainer.Add(toggle);

        // Foldout 자식으로 내부 컨텐트 추가
        foldout.Add(contentContainer);

        // Foldout의 펼침/접힘 상태가 바뀔 때도 toggle 동기화
        foldout.RegisterValueChangedCallback(evt =>
        {
            // Foldout 펼침 상태가 끔(folded)일 때는 기능 비활성화
            if (!evt.newValue)
                TerrainAlignerEditor.IsActive = false;
            // 펼침 상태가 켜짐일 때는 이전 IsActive 상태 유지(이미 toggle이 처리)
            // 메뉴 체크 상태 갱신 및 씬뷰 리페인트
            EditorApplication.delayCall += () => Menu.SetChecked("Tools/Snap to Floor", TerrainAlignerEditor.IsActive);
            SceneView.RepaintAll();
        });

        // 최상위에 Foldout 추가
        root.Add(foldout);
        return root;
    }
}

[InitializeOnLoad]
public static class TerrainAlignerEditor
{
    private const string k_PrefKey = "TerrainAligner_IsActive";
    public static bool IsActive
    {
        get => EditorPrefs.GetBool(k_PrefKey, false);
        set => EditorPrefs.SetBool(k_PrefKey, value);
    }

    static TerrainAlignerEditor()
    {
        // 혹시 이미 구독된 게 있으면 해제 → 중복 구독 방지
        SceneView.duringSceneGui -= OnSceneGUI;
        SceneView.duringSceneGui += OnSceneGUI;
    }

    private static void OnSceneGUI(SceneView sceneView)
    {
        // 오버레이가 켜져 있지 않으면 아무 것도 안 함
        if (!IsActive)
            return;

        // 씬 뷰에 포커스가 없으면 키 입력 무시
        if (SceneView.lastActiveSceneView != sceneView || !sceneView.hasFocus)
            return;

        Event e = Event.current;
        // U 키(Shift/Alt/Ctrl 없이)를 눌렀을 때만 작동
        if (e.type == EventType.KeyDown && e.keyCode == KeyCode.U
            && !e.shift && !e.alt && !e.control)
        {
            AlignSelectedToTerrain();
            e.Use();
        }
    }

    private static void AlignSelectedToTerrain()
    {
        Transform[] selectedTransforms = Selection.transforms;
        if (selectedTransforms == null || selectedTransforms.Length == 0)
            return;

        // Undo 레코드
        Undo.RecordObjects(selectedTransforms, "Align Objects to Terrain via Vertex");

        // 미리 RaycastHit 배열을 재사용(NonAlloc 버전)
        RaycastHit[] hitBuffer = new RaycastHit[8];

        // 1. 선택된 객체들 및 자식들 모두를 HashSet에 담아, Raycast 결과에서 제외할 대상 판단
        var excludedRoots = new HashSet<Transform>();
        foreach (var root in selectedTransforms)
        {
            foreach (var t in root.GetComponentsInChildren<Transform>(includeInactive: true))
            {
                excludedRoots.Add(t);
            }
        }

        // 2. 모든 선택된 오브젝트의 바닥 샘플 포인트를 수집
        var allBottomPoints = new List<Vector3>();
        var allHitPoints = new List<Vector3>();
        var allHitNormals = new List<Vector3>();

        foreach (Transform root in selectedTransforms)
        {
            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(includeInactive: true);
            foreach (Renderer rend in renderers)
            {
                Mesh mesh = null;
                if (rend is MeshRenderer)
                {
                    var mf = rend.GetComponent<MeshFilter>();
                    if (mf != null) mesh = mf.sharedMesh;
                }
                else if (rend is SkinnedMeshRenderer smr)
                {
                    mesh = smr.sharedMesh;
                }
                if (mesh == null) continue;

                // 메시 바운딩 박스의 아래쪽 4개 꼭짓점 샘플링
                Bounds b = mesh.bounds;
                Vector3[] sampleLocal = new Vector3[4]
                {
                new Vector3(b.min.x, b.min.y, b.min.z),
                new Vector3(b.max.x, b.min.y, b.min.z),
                new Vector3(b.min.x, b.min.y, b.max.z),
                new Vector3(b.max.x, b.min.y, b.max.z),
                };

                foreach (var ptLocal in sampleLocal)
                {
                    Vector3 worldPos = rend.transform.TransformPoint(ptLocal);
                    allBottomPoints.Add(worldPos);

                    Ray ray = new Ray(worldPos + Vector3.up * 10f, Vector3.down);
                    int hitCount = Physics.RaycastNonAlloc(ray, hitBuffer, 100f);
                    if (hitCount > 0)
                    {
                        // 거리 기준으로 배열 정렬
                        Array.Sort(hitBuffer, 0, hitCount, Comparer<RaycastHit>.Create(
                            (a, b) => a.distance.CompareTo(b.distance)
                        ));

                        // 가장 가까운 valid hit을 찾아 추가 (자신의 콜라이더는 제외)
                        for (int i = 0; i < hitCount; i++)
                        {
                            var hit = hitBuffer[i];
                            if (hit.collider == null)
                                continue;

                            // 이 hit이 선택된 객체 또는 그 자식의 콜라이더라면 건너뜀
                            if (excludedRoots.Contains(hit.collider.transform))
                                continue;

                            allHitPoints.Add(hit.point);
                            allHitNormals.Add(hit.normal);
                            break;
                        }
                    }
                }
            }
        }

        if (allHitPoints.Count == 0 || allBottomPoints.Count == 0)
            return;

        // 3. 전체 바닥 샘플의 평균 위치 계산
        Vector3 avgBottom = Vector3.zero;
        foreach (var bp in allBottomPoints)
            avgBottom += bp;
        avgBottom /= allBottomPoints.Count;

        // 4. 전체 히트 위치의 평균과, 최빈 노멀 계산
        Vector3 avgHitPoint = Vector3.zero;
        var normalCounts = new Dictionary<Vector3, int>();
        for (int i = 0; i < allHitPoints.Count; i++)
        {
            avgHitPoint += allHitPoints[i];
            Vector3 rn = new Vector3(
                Mathf.Round(allHitNormals[i].x * 100f) / 100f,
                Mathf.Round(allHitNormals[i].y * 100f) / 100f,
                Mathf.Round(allHitNormals[i].z * 100f) / 100f
            );
            if (!normalCounts.ContainsKey(rn))
                normalCounts[rn] = 0;
            normalCounts[rn]++;
        }
        avgHitPoint /= allHitPoints.Count;

        Vector3 chosenNormal = Vector3.up;
        int maxCount = 0;
        foreach (var kvp in normalCounts)
        {
            if (kvp.Value > maxCount)
            {
                maxCount = kvp.Value;
                chosenNormal = kvp.Key;
            }
        }

        // 5. 선택된 각 오브젝트에 동일한 위치·회전 보정 적용
        foreach (Transform root in selectedTransforms)
        {
            // 기존 회전의 수평 방향 유지하며, 노멀이 기준인 회전 계산
            Vector3 forwardDir = Vector3.Cross(root.right, chosenNormal);
            if (forwardDir.sqrMagnitude < 0.001f)
                forwardDir = root.forward; // fallback
            Quaternion newRotation = Quaternion.LookRotation(forwardDir, chosenNormal);

            // 위치 보정: 평균 바닥 버텍스 위치와 평균 히트 위치 차이만큼 이동
            Vector3 newPosition = root.position + (avgHitPoint - avgBottom);

            root.rotation = newRotation;
            root.position = newPosition;
        }
    }
}
#endif
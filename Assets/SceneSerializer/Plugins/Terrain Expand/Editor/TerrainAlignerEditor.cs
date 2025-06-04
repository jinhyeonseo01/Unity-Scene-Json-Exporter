using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class TerrainAlignerEditor
{
    // 기능 활성화 여부
    private static bool isActive = false;
    // 드래그 가능한 패널 위치 및 크기
    private static Rect windowRect = new Rect(50, 10, 270, 60);

    static TerrainAlignerEditor()
    {
        SceneView.duringSceneGui += OnSceneGUI;
    }

    private static void OnSceneGUI(SceneView sceneView)
    {
        windowRect = GUI.Window(987654, windowRect, DoWindow, "Terrain Aligner (Vertex Based)");

        if (!isActive)
            return;

        Event e = Event.current;
        // U 키를 누르면 기능 실행 (여러 오브젝트에 적용)
        if (e.type == EventType.KeyDown && e.keyCode == KeyCode.U)
        {
            Transform[] selectedTransforms = Selection.transforms;
            if (selectedTransforms.Length == 0)
                return;

            // 여러 오브젝트에 대한 Undo 기록
            Undo.RecordObjects(selectedTransforms, "Align Objects to Terrain via Vertex");

            foreach (Transform root in selectedTransforms)
            {
                List<Vector3> candidateVertexWorldPositions = new List<Vector3>();
                List<Vector3> candidateHitPoints = new List<Vector3>();
                List<Vector3> candidateHitNormals = new List<Vector3>();

                Renderer[] renderers = root.GetComponentsInChildren<Renderer>();
                foreach (Renderer rend in renderers)
                {
                    Mesh mesh = null;
                    MeshFilter mf = rend.GetComponent<MeshFilter>();
                    if (mf != null)
                        mesh = mf.sharedMesh;
                    if (mesh == null)
                    {
                        SkinnedMeshRenderer smr = rend as SkinnedMeshRenderer;
                        if (smr != null)
                            mesh = smr.sharedMesh;
                    }
                    if (mesh == null)
                        continue;

                    Vector3[] vertices = mesh.vertices;
                    // 메쉬 로컬 좌표 기준 최소 y 값 계산
                    float minLocalY = float.MaxValue;
                    foreach (Vector3 v in vertices)
                    {
                        if (v.y < minLocalY)
                            minLocalY = v.y;
                    }

                    // tolerance 값 내에서 로컬 최소 y와 같은 버텍스들 선택
                    float tolerance = 0.02f;
                    foreach (Vector3 v in vertices)
                    {
                        if (Mathf.Abs(v.y - minLocalY) < tolerance)
                        {
                            // 로컬 좌표를 world 좌표로 변환
                            Vector3 worldPos = rend.transform.TransformPoint(v);
                            Ray ray = new Ray(worldPos + Vector3.up * 10, Vector3.down);
                            RaycastHit[] hits = Physics.RaycastAll(ray, 100f);
                            foreach (var hit in hits)
                            {
                                if (!hit.collider.transform.IsChildOf(root))
                                {
                                    candidateVertexWorldPositions.Add(worldPos);
                                    candidateHitPoints.Add(hit.point);
                                    candidateHitNormals.Add(hit.normal);
                                    break; // 첫번째 자기 자신이 아닌 오브젝트에 도달하면 반복 종료
                                }
                            }
                        }
                    }
                }

                // 후보가 하나라도 있다면 평균값을 계산 후 transform 보정
                if (candidateHitPoints.Count > 0 && candidateVertexWorldPositions.Count > 0)
                {
                    Vector3 avgHitPoint = Vector3.zero;
                    Vector3 avgHitNormal = Vector3.zero;
                    Vector3 avgVertexPos = Vector3.zero;
                    int count = candidateHitPoints.Count;
                    for (int i = 0; i < count; i++)
                    {
                        avgHitPoint += candidateHitPoints[i];
                        avgHitNormal += candidateHitNormals[i];
                    }
                    avgHitPoint /= count;
                    avgHitNormal.Normalize();

                    foreach (Vector3 v in candidateVertexWorldPositions)
                    {
                        avgVertexPos += v;
                    }
                    avgVertexPos /= candidateVertexWorldPositions.Count;

                    // 기존 회전의 수평 방향(yaw)은 유지하고 피치/롤만 보정
                    Vector3 originalForward = root.forward;
                    Vector3 projectedForward = Vector3.ProjectOnPlane(originalForward, avgHitNormal);
                    if (projectedForward.sqrMagnitude < 0.001f)
                        projectedForward = originalForward;
                    Quaternion newRotation = Quaternion.LookRotation(projectedForward, avgHitNormal);

                    // 기존 위치에서 (평균 hit point와 평균 후보 버텍스 위치 차이)만큼 보정
                    Vector3 newPosition = root.position + (avgHitPoint - avgVertexPos);

                    root.rotation = newRotation;
                    root.position = newPosition;
                }
            }
            e.Use();
        }
    }

    // 드래그 가능한 윈도우 내부 내용
    private static void DoWindow(int windowID)
    {
        GUILayout.Label("선택 오브젝트 정렬: U 키");
        isActive = GUILayout.Toggle(isActive, "Enable Terrain Aligner");
        GUI.DragWindow();
    }
}
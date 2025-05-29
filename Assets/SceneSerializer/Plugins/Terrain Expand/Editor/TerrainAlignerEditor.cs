using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class TerrainAlignerEditor
{
    // ��� Ȱ��ȭ ����
    private static bool isActive = false;
    // �巡�� ������ �г� ��ġ �� ũ��
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
        // U Ű�� ������ ��� ���� (���� ������Ʈ�� ����)
        if (e.type == EventType.KeyDown && e.keyCode == KeyCode.U)
        {
            Transform[] selectedTransforms = Selection.transforms;
            if (selectedTransforms.Length == 0)
                return;

            // ���� ������Ʈ�� ���� Undo ���
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
                    // �޽� ���� ��ǥ ���� �ּ� y �� ���
                    float minLocalY = float.MaxValue;
                    foreach (Vector3 v in vertices)
                    {
                        if (v.y < minLocalY)
                            minLocalY = v.y;
                    }

                    // tolerance �� ������ ���� �ּ� y�� ���� ���ؽ��� ����
                    float tolerance = 0.02f;
                    foreach (Vector3 v in vertices)
                    {
                        if (Mathf.Abs(v.y - minLocalY) < tolerance)
                        {
                            // ���� ��ǥ�� world ��ǥ�� ��ȯ
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
                                    break; // ù��° �ڱ� �ڽ��� �ƴ� ������Ʈ�� �����ϸ� �ݺ� ����
                                }
                            }
                        }
                    }
                }

                // �ĺ��� �ϳ��� �ִٸ� ��հ��� ��� �� transform ����
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

                    // ���� ȸ���� ���� ����(yaw)�� �����ϰ� ��ġ/�Ѹ� ����
                    Vector3 originalForward = root.forward;
                    Vector3 projectedForward = Vector3.ProjectOnPlane(originalForward, avgHitNormal);
                    if (projectedForward.sqrMagnitude < 0.001f)
                        projectedForward = originalForward;
                    Quaternion newRotation = Quaternion.LookRotation(projectedForward, avgHitNormal);

                    // ���� ��ġ���� (��� hit point�� ��� �ĺ� ���ؽ� ��ġ ����)��ŭ ����
                    Vector3 newPosition = root.position + (avgHitPoint - avgVertexPos);

                    root.rotation = newRotation;
                    root.position = newPosition;
                }
            }
            e.Use();
        }
    }

    // �巡�� ������ ������ ���� ����
    private static void DoWindow(int windowID)
    {
        GUILayout.Label("���� ������Ʈ ����: U Ű");
        isActive = GUILayout.Toggle(isActive, "Enable Terrain Aligner");
        GUI.DragWindow();
    }
}
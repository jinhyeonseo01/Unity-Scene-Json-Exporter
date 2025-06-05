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
        // �ֻ��� �����̳�
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

        // Foldout�� ���� ������/��ġ�⡱ ����� �߰�
        var foldout = new Foldout
        {
            text = "Snap to Floor Settings",
            value = TerrainAlignerEditor.IsActive // ������ ���� ���� �ʱⰪ
        };

        // Foldout�� �������� ���� ǥ���� ���� ���� �����̳�
        var contentContainer = new VisualElement
        {
            style =
            {
                flexDirection = FlexDirection.Column,
                marginLeft = 4,       // ��¦ �鿩����
                paddingBottom = 4
            }
        };

        // ���� Label
        var label = new Label("Press 'U' to Snap Selected")
        {
            style =
            {
                unityTextAlign = TextAnchor.MiddleLeft,
                marginBottom = 4
            }
        };
        contentContainer.Add(label);

        // ���� Toggle
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
            // �޴� üũ ���� ����
            EditorApplication.delayCall += () => Menu.SetChecked("Tools/Snap to Floor", TerrainAlignerEditor.IsActive);
            // ���� ������Ʈ
            SceneView.RepaintAll();
        });
        contentContainer.Add(toggle);

        // Foldout �ڽ����� ���� ����Ʈ �߰�
        foldout.Add(contentContainer);

        // Foldout�� ��ħ/���� ���°� �ٲ� ���� toggle ����ȭ
        foldout.RegisterValueChangedCallback(evt =>
        {
            // Foldout ��ħ ���°� ��(folded)�� ���� ��� ��Ȱ��ȭ
            if (!evt.newValue)
                TerrainAlignerEditor.IsActive = false;
            // ��ħ ���°� ������ ���� ���� IsActive ���� ����(�̹� toggle�� ó��)
            // �޴� üũ ���� ���� �� ���� ������Ʈ
            EditorApplication.delayCall += () => Menu.SetChecked("Tools/Snap to Floor", TerrainAlignerEditor.IsActive);
            SceneView.RepaintAll();
        });

        // �ֻ����� Foldout �߰�
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
        // Ȥ�� �̹� ������ �� ������ ���� �� �ߺ� ���� ����
        SceneView.duringSceneGui -= OnSceneGUI;
        SceneView.duringSceneGui += OnSceneGUI;
    }

    private static void OnSceneGUI(SceneView sceneView)
    {
        // �������̰� ���� ���� ������ �ƹ� �͵� �� ��
        if (!IsActive)
            return;

        // �� �信 ��Ŀ���� ������ Ű �Է� ����
        if (SceneView.lastActiveSceneView != sceneView || !sceneView.hasFocus)
            return;

        Event e = Event.current;
        // U Ű(Shift/Alt/Ctrl ����)�� ������ ���� �۵�
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

        // Undo ���ڵ�
        Undo.RecordObjects(selectedTransforms, "Align Objects to Terrain via Vertex");

        // �̸� RaycastHit �迭�� ����(NonAlloc ����)
        RaycastHit[] hitBuffer = new RaycastHit[8];

        // 1. ���õ� ��ü�� �� �ڽĵ� ��θ� HashSet�� ���, Raycast ������� ������ ��� �Ǵ�
        var excludedRoots = new HashSet<Transform>();
        foreach (var root in selectedTransforms)
        {
            foreach (var t in root.GetComponentsInChildren<Transform>(includeInactive: true))
            {
                excludedRoots.Add(t);
            }
        }

        // 2. ��� ���õ� ������Ʈ�� �ٴ� ���� ����Ʈ�� ����
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

                // �޽� �ٿ�� �ڽ��� �Ʒ��� 4�� ������ ���ø�
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
                        // �Ÿ� �������� �迭 ����
                        Array.Sort(hitBuffer, 0, hitCount, Comparer<RaycastHit>.Create(
                            (a, b) => a.distance.CompareTo(b.distance)
                        ));

                        // ���� ����� valid hit�� ã�� �߰� (�ڽ��� �ݶ��̴��� ����)
                        for (int i = 0; i < hitCount; i++)
                        {
                            var hit = hitBuffer[i];
                            if (hit.collider == null)
                                continue;

                            // �� hit�� ���õ� ��ü �Ǵ� �� �ڽ��� �ݶ��̴���� �ǳʶ�
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

        // 3. ��ü �ٴ� ������ ��� ��ġ ���
        Vector3 avgBottom = Vector3.zero;
        foreach (var bp in allBottomPoints)
            avgBottom += bp;
        avgBottom /= allBottomPoints.Count;

        // 4. ��ü ��Ʈ ��ġ�� ��հ�, �ֺ� ��� ���
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

        // 5. ���õ� �� ������Ʈ�� ������ ��ġ��ȸ�� ���� ����
        foreach (Transform root in selectedTransforms)
        {
            // ���� ȸ���� ���� ���� �����ϸ�, ����� ������ ȸ�� ���
            Vector3 forwardDir = Vector3.Cross(root.right, chosenNormal);
            if (forwardDir.sqrMagnitude < 0.001f)
                forwardDir = root.forward; // fallback
            Quaternion newRotation = Quaternion.LookRotation(forwardDir, chosenNormal);

            // ��ġ ����: ��� �ٴ� ���ؽ� ��ġ�� ��� ��Ʈ ��ġ ���̸�ŭ �̵�
            Vector3 newPosition = root.position + (avgHitPoint - avgBottom);

            root.rotation = newRotation;
            root.position = newPosition;
        }
    }
}
#endif
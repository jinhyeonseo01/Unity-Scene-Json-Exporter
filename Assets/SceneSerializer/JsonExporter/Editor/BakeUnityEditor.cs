#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Overlays;
using UnityEngine;

/// <summary>
/// SceneView ��� ��Overlays�� �޴��� ��Json Baking Tool���̶�� üũ�ڽ��� �߰��ϰ�,
/// üũ�صθ� �� �� �ȿ� ������ IMGUI �� â�� �����ݴϴ�.
/// </summary>
[Overlay(typeof(SceneView), "Json Baking Tool", defaultDisplay = true)]
public class BakeUnityIMGUIOverlay : IMGUIOverlay
{
    // ��â�� �ʺ�/���� (���ϴ� ũ��� ���� ����)
    private const float kWindowWidth = 270f;
    private const float kWindowHeight = 220f;

    // ���� �����̴��� ��ũ���� �ʿ������ ���� ���� ���� ���ʿ�
    // (���⼭�� ��ũ�� ���� ���� ���̾ƿ����� ����)

    public override void OnGUI()
    {
        // 1) â�� �׸� ������ �ڽ� (�𼭸�/��� �� ��Ÿ���� box�� ��Ƶ�)
        //    GUILayout.BeginArea�� ���� ���, �ܼ��� GUILayout.Window�� ����ϰ� BeginVertical�� ó��
        GUILayout.BeginVertical("box", GUILayout.Width(kWindowWidth), GUILayout.Height(kWindowHeight));
        {
            GUILayout.Space(5);

            // 2) ���� ��
            GUILayout.Label(
                "���� # : �ش� ��ü ����\n" +
                "���� ## : �ش� ��ü�� ������ ���� ��ü ����\n" +
                "static ����(���� ���) : static���\n" +
                "layer ����(���� ���) : Deactivate���",
                EditorStyles.wordWrappedLabel
            );
            GUILayout.Space(10);

            // 3) ��ư: Scene Baking
            if (GUILayout.Button("Scene Baking", GUILayout.Height(25)))
            {
                BakeUnity.SceneBake();
            }
            GUILayout.Space(5);

            // 4) ��ư: Selected Baking
            if (GUILayout.Button("Selected Baking", GUILayout.Height(25)))
            {
                BakeUnity.SelectBake();
            }
            GUILayout.Space(10);

            // 5) ������ ���� ��ư
            GUILayout.BeginHorizontal();
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Resource Copy", GUILayout.Width(110), GUILayout.Height(25)))
                {
                    BakeUnity.CopyResources();
                }
                GUILayout.Space(5);
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(5);
            GUILayout.Label("(���� : ������ Baking Path�� ����)", EditorStyles.miniLabel);

            GUILayout.FlexibleSpace();
        }
        GUILayout.EndVertical();
    }

    // (����) ���� Overlay â�� ũ��/��ġ�� ������ �����ϰ� ������,
    // IMGUIOverlay���� override ������ �Ӽ��� �Ʒ�ó�� �ֽ��ϴ�.
    //
    // public override Vector2 GetOverlaySize() => new Vector2(kWindowWidth, kWindowHeight);
    //
    // �⺻������ Overlay API�� ���ּ� ũ�⡱�� �� ������ ����ָ�,
    // ������ �ѱ⡱ �� ������ ��ġ/���̾ƿ��� �����մϴ�.
}
#endif

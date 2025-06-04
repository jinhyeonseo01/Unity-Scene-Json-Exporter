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
<<<<<<< Updated upstream
    static BakeUnityEditor()
=======
    // ��â�� �ʺ�/���� (���ϴ� ũ��� ���� ����)
    private const float kWindowWidth = 270f;
    private const float kWindowHeight = 220f;

    // ���� �����̴��� ��ũ���� �ʿ������ ���� ���� ���� ���ʿ�
    // (���⼭�� ��ũ�� ���� ���� ���̾ƿ����� ����)

    public override void OnGUI()
>>>>>>> Stashed changes
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
<<<<<<< Updated upstream
    ~BakeUnityEditor()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
    }
    // ���� ������: ����Ƽ�� ��ũ��Ʈ�� �ε��� �� ȣ��˴ϴ�.

    public void OnGUI()
    {
        
        if (Event.current.type == EventType.KeyDown)
        {
            //FocusWindowIfItsOpen<SceneView>();
        }
    }

    public static void OnSceneGUI(SceneView sceneView)
    {
        Handles.BeginGUI();

        // GUI ����� ��ġ�� ũ�⸦ �����մϴ�.
        Vector2 offsetPos = new Vector2(sceneView.position.width, sceneView.position.height*2/3);
        Vector2 offsetSize = new Vector2(250, 220);
        
        offsetPos = new Vector2(Mathf.Min(sceneView.position.width - offsetSize.x, offsetPos.x), Mathf.Min(sceneView.position.height - offsetSize.y-30, offsetPos.y));
        Rect buttonRect = new Rect(offsetPos.x, offsetPos.y, offsetSize.x, offsetSize.y);

        Rect paddingRect = new Rect(10,10,10,10);

        float totalH = 30;
        float currentH = 0;
        GUI.Box(buttonRect, "Json Baking Tool");


        currentH = 150;
        var T1Pos = offsetPos + new Vector2(paddingRect.x, 0) + new Vector2(0, totalH + 5);
        var T1Size = new Vector2(offsetSize.x - (paddingRect.x + paddingRect.width), currentH / 2 - 5);
        GUI.Label(new Rect(T1Pos, T1Size),
            "���� # : �ش� ��ü ����\n���� ## : �ش� ��ü�� ������ ���� ��ü ����\nstatic ����(���� ���) : static���\nlayer ����(���� ���) : Deactivate���");
        totalH += currentH / 2;


        currentH = 70;
        var ButtonPos = offsetPos + new Vector2(paddingRect.x, 0) + new Vector2(0, totalH + 5);
        var ButtonSize = new Vector2(offsetSize.x -( paddingRect.x + paddingRect.width), currentH / 2 - 5);
        // ��ư ���� �� Ŭ�� �̺�Ʈ ó��
        if (GUI.Button(new Rect(ButtonPos, ButtonSize), "Baking"))
        {
            BakeUnity.Baking();
        }
        totalH += currentH / 2;


        currentH = 70;
        var ButtonPos2 = offsetPos + new Vector2(paddingRect.x, 0) + new Vector2(0, totalH + 5);
        var ButtonSize2 = new Vector2(offsetSize.x - (paddingRect.x + paddingRect.width), currentH / 2 - 5);
        // ��ư ���� �� Ŭ�� �̺�Ʈ ó��
        if (GUI.Button(new Rect(ButtonPos2, ButtonSize2), "Selected Baking"))
        {
            BakeUnity.SelectBaking();
        }
        totalH += currentH / 2;

        currentH = 70;
        ButtonPos2 = offsetPos + new Vector2(paddingRect.x, 0) + new Vector2(0, totalH + 5);
        ButtonSize2 = new Vector2(offsetSize.x - (paddingRect.x + paddingRect.width), currentH / 2 - 5);
        if (GUI.Button(new Rect(ButtonPos2, ButtonSize2), "Resource Copy"))
        {
            BakeUnity.CopyResources();
        }
        totalH += currentH / 2;

        // Scene �信�� GUI ��� �׸��⸦ �����մϴ�.
        Handles.EndGUI();
    }
}
=======

    // (����) ���� Overlay â�� ũ��/��ġ�� ������ �����ϰ� ������,
    // IMGUIOverlay���� override ������ �Ӽ��� �Ʒ�ó�� �ֽ��ϴ�.
    //
    // public override Vector2 GetOverlaySize() => new Vector2(kWindowWidth, kWindowHeight);
    //
    // �⺻������ Overlay API�� ���ּ� ũ�⡱�� �� ������ ����ָ�,
    // ������ �ѱ⡱ �� ������ ��ġ/���̾ƿ��� �����մϴ�.
}
#endif
>>>>>>> Stashed changes

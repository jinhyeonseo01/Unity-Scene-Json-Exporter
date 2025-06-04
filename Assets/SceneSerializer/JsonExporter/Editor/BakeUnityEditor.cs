#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Overlays;
using UnityEngine;

/// <summary>
/// SceneView 상단 “Overlays” 메뉴에 “Json Baking Tool”이라는 체크박스를 추가하고,
/// 체크해두면 씬 뷰 안에 고정된 IMGUI 툴 창을 보여줍니다.
/// </summary>
[Overlay(typeof(SceneView), "Json Baking Tool", defaultDisplay = true)]
public class BakeUnityIMGUIOverlay : IMGUIOverlay
{
<<<<<<< Updated upstream
    static BakeUnityEditor()
=======
    // 툴창의 너비/높이 (원하는 크기로 조정 가능)
    private const float kWindowWidth = 270f;
    private const float kWindowHeight = 220f;

    // 내부 슬라이더나 스크롤이 필요없으면 별도 상태 관리 불필요
    // (여기서는 스크롤 없이 고정 레이아웃으로 구현)

    public override void OnGUI()
>>>>>>> Stashed changes
    {
        // 1) 창을 그릴 고정된 박스 (모서리/배경 등 스타일을 box로 잡아둠)
        //    GUILayout.BeginArea를 쓰는 대신, 단순히 GUILayout.Window와 비슷하게 BeginVertical로 처리
        GUILayout.BeginVertical("box", GUILayout.Width(kWindowWidth), GUILayout.Height(kWindowHeight));
        {
            GUILayout.Space(5);

            // 2) 설명 라벨
            GUILayout.Label(
                "접두 # : 해당 객체 제외\n" +
                "접두 ## : 해당 객체를 포함한 계층 전체 제외\n" +
                "static 설정(우측 상단) : static모드\n" +
                "layer 설정(우측 상단) : Deactivate모드",
                EditorStyles.wordWrappedLabel
            );
            GUILayout.Space(10);

            // 3) 버튼: Scene Baking
            if (GUILayout.Button("Scene Baking", GUILayout.Height(25)))
            {
                BakeUnity.SceneBake();
            }
            GUILayout.Space(5);

            // 4) 버튼: Selected Baking
            if (GUILayout.Button("Selected Baking", GUILayout.Height(25)))
            {
                BakeUnity.SelectBake();
            }
            GUILayout.Space(10);

            // 5) 마지막 복사 버튼
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
            GUILayout.Label("(주의 : 마지막 Baking Path로 복사)", EditorStyles.miniLabel);

            GUILayout.FlexibleSpace();
        }
        GUILayout.EndVertical();
    }
<<<<<<< Updated upstream
    ~BakeUnityEditor()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
    }
    // 정적 생성자: 유니티가 스크립트를 로드할 때 호출됩니다.

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

        // GUI 요소의 위치와 크기를 정의합니다.
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
            "접두 # : 해당 객체 제외\n접두 ## : 해당 객체를 포함한 계층 전체 제외\nstatic 설정(우측 상단) : static모드\nlayer 설정(우측 상단) : Deactivate모드");
        totalH += currentH / 2;


        currentH = 70;
        var ButtonPos = offsetPos + new Vector2(paddingRect.x, 0) + new Vector2(0, totalH + 5);
        var ButtonSize = new Vector2(offsetSize.x -( paddingRect.x + paddingRect.width), currentH / 2 - 5);
        // 버튼 생성 및 클릭 이벤트 처리
        if (GUI.Button(new Rect(ButtonPos, ButtonSize), "Baking"))
        {
            BakeUnity.Baking();
        }
        totalH += currentH / 2;


        currentH = 70;
        var ButtonPos2 = offsetPos + new Vector2(paddingRect.x, 0) + new Vector2(0, totalH + 5);
        var ButtonSize2 = new Vector2(offsetSize.x - (paddingRect.x + paddingRect.width), currentH / 2 - 5);
        // 버튼 생성 및 클릭 이벤트 처리
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

        // Scene 뷰에서 GUI 요소 그리기를 종료합니다.
        Handles.EndGUI();
    }
}
=======

    // (선택) 만약 Overlay 창의 크기/위치를 강제로 설정하고 싶으면,
    // IMGUIOverlay에는 override 가능한 속성이 아래처럼 있습니다.
    //
    // public override Vector2 GetOverlaySize() => new Vector2(kWindowWidth, kWindowHeight);
    //
    // 기본적으로 Overlay API가 “최소 크기”를 이 값으로 잡아주며,
    // “끄고 켜기” 시 동일한 위치/레이아웃을 유지합니다.
}
#endif
>>>>>>> Stashed changes

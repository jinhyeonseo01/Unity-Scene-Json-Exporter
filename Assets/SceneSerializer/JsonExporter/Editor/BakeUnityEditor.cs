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
    // 툴창의 너비/높이 (원하는 크기로 조정 가능)
    private const float kWindowWidth = 270f;
    private const float kWindowHeight = 220f;

    // 내부 슬라이더나 스크롤이 필요없으면 별도 상태 관리 불필요
    // (여기서는 스크롤 없이 고정 레이아웃으로 구현)

    public override void OnGUI()
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

    // (선택) 만약 Overlay 창의 크기/위치를 강제로 설정하고 싶으면,
    // IMGUIOverlay에는 override 가능한 속성이 아래처럼 있습니다.
    //
    // public override Vector2 GetOverlaySize() => new Vector2(kWindowWidth, kWindowHeight);
    //
    // 기본적으로 Overlay API가 “최소 크기”를 이 값으로 잡아주며,
    // “끄고 켜기” 시 동일한 위치/레이아웃을 유지합니다.
}
#endif

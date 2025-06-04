using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;

[InitializeOnLoad]
public class BakeUnityEditor : Editor
{
    // 드래그 가능한 윈도우의 초기 위치와 크기
    private static Rect windowRect = new Rect(50, 80, 270, 220);

    static BakeUnityEditor()
    {
        SceneView.duringSceneGui += OnSceneGUI;
    }

    ~BakeUnityEditor()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
    }

    public static void OnSceneGUI(SceneView sceneView)
    {
        Handles.BeginGUI();
        // 윈도우 ID는 어떤 값이어도 상관없습니다.
        windowRect = GUI.Window(
            /* id */         123456,
            /* position */   windowRect,
            /* func */       DrawWindowContents,
            /* title */      "Json Baking Tool"
        );
        Handles.EndGUI();
    }

    private static void DrawWindowContents(int windowID)
    {
        // 내부 패딩 값
        var padding = 10f;
        var contentWidth = windowRect.width - padding * 2;

        // 첫 번째 라벨: 접두어 설명
        Rect labelRect = new Rect(padding, 20, contentWidth, 75);
        GUI.Label(labelRect,
            "접두 # : 해당 객체 제외\n" +
            "접두 ## : 해당 객체를 포함한 계층 전체 제외\n" +
            "static 설정(우측 상단) : static모드\n" +
            "layer 설정(우측 상단) : Deactivate모드"
        );

        float y = labelRect.yMax + 10;

        // Scene Baking 버튼
        Rect btnRect1 = new Rect(padding, y, contentWidth, 25);
        if (GUI.Button(btnRect1, "Scene Baking"))
            BakeUnity.SceneBake();
        y += 30;

        // Selected Baking 버튼
        Rect btnRect2 = new Rect(padding, y, contentWidth, 25);
        if (GUI.Button(btnRect2, "Selected Baking"))
            BakeUnity.SelectBake();

        y += 35;

        // Resource Copy 버튼
        Rect btnRect3 = new Rect(padding, y, contentWidth, 25);
        if (GUI.Button(btnRect3, "Resource Copy"))
            BakeUnity.CopyResources();

        y += 25;
        GUI.Label(new Rect(padding, y, contentWidth, 20), "(주의 : 마지막 Baking Path로 복사)");

        // 윈도우 전체 영역을 드래그 가능하게 만듭니다.
        GUI.DragWindow(new Rect(0, 0, windowRect.width, windowRect.height));
    }
}
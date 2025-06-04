using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;

[InitializeOnLoad]
public class BakeUnityEditor : Editor
{
    // �巡�� ������ �������� �ʱ� ��ġ�� ũ��
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
        // ������ ID�� � ���̾ ��������ϴ�.
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
        // ���� �е� ��
        var padding = 10f;
        var contentWidth = windowRect.width - padding * 2;

        // ù ��° ��: ���ξ� ����
        Rect labelRect = new Rect(padding, 20, contentWidth, 75);
        GUI.Label(labelRect,
            "���� # : �ش� ��ü ����\n" +
            "���� ## : �ش� ��ü�� ������ ���� ��ü ����\n" +
            "static ����(���� ���) : static���\n" +
            "layer ����(���� ���) : Deactivate���"
        );

        float y = labelRect.yMax + 10;

        // Scene Baking ��ư
        Rect btnRect1 = new Rect(padding, y, contentWidth, 25);
        if (GUI.Button(btnRect1, "Scene Baking"))
            BakeUnity.SceneBake();
        y += 30;

        // Selected Baking ��ư
        Rect btnRect2 = new Rect(padding, y, contentWidth, 25);
        if (GUI.Button(btnRect2, "Selected Baking"))
            BakeUnity.SelectBake();

        y += 35;

        // Resource Copy ��ư
        Rect btnRect3 = new Rect(padding, y, contentWidth, 25);
        if (GUI.Button(btnRect3, "Resource Copy"))
            BakeUnity.CopyResources();

        y += 25;
        GUI.Label(new Rect(padding, y, contentWidth, 20), "(���� : ������ Baking Path�� ����)");

        // ������ ��ü ������ �巡�� �����ϰ� ����ϴ�.
        GUI.DragWindow(new Rect(0, 0, windowRect.width, windowRect.height));
    }
}
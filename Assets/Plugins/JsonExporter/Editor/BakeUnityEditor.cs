using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;

[InitializeOnLoad]
public class BakeUnityEditor : Editor
{
    static BakeUnityEditor()
    {
        SceneView.duringSceneGui += OnSceneGUI;
    }
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
        Vector2 offsetSize = new Vector2(200, 130);
        
        offsetPos = new Vector2(Mathf.Min(sceneView.position.width - offsetSize.x, offsetPos.x), Mathf.Min(sceneView.position.height - offsetSize.y-30, offsetPos.y));
        Rect buttonRect = new Rect(offsetPos.x, offsetPos.y, offsetSize.x, offsetSize.y);

        Rect paddingRect = new Rect(10,10,10,10);

        GUI.Box(buttonRect, "Json Baking Tool");

        var ButtonPos = offsetPos + new Vector2(paddingRect.x, 0) + new Vector2(0, 30);
        var ButtonSize = new Vector2(offsetSize.x -( paddingRect.x + paddingRect.width), 40);
        // ��ư ���� �� Ŭ�� �̺�Ʈ ó��
        if (GUI.Button(new Rect(ButtonPos, ButtonSize), "Baking"))
        {
            BakeUnity.Baking();
        }

        var ButtonPos2 = offsetPos + new Vector2(paddingRect.x, 0) + new Vector2(0, 75);
        var ButtonSize2 = new Vector2(offsetSize.x - (paddingRect.x + paddingRect.width), 40);
        // ��ư ���� �� Ŭ�� �̺�Ʈ ó��
        if (GUI.Button(new Rect(ButtonPos2, ButtonSize2), "Selected Baking"))
        {
            BakeUnity.SelectBaking();
        }

        // Scene �信�� GUI ��� �׸��⸦ �����մϴ�.
        Handles.EndGUI();
    }
}

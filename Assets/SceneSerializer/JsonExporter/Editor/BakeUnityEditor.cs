#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Overlays;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Displays a “Json Baking Tool” panel under Scene View’s Overlays, built entirely with UI Toolkit.
/// When collapsed, the overlay will shrink to just the foldout header height.
/// </summary>
[Overlay(typeof(SceneView), "Json Export Tool", defaultDisplay = true)]
public class BakeUnityOverlay : Overlay
{
    private const string k_FoldoutKey = "BakeUnityOverlay_Foldout";

    public override VisualElement CreatePanelContent()
    {
        // Load foldout state (default to true)
        bool isExpanded = EditorPrefs.GetBool(k_FoldoutKey, true);

        // Root container: do NOT give it a fixed minHeight! Just a minWidth for readability.
        var root = new VisualElement
        {
            style =
            {
                minWidth = 200,
                flexDirection = FlexDirection.Column,
                paddingTop = 4,
                paddingBottom = 4,
                paddingLeft = 6,
                paddingRight = 6,
                backgroundColor = new Color(0.18f, 0.18f, 0.18f, 0.9f),
                borderTopWidth = 1,
                borderBottomWidth = 1,
                borderLeftWidth = 1,
                borderRightWidth = 1,
                borderTopColor = Color.gray,
                borderBottomColor = Color.gray,
                borderLeftColor = Color.gray,
                borderRightColor = Color.gray
            }
        };

        // 1) Foldout header
        var foldout = new Foldout
        {
            text = "Json Export Tool",
            value = isExpanded,
            style =
            {
                unityFontStyleAndWeight = FontStyle.Bold,
                fontSize = 14,
                marginBottom = 4
            }
        };
        root.Add(foldout);

        // 2) Container for all “body” controls – this is hidden when collapsed
        var content = new VisualElement
        {
            style =
            {
                flexDirection = FlexDirection.Column,
                marginLeft = 4,
                marginBottom = 4,
                display = isExpanded ? DisplayStyle.Flex : DisplayStyle.None
            }
        };
        root.Add(content);

        // 2a) Instruction label
        var instruction = new Label(
            "Prefix # : exclude this GameObject\n" +
            "Prefix ## : exclude this GameObject\n\t\tand entire hierarchy\n"
        )
        {
            style =
            {
                unityTextAlign = TextAnchor.MiddleLeft,
                marginBottom = 8,
                whiteSpace = WhiteSpace.Normal
            }
        };
        content.Add(instruction);

        // 2b) “Scene Baking” button
        var sceneBakeBtn = new Button(() => BakeUnity.SceneExport())
        {
            text = "Scene Export",
            style =
            {
                height = 25,
                marginBottom = 4
            }
        };
        content.Add(sceneBakeBtn);

        // 2c) “Selected Baking” button
        var selectedBakeBtn = new Button(() => BakeUnity.SelectExport())
        {
            text = "Selected Export",
            style =
            {
                height = 25,
                marginBottom = 8
            }
        };
        content.Add(selectedBakeBtn);

        // 2d) “Resource Copy” button aligned to the right
        var copyContainer = new VisualElement
        {
            style =
            {
                flexDirection = FlexDirection.Row,
                justifyContent = Justify.FlexEnd,
                marginBottom = 4
            }
        };
        var copyBtn = new Button(() => BakeUnity.CopyResources())
        {
            text = "Resource Copy",
            style =
            {
                width = 110,
                height = 25
            }
        };
        copyContainer.Add(copyBtn);
        content.Add(copyContainer);

        // 2e) Warning label
        var warning = new Label("(Warning: copies to last exported resources)")
        {
            style =
            {
                unityFontStyleAndWeight = FontStyle.Italic,
                fontSize = 10,
                color = new Color(0.8f, 0.8f, 0.8f, 1f),
                marginLeft = 4
            }
        };
        content.Add(warning);

        // 3) Register the foldout callback: show/hide “content” and save state to EditorPrefs
        foldout.RegisterValueChangedCallback(evt =>
        {
            bool expanded = evt.newValue;
            EditorPrefs.SetBool(k_FoldoutKey, expanded);
            content.style.display = expanded ? DisplayStyle.Flex : DisplayStyle.None;
        });

        return root;
    }
}
#endif
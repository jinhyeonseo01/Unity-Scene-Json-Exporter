using UnityEngine;
using UnityEditor;

namespace Clrain.SceneToJson.Expansions
{
    public class RaiseTerrainEditor : EditorWindow
    {
        // ���� ������ (�⺻�� 0.01)
        private float raiseAmount = 0.01f;

        [MenuItem("Tools/Json Exporter/Terrain Expansions/Raise Terrain")]
        public static void ShowWindow()
        {
            GetWindow<RaiseTerrainEditor>("Raise Terrain");
        }

        private void OnGUI()
        {
            GUILayout.Label("Terrain ���� �ø���", EditorStyles.boldLabel);
            raiseAmount = EditorGUILayout.FloatField("���� ������", raiseAmount);

            if (GUILayout.Button("���õ� Terrain ���� �ø���"))
            {
                RaiseSelectedTerrain();
            }
        }

        private void RaiseSelectedTerrain()
        {
            // ���� ���õ� ������Ʈ���� Terrain ������Ʈ�� ã��
            Terrain terrain = Selection.activeGameObject?.GetComponent<Terrain>();
            if (terrain == null)
            {
                Debug.LogWarning("���õ� ������Ʈ�� Terrain ������Ʈ�� �����ϴ�!");
                return;
            }

            TerrainData terrainData = terrain.terrainData;
            int width = terrainData.heightmapResolution;
            int height = terrainData.heightmapResolution;
            float[,] heights = terrainData.GetHeights(0, 0, width, height);

            // ��� ���̿� raiseAmount ��ŭ ���� ���� (0~1 ���̷� clamping)
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    heights[x, y] = Mathf.Clamp01(heights[x, y] + raiseAmount);
                }
            }

            terrainData.SetHeights(0, 0, heights);
            Debug.Log("Terrain ���� ���� �Ϸ�!");
        }
    }
}
using UnityEngine;
using UnityEditor;

namespace Clrain.SceneToJson.Expansions
{
    public class RaiseTerrainEditor : EditorWindow
    {
        // 높이 증가값 (기본값 0.01)
        private float raiseAmount = 0.01f;

        [MenuItem("Tools/Json Exporter/Terrain Expansions/Raise Terrain")]
        public static void ShowWindow()
        {
            GetWindow<RaiseTerrainEditor>("Raise Terrain");
        }

        private void OnGUI()
        {
            GUILayout.Label("Terrain 높이 올리기", EditorStyles.boldLabel);
            raiseAmount = EditorGUILayout.FloatField("높이 증가값", raiseAmount);

            if (GUILayout.Button("선택된 Terrain 높이 올리기"))
            {
                RaiseSelectedTerrain();
            }
        }

        private void RaiseSelectedTerrain()
        {
            // 현재 선택된 오브젝트에서 Terrain 컴포넌트를 찾음
            Terrain terrain = Selection.activeGameObject?.GetComponent<Terrain>();
            if (terrain == null)
            {
                Debug.LogWarning("선택된 오브젝트에 Terrain 컴포넌트가 없습니다!");
                return;
            }

            TerrainData terrainData = terrain.terrainData;
            int width = terrainData.heightmapResolution;
            int height = terrainData.heightmapResolution;
            float[,] heights = terrainData.GetHeights(0, 0, width, height);

            // 모든 높이에 raiseAmount 만큼 값을 더함 (0~1 사이로 clamping)
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    heights[x, y] = Mathf.Clamp01(heights[x, y] + raiseAmount);
                }
            }

            terrainData.SetHeights(0, 0, heights);
            Debug.Log("Terrain 높이 수정 완료!");
        }
    }
}
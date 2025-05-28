using UnityEngine;

public class PlayAnimation : MonoBehaviour
{
    public Terrain terrain;        // 수정할 Terrain. Inspector에 할당하거나, Terrain.activeTerrain로 자동 선택 가능.
    [Range(0f, 1f)]
    public float heightIncrease = 0.1f; // 높이 증가량 (0~1 사이의 값, 전체 높이의 비율)

    void Start()
    {
        // terrain이 지정되지 않았다면, 씬의 활성 Terrain 사용
        if (terrain == null)
            terrain = Terrain.activeTerrain;

        TerrainData tData = terrain.terrainData;
        int hmWidth = tData.heightmapResolution;
        int hmHeight = tData.heightmapResolution;

        // 기존 높이 맵 데이터 읽어오기
        float[,] heights = tData.GetHeights(0, 0, hmWidth, hmHeight);

        // 모든 높이에 heightIncrease만큼 추가 (값은 0~1 범위이므로 전체 높이의 비율임)
        for (int y = 0; y < hmHeight; y++)
        {
            for (int x = 0; x < hmWidth; x++)
            {
                heights[y, x] = Mathf.Clamp01(heights[y, x] + heightIncrease);
            }
        }

        // 수정된 높이 맵 데이터 적용
        tData.SetHeights(0, 0, heights);
    }
}

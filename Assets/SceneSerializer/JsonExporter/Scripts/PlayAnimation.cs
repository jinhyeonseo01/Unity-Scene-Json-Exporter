using UnityEngine;

public class PlayAnimation : MonoBehaviour
{
    public Terrain terrain;        // ������ Terrain. Inspector�� �Ҵ��ϰų�, Terrain.activeTerrain�� �ڵ� ���� ����.
    [Range(0f, 1f)]
    public float heightIncrease = 0.1f; // ���� ������ (0~1 ������ ��, ��ü ������ ����)

    void Start()
    {
        // terrain�� �������� �ʾҴٸ�, ���� Ȱ�� Terrain ���
        if (terrain == null)
            terrain = Terrain.activeTerrain;

        TerrainData tData = terrain.terrainData;
        int hmWidth = tData.heightmapResolution;
        int hmHeight = tData.heightmapResolution;

        // ���� ���� �� ������ �о����
        float[,] heights = tData.GetHeights(0, 0, hmWidth, hmHeight);

        // ��� ���̿� heightIncrease��ŭ �߰� (���� 0~1 �����̹Ƿ� ��ü ������ ������)
        for (int y = 0; y < hmHeight; y++)
        {
            for (int x = 0; x < hmWidth; x++)
            {
                heights[y, x] = Mathf.Clamp01(heights[y, x] + heightIncrease);
            }
        }

        // ������ ���� �� ������ ����
        tData.SetHeights(0, 0, heights);
    }
}

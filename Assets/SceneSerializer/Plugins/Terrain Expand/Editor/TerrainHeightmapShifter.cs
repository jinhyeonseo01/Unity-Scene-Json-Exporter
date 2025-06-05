using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class TerrainHeightmapShifterWithGlobalIndex : EditorWindow
{
    // �ε��� ���� ������ �� (������ ����)
    int offsetX = 0;
    int offsetY = 0;

    [MenuItem("Tools/Json Exporter/Terrain Expansions/Shift Terrain Heightmap With Neighbors (Global Index)")]
    public static void ShowWindow()
    {
        GetWindow<TerrainHeightmapShifterWithGlobalIndex>("Shift Terrain Heightmap (Global Index)");
    }

    // �ͷ��� ���� ������ �����ϴ� Ŭ����
    private class TerrainDataInfo
    {
        public Terrain terrain;
        public float[,] heights;
        public int resolution;
        public Vector3 position;
        public Vector3 size;
        public float sampleSpacingX;
        public float sampleSpacingZ;

        public TerrainDataInfo(Terrain terrain)
        {
            this.terrain = terrain;
            TerrainData data = terrain.terrainData;
            resolution = data.heightmapResolution;
            heights = data.GetHeights(0, 0, resolution, resolution);
            position = terrain.transform.position;
            size = data.size;
            sampleSpacingX = size.x / (resolution - 1);
            sampleSpacingZ = size.z / (resolution - 1);
        }
    }

    private void OnGUI()
    {
        GUILayout.Label("Shift Terrain Heightmap with Neighbor Data (Global Index)", EditorStyles.boldLabel);
        offsetX = EditorGUILayout.IntField("Offset X (columns)", offsetX);
        offsetY = EditorGUILayout.IntField("Offset Y (rows)", offsetY);

        if (GUILayout.Button("Shift Selected Terrains"))
        {
            ShiftSelectedTerrains();
        }
    }

    private void ShiftSelectedTerrains()
    {
        // ���õ� �ͷ��ε� ��������
        Terrain[] terrains = Selection.GetFiltered<Terrain>(SelectionMode.DeepAssets);
        if (terrains.Length == 0)
        {
            Debug.LogWarning("No terrains selected!");
            return;
        }

        // ��� �ͷ����� �ּ� x, z�� �۷ι� �������� ���� (�ͷ��ε��� ��ġ�� grid ���¶�� ����)
        float globalOriginX = float.MaxValue;
        float globalOriginZ = float.MaxValue;
        foreach (Terrain t in terrains)
        {
            if (t.transform.position.x < globalOriginX)
                globalOriginX = t.transform.position.x;
            if (t.transform.position.z < globalOriginZ)
                globalOriginZ = t.transform.position.z;
        }

        // ��� �ͷ����� ������ ũ��� ���� (ù ��° �ͷ����� size ���)
        TerrainDataInfo sampleInfo = new TerrainDataInfo(terrains[0]);
        Vector3 terrainSize = sampleInfo.size;
        int resolution = sampleInfo.resolution; // �� �ͷ����� �ػ�

        // �ͷ��ε��� ��ġ�� �۷ι� �׸��� ��ǥ�� ������ ����
        // grid ��ǥ: (gridX, gridZ) = (Floor((terrain.position.x - globalOriginX) / terrainSize.x),
        //                              Floor((terrain.position.z - globalOriginZ) / terrainSize.z))
        Dictionary<Vector2Int, TerrainDataInfo> terrainGrid = new Dictionary<Vector2Int, TerrainDataInfo>();
        Dictionary<Terrain, Vector2Int> terrainGridPosDict = new Dictionary<Terrain, Vector2Int>();
        foreach (Terrain t in terrains)
        {
            TerrainDataInfo info = new TerrainDataInfo(t);
            int gridX = Mathf.FloorToInt((t.transform.position.x - globalOriginX) / terrainSize.x);
            int gridZ = Mathf.FloorToInt((t.transform.position.z - globalOriginZ) / terrainSize.z);
            Vector2Int gridPos = new Vector2Int(gridX, gridZ);
            terrainGridPosDict[t] = gridPos;
            if (!terrainGrid.ContainsKey(gridPos))
            {
                terrainGrid.Add(gridPos, info);
            }
            else
            {
                Debug.LogWarning("Duplicate terrain grid position at " + gridPos);
            }
        }

        // �� �ͷ��κ��� ���ο� ����Ʈ�� ���
        foreach (Terrain t in terrains)
        {
            TerrainData data = t.terrainData;
            TerrainDataInfo currentInfo = new TerrainDataInfo(t);
            Vector2Int currentGridPos = terrainGridPosDict[t];

            float[,] newHeights = new float[resolution, resolution];

            // ���� �ͷ����� �۷ι� ���� �ε��� ����
            // �� �ͷ����� (resolution) ������ ������, (resolution - 1) ������ �����ٰ� ����
            int currentMinGlobalX = currentGridPos.x * (resolution - 1);
            int currentMinGlobalY = currentGridPos.y * (resolution - 1);
            int currentMaxGlobalX = currentMinGlobalX + (resolution - 1);
            int currentMaxGlobalY = currentMinGlobalY + (resolution - 1);

            // �� heightmap ���� (local index x, y) ó��
            for (int y = 0; y < resolution; y++)
            {
                for (int x = 0; x < resolution; x++)
                {
                    // ���� �ͷ����� ���� ���� (x, y)�� �۷ι� �ε���
                    int globalSampleX = currentMinGlobalX + x;
                    int globalSampleY = currentMinGlobalY + y;

                    // ������ ����: �۷ι� �ҽ� �ε���
                    int sourceGlobalX = globalSampleX - offsetX;
                    int sourceGlobalY = globalSampleY - offsetY;

                    float heightVal = 0f;
                    // ���� ����� �۷ι� �ε����� ���� �ͷ����� ���� ���� �ִٸ�
                    if (sourceGlobalX >= currentMinGlobalX && sourceGlobalX <= currentMaxGlobalX &&
                        sourceGlobalY >= currentMinGlobalY && sourceGlobalY <= currentMaxGlobalY)
                    {
                        // ���� �ͷ��� ���� ���� �ε����� ��ȯ
                        int localX = sourceGlobalX - currentMinGlobalX;
                        int localY = sourceGlobalY - currentMinGlobalY;
                        heightVal = currentInfo.heights[localY, localX];
                    }
                    else
                    {
                        // �׷��� ������, �̿� �ͷ����� ����
                        int neighborGridX = Mathf.FloorToInt((float)sourceGlobalX / (resolution - 1));
                        int neighborGridY = Mathf.FloorToInt((float)sourceGlobalY / (resolution - 1));
                        Vector2Int neighborGridPos = new Vector2Int(neighborGridX, neighborGridY);

                        if (terrainGrid.ContainsKey(neighborGridPos))
                        {
                            TerrainDataInfo neighborInfo = terrainGrid[neighborGridPos];
                            // �̿� �ͷ����� �۷ι� �ּ� ���� �ε���
                            int neighborMinGlobalX = neighborGridX * (resolution - 1);
                            int neighborMinGlobalY = neighborGridY * (resolution - 1);
                            // �̿� �ͷ��� ���� ���� �ε���
                            int neighborLocalX = sourceGlobalX - neighborMinGlobalX;
                            int neighborLocalY = sourceGlobalY - neighborMinGlobalY;
                            neighborLocalX = Mathf.Clamp(neighborLocalX, 0, neighborInfo.resolution - 1);
                            neighborLocalY = Mathf.Clamp(neighborLocalY, 0, neighborInfo.resolution - 1);
                            heightVal = neighborInfo.heights[neighborLocalY, neighborLocalX];
                        }
                        else
                        {
                            heightVal = 0f;
                        }
                    }
                    newHeights[y, x] = heightVal;
                }
            }
            Undo.RecordObject(data, "Shift Terrain Heightmap With Global Index");
            data.SetHeights(0, 0, newHeights);
        }
        Debug.Log("Shifted terrain heightmaps with global index neighbor handling by (" + offsetX + ", " + offsetY + ")");
    }
}
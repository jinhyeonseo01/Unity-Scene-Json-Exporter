using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class TerrainHeightmapShifterWithGlobalIndex : EditorWindow
{
    // 인덱스 단위 오프셋 값 (음수도 가능)
    int offsetX = 0;
    int offsetY = 0;

    [MenuItem("Tools/Json Exporter/Terrain Expansions/Shift Terrain Heightmap With Neighbors (Global Index)")]
    public static void ShowWindow()
    {
        GetWindow<TerrainHeightmapShifterWithGlobalIndex>("Shift Terrain Heightmap (Global Index)");
    }

    // 터레인 관련 정보를 저장하는 클래스
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
        // 선택된 터레인들 가져오기
        Terrain[] terrains = Selection.GetFiltered<Terrain>(SelectionMode.DeepAssets);
        if (terrains.Length == 0)
        {
            Debug.LogWarning("No terrains selected!");
            return;
        }

        // 모든 터레인의 최소 x, z를 글로벌 원점으로 설정 (터레인들의 배치가 grid 형태라고 가정)
        float globalOriginX = float.MaxValue;
        float globalOriginZ = float.MaxValue;
        foreach (Terrain t in terrains)
        {
            if (t.transform.position.x < globalOriginX)
                globalOriginX = t.transform.position.x;
            if (t.transform.position.z < globalOriginZ)
                globalOriginZ = t.transform.position.z;
        }

        // 모든 터레인이 동일한 크기라 가정 (첫 번째 터레인의 size 사용)
        TerrainDataInfo sampleInfo = new TerrainDataInfo(terrains[0]);
        Vector3 terrainSize = sampleInfo.size;
        int resolution = sampleInfo.resolution; // 각 터레인의 해상도

        // 터레인들이 배치된 글로벌 그리드 좌표와 정보를 저장
        // grid 좌표: (gridX, gridZ) = (Floor((terrain.position.x - globalOriginX) / terrainSize.x),
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

        // 각 터레인별로 새로운 하이트맵 계산
        foreach (Terrain t in terrains)
        {
            TerrainData data = t.terrainData;
            TerrainDataInfo currentInfo = new TerrainDataInfo(t);
            Vector2Int currentGridPos = terrainGridPosDict[t];

            float[,] newHeights = new float[resolution, resolution];

            // 현재 터레인의 글로벌 샘플 인덱스 범위
            // 각 터레인은 (resolution) 샘플을 가지며, (resolution - 1) 간격을 가진다고 가정
            int currentMinGlobalX = currentGridPos.x * (resolution - 1);
            int currentMinGlobalY = currentGridPos.y * (resolution - 1);
            int currentMaxGlobalX = currentMinGlobalX + (resolution - 1);
            int currentMaxGlobalY = currentMinGlobalY + (resolution - 1);

            // 각 heightmap 샘플 (local index x, y) 처리
            for (int y = 0; y < resolution; y++)
            {
                for (int x = 0; x < resolution; x++)
                {
                    // 현재 터레인의 로컬 샘플 (x, y)의 글로벌 인덱스
                    int globalSampleX = currentMinGlobalX + x;
                    int globalSampleY = currentMinGlobalY + y;

                    // 오프셋 적용: 글로벌 소스 인덱스
                    int sourceGlobalX = globalSampleX - offsetX;
                    int sourceGlobalY = globalSampleY - offsetY;

                    float heightVal = 0f;
                    // 만약 적용된 글로벌 인덱스가 현재 터레인의 범위 내에 있다면
                    if (sourceGlobalX >= currentMinGlobalX && sourceGlobalX <= currentMaxGlobalX &&
                        sourceGlobalY >= currentMinGlobalY && sourceGlobalY <= currentMaxGlobalY)
                    {
                        // 현재 터레인 내의 로컬 인덱스로 변환
                        int localX = sourceGlobalX - currentMinGlobalX;
                        int localY = sourceGlobalY - currentMinGlobalY;
                        heightVal = currentInfo.heights[localY, localX];
                    }
                    else
                    {
                        // 그렇지 않으면, 이웃 터레인을 참조
                        int neighborGridX = Mathf.FloorToInt((float)sourceGlobalX / (resolution - 1));
                        int neighborGridY = Mathf.FloorToInt((float)sourceGlobalY / (resolution - 1));
                        Vector2Int neighborGridPos = new Vector2Int(neighborGridX, neighborGridY);

                        if (terrainGrid.ContainsKey(neighborGridPos))
                        {
                            TerrainDataInfo neighborInfo = terrainGrid[neighborGridPos];
                            // 이웃 터레인의 글로벌 최소 샘플 인덱스
                            int neighborMinGlobalX = neighborGridX * (resolution - 1);
                            int neighborMinGlobalY = neighborGridY * (resolution - 1);
                            // 이웃 터레인 내의 로컬 인덱스
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
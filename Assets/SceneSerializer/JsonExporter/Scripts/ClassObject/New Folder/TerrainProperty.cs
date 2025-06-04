
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

[Serializable]
[BakeTargetType(typeof(Terrain))]
public class TerrainProperty : BakeObject
{
    public override void Preprocess()
    {
        base.Preprocess();
        var obj = (Terrain)target;

        TreePrototype[] treePrototypes = obj.terrainData.treePrototypes;
        //obj.terrainData.treeInstanceCount
        for (int i = 0; i < treePrototypes.Length; i++)
        {
            var proto = treePrototypes[i];
            if (proto != null && proto.prefab != null)
            {
                BakeUnity.PrevProcessingPrefab(proto.prefab);
            }
        }

    }
    public override JObject Bake()
    {
        JObject json = base.Bake();
        var obj = (Terrain)target;

        var nameKey = obj.terrainData.name;


        JArray layerBlendArray = new JArray();
        for (int i = 0; i < obj.terrainData.alphamapTextureCount; i++)
        {
            var texture = obj.terrainData.GetAlphamapTexture(i);
            byte[] bytes = texture.EncodeToPNG();

            var pathInfo = BakeExtensions.PathConvert($"Assets/Models/Terrains/Textures/{nameKey}_SplatTexture_{i}.png");
            
            if (!Directory.Exists(pathInfo.unityFilePath))
                Directory.CreateDirectory(pathInfo.unityFolderPath);
            File.WriteAllBytes(pathInfo.unityFilePath, bytes);
            BakeUnity.AddResourcePath(pathInfo.unityFilePath);

            layerBlendArray.Add(pathInfo.convertFullFilePath);
        }

        TerrainData terrainData = obj.terrainData;
        int resolution = terrainData.heightmapResolution - 1; // 높이맵 해상도 (예: 513)
        int baseResolution = resolution;
        float[,] heights = terrainData.GetHeights(0, 0, resolution, resolution);

        // 16비트 정수 2바이트씩 사용하므로 전체 바이트 배열 크기 계산
        byte[] rawData = new byte[resolution * resolution * 2];
        int index = 0;

        // 높이 데이터를 순회하며 16비트 값으로 변환 후 rawData에 저장
        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                // 높이 값은 0~1 범위이므로 65535(=0xFFFF)를 곱하여 16비트 정수로 변환
                ushort heightValue = (ushort)Mathf.RoundToInt(heights[y, x] * 65535);

                // little-endian 형식으로 저장 (하위 바이트 먼저 기록)
                rawData[index++] = (byte)(heightValue & 0xFF);         // 하위 바이트
                rawData[index++] = (byte)((heightValue >> 8) & 0xFF);    // 상위 바이트
            }
        }
        string rawPath;
        string pngPath;

        {
            var folderPath = Path.Combine("Assets/", "Models/Terrains/Heights/");
            string fileName = $"{nameKey}_Height.raw";
            rawPath = Path.Combine(folderPath, fileName);
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);
            File.WriteAllBytes(rawPath, rawData);
            BakeUnity.resourceFilePathTable.Add(rawPath);
        }
        {
            Texture2D texture = new Texture2D(baseResolution, baseResolution, TextureFormat.R16, false);//heights
            texture.Apply();
            for (int y = 0; y < baseResolution; y++)
                for (int x = 0; x < baseResolution; x++)
                {
                    texture.SetPixel(x, y, Color.red * heights[y, x]);
                }
            texture.Apply();
            var folderPath = Path.Combine("Assets/", "Models/Terrains/Heights/");
            string fileName = $"{nameKey}_Height.png";
            pngPath = Path.Combine(folderPath, fileName);
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);
            File.WriteAllBytes(pngPath, texture.EncodeToPNG());
            BakeUnity.resourceFilePathTable.Add(pngPath);
        }

        JArray prefabArray = new JArray();
        TreePrototype[] treePrototypes = obj.terrainData.treePrototypes;

        for (int i = 0; i < treePrototypes.Length; i++)
        {
            var proto = treePrototypes[i];
            string guid = "";
            if (proto != null)
                guid = BakeGuid.GetGuid(proto.prefab);
            prefabArray.Add(guid);
        }

        JArray instanceArray = new JArray();
        TreeInstance[] treeInstances = obj.terrainData.treeInstances;
        foreach (TreeInstance tree in treeInstances)
        {
            JObject trans = new JObject();
            trans.Add("index", tree.prototypeIndex);
            trans.Add("position", BakeExtensions.ToJson(Vector3.Scale(tree.position, obj.terrainData.size) + obj.transform.position));
            trans.Add("rotation", tree.rotation);
            trans.Add("scale", BakeExtensions.ToJson(new Vector3(tree.widthScale, tree.heightScale, tree.widthScale)));
            instanceArray.Add(trans);
        }

        TerrainLayer[] layers = terrainData.terrainLayers;

        JArray layerArray = new JArray();
        for (int i = 0; i < layers.Length; i++)
        {
            JObject layerData = new JObject();
            TerrainLayer layer = layers[i];
            // TerrainLayer에서 기본 텍스처(보통 diffuseTexture)를 가져옵니다.
            layerData.Add("diffuse", BakeExtensions.ToJson(layer.diffuseTexture));
            if (layer.normalMapTexture != null)
                layerData.Add("normal", BakeExtensions.ToJson(layer.normalMapTexture));
            if (layer.maskMapTexture != null)
                layerData.Add("mask", BakeExtensions.ToJson(layer.maskMapTexture));
            layerData.Add("tileSize", BakeExtensions.ToJson(layer.tileSize));
            layerData.Add("tileOffset", BakeExtensions.ToJson(layer.tileOffset));
            layerArray.Add(layerData);
        }


        AssetDatabase.Refresh();



        json.Add("name", nameKey);
        json.Add("size", BakeExtensions.ToJson(obj.terrainData.size));
        json.Add("heightmapResolution", resolution);
        json.Add("alphamapResolution", obj.terrainData.alphamapResolution);
        json.Add("alphamapTextureCount", obj.terrainData.alphamapTextureCount);
        json.Add("rawPath", BakeExtensions.PathConvert(rawPath).convertFullFilePath);
        json.Add("pngPath", BakeExtensions.PathConvert(pngPath).convertFullFilePath);
        json.Add("layerBlendTextures", layerBlendArray);
        json.Add("layers", layerArray);
        json.Add("instanceCount", obj.terrainData.treeInstanceCount);
        json.Add("instanceTable", prefabArray);
        json.Add("instanceDatas", instanceArray);

        return json;
    }
}

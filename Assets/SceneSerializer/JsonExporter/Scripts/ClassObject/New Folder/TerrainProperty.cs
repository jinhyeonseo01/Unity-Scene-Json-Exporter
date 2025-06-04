
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.XR;

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

            var pathInfo = BakeExtensions.PathConvert($"Assets/Terrains/Textures/{nameKey}_SplatTexture_{i}.png");
            if (!Directory.Exists(pathInfo.unityFolderPath))
                Directory.CreateDirectory(pathInfo.unityFolderPath);
            File.WriteAllBytes(pathInfo.unityFilePath, texture.EncodeToPNG());
            BakeUnity.AddResourcePath(pathInfo.unityFilePath);

            layerBlendArray.Add(pathInfo.convertFullFilePath);
        }

        TerrainData terrainData = obj.terrainData;
        int resolution = terrainData.heightmapResolution - 1; // ���̸� �ػ� (��: 513)
        int baseResolution = resolution;
        float[,] heights = terrainData.GetHeights(0, 0, resolution, resolution);

        // 16��Ʈ ���� 2����Ʈ�� ����ϹǷ� ��ü ����Ʈ �迭 ũ�� ���
        byte[] rawData = new byte[resolution * resolution * 2];
        int index = 0;

        // ���� �����͸� ��ȸ�ϸ� 16��Ʈ ������ ��ȯ �� rawData�� ����
        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                // ���� ���� 0~1 �����̹Ƿ� 65535(=0xFFFF)�� ���Ͽ� 16��Ʈ ������ ��ȯ
                ushort heightValue = (ushort)Mathf.RoundToInt(heights[y, x] * 65535);

                // little-endian �������� ���� (���� ����Ʈ ���� ���)
                rawData[index++] = (byte)(heightValue & 0xFF);         // ���� ����Ʈ
                rawData[index++] = (byte)((heightValue >> 8) & 0xFF);    // ���� ����Ʈ
            }
        }
        string rawPath;
        string pngPath;

        {
            var pathInfo = BakeExtensions.PathConvert($"Assets/Terrains/Heights/{nameKey}_Height.raw");
            if (!Directory.Exists(pathInfo.unityFolderPath))
                Directory.CreateDirectory(pathInfo.unityFolderPath);
            File.WriteAllBytes(pathInfo.unityFilePath, rawData);
            BakeUnity.AddResourcePath(pathInfo.unityFilePath);
            rawPath = pathInfo.convertFullFilePath;
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

            var pathInfo = BakeExtensions.PathConvert($"Assets/Terrains/Heights/{nameKey}_Height.png");
            if (!Directory.Exists(pathInfo.unityFolderPath))
                Directory.CreateDirectory(pathInfo.unityFolderPath);
            File.WriteAllBytes(pathInfo.unityFilePath, texture.EncodeToPNG());
            BakeUnity.AddResourcePath(pathInfo.unityFilePath);
            pngPath = pathInfo.convertFullFilePath;
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
            // TerrainLayer���� �⺻ �ؽ�ó(���� diffuseTexture)�� �����ɴϴ�.
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
        json.Add("rawPath", rawPath);
        json.Add("pngPath", pngPath);
        json.Add("layerBlendTextures", layerBlendArray);
        json.Add("layers", layerArray);
        json.Add("instanceCount", obj.terrainData.treeInstanceCount);
        json.Add("instanceTable", prefabArray);
        json.Add("instanceDatas", instanceArray);

        return json;
    }
}

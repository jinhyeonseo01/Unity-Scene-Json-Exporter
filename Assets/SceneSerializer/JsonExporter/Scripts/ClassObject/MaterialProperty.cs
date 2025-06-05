using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


[Serializable]
[BakeTarget(typeof(Material))]
public class MaterialProperty : BakeObject
{
    public override void Preprocess()
    {
        base.Preprocess();
        var obj = target as Material;
    }

    public override JObject Bake(JObject totalJson)
    {
        JObject json = base.Bake(totalJson);
        var material = (Material)target;


        Dictionary<string, List<string>> propertyNameTable = new Dictionary<string, List<string>>();
        propertyNameTable.Add("texture",
            material.GetPropertyNames(MaterialPropertyType.Texture).ToList());
        propertyNameTable.Add("float",
            material.GetPropertyNames(MaterialPropertyType.Float).ToList());
        propertyNameTable.Add("vector",
            material.GetPropertyNames(MaterialPropertyType.Vector).ToList());
        propertyNameTable.Add("matrix",
            material.GetPropertyNames(MaterialPropertyType.Matrix).ToList());
        propertyNameTable.Add("int",
            material.GetPropertyNames(MaterialPropertyType.Int).ToList());

        JObject materialJson = new JObject();
        JObject dataJson = new JObject();

        materialJson.Add("name", material.name);
        materialJson.Add("guid", BakeGuid.GetGuid(material));
        materialJson.Add("shaderName", material.shader.name.Split("/")[^1]);
        materialJson.Add("renderOrder", material.renderQueue);
        materialJson.Add("culling", "back");

        materialJson.Add("datas", dataJson);


        JArray textureDatas = new JArray();
        JArray floatDatas = new JArray();
        JArray intDatas = new JArray();
        JArray vectorDatas = new JArray();
        JArray matrixDatas = new JArray();

        dataJson.Add("textures", textureDatas);
        foreach (var value in propertyNameTable["texture"])
        {
            JObject data = new JObject();
            data["name"] = value;
            var texture = material.GetTexture(value);
            if (texture == null)
                continue;

            textureDatas.Add(BakeExtensions.ToJson(texture));


            Vector2 offset = material.GetTextureOffset(value);
            Vector2 size = material.GetTextureScale(value);
            JObject dataST = new JObject();
            dataST["name"] = value + $"_ST";
            dataST["data"] = BakeExtensions.ToJson(new Vector4(size.x, size.y, offset.x, offset.y));
            vectorDatas.Add(dataST);

        }

        dataJson.Add("floats", floatDatas);
        foreach (var value in propertyNameTable["float"])
        {
            JObject data = new JObject();
            data["name"] = value;
            data["data"] = material.GetFloat(value);
            floatDatas.Add(data);
        }

        dataJson.Add("ints", intDatas);
        foreach (var value in propertyNameTable["int"])
        {
            JObject data = new JObject();
            data["name"] = value;
            data["data"] = material.GetInt(value);
            intDatas.Add(data);
        }

        dataJson.Add("vectors", vectorDatas);
        foreach (var value in propertyNameTable["vector"])
        {
            JObject data = new JObject();
            data["name"] = value;
            data["data"] = BakeExtensions.ToJson(material.GetVector(value));
            vectorDatas.Add(data);
        }
        dataJson.Add("matrixs", matrixDatas);
        foreach (var value in propertyNameTable["matrix"])
        {
            JObject data = new JObject();
            data["name"] = value;
            data["data"] = BakeExtensions.ToJson(material.GetMatrix(value));
            matrixDatas.Add(data);
        }

        return materialJson;
    }
}
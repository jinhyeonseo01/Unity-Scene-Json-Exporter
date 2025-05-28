using UnityEngine;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using UnityEditor;
using System.Linq;
using static UnityEngine.Rendering.DebugUI;

public class BakeExtensions
{
    public static string PathConvert(string path)
    {
        return path.Replace("Assets/", BakeUnity.definePath_Resources); ;
    }
    public static JToken ToJson(object obj, bool changeLink = false)
    {
        if (obj is Matrix4x4)
        {
            var value = (Matrix4x4)obj;
            JArray array = new JArray();
            for (int i = 0; i < 16; i++)
                array.Add(value[i]);
            return array;
        }
        if (obj is Quaternion)
        {
            var value = (Quaternion)obj;
            JArray array = new JArray();
            array.Add(value.x);
            array.Add(value.y);
            array.Add(value.z);
            array.Add(value.w);
            return array;
        }
        if (obj is Vector4)
        {
            var value = (Vector4)obj;
            JArray array = new JArray();
            array.Add(value.x);
            array.Add(value.y);
            array.Add(value.z);
            array.Add(value.w);
            return array;
        }
        if (obj is Vector3)
        {
            var value = (Vector3)obj;
            JArray array = new JArray();
            array.Add(value.x);
            array.Add(value.y);
            array.Add(value.z);
            return array;
        }
        if (obj is Vector2)
        {
            var value = (Vector2)obj;
            JArray array = new JArray();
            array.Add(value.x);
            array.Add(value.y);
            return array;
        }
        if (obj is UnityEngine.Color)
        {
            var value = (Color)obj;
            JArray array = new JArray();
            array.Add(value.r);
            array.Add(value.g);
            array.Add(value.b);
            array.Add(value.a);
            return array;
        }
        if (obj is Color32)
        {
            var value = (Color32)obj;
            JArray array = new JArray();
            array.Add(value.r / (float)255);
            array.Add(value.g / (float)255);
            array.Add(value.b / (float)255);
            array.Add(value.a / (float)255);
            return array;
        }

        if (changeLink)
        {
            if (BakeUnity.objectToGuidTable.TryGetValue(obj, out string guid))
                return guid;
            return "null";
        }
        else
        {
            if (obj is Material)
            {
                var material = obj as Material;

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
                if (BakeUnity.TryGetGuid(material, out var guid))
                    materialJson.Add("guid", guid);
                materialJson.Add("shaderName", material.shader.name.Split("/")[^1]);
                materialJson.Add("renderOrder", material.renderQueue);
                //try
                //{
                //    float Data = material.GetFloat("_Cull");
                //    if(Data == 0)
                //        materialJson.Add("culling", "both");
                //    if (Data == 1)
                //        materialJson.Add("culling", "front");
                //    if (Data == 2)
                //        materialJson.Add("culling", "back");
                //}
                //catch {
                //    materialJson.Add("culling", "back");
                //}
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
                    var path = AssetDatabase.GetAssetPath(texture).Trim();
                    BakeUnity.filePathSet.Add(path);

                    path = path.Replace("Assets/", BakeUnity.definePath_Resources);
                    var fileName = path.Split("/").Last();
                    var name = string.Join(".", fileName.Split(".")[0..^1]);
                    if (path.Contains("unity_builtin_extra"))
                    {
                        data["path"] = "none";
                        data["fileName"] = "none";
                        data["originalName"] = "none";
                    }
                    else
                    {
                        data["path"] = path;
                        data["fileName"] = fileName;
                        data["originalName"] = name;
                    }
                    if (!string.IsNullOrEmpty(path))
                    {
                        textureDatas.Add(data);

                        Vector2 offset = material.GetTextureOffset(value);
                        Vector2 size = material.GetTextureScale(value);
                        JObject dataST = new JObject();
                        dataST["name"] = value + $"_ST";
                        dataST["data"] = ToJson(new Vector4(size.x, size.y, offset.x, offset.y));
                        vectorDatas.Add(dataST);
                    }
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
                    data["data"] = ToJson(material.GetVector(value));
                    vectorDatas.Add(data);
                }
                dataJson.Add("matrixs", matrixDatas);
                foreach (var value in propertyNameTable["matrix"])
                {
                    JObject data = new JObject();
                    data["name"] = value;
                    data["data"] = ToJson(material.GetMatrix(value));
                    matrixDatas.Add(data);
                }

                return materialJson;
            }

            if (obj is Texture2D)
            {
                JObject data = new JObject();
                var path = AssetDatabase.GetAssetPath((Texture2D)obj).Trim();
                if (path.Contains("unity_builtin_extra"))
                {
                    data["path"] = "none";
                    data["fileName"] = "none";
                    data["originalName"] = "none";
                    return data;
                }
                BakeUnity.filePathSet.Add(path);

                path = path.Replace("Assets/", BakeUnity.definePath_Resources);
                var fileName = path.Split("/").Last();
                var name = string.Join(".", fileName.Split(".")[0..^1]);

                data["path"] = path;
                data["fileName"] = fileName;
                data["originalName"] = name;
                return data;
            }

            if (obj is Mesh)
            {
                var mesh = obj as Mesh;
                JObject data = new JObject();
                var path = AssetDatabase.GetAssetPath(mesh).Trim();
                if (!string.IsNullOrEmpty(path))
                {
                    switch (path)
                    {
                        case "Library/unity default resources":
                            {
                                if (!BakeUnity.nameToPathTable.ContainsKey(mesh.name))
                                    BakeUnity.nameToPathTable[mesh.name] = mesh.name + ".fbx";

                                path = BakeUnity.definePath_ConfigResources + BakeUnity.nameToPathTable[mesh.name];
                                break;
                            }
                        default:
                            {
                                BakeUnity.filePathSet.Add(path);
                                path = path.Replace("Assets/", BakeUnity.definePath_Resources);
                                break;
                            }
                    }
                    var fileName = path.Split("/").Last();
                    var name = string.Join(".", fileName.Split(".")[0..^1]);

                    data["path"] = path;
                    data["fileName"] = fileName;
                    data["modelName"] = name;
                    data["meshName"] = mesh.name;
                    data["boundCenter"] = ToJson(mesh.bounds.center);
                    data["boundExtent"] = ToJson(mesh.bounds.extents);
                }
                return data;
            }

        }
        //string jsonString = System.Text.Encoding.UTF8.GetString(SerializationUtility.SerializeValue(obj, DataFormat.JSON));
        return new JObject(obj);
    }
}

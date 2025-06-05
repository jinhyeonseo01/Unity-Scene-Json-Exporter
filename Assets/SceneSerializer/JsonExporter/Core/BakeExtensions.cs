using UnityEngine;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using UnityEditor;
using System.Linq;


public struct BakePathInfo
{
    public string unityFilePath;
    public string unityFolderPath;
    public string convertFullFilePath;
    public string convertFullFolderPath;
    
    public string fileFullName;
    public string fileName;
    public string fileExtension;
}
public class BakeExtensions
{
    public static BakePathInfo GetPathInfoFromAsset<T>(T asset) where T : UnityEngine.Object
    {
        if (asset == null)
            return new BakePathInfo();
        var assetPath = AssetDatabase.GetAssetPath(asset).Trim();

        if (string.IsNullOrEmpty(assetPath))
            return new BakePathInfo();
        bool configFile = false;
        if (assetPath.Contains("unity default resources") || assetPath.Contains("unity_builtin_extra"))
        {
            configFile = true;
            if (asset is Mesh mesh)
                assetPath = "Assets/" + mesh.name + ".fbx";
            if (asset is Texture2D texture2d)
                assetPath = "Assets/" + texture2d.name + ".png";
            if (asset is Font font)
                assetPath = "Assets/" + font.name + ".tff";
        }
        else
        {
            BakeUnity.AddResourcePath(assetPath);
        }

        BakePathInfo pathInfo = PathConvert(assetPath, configFile);
        return pathInfo;
        
    }
    public static BakePathInfo PathConvert(string path, bool configFile = false)
    {
        BakePathInfo info;

        info.unityFilePath = path.Trim();
        if(configFile)
            info.convertFullFilePath = info.unityFilePath.Replace("Assets/", BakeUnity.DefinePathConfigResources);
        else
            info.convertFullFilePath = info.unityFilePath.Replace("Assets/", BakeUnity.DefinePathResources);

        int lastSlash = info.convertFullFilePath.LastIndexOf('/');
        if (lastSlash >= 0)
            info.convertFullFolderPath = info.convertFullFilePath[..lastSlash];
        else
            info.convertFullFolderPath = info.convertFullFilePath;

        lastSlash = info.unityFilePath.LastIndexOf('/');
        if (lastSlash >= 0)
            info.unityFolderPath = info.unityFilePath[..lastSlash];
        else
            info.unityFolderPath = info.unityFilePath;


        info.fileFullName = info.unityFilePath.Split("/").Last();

        var fileParts = info.fileFullName.Split('.');
        if (fileParts.Length > 1)
        {
            info.fileName = string.Join(".", fileParts[0..^1]);
            info.fileExtension = fileParts[^1];
        }
        else
        {
            info.fileName = info.fileFullName;
            info.fileExtension = "";
        }

        return info;
    }
    public static JToken ToJson(object obj)
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
        
        if (obj is Texture2D texture2d)
        {
            JObject data = new JObject();
            var pathInfo = GetPathInfoFromAsset(texture2d);

            data["path"] = pathInfo.convertFullFilePath;
            data["fileName"] = pathInfo.fileFullName;
            data["originalName"] = pathInfo.fileName;
            return data;
        }

        if (obj is Mesh)
        {
            var mesh = obj as Mesh;
            JObject data = new JObject();
            var pathInfo = GetPathInfoFromAsset(mesh);

            data["path"] = pathInfo.convertFullFilePath;
            data["fileName"] = pathInfo.fileFullName;
            data["modelName"] = pathInfo.fileName;
            data["meshName"] = mesh.name;
            data["boundCenter"] = ToJson(mesh.bounds.center);
            data["boundExtent"] = ToJson(mesh.bounds.extents);
                
            return data;
        }

        if(obj == null)
            return new JObject();
        return new JObject(obj);
    }
}

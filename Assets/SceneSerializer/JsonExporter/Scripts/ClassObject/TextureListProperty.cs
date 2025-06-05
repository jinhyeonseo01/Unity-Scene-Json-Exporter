using Newtonsoft.Json.Linq;
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

[Serializable]
[BakeTarget(typeof(TextureList))]
public class TextureListProperty : BakeObject
{

    public override JObject Bake(JObject totalJson)
    {
        JObject json = base.Bake(totalJson);
        var obj = (TextureList)target;

        JObject json2 = new JObject();

        foreach (var clip in obj.textureTable)
        {
            JArray json3 = new JArray();
            foreach (var texture in clip.textures)
            {
                var pathInfo = BakeExtensions.GetPathInfoFromAsset(texture);
                BakeUnity.AddResourcePath(pathInfo.unityFilePath);
                json3.Add(pathInfo.convertFullFilePath);
            }
            json2.Add(clip.name, json3);
        }

        json.Add("textures", json2);

        return json;
    }
}
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

[Serializable]
[BakeTarget(typeof(AnimationList))]
public class AnimationListProperty : BakeObject
{

    public override JObject Bake(JObject totalJson)
    {
        JObject json = base.Bake(totalJson);
        var obj = (AnimationList)target;

        JObject json2 = new JObject();

        foreach ((string key, AnimationClip clip) in obj.animations)
        {
            var pathInfo = BakeExtensions.GetPathInfoFromAsset(clip);
            BakeUnity.AddResourcePath(pathInfo.unityFilePath);
            json2.Add(key, pathInfo.convertFullFilePath + "|" + AnimationList.ExtractTakeName(clip));
        }

        json.Add("animationKeys", json2);

        return json;
    }
}
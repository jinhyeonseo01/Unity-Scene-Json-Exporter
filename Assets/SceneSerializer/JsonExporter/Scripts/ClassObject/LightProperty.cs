using Newtonsoft.Json.Linq;
using System;
using UnityEngine;

[Serializable]
[BakeTarget(typeof(Light))]
public class LightProperty : BakeObject
{
    public override JObject Bake(JObject totalJson)
    {
        JObject json = base.Bake(totalJson);
        var obj = (Light)target;
        json.Add("lightType", obj.type.ToString());
        json.Add("color", BakeExtensions.ToJson(obj.color));
        json.Add("intensity", obj.intensity);
        json.Add("range", obj.range);
        json.Add("innerSpotAngle", obj.innerSpotAngle);
        json.Add("spotAngle", obj.spotAngle);
        json.Add("shadowAngle", obj.shadowAngle);
        return json;
    }
}
using Newtonsoft.Json.Linq;
using System;
using UnityEngine;

[Serializable]
[BakeTargetType(typeof(Transform))]
public class TransformProperty : BakeObject
{
    public override JObject Bake()
    {
        JObject json = base.Bake();
        var trans = (Transform)target;

        json["position"] = BakeExtensions.ToJson(trans.localPosition);
        json["rotation"] = BakeExtensions.ToJson(trans.localRotation);
        json["scale"] = BakeExtensions.ToJson(trans.localScale);

        return json;
    }
}
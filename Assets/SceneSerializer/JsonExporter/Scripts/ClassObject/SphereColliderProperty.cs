using Newtonsoft.Json.Linq;
using System;
using UnityEngine;

[Serializable]
[BakeTarget(typeof(SphereCollider))]
public class SphereColliderProperty : ColliderProperty
{

    public override JObject Bake(JObject totalJson)
    {
        JObject json = base.Bake(totalJson);
        var obj = (SphereCollider)target;
        
        json.Add("colliderType", "sphere");
        json.Add("center", BakeExtensions.ToJson(obj.center));
        json.Add("radius", obj.radius);

        return json;
    }
}
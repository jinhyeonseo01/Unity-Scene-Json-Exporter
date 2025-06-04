using Newtonsoft.Json.Linq;
using System;
using UnityEngine;

[Serializable]
[BakeTargetType(typeof(MeshFilter))]
public class MeshFilterProperty : BakeObject
{
    public override JObject Bake()
    {
        JObject json = base.Bake();
        var obj = (MeshFilter)target;
        json.Add("mesh", BakeExtensions.ToJson(obj.sharedMesh));

        return json;
    }
}

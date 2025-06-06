using Newtonsoft.Json.Linq;
using System;
using UnityEngine;

namespace Clrain.SceneToJson
{
    [Serializable]
    [BakeTarget(typeof(MeshFilter))]
    public class MeshFilterProperty : BakeObject
    {
        public override JObject Bake(JObject totalJson)
        {
            JObject json = base.Bake(totalJson);
            var obj = (MeshFilter)target;
            json.Add("mesh", BakeExtensions.ToJson(obj.sharedMesh));

            return json;
        }
    }
}
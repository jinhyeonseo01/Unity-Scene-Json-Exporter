using Newtonsoft.Json.Linq;
using System;
using UnityEngine;

namespace Clrain.SceneToJson
{
    [Serializable]
    [BakeTarget(typeof(MeshCollider))]
    public class MeshColliderProperty : ColliderProperty
    {

        public override JObject Bake(JObject totalJson)
        {
            JObject json = base.Bake(totalJson);
            var obj = (MeshCollider)target;
            //json
            json.Add("colliderType", "mesh");
            json.Add("convex", obj.convex);
            if (obj.sharedMesh != null)
                json.Add("mesh", BakeExtensions.ToJson(obj.sharedMesh));

            return json;
        }
    }
}
using Newtonsoft.Json.Linq;
using System;
using UnityEngine;

namespace Clrain.SceneToJson
{
    [Serializable]
    [BakeTarget(typeof(CapsuleCollider))]
    public class CapsuleColliderProperty : ColliderProperty
    {

        public override JObject Bake(JObject totalJson)
        {
            JObject json = base.Bake(totalJson);
            var obj = (CapsuleCollider)target;
            //json\
            json.Add("colliderType", "capsule");
            json.Add("center", BakeExtensions.ToJson(obj.center));
            json.Add("radius", obj.radius);
            json.Add("height", obj.height);

            return json;
        }
    }
}
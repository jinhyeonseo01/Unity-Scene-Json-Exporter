using Newtonsoft.Json.Linq;
using System;
using UnityEngine;


namespace Clrain.SceneToJson
{
    [Serializable]
    [BakeTarget(typeof(BoxCollider))]
    public class BoxColliderProperty : ColliderProperty
    {

        public override JObject Bake(JObject totalJson)
        {
            JObject json = base.Bake(totalJson);
            var obj = (BoxCollider)target;
            json.Add("colliderType", "box");
            json.Add("center", BakeExtensions.ToJson(obj.center));
            json.Add("size", BakeExtensions.ToJson(obj.size));

            return json;
        }
    }
}
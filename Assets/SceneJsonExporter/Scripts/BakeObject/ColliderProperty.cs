using Newtonsoft.Json.Linq;
using System;
using UnityEngine;


namespace Clrain.SceneToJson
{
    [Serializable]
    public class ColliderProperty : BakeObject
    {
        public override JObject Bake(JObject totalJson)
        {
            JObject json = base.Bake(totalJson);
            var obj = (Collider)target;
            json.Add("aabbCenter", BakeExtensions.ToJson(obj.bounds.center));
            json.Add("aabbExtent", BakeExtensions.ToJson(obj.bounds.extents));

            json.Add("isTrigger", obj.isTrigger);

            return json;
        }
    }
}
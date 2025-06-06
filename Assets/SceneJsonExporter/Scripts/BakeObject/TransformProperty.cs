using Newtonsoft.Json.Linq;
using System;
using UnityEngine;

namespace Clrain.SceneToJson
{
    [Serializable]
    [BakeTarget(typeof(Transform))]
    public class TransformProperty : BakeObject
    {
        public override JObject Bake(JObject totalJson)
        {
            JObject json = base.Bake(totalJson);
            var trans = (Transform)target;

            json["position"] = BakeExtensions.ToJson(trans.localPosition);
            json["rotation"] = BakeExtensions.ToJson(trans.localRotation);
            json["scale"] = BakeExtensions.ToJson(trans.localScale);

            return json;
        }
    }
}
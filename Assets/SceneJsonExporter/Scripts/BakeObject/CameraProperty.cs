using Newtonsoft.Json.Linq;
using System;
using UnityEngine;

namespace Clrain.SceneToJson
{
    [Serializable]
    [BakeTarget(typeof(Camera))]
    public class CameraProperty : BakeObject
    {
        public override JObject Bake(JObject totalJson)
        {
            JObject json = base.Bake(totalJson);
            var obj = (Camera)target;
            json.Add("isOrtho", obj.orthographic);
            json.Add("orthoSize", obj.orthographicSize);
            json.Add("near", obj.nearClipPlane);
            json.Add("far", obj.farClipPlane);
            json.Add("fovy", obj.fieldOfView);

            return json;
        }
    }
}
using Newtonsoft.Json.Linq;
using System.Linq;
using System;
using UnityEngine;


namespace Clrain.SceneToJson
{
    [Serializable]
    [BakeTarget(typeof(MeshRenderer))]
    public class MeshRendererProperty : BakeObject
    {
        public override void Preprocess()
        {
            base.Preprocess();
            var obj = (MeshRenderer)target;
            var materialList = obj.sharedMaterials.ToList();
            foreach (var material in materialList)
                BakeUnity.EnqueuePreprocess(material);
        }

        public override JObject Bake(JObject totalJson)
        {
            JObject json = base.Bake(totalJson);

            var obj = (MeshRenderer)target;
            var materialList = obj.sharedMaterials.ToList();
            var materials = new JArray();
            json.Add("mesh", BakeExtensions.ToJson(obj.gameObject.GetComponent<MeshFilter>().sharedMesh));
            json.Add("shadowCast", obj.shadowCastingMode.ToString());

            json.Add("materials", materials);
            foreach (var material in materialList)
            {
                materials.Add(BakeGuid.GetGuid(material));
            }

            return json;
        }
    }
}
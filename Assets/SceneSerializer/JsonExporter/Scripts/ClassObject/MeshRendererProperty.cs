using Newtonsoft.Json.Linq;
using System.Linq;
using System;
using UnityEngine;



[Serializable]
[BakeTargetType(typeof(MeshRenderer))]
public class MeshRendererProperty : BakeObject
{
    public override void Preprocess()
    {
        base.Preprocess();
        var obj = (MeshRenderer)target;
        var materialList = obj.sharedMaterials.ToList();
        foreach (var material in materialList)
            if (!BakeUnity.refList_Material.Contains(material))
                BakeUnity.refList_Material.Add(material);
    }

    public override JObject Bake()
    {
        JObject json = base.Bake();

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
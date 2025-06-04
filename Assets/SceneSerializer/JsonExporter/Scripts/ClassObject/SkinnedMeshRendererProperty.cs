
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using UnityEngine;

[Serializable]
[BakeTargetType(typeof(SkinnedMeshRenderer))]
public class SkinnedMeshRendererProperty : BakeObject
{
    public override void Preprocess()
    {
        base.Preprocess();
        var obj = (SkinnedMeshRenderer)target;
        var materialList = obj.sharedMaterials.ToList();
        foreach (var material in materialList)
            if (!BakeUnity.refList_Material.Contains(material))
                BakeUnity.refList_Material.Add(material);
    }


    public override JObject Bake()
    {
        JObject json = base.Bake();


        var obj = (SkinnedMeshRenderer)target;
        var materialList = obj.sharedMaterials.ToList();
        var materials = new JArray();

        json.Add("shadowCast", obj.shadowCastingMode.ToString());

        json.Add("mesh", BakeExtensions.ToJson(obj.sharedMesh));
        json.Add("boneRoot", obj.rootBone.gameObject.name);

        json.Add("materials", materials);
        foreach (var material in materialList)
        {
            materials.Add(BakeGuid.GetGuid(material));
        }

        JObject blendShapes = new JObject();
        json.Add("blendShapeCount", obj.sharedMesh.blendShapeCount);
        json.Add("blendShapes", blendShapes);
        for (int i = 0; i < obj.sharedMesh.blendShapeCount; i++)
        {
            if (!blendShapes.ContainsKey(obj.sharedMesh.GetBlendShapeName(i)))
                blendShapes.Add(obj.sharedMesh.GetBlendShapeName(i), obj.GetBlendShapeWeight(i));
        }
        return json;
    }
}
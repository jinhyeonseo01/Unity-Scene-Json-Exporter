using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

[Serializable]
[BakeTarget(typeof(ModelList))]
public class ModelListProperty : BakeObject
{
    public override JObject Bake(JObject totalJson)
    {
        JObject json = base.Bake(totalJson);
        var modelList = (ModelList)target;
        
        var bone = modelList.BakeBone();
        JArray jsonModels = new JArray();
        foreach (var modelListPack in modelList.modelsTableList)
        {
            var list = modelListPack.models;
            foreach (var obj in list)
            {
                if (obj != null)
                {
                    JObject json2 = new JObject();
                    var path = BakeExtensions.GetPathInfoFromAsset(obj);
                    BakeUnity.AddResourcePath(path.unityFilePath);
                    json2.Add("path", path.convertFullFilePath);
                    json2.Add("modelName", path.fileName);
                    json2.Add("extension", path.fileExtension);

                    jsonModels.Add(json2);
                }
            }
        }
        json.Add("models", jsonModels);
        json.Add("boneMappingTable", JObject.FromObject(bone));
        return json;
    }
}

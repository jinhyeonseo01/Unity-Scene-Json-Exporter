using Newtonsoft.Json.Linq;
using System;

[Serializable]
[BakeTarget(typeof(ModelLoader))]
public class ModelLoaderProperty : BakeObject
{
    public override JObject Bake(JObject totalJson)
    {
        JObject json = base.Bake(totalJson);
        var obj = (ModelLoader)target;
        json.Add("modelKey", BakeExtensions.ToJson(obj.modelKey));

        return json;
    }
}
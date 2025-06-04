using Newtonsoft.Json.Linq;
using System;

[Serializable]
[BakeTargetType(typeof(ModelLoader))]
public class ModelLoaderProperty : BakeObject
{
    public override JObject Bake()
    {
        JObject json = base.Bake();
        var obj = (ModelLoader)target;
        json.Add("modelKey", BakeExtensions.ToJson(obj.modelKey));

        return json;
    }
}
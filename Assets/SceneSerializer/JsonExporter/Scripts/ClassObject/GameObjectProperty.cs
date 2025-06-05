using GLTFast.Schema;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;


[Serializable]
[BakeTarget(typeof(GameObject))]
public class GameObjectProperty : BakeObject
{
    public override void Preprocess()
    {
        base.Preprocess();
        var obj = target as GameObject;

        BakeUnity.AddPreprocess(obj);
        foreach (var component in obj.GetComponents<Component>().Where(e => e != null).ToList())
            BakeUnity.AddPreprocess(component);

        BakeUnity.GameObjectFilter(obj.GetComponents<Transform>().Select(e => e.gameObject).ToList()).ForEach(e => BakeUnity.AddPreprocess(e));
    }

    public override JObject Bake(JObject totalJson)
    {
        JObject json = base.Bake(totalJson);
        var gameObject = (GameObject)target;

        json["name"] = gameObject.name;
        json["guid"] = BakeGuid.GetGuid(gameObject);
        json["active"] = gameObject.activeSelf;
        json["static"] = gameObject.isStatic;
        json["deactivate"] = gameObject.layer == LayerMask.NameToLayer("Deactivate");

        json["components"] ??= new JArray();
        json["childs"] ??= new JArray();
        json["parent"] = "";

        //----------------------------------------
        json["parent"] = BakeGuid.GetGuid(gameObject.transform.parent);

        for (int i = 0; i < gameObject.transform.childCount; i++)
        {
            (json["childs"] as JArray).Add(BakeGuid.GetGuid(gameObject.transform.GetChild(i).gameObject));
        }

        foreach (var component in gameObject.GetComponents<Component>())
        {
            (json["components"] as JArray).Add(BakeGuid.GetGuid(component));
        }
        return json;
    }
}
using Newtonsoft.Json.Linq;
using System;
using UnityEngine;

[Serializable]
[BakeTarget(typeof(Rigidbody))]
public class RigidbodyProperty : BakeObject
{

    public override JObject Bake(JObject totalJson)
    {
        JObject json = base.Bake(totalJson);

        return json;
    }
}
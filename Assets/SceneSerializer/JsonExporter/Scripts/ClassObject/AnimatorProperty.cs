using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System;
using UnityEngine;

[Serializable]
[BakeTarget(typeof(Animator))]
public class AnimatorProperty : BakeObject
{

    public override JObject Bake(JObject totalJson)
    {
        JObject json = base.Bake(totalJson);
        var obj = (Animator)target;

        Dictionary<string, Transform> boneMappingTable = new Dictionary<string, Transform>();
        if (obj.avatar != null && obj.avatar.isHuman)
        {
            JObject boneMappingJson = new JObject();
            foreach (HumanBodyBones humanBone in System.Enum.GetValues(typeof(HumanBodyBones))) {
                if (humanBone == HumanBodyBones.LastBone)
                    continue;

                Transform boneTransform = obj.GetBoneTransform(humanBone);
                if (boneTransform != null)
                {
                    boneMappingTable.Add(humanBone.ToString(), boneTransform);
                    boneMappingJson.Add(humanBone.ToString(), boneTransform.name);
                }
            }
            json.Add("boneMapping", boneMappingJson);
        }

        return json;
    }
}
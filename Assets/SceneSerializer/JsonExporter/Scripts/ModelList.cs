using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class ModelList : MonoBehaviour
{
    // Inspector에서 할당할 SharedModelsData 에셋
    [Header("할당된 .fbx 모델을 모두 복사할 리스트에 등록")]
    public List<BakeModelsData> modelsTableList;

    public Dictionary<string, string> BakeBone()
    {
        Dictionary<string, string> allBoneMappings = new Dictionary<string, string>();
        foreach (var sharedModelsData in modelsTableList)
        {
            foreach (GameObject model in sharedModelsData.models)
            {
                Animator animator = model.GetComponent<Animator>();
                if (animator != null && animator.isHuman)
                {
                    Dictionary<string, string> boneMapping = new Dictionary<string, string>();
                    foreach (HumanBodyBones bone in System.Enum.GetValues(typeof(HumanBodyBones)))
                    {
                        if (bone == HumanBodyBones.LastBone)
                            continue;

                        Transform boneTransform = animator.GetBoneTransform(bone);
                        if (boneTransform != null)
                        {
                            if (!boneMapping.ContainsKey(boneTransform.gameObject.name))
                                boneMapping.Add(boneTransform.gameObject.name, bone.ToString());
                            if (!boneMapping.ContainsKey(model.name + "$" + boneTransform.gameObject.name))
                                boneMapping.Add(model.name + "$" + boneTransform.gameObject.name, bone.ToString());
                        }
                    }
                    if (boneMapping.Count != 0)
                        foreach (var boneName in boneMapping)
                            allBoneMappings.TryAdd(boneName.Key, boneName.Value);
                }
                else
                {
                    Debug.LogWarning($"[{model.name}] No humanoid Animator found.");
                }
            }
        }
        return allBoneMappings;
    }


    void Update()
    {

    }
}
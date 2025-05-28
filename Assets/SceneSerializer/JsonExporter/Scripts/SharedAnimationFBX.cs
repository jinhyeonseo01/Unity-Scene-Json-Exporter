using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class SharedAnimationFBX : MonoBehaviour
{
    // Inspector���� �Ҵ��� SharedModelsData ����
    [Header("�Ҵ�� .fbx ���� ��� ������ ����Ʈ�� ���")]
    public SharedModelsData sharedModelsData;
    public static List<string> models = new List<string>();

    public static Dictionary<string, string> allBoneMappings = new Dictionary<string, string>();

    public void BakeBone()
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
                        if(!boneMapping.ContainsKey(boneTransform.gameObject.name))
                            boneMapping.Add(boneTransform.gameObject.name, bone.ToString());
                        if (!boneMapping.ContainsKey(model.name + "$" + boneTransform.gameObject.name))
                            boneMapping.Add(model.name + "$" + boneTransform.gameObject.name, bone.ToString());
                        //boneMapping.Add("" + boneTransform.gameObject.name, bone.ToString());
                    }
                }
                if(boneMapping.Count != 0)
                    foreach (var boneName in boneMapping)
                        allBoneMappings.TryAdd(boneName.Key, boneName.Value);
            }
            else {
                Debug.LogWarning($"[{model.name}] No humanoid Animator found.");
            }
        }
    }

    public string GetBoneMapping(string modelName)
    {
        if (allBoneMappings.TryGetValue(modelName, out string mapping))
            return mapping;
        return null;
    }

    void Update()
    {
        // �ʿ信 ���� Update���� �߰� �۾� ����
        Animator ani;
        Avatar avatar;
        HumanBodyBones bone;
        

        //avatar.
        //new HumanPoseHandler
    }
}
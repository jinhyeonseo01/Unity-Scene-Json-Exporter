using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using Sirenix.OdinInspector;


public class BakeSetting : MonoBehaviour
{
    //[FolderPath]
    [Header("유니티 경로(Assets/)를 특정 프로젝트의 Root 경로로 변환 하기 위한 경로명")]
    public string rootResourcesPathStr = "Assets/";
    [Header("유니티 리소스(ex. box.fbx, shpere.fbx 등)를 자체 리소스로 연결하기 위한 경로명")]
    [Tooltip("이 숫자는 플레이어의 체력을 나타냅니다.")]
    public string rootConfigResourcesPath = "Assets/Configs/";
    [HideInInspector]
    public string jsonExportPath = "./Assets/Exports/";
    [HideInInspector]
    public string resourceExportPath = "./Assets/Exports/Resources/";
    public void PathUpdate()
    {
        BakeUnity.definePath_Resources = rootResourcesPathStr;
        BakeUnity.definePath_ConfigResources = rootConfigResourcesPath;
        BakeUnity.exportPath = jsonExportPath;
        BakeUnity.exportResourcePath = resourceExportPath;
    }
}

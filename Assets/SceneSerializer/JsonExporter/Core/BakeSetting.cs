using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using Sirenix.OdinInspector;


public class BakeSetting : MonoBehaviour
{
    //[FolderPath]
    [Header("����Ƽ ���(Assets/)�� Ư�� ������Ʈ�� Root ��η� ��ȯ �ϱ� ���� ��θ�")]
    public string rootResourcesPathStr = "Assets/";
    [Header("����Ƽ ���ҽ�(ex. box.fbx, shpere.fbx ��)�� ��ü ���ҽ��� �����ϱ� ���� ��θ�")]
    [Tooltip("�� ���ڴ� �÷��̾��� ü���� ��Ÿ���ϴ�.")]
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

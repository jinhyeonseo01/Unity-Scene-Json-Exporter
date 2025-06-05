using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using Sirenix.OdinInspector;


public class ExportSetting : MonoBehaviour
{
    //[FolderPath]
    [Header("Path prefix to convert Unity¡¯s Assets/ to a specific project¡¯s root directory")]
    public string rootResourcesPathStr = "Assets/";

    [Header("Path prefix for linking Unity resources (e.g., box.fbx, sphere.fbx) as internal resources")]
    [Tooltip("This number represents the player¡¯s health.")]
    public string rootConfigResourcesPath = "Assets/Configs/";

    [HideInInspector]
    public string jsonExportPath = "./Assets/Exports/";

    [HideInInspector]
    public string resourceExportPath = "./Assets/Exports/Resources/";

    public void PathUpdate()
    {
        BakeUnity.DefinePathResources = rootResourcesPathStr;
        BakeUnity.DefinePathConfigResources = rootConfigResourcesPath;
        BakeUnity.ExportJsonPath = jsonExportPath;
        BakeUnity.ExportResourcePath = resourceExportPath;
    }
}
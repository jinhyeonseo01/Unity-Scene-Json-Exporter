using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using NUnit.Framework;
using Toolbox.Editor.Internal;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;


[Serializable]
public class CList
{
    [ReorderableList]                // ¡ç ¿©±â!
    public List<Texture> textures;
}

public class TextureList : MonoBehaviour
{
    [ReorderableList]
    public SerializedDictionary<string, CList> textureTable;


    private void Start()
    {

    }
}

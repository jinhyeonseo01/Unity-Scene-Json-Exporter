using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Clrain.SceneToJson
{
    [Serializable]
    public class CList
    {
        public string name;
        public List<Texture> textures;

        public void Deconstruct(out string name, out List<Texture> textures)
        {
            name = this.name;
            textures = this.textures;
        }
    }

    public class TextureList : MonoBehaviour
    {
        public List<CList> textureTable;


        private void Start()
        {

        }
    }

}
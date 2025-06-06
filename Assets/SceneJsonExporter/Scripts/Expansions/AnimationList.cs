using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;


namespace Clrain.SceneToJson
{

    [Serializable]
    public class AnimationPair
    {
        public string name;
        public AnimationClip clip;
        public void Deconstruct(out string name, out AnimationClip clip)
        {
            name = this.name;
            clip = this.clip;
        }
    }

    public class AnimationList : MonoBehaviour
    {
        public List<AnimationPair> animations;

        private string takeName = "";
        private void Start()
        {

        }
        public static string ExtractTakeName(AnimationClip clip)
        {
            // FBX ������ ��θ� ������ �� meta ���� ��� ����
            string assetPath = AssetDatabase.GetAssetPath(clip);
            string metaPath = assetPath + ".meta";

            if (!File.Exists(metaPath))
            {
                Debug.LogWarning("Meta ������ ã�� �� �����ϴ�: " + metaPath);
                return "";
            }
            Dictionary<string, string> extractedPairs = new Dictionary<string, string>();
            string[] lines = File.ReadAllLines(metaPath);

            Regex nameRegex = new Regex(@"name:\s*(\S+)", RegexOptions.IgnoreCase);
            Regex takeNameRegex = new Regex(@"takeName:\s*(\S+)", RegexOptions.IgnoreCase);

            for (int i = 1; i < lines.Length; i++)
            {
                string currentLine = lines[i].Trim();
                if (currentLine.StartsWith("takeName:"))
                {
                    Match takeNameMatch = takeNameRegex.Match(currentLine);
                    if (takeNameMatch.Success)
                    {
                        string takeNameValue = takeNameMatch.Groups[1].Value;

                        // �ٷ� �� ���� name:���� Ȯ��
                        string previousLine = lines[i - 1].Trim();
                        Match nameMatch = nameRegex.Match(previousLine);
                        if (nameMatch.Success)
                        {
                            string nameValue = nameMatch.Groups[1].Value;
                            extractedPairs.Add(nameValue, takeNameValue);
                        }
                    }
                }
            }
            if (extractedPairs.ContainsKey(clip.name))
                return extractedPairs[clip.name];
            return clip.name;
        }
    }
}
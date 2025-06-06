using System.Collections.Generic;
using UnityEngine;

namespace Clrain.SceneToJson
{
    [CreateAssetMenu(fileName = "SharedModelsData", menuName = "Shared/Models Data", order = 1)]
    public class BakeModelsData : ScriptableObject
    {
        public List<GameObject> models = new List<GameObject>();
    }
}
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SharedModelsData", menuName = "Shared/Models Data", order = 1)]
public class SharedModelsData : ScriptableObject
{
    public List<GameObject> models = new List<GameObject>();
}
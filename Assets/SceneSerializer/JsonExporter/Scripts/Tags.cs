using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public enum MultiTag
{ 
    NonCulling = 0,
    NonInstancing = 1,
    Player = 2,
    NPC = 3,
    Monster = 4,

}

public class Tags : MonoBehaviour
{
    public List<MultiTag> tagList;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

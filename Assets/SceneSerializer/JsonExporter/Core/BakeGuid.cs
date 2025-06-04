#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;


public class BakeGuid
{
    protected static Dictionary<object, string> guidTable;
    protected static Dictionary<Type, List<object>> typeObjectTable;


    private static void Init()
    {
        if (guidTable == null)
        {
            guidTable ??= new Dictionary<object, string>(2048);
        }

        if (typeObjectTable == null)
        {
            typeObjectTable ??= new Dictionary<Type, List<object>>(128);
            typeObjectTable.Clear();

            typeObjectTable.Add(typeof(GameObject), new List<object>(8192));
            typeObjectTable.Add(typeof(Material), new List<object>(8192));
            typeObjectTable.Add(typeof(Component), new List<object>(8192));
        }
    }

    public static bool SetGuid<T>(T element) where T : class
    {
        Init();

        var obj = element as UnityEngine.Object;
        if (element == null)
            return false;
        if (guidTable.TryGetValue(element, out var existing))
            return false;

        if (typeObjectTable.ContainsKey(typeof(T)))
            typeObjectTable[typeof(T)].Add(element);

        if (obj != null)
        {
#if UNITY_EDITOR
            GlobalObjectId gid = GlobalObjectId.GetGlobalObjectIdSlow(obj);
            guidTable.Add(element, $"static_{gid.assetGUID}_{gid.targetObjectId}");
            return true;
#else
            guidTable.Add(element, System.Guid.NewGuid().ToString());
            return true;
#endif
        }
        else
        {
            guidTable.Add(element, System.Guid.NewGuid().ToString());
            return true;
        }
        return false;
    }

    public static string GetGuid<T>(T element) where T : class
    {
        string id = "";
        if (element == null)
            return id;
        SetGuid(element);
        if (guidTable.ContainsKey(element))
            id = guidTable[element];
        return id;
    }

}

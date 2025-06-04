using System;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEngine;
using System.IO;
using UnityEditor;
using System.Collections.Generic;
using UnityEngine.AI;
using Unity.AI.Navigation;
using System.Reflection;


[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public class BakeTargetTypeAttribute : Attribute
{
    /// 이 프로퍼티 클래스가 타겟으로 삼을 Unity 컴포넌트 타입
    /// </summary>
    public Type TargetType { get; }

    public BakeTargetTypeAttribute(Type type) { TargetType = type; }
}
public interface IBakeProcess
{
    /// <summary>
    /// Performs necessary preprocessing tasks before baking.
    /// </summary>
    public void Preprocess();
    /// <summary>
    /// Executes the bake operation and returns the result in JSON format.
    /// </summary>
    public JObject Bake();
}

[Serializable]
public class BakeObject : IBakeProcess
{
    private static Dictionary<string, BakeObject> bakeClassObjectNameTable;
    private static Dictionary<string, BakeObject> bakeClassObjectGuidTable;

    private string typeName;
    protected string guid;
    protected object target;

    public virtual void Preprocess()
    {

    }

    public virtual JObject Bake()
    {
        JObject json = new JObject();
        json["type"] = typeName;
        json["guid"] = guid;
        return json;
    }
    public void SetTarget<T>(T element) where T : class
    {
        target = element;
        typeName = element.GetType().Name;
        guid = BakeGuid.GetGuid(element);
    }

    public static void Init()
    {
        bakeClassObjectNameTable = new Dictionary<string, BakeObject>(128);
        bakeClassObjectGuidTable = new Dictionary<string, BakeObject>(2048);

        Type baseType = typeof(BakeObject);
        Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
        var derivedTypes = new List<Type>();

        foreach (var asm in assemblies)
        {
            Type[] types;
            try
            {
                types = asm.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                types = ex.Types.Where(t => t != null).ToArray();
            }

            foreach (var t in types)
            {
                if (t.IsClass && !t.IsAbstract && baseType.IsAssignableFrom(t) && t != baseType)
                {
                    var attrib = t.GetCustomAttribute<BakeTargetTypeAttribute>();
                    if (attrib != null)
                        derivedTypes.Add(t);
                }
            }
        }

        foreach (var bakeClassObjectType in derivedTypes)
        {
            var attrib = (bakeClassObjectType.GetCustomAttribute<BakeTargetTypeAttribute>() as BakeTargetTypeAttribute);
            bakeClassObjectNameTable.Add(attrib.TargetType.Name, Activator.CreateInstance(bakeClassObjectType) as BakeObject);
        }
    }
    public static BakeObject CreateProperty<T>(T element) where T : class
    {
        string typeName = element.GetType().Name;
        string guid = BakeGuid.GetGuid(element);
        if (bakeClassObjectNameTable.TryGetValue(typeName, out var value) && (!bakeClassObjectGuidTable.ContainsKey(guid)))
        {
            var bakeData = value.MemberwiseClone() as BakeObject;
            bakeData.target = element;
            bakeData.typeName = typeName;
            bakeData.guid = guid;
            bakeClassObjectGuidTable.Add(BakeGuid.GetGuid(element), bakeData);
            return bakeData;
        }
        return null;
    }

    public static BakeObject GetProperty<T>(T element) where T : class
    {
        if (bakeClassObjectGuidTable.ContainsKey(BakeGuid.GetGuid(element)))
            return bakeClassObjectGuidTable[BakeGuid.GetGuid(element)];
        else
            return CreateProperty(element);
        return null;
    }
}
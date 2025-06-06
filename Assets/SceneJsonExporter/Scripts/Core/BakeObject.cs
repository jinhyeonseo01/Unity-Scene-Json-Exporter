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
using System.ComponentModel;

namespace Clrain.SceneToJson
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class BakeTargetAttribute : Attribute
    {
        /// 이 프로퍼티 클래스가 타겟으로 삼을 Unity 컴포넌트 타입
        /// </summary>
        public Type TargetType { get; }

        public BakeTargetAttribute(Type type) { TargetType = type; }
    }
    public interface IBakeProcess
    {
        public object target { get; set; }
        /// <summary>
        /// Performs necessary preprocessing tasks before baking.
        /// </summary>
        public void Preprocess();
        /// <summary>
        /// Executes the bake operation and returns the result in JSON format.
        /// </summary>
        public JObject Bake(JObject totalJson);
    }

    [Serializable]
    public class BakeObject : IBakeProcess
    {
        private static Dictionary<string, BakeObject> bakeObjectNameTable;
        private static Dictionary<string, BakeObject> bakeObjectGuidTable;

        private string typeName;
        protected string guid;

        public object target { get; set; }

        public virtual void Preprocess()
        {
            BakeGuid.SetGuid(target);
        }

        public virtual JObject Bake(JObject totalJson)
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
            bakeObjectNameTable = new Dictionary<string, BakeObject>(128);
            bakeObjectGuidTable = new Dictionary<string, BakeObject>(2048);

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
                        var attrib = t.GetCustomAttribute<BakeTargetAttribute>();
                        if (attrib != null)
                            derivedTypes.Add(t);
                    }
                }
            }

            foreach (var bakeClassObjectType in derivedTypes)
            {
                var attrib = (bakeClassObjectType.GetCustomAttribute<BakeTargetAttribute>() as BakeTargetAttribute);
                bakeObjectNameTable.Add(attrib.TargetType.Name, Activator.CreateInstance(bakeClassObjectType) as BakeObject);
            }
        }
        public static BakeObject CreateProperty<T>(T element) where T : class
        {
            string typeName = element.GetType().Name;
            string guid = BakeGuid.GetGuid(element);
            if (bakeObjectNameTable.TryGetValue(typeName, out var value))
            {
                if ((!bakeObjectGuidTable.ContainsKey(guid)))
                {
                    var bakeData = value.MemberwiseClone() as BakeObject;
                    bakeData.SetTarget(element);
                    bakeObjectGuidTable.Add(guid, bakeData);
                    return bakeData;
                }
                return bakeObjectGuidTable[guid];
            }
            return null;
        }

        public static BakeObject GetProperty<T>(T element) where T : class
        {
            if (bakeObjectGuidTable.ContainsKey(BakeGuid.GetGuid(element)))
                return bakeObjectGuidTable[BakeGuid.GetGuid(element)];
            else
                return CreateProperty(element);
            return null;
        }
    }
}
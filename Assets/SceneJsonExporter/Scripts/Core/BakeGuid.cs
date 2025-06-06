#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Clrain.SceneToJson
{
    public static class BakeGuid
    {
        private static readonly Dictionary<object, string> guidTable;
        private static readonly Dictionary<Type, List<object>> typeObjectTable;


        static BakeGuid()
        {
            guidTable = new Dictionary<object, string>(2048);
            typeObjectTable = new Dictionary<Type, List<object>>(128);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void TryRegisterType(Type t)
        {
            if (!typeObjectTable.ContainsKey(t))
                typeObjectTable[t] = new List<object>(2048);
        }


        public static bool SetGuid<T>(T element) where T : class
        {
            if (element == null) return false;
            if (guidTable.ContainsKey(element)) return false;

            // 타입별 객체 테이블에 보관
            TryRegisterType(typeof(T));
            typeObjectTable[typeof(T)].Add(element);

            if (element is UnityEngine.Object uObj)
            {
#if UNITY_EDITOR
                GlobalObjectId gid = GlobalObjectId.GetGlobalObjectIdSlow(uObj);
                guidTable[element] = $"fixed_{gid.assetGUID}_{gid.targetObjectId}";
#else
                guidTable[element] = Guid.NewGuid().ToString();
#endif
            }
            else
            {
                guidTable[element] = Guid.NewGuid().ToString();
            }

            return true;
        }

        public static string GetGuid<T>(T element) where T : class
        {
            if (element == null) return string.Empty;

            if (!guidTable.ContainsKey(element))
                SetGuid(element);

            return guidTable.TryGetValue(element, out var id) ? id : string.Empty;
        }
    }
}

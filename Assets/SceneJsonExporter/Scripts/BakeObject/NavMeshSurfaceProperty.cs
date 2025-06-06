using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System;
using Unity.AI.Navigation;
using UnityEngine.AI;
using UnityEngine;
using System.Linq;

namespace Clrain.SceneToJson
{
    public struct Edge
    {
        public int v1, v2;
        public Edge(int a, int b)
        {
            if (a < b)
            {
                v1 = a;
                v2 = b;
            }
            else
            {
                v1 = b;
                v2 = a;
            }
        }
        public override bool Equals(object obj)
        {
            if (!(obj is Edge))
                return false;
            var e = (Edge)obj;
            return v1 == e.v1 && v2 == e.v2;
        }
        public override int GetHashCode()
        {
            unchecked { return v1 * 16548 ^ v2; }
        }
    }

    class Vector3Comparer : IEqualityComparer<Vector3>
    {
        const float EPS = 0.001f;
        public bool Equals(Vector3 a, Vector3 b)
            => (a - b).sqrMagnitude < EPS * EPS;
        public int GetHashCode(Vector3 v)
        {
            unchecked
            {
                int x = Mathf.RoundToInt(v.x / EPS);
                int y = Mathf.RoundToInt(v.y / EPS);
                int z = Mathf.RoundToInt(v.z / EPS);
                return x * 73856093 ^ y * 19349663 ^ z * 83492791;
            }
        }
    }

    [Serializable]
    [BakeTarget(typeof(NavMeshSurface))]
    public class NavMeshSurfaceProperty : BakeObject
    {

        public override JObject Bake(JObject totalJson)
        {
            JObject json = base.Bake(totalJson);
            var obj = (NavMeshSurface)target;

            //NavMeshTriangulation navMeshData = NavMesh.CalculateTriangulation();

            BuildGraph(out var verts, out var adjacency, out var boundaryEdges, out var tri);
            JArray json2 = new JArray();
            for (int i = 0; i < verts.Count; i++)
            {
                json2.Add(BakeExtensions.ToJson(verts[i]));
            }
            json.Add("vertexs", json2);

            JArray json3 = new JArray();
            for (int i = 0; i < adjacency.Count; i++)
            {
                JArray json4 = new JArray();
                for (int j = 0; j < adjacency[i].Count; j++)
                {
                    json4.Add(adjacency[i][j]);
                }
                json3.Add(json4);
            }
            json.Add("indexAdjacencies", json3);

            JArray json7 = new JArray();
            for (int i = 0; i < tri.Count; i++)
            {
                JArray json8 = new JArray();
                for (int j = 0; j < tri[i].Length; j++)
                {
                    json8.Add(tri[i][j]);
                }
                json7.Add(json8);
            }
            json.Add("indexTriangles", json7);


            JArray json5 = new JArray();
            for (int i = 0; i < boundaryEdges.Count; i++)
            {
                JArray json6 = new JArray();
                json6.Add(boundaryEdges[i].v1);
                json6.Add(boundaryEdges[i].v2);
                json5.Add(json6);
            }
            json.Add("indexEdges", json5);

            return json;
        }

        public static void BuildGraph(
            out List<Vector3> vertices,
            out List<List<int>> adjacency,
            out List<Edge> edges,
            out List<int[]> triangles)
        {
            // 1) NavMesh 데이터 가져오기
            var nav = NavMesh.CalculateTriangulation();
            var rawVerts = nav.vertices;
            var rawIdxs = nav.indices;

            // 2) 중복 제거: 원본 정점 → 고유 정점 인덱스 매핑
            var uniqueVerts = new List<Vector3>();
            var mapIndex = new int[rawVerts.Length];
            var dict = new Dictionary<Vector3, int>(new Vector3Comparer());

            for (int i = 0; i < rawVerts.Length; i++)
            {
                var v = rawVerts[i];
                if (!dict.TryGetValue(v, out int uid))
                {
                    uid = uniqueVerts.Count;
                    uniqueVerts.Add(v);
                    dict[v] = uid;
                }
                mapIndex[i] = uid;
            }

            vertices = uniqueVerts;

            // 3) 인접 리스트 초기화
            var adjacency2 = new List<List<int>>(vertices.Count);
            adjacency2 = new List<List<int>>(vertices.Count);
            for (int i = 0; i < vertices.Count; i++)
                adjacency2.Add(new List<int>());

            // 4) 에지(정점 쌍)별 카운트
            var edgeCount = new Dictionary<Edge, int>();
            var tris = new List<int[]>();

            // Helper: 인접 추가
            void AddAdj(int from, int to)
            {
                var list = adjacency2[from];
                if (!list.Contains(to))
                    list.Add(to);
            }
            // Helper: 에지 카운트
            void CountEdge(Edge e)
            {
                if (edgeCount.ContainsKey(e)) edgeCount[e]++;
                else edgeCount[e] = 1;
            }

            // 5) 삼각형 순회
            for (int i = 0; i < rawIdxs.Length; i += 3)
            {
                int a = mapIndex[rawIdxs[i + 0]];
                int b = mapIndex[rawIdxs[i + 1]];
                int c = mapIndex[rawIdxs[i + 2]];

                // 인접 관계
                AddAdj(a, b); AddAdj(a, c);
                AddAdj(b, a); AddAdj(b, c);
                AddAdj(c, a); AddAdj(c, b);

                // 에지 카운트
                CountEdge(new Edge(a, b));
                CountEdge(new Edge(b, c));
                CountEdge(new Edge(c, a));
                tris.Add(new[] { a, b, c });
            }
            triangles = tris;

            // 6) 경계 에지(카운트 == 1)만 추출
            edges = edgeCount
                .Where(kv => kv.Value == 1)
                .Select(kv => kv.Key)
                .ToList();
            adjacency = adjacency2;
        }
    }
}
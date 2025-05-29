using System;
using System.Linq;
using Newtonsoft.Json.Linq;
//using Sirenix.Serialization;
using UnityEngine;
using UnityEditor.TerrainTools;
using static UnityEditor.Experimental.GraphView.GraphView;
using System.IO;
using UnityEditor;
using System.Collections.Generic;
using UnityEngine.AI;
using Unity.AI.Navigation;
using Unity.VisualScripting;
using System.Xml.Linq;


[Serializable]
public class BaseBakeComponent
{
    public string type;
    public string guid;
    //[OdinSerialize, HideInInspector]
    public Component target;

    public virtual void PrevProcessing()
    {
        
    }

    public virtual JObject BakeComponent()
    {
        JObject json = new JObject();
        json["type"] = type;
        json["guid"] = guid;
        return json;
    }

    public static BaseBakeComponent CreateProperty(Component component)
    {
        BaseBakeComponent property = null;
        if(component.GetType().Name == "Transform")
        {
            var transformProperty = new TransformProperty();

            property = transformProperty;
        }
        if (component.GetType().Name == "RectTransform")
        {
            var transformProperty = new UITransformProperty();

            property = transformProperty;
        }
        if (component.GetType().Name == "Camera")
        {
            var cameraProperty = new CameraProperty();

            property = cameraProperty;
        }
        if (component.GetType().Name == "Light")
        {
            var lightProperty = new LightProperty();

            property = lightProperty;
        }
        if (component.GetType().Name == "MeshRenderer")
        {
            var meshRendererProperty = new MeshRendererProperty();

            property = meshRendererProperty;
        }
        if (component.GetType().Name == "MeshFilter")
        {
            var meshFilterProperty = new MeshFilterProperty();

            property = meshFilterProperty;
        }
        if (component.GetType().Name == "SkinnedMeshRenderer")
        {
            var skinnedMeshRendererProperty = new SkinnedMeshRendererProperty();

            property = skinnedMeshRendererProperty;
        }

        if (component.GetType().Name == "BoxCollider")
        {
            var boxCollider = new BoxColliderProperty();

            property = boxCollider;
        }
        if (component.GetType().Name == "SphereCollider")
        {
            var sphereCollider = new CapualeColliderProperty();

            property = sphereCollider;
        }

        if (component.GetType().Name == "CapsuleCollider")
        {
            var capsuleCollider = new CapsuleColliderProperty();

            property = capsuleCollider;
        }

        if (component.GetType().Name == "MeshCollider")
        {
            var meshCollider = new MeshColliderProperty();

            property = meshCollider;
        }
        if (component.GetType().Name == "ModelLoader")
        {
            var modelLoader = new ModelLoaderProperty();

            property = modelLoader;
        }
        if (component.GetType().Name == "Terrain")
        {
            var terrain = new TerrainProperty();

            property = terrain;
        }
        if (component.GetType().Name == "Rigidbody")
        {
            var physics = new PhysicsProperty();

            property = physics;
        }
        if (component.GetType().Name == "Animator")
        {
            var prop = new AnimatorProperty();

            property = prop;
        }
        if (component.GetType().Name == "NavMeshSurface")
        {
            var prop = new NavMeshSurfaceProperty();
            
            property = prop;
        }
        if (component.GetType().Name == "Tags")
        {
            var prop = new TagsProperty();

            property = prop;
        }
        if (component.GetType().Name == "Scripts")
        {
            var prop = new ScriptsProperty();

            property = prop;
        }
        if (component.GetType().Name == "AnimationList")
        {
            var prop = new AnimationListProperty();

            property = prop;
        }
        if (component.GetType().Name == "TextureList")
        {
            var prop = new TextureListProperty();

            property = prop;
        }

        //Debug.Log(component.GetType().Name);

        if (property != null)
        {
            property.target = component;

            if (string.IsNullOrEmpty(property.type))
                property.type = component.GetType().Name;
            if(string.IsNullOrEmpty(property.guid))
                property.guid = BakeUnity.NewGuid();
        }
        return property;
    }
}

[Serializable]
public class TransformProperty : BaseBakeComponent
{
    public override JObject BakeComponent()
    {
        JObject json = base.BakeComponent();
        var trans = (Transform)target;

        json["position"] = BakeExtensions.ToJson(trans.localPosition);
        json["rotation"] = BakeExtensions.ToJson(trans.localRotation);
        json["scale"] = BakeExtensions.ToJson(trans.localScale);

        return json;
    }
}


[Serializable]
public class UITransformProperty : BaseBakeComponent
{
    public override JObject BakeComponent()
    {
        JObject json = base.BakeComponent();
        var trans = (RectTransform)target;


        json["position"] = BakeExtensions.ToJson(trans.localPosition);
        json["rotation"] = BakeExtensions.ToJson(trans.localRotation);
        json["scale"] = BakeExtensions.ToJson(trans.localScale);
        json["pivot"] = BakeExtensions.ToJson(trans.pivot);
        json["anchorMax"] = BakeExtensions.ToJson(trans.anchorMax);
        json["anchorMin"] = BakeExtensions.ToJson(trans.anchorMin);
        json["anchoredPosition"] = BakeExtensions.ToJson(trans.anchoredPosition);
        json["sizeDelta"] = BakeExtensions.ToJson(trans.sizeDelta);
        json["offsetMin"] = BakeExtensions.ToJson(trans.offsetMin);
        json["offsetMax"] = BakeExtensions.ToJson(trans.offsetMax);
        json["rectSize"] = BakeExtensions.ToJson(new Vector2(trans.rect.width, trans.rect.height));

        return json;
    }
}

[Serializable]
public class MeshRendererProperty : BaseBakeComponent
{
    public override void PrevProcessing()
    {
        base.PrevProcessing();
        var obj = (MeshRenderer)target;
        var materialList = obj.sharedMaterials.ToList();
        foreach (var material in materialList)
            if (!BakeUnity.refList_Material.Contains(material))
                BakeUnity.refList_Material.Add(material);
    }

    public override JObject BakeComponent()
    {
        JObject json = base.BakeComponent();

        var obj = (MeshRenderer)target;
        var materialList = obj.sharedMaterials.ToList();
        var materials = new JArray();
        json.Add("mesh", BakeExtensions.ToJson(obj.gameObject.GetComponent<MeshFilter>().sharedMesh));
        json.Add("shadowCast", obj.shadowCastingMode.ToString());

        json.Add("materials", materials);
        foreach (var material in materialList){
            if(BakeUnity.TryGetGuid(material, out var guid))
                materials.Add(guid);
        }

        return json;
    }
}

[Serializable]
public class SkinnedMeshRendererProperty : BaseBakeComponent
{
    public override void PrevProcessing()
    {
        base.PrevProcessing();
        var obj = (SkinnedMeshRenderer)target;
        var materialList = obj.sharedMaterials.ToList();
        foreach (var material in materialList)
            if(!BakeUnity.refList_Material.Contains(material))
                BakeUnity.refList_Material.Add(material);
    }
    public override JObject BakeComponent()
    {
        JObject json = base.BakeComponent();
        

        var obj = (SkinnedMeshRenderer)target;
        var materialList = obj.sharedMaterials.ToList();
        var materials = new JArray();

        json.Add("shadowCast", obj.shadowCastingMode.ToString());

        json.Add("mesh", BakeExtensions.ToJson(obj.sharedMesh));
        json.Add("boneRoot", obj.rootBone.gameObject.name);

        json.Add("materials", materials);
        foreach (var material in materialList) {
            if (BakeUnity.TryGetGuid(material, out var guid))
                materials.Add(guid);
        }

        JObject blendShapes = new JObject();
        json.Add("blendShapeCount", obj.sharedMesh.blendShapeCount);
        json.Add("blendShapes", blendShapes);
        for (int i = 0; i < obj.sharedMesh.blendShapeCount; i++) {
            if (!blendShapes.ContainsKey(obj.sharedMesh.GetBlendShapeName(i)))
                blendShapes.Add(obj.sharedMesh.GetBlendShapeName(i), obj.GetBlendShapeWeight(i));
        }
        return json;
    }
}

[Serializable]
public class MeshFilterProperty : BaseBakeComponent
{
    public override JObject BakeComponent()
    {
        JObject json = base.BakeComponent();
        var obj = (MeshFilter)target;
        json.Add("mesh", BakeExtensions.ToJson(obj.sharedMesh));

        return json;
    }
}

[Serializable]
public class ModelLoaderProperty : BaseBakeComponent
{
    public override JObject BakeComponent()
    {
        JObject json = base.BakeComponent();
        var obj = (ModelLoader)target;
        json.Add("modelKey", BakeExtensions.ToJson(obj.modelKey));

        return json;
    }
}
[Serializable]
public class TerrainProperty : BaseBakeComponent
{
    public override void PrevProcessing()
    {
        base.PrevProcessing();
        var obj = (Terrain)target;

        TreePrototype[] treePrototypes = obj.terrainData.treePrototypes;
        //obj.terrainData.treeInstanceCount
        for (int i = 0; i < treePrototypes.Length; i++)
        {
            var proto = treePrototypes[i];
            if (proto != null && proto.prefab != null)
            {
                BakeUnity.PrevProcessingPrefab(proto.prefab);
            }
        }

        //if (!BakeUnity.gameObjectToBakeTable.TryGetValue(gameObject, out var bakingInfo))
        //{
        //    bakingInfo = new BakeGameObject(gameObject);
        //    BakeUnity.gameObjectToBakeTable.Add(gameObject, bakingInfo);
        //}

        //BakeUnity.SetGuidAndUpdate(gameObject, bakingInfo.guid);
    }
    public override JObject BakeComponent()
    {
        JObject json = base.BakeComponent();
        var obj = (Terrain)target;

        var nameKey = obj.terrainData.name;


        JArray layerBlendArray = new JArray();
        for (int i = 0; i < obj.terrainData.alphamapTextureCount; i++)
        {
            var texture = obj.terrainData.GetAlphamapTexture(i);
            byte[] bytes = texture.EncodeToPNG();
            string fileName = $"{nameKey}_SplatTexture_{i}.png";
            // 프로젝트 Assets 폴더 내에 저장
            var folderPath = Path.Combine("Assets/", "Models/Terrains/Textures/");
            string path = Path.Combine(folderPath, fileName);
            if(!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);
            File.WriteAllBytes(path, bytes);
            BakeUnity.filePathSet.Add(path);

            layerBlendArray.Add(BakeExtensions.PathConvert(path));
        }

        TerrainData terrainData = obj.terrainData;
        int resolution = terrainData.heightmapResolution - 1; // 높이맵 해상도 (예: 513)
        int baseResolution = resolution;
        float[,] heights = terrainData.GetHeights(0, 0, resolution, resolution);

        // 16비트 정수 2바이트씩 사용하므로 전체 바이트 배열 크기 계산
        byte[] rawData = new byte[resolution * resolution * 2];
        int index = 0;

        // 높이 데이터를 순회하며 16비트 값으로 변환 후 rawData에 저장
        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                // 높이 값은 0~1 범위이므로 65535(=0xFFFF)를 곱하여 16비트 정수로 변환
                ushort heightValue = (ushort)Mathf.RoundToInt(heights[y, x] * 65535);

                // little-endian 형식으로 저장 (하위 바이트 먼저 기록)
                rawData[index++] = (byte)(heightValue & 0xFF);         // 하위 바이트
                rawData[index++] = (byte)((heightValue >> 8) & 0xFF);    // 상위 바이트
            }
        }
        string rawPath;
        string pngPath;

        {
            var folderPath = Path.Combine("Assets/", "Models/Terrains/Heights/");
            string fileName = $"{nameKey}_Height.raw";
            rawPath = Path.Combine(folderPath, fileName);
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);
            File.WriteAllBytes(rawPath, rawData);
            BakeUnity.filePathSet.Add(rawPath);
        }
        {
            Texture2D texture = new Texture2D(baseResolution, baseResolution, TextureFormat.R16, false);//heights
            texture.Apply();
            for (int y = 0; y < baseResolution; y++)
                for (int x = 0; x < baseResolution; x++)
                {
                    texture.SetPixel(x, y, Color.red * heights[y, x]);
                }
            texture.Apply();
            var folderPath = Path.Combine("Assets/", "Models/Terrains/Heights/");
            string fileName = $"{nameKey}_Height.png";
            pngPath = Path.Combine(folderPath, fileName);
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);
            File.WriteAllBytes(pngPath, texture.EncodeToPNG());
            BakeUnity.filePathSet.Add(pngPath);
        }

        JArray prefabArray = new JArray();
        TreePrototype[] treePrototypes = obj.terrainData.treePrototypes;
        //obj.terrainData.treeInstanceCount
        for (int i = 0; i < treePrototypes.Length; i++)
        {
            var proto = treePrototypes[i];
            string guid = "";
            if (proto != null && proto.prefab != null)
            {
                BakeUnity.TryGetGuid(proto.prefab, out guid);
            }
            prefabArray.Add(guid);
        }

        JArray instanceArray = new JArray();
        TreeInstance[] treeInstances = obj.terrainData.treeInstances;
        foreach (TreeInstance tree in treeInstances)
        {
            JObject trans = new JObject();
            trans.Add("index", tree.prototypeIndex);
            trans.Add("position", BakeExtensions.ToJson(Vector3.Scale(tree.position, obj.terrainData.size) + obj.transform.position));
            trans.Add("rotation", tree.rotation);
            trans.Add("scale", BakeExtensions.ToJson(new Vector3(tree.widthScale, tree.heightScale, tree.widthScale)));
            //Vector3 worldPos = Vector3.Scale(tree.position, terrain.terrainData.size) + terrain.transform.position;
            instanceArray.Add(trans);
        }

        TerrainLayer[] layers = terrainData.terrainLayers;

        JArray layerArray = new JArray();
        for (int i = 0; i < layers.Length; i++)
        {
            JObject layerData = new JObject();
            TerrainLayer layer = layers[i];
            // TerrainLayer에서 기본 텍스처(보통 diffuseTexture)를 가져옵니다.
            layerData.Add("diffuse", BakeExtensions.ToJson(layer.diffuseTexture));
            if(layer.normalMapTexture != null)
                layerData.Add("normal", BakeExtensions.ToJson(layer.normalMapTexture));
            if (layer.maskMapTexture != null)
                layerData.Add("mask", BakeExtensions.ToJson(layer.maskMapTexture));
            layerData.Add("tileSize", BakeExtensions.ToJson(layer.tileSize));
            layerData.Add("tileOffset", BakeExtensions.ToJson(layer.tileOffset));
            layerArray.Add(layerData);
        }

        //TreePrototype[] treePrototypes = terrainData.treePrototypes;
        //for (int i = 0; i < terrainData.treeInstanceCount; i++)
        //{
        //    terrainData.treeInstances[i].
        //}

        AssetDatabase.Refresh();



        json.Add("name", nameKey);
        json.Add("size", BakeExtensions.ToJson(obj.terrainData.size));
        json.Add("heightmapResolution", resolution);
        json.Add("alphamapResolution", obj.terrainData.alphamapResolution);
        json.Add("alphamapTextureCount", obj.terrainData.alphamapTextureCount);
        json.Add("rawPath", BakeExtensions.PathConvert(rawPath));
        json.Add("pngPath", BakeExtensions.PathConvert(pngPath));
        json.Add("layerBlendTextures", layerBlendArray);
        json.Add("layers", layerArray);
        json.Add("instanceCount", obj.terrainData.treeInstanceCount);
        json.Add("instanceTable", prefabArray);
        json.Add("instanceDatas", instanceArray);

        return json;
    }
}

[Serializable]
public class LightProperty : BaseBakeComponent
{
    public override JObject BakeComponent()
    {
        JObject json = base.BakeComponent();
        var obj = (Light)target;
        json.Add("lightType", obj.type.ToString());
        json.Add("color", BakeExtensions.ToJson(obj.color));
        json.Add("intensity", obj.intensity);
        json.Add("range", obj.range);
        json.Add("innerSpotAngle", obj.innerSpotAngle);
        json.Add("spotAngle", obj.spotAngle);
        json.Add("shadowAngle", obj.shadowAngle);
        return json;
    }
}


[Serializable]
public class CameraProperty : BaseBakeComponent
{
    public override JObject BakeComponent()
    {
        JObject json = base.BakeComponent();
        var obj = (Camera)target;
        json.Add("isOrtho", obj.orthographic);
        json.Add("orthoSize", obj.orthographicSize);
        json.Add("near", obj.nearClipPlane);
        json.Add("far", obj.farClipPlane);
        json.Add("fovy", obj.fieldOfView);

        return json;
    }
}


[Serializable]
public class ColliderProperty : BaseBakeComponent
{
    public ColliderType colliderType;

    public override JObject BakeComponent()
    {
        JObject json = base.BakeComponent();
        var obj = (Collider)target;
        json.Add("aabbCenter", BakeExtensions.ToJson(obj.bounds.center));
        json.Add("aabbExtent", BakeExtensions.ToJson(obj.bounds.extents));

        json.Add("isTrigger", obj.isTrigger);

        return json;
    }
}

[Serializable]
public class BoxColliderProperty : ColliderProperty
{

    public override JObject BakeComponent()
    {
        JObject json = base.BakeComponent();
        var obj = (BoxCollider)target;
        json.Add("colliderType", "box");
        json.Add("center", BakeExtensions.ToJson(obj.center));
        json.Add("size", BakeExtensions.ToJson(obj.size)); 

        return json;
    }
}

[Serializable]
public class CapualeColliderProperty : ColliderProperty
{

    public override JObject BakeComponent()
    {
        JObject json = base.BakeComponent();
        var obj = (SphereCollider)target;
        //json\
        json.Add("colliderType", "sphere");
        json.Add("center", BakeExtensions.ToJson(obj.center));
        json.Add("radius", obj.radius);

        return json;
    }
}
[Serializable]
public class CapsuleColliderProperty : ColliderProperty
{

    public override JObject BakeComponent()
    {
        JObject json = base.BakeComponent();
        var obj = (CapsuleCollider)target;
        //json\
        json.Add("colliderType", "capsule");
        json.Add("center", BakeExtensions.ToJson(obj.center));
        json.Add("radius", obj.radius);
        json.Add("height", obj.height);

        return json;
    }
}


[Serializable]
public class MeshColliderProperty : ColliderProperty
{

    public override JObject BakeComponent()
    {
        JObject json = base.BakeComponent();
        var obj = (MeshCollider)target;
        //json
        json.Add("colliderType", "mesh");
        json.Add("convex", obj.convex);
        if(obj.sharedMesh != null && (!obj.sharedMesh.IsUnityNull()))
            json.Add("mesh", BakeExtensions.ToJson(obj.sharedMesh));

        return json;
    }
}

[Serializable]
public class PhysicsProperty : BaseBakeComponent
{

    public override JObject BakeComponent()
    {
        JObject json = base.BakeComponent();
        //var obj = (MeshCollider)target;
        ////json
        //json.Add("colliderType", "mesh");
        //json.Add("convex", obj.convex);
        //if (obj.sharedMesh != null)
        //    json.Add("mesh", BakeExtensions.ToJson(obj.sharedMesh));

        return json;
    }
}

[Serializable]
public class AnimatorProperty : BaseBakeComponent
{

    public override JObject BakeComponent()
    {
        JObject json = base.BakeComponent();
        var obj = (Animator)target;

        Dictionary<string, Transform> boneMapping = new Dictionary<string, Transform>();
        JObject json2 = new JObject();
        if ((!obj.avatar.IsUnityNull()) && obj.avatar != null)
        {
            if (obj.avatar.isHuman)
            {
                foreach (HumanBodyBones humanBone in System.Enum.GetValues(typeof(HumanBodyBones)))
                {
                    if (humanBone == HumanBodyBones.LastBone)
                        continue;

                    Transform boneTransform = obj.GetBoneTransform(humanBone);
                    if (boneTransform != null)
                    {
                        // 예: key = "Hips", value = 실제 본의 Transform
                        boneMapping.Add(humanBone.ToString(), boneTransform);
                        json2.Add(humanBone.ToString(), boneTransform.name);
                    }
                }
            }
        }
        json.Add("boneMapping", json2);

        ////json
        //json.Add("colliderType", "mesh");
        //json.Add("convex", obj.convex);
        //if (obj.sharedMesh != null)
        //    json.Add("mesh", BakeExtensions.ToJson(obj.sharedMesh));

        return json;
    }
}


public struct Edge
{
    public int v1, v2;
    public Edge(int a, int b)
    {
        if (a < b) { v1 = a; v2 = b; }
        else { v1 = b; v2 = a; }
    }
    public override bool Equals(object obj)
    {
        if (!(obj is Edge)) return false;
        var e = (Edge)obj;
        return v1 == e.v1 && v2 == e.v2;
    }
    public override int GetHashCode()
    {
        unchecked { return v1 * 397 ^ v2; }
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
public class NavMeshSurfaceProperty : BaseBakeComponent
{

    public override JObject BakeComponent()
    {
        JObject json = base.BakeComponent();
        var obj = (NavMeshSurface)target;

        //NavMeshTriangulation navMeshData = NavMesh.CalculateTriangulation();

        BuildGraph( out var verts, out var adjacency, out var boundaryEdges, out var tri);
        JArray json2 = new JArray();
        for (int i = 0; i < verts.Count; i++) {
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
        json.Add("adjacency", json3);

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
        json.Add("tris", json7);


        JArray json5 = new JArray();
        for (int i = 0; i < boundaryEdges.Count; i++)
        {
            JArray json6 = new JArray();
            json6.Add(BakeExtensions.ToJson(verts[boundaryEdges[i].v1]));
            json6.Add(BakeExtensions.ToJson(verts[boundaryEdges[i].v2]));
            json5.Add(json6);
        }
        json.Add("edge", json5);
        //navMeshData.
        //obj.

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

[Serializable]
public class TagsProperty : BaseBakeComponent
{

    public override JObject BakeComponent()
    {
        JObject json = base.BakeComponent();
        var obj = (Tags)target;

        JArray json2 = new JArray();
        foreach (var a in obj.tagList)
            json2.Add(a.ToString());
        json.Add("tags", json2);
        return json;
    }
}


[Serializable]
public class ScriptsProperty : BaseBakeComponent
{

    public override JObject BakeComponent()
    {
        JObject json = base.BakeComponent();
        var obj = (Scripts)target;

        JArray json2 = new JArray();
        foreach (var a in obj.scripts)
            json2.Add(a.ToString());
        json.Add("scripts", json2);

        return json;
    }
}


[Serializable]
public class AnimationListProperty : BaseBakeComponent
{

    public override JObject BakeComponent()
    {
        JObject json = base.BakeComponent();
        var obj = (AnimationList)target;

        JObject json2 = new JObject();

        foreach ((string key, AnimationClip clip) in obj.animations)
        {
            var filePath = AssetDatabase.GetAssetPath(clip);
            string fileName = Path.GetFileNameWithoutExtension(filePath);
            string finalName = fileName + "|" + AnimationList.ExtractTakeName(clip);
            BakeFBXModel.modelPathList.Add(filePath);
            json2.Add(key, finalName);
        }

        json.Add("animationKeys", json2);

        return json;
    }
}



[Serializable]
public class TextureListProperty : BaseBakeComponent
{

    public override JObject BakeComponent()
    {
        JObject json = base.BakeComponent();
        var obj = (TextureList)target;

        JObject json2 = new JObject();

        foreach ((string key, CList clip) in obj.textureTable)
        {
            JArray json3 = new JArray();
            //clip.textures
            foreach (var texture in clip.textures)
            {
                var filePath = AssetDatabase.GetAssetPath(texture);
                string fileName = Path.GetFileNameWithoutExtension(filePath);

                BakeUnity.filePathSet.Add(filePath);
                json3.Add(filePath);
            }
            json2.Add(key, json3);
        }

        json.Add("textureTable", json2);

        return json;
    }
}


public enum ColliderType
{
    box,
    sphere,
    capsule,
    mesh
}
// ExportTerrainToMesh.cs – Unity 2022+ FBX Exporter
// -----------------------------------------------------------------------------
// 2024‑06‑06  (v3)
//   • Fix OBJ export left‑hand ⇄ right‑hand mismatch (mirrored X).
//        - When writing OBJ vertices:  X → -X  (switch to right‑hand)
//        - Swap triangle winding (a,c,b) to keep front‑face CCW in OBJ.
// -----------------------------------------------------------------------------

#if UNITY_EDITOR
using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.Formats.Fbx.Exporter;
using UnityEngine;

namespace Clrain.SceneToJson.Expansions
{
    internal enum SaveResolution { Full, Half, Quarter, Eighth, Sixteenth }
    internal enum MeshFormat { FBX, OBJ }

    internal sealed class ExportTerrainToMesh : EditorWindow
    {
        private SaveResolution _resolution = SaveResolution.Half;
        private MeshFormat _format = MeshFormat.FBX;
        private bool _exportHeightmap;
        private bool _pivotAtWorldOrigin;

        private TerrainData _terrain;
        private Vector3 _terrainPos;

        [MenuItem("Tools/Json Exporter/Terrain Expansions/Export Terrain Mesh…")]
        private static void ShowWindow()
        {
            var wnd = GetWindow<ExportTerrainToMesh>(utility: true, title: "Terrain Mesh Export");
            wnd.minSize = new Vector2(340, 160);
            wnd.TryFindTerrain();
        }

        private void TryFindTerrain()
        {
            _terrain = null;
            var t = Selection.activeObject as Terrain ?? Terrain.activeTerrain;
            if (t)
            {
                _terrain = t.terrainData;
                _terrainPos = t.transform.position;
            }
        }

        /* ---------------- IMGUI ---------------- */
        private void OnGUI()
        {
            if (_terrain == null)
            {
                EditorGUILayout.HelpBox("No terrain selected or found in scene.", MessageType.Warning);
                if (GUILayout.Button("Refresh")) TryFindTerrain();
                return;
            }

            _resolution = (SaveResolution)EditorGUILayout.EnumPopup("Resolution", _resolution);
            _format = (MeshFormat)EditorGUILayout.EnumPopup("Mesh Format", _format);
            _exportHeightmap = EditorGUILayout.Toggle("Export Heightmap (PNG R16)", _exportHeightmap);
            _pivotAtWorldOrigin = EditorGUILayout.Toggle("Center Pivot at World Origin", _pivotAtWorldOrigin);

            GUILayout.FlexibleSpace();
            using (new EditorGUI.DisabledScope(_terrain == null))
            {
                if (GUILayout.Button(_format == MeshFormat.FBX ? "Export FBX" : "Export OBJ", GUILayout.Height(32)))
                    Export();
            }
        }

        /* ---------------- Export Logic ---------------- */
        private void Export()
        {
            string ext = _format == MeshFormat.FBX ? "fbx" : "obj";
            string path = EditorUtility.SaveFilePanel("Save Mesh", "", "Terrain", ext);
            if (string.IsNullOrEmpty(path)) return;

            try
            {
                Mesh mesh = BuildTerrainMesh(out _, out _); if (mesh == null) return;

                if (_format == MeshFormat.FBX) WriteFbx(path, mesh);
                else WriteObj(path, mesh);

                if (_exportHeightmap) WriteHeightmapPng(Path.ChangeExtension(path, "png"));
            }
            catch (Exception ex)
            {
                Debug.LogError($"[TerrainExport] {ex.Message}");
            }
            finally { EditorUtility.ClearProgressBar(); }

            AssetDatabase.Refresh();
        }

        /* ---------- FBX ---------- */
        private void WriteFbx(string outPath, Mesh mesh)
        {
            var go = new GameObject("_TempTerrainMesh");
            var mf = go.AddComponent<MeshFilter>();
            var mr = go.AddComponent<MeshRenderer>();
            mf.sharedMesh = mesh;
            mr.sharedMaterial = new Material(Shader.Find("Standard"));
            go.transform.position = _pivotAtWorldOrigin ? Vector3.zero : _terrainPos;

            var opt = new ExportModelOptions
            {
                UseMayaCompatibleNames = true,
                PreserveImportSettings = true
            };
            EditorUtility.DisplayProgressBar("Export FBX", "Writing file…", 1f);
            ModelExporter.ExportObject(outPath, go, opt);
            DestroyImmediate(go);
        }

        /* ---------- OBJ (Right‑handed) ---------- */
        private void WriteObj(string outPath, Mesh mesh)
        {
            EditorUtility.DisplayProgressBar("Export OBJ", "Writing file…", 0f);
            using var sw = new StreamWriter(outPath, false, Encoding.ASCII);
            System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
            sw.WriteLine("# Unity terrain OBJ export");

            Vector3 off = _pivotAtWorldOrigin ? -_terrainPos : Vector3.zero;

            // ► Vertices: flip X to convert Unity (left‑hand) → OBJ (right‑hand)
            foreach (var v in mesh.vertices)
                sw.WriteLine($"v {-v.x + off.x} {v.y + off.y} {v.z + off.z}");

            foreach (var uv in mesh.uv)
                sw.WriteLine($"vt {uv.x} {uv.y}");

            int[] tris = mesh.triangles;
            for (int i = 0; i < tris.Length; i += 3)
            {
                // Swap order to keep face orientation (CCW)
                int a = tris[i] + 1;
                int b = tris[i + 1] + 1;
                int c = tris[i + 2] + 1;
                sw.WriteLine($"f {a}/{a} {c}/{c} {b}/{b}");
            }
        }

        /* ---------- Heightmap PNG (R16) ---------- */
        private void WriteHeightmapPng(string pngPath)
        {
            int res = _terrain.heightmapResolution;
            float[,] h = _terrain.GetHeights(0, 0, res, res);
            var tex = new Texture2D(res, res, TextureFormat.R16, false, true) { wrapMode = TextureWrapMode.Clamp };

            ushort[] buf = new ushort[res * res];
            for (int y = 0; y < res; y++)
                for (int x = 0; x < res; x++)
                    buf[y * res + x] = (ushort)Mathf.Clamp(Mathf.RoundToInt(h[y, x] * 65535f), 0, 65535);

            tex.SetPixelData(buf, 0);
            tex.Apply();
            File.WriteAllBytes(pngPath, tex.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(tex);
        }

        /* ---------------- Mesh Builder (unchanged) ---------------- */
        private Mesh BuildTerrainMesh(out int vCount, out int idxCount)
        {
            int hm = _terrain.heightmapResolution;
            int step = 1 << (int)_resolution;
            int w = (hm - 1) / step + 1;
            int h = (hm - 1) / step + 1;

            var verts = new Vector3[w * h];
            var uvs = new Vector2[w * h];
            var tris = new int[(w - 1) * (h - 1) * 6];

            Vector3 scale = new(
                _terrain.size.x / (hm - 1) * step,
                _terrain.size.y,
                _terrain.size.z / (hm - 1) * step);

            float[,] H = _terrain.GetHeights(0, 0, hm, hm);

            for (int y = 0, vi = 0; y < h; y++)
                for (int x = 0; x < w; x++, vi++)
                {
                    int hx = x * step;
                    int hy = y * step;
                    verts[vi] = Vector3.Scale(scale, new Vector3(x, H[hy, hx], y));
                    uvs[vi] = new Vector2(hx / (float)(hm - 1), hy / (float)(hm - 1));
                }

            int ti = 0;
            for (int y = 0; y < h - 1; y++)
                for (int x = 0; x < w - 1; x++)
                {
                    int bl = y * w + x;
                    int tl = bl + w;
                    int br = bl + 1;
                    int tr = tl + 1;
                    tris[ti++] = bl; tris[ti++] = tl; tris[ti++] = br;
                    tris[ti++] = tl; tris[ti++] = tr; tris[ti++] = br;
                }

            var m = new Mesh { name = $"TerrainMesh_{w}x{h}" };
            if (verts.Length > 65000) m.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            m.vertices = verts; m.uv = uvs; m.triangles = tris;
            m.RecalculateNormals(); m.RecalculateBounds();

            if (!_pivotAtWorldOrigin)
            {
                for (int i = 0; i < verts.Length; i++) verts[i] += _terrainPos;
                m.vertices = verts;
            }

            vCount = verts.Length; idxCount = tris.Length; return m;
        }
    }
}
#endif

#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Editor utility for creating flat regular polygon mesh assets (equivalent to the built-in Quad,
/// but with N sides) in the currently selected project folder.
///
/// Assets > Create > Mesh > Regular Polygon... opens a window to choose the side count and size.
/// Shortcut menu items are also provided for common shapes.
///
/// Layout matches Unity's Quad: centred on the origin, lies in the XY plane, faces -Z.
/// By default the apothem is 0.5, so flat edges touch the +/-0.5 unit square where they line up
/// with the axes. The bottom edge is always flat. UVs are mapped so the polygon fills 0..1 on its
/// widest axis without distortion.
/// </summary>
public class MeshCreator : EditorWindow
{
    public enum RadiusMode
    {
        Apothem,      // centre to middle of an edge
        Circumradius, // centre to a vertex
    }

    const int MinSides = 3;
    const int MaxSides = 256;

    int _sides = 8;
    RadiusMode _radiusMode = RadiusMode.Apothem;
    float _radius = 0.5f;

    [MenuItem("Assets/Create/Mesh/Regular Polygon...")]
    static void OpenWindow()
    {
        var window = GetWindow<MeshCreator>(true, "Regular Polygon Mesh");
        window.minSize = new Vector2(280f, 110f);
        window.ShowUtility();
    }

    [MenuItem("Assets/Create/Mesh/Triangle")] static void CreateTriangle() => CreateAsset(3);
    [MenuItem("Assets/Create/Mesh/Pentagon")] static void CreatePentagon() => CreateAsset(5);
    [MenuItem("Assets/Create/Mesh/Hexagon")] static void CreateHexagon() => CreateAsset(6);
    [MenuItem("Assets/Create/Mesh/Octagon")] static void CreateOctagon() => CreateAsset(8);

    void OnGUI()
    {
        _sides = EditorGUILayout.IntSlider("Sides", _sides, MinSides, MaxSides);
        _radiusMode = (RadiusMode)EditorGUILayout.EnumPopup("Radius Mode", _radiusMode);
        _radius = Mathf.Max(0.001f, EditorGUILayout.FloatField("Radius", _radius));

        EditorGUILayout.Space();
        if (GUILayout.Button("Create"))
        {
            CreateAsset(_sides, _radiusMode, _radius);
        }
    }

    static void CreateAsset(int sides, RadiusMode radiusMode = RadiusMode.Apothem, float radius = 0.5f)
    {
        string name = GetPolygonName(sides);
        string folder = GetSelectedFolder();
        string path = AssetDatabase.GenerateUniqueAssetPath(Path.Combine(folder, name + ".asset"));

        Mesh mesh = BuildMesh(sides, radiusMode, radius);
        mesh.name = name;
        AssetDatabase.CreateAsset(mesh, path);
        AssetDatabase.SaveAssets();

        EditorUtility.FocusProjectWindow();
        Selection.activeObject = mesh;
    }

    static string GetSelectedFolder()
    {
        string path = AssetDatabase.GetAssetPath(Selection.activeObject);
        if (string.IsNullOrEmpty(path)) return "Assets";
        if (!AssetDatabase.IsValidFolder(path)) path = Path.GetDirectoryName(path);
        return path.Replace('\\', '/');
    }

    static string GetPolygonName(int sides) => sides switch
    {
        3 => "Triangle",
        4 => "Square",
        5 => "Pentagon",
        6 => "Hexagon",
        7 => "Heptagon",
        8 => "Octagon",
        9 => "Nonagon",
        10 => "Decagon",
        12 => "Dodecagon",
        _ => $"Polygon{sides}",
    };

    public static Mesh BuildMesh(int sides, RadiusMode radiusMode = RadiusMode.Apothem, float radius = 0.5f)
    {
        sides = Mathf.Clamp(sides, MinSides, MaxSides);

        float circumradius = radiusMode == RadiusMode.Apothem
            ? radius / Mathf.Cos(Mathf.PI / sides)
            : radius;

        // Start half a step past straight down so the bottom edge is flat.
        float step = 2f * Mathf.PI / sides;
        float startAngle = -0.5f * Mathf.PI + 0.5f * step;

        var vertices = new Vector3[sides + 1];
        var normals = new Vector3[sides + 1];
        var uvs = new Vector2[sides + 1];
        var triangles = new int[sides * 3];

        // Centre vertex
        vertices[0] = Vector3.zero;

        // Rim vertices
        float maxExtent = 0f;
        for (int i = 0; i < sides; i++)
        {
            float a = startAngle + i * step;
            var p = new Vector3(Mathf.Cos(a) * circumradius, Mathf.Sin(a) * circumradius, 0f);
            vertices[i + 1] = p;
            maxExtent = Mathf.Max(maxExtent, Mathf.Abs(p.x), Mathf.Abs(p.y));
        }

        // UVs centred on the origin, scaled so the widest axis spans 0..1
        float uvScale = 0.5f / maxExtent;
        for (int i = 0; i <= sides; i++)
        {
            normals[i] = Vector3.back;
            uvs[i] = new Vector2(vertices[i].x * uvScale + 0.5f, vertices[i].y * uvScale + 0.5f);
        }

        // Triangle fan. Rim vertices run anticlockwise, so wind each triangle in reverse to be
        // clockwise when viewed from -Z, matching the Quad's front face.
        for (int i = 0; i < sides; i++)
        {
            int t = i * 3;
            triangles[t] = 0;
            triangles[t + 1] = (i + 1) % sides + 1;
            triangles[t + 2] = i + 1;
        }

        var mesh = new Mesh { name = GetPolygonName(sides) };
        mesh.vertices = vertices;
        mesh.normals = normals;
        mesh.uv = uvs;
        mesh.triangles = triangles;
        mesh.RecalculateBounds();
        mesh.RecalculateTangents();
        return mesh;
    }
}
#endif

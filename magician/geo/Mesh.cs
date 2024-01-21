namespace Magician.Geo;

public class Mesh
{
    List<int[]> faces;
    public List<int[]> Faces => faces;
    public Mesh(int spacing, params int[] idcs)
    {
        if (idcs.Length % spacing != 0)
            Scribe.Error($"Indivisible Mesh! {idcs.Length} is not divisible by {spacing}");
        if (idcs.Length / spacing < 3)
            Scribe.Error($"Not enough points in mesh! Need {3 * spacing - idcs.Length} more");
        
        faces = new();
        List<int> currentFace = new();
        for (int i = 0; i < idcs.Length; i++)
        {
            currentFace.Add(idcs[i]);
            if (i % spacing == spacing - 1)
            {
                faces.Add(currentFace.ToArray());
                currentFace = new List<int>();
            }
        }
    }
    public Mesh(List<int[]> fs)
    {
        faces = new();
        faces.AddRange(fs);
    }

    public readonly static Mesh Cubic = new(4,
        0, 1, 2, 3,
        4, 5, 6, 7,
        0, 4, 5, 1,
        3, 7, 6, 2,
        1, 5, 6, 2,
        0, 4, 7, 3
    );
    public readonly static Mesh SimplePyramid = new(3,
            0, 1, 2,
            0, 1, 3,
            1, 2, 3,
            0, 2, 3
    );
    public static Mesh Square(int w, int area)
    {
        List<int[]> faces = new();
        for (int n = 0; n < area; n++)
        {
            bool edgeRow = false;
            bool edgeCol = false;
            if (n % w >= w - 1)
            {
                edgeCol = true;
            }
            if (n >= area - w)
            {
                edgeRow = true;
            }
            if (!edgeCol && !edgeRow && n + w + 1 < area)
            {
                faces.Add(new int[] { w + n + 1, w + n, n, n + 1 });
            }
        }
        return new(faces);
    }
    public static Mesh Rect(int w, int h) => Square(w, w*h);
}
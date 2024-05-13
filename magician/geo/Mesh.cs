namespace Magician.Geo;

// TODO make Mesh a struct
public class Mesh
{
    readonly List<int[]> faces;
    public List<int[]> Faces => faces;

    public Mesh(List<int[]> fs)
    {
        faces = new();
        faces.AddRange(fs);
    }
    // Create a mesh from a flat list of indices, and the num of points per face
    public Mesh(int pointsPerFace, params int[] idcs)
    {
        if (idcs.Length % pointsPerFace != 0)
            Scribe.Error($"Indivisible Mesh! {idcs.Length} is not divisible by {pointsPerFace}");
        if (idcs.Length / pointsPerFace < 3)
            Scribe.Error($"Not enough points in mesh! Need {3 * pointsPerFace - idcs.Length} more");

        faces = new();
        List<int> currentFace = new();
        for (int i = 0; i < idcs.Length; i++)
        {
            currentFace.Add(idcs[i]);
            if (i % pointsPerFace == pointsPerFace - 1)
            {
                faces.Add(currentFace.ToArray());
                currentFace = new List<int>();
            }
        }
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
    public static Mesh Jagged(List<(int, int)> idcs, Region[,] map, int offset = 0)
    {
        List<int[]> faces = new();
        int height = map.GetLength(0);
        int width = map.GetLength(1);
        foreach ((int y, int x) idx in idcs)
        {
            // detect edges
            if (idx.x + 1 >= width)
                continue;
            if (idx.y + 1 >= height)
                continue;

            // mesh if local region matches nearby
            Region local = map[idx.y, idx.x];
            if (local == Region.BORDER)
                continue;
            Region east =      map[idx.y,   idx.x+1];
            Region south =     map[idx.y+1, idx.x];
            Region southEast = map[idx.y+1, idx.x+1];
            if (east != local || south != local || southEast != local)
                continue;
            int localIdx =     idx.y*width + idx.x;
            int eastIdx =      idx.y*width + idx.x + 1;
            int southIdx =     (idx.y+1)*width + idx.x;
            int southEastIdx = (idx.y+1)*width + idx.x + 1;
            faces.Add(new int[]{localIdx+offset, southIdx+offset, southEastIdx+offset, eastIdx+offset});
        }
        return new(faces);
    }
    public static Mesh Rect(int w, int area, int offset = 0)
    {
        List<int[]> faces = new();
        for (int n = 0; n < area; n++)
        {
            if (n % w >= w - 1)
                continue;
            if (n >= area - w)
                continue;
            if (n + w + 1 < area)
                faces.Add(new int[] { w + n + 1 + offset, w + n + offset, n + offset, n + 1 + offset });
        }
        return new(faces);
    }

    public enum Region
    {
        BORDER = -1,
        UNEXPLORED = 0
    }

    static void FloodFill(List<Region> regions, int width, int x0, int y0, int paint)
    {
        int height = regions.Count / width;
        Stack<(int, int)> positions = new();
        positions.Push((x0, y0));

        while (positions.Count > 0)
        {
            (int x, int y) = positions.Pop();
            int idx = Math.Abs(y * width + x);
            if (x < 0 || y < 0 || x > width - 1 || y > height - 1 || regions[idx] != Region.UNEXPLORED)
                continue;
            regions[idx] = (Region)paint;
            positions.Push((x + 1, y));
            positions.Push((x - 1, y));
            positions.Push((x, y + 1));
            positions.Push((x, y - 1));
        }
    }
    // Use flood fill to divide the world into regions
    public static int DivideRegions(List<Region> world, int width)
    {
        int row = -1;
        int numRegions = 0;
        for (int i = 0; i < world.Count; i++)
        {
            if (i % width == 0)
                row++;

            if (world[i] == Region.UNEXPLORED)
            {
                FloodFill(world, width, i, row, ++numRegions);
            }
        }
        return numRegions;
    }
}
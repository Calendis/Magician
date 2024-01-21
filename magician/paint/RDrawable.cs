namespace Magician.Paint;

public abstract class RDrawable
{
    protected const int posLength = 3;
    protected const int colLength = 4;

    protected const float zFactor = 9999;
    public int Layer { get; set; }
    public byte[] rgba = new byte[4];

    protected float[]? vertices;

    public abstract void Draw();

}


internal class RPoint : RDrawable
{
    public float[] pos = new float[3];
    public RPoint(double x, double y, double z, double r, double g, double b, double a)
    {
        pos[0] = (float)x; pos[1] = (float)y; pos[2] = (float)z;
        rgba[0] = (byte)r; rgba[1] = (byte)g; rgba[2] = (byte)b; rgba[3] = (byte)a;
    }
    public override void Draw()
    {
        Scribe.Warn("single-point drawing not supported");
    }
}

// Handles drawing of points
internal class RPoints : RDrawable
{
    protected static int dataLength = posLength + colLength;
    public RPoints(RPoint[] pts)
    {
        int numPts = pts.Length;
        vertices = new float[numPts * dataLength];

        for (int i = 0; i < numPts; i++)
        {
            RPoint currentPoint = pts[i];
            vertices[dataLength * i + 0] = currentPoint.pos[0];
            vertices[dataLength * i + 1] = currentPoint.pos[1];
            vertices[dataLength * i + 2] = currentPoint.pos[2] / zFactor;
            // Color
            vertices[dataLength * i + 3] = pts[i].rgba[0] / 255f;
            vertices[dataLength * i + 4] = pts[i].rgba[1] / 255f;
            vertices[dataLength * i + 5] = pts[i].rgba[2] / 255f;
            vertices[dataLength * i + 6] = pts[i].rgba[3] / 255f;
        }
    }

    public override void Draw()
    {
        uint vao = Shaders.Prepare(vertices!, new int[]{posLength, colLength});
        Renderer.GL.DrawArrays(Silk.NET.OpenGL.GLEnum.Points, 0, (uint)vertices!.Length);
        Shaders.Post(vao);
    }
}

internal class RLine : RDrawable
{
    public float[] p0 = new float[3];
    public float[] p1 = new float[3];
    public RLine(
    double x0, double y0, double z0,
    double x1, double y1, double z1,
    double r, double g, double b, double a)
    {
        p0[0] = (float)x0; p0[1] = (float)y0; p0[2] = (float)z0;
        p1[0] = (float)x1; p1[1] = (float)y1; p1[2] = (float)z1;
        rgba[0] = (byte)r; rgba[1] = (byte)g; rgba[2] = (byte)b; rgba[3] = (byte)a;
    }
    public override void Draw()
    {
        Scribe.Issue("single-line drawing not supported");
    }
}

internal class RLines : RDrawable
{
    protected static int dataLength = 2 * (posLength + colLength);
    public RLines(RLine[] lines)
    {
        int numLines = lines.Length;
        vertices = new float[numLines * dataLength];
        for (int i = 0; i < numLines; i++)
        {
            RLine currentLine = lines[i];

            vertices[dataLength * i + 0] = currentLine.p0[0];
            vertices[dataLength * i + 1] = currentLine.p0[1];
            vertices[dataLength * i + 2] = currentLine.p0[2] / zFactor;
            vertices[dataLength * i + 7] = currentLine.p1[0];
            vertices[dataLength * i + 8] = currentLine.p1[1];
            vertices[dataLength * i + 9] = currentLine.p1[2] / zFactor;

            vertices[dataLength * i +  3] = lines[i].rgba[0] / 255f;
            vertices[dataLength * i +  4] = lines[i].rgba[1] / 255f;
            vertices[dataLength * i +  5] = lines[i].rgba[2] / 255f;
            vertices[dataLength * i +  6] = lines[i].rgba[3] / 255f;
            vertices[dataLength * i + 10] = lines[i].rgba[0] / 255f;
            vertices[dataLength * i + 11] = lines[i].rgba[1] / 255f;
            vertices[dataLength * i + 12] = lines[i].rgba[2] / 255f;
            vertices[dataLength * i + 13] = lines[i].rgba[3] / 255f;

        }
    }

    public override unsafe void Draw()
    {
        uint vao = Shaders.Prepare(vertices!, new int[]{posLength, colLength});
        Renderer.GL.DrawArrays(Silk.NET.OpenGL.GLEnum.Lines, 0, (uint)vertices!.Length);
        Shaders.Post(vao);
    }
}

internal class RTriangle : RDrawable
{
    public float[] p0 = new float[3];
    public float[] p1 = new float[3];
    public float[] p2 = new float[3];
    public RTriangle(double x0, double y0, double z0, double x1, double y1, double z1, double x2, double y2, double z2, double r, double g, double b, double a)
    {
        p0[0] = (float)x0; p0[1] = (float)y0; p0[2] = (float)z0;
        p1[0] = (float)x1; p1[1] = (float)y1; p1[2] = (float)z1;
        p2[0] = (float)x2; p2[1] = (float)y2; p2[2] = (float)z2;
        rgba[0] = (byte)r; rgba[1] = (byte)g; rgba[2] = (byte)b; rgba[3] = (byte)a;
    }

    public override void Draw()
    {
        Scribe.Issue("single-triangle drawing not supported");
    }
}

// Handles drawing of filled polygons
internal class RTriangles : RDrawable
{
    protected static int dataLength = 3 * (posLength + colLength);
    public RTriangles(params RTriangle[] tris)
    {
        int numTriangles = tris.Length;
        vertices = new float[numTriangles * dataLength];
        for (int i = 0; i < numTriangles; i++)
        {
            RTriangle currentTriangle = tris[i];

            vertices[dataLength * i + 0] = currentTriangle.p0[0];
            vertices[dataLength * i + 1] = currentTriangle.p0[1];
            vertices[dataLength * i + 2] = currentTriangle.p0[2] / zFactor;

            vertices[dataLength * i + 7] = currentTriangle.p1[0];
            vertices[dataLength * i + 8] = currentTriangle.p1[1];
            vertices[dataLength * i + 9] = currentTriangle.p1[2] / zFactor;

            vertices[dataLength * i + 14] = currentTriangle.p2[0];
            vertices[dataLength * i + 15] = currentTriangle.p2[1];
            vertices[dataLength * i + 16] = currentTriangle.p2[2] / zFactor;

            vertices[dataLength * i + 3] = tris[i].rgba[0] / 255f;
            vertices[dataLength * i + 4] = tris[i].rgba[1] / 255f;
            vertices[dataLength * i + 5] = tris[i].rgba[2] / 255f;
            vertices[dataLength * i + 6] = tris[i].rgba[3] / 255f;

            vertices[dataLength * i + 10] = tris[i].rgba[0] / 255f;
            vertices[dataLength * i + 11] = tris[i].rgba[1] / 255f;
            vertices[dataLength * i + 12] = tris[i].rgba[2] / 255f;
            vertices[dataLength * i + 13] = tris[i].rgba[3] / 255f;

            vertices[dataLength * i + 17] = tris[i].rgba[0] / 255f;
            vertices[dataLength * i + 18] = tris[i].rgba[1] / 255f;
            vertices[dataLength * i + 19] = tris[i].rgba[2] / 255f;
            vertices[dataLength * i + 20] = tris[i].rgba[3] / 255f;
        }
    }

    public override unsafe void Draw()
    {
        uint vao = Shaders.Prepare(vertices!, new int[]{posLength, colLength});
        Renderer.GL.DrawArrays(Silk.NET.OpenGL.GLEnum.Triangles, 0, (uint)vertices!.Length);
        Shaders.Post(vao);
    }
}
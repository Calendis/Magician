namespace Magician.Geo;
public class Matrix
{
    public int width;
    public int height;
    double[,] mx;

    public double Get(int row, int col)
    {
        return mx[row, col];
    }

    public double X => Get(0, 0);
    public double Y => Get(0, 1);
    public double Z => Get(0, 2);

    public Matrix(double[,] mx)
    {
        height = mx.GetLength(0); // rows
        width = mx.GetLength(1);  // columns
        this.mx = mx;
    }
    public Matrix(Multi m) : this(new double[,] { { m.X, 0, 0 }, { 0, m.Y, 0 }, { 0, 0, m.Z } }) { }
    public static Matrix Row(Multi m) { return new Matrix(new double[,] { { m.X, m.Y, m.Z } }); }

    public Matrix Mult(Matrix mox)
    {
        if (mox.width != height)
        {
            throw Scribe.Error($"Columns of {mox} must match rows of {this}");
        }

        double[,] result = new double[mox.height, width];
        for (int row = 0; row < mox.height; row++)
        {
            for (int col = 0; col < width; col++)
            {
                double sum = 0;
                for (int k = 0; k < width; k++)
                {
                    sum += mox.Get(row, k) * Get(k, col);
                }
                result[row, col] = sum;
            }
        }
        return new Matrix(result);
    }

    public static Matrix RotationX(double theta)
    {
        return new Matrix(new double[,]
        {
                { 1, 0, 0 },
                { 0, Math.Cos(theta), -Math.Sin(theta) },
                { 0, Math.Sin(theta), Math.Cos(theta) }
        });
    }
    public static Matrix RotationY(double theta)
    {
        return new Matrix(new double[,]
        {
                { Math.Cos(theta), 0, Math.Sin(theta) },
                { 0, 1, 0 },
                { -Math.Sin(theta), 0, Math.Cos(theta) }
        });
    }
    public static Matrix RotationZ(double theta)
    {
        return new Matrix(new double[,]
        {
                { Math.Cos(theta), -Math.Sin(theta), 0 },
                { Math.Sin(theta), Math.Cos(theta), 0 },
                { 0, 0, 1 }
        });
    }
    public static Matrix Orthographic = new Matrix(new double[,] { { 1, 0, 0 }, { 0, 1, 0 }, { 0, 0, 0 } });
    // TODO: fix this
    public static Matrix Isometric = new Matrix(new double[,] { { 0, 0, 0 }, { 0, 0, 0 }, { 0, 0, 0 } });

    public override string ToString()
    {
        string s = "";
        for (int row = 0; row < height; row++)
        {
            for (int col = 0; col < width; col++)
            {
                s += $"{Get(row, col)} ";
            }
            s += "\n";
        }
        return s;
    }

    internal Matrix ToCartesian(double xOffset, double yOffset)
    {
        for (int row = 0; row < height; row++)
        {
            mx[row, 0] = mx[row, 0] + Data.Globals.winWidth / 2 + xOffset;
            mx[row, 1] = -mx[row, 1] + Data.Globals.winHeight / 2 + yOffset;
        }
        return this;
    }
}
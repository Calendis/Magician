using System;

namespace Magician.Symbols;
public interface ITiling
{
    public abstract Multi Render(double d);
}

// Using Egyptian fractions, define an edge-to-edge tiling of the plane ...
// ... using regular polygons
/*
public class Archimedian : Tiling
{
    //
}

public class Regular : Archimedian
{
    // Attempt to tile the given multi
    public static Multi Tile(Multi m)
    {
        throw new NotImplementedException("Not supported yet.");
    }
}
*/

/* Special hexagonal tiling */
public class Hexagonal : ITiling
{
    int width;
    int height;
    public Hexagonal(int w, int h)
    {
        width = w;
        height = h;
    }

    public Multi Render(double radius)
    {
        double hexHeight = radius * Math.Cos(Math.PI / 6);
        double sideLength = radius * Math.Sin(Math.PI / 6);

        Multi hexGrid = new();
        for (int row = 0; row < height; row++)
        {
            for (int col = 0; col < width; col++)
            {
                // Calculate x and y for hexagonal grid
                double hexX = row % 2 == 0 ? 2 * col * (radius + sideLength) : 2 * col * (radius + sideLength) + radius + sideLength;
                double hexY = row * hexHeight;

                Multi hexagon = Geo.Create.RegularPolygon(hexX, hexY, new HSLA(2 * Math.PI * row / height, 1, 1, 120), 6, radius)
                ;
                hexGrid.Add(hexagon);
            }
        }
        return hexGrid.WithFlags(DrawMode.INVISIBLE);
    }
}
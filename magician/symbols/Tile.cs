using System;

namespace Magician.Symbols
{
    public interface Tiling
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
    public class Hexagonal : Tiling
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
            Multi hexGrid = new Multi();
            for (int row = 0; row < height; row++)
            {
                for (int col = 0; col < width; col++)
                {
                    // This is easy to see if you draw some hexagons in a grid and look at the spacing between them
                    double hexX = row%2==0? 3*col*radius : 3*col*radius + 1.5*radius;
                    double hexY = row%2==0? 2*row*radius : 2*row*radius + radius;
                    
                    Multi hexagon = Geo.Create.RegularPolygon(hexX, hexY, HSLA.RandomVisible(), 6, radius);
                    hexGrid.Add(hexagon);
                }
            }
            return hexGrid.DrawFlags(DrawMode.INVISIBLE);
        }
    }
}

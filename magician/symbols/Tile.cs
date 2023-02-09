using System;

namespace Magician.Symbols
{
    public interface Tiling
    {
        //
    }

    // Using Egyptian fractions, define an edge-to-edge tiling of the plane ...
    // ... using regular polygons
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
}

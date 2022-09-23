/*
    A driver, or modulator, modifies properties of
    math objects (Single, Multi, Polygon, etc)
*/

namespace Magician
{
    class Driver
    {
        private delegate double driveFunction(double x);
    }

    class MultiDriver : Driver
    {
        // Dimensionality of the MultiDriver
        private int ins;
        private int outs;
    }
}
/*
    A driver, or modulator, modifies properties of
    math objects (Single, Multi, Polygon, etc)
*/

namespace Magician
{
    delegate double DriveFunction(params double[] x);
    class Driver : Map
    {
        private DriveFunction driveFunction;
        private int ins;
        public Driver(int ins, Func<double[], double> df)
        {
            this.ins = ins;
            driveFunction = new DriveFunction(df);
        }
    }
}
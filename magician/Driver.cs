/*
    A driver, or modulator, modifies properties of
    math objects (Single, Multi, Polygon, etc)
*/

namespace Magician
{
    delegate double DriveFunction(params double[] x);
    public class Driver : IMap
    {
        private DriveFunction driveFunction;
        private int ins;
        
        public Driver(int ins, Func<double[], double> df)
        {
            this.ins = ins;
            driveFunction = new DriveFunction(df);
        }

        // Default "zero-driver"
        public Driver()
        {
            ins = 0;
            driveFunction = new DriveFunction(x => 0);
        }

        public double Evaluate(params double[] x)
        {
            return driveFunction(x);
        }
    }
}
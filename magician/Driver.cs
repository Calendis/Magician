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
        private Action<double> output;
        private int ins;
        private int current;
        private int start;
        private int end;
        
        public Driver(int ins, Func<double[], double> df)
        {
            this.ins = ins;
            driveFunction = new DriveFunction(df);
        }
        public Driver(int ins, Func<double[], double> df, Action<double> output)
        {
            this.ins = ins;
            driveFunction = new DriveFunction(df);
            this.output = output;
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

        public void Drive(params double[] x)
        {
            output(driveFunction(x));
        }
    }
}
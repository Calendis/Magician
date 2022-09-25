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
        private Action<double>? output;
        private int ins;
        private double[] offsets;
        
        /*
        private int current;
        private int start;
        private int end;
        */
        
        public Driver(Func<double[], double> df)
        {
            this.ins = 1;
            driveFunction = new DriveFunction(df);
            offsets = new double[]{0};
        }
        public Driver(Func<double[], double> df, Action<double> output)
        {
            this.ins = 1;
            driveFunction = new DriveFunction(df);
            this.output = output;
            offsets = new double[]{0};
        }
        public Driver(double[] offsets, Func<double[], double> df)
        {
            this.ins = offsets.Length;
            driveFunction = new DriveFunction(df);
            this.offsets = offsets;
        }
        public Driver(double[] offsets, Func<double[], double> df, Action<double> output)
        {
            this.ins = offsets.Length;
            driveFunction = new DriveFunction(df);
            this.output = output;
            this.offsets = offsets;
        }

        // Default identity driver
        public Driver(Action<double> output)
        {
            ins = 1;
            driveFunction = new DriveFunction(x => x[0]);
            this.output = output;
        }

        public void SetOutput(Action<double> o)
        {
            output =  o;
        }

        public double Evaluate(params double[] x)
        {
            double[] offsetX = new double[x.Length];
            for (int i = 0; i < x.Length; i++)
            {
                offsetX[i] = x[i] + offsets[i];
            }
            return driveFunction(offsetX);
        }

        public void Drive(params double[] x)
        {
            if (output is not null)
            {
                output(Evaluate(x));
            }
            else
            {
                Console.WriteLine("ERROR: null output on driver");
            }
        }
    }
}
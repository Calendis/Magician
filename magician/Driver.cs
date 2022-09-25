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
        
        /*
        private int current;
        private int start;
        private int end;
        */
        
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

        // Default identity driver
        public Driver(Action<double> output)
        {
            ins = 0;
            driveFunction = new DriveFunction(x => x[0]);
            this.output = output;
        }

        public void SetOutput(Action<double> o)
        {
            output =  o;
        }

        public Action<double> Output
        {
            get => output;
        }

        public double Evaluate(params double[] x)
        {
            return driveFunction(x);
        }

        public void Drive(params double[] x)
        {
            if (output is not null)
            {
                output(driveFunction(x));
            }
            else
            {
                Console.WriteLine("ERROR: null output on driver");
            }
        }
    }
}
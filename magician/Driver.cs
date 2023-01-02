/*
    A driver, or modulator, modifies properties of
    math objects (Single, Multi, Polygon, etc)
*/

namespace Magician
{
    public delegate double[] DriveFunction(params double[] x);
    public class Driver : IMap, Driveable
    {
        protected DriveFunction driveFunction;
        private Action<double[]>? output;
        private string actionString;
        
        // Full constructor
        public Driver(Func<double[], double> df, Action<double>? output)
        {
            // converts driver with one output to multi-output format
            driveFunction = new DriveFunction(new Func<double[], double[]>(x => new double[]{df.Invoke(x)}));
            if (output is not null)
            {
                this.output = new Action<double[]>(x => output.Invoke(x[0]));
            }
            actionString = "";
        }
        public Driver(Func<double[], double[]> df, Action<double[]>? output)
        {
            driveFunction = new DriveFunction(df);
            this.output = output;
            actionString = "";
        }
        public Driver(Func<double[], double> df) : this(df, null) {}
        public Driver(Func<double[], double[]> df) : this(df, null) {}

        // Used for making copies of Drivers
        public Driver(Driver d, Multi m, string s)
        {
            driveFunction = new DriveFunction(d.GetDriveFunction());
            output = new Action<double[]>(x => Multi.StringMap(m, s).Invoke(x[0]));
            actionString = s;
        }

        public string ActionString
        {
            get => actionString;
            set => actionString = value;
        }

        public void SetOutput(Action<double[]> o)
        {
            output =  o;
        }

        public void SetOutput(Action<double> o)
        {
            output = new Action<double[]>(x => o.Invoke(x[0]));
        }

        public double[] Evaluate(params double[] x)
        {
            return driveFunction(x);
        }

        public double Evaluate(double x)
        {
            return driveFunction(x)[0];
        }

        public Func<double[], double[]> GetDriveFunction()
        {
            return new Func<double[], double[]>(driveFunction);
        }

        public void Go(params double[] x)
        {            
            if (output is not null)
            {
                output(Evaluate(x));
            }
            else
            {
                Evaluate(x);
            }
        }

        public Driver CopiedTo(Multi m)
        {
            return new Driver(this, m, actionString);
        }

        public override string ToString()
        {
            return $"Driver on s({actionString}), o({output})";
        }
    }
}
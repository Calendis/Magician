/*
    A driver, or modulator, modifies properties of
    math objects (Single, Multi, Polygon, etc)
*/

/*
namespace Magician
{
    public delegate double[] DriveFunction(params double[] x);
    public class Driver : IMap, IDriveable
    {
        protected DriveFunction driveFunction;
        private Func<double[], Quantity>? output;
        private string actionString;
        
        // Full constructor
        public Driver(Func<double[], double> df, Func<double, Quantity>? output)
        {
            // converts driver with one output to multi-output format
            driveFunction = new DriveFunction(new Func<double[], double[]>(x => new double[]{df.Invoke(x)}));
            
            if (output is not null)
            {
                //this.output = new Action<double[]>(x => output.Invoke(x[0]));
                this.output = new Func<double[], Quantity>(d => output.Invoke(d[0]));
            }
            actionString = "";
        }
        public Driver(Func<double[], double[]> df, Func<double[], Quantity>? output)
        {
            driveFunction = new DriveFunction(df);
            this.output = output;
            actionString = "";
        }
        public Driver(Func<double[], double> df) : this(df, null) {}
        public Driver(Func<double[], double[]> df) : this(df, null) {}
        public Driver(IMap df, Func<double, Quantity>? output=null) : this(new Func<double[], double[]>(x => new double[]{df.Evaluate(x[0])}))
        {
            if (output is not null)
            {
                //this.output = new Action<double[]>(x => output.Invoke(x[0]));
                this.output = new Func<double[], Quantity>(x => output.Invoke(x[0]));
            }
        }

        // Used for making copies of Drivers
        public Driver(Driver d, Multi m, string s)
        {
            driveFunction = new DriveFunction(d.GetDriveFunction());
            output = new Func<double[], Quantity>(x => Multi.StringMap(m, s).Invoke(x[0]));
            actionString = s;
        }

        public string ActionString
        {
            get => actionString;
            set => actionString = value;
        }

        public void SetOutput(Func<double[], Quantity> o)
        {
            output =  o;
        }

        public void SetOutput(Func<double, Quantity> o)
        {
            output = new Func<double[], Quantity>(x => o.Invoke(x[0]));
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
*/
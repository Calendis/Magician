/*
    A driver, or modulator, modifies properties of
    math objects (Single, Multi, Polygon, etc)
*/

namespace Magician
{
    public delegate double DriveFunction(params double[] x);
    public class Driver : IMap, Driveable
    {
        protected DriveFunction driveFunction;
        private Action<double>? output;
        private string actionString;
        
        // Full constructor
        public Driver(Func<double[], double> df, Action<double>? output)
        {
            driveFunction = new DriveFunction(df);
            this.output = output;
            actionString = "";
        }
        public Driver(Func<double[], double> df) : this(df, null) {}

        // Used for making copies of Drivers
        public Driver(Driver d, Multi m, string s)
        {
            driveFunction = new DriveFunction(d.GetDriveFunction());
            output = Multi.StringMap(m, s);
            actionString = s;
        }

        public string ActionString
        {
            get => actionString;
            set => actionString = value;
        }

        public void SetOutput(Action<double> o)
        {
            output =  o;
        }

        public double Evaluate(params double[] x)
        {
            return driveFunction(x);
        }

        public Func<double[], double> GetDriveFunction()
        {
            return new Func<double[], double>(driveFunction);
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

        // This acts as function composition
        //public Driver Driven(Driver d)

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
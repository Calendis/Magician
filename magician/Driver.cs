/*
    A driver, or modulator, modifies properties of
    math objects (Single, Multi, Polygon, etc)
*/

namespace Magician
{
    public delegate double DriveFunction(params double[] x);
    public class Driver : IMap
    {
        protected DriveFunction driveFunction;
        private Action<double>? output;
        private int ins;
        private double[] offsets;
        private string actionString;
        
        public Driver(Func<double[], double> df)
        {
            this.ins = 1;
            driveFunction = new DriveFunction(df);
            offsets = new double[]{0};
        }
        public Driver(Func<double[], double> df, Action<double> output) : this(new double[]{0}, df)
        {
            this.output = output;
        }
        private Driver(double[] offsets, Func<double[], double> df) : this(df)
        {
            this.ins = offsets.Length;
            this.offsets = offsets;
        }
        // Full constructor
        public Driver(double[] offsets, Func<double[], double> df, Action<double> output) : this(offsets, df)
        {
            this.output = output;
        }
        // Output from stringmap
        public Driver(Func<double[], double> df, ref Multi m, string s) : this(new double[]{0}, df)
        {
            output = StringMap(m, s);
        }

        public Driver(Driver d, Multi m, string s)
        {
            driveFunction = d.GetDriveFunction();
            offsets = d.offsets;
            output = StringMap(m, s);
            actionString = d.actionString;
        }
        public void SetRef(Multi m)
        {
            output = StringMap(m, actionString);
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

        public DriveFunction GetDriveFunction()
        {
            return driveFunction;
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

        public static Action<double> StringMap(Multi m, string s)
        {
            Action<double> o;
            s = s.ToUpper();
            switch(s)
            {
                case "X":
                    o = m.SetX;
                    break;
                case "X+":
                    o = m.IncrX;
                    break;
                
                case "Y":
                    o = m.SetY;
                    break;
                case "Y+":
                    o = m.IncrY;
                    break;

                case "PHASE":
                    o = m.SetPhase;
                    break;
                case "PHASE+":
                    o = m.IncrPhase;
                    break;
                
                default:
                    Console.WriteLine($"ERROR: Unknown driver string {s}");
                    throw new NotImplementedException();
            }
            return o;
        }
    }
}
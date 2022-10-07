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
        
        // Full constructor
        public Driver(double[] offsets, Func<double[], double> df, Action<double> output)
        {
            ins = offsets.Length;
            driveFunction = new DriveFunction(df);
            this.offsets = offsets;
            this.output = output;
            actionString = "";
        }
        public Driver(Func<double[], double> df, Action<double> output) : this(new double[]{0}, df, output) {}
        private Driver(double[] offsets, Func<double[], double> df) : this(offsets, df, null) {}
        public Driver(Func<double[], double> df) : this(new double[] {0}, df) {}

        public Driver(Driver d, Multi m, string s)
        {
            driveFunction = new DriveFunction(d.GetDriveFunction());
            offsets = d.offsets.ToArray();
            output = StringMap(m, s);
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
            double[] offsetX = new double[x.Length];
            for (int i = 0; i < x.Length; i++)
            {
                offsetX[i] = x[i] + offsets[i];
            }
            return driveFunction(offsetX);
        }

        public Func<double[], double> GetDriveFunction()
        {
            return driveFunction.Invoke;
            //return driveFunction;
        }

        public void Drive(params double[] x)
        {            
            if (output is not null)
            {
                output(Evaluate(x));
            }
            else
            {
                Console.WriteLine($"ERROR: null output on a:({actionString}) driver");
            }
        }

        public Driver CopiedTo(Multi m)
        {
            return new Driver(this, m, actionString);
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

                case "MAGNITUDE":
                    o = m.SetMagnitude;
                    break;
                
                case "MAGNITUDE+":
                    o = m.IncrMagnitude;
                    break;

                case "COL0":
                    o = m.SetCol0;
                    break;
                
                case "COL1":
                    o = m.SetCol1;
                    break;

                case "COL2":
                    o = m.SetCol2;
                    break;

                case "COL3":
                    o = m.SetAlpha;
                    break;

                case "COL0+":
                    o = m.IncrCol0;
                    break;
                
                case "COL1+":
                    o = m.IncrCol1;
                    break;

                case "COL2+":
                    o = m.IncrCol2;
                    break;

                case "COL3+":
                    o = m.IncrAlpha;
                    break;
                
                default:
                    Console.WriteLine($"ERROR: Unknown driver string {s}");
                    throw new NotImplementedException();
            }
            return o;
        }

        public override string ToString()
        {
            return $"Driver on s({actionString}), o({output})";
        }
    }
}
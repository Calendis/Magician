/*
    A Quantity is the basic "unit" math object in Magician
    from which more complex kinds of objects are derived
*/

namespace Magician
{
    public class Quantity : IMap, IDriveable
    {
        protected List<Driver> drivers = new List<Driver>();

        // Global container for created quantites
        // This can be used to Drive the quantities
        public static List<Quantity> ExtantQuantites = new List<Quantity>();

        protected double q;
        public Quantity(double q)
        {
            this.q = q;
        }

        public void Set(double x)
        {
            q = x;
        }
        public Quantity As(double x)
        {
            q = x;
            return this;
        }

        public void Incr(double x)
        {
            q += x;
        }
        // Converts to double
        public double Evaluate(double offset=0)
        {
            return q + offset;
        }

        public double[] Evaluate(double[] offsets)
        {
            double[] outputs = new double[offsets.Length];
            for (int i = 0; i < offsets.Length; i++)
            {
                outputs[i] = q + outputs[i];
            }
            return outputs;
        }

        // Operators
        public Quantity Delta(double x)
        {
            q += x;
            return this;
        }
        public Quantity GetDelta(double x)
        {
            return new Quantity(q + x);
        }
        public Quantity Mult(double x)
        {
            q *= x;
            return this;
        }

        // Driver code
        protected void AddDriver(Driver d)
        {
            drivers.Add(d);
        }

        public void Go(params double[] x)
        {
            foreach (Driver d in drivers)
            {
                d.Go(x);
            }
        }

        public Quantity Driven(Func<double[], double> df)
        {
            Func<double, Quantity> output = As;
            Driver d = new Driver(df, output);
            AddDriver(d);
            return this;
        }

        public override string ToString()
        {
            return q.ToString();
        }
    }
}
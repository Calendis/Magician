/*
    A Quantity is the basic "unit" math object in Magician
    from which more complex kinds of objects are derived
*/

namespace Magician
{
    public class Quantity : IMap, Driveable
    {
        protected List<Driver> drivers = new List<Driver>();
        protected Drawable parent = Point.Origin;

        // Global container for created quantites
        // This can be used to Drive the quantities
        public static List<Quantity> ExtantQuantites = new List<Quantity>();

        protected double q;
        public Quantity(double q)
        {
            this.q = q;
        }

        public Drawable Parent()
        {
            return parent;
        }

        public void Set(double x)
        {
            this.q = x;
        }
        // Converts to double
        public double Evaluate(params double[] xs)
        {
            return q;
        }

        // Operators
        public Quantity Delta(double x)
        {
            q += x;
            return this;
        }
        public Quantity Mult(double x)
        {
            q *= x;
            return this;
        }

        // Driver code
        public void AddDriver(Driver d)
        {
            drivers.Add(d);
        }

        public void Drive(params double[] x)
        {
            foreach (Driver d in drivers)
            {
                d.Drive(x);
            }
        }

        public Quantity Driven(Func<double[], double> df)
        {
            Action<double> output = Set;
            Driver d = new Driver(df, output);
            AddDriver(d);
            return this;
        }
    }
}
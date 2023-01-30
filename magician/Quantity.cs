/*
    A Quantity is the basic "unit" math object in Magician
    from which more complex kinds of objects are derived
*/

namespace Magician
{
    public class Quantity : IMap, IDriveable
    {
        protected List<IMap> drivers = new List<IMap>();

        // Global container for created quantites
        // This can be used to Drive the quantities
        public static List<Quantity> ExtantQuantites = new List<Quantity>();

        protected double q;
        // Setting the relative offset is useful when you want to offset a quantity while keeping the same reference
        double relativeOffset = 0;
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
        public double Evaluate(double offset = 0)
        {
            return q + offset + relativeOffset;
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
            //return new Quantity(q + x);
            relativeOffset = x;
            return this;
        }
        public Quantity Mult(double x)
        {
            q *= x;
            return this;
        }

        // Driver code
        protected static void _AddDriver(Quantity q, IMap imap)
        {
            q.drivers.Add(imap);
        }
        public Quantity Driven(IMap imap)
        {
            _AddDriver(this, imap);
            return this;
        }
        // Allow driving with lambdas
        public Quantity Driven(Func<double, double> f)
        {
            return Driven(new DirectMap(f));
        }

        public void Drive(double offset=0)
        {
            // band-aid
            relativeOffset = 0;
            
            foreach (IMap imap in drivers)
            {
                double result = imap.Evaluate(q+offset);
                q = result;
            }
        }

        public override string ToString()
        {
            return "Quantity " + q.ToString();
        }
    }
}
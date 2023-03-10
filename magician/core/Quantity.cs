/*
    A Quantity is the basic "unit" math object in Magician
    from which more complex kinds of objects are derived
*/
namespace Magician;

public class Quantity : CustomMap
{
    List<IMap> drivers = new List<IMap>();

    // Global container for created quantites
    // This can be used to Drive the quantities
    public static List<Quantity> ExtantQuantites = new List<Quantity>();

    protected double q;
    // Setting the relative offset is useful when you want to offset a quantity while keeping the same reference
    public Quantity(double q)
    {
        this.q = q;
    }
    public Quantity(Quantity qq)
    {
        q = qq.Evaluate();
        drivers.AddRange(qq.drivers);
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
    public void From(Quantity oq)
    {
        q = oq.q;
    }

    public void Incr(double x)
    {
        q += x;
    }
    // Converts to double
    public new double Evaluate(double offset = 0)
    {
        return q + offset;
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
        return Driven(new CustomMap(f));
    }

    // Remove the drivers
    public void Eject()
    {
        drivers.Clear();
    }
    public List<IMap> GetDrivers()
    {
        return drivers;
    }

    public override string ToString()
    {
        return "Quantity " + q.ToString();
    }
}
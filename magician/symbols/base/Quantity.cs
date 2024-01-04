/*
    A Quantity is the basic "unit" math object in Magician
    from which more complex kinds of objects are derived
    TODO:
        make this a subclass of Oper, merge with Variable
*/
namespace Magician;


public class Quantity_old
{
    //List<IMap> drivers = new List<IMap>();

    protected double q;
    // Setting the relative offset is useful when you want to offset a quantity while keeping the same reference
    public Quantity_old(double q)
    {
        this.q = q;
    }
    public Quantity_old(Quantity_old qq)
    {
        q = qq.Get();
        //drivers.AddRange(qq.drivers);
    }

    public void Set(double x)
    {
        q = x;
    }
    public void Set(Quantity_old oq)
    {
        this.q = oq.q;
    }

    public void Incr(double x)
    {
        q += x;
    }
    public double Get(double offset = 0)
    {
        return q + offset;
    }
    public Quantity_old Delta(double x)
    {
        q += x;
        return this;
    }
    public Quantity_old GetDelta(double x)
    {
        return new Quantity_old(q + x);
    }

    public static Quantity_old operator +(Quantity_old q1, Quantity_old q2)
    {
        return new Quantity_old(q1.q + q2.q);
    }
    public static Quantity_old operator-(Quantity_old q1, Quantity_old q2)
    {
        return new Quantity_old(q1.q - q2.q);
    }

    // Driver code
    //protected static void _AddDriver(Quantity q, IMap imap)
    //{
    //    q.drivers.Add(imap);
    //}
    //public Quantity Driven(IMap imap)
    //{
    //    _AddDriver(this, imap);
    //    return this;
    //}
    // Allow driving with lambdas
    //public Quantity Driven(Func<double, double> f)
    //{
    //    return Driven(new CustomMap(f));
    //}
//
    //// Remove the drivers
    //public void Eject()
    //{
    //    drivers.Clear();
    //}
    //public List<IMap> GetDrivers()
    //{
    //    return drivers;
    //}

    public override string ToString()
    {
        return "Quantity " + q.ToString();
    }
}

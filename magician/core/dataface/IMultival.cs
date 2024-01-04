namespace Magician.Symbols;

public interface IMultival
{
    public IVal[] Values { get; }
    public int Dims { get => Values.Select(v => v.Dims).Sum(); }

    public Variable ToVariable()
    {
        List<double> vs = new();
        foreach (IVal iv in Values)
        {
            vs.AddRange(iv.All);
        }
        return new Variable(vs.ToArray());
    }

    public static Geo.Vec operator +(IMultival i, IMultival v)
    {
        return new(i.Values.Zip(v.Values, (a, b) => a + b).ToArray());
    }
    public static Geo.Vec operator -(IMultival i, IMultival v)
    {
        return new(i.Values.Zip(v.Values, (a, b) => a - b).ToArray());
    }

    public static IMultival operator *(IMultival i, double x)
    {
        return new Geo.Vec(i.Values.Select(k => k*x).ToArray());
    }
    public static IMultival operator /(IMultival i, double x)
    {
        return new Geo.Vec(i.Values.Select(k => k/x).ToArray());
    }
    public static IMultival operator *(IMultival i, IVal x)
    {
        return new Geo.Vec(i.Values.Select(k => k*x).ToArray());
    }
    public static IMultival operator /(IMultival i, IVal x)
    {
        return new Geo.Vec(i.Values.Select(k => k/x).ToArray());
    }
}
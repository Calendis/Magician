namespace Magician.Core.Maps;

using Magician.Geo;
using Magician.Alg;
using static Magician.Geo.Create;

public interface IRelation
{
    // TODO: make this an IVal
    IVal Cache { get; }
    public IVal Evaluate(params double[] args);
    public IVal Evaluate(IVal arg) => Evaluate(arg.Values.ToArray());
    public double[] Evaluate(List<double> args) => Evaluate(args.ToArray()).Values.ToArray();
    public int Ins { get; }
    // TODO: change this
    public int Outs { get { return 0; } }
}

public interface IParametric : IRelation
{
    int IRelation.Ins => 1;
    public IVal Evaluate(double x) => Evaluate(new double[] {x});
    IVal IRelation.Evaluate(params double[] args) => Evaluate(args[0]);
    IVal IRelation.Evaluate(IVal args) => Evaluate(args.Get());
}
public interface IMap : IRelation
{
    int IRelation.Ins => 1;
    //public IVal Evaluate(double x);
    public double Evaluate(double x) => ((IRelation)this).Evaluate(x).Get();
    //IVal IFunction.Evaluate(params double[] args) => Evaluate(args[0]);
    //IVal IFunction.Evaluate(IVal args) => Evaluate(args.Get());
}


// Multiple inputs, multiple outputs
// Plottable when the number of inputs is specified
public class Relational : IRelation
{
    protected readonly Val vCache = new(0);
    public IVal Cache => vCache;
    Func<double[], double[]> map;
    public int Ins { get; set; }
    public Relational(Func<double[], double[]> f, int inputs)
    {
        Ins = inputs;
        map = f;
    }
    public Relational(Func<double[], IVal> f, int inputs) : this(xs => f.Invoke(xs).Values.ToArray(), inputs) {}

    //public double[] Evaluate(List<double> args)
    //{
    //    return map.Invoke(args.ToArray());
    //}
    public IVal Evaluate(params double[] args)
    {
        Cache.Set(map.Invoke(args));
        return Cache;
    }
}

// Parametric equation. One input, multiple outputs
public class Parametric : IParametric
{
    //public int Params { get; set; }
    private readonly Val vCache = new(0);
    public IVal Cache => vCache;
    public int Outs { get; set; }
    Func<double, double[]> map;
    //public ParamMap(params Func<double, double>[] fs) : base(xs => fs.Select(m => m.Invoke(xs[0])).ToArray())
    public Parametric(Func<double, double[]> m)
    {
        // TODO: change the outs
        Outs = 0;
        map = m;
    }
    public Parametric(params Func<double, double>[] ns) : this(x => ns.Select(f => f.Invoke(x)).ToArray()) { }
    public Parametric(params IMap[] fs) : this(fs.Select<IMap, Func<double, double>>(f => f.Evaluate).ToArray()) { }
    public IVal Evaluate(double x = 0)
    {
        Cache.Set(map.Invoke(x));
        return Cache;
    }
}

// One or fewer input, one output
public class Direct : IMap
{
    public readonly static Direct Dummy = new(x => 0);
    private readonly Val vCache = new(0);
    public IVal Cache => vCache;
    Func<double, double> map;
    public Direct(Func<double, double> f)
    {
        map = f;
    }

    public IVal Evaluate(params double[] args)
    {
        Cache.Set(map.Invoke(args[0]));
        return Cache;
    }
    public IVal Evaluate(double a=0)
    {
        Cache.Set(map.Invoke(a));
        return Cache;
    }
}
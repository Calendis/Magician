namespace Magician.Symbols;
using Core;

public class Variable : Invertable, IVar
{
    protected List<double> qs;
    protected List<IVal> ivals;
    List<double> IDimensional<double>.Values { get => qs; }
    List<IVal> IDimensional<IVal>.Values { get => ivals; }
    bool found;
    public bool Found => found;
    int IDimensional<double>.Dims => qs is null ? 0 : qs.Count;
    public bool IsVector => ivals.Count > 0 && ((IVec)this).Dims > 1;
    public IVal Value => (IVal)this;
    public IVec Vector => (IVec)this;
    public double Magnitude
    {
        get
        {
            if (IsVector)
            {
                //Scribe.Info($"vector magnitude");
                return new Vec(ivals.ToArray()).Magnitude;
            }
            else
            {
                //Scribe.Info($"value magnitude");
                return ((IVal)new Num(qs.ToArray())).Magnitude;
            }
        }
    }
    // Creating an unsolved variable
    public Variable(string n) : base(n) {qs ??= new(); ivals ??= new();}
    public Variable(string n, params double[] v) : base(n)
    {
        qs = new();
        Set(v);
        ivals ??= new();
        //Vector = false;
    }
    public Variable(params double[] v) : this($"constant({v})", v) { }
    public Variable(IVal iv) : this(iv.Values.ToArray()) { }
    public Variable(string n, params IVal[] ivs) : base(n)
    {
        ivals = ivs.ToList();
        Set(ivs);
        qs ??= new(); ivals ??= new();
        //Vector = true;
    }
    public Variable(params IVal[] ivs) : this($"vector({ivs})", ivs) { }
    public Variable(IVec iv) : this(iv.Values.ToArray()) { }

    double IVal.Get(int i)
    {
        return qs[i];
    }
    void IVal.Set(params double[] vs)
    {
        if (qs is not null && qs.Count > 0 && vs.Length != Value.Dims)
            throw Scribe.Error("Mismatch");
        qs = vs.ToList();
    }
    public void Set(params double[] vs)
    {
        ((IVal)this).Set(vs);
        found = true;
        //Vector = false;
    }
    public void Set(params IVal[] ivs)
    {
        ivals = ivs.ToList();
        found = true;
        //Vector = true;
    }
    public void Reset()
    {
        found = false;
    }

    public void Pad(int d)
    {
        double[] newAll = new double[d];
        for (int i = 0; i < qs.Count; i++)
        {
            newAll[i] = qs[i];
        }
        qs = newAll.ToList();
    }

    public void Normalize()
    {
        double m = ((IDimensional<double>)this).Magnitude;
        for (int i = 0; i < qs.Count; i++)
        {
            qs[i] /= m;
        }
    }

    public override void ReduceOuter() { }

    // Inverting a variable with no operation (other than the variable itself) does NOTHING
    public override Oper Inverse(Oper axis, Oper? opp = null) { return this; }

    public override Variable Copy()
    {
        // Unknown variables share an instance
        if (!found)
            return this;
        // Knowns actually get copied
        return new Variable(qs.ToArray());
    }
    public override string ToString()
    {
        if (!found)
            return name;
        IVal trim = Value.Trim();
        return trim.Dims < 2 ? $"{Value.Get()}" : $"({Value.Values.Aggregate("", (d, n) => $"{d + n},").TrimEnd(',')})";
        //return found ? qs.Length == 1 ? Value.Get().ToString() : Scribe.Expand<IEnumerable<double>, double>(qs) : name;
    }

    /* This is good design */
    // TODO: fix it
    public override Oper New(IEnumerable<Oper> a, IEnumerable<Oper> b)
    {
        throw Scribe.Error("Variables are not newable. Use .Copy if you need a copy");
    }
    public static Oper StaticNew(IEnumerable<Oper> a, IEnumerable<Oper> b)
    {
        throw Scribe.Error("Variables are not newable. Use .Copy if you need a copy");
    }

    public override Variable Sol()
    {
        return new Variable(qs.ToArray());
        // TODO: test this
        //return this;
    }

    public override Oper Degree(Oper v)
    {
        if (Like(v))
            return new Variable(1);
        return new Variable(0);
    }

    public override Oper Add(Oper o)
    {
        if (Found && o.IsConstant)
        {
            //return new Variable(Val + o.Solution().Val);
            AssertLengthMatch(o.Sol());
            return new Variable((IVal)this + o.Sol());
        }
        return base.Add(o);
    }
    public override Oper Subtract(Oper o)
    {
        if (Found && o.IsConstant)
        {
            AssertLengthMatch(o.Sol());
            return new Variable((IVal)this - o.Sol());
        }
        return base.Subtract(o);
    }
    public override Oper Mult(Oper o)
    {
        if (Found && o.IsConstant)
        {
            AssertLengthMatch(o.Sol());
            return new Variable(((IVal)this) * o.Sol());
        }
        return base.Mult(o);
    }
    public override Oper Divide(Oper o)
    {
        //if (o.IsDetermined && o.Sol().Value.Trim().Dims == 1 && o.Sol().Value.Get() == 1)
        //    return Copy();
        if (Found && o.IsConstant)
        {
            AssertLengthMatch(o.Sol());
            return new Variable(((IVal)this) / o.Sol());
        }
        return base.Divide(o);
    }

    //public static Variable operator *(IVal i, Variable v)
    //{
    //    throw Scribe.Issue($"Noooo don't do this");
    //}

    internal void AssertLengthMatch(IVal other)
    {
        if (Value.Trim().Dims != other.Trim().Dims)
            throw Scribe.Error($"Length of {Value.Trim()} ({Value.Trim().Dims}) did not match length of {other.Trim()} ({other.Trim().Dims})");
    }

    public static readonly Variable Undefined = new("undefined");

}
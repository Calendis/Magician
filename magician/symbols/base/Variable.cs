
namespace Magician.Symbols;

/* TODO: Merge with Quantity */
public class Variable : Invertable, IVal
{
    bool found = false;
    protected double[] qs;
    double[] IVal.All { get => qs; }
    int IDimensional.Dims => qs is null ? 0 : qs.Length;
    double IVal.Get(int i)
    {
        return qs[i];
    }
    void IVal.Set(params double[] vs)
    {
        if (vs.Length != qs.Length)
            throw Scribe.Error("Mismatch");
        qs = vs.ToArray();
    }
    public void Set(params double[] vs)
    {
        ((IVal)this).Set(vs);
        found = true;
    }
    public void Reset()
    {
        found = false;
    }
    public bool Found
    {
        get => found;
    }

    public void Pad(int d)
    {
        double[] newAll = new double[d];
        for (int i = 0; i < qs.Length; i++)
        {
            newAll[i] = qs[i];
        }
        qs = newAll;
    }

    public override void ReduceOuter()
    {
        // do nothing
    }

    // Inverting a variable with no operation (other than the variable itself) does NOTHING
    public override Oper Inverse(Oper axis, Oper? opp = null) { return this; }

    // Creating an unsolved variable
    public Variable(string n) : base(n) {/* trivialAssociative = true; */}
    public Variable(string n, params double[] v) : base(n)
    {
        qs = new double[v.Length];
        Set(v);
        //Val = v;
        //trivialAssociative = true;
    }
    public Variable(params double[] v) : this($"constant({v})", v) { }
    public Variable(IVal iv) : this(iv.All) {}

    public override Variable Copy()
    {
        // Unknown variables share an instance
        if (!found)
            return this;
        // Knowns actually get copied
        return new Variable(qs);
    }
    public override string ToString()
    {
        IVal trim = Value.Trim();
        return !found ? name : trim.Dims < 2 ? $"{Value.Get()}" : $"({Value.All.Aggregate("", (d, n) => $"{d+n},").TrimEnd(',')})";
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
        return new Variable(qs);
        // TODO: test this
        //return this;
    }
    public IVal Value => (IVal)this;

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
        if (qs.Length != other.All.Length)
            throw Scribe.Error($"Length of {this} did not match length of {other}");
    }

    public static readonly Variable Undefined = new("undefined");

}
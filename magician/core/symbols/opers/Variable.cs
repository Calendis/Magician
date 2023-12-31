namespace Magician.Symbols;

/* TODO: Merge with Quantity */
public class Variable : Invertable, IVal
{
    bool found = false;
    protected double[] qs;
    //Quantity q = new(0);
    double[] IVal.Quantities { get => qs; set => qs = value; }
    public double Val
    {
        get
        {
            if (!found)
                throw Scribe.Error($"Variable {name} is unknown: {q}");
            if (qs.Length != 1)
                throw Scribe.Error($"Invalid IVal {Scribe.Expand<IEnumerable<double>, double>(qs)}");
            //return q.Get();
            return qs[0];
        }
        set
        {
            //q.Set(value);
            if (qs.Length != 1)
                throw Scribe.Error($"Invalid IVal {Scribe.Expand<IEnumerable<double>, double>(qs)}");
            qs[0] = value;
            found = true;
        }
    }

    public bool Found
    {
        get => found;
    }

    public void Reset()
    {
        found = false;
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
        qs = v.ToArray();
        //Val = v;
        //trivialAssociative = true;
    }
    public Variable(params double[] v) : this($"constant({v})", v) { }

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
        return found ? Scribe.Expand<IEnumerable<double>, double>(qs) : name;
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

    public override Variable Solution()
    {
        return new Variable(qs);
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
            return new Variable(this + o.Solution());
        }
        return base.Add(o);
    }
    public override Oper Subtract(Oper o)
    {
        if (Found && o.IsConstant)
            return new Variable(Val - o.Solution().Val);
        return base.Subtract(o);
    }
    public override Oper Mult(Oper o)
    {
        if (Found && o.IsConstant)
            return new Variable(Val * o.Solution().Val);
        return base.Mult(o);
    }
    public override Oper Divide(Oper o)
    {
        if (Found && o.IsConstant)
            return new Variable(Val / o.Solution().Val);
        return base.Divide(o);
    }

    public static readonly Variable Undefined = new("undefined");
}
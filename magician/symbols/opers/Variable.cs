namespace Magician.Symbols;

/* TODO: Merge with Quantity */
public class Variable : Oper
{
    protected override int identity { get => throw Scribe.Error("Undefined");}
    public bool found = false;
    Quantity q = new(0);
    public double Val
    {
        get
        {
            if (!found)
                throw Scribe.Error($"Variable {name} is unknown: {q}");
            return q.Get();
        }
        set
        {
            q.Set(value);
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

    // Creating an unsolved variable
    public Variable(string n) : base(n){}
    public Variable(string n, double v) : base(n)
    {
        Val = v;
    }
    public Variable(double v) : this($"constant({v})", v) { }

    public override Variable Copy()
    {
        if (!found)
            return this;
        return new Variable(Val);
    }
    public override string ToString()
    {
        return found ? Val.ToString() : name;
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
        return new Variable(Val);
    }

    public override double Degree(Variable v)
    {
        if (!found && this == v)
            return 1;
        return 0;
    }

    public override Oper Add(Oper o)
    {
        if (Found && o.IsConstant)
            return new Variable(Val+o.Solution().Val);
        return base.Add(o);
    }
    public override Oper Subtract(Oper o)
    {
        if (Found && o.IsConstant)
            return new Variable(Val-o.Solution().Val);
        return base.Subtract(o);
    }
    public override Oper Mult(Oper o)
    {
        if (Found && o.IsConstant)
            return new Variable(Val*o.Solution().Val);
        return base.Mult(o);
    }
    public override Oper Divide(Oper o)
    {
        if (Found && o.IsConstant)
            return new Variable(Val/o.Solution().Val);
        return base.Divide(o);
    }

    

    public static Variable Undefined = new Variable("undefined");
}
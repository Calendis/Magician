namespace Magician.Symbols;

public class Variable : Oper
{
    protected override int identity { get => throw Scribe.Error("Undefined");}
    public bool found = false;
    Quantity q = new Quantity(0);
    public double Val
    {
        get
        {
            if (!found)
                throw Scribe.Error($"Variable {name} is unknown");
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
        {
            return this;
        }
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
        throw Scribe.Error("Variable cannot store arguments. Pass the variable or use Notate.Val to represent a constant");
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

    public override Fraction Divide(params Oper[] os)
    {
        if (os.Length == 1 && ((os[0] == this)) || (os[0].Contains(this) && os[0].posArgs.Count == 1 && os[0].negArgs.Count == 0))
            return new();
        return base.Divide(os);
    }
}
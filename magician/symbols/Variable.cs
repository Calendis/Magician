namespace Magician.Symbols;

public class Variable : Oper
{
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

    public Variable(string n) : base(n, new Oper[0])
    {

    }
    public Variable(string n, double v) : this(n)
    {
        Val = v;
    }
    public Variable(double v) : this($"constant({v})", v) { }

    public override Oper Copy()
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
    public override Oper New(List<Oper> a, List<Oper> b)
    {
        throw Scribe.Error("Variable cannot store arguments. Pass the variable or use Notate.Val to represent a constant");
    }

    public override Variable Solution()
    {
        throw Scribe.Issue("This should never occur");
    }

    public override double Degree(Variable v)
    {
        if (!found && this == v)
            return 1;
        return 0;
    }
}
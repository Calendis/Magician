using System.Diagnostics;

namespace Magician.Algo;

public abstract class Oper
{
    protected string name;
    public string Name => name;
    protected int numArgs;
    public int NumArgs
    {
        get => numArgs;
        set => numArgs = value;
    }
    //public int hasUnknownVars = 0;
    List<Variable> eventuallyContains = new();
    public bool Contains(Variable v) => eventuallyContains.Contains(v);
    protected int identity;
    public int Identity => identity;
    protected bool associative = false;
    protected bool commutative = false;
    protected bool invertable = true;

    public Oper[] args;
    protected Oper(string name, params Oper[] cstArgs)
    {
        this.name = name;
        numArgs = cstArgs.Length;
        args = cstArgs;

        foreach (Oper o in cstArgs)
        {
            if (o is Variable v)
            {
                if (!v.Found)
                {
                    eventuallyContains.Add(v);
                }
            }
            eventuallyContains = eventuallyContains.Union(o.eventuallyContains).ToList();
        }
    }

    public abstract Oper New(params Oper[] cstArgs);
    public abstract Oper Inverse(int argIndex);
    public virtual Variable Eval()
    {
        throw Scribe.Error("Operator is undefined");
    }
    public Oper DeepEval()
    {
        //foreach (Oper arg in args)
        for (int i = 0; i < args.Length; i++)
        {
            Oper arg = args[i];
            // Assume this variable is defined
            Oper o_ = arg is Variable v ? v.Eval() : arg.DeepEval();
            args[i] = o_;
        }
        //
        return this;
    }

    public virtual void Commute(int arg0, int arg1)
    {
        if (!commutative)
            throw Scribe.Error("Operator is not commutative");

        Oper temp = args[arg0];
        args[arg0] = args[arg1];
        args[arg1] = temp;
    }

    public virtual void Associate(int[] path0)
    {
        if (!associative)
            throw Scribe.Error("Operator is not associative");
        //Variable[] assocVars = AssociativeBlockMgr.
        Oper arg0 = this;
        Oper temp = arg0;
        int idx0 = -1;
        foreach (int i in path0)
        {
            arg0 = arg0.args[i];
            if (arg0.args.Length == 0)
            {
                idx0 = i;
                arg0 = temp;
                List<Oper> newSubArgs = temp.args.ToList();
                newSubArgs.RemoveAt(idx0);
                temp.args = newSubArgs.ToArray();
                break;
            }
            if (name != arg0.name)
                throw Scribe.Error("Path 1 breaches associative bounds");
        }
        List<Oper> newArgs = args.ToList();
        newArgs.Add(temp.args[idx0]);
        args = newArgs.ToArray();
    }
    /*                            */

    // Recursively gather all variables, constants, and operators in an Oper
    public void CollectOpers(ref List<Oper> varBasket, ref List<int> layerBasket, ref int knowns, ref int unknowns, int counter = 0)
    {
        // This will occur because each time "x" appears in the equation, it refers to a sole instance...
        // ... of Variable with the name "x"
        bool likeTermSeen = varBasket.Contains(this);
        if (this is Variable v)
        {
            // Handles constants
            if (v.found)
            {
                knowns++;
            }
            // Handles free variables
            else if (!likeTermSeen)
            {
                unknowns++;
            }
            // Track all variables in the basket
            varBasket.Add((Variable)this);
            layerBasket.Add(counter);
            return;
        }
        varBasket.Add(this);
        layerBasket.Add(counter);
        // Collect opers recursively
        foreach (Oper o in args)
        {
            o.CollectOpers(ref varBasket, ref layerBasket, ref knowns, ref unknowns, counter + 1);
        }
    }

    public virtual Oper Copy()
    {
        return New(args.Select((o, i) => o.Copy()).ToArray());
    }
}

public class Variable : Oper
{
    public bool found = false;
    Quantity foundVal = new Quantity(0);
    public double Val
    {
        get
        {
            if (!found)
                throw Scribe.Error($"Variable {name} is undefined");
            return foundVal.Evaluate();
        }
        set
        {
            foundVal.Set(value);
            found = true;
        }
    }

    public bool Found
    {
        get => found;
    }
    public Variable(string n) : base(n, new Oper[0])
    {
        // IMPORTANT: A variable has numArgs=1, but args.Length = 0
        //            This is the only case where these two values will not match
        numArgs = 1;
    }
    public Variable(string n, double v) : this(n)
    {
        Val = v;
    }
    public Variable(double v) : this($"constant({v})", v) { }

    public override string ToString()
    {
        //return $"Variable({(found ? Val : name)})";
        return found ? Val.ToString() : name;
    }

    public override Oper New(params Oper[] cstArgs)
    {
        throw Scribe.Issue("This should never occur");
    }
    public override Oper Inverse(int argIndex)
    {
        throw Scribe.Issue("This should never occur");
    }

    public override Oper Copy()
    {
        if (!found)
        {
            return this;
        }
        return new Variable(Val);
    }
}

// Minus produces an alternating sum
public class SumDiff : Oper
{
    public SumDiff(params Oper[] ops) : base("sumdiff", ops)
    {
        identity = 0;
    }
    public override Variable Eval()
    {
        double alternSum = 0;
        int count = 0;
        foreach (Variable v in args)
        {
            alternSum += v.Val * count++ % 2 == 0 ? 1 : -1;
        }
        return new Variable(alternSum);
    }

    public override Oper New(params Oper[] cstArgs)
    {
        return new SumDiff(cstArgs);
    }

    public override SumDiff Inverse(int argIndex)
    {
        SumDiff inverse = new SumDiff(args);
        inverse.args[argIndex] = new Variable(identity);
        if (argIndex % 2 == 0)
        {
            inverse.args = new Oper[] { new Variable(identity) }.Concat(inverse.args).ToArray();
            inverse.numArgs = inverse.args.Length;
        }
        return inverse;
    }

    public override string ToString()
    {
        string argsSigns = $"{string.Join("", args.Select((a, i) => i % 2 == 0 ? $"+{a.ToString()}" : $"-{a.ToString()}"))}";
        return "(" + string.Join("", argsSigns.Skip(1)) + ")";
    }

}

// Alternating reciprocal
public class Fraction : Oper
{
    public Fraction(params Oper[] ops) : base("fraction", ops)
    {
        identity = 1;
    }

    public override Variable Eval()
    {
        double quo = 1;
        int count = 0;
        foreach (Variable v in args)
        {
            quo = count++ % 2 == 0 ? quo * v.Val : quo / v.Val;
        }
        return new Variable(quo);
    }

    public override Oper New(params Oper[] cstArgs)
    {
        return new Fraction(cstArgs);
    }

    public override Fraction Inverse(int argIndex)
    {
        Fraction inverse = new Fraction(args);
        inverse.args[argIndex] = new Variable(identity);
        if (argIndex % 2 == 0)
        {
            inverse.args = new Oper[] { new Variable(identity) }.Concat(inverse.args).ToArray();
            inverse.numArgs = inverse.args.Length;
        }
        return inverse;
    }

    public override string ToString()
    {
        string numerator = "";
        string denominator = "";
        string[] frs = new[] {numerator, denominator};

        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] is Variable v)
            {
                if (v.Found)
                {
                    frs[i%2] += "*";
                }
            }
            else
            {
                frs[i%2] += "*";
            }
            frs[i%2] += args[i].ToString();
        }

        return $"({frs[0].TrimStart('*')} / {frs[1].TrimStart('*')})";
        //return $"Fraction({string.Join(", ", args.Select(x => x.ToString()))})";
    }
}
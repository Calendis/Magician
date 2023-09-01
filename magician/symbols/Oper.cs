using System.Diagnostics;

namespace Magician.Symbols;

public abstract class Oper
{
    public Oper[] args;
    public abstract double Degree(Variable v);
    public string Name => name;
    public int NumArgs
    {
        get => numArgs;
        set => numArgs = value;
    }
    public List<Variable> eventuallyContains = new();
    public bool Contains(Variable v) => eventuallyContains.Contains(v);

    protected string name;
    protected int numArgs;
    protected int identity = -999;
    protected bool associative = false;
    protected bool commutative = false;
    protected bool invertable = true;
    protected bool cancelled = false;

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
    public abstract Variable Solution();
    public Oper Optimized()
    {
        int counter = 0;
        List<Oper> newArgs = new();
        foreach (Oper o in args)
        {
            args[counter] = o.Simplified();
            counter++;
        }
        return this;
    }
    public virtual Oper Simplified()
    {
        return this;
    }


    public void PrependIdentity()
    {
        args = new Oper[] { new Variable(identity) }.Concat(args).ToArray();
        numArgs = args.Length;
    }

    public void AppendIdentity()
    {
        args = args.Concat(new Oper[] { new Variable(identity) }).ToArray();
        numArgs = args.Length;
    }

    public virtual void Commute(int arg0, int arg1)
    {
        if (!commutative)
            throw Scribe.Error("Operator is not commutative");

        (args[arg1], args[arg0]) = (args[arg0], args[arg1]);
    }

    // Format arguments into the required alternating form
    static Oper[] AssembleArgs(List<(Oper, bool)> flaggedArgs, int id)
    {
        List<Oper> positiveArgs = new();
        List<Oper> negativeArgs = new();
        foreach ((Oper, bool) ob in flaggedArgs)
        {
            if (ob.Item2)
                negativeArgs.Add(ob.Item1);
            else
                positiveArgs.Add(ob.Item1);
        }
        int numAssociated = Math.Max(positiveArgs.Count, negativeArgs.Count) * 2;
        List<Oper> finalArgs = new();
        for (int i = 0; i < numAssociated; i ++)
        {
            (Oper, Oper) invPair = (Notate.Val(id), Notate.Val(id));
            if (i < positiveArgs.Count)
                invPair.Item1 = positiveArgs[i];
            if (i < negativeArgs.Count)
                invPair.Item2 = negativeArgs[i];

            finalArgs.Add(invPair.Item1);
            finalArgs.Add(invPair.Item2);

        }

        return finalArgs.ToArray();
    }

    // Find all associated arguments and add return them as an array
    // true means inverted
    public Oper[] Associate<T>(List<(Oper, bool)>? associatedArguments = null) where T : Oper
    {
        associatedArguments ??= new();
        bool switcher = false;
        foreach (Oper o in args)
        {
            if (o is T associativeOperation)
            {
                associativeOperation.Associate<T>(associatedArguments);
            }
            else
            {
                associatedArguments.Add((o, switcher));
            }
            switcher = !switcher;
        }
        return AssembleArgs(associatedArguments, identity);
    }


    // Recursively gather all variables, constants, and operators in an Oper
    public void CollectOpers(
        ref List<Oper> varBasket,
        ref List<int> layerBasket,
        ref int knowns,
        ref int unknowns,
        int counter = 0)
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
        // IMPORTANT: A variable has numArgs=1, but args.Length = 0
        //            This is the only case where these two values will not match
        numArgs = 1;
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
    public override Oper New(params Oper[] cstArgs)
    {
        throw Scribe.Issue("This should never occur");
    }
    public override Oper Inverse(int argIndex)
    {
        throw Scribe.Issue("This should never occur");
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

// Alternating sum to reprsent addition and subtraction
public class SumDiff : Oper
{
    public SumDiff(params Oper[] ops) : base("sumdiff", ops)
    {
        identity = 0;
    }
    public override Variable Solution()
    {
        double alternSum = 0;
        int count = 0;
        foreach (Oper o in args)
        {
            if (o is Variable v)
            {
                alternSum += v.Val * ((count % 2 == 0) ? 1 : -1);
            }
            else
            {
                alternSum += o.Solution().Val * ((count % 2 == 0) ? 1 : -1);
            }
            count++;
        }
        return new Variable(alternSum);
    }

    public override SumDiff New(params Oper[] cstArgs)
    {
        return new SumDiff(cstArgs.Select(a => a.Copy()).ToArray());
    }

    public override SumDiff Inverse(int argIndex)
    {
        SumDiff inverse = new(args);
        inverse.args[argIndex] = new Variable(identity);
        inverse.PrependIdentity();
        return inverse;
    }

    public override double Degree(Variable v)
    {
        return args.Select<Oper, double>(o => o.Degree(v)).Max();
    }

    // TODO: rewrite this Optimization
    public override SumDiff Simplified()
    {
        //args = Associate<SumDiff>();

        // Combine constants
        //

        // Combine like variables
        //
        return this;
    }
    public SumDiff OldOptimized()
    {
        // A sumdiff can always be expressed with a maximum of 2+n arguments, where n is the number of unknowns
        double total = 0;
        int counter = 0;
        Dictionary<Oper, int> unknownsAndOpers = new();
        foreach (Oper o in args)
        {
            if (o is Variable v)
            {
                if (v.Found)
                {
                    total += v.Val * ((counter % 2 == 0) ? 1 : -1);
                }
                else
                {
                    if (unknownsAndOpers.ContainsKey(v))
                    {
                        unknownsAndOpers[v] += counter % 2 == 0 ? 1 : -1;
                    }
                    else
                    {
                        unknownsAndOpers.Add(v, counter % 2 == 0 ? 1 : -1);
                    }
                }
            }
            else
            {
                unknownsAndOpers.Add(o, counter % 2 == 0 ? 1 : -1);
            }
            counter++;
        }
        List<Oper> positiveTerms = new();
        List<Oper> negativeTerms = new();
        if (total < 0)
        {
            negativeTerms.Add(new Variable(-total));
        }
        else if (total > 0)
        {
            positiveTerms.Add(new Variable(total));
        }

        foreach ((Oper o, int i) in unknownsAndOpers)
        {
            Oper newOper;
            int absI = Math.Abs(i);
            if (absI == 0)
            {
                continue;
            }
            if (absI != 1)
            {
                newOper = new Fraction(new Variable(absI), new Variable(1), o);
            }
            else
            {
                newOper = o;
            }
            if (i < 0)
            {
                negativeTerms.Add(newOper);
            }
            else if (i > 0)
            {
                positiveTerms.Add(newOper);
            }
        }

        int imbalance = positiveTerms.Count - negativeTerms.Count;
        for (int i = 0; i < Math.Abs(imbalance); i++)
        {
            if (imbalance < 0)
            {
                positiveTerms.Add(new Variable(identity));
            }
            else
            {
                negativeTerms.Add(new Variable(identity));
            }
        }
        if (positiveTerms.Count != negativeTerms.Count)
        {
            throw Scribe.Issue("we broke it");
        }
        List<Oper> newArgs = new();
        foreach ((Oper pos, Oper neg) in Enumerable.Zip(positiveTerms, negativeTerms))
        {
            newArgs.Add(pos);
            newArgs.Add(neg);
        }
        while (newArgs.Last() is Variable v_ && v_.Found && v_.Val == identity)
        {
            newArgs.RemoveAt(newArgs.Count - 1);
        }

        return New(newArgs.ToArray());
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

    public override Variable Solution()
    {
        double quo = 1;
        int count = 0;
        foreach (Oper o in args)
        {
            if (o is Variable v)
            {
                quo = count % 2 == 0 ? quo * v.Val : quo / v.Val;
            }
            else
            {
                quo = count % 2 == 0 ? quo * o.Solution().Val : quo / o.Solution().Val;
            }
            count++;
        }
        return new Variable(quo);
    }

    public override Oper New(params Oper[] cstArgs)
    {
        return new Fraction(cstArgs.Select(a => a.Copy()).ToArray());
    }

    public override Fraction Inverse(int argIndex)
    {
        Fraction inverse = new Fraction(args);
        inverse.args[argIndex] = new Variable(identity);
        inverse.PrependIdentity();
        return inverse;
    }

    public override double Degree(Variable v)
    {
        return args.Select<Oper, double>((o, i) =>
        {
            if (i % 2 == 0)
                return o.Degree(v);
            else
                return -o.Degree(v);
        }).Sum();
    }

    public override string ToString()
    {
        string numerator = "";
        string denominator = "";
        string[] frs = new[] { numerator, denominator };

        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] is Variable v)
            {
                if (v.Found)
                {
                    frs[i % 2] += "*";
                }
            }
            else
            {
                frs[i % 2] += "*";
            }
            frs[i % 2] += args[i].ToString();
        }

        return $"({frs[0].TrimStart('*')} / {frs[1].TrimStart('*')})";
        //return $"Fraction({string.Join(", ", args.Select(x => x.ToString()))})";
    }
}
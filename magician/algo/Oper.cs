using System.Diagnostics;

namespace Magician.Algo;

public abstract class Oper
{
    protected string name = "DEFAULT OPERATOR";
    public string Name => name;
    protected int numArgs;
    public int NumArgs => numArgs;
    //public int hasUnknownVars = 0;
    List<Variable> eventuallyContains = new();
    public bool Contains(Variable v) => eventuallyContains.Contains(v);
    protected bool associative = false;
    protected bool commutative = false;
    protected bool invertable = true;

    public Oper[] args;
    protected Oper(params Oper[] cstArgs)
    {
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

    public abstract Oper New( params Oper[] cstArgs);
    public abstract Oper Inverse(Oper? v=null);

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
}
/* public static class AssociativeBlockMgr
{
    static Dictionary<Oper, int> assocBlockKeys = new Dictionary<Oper, int>();
    static List<Variable[]> assocBlocks = new List<Variable[]>();
    public Variable[] GetAssociativeBlock(Oper op)
    {
        // Check the cache
        if (assocBlockKeys.ContainsKey(op))
            return assocBlocks[assocBlockKeys[op]];

        List<Variable> newAssocBlock = new();
        List<Oper> newAssocBlockKeys = new(){op};

        // Recurse down the operator tree
        return

    }

    // Build a tree of those Opers which
    public static List<Variable> CollectArgs(Oper op)
    {
        List<Variable> variableBasket = new();
        if (op.args.Length == 0)
        {
            return variableBasket;
        }

        foreach (Oper opSub in op.args)
        {
            // Oper is a Variable
            if (opSub.args.Length == 0)
            {
                variableBasket.Add((Variable)opSub);
            }

            // Recurse
            if (opSub.GetType().Name == op.GetType().Name)
            {
                variableBasket.AddRange(CollectArgs(opSub));
            }
            // TODO: make sure this occurs
            else
            {
                Scribe.Info($"differing opers {opSub.GetType().Name} and {op.GetType().Name} do not associate");
            }
        }
        return variableBasket;
    }
} */

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
    public Variable(string n) : base(new Oper[0])
    {
        name = n;
        // IMPORTANT: A variable has numArgs=1, but args.Length = 0
        //            This is the only case where these two values will not match
        numArgs = 1;
    }
    public Variable(string n, double v) : this(n)
    {
        Val = v;
    }
    public Variable(double v) : this($"constant({v})", v) { }
    // public Oper Implicit() {}

    public override string ToString()
    {
        return $"Variable({(found ? Val : name)})";
    }

    public override Oper New(params Oper[] cstArgs)
    {
        throw Scribe.Issue("This should never occur.");
    }
    public override Oper Inverse(Oper? v = null)
    {
        throw Scribe.Issue("This should never occur");
    }
}

// Plus produces a sum
public class Plus : Oper
{
    public Plus(params Oper[] ops) : base(ops)
    {
        name = "plus";
        associative = true;
        commutative = true;
    }
    public override Variable Eval()
    {
        double sum = 0;
        foreach (Oper o in args)
        {
            if (o is Variable v)
            {
                sum += v.Val;
            }
            else
            {
                sum += ((Variable)o.Eval()).Val;
            }

        }
        return new Variable(sum);
    }
    public override Minus Inverse(Oper? v=null)
    {
        if (v == null)
        {
            return new Minus(args);
        }
        return new Minus(new Variable(0), new Plus(args.Where(v2 => v != v2).ToArray()));
    }

    public override Oper New(params Oper[] cstArgs)
    {
        return new Plus(cstArgs);
    }

    public override string ToString()
    {
        return $"Plus({string.Join(", ", args.Select(x => x.ToString()))})";
    }
}

// Minus produces an alternating sum
public class Minus : Oper
{
    public Minus(params Oper[] ops) : base(ops)
    {
        name = "minus";
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
        return new Minus(cstArgs);
    }

    public override Plus Inverse(Oper? v=null)
    {
        if (v == null)
        {
            return new Plus(args);
        }
        //return new Plus(args[1].args.Union(args.Where(v2 => v != v2)).ToArray());
        Scribe.Warn($"{this} needs to be inverted");
        Plus i = new Plus(args.Where(v2 => v != v2).ToArray());
        Scribe.Warn($"Got {i}");
        return i;
    }

    public override string ToString()
    {
        return $"Minus({string.Join(", ", args.Select(x => x.ToString()))})";
    }

}

public class Mult : Oper
{
    public Mult(params Oper[] ops) : base(ops)
    {
        name = "mult";
        associative = true;
        commutative = true;
    }

    public override Variable Eval()
    {
        double prod = 1;
        foreach (Variable v in args)
        {
            prod *= v.Val;
        }
        return new Variable(prod);
    }

    public override Oper New(params Oper[] cstArgs)
    {
        throw new NotImplementedException();
    }
    public override Oper Inverse(Oper? v = null)
    {
        throw Scribe.Issue("Reciprocal not yet implemented");
    }
}

public class Polynom : Oper
{
    public Polynom(params double[] coefficients)
    {
        //
    }

    public override Oper New(params Oper[] cstArgs)
    {
        throw new NotImplementedException();
    }
    public override Oper Inverse(Oper? v = null)
    {
        throw Scribe.Issue("Polynomial inversion not implemented");
    }
}
namespace Magician.Algo;

public class Oper
{
    protected string name = "DEFAULT OPERATOR";
    public string Name => name;
    protected int numArgs;
    public int NumArgs => numArgs;
    //public int hasUnknownVars = 0;
    protected bool associative = false;
    protected bool commutative = false;
    protected bool invertable = true;

    public Oper[] args;
    public Oper(params Oper[] cstArgs)
    {
        numArgs = cstArgs.Length;
        args = cstArgs;
        //foreach (Variable v in cstArgs)
        foreach (Oper o in cstArgs)
        {
            if (o is Variable v)
            {
                if (!v.Found)
                {
                    //Scribe.Warn("  variable is unknown");
                    //hasUnknownVars++;
                }
            }
            //Scribe.Warn($"Variable {v.name}");
        }
    }

    public Oper New(string name, params Oper[] cstArgs)
    {
        Oper o = new Oper(cstArgs);
        o.name = $"{name}_(built)";
        return o;
    }
    public virtual Oper Inverse(Variable? v=null)
    {
        throw Scribe.Error("Operator could not be inverted");
    }

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
        if (this is Variable v_)
        {
            // Handles constants
            if (v_.found)
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
        return $"{name}";
    }

}

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
    public override Oper Inverse(Variable? v=null)
    {
        if (v == null)
        {
            return new Minus(args);
        }
        return new Minus(args.OfType<Variable>().Where(v2 => v == v2).ToArray());
    }
}

public class Minus : Oper
{
    public Minus(params Oper[] ops) : base(ops)
    {
        name = "minus";
    }
    public override Variable Eval()
    {
        double negativeSum = 0;
        foreach (Variable v in args)
        {
            negativeSum -= v.Val;
        }
        return new Variable(negativeSum);
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
}

public class Polynom : Oper
{
    public Polynom(params double[] coefficients)
    {
        //
    }
}
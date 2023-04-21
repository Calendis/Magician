namespace Magician.Algo;

public class Oper
{
    protected string name = "DEFAULT OPERATOR";
    public int numArgs;
    public int hasUnknownVars = 0;
    protected bool associative = false;
    protected bool commutative = false;
    protected bool invertable = true;
    public Oper[] args;
    public Oper(params Oper[] cstArgs)
    {
        numArgs = cstArgs.Length;
        args = new Oper[numArgs];
        //foreach (Variable v in cstArgs)
        foreach (Oper o in cstArgs)
        {
            if (o is Variable v)
            {
                if (!v.Found)
                {
                    //Scribe.Warn("  variable is unknown");
                    hasUnknownVars++;
                }
            }
            //Scribe.Warn($"Variable {v.name}");
        }
    }

    public virtual Oper Eval()
    {
        throw Scribe.Error("Operator is undefined");
    }

    public virtual Oper Inverse()
    {
        throw Scribe.Error("Operator could not be inverted");
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
    bool found = false;
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
    public double Coefficient
    {
        get => foundVal.Evaluate();
        set => foundVal.Set(value);
    }
    public bool Found
    {
        get => found;
    }
    public Variable(string n) : base(new Oper[0])
    {
        name = n;
        numArgs = 1;
    }
    public Variable(string n, double v) : this(n)
    {
        Val = v;
    }
    public Variable(double v) : this("untitled", v) { }
    // public Oper Implicit() {}

}

public class Plus : Oper
{
    public Plus(params Oper[] ops) : base(ops)
    {
        name = "plus";
        associative = true;
        commutative = true;
    }
    public override Oper Eval()
    {
        double sum = 0;
        foreach (Variable v in args)
        {
            sum += v.Val;
        }
        return new Variable(sum);
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

    public override Oper Eval()
    {
        double prod = 1;
        foreach (Variable v in args)
        {
            prod *= v.Val;
        }
        return new Variable(prod);
    }
}
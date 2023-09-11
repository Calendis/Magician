namespace Magician.Symbols;
public abstract class Oper
{
    public Oper[] trueArgsOLD;
    public List<Oper> _oldArgs { get => posArgs.Concat(negArgs).ToList();}
    public List<Oper> posArgs = new();
    public List<Oper> negArgs = new();
    public abstract double Degree(Variable v);
    public string Name => name;
    public List<Variable> eventuallyContains = new();
    public bool Contains(Variable v) => eventuallyContains.Contains(v);

    protected string name;
    // TODO: fix bad design. Identity should be a double<T>
    protected int identity = -999;
    protected bool associative = false;
    protected bool commutative = false;
    protected bool invertable = true;
    protected bool cancelled = false;
    internal Oper? parent = null;

    // Alternating form
    protected Oper(string name, params Oper[] cstArgs) : this(name, cstArgs.Where((o, i) => i % 2 == 0).ToList(), cstArgs.Where((o, i) => i % 2 == 1).ToList())
    {
        this.name = name;
        //ArgsOLD = cstArgs;
    }

    protected Oper(string name, List<Oper> posa, List<Oper> nega)
    {
        posArgs = posa;
        negArgs = nega;
        foreach (Oper o in _oldArgs)
        {
           o.parent = this;
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

    public abstract Oper New(List<Oper> pa, List<Oper> na);

    public static (Oper, Oper) InvertEquationAround(Oper chosenSide, Oper oppositeSide, Oper axis)
    {
        Scribe.Warn($"Inverting {chosenSide} = {oppositeSide} around {axis}");
        bool capitalize = false;
        if (chosenSide.negArgs.Contains(axis) && !chosenSide.posArgs.Contains(axis))
        {
            capitalize = true;
            chosenSide.negArgs.Remove(axis);
        }
        else
        {
            chosenSide.posArgs.Remove(axis);
        }
        //(chosenSide.posArgs, chosenSide.negArgs) = (chosenSide.negArgs, chosenSide.posArgs);
        Oper newChosen = axis;
        Oper newOpposite;
        Scribe.Warn($"New chosen side is {newChosen}");
        Scribe.Warn($"Chosen is {chosenSide}");
        // Invert chosen
        (chosenSide.posArgs, chosenSide.negArgs) = (chosenSide.negArgs, chosenSide.posArgs);
        chosenSide.posArgs.Add(oppositeSide);
        // Invert it again
        if (capitalize)
            (chosenSide.posArgs, chosenSide.negArgs) = (chosenSide.negArgs, chosenSide.posArgs);
        //chosenSide.negArgs.AddRange(oppositeSide.negArgs);
        //if (capitalize)
        //    newOpposite = chosenSide.New(chosenSide.posArgs.Concat(oppositeSide.negArgs).ToList(), oppositeSide.posArgs.Concat(chosenSide.negArgs).ToList());
        //else
        //    newOpposite = chosenSide.New(oppositeSide.posArgs.Concat(chosenSide.negArgs).ToList(), chosenSide.posArgs.Concat(oppositeSide.negArgs).ToList());
        Scribe.Warn($"New opposite side is {chosenSide}");
        return (newChosen, chosenSide.Copy());

    }
    public abstract Variable Solution();
    public virtual void Simplify()
    {
    }

    // An Oper is considered a term when it:
    // 1.  Has no SumDiffs anywhere in the tree
    // and either
    // 2a. Is the root node
    // or
    // 2b. Has the root node as a parent, which must be a SumDiff
    public bool IsTerm
    {
        get
        {   
            bool noSumDiffs = false;
            bool rootOrSumDiffParentRoot = false;
            
            // Condition 2
            if (parent == null)
                rootOrSumDiffParentRoot = true;
            else if (parent.parent == null && parent is SumDiff)
                rootOrSumDiffParentRoot = true;

            // Condition 1
            foreach (Oper o in _oldArgs)
            {
                 if (o.Find<SumDiff>())
                 {
                    noSumDiffs = false;
                    break;
                 }
                 noSumDiffs = true;
            }

            return noSumDiffs && rootOrSumDiffParentRoot;
        }
    }

    public bool Find<T>() where T : Oper
    {
        if (this is T)
        {
            return true;
        }
        foreach (Oper o in _oldArgs)
        {
            if (o.Find<T>())
                return true;
        }
        return false;
    }

    public int Size(int? basket = null)
    {
        basket ??= 0;

        foreach (Oper o in _oldArgs)
        {
            basket += o.Size(basket);
        }

        return (int)basket;
    }

    public virtual void Commute(int arg0, int arg1)
    {
        if (!commutative)
            throw Scribe.Error("Operator is not commutative");

        (_oldArgs[arg1], _oldArgs[arg0]) = (_oldArgs[arg0], _oldArgs[arg1]);
    }

    // Format arguments into the required alternating form
    protected static Oper[] AssembleArgs(List<(Oper, bool)> flaggedArgs, int id)
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
        List<Oper> finalArgs = new();
        for (int i = 0; i < Math.Max(positiveArgs.Count, negativeArgs.Count); i++)
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
    public Oper[] Associate<T>(List<(Oper, bool)>? associatedArguments = null, bool? switcher = null) where T : Oper
    {
        associatedArguments ??= new();
        switcher ??= false;
        foreach (Oper o in _oldArgs)
        {
            if (o is T associativeOperation)
            {
                associativeOperation.Associate<T>(associatedArguments);
            }
            else
            {
                associatedArguments.Add((o, (bool)!switcher));
            }
            switcher = !switcher;
        }
        //Scribe.Info($"Associated args:");
        //Scribe.Info(AssembleArgs(associatedArguments, identity));
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
        foreach (Oper o in _oldArgs)
        {
            o.CollectOpers(ref varBasket, ref layerBasket, ref knowns, ref unknowns, counter + 1);
        }
    }

    // TODO: get rid of this method if possible
    public virtual Oper Copy()
    {
        return New(posArgs.Select((o, i) => o.Copy()).ToList(), negArgs.Select((o, i) => o.Copy()).ToList());
    }
}
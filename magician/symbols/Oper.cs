namespace Magician.Symbols;
public abstract partial class Oper : IArithmetic
{
    public List<Oper> AllArgs { get => posArgs.Concat(negArgs).ToList(); }
    public List<Oper> posArgs = new();
    public List<Oper> negArgs = new();
    public abstract double Degree(Variable v);
    public string Name => name;
    public bool cancelled = false;
    public List<Variable> eventuallyContains = new();
    public bool Contains(Variable v) => (this is Variable v_ && v_ == v) || eventuallyContains.Contains(v);

    public bool IsEmpty => AllArgs.Count == 0 && this is not Variable;
    protected string name;
    // TODO: make this a generic property
    protected abstract int identity {get;}
    protected bool associative = false;
    protected bool commutative = false;
    protected bool invertable = true;
    protected bool unaryNoOp = false;
    internal Oper? parent = null;

    // Alternating form
    protected Oper(string name, params Oper[] cstArgs) : this(name, cstArgs.Where((o, i) => i % 2 == 0).ToList(), cstArgs.Where((o, i) => i % 2 == 1).ToList())
    {
        if (this is not Variable && cstArgs.Length == 0)
        {
            posArgs.Add(new Variable(identity));
        }
    }

    protected Oper(string name, IEnumerable<Oper> posa, IEnumerable<Oper> nega)
    {
        this.name = name;
        posArgs = posa.ToList();
        negArgs = nega.ToList();
        foreach (Oper o in AllArgs)
        {
            o.parent = this;
            if (o is Variable v && !v.Found)
                eventuallyContains.Add(v);
            eventuallyContains = eventuallyContains.Union(o.eventuallyContains).ToList();
        }
    }

    public bool Like(Oper o)
    {
        if (o.unaryNoOp && o.posArgs.Count == 1 && o.negArgs.Count == 0)
            return Like(o.posArgs[0]);

        if (o.GetType() != GetType())
            return false;

        if (AllArgs.Count != o.AllArgs.Count)
            return false;

        if (this is Variable v && o is Variable u)
        {
            if (v.Found && u.Found)
                return v.Val == u.Val;
            else
                return v == u;
        }

        for (int i = 0; i < AllArgs.Count; i++)
        {
            if (!o.AllArgs[i].Like(AllArgs[i]))
                return false;
        }

        return true;
    }

    public abstract Variable Solution();
    public void Simplify(Variable v, Oper? parent = null)
    {
        // Brackets first, recursively
        foreach (Oper o in AllArgs)
            o.Simplify(v, this);
        Reduce(v);

        // Drop identities
        posArgs = posArgs.Where(o => !(o is Variable v && v.Found && v.Val == identity)).ToList();
        if (commutative)
            negArgs = negArgs.Where(o => !(o is Variable v && v.Found && v.Val == identity)).ToList();

        // Absorb trivial Opers
        if (parent is not null)
        {
            if (unaryNoOp && posArgs.Count == 1 && negArgs.Count == 0)
            {
                if (parent!.negArgs.Contains(this))
                {
                    parent.negArgs.Remove(this);
                    parent.negArgs.AddRange(posArgs);
                }
                else if (parent.posArgs.Contains(this))
                {
                    parent.posArgs.Remove(this);
                    parent.posArgs.AddRange(posArgs);
                }
            }
            if (IsEmpty)
            {
                if (parent!.negArgs.Contains(this))
                {
                    parent.negArgs.Remove(this);
                    parent.negArgs.Add(new Variable(identity));
                }
                else if (parent.posArgs.Contains(this))
                {
                    parent.posArgs.Remove(this);
                    parent.posArgs.Add(new Variable(identity));
                }
            }
        }
    }

    public virtual void Reduce(Variable axis) { }

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
            foreach (Oper o in AllArgs)
            {
                if (o.CheckFor<SumDiff>())
                {
                    noSumDiffs = false;
                    break;
                }
                noSumDiffs = true;
            }

            return noSumDiffs && rootOrSumDiffParentRoot;
        }
    }
    public abstract Oper New(IEnumerable<Oper> pa, IEnumerable<Oper> na);

    public bool CheckFor<T>() where T : Oper
    {
        if (this is T)
        {
            return true;
        }
        foreach (Oper o in AllArgs)
        {
            if (o.CheckFor<T>())
                return true;
        }
        return false;
    }

    // Return the coordinates of the shallowest match of a given Oper in the tree, (-1, -1) if not found
    public (int, int) Locate(Oper target, int depth = 0)
    {
        int x = 0;
        foreach (Oper o in AllArgs)
        {
            if (o == target)
            {
                return (x, depth);
            }
            else
            {
                (int, int) location = o.Locate(target, depth + 1);
                if (location.Item1 == -1 && location.Item2 == -1)
                {
                    return location;
                }
            }
            x++;
        }
        return (-1, -1);
    }

    // Write this if necessary
    //public List<(int, int)> LocateAll

    public void Associate(Oper? parent = null)
    {
        foreach (Oper o in AllArgs)
            o.Associate(this);

        if (!associative)
            return;

        if (parent is not null)
        {
            if (parent.GetType() == GetType())
            {
                if (parent.posArgs.Contains(this))
                {
                    parent.posArgs.Remove(this);
                    parent.posArgs.AddRange(posArgs);
                    parent.negArgs.AddRange(negArgs);
                }
                else if (parent.negArgs.Contains(this))
                {
                    parent.negArgs.Remove(this);
                    parent.posArgs.AddRange(negArgs);
                    parent.negArgs.AddRange(posArgs);
                }
            }
        }
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
        foreach (Oper o in AllArgs)
        {
            o.CollectOpers(ref varBasket, ref layerBasket, ref knowns, ref unknowns, counter + 1);
        }
    }

    public virtual Oper Copy()
    {
        return New(posArgs.Select((o, i) => o.Copy()).ToList(), negArgs.Select((o, i) => o.Copy()).ToList());
    }

    public virtual SumDiff Add(params Oper[] os)
    {
        return new SumDiff(os.Where(o => o is not Variable v || !v.Found || v.Val != 0).Append(this), new List<Oper>() { });
    }
    public virtual SumDiff Subtract(params Oper[] os)
    {
        return new SumDiff(new List<Oper>() { this }, os.Where(o => o is not Variable v || !v.Found || v.Val != 0));
    }
    public virtual Fraction Mult(params Oper[] os)
    {
        return new Fraction(os.Where(o => o is not Variable v || !v.Found || v.Val != 1).Append(this), new List<Oper>() { });
    }
    public virtual Fraction Divide(params Oper[] os)
    {
        return new Fraction(new List<Oper>() { this }, os.Where(o => o is not Variable v || !v.Found || v.Val != 1));
    }
}

internal class OperCompare : IEqualityComparer<Oper>
{
    public bool Equals(Oper? x, Oper? y)
    {
        if (x is null || y is null)
            throw Scribe.Error("Null Oper comparison");
        if (x == y)
            return true;
        if (x.Like(y))
            return true;
        return false;
    }

    int IEqualityComparer<Oper>.GetHashCode(Oper obj)
    {
        return obj.GetHashCode();
    }
}
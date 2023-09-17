using System.Diagnostics.CodeAnalysis;

namespace Magician.Symbols;
public abstract class Oper
{
    public List<Oper> AllArgs { get => posArgs.Concat(negArgs).ToList(); }
    public List<Oper> posArgs = new();
    public List<Oper> negArgs = new();
    public abstract double Degree(Variable v);
    public string Name => name;
    public List<Variable> eventuallyContains = new();
    public bool Contains(Variable v) => eventuallyContains.Contains(v);

    public bool IsEmpty => AllArgs.Count == 0;
    protected string name;
    // TODO: fix bad design. Identity should be a double<T>
    protected int identity = -999;
    protected bool associative = false;
    protected bool commutative = false;
    protected bool invertable = true;
    /* protected bool Rational
    {
        get
        {
            if (this is Variable v)
            {
                if (!v.Found)
                    return true;
                // Floats are NOT rational
                else
                    return (int)v.Val == v.Val;
            }
            foreach (Oper o in AllArgs)
            {
                if (!o.Rational)
                {
                    return false;
                }
            }
            return true;
        }
    } */
    internal Oper? parent = null;

    // Alternating form
    protected Oper(string name, params Oper[] cstArgs) : this(name, cstArgs.Where((o, i) => i % 2 == 0).ToList(), cstArgs.Where((o, i) => i % 2 == 1).ToList())
    {
        if (this is not Variable && cstArgs.Length == 0)
        {
            posArgs.Add(new Variable(identity));
        }
    }

    protected Oper(string name, List<Oper> posa, List<Oper> nega)
    {
        this.name = name;
        posArgs = posa;
        negArgs = nega;
        foreach (Oper o in AllArgs)
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
        bool invertedAxis = false;
        if (chosenSide.negArgs.Contains(axis) && !chosenSide.posArgs.Contains(axis))
        {
            invertedAxis = true;
            chosenSide.negArgs.Remove(axis);
        }
        else
        {
            chosenSide.posArgs.Remove(axis);
        }
        Oper newChosen = axis;

        chosenSide.negArgs.Add(oppositeSide);
        // Invert if the axis wasn't already inverted
        if (!invertedAxis)
            (chosenSide.posArgs, chosenSide.negArgs) = (chosenSide.negArgs, chosenSide.posArgs);

        return (newChosen, chosenSide.Copy());

    }

    public static (Oper, Oper) ExtractOperFrom(Oper chosenSide, Oper oppositeSide, Oper axis)
    {
        Oper newOpposite;
        if (chosenSide.posArgs.Contains(axis))
        {
            chosenSide.posArgs.Remove(axis);
            newOpposite = chosenSide.New(new() { oppositeSide }, new() { axis });
        }
        else if (chosenSide.negArgs.Contains(axis))
        {
            chosenSide.negArgs.Remove(axis);
            newOpposite = chosenSide.New(new() { oppositeSide, axis }, new() { });

        }
        else
        {
            throw Scribe.Issue($"Could not extract {axis} from {chosenSide}");
        }
        return (chosenSide, newOpposite);
    }

    public bool Like(Oper o)
    {
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

    public Fraction ByCoefficient()
    {
        if (this is Fraction f)
        {
            return f;
        }
        else
            return new Fraction(this, new Variable(1));
    }
    public abstract Variable Solution();
    // Simplify in BEDMAS order, in terms of v
    public void Simplify(Variable v)
    {
        // Brackets first, recursively
        foreach (Oper o in AllArgs)
            o.Simplify(v);

        Reduce(v);
    }

    // Trivial reduction. Drops identities from the positive arguments
    // Reduce does not necessarily need to use the axis variable, but it may
    public virtual void Reduce(Variable axis)
    {
        posArgs = posArgs.Where(o => !(o is Variable v && v.Found && v.Val == identity)).ToList();
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

    // TODO: get rid of this method if possible
    public virtual Oper Copy()
    {
        return New(posArgs.Select((o, i) => o.Copy()).ToList(), negArgs.Select((o, i) => o.Copy()).ToList());
    }
}

internal class OperCompare : IEqualityComparer<Oper>
{
    public bool Equals(Oper? x, Oper? y)
    {
        if (x is null || y is null)
            throw Scribe.Error("Null Oper comparison");
        if (x.Like(y))
            return true;
        return false;
    }

    public int GetHashCode([DisallowNull] Oper obj)
    {
        throw new NotImplementedException();
    }
}
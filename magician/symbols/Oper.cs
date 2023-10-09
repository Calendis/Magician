namespace Magician.Symbols;
public abstract partial class Oper : IArithmetic
{
    public List<Oper> AllArgs { get => posArgs.Concat(negArgs).ToList(); }
    public List<Oper> posArgs = new();
    public List<Oper> negArgs = new();
    public string Name => name;
    public bool cancelled = false;

    public bool IsEmpty => AllArgs.Count == 0 && this is not Variable;
    public bool Contains(Oper o) => this == o || AllArgs.Contains(o);
    protected string name;
    // TODO: make this a generic property
    protected abstract int identity { get; }
    protected bool associative = false;
    protected bool commutative = false;
    protected bool invertable = true;
    protected bool posUnaryIdentity = false;
    //internal Oper? parent = null;

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
    }

    public abstract double Degree(Variable v);
    public bool Like(Oper o)
    {
        if (o.posUnaryIdentity && o.posArgs.Count == 1 && o.negArgs.Count == 0)
            return o.posArgs[0].Like(this);

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

        List<Oper> args = AllArgs;
        for (int i = 0; i < AllArgs.Count; i++)
        {
            if (!o.AllArgs[i].Like(args[i]))
                return false;
        }

        return true;
    }

    public string Ord(string? ord = null)
    {
        Dictionary<Type, (char neg, char pos)> typeHeaders = new()
        {
            { typeof(Variable), ('v', 'V') },
            { typeof(SumDiff),  ('-', '+') },
            { typeof(Fraction), ('/', '*') }
        };
        int totalHeaders = 1;
        int totalLeaves = 0;
        ord ??= "";

        // Assemble the ord string
        ord += typeHeaders[GetType()].pos;
        ord += AllArgs.Count.ToString();
        foreach (Oper p in posArgs)
        {
            ord += typeHeaders[p.GetType()].pos;
            ord += p.AllArgs.Count.ToString();
            totalHeaders += p.AllArgs.Count;
            if (p is Variable v)
            {
                string leafOrd;
                if (v.Found)
                    leafOrd = $"#{v.Val}#";
                else
                    leafOrd = $"${v.Name}$";
                totalLeaves += leafOrd.Length;
                ord += leafOrd;
            }
            else
                ord += p.Ord();
        }
        foreach (Oper n in negArgs)
        {
            ord += typeHeaders[n.GetType()].neg;
            ord += n.AllArgs.Count.ToString();
            totalHeaders += n.AllArgs.Count;
            if (n is Variable v)
            {
                string leafOrd;
                if (v.Found)
                    leafOrd = $"#{v.Val}#";
                else
                    leafOrd = $"${v.Name}$";
                totalLeaves += leafOrd.Length;
                ord += leafOrd;
            }
            else
                ord += n.Ord();
        }
        //Scribe.Info($"Total ord size of {this} is {ord.Length} bytes");
        return ord;
    }

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

    public void Commute()
    {
        foreach (Oper o in AllArgs)
            o.Commute();

        if (!commutative)
            return;

        posArgs = posArgs.OrderBy(o => o.Ord()).ToList();
        negArgs = negArgs.OrderBy(o => o.Ord()).ToList();
    }

    public abstract Variable Solution();
    public void Simplify(Variable v, Oper? parent = null)
    {
        // Brackets first, recursively
        foreach (Oper o in AllArgs)
            o.Simplify(v, this);
        Reduce(v);
        BaseReduce(parent);
    }

    public virtual void Reduce(Variable axis){}
    public void BaseReduce(Oper? parent)
    {
        // Drop identities
        posArgs = posArgs.Where(o => !o.IsIdentity(identity)).ToList();
        if (commutative)
            negArgs = negArgs.Where(o => !o.IsIdentity(identity)).ToList();

        if (parent is null)
            return;

        // Absorb trivial
        if (posUnaryIdentity && posArgs.Count == 1 && negArgs.Count == 0)
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
        if (IsEmpty && this is not Variable)
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

    public Oper Trim()
    {
        if (posUnaryIdentity && posArgs.Count == 1 && negArgs.Count == 0)
            return posArgs[0];
        return this;
    }
    public bool IsIdentity(int id)
    {
        if (this is Variable v)
            return v.Found && v.Val == id;
        if (IsEmpty)
            return true;
        return false;
    }

    // An Oper is considered a Term when it:
    // 1.  Has no SumDiffs anywhere in the tree
    // and either
    // 2a. Is the root node
    // or
    // 2b. Has the root node as a parent, which must be a SumDiff
    /* public bool IsTerm(Oper? parent = null)
    {
        if (this is SumDiff && AllArgs.Count > 1)
            return false;

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
            if (this is SumDiff && AllArgs.Count > 1)
                noSumDiffs = false;

            return noSumDiffs && rootOrSumDiffParentRoot;
    } */
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

internal class OperLike : IEqualityComparer<Oper>
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
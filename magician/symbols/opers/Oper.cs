using System.Globalization;
using System.Numerics;
using Magician.Maps;

namespace Magician.Symbols;
public abstract partial class Oper : IFunction
{
    //public List<INum> AllArgs { get => posArgs.Concat(negArgs).ToList(); }
    //public List<INum> posArgs = new();
    //public List<INum> negArgs = new();
    public string Name => name;
    public bool cancelled = false;
    public List<Oper> posArgs = new();
    public List<Oper> negArgs = new();
    public List<Oper> AllArgs => posArgs.Concat(negArgs).ToList();

    public bool IsEmpty => AllArgs.Count == 0 && this is not Variable;
    public bool Contains(Oper o) => this == o || AllArgs.Contains(o);
    public bool PosContains(Oper o) => !Contains(o) ? throw Scribe.Error($"{o} not found in {this}") : posArgs.Contains(o);
    List<Variable> AssociatedVars;
    protected string name;
    // TODO: make this a generic property
    protected abstract int identity { get; }
    protected bool associative = false;
    protected bool commutative = false;
    public bool invertible = true;
    public bool unary = false;
    protected bool posUnaryIdentity = false;

    public int Ins { get; set; }
    public int Dimensions { get { return Ins + 1; } set { } }

    public bool IsConstant => this is Variable v && v.Found;

    protected Oper(string name, IEnumerable<Oper> posa, IEnumerable<Oper> nega)
    {
        this.name = name;
        posArgs = posa.ToList();
        negArgs = nega.ToList();
        OperLayers ol = new(this, Variable.Undefined);
        AssociatedVars = ol.GetInfo(0, 0).assocArgs.Distinct().ToList();
        //if (this is Variable v && !v.Found)
        //    AssociatedVars.Add(v);

        //map = new Func<double[], double>(vals => Evaluate(vals));
    }
    // Alternating form
    protected Oper(string name, params Oper[] cstArgs) : this(name, cstArgs.Where((o, i) => i % 2 == 0).ToList(), cstArgs.Where((o, i) => i % 2 == 1).ToList())
    {
        if (this is not Variable && cstArgs.Length == 0)
            posArgs.Add(new Variable(identity));
    }

    public double Evaluate(params double[] args)
    {
        HashSet<Variable> associates = AssociatedVars.Where(v => !v.Found).ToHashSet();
        if (associates.Count != args.Length)
            throw Scribe.Error($"{name} {this} expected {associates.Count} arguments, got {args.Length}");

        int counter = 0;
        foreach (Variable a in associates)
            a.Val = args[counter++];
        double s = Solution().Val;
        associates.ToList().ForEach(a => a.Reset());
        return s;
    }

    public abstract double Degree(Variable v);
    public bool Like(Oper o)
    {
        if (o.posUnaryIdentity && o.posArgs.Count == 1 && o.negArgs.Count == 0)
            return ((Oper)o.posArgs[0]).Like(this);

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
            if (!((Oper)o.AllArgs[i]).Like(args[i]))
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
        if (this is Variable u)
        {
            if (u.Found)
                ord += $"#{u.Val}#";
            else
                ord += $"${u.Name}$";
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

        posArgs = posArgs.OrderBy(o => ((Oper)o).Ord()).ToList();
        negArgs = negArgs.OrderBy(o => ((Oper)o).Ord()).ToList();
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

    public virtual void Reduce(Variable axis) { }
    private void BaseReduce(Oper? parent)
    {
        // Drop identities
        posArgs = posArgs.Where(o => !(o.IsConstant && ((Variable)o).Val == identity)).ToList();
        negArgs = negArgs.Where(o => !(o.IsConstant && ((Variable)o).Val == identity)).ToList();

        // Drop things found in both sets of arguments
        if (commutative)
        {
            Dictionary<string, Oper> selectedOpers = new();
            Dictionary<string, int> operCoefficients = new();
            foreach (Oper o in posArgs)
            {
                string ord = o.Ord();
                if (selectedOpers.ContainsKey(ord))
                {
                    operCoefficients[ord]++;
                }
                else
                {
                    selectedOpers.Add(ord, o);
                    operCoefficients.Add(ord, 1);
                }
            }
            foreach (Oper o in negArgs)
            {
                string ord = o.Ord();
                if (selectedOpers.ContainsKey(ord))
                {
                    operCoefficients[ord]--;
                }
                else
                {
                    selectedOpers.Add(ord, o);
                    operCoefficients.Add(ord, -1);
                }
            }
            posArgs.Clear();
            negArgs.Clear();
            foreach (string ord in operCoefficients.Keys)
            {
                int coefficient = operCoefficients[ord];
                Oper o = selectedOpers[ord];
                if (coefficient == 0)
                    continue;
                else if (coefficient > 0)
                {
                    while (coefficient-- > 0)
                        posArgs.Add(o.Copy());
                }
                else if (coefficient < 0)
                {
                    while (coefficient++ < 0)
                        negArgs.Add(o.Copy());
                }
            }
        }

        // Make identities explicit
        if (posArgs.Count == 0 && this is not Variable)
            posArgs.Add(New(new List<Oper>{Notate.Val(identity)}, new List<Oper>{}));

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
            return (Oper)posArgs[0];
        return this;
    }

    public (List<List<(int, int, bool, bool)>>, List<List<(Oper, bool)>>) FlaggedHandshakes(Variable axis)
    {

        List<(Oper, bool)> argsContainingAxis = new();
        List<(Oper, bool)> argsNotContainingAxis = new();
        List<List<(int, int, bool, bool)>> groupedHandshakes = new() { new() { }, new() { } };

        int posLimit = posArgs.Count;
        // Group the Opers into terms containing the axis, and terms not containing the axis
        for (int i = 0; i < AllArgs.Count; i++)
        {
            Oper o = AllArgs[i];
            if (o.Contains(axis))
                argsContainingAxis.Add((o, i < posLimit));
            else
                argsNotContainingAxis.Add((o, i < posLimit));
        }
        int c = 0;
        foreach (List<(Oper, bool parity)> flaggedArgs in new List<List<(Oper, bool)>>
            { argsNotContainingAxis, argsContainingAxis })
        {
            if (flaggedArgs.Count % 2 != 0)
                flaggedArgs.Add((new Variable(identity), true));
            for (int i = 0; i < flaggedArgs.Count; i++)
            {
                for (int j = i + 1; j < flaggedArgs.Count; j++)
                {
                    groupedHandshakes[c].Add((i, j, flaggedArgs[i].parity, flaggedArgs[j].parity));
                }
            }
            c++;
        }

        return (groupedHandshakes, new List<List<(Oper, bool)>>
            { argsNotContainingAxis, argsContainingAxis });
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

    public virtual Oper Copy()
    {
        return New(posArgs.Select((o, i) => ((Oper)o).Copy()).ToList(), negArgs.Select((o, i) => ((Oper)o).Copy()).ToList());
    }

    public virtual Oper Add(Oper o)
    {
        return new SumDiff(new List<Oper> { this, o }, new List<Oper> { });
        // Old params Oper[] implementation
        //return new SumDiff(os.Where(o => o is not Variable v || !v.Found || v.Val != 0).Append(this), new List<Oper>() { });
    }
    public virtual Oper Subtract(Oper o)
    {
        return new SumDiff(new List<Oper> { this }, new List<Oper> { o });
        //return new SumDiff(new List<Oper>() { this }, os.Where(o => o is not Variable v || !v.Found || v.Val != 0));
    }
    public virtual Oper Mult(Oper o)
    {
        return new Fraction(new List<Oper> { this, o }, new List<Oper> { });
        //return new Fraction(os.Where(o => o is not Variable v || !v.Found || v.Val != 1).Append(this), new List<Oper>() { });
    }
    public virtual Oper Divide(Oper o)
    {
        return new Fraction(new List<Oper> { this }, new List<Oper> { o });
        //return new Fraction(new List<Oper>() { this }, os.Where(o => o is not Variable v || !v.Found || v.Val != 1));
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
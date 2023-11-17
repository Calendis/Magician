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
    protected List<Variable> AssociatedVars = new();

    // TODO: Maybe put some of these behind an interface
    protected string name;
    protected virtual int? Identity { get; }
    protected bool associative = false;
    protected bool commutative = false;
    public bool invertible = true;
    public bool unary = false;
    protected bool posUnaryIdentity = false;

    public int Ins { get; set; }
    public int Dimensions { get { return Ins + 1; } set { } }

    // TODO: maybe implement Forms for these
    public bool IsConstant => this is Variable v && v.Found;
    public bool IsDetermined => IsConstant || (AssociatedVars.Count == 0 && this is not Variable);

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
    protected Oper(string name, params Oper[] cstArgs) : this(name, cstArgs.Where((o, i) => i % 2 == 0).ToList(), cstArgs.Where((o, i) => i % 2 == 1).ToList()) { }

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

    public abstract Oper Degree(Variable v);
    public Oper Degree()
    {
        if (IsDetermined)
            return new Variable(0);
        List<Oper> degs = AssociatedVars.Select(av => Degree(av)).ToList();
        Oper result = new SumDiff(degs, new List<Oper> { });
        result = Form.Canonical(result);
        if (result.IsDetermined)
            return result.Solution();
        return result;
    }

    public virtual void Combine(Variable axis) { }
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
        /* YOU MUST DEFINE A TYPE HEADER FOR EVERY CLASS THAT INHERITS OPER */
        Dictionary<Type, (char neg, char pos)> typeHeaders = new()
        {
            {typeof(Variable),  ('v', 'V')},
            {typeof(SumDiff),   ('-', '+')},
            {typeof(Fraction),  ('/', '*')},
            {typeof(Funcs.Abs), ('A', 'A')},
            {typeof(Funcs.Min), ('m', 'm')},
            {typeof(Funcs.Max), ('M', 'M')}
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

    // Recursively moves args into a canonical form
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
        Combine(v);
        Reduce(parent);
    }
    public virtual void Reduce(Oper? parent = null)
    {
        foreach (Oper a in AllArgs)
        {
            a.Reduce(this);
        }

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
        if (Identity is null)
            return;

        // Drop identities
        posArgs = posArgs.Where(o => !(o.IsConstant && ((Variable)o).Val == Identity)).ToList();
        negArgs = negArgs.Where(o => !(o.IsConstant && ((Variable)o).Val == Identity)).ToList();

        // Prefer explicit identity over empty args
        // TODO: account for the fact that this implementation will break ExpLogs, as they have no identity
        if (posArgs.Count == 0 && this is not Variable)
            posArgs.Add(New(new List<Oper> { Notate.Val((int)Identity) }, new List<Oper> { }));

        if (parent is null)
            return;

        // Destructive routine that uses Reduced instead of Reduce
        for (int i = 0; i < AllArgs.Count; i++)
        {
            AllArgs[i] = AllArgs[i].Reduced(this);
        }

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
                parent.negArgs.Add(new Variable((int)Identity));
            }
            else if (parent.posArgs.Contains(this))
            {
                parent.posArgs.Remove(this);
                parent.posArgs.Add(new Variable((int)Identity));
            }
        }
    }
    internal Oper Reduced(Oper? parent = null)
    {
        if (IsDetermined && this is not Variable)
            return Solution();
        if (parent is null)
        {
            Oper r = new SumDiff(this);
            r.Reduce();
            return r;
        }
        parent.Reduce();
        return parent;
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
            {
                argsContainingAxis.Add((o, i < posLimit));
                //Scribe.Info($"\t{o} does contain {axis}, degree {o.Degree(axis)}, total degree {o.Degree()}");
            }
            else
            {
                argsNotContainingAxis.Add((o, i < posLimit));
                //Scribe.Info($"\t{o} does not contain {axis}");
            }
        }
        int c = 0;
        foreach (List<(Oper, bool parity)> flaggedArgs in new List<List<(Oper, bool)>> { argsNotContainingAxis, argsContainingAxis })
        {
            if (Identity is null)
                throw Scribe.Issue($"TODO: support this case");
            if (flaggedArgs.Count % 2 != 0)
                flaggedArgs.Add((new Variable((int)Identity), true));
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

    // Deep-copies an Oper. Unknown variables always share an instance, however (see Variable.Copy)
    public virtual Oper Copy()
    {
        return New(posArgs.Select((o, i) => ((Oper)o).Copy()).ToList(), negArgs.Select((o, i) => ((Oper)o).Copy()).ToList());
    }

    /* Binary arithmetic methods */
    public virtual Oper Add(Oper o)
    {
        return new SumDiff(new List<Oper> { this, o }, new List<Oper> { });
    }
    public virtual Oper Subtract(Oper o)
    {
        return new SumDiff(new List<Oper> { this }, new List<Oper> { o });
    }
    public virtual Oper Mult(Oper o)
    {
        return new Fraction(new List<Oper> { this, o }, new List<Oper> { });
    }
    public virtual Oper Divide(Oper o)
    {
        return new Fraction(new List<Oper> { this }, new List<Oper> { o });
    }
    public virtual Oper Pow(Oper o)
    {
        return new PowRoot(new List<Oper> { this, o }, new List<Oper> { });
    }
    public virtual Oper Root(Oper o)
    {
        return new PowRoot(new List<Oper> { this }, new List<Oper> { o });
    }
    public virtual Oper Exp(Oper o)
    {
        return new ExpLog(new List<Oper> { this, o }, new List<Oper> { });
    }
    public virtual Oper Log(Oper o)
    {
        return new ExpLog(new List<Oper> { this }, new List<Oper> { o });
    }

    /*
    public int CompareTo(object? obj)
    {
        if (obj is null || obj is not Oper)
            throw Scribe.Error($"Cannot compare {this} with {obj}");
        Oper o = (Oper)obj;
        bool lessThan = IsDetermined ? !o.IsDetermined || Solution().Val < o.Solution().Val : !o.IsDetermined && this < (Oper)obj;
        if (lessThan)
            return -1;
        else if (Form.Canonical(Degree()).Like(Form.Canonical(((Oper)obj).Degree())))
            return 0;
        else
            return 1;
    }
    */

    public static bool operator <(Oper o0, Oper o1)
    {
        if (o0.IsDetermined)
        {
            if (o1.IsDetermined)
            {
                return o0.Solution().Val < o1.Solution().Val;
            }
            else
                return true;
        }
        else if (o1.IsDetermined)
            return false;
        //Scribe.Info($"{o0} < {o1} ?");
        Oper d0 = o0.Degree(); Oper d1 = o1.Degree();
        //d0.Reduce(); d1.Reduce();
        return d0 < d1;
    }
    public static bool operator >(Oper o0, Oper o1)
    {
        if (o0.IsDetermined)
        {
            if (o1.IsDetermined)
                return o0.Solution().Val > o1.Solution().Val;
            else
                return true;
        }
        else if (o1.IsDetermined)
            return false;
        return o0.Degree() > o1.Degree();
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
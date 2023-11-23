using Magician.Maps;

namespace Magician.Symbols;
public abstract partial class Oper : IFunction
{
    // TODO: Maybe put some of these fields/properties behind an interface
    public string Name => name;
    public List<Oper> posArgs = new();
    public List<Oper> negArgs = new();
    public List<Oper> AllArgs => posArgs.Concat(negArgs).ToList();
    public readonly List<Variable> AssociatedVars = new();

    protected readonly string name;
    protected bool associative = false;
    protected bool commutative = false;
    public bool unaryAssociative = true;
    public bool invertible = true;

    public int Ins { get; set; }
    // TODO: move to form or something
    public bool IsConstant => this is Variable v && v.Found;
    public bool IsDetermined {get {
        return IsConstant || (AssociatedVars.Count == 0 && this is not Variable);
        //try
        //{
        //    Solution();
        //    return true;
        //}
        //catch (Scribe.MagicianError)
        //{
        //    return false;
        //}
    }}
    public bool IsUnary => AllArgs.Count == 1 && posArgs.Count == 1;
    public bool IsTrivial => unaryAssociative && IsUnary;

    // Create a new Oper of the same type
    public abstract Oper New(IEnumerable<Oper> pa, IEnumerable<Oper> na);
    public abstract Variable Solution();
    public abstract Oper Degree(Variable v);

    protected Oper(string name, IEnumerable<Oper> posa, IEnumerable<Oper> nega)
    {
        this.name = name;
        posArgs = posa.ToList();
        negArgs = nega.ToList();
        /* Eventually we could optimize with something like */
        /*
            if (LayerCache.Locked)
                LayerCache.TryKry(this)  // the key would consist of am ord-address pair
                return;                  // break out early if we can't get access to the layer cache. the assumption
                                         // is that a the root constructor will successfully unlock the cache
                                         // and distribute AssociatedVar information
        */
        OperLayers ol = new(this, Variable.Undefined);
        // TODO: obfuscate this behind some string constant
        // This is a hack, but it works. Normally I would check the found property of the Variable, but this is the
        // base ctor, which is called before that property can be set. Another alternative might be to seperate
        // variables and constants into distinct classes
        if (!(this is Variable && new string(name.Take(8).ToArray()) == "constant"))
            AssociatedVars = ol.GetInfo(0, 0).assocArgs.Distinct().ToList();
    }
    // Alternating form ctor. Handy in some cases
    protected Oper(string name, params Oper[] cstArgs) : this(name, cstArgs.Where((o, i) => i % 2 == 0).ToList(), cstArgs.Where((o, i) => i % 2 == 1).ToList()) { }

    // Numeric value that has no effect when used as an argument
    protected virtual int? Identity { get; }
    // Deep-copies an Oper. Unknown variables always share an instance, however (see Variable.Copy)
    public virtual Oper Copy() { return New(posArgs.Select((o, i) => ((Oper)o).Copy()).ToList(), negArgs.Select((o, i) => ((Oper)o).Copy()).ToList()); }
    
    // Perform basic simplifications on this Oper
    public abstract void Reduce();
    // Perform basic simplifications on this Oper, recursively
    public void ReduceAll()
    {
        foreach (Oper a in AllArgs)
            a.ReduceAll();
        Reduce();
    }
    // Perform advanced simplifications on this Oper
    public virtual void Simplify(Variable? axis=null)
    {
        Associate();
    }
    // Perform advanced simplifications on this Oper, recursivelyz
    public void SimplifyAll(Variable? axis=null)
    {
        foreach (Oper a in AllArgs)
            a.SimplifyAll(axis);
        Simplify(axis);
    }
    // Perform advanced simplifications in this Oper as much as possible
    public void SimplifyFull(Variable? axis=null)
    {
        Oper prev;
        do 
        {
            prev = Copy();
            Simplify(axis);
        } while (!Like(prev));
    }
    // Perform advanced simplifications in this Oper as much as possible, recursively
    public void SimplifyFullAll(Variable? axis=null)
    {
        foreach (Oper a in AllArgs)
            a.SimplifyFullAll();
        SimplifyFull();
    }

    // Provide arguments to solve the expression at a point
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

    // Overall degree of the expression
    public Oper Degree()
    {
        if (IsDetermined)
            return new Variable(0);
        List<Oper> degs = AssociatedVars.Select(av => Degree(av)).ToList();
        Oper result = new SumDiff(degs, new List<Oper> { });
        result = LegacyForm.Canonical(result);
        if (result.IsDetermined)
            return result.Solution();
        return result;
    }

    // Recursively move branches up
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
        return new PowTowRootLog(new List<Oper> { this, o }, new List<Oper> { });
    }
    public virtual Oper Exp(Oper o)
    {
        return new PowTowRootLog(new List<Oper> { o, this }, new List<Oper> { });
    }
    public virtual Oper Root(Oper o)
    {
        return new PowTowRootLog(new List<Oper> { this }, new List<Oper> { o });
    }
    public virtual Oper Log(Oper o)
    {
        return new PowTowRootLog(new List<Oper> { this }, new List<Oper> { new Variable(1), o });
    }

    public virtual Fraction Factors()
    {
        return new Fraction(Copy());
    }

    public Oper CommonFactors(Oper o)
    {
        OperLike ol = new();
        Fraction facs = Factors();
        Fraction oFacs = o.Factors();
        
        List<Oper> commonPos = new();
        foreach (Oper fac in facs.posArgs)
            if (oFacs.posArgs.Contains(fac, ol))
                commonPos.Add(fac);
        List<Oper> commonNeg = new();
        foreach (Oper fac in facs.negArgs)
            if (oFacs.negArgs.Contains(fac, ol))
                commonNeg.Add(fac);
        if (commonPos.Count == 0)
            commonPos.Add(new Variable(1));
        return new Fraction(commonPos, commonNeg);
    }

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
            {typeof(PowTowRootLog),  ('R', '^')},
            {typeof(Funcs.Abs), ('|', '|')},
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
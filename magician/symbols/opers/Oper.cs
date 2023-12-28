using Magician.Maps;

namespace Magician.Symbols;
public abstract partial class Oper : IFunction
{
    // TODO: Maybe put some of these fields/properties behind an interface
    public string Name => name;
    public List<Oper> posArgs = new();
    public List<Oper> negArgs = new();
    public List<Oper> AllArgs => posArgs.Concat(negArgs).ToList();
    public List<Variable> AssociatedVars = new();

    protected readonly string name;
    protected bool associative = false;
    protected bool commutative = false;
    public bool trivialAssociative = true;
    public bool invertible = true;

    public int Ins { get; set; }
    public bool IsConstant => this is Variable v && v.Found;
    public bool IsDetermined
    {
        get
        {
            return IsConstant || (AssociatedVars.Count == 0 && this is not Variable);
        }
    }
    public bool IsUnary => AllArgs.Count == 1 && posArgs.Count == 1;
    public bool IsTrivial => trivialAssociative && IsUnary;

    // Create a new Oper of the same type
    public abstract Oper New(IEnumerable<Oper> pa, IEnumerable<Oper> na);
    public abstract Variable Solution();
    public abstract Oper Degree(Oper v);

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
        // base ctor, which is called before that property can be set
        if (!(this is Variable && new string(name.Take(8).ToArray()) == "constant"))
            AssociatedVars = ol.GetInfo(0, 0).assocArgs.Distinct().ToList();
        //AssociatedVars.Sort((v0, v1) => v0.Name[0] < v1.Name[0] ? 1 : v0.Name[0] > v1.Name[0] ? -1 : 0);
    }
    // Alternating form ctor. Handy in some cases
    protected Oper(string name, params Oper[] cstArgs) : this(name, cstArgs.Where((o, i) => i % 2 == 0).ToList(), cstArgs.Where((o, i) => i % 2 == 1).ToList()) { }

    // Numeric value that has no effect when used as an argument
    protected virtual int? Identity { get; }
    // Deep-copies an Oper. Unknown variables always share an instance, however (see Variable.Copy)
    public virtual Oper Copy() { return New(posArgs.Select((o, i) => ((Oper)o).Copy()).ToList(), negArgs.Select((o, i) => ((Oper)o).Copy()).ToList()); }

    // Perform basic simplifications on this Oper
    public abstract void ReduceOuter();
    // Perform basic simplifications on this Oper, recursively
    /* TODO: eventually make the void simplification/reduction methods internal. */
    /*       They modify state in a way that's safe inside the algebra solver,   */
    /*       but not necessarily otherwise. Returning a new Oper is always safe. */
    public void Reduce(int depth=-1)
    {
        if (depth == 0)
            return;
        if (depth > 0)
            depth --;
        Associate();
        foreach (Oper a in AllArgs)
            a.Reduce(depth);
        ReduceOuter();
    }
    // Perform advanced simplifications on this Oper
    internal virtual void SimplifyOnceOuter(Variable? axis = null) { ReduceOuter(); }
    // Perform advanced simplifications on this Oper, recursivel
    public void SimplifyOnce(Variable? axis = null, int depth=1)
    {
        if (depth == 9)
            return;
        if (depth > 0)
            depth--;
        foreach (Oper a in AllArgs)
            a.SimplifyOnce(axis, depth);
        SimplifyOnceOuter(axis);
    }
    // Perform advanced simplifications in this Oper as much as possible
    public void Simplify(Variable? axis = null, int bailout=9999)
    {
        int iters = 0;
        Oper prev;
        do
        {
            prev = Copy();
            SimplifyOnceOuter(axis);
            if (++iters >= bailout)
            {
                Scribe.Warn("Bailed out after {iters} simplifications");
                break;
            }
        } while (!Like(prev));
    }
    // Perform advanced simplifications in this Oper as much as possible, recursively
    public void SimplifyAll(Variable? axis = null)
    {
        foreach (Oper a in AllArgs)
            a.SimplifyAll(axis);
        Simplify(axis);
    }

    // Provide arguments to solve the expression at a point
    public double Evaluate(params double[] args)
    {
        HashSet<Variable> associates = AssociatedVars.Where(v => !v.Found).ToHashSet();
        if (associates.Count != args.Length)
            throw Scribe.Error($"{name} {this} expected {associates.Count} arguments, got {args.Length}");

        int counter = 0;
        foreach (Variable a in associates.OrderBy(v => v.Name))
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
        // Absorb trivial
        for (int i = 0; i < posArgs.Count; i++)
            posArgs[i] = LegacyForm.Shed(posArgs[i]);
        for (int i = 0; i < negArgs.Count; i++)
            negArgs[i] = LegacyForm.Shed(negArgs[i]);
        
        foreach (Oper o in AllArgs)
            o.Associate(this);

        if (!associative)
            return;

        if (parent is not null)
        {
            // Associate if types match
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
        if (Like(o))
            return new Variable(1);
        return new Fraction(new List<Oper> { this }, new List<Oper> { o });
    }
    public virtual PowTowRootLog Pow(Oper o)
    {
        return new PowTowRootLog(new List<Oper> { this, o }, new List<Oper> { });
    }
    public virtual PowTowRootLog Exp(Oper o)
    {
        return new PowTowRootLog(new List<Oper> { o, this }, new List<Oper> { });
    }
    public virtual Oper Root(Oper o)
    {
        return new PowTowRootLog(new List<Oper> { this , new Fraction(Notate.Val(1), o)}, new List<Oper> {});
    }
    public virtual Oper Log(Oper o)
    {
        return new PowTowRootLog(new List<Oper> { this }, new List<Oper> {o});
    }

    // TODO: in the future, this could have a type form like Fraction(n->PowTowRootLog, n->PowTowRootLog)
    public virtual Fraction Factors(){return new Fraction(new PowTowRootLog(new List<Oper>{Copy(), new Variable(1)}, new List<Oper>{}));}

    public Fraction CommonFactors(Oper o)
    {
        //Scribe.Info($"\t\tCommon factors of {this} and {o}...");
        if (Like(o))
            if (this is Fraction)
                return (Fraction)Copy();
            else
                return new Fraction(Copy());
        OperLike ol = new();
        Fraction facs0 = Factors();
        Fraction facs1 = o.Factors();
        List<Oper> basesPos = new();
        List<Oper> basesNeg = new();

        //Scribe.Info($"\t\tGot factors: {facs0} and {facs1}...");

        
        /* Get positive and negative factors from the passed Oper o */
        foreach (Oper p in facs1.Numerator)
        {
            if (p is not PowTowRootLog ptrl)
                throw Scribe.Issue("Fac was not ptrl");
            //Scribe.Info($"\t\t\tGot positive base {ptrl.posArgs[0]} from fac {ptrl}");
            basesPos.Add(ptrl.posArgs[0]);
        }
        foreach (Oper p in facs1.Denominator)
        {
            if (p is not PowTowRootLog ptrl)
                throw Scribe.Issue("Fac was not ptrl");
            //Scribe.Info($"\t\t\tGot negative base {ptrl.posArgs[0]} from fac {ptrl}");
            basesNeg.Add(ptrl.posArgs[0]);
        }

        Fraction commonFactors = new();
        /* Find positive common factors */
        foreach (Oper facPos in facs0.Numerator)
        {
            // TODO: type forms should be able to handle thiss
            if (facPos is not PowTowRootLog)
                throw Scribe.Issue($"Factor {facPos} is not a PowTowRootLog!");
            if (basesPos.Contains(facPos.posArgs[0], ol))
            {
                Oper A = facPos.posArgs[1];
                Oper B = o.Degree(facPos.posArgs[0]);
                Oper ABsign = new Funcs.Sign(A.Mult(B)); //AB.ReduceOuter();
                // common factors of x^A and x^B are: Max(AB.sign*A, AB.sign*B) - AB.sign*|A - B|
                Oper commonFactorExponent = new Funcs.Max(ABsign.Mult(A), ABsign.Mult(B)).Subtract(ABsign.Mult(new Funcs.Abs(A.Subtract(B))));
                PowTowRootLog commonFactor;
                if (commonFactorExponent.IsDetermined)
                    commonFactor = facPos.posArgs[0].Pow(commonFactorExponent.Solution());
                else
                    commonFactor = facPos.posArgs[0].Pow(commonFactorExponent);
                commonFactors.Numerator.Add(commonFactor);
            }
        }
        /* Find positive common factors */
        foreach (Oper facNeg in facs0.Denominator)
        {
            if (facNeg is not PowTowRootLog)
                throw Scribe.Issue($"Factor {facNeg} is not a PowTowRootLog!");
            if (basesNeg.Contains(facNeg.posArgs[0], ol))
            {
                Oper A = facNeg.posArgs[1];
                Oper B = new Funcs.Abs(o.Degree(facNeg.posArgs[0]));
                Oper ABsign = new Funcs.Sign(A.Mult(B)); //AB.ReduceOuter();
                Oper commonFactorExponent = new Funcs.Max(ABsign.Mult(A), ABsign.Mult(B)).Subtract(ABsign.Mult(new Funcs.Abs(A.Subtract(B))));
                PowTowRootLog commonFactor;
                if (commonFactorExponent.IsDetermined)
                    commonFactor = facNeg.posArgs[0].Pow(commonFactorExponent.Solution());
                else
                    commonFactor = facNeg.posArgs[0].Pow(commonFactorExponent);
                commonFactors.Denominator.Add(commonFactor);
            }
        }
        

        if (commonFactors.Numerator.Count == 0)
            commonFactors.Numerator.Add(new Variable(1));

        return commonFactors;
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
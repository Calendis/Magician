namespace Magician.Alg.Symbols;
using Magician.Core;
using Magician.Core.Maps;

public abstract partial class Oper : IRelation
{
    protected readonly Variable solution;
    public IVal Cache => solution;
    public string Name => name;
    // TODO: make these private or protected and use an indexer
    public List<Oper> posArgs = new();
    public List<Oper> negArgs = new();
    public List<Oper> AllArgs => posArgs.Concat(negArgs).ToList();
    public List<Variable> AssociatedVars = new();
    public Variable ByName(string n)
    {
        foreach (Variable v in AssociatedVars)
        {
            if (v.Name == n)
                return v;
        }
        throw Scribe.Error($"Variable {n} was not found in {this}");
    }

    protected readonly string name;
    protected bool associative = false;
    protected bool commutative = false;
    public bool trivialAssociative = true;
    public bool invertible = true;
    protected virtual int? Identity { get; }
    public int Ins { get { return AssociatedVars.Where(v => !v.Found).Count(); } }
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
    public abstract Variable Sol();
    public abstract Oper Degree(Oper v);

    protected Oper(string name, IEnumerable<Oper> posa, IEnumerable<Oper> nega)
    {
        this.name = name;
        posArgs = posa.ToList();
        negArgs = nega.ToList();
        
        // Collect the data
        // This is a hack, but it works. Normally I would check the found property of the Variable, but this is the
        // base ctor, which is called before that property can be set
        if (!(this is Variable && (new string(name.Take(8).ToArray()) == "constant") || name == "undefined"))
        {
            OperLayers ol = new(this, Variable.Undefined);
            AssociatedVars = ol.GetInfo(0, 0).assocArgs.Distinct().ToList();
        }
        if (this is not Variable)
        {
            solution = new("sol", double.NaN);
            //solution = new("undefinedo");
        }
    }
    // Alternating form ctor. Handy in some cases
    protected Oper(string name, params Oper[] cstArgs) : this(name, cstArgs.Where((o, i) => i % 2 == 0).ToList(), cstArgs.Where((o, i) => i % 2 == 1).ToList()) { }

    // Provide real-valued arguments to solve the expression at a point
    public IVal Evaluate(params double[] args)
    {
        HashSet<Variable> associates = AssociatedVars.Where(v => !v.Found).ToHashSet();
        if (associates.Count != args.Length)
            throw Scribe.Error($"{name} {this} expected {associates.Count} arguments, got {args.Length}");

        int counter = 0;
        foreach (Variable a in associates.OrderBy(v => v.Name))
            a.Set(args[counter++]);
        // TODO: this copy probably isn't necessary
        Variable s = Sol().Copy();
        associates.ToList().ForEach(a => a.Reset());
        return s;
    }
    // Evaluate using named arguments
    public IVal Evaluate(params (string, IVal)[] args)
    {
        foreach ((string name, IVal val) in args)
        {
            Substitute(name, val);
        }
        Variable sol = Sol();
        AssociatedVars.ForEach(v => v.Reset());
        return sol;
    }
    // Substitute a value into a variable
    public void Substitute(string n, IVal val)
    {
        ByName(n).Set(val.Values.ToArray());
    }
    // Substitute one Oper for another
    public void Substitute(Oper toReplace, Oper sub)
    {
        bool didSub = false;
        for (int i = 0; i < posArgs.Count; i++)
        {
            posArgs[i].Substitute(toReplace, sub);
            if (posArgs[i].Like(toReplace))
            {
                didSub = true;
                posArgs[i] = sub.Copy();
            }
        }
        for (int i = 0; i < negArgs.Count; i++)
        {
            negArgs[i].Substitute(toReplace, sub);
            if (negArgs[i].Like(toReplace))
            {
                didSub = true;
                negArgs[i] = sub.Copy();
            }
        }
        if (didSub)
            AssociatedVars.AddRange(sub.AssociatedVars);
    }

    // Deep-copies an Oper. Unknown variables always share an instance, however (see Variable.Copy)
    public virtual Oper Copy() { return New(posArgs.Select((o, i) => o.Copy()).ToList(), negArgs.Select((o, i) => o.Copy()).ToList()); }

    // Perform basic simplifications on this Oper
    public abstract void ReduceOuter();
    // Perform basic simplifications on this Oper, recursively
    /* TODO: eventually make the void simplification/reduction methods internal. */
    /*       They modify state in a way that's safe inside the algebra solver,   */
    /*       but not necessarily otherwise. Returning a new Oper is always safe. */
    public void Reduce(int depth = -1)
    {
        if (depth == 0)
            return;
        if (depth > 0)
            depth--;
        Associate();
        foreach (Oper a in AllArgs)
            a.Reduce(depth);
        ReduceOuter();
    }
    // Perform advanced simplifications on this Oper
    internal virtual void CombineOuter(Variable? axis = null) { ReduceOuter(); }
    // Perform advanced simplifications on this Oper, recursively
    public void Combine(Variable? axis = null, int depth = 1)  // TODO: default should be -1
    {
        if (depth == 0)
            return;

        foreach (Oper a in AllArgs)
        {
            a.Combine(axis, depth - 1);
        }
        CombineOuter(axis);
    }
    // Perform advanced simplifications in this Oper recursively, as much as possible
    public void CombineAll(Variable? axis = null, int bailout = 999)
    {
        int iters = 0;
        Oper prev;
        do
        {
            prev = Copy();
            Reduce();
            Combine(axis, -1);
            Commute();
            if (++iters >= bailout)
            {
                Scribe.Warn($"Bailed out after {iters} combinations: {prev} vs {this}");
                break;
            }
            //Scribe.Info($"  ...{this} vs. {prev}");
        } while (!Like(prev));
        //Scribe.Info($" done, as {this} is equal to {prev}");
    }

    // For simplifications that change the parent node type
    public virtual Oper Simplified() => Copy();
    public Oper SimplifiedAll(int bailout = 99)
    {
        if (IsDetermined)
            return Sol();
        if (this is Variable)
            return this;

        int iters = 0;
        Oper prev = new Variable("sa_placeholder");
        Oper simp = Simplified();
        simp.Reduce();

        while (!prev.Like(simp))
        {
            if (simp is Variable)
                return simp;
            prev = simp.Copy();
            simp = simp.New(simp.posArgs.Select(a => a.Copy().SimplifiedAll()).ToList(), simp.negArgs.Select(a => a.Copy().SimplifiedAll()).ToList());
            simp.Reduce();
            if (++iters >= bailout)
            {
                Scribe.Warn($"Bailed out after {iters} simplifications: {simp.Name} {simp} vs {prev.Name} {prev}");
                break;
            }
        }
        simp.Reduce();
        return simp;

        /* for (int i = 0; i < posArgs.Count; i++)
            posArgs[i] = posArgs[i].SimplifiedAll();
        for (int i = 0; i < negArgs.Count; i++)
            negArgs[i] = negArgs[i].SimplifiedAll();
        Oper s = Simplified();
        return s; */

    }
    public virtual Oper Canonical()
    {
        Oper o = Copy().SimplifiedAll();
        o.CombineAll();
        o.Reduce();
        o = o.SimplifiedAll();
        o.Commute();
        return o.Trim();
    }
    public Oper Trim()
    {
        if (IsTrivial)
            return posArgs[0].Trim();
        return this;
    }

    // Overall degree of the expression
    public Oper Degree()
    {
        if (IsDetermined)
            return new Variable(0);
        List<Oper> degs = AssociatedVars.Select(av => Degree(av)).ToList();
        Oper result = new SumDiff(degs, new List<Oper> { });
        result.Combine(null, 2);
        result.Reduce(2);
        result.Commute();
        if (result.IsDetermined)
            return result.Sol();
        return result;
    }

    // Recursively move branches up
    public void Associate(Oper? parent = null)
    {
        // Absorb trivial
        for (int i = 0; i < posArgs.Count; i++)
            posArgs[i] = posArgs[i].Trim();
        for (int i = 0; i < negArgs.Count; i++)
            negArgs[i] = negArgs[i].Trim();

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

    /* Customizable symbolic operator methods */
    public virtual Oper Plus(Oper o)
    {
        return new SumDiff(new List<Oper> { this, o }, new List<Oper> { });
    }
    public virtual Oper Minus(Oper o)
    {
        return new SumDiff(new List<Oper> { this }, new List<Oper> { o });
    }
    public virtual Oper Mult(Oper o)
    {
        return new Fraction(new List<Oper> { this, o }, new List<Oper> { });
    }
    public static SumDiff Mult(Oper o, SumDiff sd)
    {
        return new SumDiff(sd.posArgs.Select(a => a.Mult(o.Copy())).ToList(), sd.negArgs.Select(a => a.Mult(o.Copy())).ToList());
    }
    public virtual Oper Divide(Oper o)
    {
        if (Like(o))
            return new Variable(1);
        return new Fraction(this, o);
    }
    public virtual ExpLog Pow(Oper o)
    {
        return new ExpLog(new List<Oper> { this, o }, new List<Oper> { });
    }
    public virtual ExpLog Exp(Oper o)
    {
        return new ExpLog(new List<Oper> { o, this }, new List<Oper> { });
    }
    public virtual Oper Root(Oper o)
    {
        return new ExpLog(new List<Oper> { this, new Fraction(Notate.Val(1), o) }, new List<Oper> { });
    }
    public virtual Oper Log(Oper o)
    {
        return new ExpLog(new List<Oper> { this }, new List<Oper> { o });
    }

    // TODO: in the future, this could have a type form like Fraction(n->PowTowRootLog, n->PowTowRootLog)
    //public virtual Fraction Factors(){return new Fraction(new ExpLog(new List<Oper>{Copy(), new Variable(1)}, new List<Oper>{}));}
    public virtual FactorMap Factors() => new(this);

    public Oper CommonFactors(Oper o) => Factors().Common(o.Factors()).ToFraction();

    public static bool operator <(Oper o0, Oper o1)
    {
        if (o0.IsDetermined)
        {
            if (o1.IsDetermined)
            {
                return o0.Sol().Value < o1.Sol().Value;
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
                return o0.Sol().Value > o1.Sol().Value;
            else
                return true;
        }
        else if (o1.IsDetermined)
            return false;
        return o0.Degree() > o1.Degree();
    }
    public bool Like(Oper o)
    {
        if (o == null)
            return false;
        if (o.GetType() != GetType())
        {
            if (o is not Variable || this is not Variable)
                return false;
        }

        if (posArgs.Count != o.posArgs.Count)
            return false;
        if (negArgs.Count != o.negArgs.Count)
            return false;

        if (this is Variable v && o is Variable u)
        {
            if (v.Found && u.Found)
            {
                IVar ivV = v.Sol().Var; IVar ivU = u.Sol().Var;

                if ((ivV.Is1DVector && ivU.Is1DVector) || (ivV.IsScalar && ivU.Is1DVector) || (ivV.Is1DVector && ivU.IsScalar))
                    return new Variable(ivV.ToIVal()).Like(new Variable(ivU.ToIVal()));
                else if (ivV.IsScalar && ivU.IsScalar)
                    return ((IVal)ivV).EqValue((IVal)ivU);
                else if (ivV.IsVector && ivU.IsVector)
                {
                    if (ivV.Dims != ivU.Dims)
                        return false;
                    for (int i = 0; i < ivV.Dims; i++)
                        if (!ivV.Get(i).EqValue(ivU.Get(i)))
                            return false;
                }
            }
            else
                return v == u;
        }

        for (int i = 0; i < posArgs.Count; i++)
        {
            if (!((Oper)o.posArgs[i]).Like(posArgs[i]))
                return false;
        }
        for (int i = 0; i < negArgs.Count; i++)
        {
            if (!((Oper)o.negArgs[i]).Like(negArgs[i]))
                return false;
        }

        return true;
    }
    public int Contains(Oper o)
    {
        int c = 0;
        foreach (Oper arg in AllArgs)
        {
            if (arg.Like(o))
                c++;
            c += arg.Contains(o);
        }
        return c;
    }

    public string Ord(string? ord = null)
    {
        Dictionary<Type, (char neg, char pos)> typeHeaders = new()
        {
            {typeof(Variable),  ('v', 'V')},
            {typeof(Rational),  ('r', 'R')},
            {typeof(Multivalue),('w', 'W')},
            {typeof(SumDiff),   ('-', '+')},
            {typeof(Fraction),  ('/', '*')},
            {typeof(ExpLog),    ('R', '^')},
            {typeof(Commonfuncs.Abs), ('-', '|')},
            {typeof(Commonfuncs.Min), ('-', 'm')},
            {typeof(Commonfuncs.Max), ('-', 'M')},
            {typeof(Derivative), ('d', 'd')}
        };
        int totalHeaders = 1;
        int totalLeaves = 0;
        ord ??= "";

        // Assemble the ord string
        ord += typeHeaders[GetType()].pos;
        ord += posArgs.Count.ToString();
        ord += typeHeaders[GetType()].neg;
        ord += negArgs.Count.ToString();
        foreach (Oper p in posArgs)
        {
            ord += p.Ord();
        }
        foreach (Oper n in negArgs)
        {
            ord += n.Ord();
        }
        if (this is Variable u)
        {
            if (u.Found)
                ord += $"#{u}#";
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

namespace Magician.Symbols.Funcs;

public class Abs : Oper
{
    public Abs(Oper o) : base("abs", o)
    {
        unary = true;
        absorbable = true;
    }

    public override Oper Degree(Variable v)
    {
        return posArgs[0].Degree(v);
    }

    public override Oper New(IEnumerable<Oper> pa, IEnumerable<Oper> na)
    {
        //Scribe.Info($"\t  {this} newing {Scribe.Expand<IEnumerable<Oper>, Oper>(pa)}, {Scribe.Expand<IEnumerable<Oper>, Oper>(na)}...");
        if (!(pa.Count() <= 1 && na.Count() == 0))
        {
            //Scribe.Warn($"got {Scribe.Expand<IEnumerable<Oper>, Oper>(pa)} and {Scribe.Expand<IEnumerable<Oper>, Oper>(na)} on unary");
            throw Scribe.Error($"{this.GetType().Name} is a unary Oper");
        }
        //Scribe.Info($"\tNEW POSARGS [{pa.Count()}]: {Scribe.Expand<List<Oper>, Oper>(pa.ToList())}");
        return new Abs(pa.ToList()[0]);
    }

    public override void Reduce(Oper? parent = null)
    {
        posArgs[0].Reduce(this);
        base.Reduce(parent);
    }

    public override Variable Solution()
    {
        return Notate.Val(Math.Abs(posArgs[0].Solution().Val));
    }

    public override string ToString()
    {
        return $"|{posArgs[0]}|";
    }
}

public class Max : Oper
{
    public Max(params Oper[] os) : base("max", os, new List<Oper>{})
    {
        commutative = true;
        associative = true;
        absorbable = true;
    }
    public Max(IEnumerable<Oper> pa, IEnumerable<Oper> na) : base("max", pa.Concat(na), new List<Oper>{})
    {
        commutative = true;
        associative = true;
        absorbable = true;
    }

    public override Oper Degree(Variable v)
    {
        return posArgs[0].Degree(v);
    }

    public override Oper New(IEnumerable<Oper> pa, IEnumerable<Oper> na)
    {
        return new Max(pa, na);
    }

    public override Variable Solution()
    {
        List<Variable> sols = AllArgs.Select(a => a.Solution()).ToList();
        //Scribe.Info($"  Max sols: {Scribe.Expand<List<Variable>, Variable>(sols)}");
        // TODO: find out why .Max wasn't working with my IComparer implementation
        //Scribe.Info($"  Sol: {AllArgs.Select(a => a.Solution()).Max() ?? throw Scribe.Error("Undefined")}");
        //return AllArgs.Select(a => a.Solution()).Max() ?? throw Scribe.Error("Undefined");
        int maxIdx = 0;
        double max = double.MinValue;
        for (int i = 0; i < sols.Count; i++)
        {
            double sol = sols[i].Solution().Val;
            if (sol > max)
            {
                max = sol;
                maxIdx = i;
            }
        }
        return sols[maxIdx];
    }

    public override void Reduce(Oper? parent = null)
    {
        Oper max = AllArgs.Max();
        posArgs.Clear(); negArgs.Clear();
        posArgs.Add(max);
        base.Reduce(parent);
    }

    public override string ToString()
    {
        return $"Max({AllArgs.Aggregate("", (s, o) => s += o.ToString()+", ").Trim().TrimEnd(',')})";
    }
}

public class Min : Oper
{
    public Min(params Oper[] os) : base("max", os, new List<Oper>{})
    {
        commutative = true;
        associative = true;
        absorbable = true;
    }
    public Min(IEnumerable<Oper> pa, IEnumerable<Oper> na) : base("max", pa.Concat(na), new List<Oper>{})
    {
        commutative = true;
        associative = true;
        absorbable = true;
    }

    public override Oper Degree(Variable v)
    {
        return posArgs[0].Degree(v);
    }

    public override Oper New(IEnumerable<Oper> pa, IEnumerable<Oper> na)
    {
        return new Min(pa, na);
    }

    public override Variable Solution()
    {
        return AllArgs.Select(a => a.Solution()).Min() ?? throw Scribe.Error("Undefined");
    }

    public override void Reduce(Oper? parent = null)
    {
        Oper min = AllArgs.Min();
        posArgs.Clear(); negArgs.Clear();
        posArgs.Add(min);
        base.Reduce(parent);
    }
}
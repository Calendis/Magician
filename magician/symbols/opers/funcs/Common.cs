
namespace Magician.Symbols.Funcs;

public class Abs : Oper
{
    public Abs(Oper o) : base("abs", o) {}

    public override Oper Degree(Variable v)
    {
        return posArgs[0].Degree(v);
    }

    public override Oper New(IEnumerable<Oper> pa, IEnumerable<Oper> na)
    {
        //Scribe.Info($"\t  {this} newing {Scribe.Expand<IEnumerable<Oper>, Oper>(pa)}, {Scribe.Expand<IEnumerable<Oper>, Oper>(na)}...");
        if (!(pa.Count() <= 1 && !na.Any()))
        {
            //Scribe.Warn($"got {Scribe.Expand<IEnumerable<Oper>, Oper>(pa)} and {Scribe.Expand<IEnumerable<Oper>, Oper>(na)} on unary");
            throw Scribe.Error($"{this.GetType().Name} is a unary Oper");
        }
        //Scribe.Info($"\tNEW POSARGS [{pa.Count()}]: {Scribe.Expand<List<Oper>, Oper>(pa.ToList())}");
        return new Abs(pa.ToList()[0]);
    }

    public override void Reduce()
    {
        for (int i = 0; i < posArgs.Count; i++)
            posArgs[i] = Form.Shed(AllArgs[i]);
        for (int i = 0; i < posArgs.Count; i++)
            posArgs[i] = Form.Shed(AllArgs[i]);
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
    }
    public Max(IEnumerable<Oper> pa, IEnumerable<Oper> na) : base("max", pa.Concat(na), new List<Oper>{})
    {
        commutative = true;
        associative = true;
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

    public override void Reduce()
    {
        Oper max = AllArgs.Max();
        posArgs.Clear(); negArgs.Clear();
        posArgs.Add(max);
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
    }
    public Min(IEnumerable<Oper> pa, IEnumerable<Oper> na) : base("max", pa.Concat(na), new List<Oper>{})
    {
        commutative = true;
        associative = true;
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

    public override void Reduce()
    {
        Oper min = AllArgs.Min();
        posArgs.Clear(); negArgs.Clear();
        posArgs.Add(min);
    }
}
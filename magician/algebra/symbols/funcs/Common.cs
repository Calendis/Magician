namespace Magician.Alg.Symbols.Commonfuncs;
using Core;

public class Abs : Oper
{
    public Abs(Oper o) : base("abs", o) { trivialAssociative = false; associative = true; }

    public override Oper Degree(Oper v)
    {
        return posArgs[0].Degree(v);
    }

    public override Oper New(IEnumerable<Oper> pa, IEnumerable<Oper> na)
    {
        if (!(pa.Count() <= 1 && !na.Any()))
        {
            throw Scribe.Error($"{this.GetType().Name} is a unary Oper");
        }
        return new Abs(pa.ToList()[0]);
    }

    public override void ReduceOuter()
    {
        Oper o = posArgs[0];
        posArgs.Clear();
        posArgs.Add(o);
    }

    public override Variable Sol()
    {
        solution.Set(((IVal)AllArgs[0].Sol()).Magnitude);
        return solution;
    }

    public override string ToString()
    {
        return $"|{posArgs[0]}|";
    }
}
public class Sign : Oper
{
    public Sign(Oper o) : base("sign", o) { trivialAssociative = false; associative = true; }
    public override Oper Degree(Oper v)
    {
        return new Variable(0);
    }

    public override Oper New(IEnumerable<Oper> pa, IEnumerable<Oper> na)
    {
        if (!(pa.Count() <= 1 && !na.Any()))
            throw Scribe.Error($"{this.GetType().Name} is a unary Oper");
        return new Sign(pa.ToList()[0]);
    }

    public override void ReduceOuter()
    {
        Oper o = posArgs[0];
        posArgs.Clear();
        posArgs.Add(o);
    }

    public override Variable Sol()
    {
        IVal result = posArgs[0].Sol();
        if (result.Get() == 0)
            solution.Set(0);
        else if (result.Get() > 0)
            solution.Set(1);
        else
            solution.Set(-1);
        return solution;
    }
    public override string ToString()
    {
        return $"Sign({posArgs[0]})";
    }
}

//
public class Max : Oper
{
    public Max(params Oper[] os) : base("max", os, new List<Oper> { })
    {
        commutative = true;
        associative = true;
    }
    public Max(IEnumerable<Oper> pa, IEnumerable<Oper> na) : base("max", pa.Concat(na), new List<Oper> { })
    {
        commutative = true;
        associative = true;
    }

    public override Oper Degree(Oper v)
    {
        return New(posArgs.Select(pa => pa.Degree()), new List<Oper> { });
    }

    public override Oper New(IEnumerable<Oper> pa, IEnumerable<Oper> na)
    {
        return new Max(pa, na);
    }

    public override Variable Sol()
    {
        List<Variable> sols = AllArgs.Select(a => a.Sol()).ToList();
        int maxIdx = 0;
        IVal max = new Variable(double.MinValue);
        for (int i = 0; i < sols.Count; i++)
        {
            IVal sol = sols[i].Sol();
            if (sol > max)
            {
                max = sol;
                maxIdx = i;
            }
        }
        return sols[maxIdx];
    }

    public override void ReduceOuter()
    {
        //Oper max = AllArgs.Max();
        //posArgs.Clear(); negArgs.Clear();
        //posArgs.Add(max);
    }

    public override string ToString()
    {
        return $"Max({AllArgs.Aggregate("", (s, o) => s += o.ToString() + ", ").Trim().TrimEnd(',')})";
    }
}

public class Min : Oper
{
    public Min(params Oper[] os) : base("max", os, new List<Oper> { })
    {
        commutative = true;
        associative = true;
    }
    public Min(IEnumerable<Oper> pa, IEnumerable<Oper> na) : base("max", pa.Concat(na), new List<Oper> { })
    {
        commutative = true;
        associative = true;
    }

    public override Oper Degree(Oper v)
    {
        return New(posArgs.Select(pa => pa.Degree()), new List<Oper> { });
    }

    public override Oper New(IEnumerable<Oper> pa, IEnumerable<Oper> na)
    {
        return new Min(pa, na);
    }

    public override Variable Sol()
    {
        List<Variable> sols = AllArgs.Select(a => a.Sol()).ToList();
        int minIdx = 0;
        IVal min = new Variable(double.MaxValue);
        for (int i = 0; i < sols.Count; i++)
        {
            IVal sol = sols[i].Sol();
            if (sol < min)
            {
                min = sol;
                minIdx = i;
            }
        }
        return sols[minIdx];
    }

    public override void ReduceOuter()
    {
        //Oper min = AllArgs.Min();
        //posArgs.Clear(); negArgs.Clear();
        //posArgs.Add(min);
    }
    public override string ToString()
    {
        return $"Min({AllArgs.Aggregate("", (s, o) => s += o.ToString() + ", ").Trim().TrimEnd(',')})";
    }
}
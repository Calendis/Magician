namespace Magician.Symbols;

/* Combines powers, exponents, logs, and roots using the form logD(logB((a^b^c...)^A^-1)^C-1)... */
public class PowTowRootLog : Oper
{
    protected override int? Identity => 1;

    public PowTowRootLog(IEnumerable<Oper> posArgs, IEnumerable<Oper> negArgs) : base("pwtwrtlg", posArgs, negArgs)
    {
        if (!posArgs.Any())
            this.posArgs.Add(new Variable((int)Identity!));
        trivialAssociative = true;
    }
    public PowTowRootLog(params Oper[] ops) : base("pwtwrtlg", ops) { trivialAssociative = true; }

    public override Oper Degree(Oper v)
    {
        Oper deg;
        int c = 1;
        foreach (Oper pa in posArgs)
        {
            if (pa.Like(v))
                break;
            c++;
        }
        deg = New(posArgs.Skip(c), new List<Oper> { });
        c = 0;
        foreach (Oper na in negArgs)
        {
            if (c % 2 == 0)
            {
                deg = deg.Divide(na);
            }
            else
            {
                // TODO: test the consequences of defining log degree like this
                deg = deg.Log(na);
            }
            c++;
        }
        return deg;
    }

    public override Fraction Factors()
    {
        return new Fraction(Copy());
    }

    public override Oper New(IEnumerable<Oper> pa, IEnumerable<Oper> na)
    {
        return new PowTowRootLog(pa, na);
    }

    public override Variable Solution()
    {
        if (posArgs.Count == 0)
        {
            return new Variable(1);
        }
        Variable sol0 = posArgs[0].Solution();
        Variable pos;
        //Variable neg;
        if (posArgs.Count == 0)
            pos = new Variable(1);
        else if (posArgs.Count == 1)
            pos = posArgs[0].Solution();
        else if (posArgs.Count == 2)
            pos = new Variable(Math.Pow(sol0.Val, posArgs[1].Solution().Val));
        else
            pos = new PowTowRootLog(new List<Oper> { sol0, new PowTowRootLog(posArgs.Skip(1), new List<Oper> { }) }, new List<Oper> { }).Solution();

        if (negArgs.Count == 0)
            return pos;
        else if (negArgs.Count == 1)
            return new Variable(Math.Pow(pos.Val, 1d / negArgs[0].Solution().Val));
        else if (negArgs.Count == 2)
            return new Variable(Math.Log(Math.Pow(pos.Val, 1d / negArgs[0].Solution().Val), negArgs[1].Solution().Val));
        else
            return new PowTowRootLog(new List<Oper> { new PowTowRootLog(new List<Oper> { pos }, new List<Oper> { negArgs[^1], negArgs[^2] }) }, negArgs.SkipLast(2)).Solution();
    }

    public override void ReduceOuter()
    {
        /* Truncate power tower at any 1s */
        int? idIdx = null;
        for (int i = 0; i < posArgs.Count; i++)
        {
            if (posArgs[i].IsConstant && posArgs[i].Solution().Val == Identity)
            {
                idIdx = i;
                break;
            }
        }
        if (idIdx is not null)
            posArgs = posArgs.Take((int)idIdx).ToList();

        // Combine positive constant terms
        posArgs.Reverse();
        int runLen = 0;
        foreach (Oper pa in posArgs)
        {
            if (!pa.IsDetermined)
                break;
            runLen++;
        }
        posArgs.Reverse();
        if (runLen > 0)
        {
            List<Oper> toCombine = posArgs.TakeLast(runLen).ToList();
            Variable combined = new PowTowRootLog(toCombine, new List<Oper> { }).Solution();
            posArgs = posArgs.Take(posArgs.Count - runLen).ToList();
            posArgs.Add(combined);
        }
    }

    //public override PowTowRootLog Mult(Oper o)
    //{
    //    if (o is PowTowRootLog)
    //    {
    //        if (posArgs[0].Like(o.posArgs[0]))
    //        {
    //            return new PowTowRootLog(new List<Oper> {posArgs[0]}, new List<Oper> {});
    //        }
    //    }
    //    //return base.Pow(o);
    //}

    public override string ToString()
    {
        string s = "";
        foreach (Oper a in posArgs)
            s += $"{a}^";
        s = s.TrimEnd('^');
        s = $"({s})";
        if (negArgs.Count == 0)
            return s;
        else
        {
            for (int i = 0; i < negArgs.Count; i++)
            {
                if (i % 2 == 0)
                    s += $"^({negArgs[i]}^-1)";
                else
                    s = $"log_{negArgs[i]}({s})";
            }
            return s;
        }
    }
}

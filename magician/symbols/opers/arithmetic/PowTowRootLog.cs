namespace Magician.Symbols;

/* Combines powers, exponents, logs, and roots using the form logD(logB((a^b^c...)^A^-1)^C-1)... */
public class PowTowRootLog : Oper
{
    protected override int? Identity => 1;

    public PowTowRootLog(IEnumerable<Oper> posArgs, IEnumerable<Oper> negArgs) : base("pwtwrtlg", posArgs, negArgs)
    {
        if (!posArgs.Any())
            posArgs = posArgs.Append(new Variable((int)Identity!));
    }
    public PowTowRootLog(params Oper[] ops) : base("pwtwrtlg", ops) { }

    public override Oper Degree(Variable v)
    {
        throw new NotImplementedException();
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

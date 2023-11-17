namespace Magician.Symbols;

// SumDiff objects represent addition and subtraction operations with any number of arguments
public class SumDiff : Oper
{
    protected override int? Identity { get => 0; }

    // TODO: make it so that you don't need to write both constructors
    public SumDiff(params Oper[] ops) : base("sumdiff", ops)
    {
        commutative = true;
        associative = true;
        absorbable = true;
    }
    public SumDiff(IEnumerable<Oper> a, IEnumerable<Oper> b) : base("sumdiff", a, b)
    {
        commutative = true;
        associative = true;
        absorbable = true;
    }
    public override Variable Solution()
    {
        double total = 0;
        foreach (Oper o in posArgs)
            if (o is Variable v)
                total += v.Val;
            else
                total += o.Solution().Val;
        foreach (Oper o in negArgs)
            if (o is Variable v)
                total -= v.Val;
            else
                total -= o.Solution().Val;
        return new Variable(total);
    }

    public override SumDiff New(IEnumerable<Oper> a, IEnumerable<Oper> b)
    {
        return new SumDiff(a, b);
    }
    public static SumDiff StaticNew(IEnumerable<Oper> a, IEnumerable<Oper> b)
    {
        return new SumDiff(a, b);
    }

    public override Oper Degree(Variable v)
    {
        if (IsDetermined)
            return new Variable(0);
        Oper minD = new Variable(0);
        Oper maxD = new Variable(1);
        foreach (Oper o in AllArgs)
        {
            Oper d = o.Degree(v);
            minD = d < minD ? d : minD;
            maxD = d > maxD ? d : maxD;
        }
        return Form.Canonical(new Funcs.Abs(maxD.Subtract(minD)));
    }

    public override void Combine(Variable axis)
    {
        //Scribe.Info($"Combining {name} {this}");
        List<Oper> finalPosArgs = new();
        List<Oper> finalNegArgs = new();

        var (fhs, gao) = FlaggedHandshakes(axis);
        List<List<(int, int, bool, bool)>> grpFlagHandshakes = fhs;
        List<List<(Oper, bool)>> grpAxisOpers = gao;

        for (int c = 0; c < 2; c++)
        {
            List<(int, int, bool, bool)> flaggedHandshakes = grpFlagHandshakes[c];
            List<(Oper, bool)> termsToCombine = grpAxisOpers[c];
            List<int> termsNeedingHandshake = Enumerable.Range(0, termsToCombine.Count).ToList();

            List<int> termsFoundHandshake = new();
            while (termsNeedingHandshake.Count > 0)
            {
                foreach ((int, int, bool, bool) flaggedHandshake in flaggedHandshakes)
                {
                    int i = flaggedHandshake.Item1;
                    int j = flaggedHandshake.Item2;
                    Oper A = termsToCombine[i].Item1;
                    Oper B = termsToCombine[j].Item1;
                    bool aPositive = flaggedHandshake.Item3;
                    bool bPositive = flaggedHandshake.Item4;
                    bool positive = !(aPositive ^ bPositive);

                    if (termsFoundHandshake.Contains(i) || termsFoundHandshake.Contains(j))
                        continue;

                    Oper combined;
                    Oper AB = Intersect(A, B);
                    Oper ABbar;
                    if (positive)
                        ABbar = A.Divide(AB).Add(B.Divide(AB));
                    else if (aPositive)
                        ABbar = A.Divide(AB).Subtract(B.Divide(AB));
                    else if (bPositive)
                        ABbar = B.Divide(AB).Subtract(A.Divide(AB));
                    else
                        throw Scribe.Issue("haggu!");

                    if (A is Variable av && av.Found && av.Val == 0)
                        combined = B;
                    else if (B is Variable bv && bv.Found && bv.Val == 0)
                        combined = A;
                    else
                        combined = AB.Mult(ABbar);
                    //Scribe.Info($"  combined: {combined}");
                    combined.Reduce();
                    //Scribe.Info($"  Reduced combined: {combined}");

                    //Scribe.Warn($"  A, B, combined, +-: {A}, {B}, {combined}, {aPositive}{bPositive}");

                    if ((positive || aPositive) && (aPositive || bPositive))
                        finalPosArgs.Add(combined);
                    else
                        finalNegArgs.Add(combined);

                    // We're done for this term, and this also implies a handshake for the pair term
                    termsNeedingHandshake.Remove(i);
                    termsFoundHandshake.Add(i);
                    termsNeedingHandshake.Remove(j);
                    termsFoundHandshake.Add(j);
                    break;
                }
            }
        }
        // Apply the changes
        posArgs.Clear();
        posArgs.AddRange(finalPosArgs);
        negArgs.Clear();
        negArgs.AddRange(finalNegArgs);
        //Scribe.Info($"Got {this}");
    }

    public override string ToString()
    {
        if (AllArgs.Count == 0)
            return "0";
        string sumdiff = "";
        foreach (Oper o in posArgs)
        {
            sumdiff += " + " + o.ToString();
        }
        sumdiff = sumdiff.TrimStart(' ');
        sumdiff = sumdiff.TrimStart('+');
        sumdiff = sumdiff.TrimStart(' ');

        foreach (Oper o in negArgs)
        {
            sumdiff += " - " + o.ToString();
        }
        return "(" + sumdiff + ")";
    }
}

// Fraction objects represent multiplication and division operations with any number of arguments
public class Fraction : Oper
{
    protected override int? Identity { get => 1; }
    public Fraction(IEnumerable<Oper> a, IEnumerable<Oper> b) : base("fraction", a, b)
    {
        commutative = true;
        associative = true;
        absorbable = true;
    }
    public Fraction(params Oper[] ops) : base("fraction", ops)
    {
        commutative = true;
        associative = true;
        absorbable = true;
    }

    public override Variable Solution()
    {
        double quo = 1;
        foreach (Oper o in posArgs)
            if (o is Variable v)
                quo *= v.Val;
            else
                quo *= o.Solution().Val;
        foreach (Oper o in negArgs)
            if (o is Variable v)
                quo /= v.Val;
            else
                quo /= o.Solution().Val;
        return new Variable(quo);
    }

    public override Fraction New(IEnumerable<Oper> a, IEnumerable<Oper> b)
    {
        return new Fraction(a, b);
    }
    public static Fraction StaticNew(IEnumerable<Oper> a, IEnumerable<Oper> b)
    {
        return new Fraction(a, b);
    }

    public override Oper Degree(Variable v)
    {
        if (IsDetermined)
            return new Variable(0);
        return new SumDiff(posArgs.Select(a => a.Degree(v)), negArgs.Select(a => a.Degree(v)));
        //return Form.Canonical(posArgs.Aggregate<Oper, Oper>(new SumDiff(), (o, a) => o.Add(a)).Subtract(negArgs.Aggregate<Oper, Oper>(new SumDiff(), (o, a) => o.Add(a))));
    }

    public override void Combine(Variable axis)
    {
        return;
        List<Oper> finalPosArgs = new();
        List<Oper> finalNegArgs = new();

        var (fhs, gao) = FlaggedHandshakes(axis);
        List<List<(int, int, bool, bool)>> grpFlagHandshakes = fhs;
        List<List<(Oper, bool)>> grpAxisOpers = gao;

        for (int c = 0; c < 2; c++)
        {
            List<(int, int, bool, bool)> flaggedHandshakes = grpFlagHandshakes[c];
            List<(Oper, bool)> termsToCombine = grpAxisOpers[c];
            List<int> termsNeedingHandshake = Enumerable.Range(0, termsToCombine.Count).ToList();

            List<int> termsFoundHandshake = new();
            while (termsNeedingHandshake.Count > 0)
            {
                foreach ((int, int, bool, bool) flaggedHandshake in flaggedHandshakes)
                {
                    int i = flaggedHandshake.Item1;
                    int j = flaggedHandshake.Item2;
                    Oper A = termsToCombine[i].Item1;
                    Oper B = termsToCombine[j].Item1;
                    bool aPositive = flaggedHandshake.Item3;
                    bool bPositive = flaggedHandshake.Item4;
                    bool positive = !(aPositive ^ bPositive);

                    if (termsFoundHandshake.Contains(i) || termsFoundHandshake.Contains(j))
                        continue;

                    Oper AB = Intersect(A, B);
                    Oper ABbar;
                    if (positive)
                        ABbar = A.Divide(AB).Add(B.Divide(AB));
                    else if (aPositive)
                        ABbar = A.Divide(AB).Subtract(B.Divide(AB));
                    else if (bPositive)
                        ABbar = B.Divide(AB).Subtract(A.Divide(AB));
                    else
                        throw Scribe.Issue("haggu!");

                    //
                    Oper combined;
                    if (A is Variable av && av.Found && av.Val == 0)
                        combined = B;
                    else if (B is Variable bv && bv.Found && bv.Val == 0)
                        combined = A;
                    else
                        combined = AB.Pow(ABbar);

                    if ((positive || aPositive) && (aPositive || bPositive))
                        finalPosArgs.Add(combined);
                    else
                        finalNegArgs.Add(combined);

                    // We're done for this term, and this also implies a handshake for the pair term
                    termsNeedingHandshake.Remove(i);
                    termsFoundHandshake.Add(i);
                    termsNeedingHandshake.Remove(j);
                    termsFoundHandshake.Add(j);
                    break;
                }
            }
        }
        posArgs.Clear();
        posArgs.AddRange(finalPosArgs);
        negArgs.Clear();
        negArgs.AddRange(finalNegArgs);
    }

    //public static Fraction Mult(Oper a, Oper b)
    //{
    //    //
    //}

    public override Fraction Mult(Oper o)
    {
        if (o is Fraction)
            return new Fraction(posArgs.Concat(o.posArgs), negArgs.Concat(o.negArgs));
        return (Fraction)base.Mult(o);
    }

    public override Fraction Divide(Oper o)
    {
        if (o is Fraction)
            return new Fraction(posArgs.Concat(o.negArgs), negArgs.Concat(o.posArgs));
        return (Fraction)base.Divide(o);
    }

    public override string ToString()
    {
        string numerator = "";
        string denominator = "";

        foreach (Oper o in posArgs)
        {
            if (!(o is Variable v && !v.Found))
                numerator += "*" + o.ToString();
            else
                numerator += o.ToString();
        }
        foreach (Oper o in negArgs)
        {
            denominator += "*" + o.ToString();
        }
        return $"({(negArgs.Count == 0 ? numerator.TrimStart('*') : $"{numerator.TrimStart('*')}/{denominator.TrimStart('*')}")})";
    }
}

public class PowTowRootLog : Oper
{
    protected override int? Identity => 1;

    public PowTowRootLog(IEnumerable<Oper> posArgs, IEnumerable<Oper> negArgs) : base("powtow", posArgs, negArgs)
    {
        if (!posArgs.Any())
            throw Scribe.Error($"{this.GetType().Name} requires at least one positive argument");
    }
    public PowTowRootLog(params Oper[] ops) : base("powtowlogroot", ops) { }

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
        Variable sol0 = posArgs[0].Solution();
        Variable pos;
        Variable neg;
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

    public override void Reduce(Oper? parent = null)
    {
        foreach (Oper a in AllArgs)
            a.Reduce(this);

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
        if (parent is null)
            return;
        for (int i = 0; i < AllArgs.Count; i++)
            AllArgs[i] = AllArgs[i].Reduced(this);
        AbsorbTrivial(parent);
    }
}

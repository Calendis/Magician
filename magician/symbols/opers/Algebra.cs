namespace Magician.Symbols;

public abstract partial class Oper
{
    public void DropIdentities()
    {
        if (Identity is null)
            throw Scribe.Error($"{this} has no identity to drop");
        // Drop identities
        posArgs = posArgs.Where(o => !(o.IsConstant && ((Variable)o).Val == Identity)).ToList();
        negArgs = negArgs.Where(o => !(o.IsConstant && ((Variable)o).Val == Identity)).ToList();
        // Prefer explicit identity over empty args
        // TODO: account for the fact that this implementation will break ExpPows, as they have no identity
        if (posArgs.Count == 0 && this is not Variable)
            posArgs.Add(New(new List<Oper> { Notate.Val((int)Identity) }, new List<Oper> { }));
    }
    public void BalanceArguments()
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
    public void AbsorbTrivial(Oper parent)
    {
        // Absorb trivial
        if (absorbable && posArgs.Count == 1 && negArgs.Count == 0)
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
    }
    public static (Oper, Oper) IsolateOperOn(Oper chosenSide, Oper oppositeSide, Oper axis, Variable v)
    {
        if (!chosenSide.commutative)
            throw Scribe.Issue("You need to fix this to support non-commutative Opers");
        //Oper os = oppositeSide.Copy();
        bool invertedAxis = false;
        if (chosenSide.negArgs.Contains(axis))
        {
            invertedAxis = true;
            chosenSide.negArgs.Remove(axis);
        }
        else if (chosenSide.posArgs.Contains(axis))
        {
            chosenSide.posArgs.Remove(axis);
        }
        else
        {
            throw Scribe.Issue($"Axis {axis} not in {chosenSide}");
        }

        Oper cs = chosenSide.New(new List<Oper> { }, new List<Oper> { });
        cs.posArgs.AddRange(chosenSide.posArgs);
        cs.negArgs.AddRange(chosenSide.negArgs);
        (cs.posArgs, cs.negArgs) = (cs.negArgs, cs.posArgs);

        //cs = Upcast(cs, axis);
        //cs.posArgs.Add(chosenSide);

        cs.posArgs.Add(oppositeSide.Copy());
        // Perform an additional inversion if necessary
        if (invertedAxis)
            (cs.posArgs, cs.negArgs) = (cs.negArgs, cs.posArgs);

        //cs.Simplify(v);
        return (axis.Trim(), cs.Trim());
    }

    public static (Oper, Oper) ExtractOperFrom(Oper chosenSide, Oper oppositeSide, Oper axis)
    {
        // need extract method
        if (chosenSide is Variable || axis == chosenSide)
            return ExtractOperFrom(SumDiff.StaticNew(new List<Oper> { chosenSide }, new List<Oper>()), oppositeSide, axis);
        if (chosenSide.posArgs.Contains(axis))
            chosenSide.posArgs.Remove(axis);
        else if (chosenSide.negArgs.Contains(axis))
            chosenSide.negArgs.Remove(axis);
        else
            throw Scribe.Issue($"Could not extract {axis} from {chosenSide.GetType()} {chosenSide}");
        return (chosenSide.Trim(), chosenSide.New(new List<Oper> { oppositeSide }, new List<Oper> { axis }).Trim());
    }

    public static Oper Intersect(Oper o, Oper p)
    {
        if (o.IsDetermined || p.IsDetermined)
            return new Variable(1);
        if (o.Like(p))
        {
            if (o.IsDetermined)
                return new Variable(1);
            else return o;
        }
        if (o.IsDetermined)
        {

        }
        if (o is Variable v)
        {
            if (p is Variable u)
                if (v == u)
                    return v;
                else
                    return new Variable(1);
            if (v.Found)
                return new Variable(v.Val == 0 ? 0 : 1);
            o = p.New(new List<Oper> { o }, new List<Oper> { });
        }
        if (p is Variable uu)
        {
            if (uu.Found)
                return new Variable(uu.Val == 0 ? 0 : 1);
            p = o.New(new List<Oper> { p }, new List<Oper> { });
        }
        //if (o.Like(p))
        //    return new Variable(1);
        IEnumerable<Oper> pos = IntersectPos(o, p);
        IEnumerable<Oper> neg = IntersectNeg(o, p);
        return o.New(pos, neg);
    }

    public static IEnumerable<Oper> IntersectPos(Oper o, Oper p)
    {
        OperLike oc = new();
        Dictionary<Oper, int> d = new(oc);
        foreach (Oper c in p.posArgs)
        {
            d.TryGetValue(c, out int matches);
            d[c] = matches + 1;
        }
        foreach (Oper c in o.posArgs)
        {
            d.TryGetValue(c, out int matches);
            if (matches > 0)
            {
                yield return c.Copy();
                d[c] = matches - 1;
            }
        }
    }
    public static IEnumerable<Oper> IntersectNeg(Oper o, Oper p)
    {
        OperLike oc = new();
        Dictionary<Oper, int> d = new(oc);
        foreach (Oper c in p.negArgs)
        {
            d.TryGetValue(c, out int matches);
            d[c] = matches + 1;
        }
        foreach (Oper c in o.negArgs)
        {
            d.TryGetValue(c, out int matches);
            if (matches > 0)
            {
                yield return c.Copy();
                d[c] = matches - 1;
            }
        }
    }
}
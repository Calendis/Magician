namespace Magician.Symbols;

public abstract partial class Oper
{
    public static (Oper, Oper) IsolateOperOn(Oper chosenSide, Oper oppositeSide, Oper axis, Variable v)
    {
        if (!chosenSide.commutative)
            throw Scribe.Issue("You need to fix this to support non-commutative Opers");
        bool invertedAxis = false;
        if (chosenSide.negArgs.Contains(axis) && chosenSide.Degree(v) < 0)
        {
            invertedAxis = true;
            chosenSide.negArgs.Remove(axis);
        }
        else
        {
            chosenSide.posArgs.Remove(axis);
        }
        Oper newChosen = axis;

        chosenSide.negArgs.Add(oppositeSide);
        // Invert if the axis wasn't already inverted
        if (!invertedAxis)
            (chosenSide.posArgs, chosenSide.negArgs) = (chosenSide.negArgs, chosenSide.posArgs);

        return (newChosen, chosenSide.Copy());

    }

    public static (Oper, Oper) ExtractOperFrom(Oper chosenSide, Oper oppositeSide, Oper axis)
    {
        Oper newOpposite;
        if (chosenSide.posArgs.Contains(axis))
        {
            chosenSide.posArgs.Remove(axis);
            newOpposite = chosenSide.New(new List<Oper>() { oppositeSide }, new List<Oper>() { axis });
        }
        else if (chosenSide.negArgs.Contains(axis))
        {
            chosenSide.negArgs.Remove(axis);
            newOpposite = chosenSide.New(new List<Oper>() { oppositeSide, axis }, new List<Oper>() { });

        }
        else
        {
            throw Scribe.Issue($"Could not extract {axis} from {chosenSide}");
        }
        return (chosenSide, newOpposite);
    }

    public static void CombineConstantTerms(SumDiff sd)
    {
        /* Combine constant integer terms */
        IEnumerable<Oper> plus = sd.posArgs.Where(o => o is Variable v && v.Found && (int)v.Val == v.Val && (int)v.Val != sd.identity);
        IEnumerable<Oper> minus = sd.negArgs.Where(o => o is Variable v && v.Found && (int)v.Val == v.Val && (int)v.Val != sd.identity);
        List<Oper> posChaff = sd.posArgs.Where(o => !(o is Variable v && v.Found && (int)v.Val == v.Val && (int)v.Val != sd.identity)).ToList();
        List<Oper> negChaff = sd.negArgs.Where(o => !(o is Variable v && v.Found && (int)v.Val == v.Val && (int)v.Val != sd.identity)).ToList();
        int newPlus = plus.Aggregate(0, (total, o) => total + (int)((Variable)o).Val);
        int newMinus = minus.Aggregate(0, (total, o) => total + (int)((Variable)o).Val);
        int reducedConstant = newPlus - newMinus;
        if (reducedConstant < 0)
            negChaff.Add(new Variable(-reducedConstant));
        else if (reducedConstant > 0)
            posChaff.Add(new Variable(reducedConstant));
        sd.posArgs.Clear(); sd.posArgs.AddRange(posChaff);
        sd.negArgs.Clear(); sd.negArgs.AddRange(negChaff);
    }

    public static void CombineLikeTerms(SumDiff sd, Variable axis)
    {

        Scribe.Warn($"  Combining like terms for {sd} in terms of {axis}");
        List<Oper> finalPosArgs = new();
        List<Oper> finalNegArgs = new();
        List<(Oper, bool)> termsContainingAxis = new();
        List<(Oper, bool)> termsNotContainingAxis = new();

        // Separate out terms containing our axis, and separate positive from negative
        // This ensures our axis remains on the outside
        foreach (Oper po in sd.posArgs)
            if (po.Contains(axis))
                termsContainingAxis.Add((po, true));
            else if (po is not Variable v || !v.Found)
                termsNotContainingAxis.Add((po, true));
            else if (v.Found)
                finalPosArgs.Add(v);
        foreach (Oper no in sd.negArgs)
            if (no.Contains(axis))
                termsContainingAxis.Add((no, false));
            else if (no is not Variable v || !v.Found)
                termsNotContainingAxis.Add((no, false));
            else if (v.Found)
                finalNegArgs.Add(v);

        foreach (List<(Oper, bool)> separatedTerms in new List<List<(Oper, bool)>> { termsContainingAxis, termsNotContainingAxis })
        {
            List<(int, int, bool, bool)> handshakes = new();
            Dictionary<int, List<(int, int, bool, bool)>> termsToHandshakes = new();
            
            // Trivial case
            if (separatedTerms.Count == 1)
            {
                if (separatedTerms[0].Item2)
                {
                    finalPosArgs.Add(separatedTerms[1].Item1);
                    continue;
                }
                else
                {
                    finalNegArgs.Add(separatedTerms[0].Item1);
                    continue;
                }
            }

            List<(Oper, bool)> termsToCombine = separatedTerms.Count % 2 == 1 ? separatedTerms.Append((new Variable(0), true)).ToList() : separatedTerms;
            int counter = 0;
            for (int i = 0; i < termsToCombine.Count; i++)
            {
                for (int j = i + 1; j < termsToCombine.Count; j++)
                {
                    Oper A = termsToCombine[i].Item1;
                    Oper B = termsToCombine[j].Item1;
                    //bool positive = !(termsToCombine[i].Item2 ^ termsToCombine[j].Item2);
                    bool aPositive = termsToCombine[i].Item2;
                    bool bPositive = termsToCombine[j].Item2;
                    (int, int, bool, bool) flaggedHandshake = (i, j, aPositive, bPositive);
                    handshakes.Add(flaggedHandshake);

                    if (termsToHandshakes.ContainsKey(i))
                        termsToHandshakes[i].Add(flaggedHandshake);
                    else
                        termsToHandshakes.Add(i, new List<(int, int, bool, bool)> { flaggedHandshake });
                    if (termsToHandshakes.ContainsKey(j))
                        termsToHandshakes[j].Add(flaggedHandshake);
                    else
                        termsToHandshakes.Add(j, new List<(int, int, bool, bool)> { flaggedHandshake });

                }
                counter++;
            }
            // Pick a handshake for each term
            //List<Oper> termsNeedingHandshake = termsToCombine.Select(ob => ob.Item1).ToList();
            List<int> termsNeedingHandshake = Enumerable.Range(0, termsToCombine.Count).ToList();
            List<int> termsFoundHandshake = new();
            while (termsNeedingHandshake.Count > 0)
            {
                int termIdx = 0;//termsNeedingHandshake[0];
                Scribe.Warn($"Term {termsNeedingHandshake[termIdx]} needs a handshake, remaining: {termsNeedingHandshake.Count} ({Scribe.Expand<List<int>, int>(termsNeedingHandshake)})");
                // Pick one of the possible handshakes for this term
                foreach ((int, int, bool, bool) flaggedHandshake in termsToHandshakes[termsNeedingHandshake[0]])
                {
                    int i = flaggedHandshake.Item1;
                    int j = flaggedHandshake.Item2;
                    Oper A = termsToCombine[i].Item1;
                    Oper B = termsToCombine[j].Item1;
                    bool aPositive = flaggedHandshake.Item3;
                    bool bPositive = flaggedHandshake.Item4;
                    bool positive = !(aPositive ^ bPositive);
                    if (termsFoundHandshake.Contains(i) || termsFoundHandshake.Contains(j))
                    {
                        //Scribe.Warn($"  Skipping missed handshake {i}, {j}...");
                        continue;
                    }
                    
                    Oper AB = Intersect(A, B);
                    SumDiff ABbar;
                    if (positive)
                        ABbar = A.Divide(AB).Add(B.Divide(AB));
                    else if (aPositive)
                        ABbar = A.Divide(AB).Subtract(B.Divide(AB));
                    else if (bPositive)
                        ABbar = B.Divide(AB).Subtract(A.Divide(AB));
                    else
                        ABbar = A.Divide(AB).Add(B.Divide(AB));

                    Oper combined = AB.Mult(ABbar);
                    Scribe.Warn($" A, B, AB, ABbar, combined: {A}, {B}, {AB}, {ABbar}, {combined}");
                    
                    // TODO: you must account for both polarities, not just one
                    if (aPositive || bPositive)
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
        Scribe.Info($"Got {new SumDiff(finalPosArgs, finalNegArgs)}");
        sd.posArgs.Clear();
        sd.posArgs.AddRange(finalPosArgs);
        sd.negArgs.Clear();
        sd.negArgs.AddRange(finalNegArgs);
    }

    public static Oper Exclude(Oper o, Oper p)
    {
        Scribe.Warn($"\t{o} exclude {p}");
        if (o is Variable)
            if (o == p)
                return new Variable(1);
            else
                return o;
        if (o.Like(p))
            return new Variable(1);
        OperCompare oc = new();
        Scribe.Warn($"\t{Scribe.Expand<List<Oper>, Oper>(o.posArgs)} except {Scribe.Expand<List<Oper>, Oper>(p.posArgs)}");
        List<Oper> finalPosArgs = new();
        List<Oper> finalNegArgs = new();
        List<Oper>[] finalArgs = new[] {finalPosArgs, finalNegArgs};
        List<Oper>[] keepArgs = new[] {o.posArgs, o.negArgs};
        List<Oper>[] exclArgs = new[] {p.posArgs, p.negArgs};
        for (int i = 0; i < 2; i++)
        {
            List<Oper> currentKeepArgs = keepArgs[i];
            List<Oper> currentExclArgs = exclArgs[i];
            foreach (Oper exclArg in currentExclArgs)
            {
                List<int> toRemove = new();
                for (int j = 0; j < currentKeepArgs.Count; j++)
                {
                    Oper a = currentKeepArgs[j];
                    if (a.Like(exclArg))
                    {
                        toRemove.Add(j);
                        break;  // Each exclude works for one Oper only. This is why we can't use .Except()
                    }
                }
                toRemove.ForEach(i => currentKeepArgs.RemoveAt(i));
            }
            finalArgs[i].AddRange(currentKeepArgs);
        }
        return o.New(finalPosArgs, finalNegArgs);
        //return o.New(o.posArgs.Except(p.posArgs, oc), o.negArgs.Except(p.negArgs, oc)).Copy();
    }

    public static Oper Intersect(Oper o, Oper p)
    {
        if (o is Variable v)
        {
            if (p is Variable u)
                if (v == u)
                    return v;
                else
                    return new Variable(1);
            if (v.Found)
                return new Variable(1);
            o = p.New(new List<Oper>{o}, new List<Oper>{});
        }
        if (p is Variable uu)
        {
            if (uu.Found)
                return new Variable(1);
            p = o.New(new List<Oper>{p}, new List<Oper>{});
        }
        IEnumerable<Oper> pos = IntersectPos(o, p);
        IEnumerable<Oper> neg = IntersectNeg(o, p);
        return o.New(pos, neg);
    }

    public static IEnumerable<Oper> IntersectPos(Oper o, Oper p)
    {
        OperCompare oc = new();
        Dictionary<Oper, int> d = new(oc);
        foreach (Oper c in p.posArgs)
        {
            d.TryGetValue(c, out int matches);
            d[c] = matches + 1;
        }
        foreach (Oper c in o.posArgs)
        {
            int matches;
            d.TryGetValue(c, out matches);
            if (matches > 0)
            {
                yield return c.Copy();
                d[c] = matches - 1;
            }
        }
    }
    public static IEnumerable<Oper> IntersectNeg(Oper o, Oper p)
    {
        OperCompare oc = new();
        Dictionary<Oper, int> d = new(oc);
        foreach (Oper c in p.negArgs)
        {
            d.TryGetValue(c, out int matches);
            d[c] = matches + 1;
        }
        foreach (Oper c in o.negArgs)
        {
            int matches;
            d.TryGetValue(c, out matches);
            if (matches > 0)
            {
                yield return c.Copy();
                d[c] = matches - 1;
            }
        }
    }
}
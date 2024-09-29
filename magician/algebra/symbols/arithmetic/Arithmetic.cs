namespace Magician.Alg.Symbols;

public abstract class Arithmetic : Invertable
{
    protected virtual new int Identity => throw Scribe.Issue("Identity was not implemented for arithmetic {this}");
    protected Arithmetic(string name, IEnumerable<Oper> pa, IEnumerable<Oper> na) : base(name, pa, na)
    {
        associative = true;
        commutative = true;
        trivialAssociative = true;
    }
    protected Arithmetic(string name, params Oper[] cstArgs) : base(name, cstArgs)
    {
        associative = true;
        commutative = true;
        trivialAssociative = true;
    }
    protected abstract Oper Handshake(Variable axis, Oper A, Oper B, Oper AB, bool aPositive, bool bPositive);

    public override void ReduceOuter()
    {
        Balance();
        // Combine constant terms
        List<Oper> posDetermined = posArgs.Where(o => o.IsDetermined).ToList();
        List<Oper> negDetermined = negArgs.Where(o => o.IsDetermined).ToList();
        posArgs.RemoveAll(o => o.IsDetermined);
        negArgs.RemoveAll(o => o.IsDetermined);

        Variable p = New(posDetermined, new List<Oper> { }).Sol();
        Variable n = New(negDetermined, new List<Oper> { }).Sol();
        posArgs.Add(p);
        if (!n.Value.EqValue(Identity))
            negArgs.Add(n);

        DropIdentities();
        if (posArgs.Count == 0)
            posArgs.Add(new Variable((int)Identity));
    }
    public void Combine(Variable? axis)
    {
        //Scribe.Info($"Combinin' {this}...");
        if (AllArgs.Count < 2)
            return;
        if (axis == null)
        {
            if (AssociatedVars.Count > 0)
                axis = AssociatedVars[0];
            else
                axis = Variable.Undefined;
        }
        List<Oper> finalPosArgs = new();
        List<Oper> finalNegArgs = new();

        var (fhs, gao) = GrpFlagHshakes(axis);
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

                    A.Reduce(3);
                    B.Reduce(3);
                    Oper AB = A.Trim().CommonFactors(B.Trim());
                    AB.Reduce(2);
                    AB = AB.Trim();

                    //Scribe.Info($"\tA, B: {A}, {B}, AB: {AB}");
                    bool aPositive = flaggedHandshake.Item3;
                    bool bPositive = flaggedHandshake.Item4;
                    bool positive = !(aPositive ^ bPositive);

                    if (termsFoundHandshake.Contains(i) || termsFoundHandshake.Contains(j))
                        continue;

                    //Scribe.Info($"\t\tA, B, AB: {A}, {B}, {AB}, {aPositive}, {bPositive}");
                    //AB.ReduceAll();

                    Oper combined = Handshake(axis, A, B, AB, aPositive, bPositive);
                    combined.Reduce();
                    //Scribe.Info($"\t\tCombined: {combined}");

                    if ((positive || aPositive) && (aPositive || bPositive))
                        finalPosArgs.Add(combined);
                    else
                        finalNegArgs.Add(combined);

                    //if (positive)
                    //    finalPosArgs.Add(combined);
                    //else
                    //{
                    //    finalNegArgs.Add(combined);
                    //}

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
        //Scribe.Info($"Now: {this}");
    }
    internal override void CombineOuter(Variable? axis=null)
    {
        Combine(axis);
        Reduce(2);
    }

    public override Oper Inverse(Oper axis, Oper? opp = null)
    {
        Oper inverse = New(posArgs, negArgs);
        OperLike ol = new();
        // find axis
        bool pos;
        if (posArgs.Contains(axis, ol))
            pos = true;
        else if (negArgs.Contains(axis, ol))
            pos = false;
        else
            throw Scribe.Error($"Inversion failed, as {name} {this} does not directly contain axis {axis}");

        opp ??= axis;

        // Flip everything
        (inverse.posArgs, inverse.negArgs) = (inverse.negArgs, inverse.posArgs);
        // Take the axis away and add it back to the opposite side
        if (pos)
        {
            inverse.negArgs = inverse.negArgs.Where(o => !o.Like(axis)).ToList();
            inverse.posArgs = new List<Oper> { opp }.Concat(inverse.posArgs).ToList();
        }
        else
        {
            inverse.posArgs = inverse.posArgs.Where(o => !o.Like(axis)).ToList();
            inverse.negArgs = new List<Oper> { opp }.Concat(inverse.negArgs).ToList();
            (inverse.posArgs, inverse.negArgs) = (inverse.negArgs, inverse.posArgs);
        }
        // path is always [(0, 0)]
        return inverse;
    }

    internal void DropIdentities()
    {
        if (posArgs.Count > 1)
            posArgs = posArgs.Where(o => !(o.IsConstant && ((Variable)o).Value.Trim().Dims == 1 && o.Sol().Value.EqValue(Identity))).ToList();
        if (negArgs.Count > 0)
            negArgs = negArgs.Where(o => !(o.IsConstant && ((Variable)o).Value.Trim().Dims == 1 && o.Sol().Value.EqValue(Identity))).ToList();
    }

    internal Dictionary<string, (Oper, int)> ArgBalance()
    {
        Dictionary<string, (Oper op, int co)> operCos = new();
        foreach (Oper o in posArgs)
        {
            string ord = o.Ord();
            if (operCos.ContainsKey(ord))
            {
                operCos[ord] = (operCos[ord].op, operCos[ord].co+1);
            }
            else
            {
                operCos.Add(ord, (o, 1));
            }
        }
        foreach (Oper o in negArgs)
        {
            string ord = o.Ord();
            if (operCos.ContainsKey(ord))
            {
                operCos[ord] = (operCos[ord].op, operCos[ord].co-1);
            }
            else
            {
                operCos.Add(ord, (o, -1));
            }
        }
        return operCos;
    }

    public void Balance()
    {
        Dictionary<string, (Oper op, int co)> operCos = ArgBalance();
        posArgs.Clear();
        negArgs.Clear();
        foreach (string ord in operCos.Keys)
        {
            Oper o = operCos[ord].op;
            int coefficient = operCos[ord].co;
            if (coefficient == 0)
            {
                // Update assoc arg info on reduce
                if (o is Variable v && !v.Found)
                {
                    if (AssociatedVars.Contains(v))
                        AssociatedVars.Remove(v);
                }
            }
            else if (coefficient > 0)
            {
                while (coefficient-- > 0)
                    posArgs.Add(o.Copy());
                if (o is Variable v && !v.Found)
                {
                    if (!AssociatedVars.Contains(v))
                        AssociatedVars.Add(v);
                }
            }
            else if (coefficient < 0)
            {
                while (coefficient++ < 0)
                    negArgs.Add(o.Copy());
                if (o is Variable v && !v.Found)
                {
                    if (!AssociatedVars.Contains(v))
                        AssociatedVars.Add(v);
                }
            }
        }
    }
    // Flagged grouped handshakes, flagged grouped opers, pos/neg filtered args
    internal (List<List<(int, int, bool, bool)>>, List<List<(Oper, bool)>>) GrpFlagHshakes(Variable axis)
    {

        List<(Oper, bool)> argsContainingAxis = new();
        List<(Oper, bool)> argsNotContainingAxis = new();
        List<List<(int, int, bool, bool)>> groupedHandshakes = new() { new() { }, new() { } };

        // Group the Opers into terms containing the axis, and terms not containing the axis
        int posLimit = posArgs.Count;
        for (int i = 0; i < AllArgs.Count; i++)
        {
            Oper o = AllArgs[i];
            if (o.AssociatedVars.Contains(axis) || (o is Variable v && v == axis))
                argsContainingAxis.Add((o, i < posLimit));
            else
                argsNotContainingAxis.Add((o, i < posLimit));
        }
        int c = 0;
        foreach (List<(Oper, bool parity)> flaggedArgs in new List<List<(Oper, bool)>> { argsNotContainingAxis, argsContainingAxis })
        {
            if (flaggedArgs.Count % 2 != 0)
                flaggedArgs.Add((new Variable(Identity), true));
            for (int i = 0; i < flaggedArgs.Count; i++)
                for (int j = i + 1; j < flaggedArgs.Count; j++)
                    groupedHandshakes[c].Add((i, j, flaggedArgs[i].parity, flaggedArgs[j].parity));
            c++;
        }

        return (groupedHandshakes, new List<List<(Oper, bool)>>
            { argsNotContainingAxis, argsContainingAxis });
    }
}
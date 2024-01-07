namespace Magician.Symbols;

public abstract class Arithmetic : Invertable
{
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

        Variable p = New(posDetermined, new List<Oper>{}).Sol();
        Variable n = New(negDetermined, new List<Oper>{}).Sol();
        posArgs.Add(p);
        if (n.Value.Magnitude != Identity)
            negArgs.Add(n);

        // Remove unnecessary arguments
        if (Identity is null)
            return;
        DropIdentities();
        if (posArgs.Count == 0)
            posArgs.Add(new Variable((int)Identity));
    }
    internal void Combine(Variable? axis)
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
                    Oper AB = LegacyForm.Shed(A).CommonFactors(LegacyForm.Shed(B));
                    AB.Reduce(2);
                    AB = LegacyForm.Shed(AB);

                    //Scribe.Info($"\tA, B: {A}, {B}, AB: {AB}");
                    bool aPositive = flaggedHandshake.Item3;
                    bool bPositive = flaggedHandshake.Item4;
                    bool positive = !(aPositive ^ bPositive);

                    if (termsFoundHandshake.Contains(i) || termsFoundHandshake.Contains(j))
                        continue;

                    //Scribe.Info($"\t\tA, B, AB: {A}, {B}, {AB}, {aPositive}, {bPositive}");
                    //AB.ReduceAll();
                    
                    Oper combined = Handshake(axis, A, B, AB, aPositive, bPositive);
                    //Scribe.Info($"\t\tCombined: {combined}");

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
        //Scribe.Info($"Now: {this}");
    }
    internal override void SimplifyOuter(Variable? axis = null)
    {
        Combine(axis);
        Reduce(2);
    }

    public override Oper Inverse(Oper axis, Oper? opp=null)
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
            inverse.posArgs = new List<Oper> {opp}.Concat(inverse.posArgs).ToList();
        }
        else
        {
            inverse.posArgs = inverse.posArgs.Where(o => !o.Like(axis)).ToList();
            inverse.negArgs = new List<Oper> {opp}.Concat(inverse.negArgs).ToList();
            (inverse.posArgs, inverse.negArgs) = (inverse.negArgs, inverse.posArgs);
        }
        // path is always [(0, 0)]
        return inverse;
    }
}
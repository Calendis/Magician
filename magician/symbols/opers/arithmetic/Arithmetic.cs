namespace Magician.Symbols;

public abstract class Arithmetic : Oper
{
    protected Arithmetic(string name, IEnumerable<Oper> pa, IEnumerable<Oper> na) : base(name, pa, na) { }
    protected Arithmetic(string name, params Oper[] cstArgs) : base(name, cstArgs) { }
    protected abstract Oper Handshake(Variable axis, Oper A, Oper B, Oper AB, bool aPositive, bool bPositive);

    public override void Reduce()
    {
        // Drop things found in both sets of arguments
        if (commutative)
            Balance();
        for (int i = 0; i < posArgs.Count; i++)
            posArgs[i] = Form.Shed(posArgs[i]);
        for (int i = 0; i < negArgs.Count; i++)
            negArgs[i] = Form.Shed(negArgs[i]);

        // Combine constant terms
        List<Oper> posDetermined = posArgs.Where(o => o.IsDetermined).ToList();
        List<Oper> negDetermined = negArgs.Where(o => o.IsDetermined).ToList();
        posArgs.RemoveAll(o => o.IsDetermined);
        negArgs.RemoveAll(o => o.IsDetermined);
        double posMag = New(posDetermined, new List<Oper>{}).Solution().Val;
        double negMag = New(negDetermined, new List<Oper>{}).Solution().Val;
        Variable consts;
        if (posMag >= negMag)
        {
            consts = New(posDetermined, negDetermined).Solution();
            posArgs.Add(consts);
        }
        else
        {
            consts = New(negDetermined, posDetermined).Solution();
            negArgs.Add(consts);
        }
        

        if (Identity is null)
            return;
        DropIdentities();
        MakeExplicit();
    }
    public void Combine(Variable? axis)
    {
        if (AllArgs.Count < 2)
            return;
        axis ??= new Variable(0);
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

                    Oper AB = Intersect(Form.Term(A), Form.Term(B));

                    bool aPositive = flaggedHandshake.Item3;
                    bool bPositive = flaggedHandshake.Item4;
                    bool positive = !(aPositive ^ bPositive);

                    if (termsFoundHandshake.Contains(i) || termsFoundHandshake.Contains(j))
                        continue;

                    /* Inner combine */
                    Oper combined;
                    if (A is Variable av && av.Found && av.Val == 0)
                        combined = B;
                    else if (B is Variable bv && bv.Found && bv.Val == 0)
                        combined = A;
                    else
                        combined = Handshake(axis, A, B, AB, aPositive, bPositive);
                    /* End inner combine */
                    //Scribe.Info($"  combined: {combined}");

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
        Reduce();
    }
    public override void Simplify(Variable? axis = null)
    {
        Combine(axis);
        ReduceAll();
        base.Simplify(axis);
    }
}
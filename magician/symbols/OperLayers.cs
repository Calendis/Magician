namespace Magician.Symbols;

internal class OperLayers
{
    readonly List<List<Oper>> OpTree = new();
    Dictionary<Oper, OperInfo> OpInfoMap = new();
    public OperLayers(Oper o, Variable v)
    {
        OpTree = new() {new() {o}};
        GatherInfo(o, v);
    }

    void GatherInfo(Oper o, Variable v, int depth=0, List<Variable>? assocArgs=null, bool isTerm=true, List<List<Oper>>? opTree=null)
    {
        opTree ??= OpTree;
        assocArgs ??= new();
        if (opTree.Count > depth)
            opTree[depth].Add(o);
        else if (OpTree.Count == depth)
            opTree.Add(new List<Oper>{o});
        else
            throw Scribe.Issue("this can't happen");

        foreach (Oper a in o.AllArgs)
        {
            if (a is Variable v2 && !v2.Found)
                assocArgs.Add(v2);
            GatherInfo(a, v, depth+1, assocArgs, o is not SumDiff && isTerm, opTree);
        }
        OperInfo i = new(o, v, assocArgs, isTerm);
        OpInfoMap.TryAdd(o, i);
    }
    
    public List<Oper> Root => OpTree[0];
    public Oper Get(int n, int k)
    {
        return OpTree[n][k];
    }
    public OperInfo GetInfo(int n, int k)
    {
        return GetInfo(Get(n, k));
    }
    public OperInfo GetInfo(Oper o)
    {
        return OpInfoMap[o];
    }
    public List<Oper> LiveBranches(Oper o)
    {
        return o.AllArgs.Where(a => GetInfo(a).ms != Equation.MatchState.NONE).ToList();
    }
    public List<Oper> LiveBranches(int n, int k)
    {
        return LiveBranches(Get(n, k));
    }
}

internal readonly struct OperInfo
{
    public readonly double deg;
    public readonly Equation.MatchState ms;
    public readonly List<Variable> assocArgs;
    public OperInfo(Oper o, Variable v, IEnumerable<Variable> assocArgs, bool isTerm)
    {
        deg = o.Degree(v);
        this.assocArgs = assocArgs.ToList();
        ms = Equation.MatchState.NONE;
        if (o is Variable v2)
            if (v2 == v)
                ms = Equation.MatchState.DIRECT;
            else
                ms = Equation.MatchState.NONE;
        else if (assocArgs.Contains(v))
        {
            if (isTerm)
                ms = Equation.MatchState.INDIRECT;
            else
                ms = Equation.MatchState.INDIRECT;
        }
    }
}
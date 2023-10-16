namespace Magician.Symbols;

internal class OperLayers
{
    readonly List<List<Oper>> OpTree = new();
    readonly Dictionary<Oper, OperInfo> OpInfoMap = new();
    //public readonly List<(int, int)> Matches = new();
    public OperLayers(Oper o, Variable v)
    {
        OpTree = new() { new() { o } };
        GatherInfo(o, v);
    }

    void GatherInfo(Oper o, Variable v, int depth = 0, List<Variable>? assocArgs = null, List<List<Oper>>? opTree = null)
    {
        opTree ??= OpTree;
        assocArgs ??= new();
        if (opTree.Count > depth)
            opTree[depth].Add(o);
        else if (OpTree.Count == depth)
            opTree.Add(new List<Oper> { });
        else
            throw Scribe.Issue("this can't happen");

        if (o is Variable u && !u.Found)
            assocArgs.Add(u);

        List<List<Variable>> assocArgBranches = new();
        foreach (Oper a in o.AllArgs)
        {
            assocArgBranches.Add(new());
            GatherInfo(a, v, depth + 1, assocArgBranches.Last(), opTree);
        }
        List<Variable> combinedAssocBranches = new();
        assocArgBranches.ForEach(assocArgBranch => combinedAssocBranches.AddRange(assocArgBranch));
        assocArgs.AddRange(combinedAssocBranches);

        OperInfo i = new(o, v, assocArgs);
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
    public OperInfo(Oper o, Variable v, IEnumerable<Variable> assocArgs)
    {
        deg = o.Degree(v);
        this.assocArgs = assocArgs.ToList();
        int liveBranches = assocArgs.Where(a => { return a == v; }).Count();
        ms = Equation.MatchState.NONE;
        if (o is Variable v2 && v2 == v)
            ms = Equation.MatchState.DIRECT;
        else if (liveBranches > 1)
            ms = Equation.MatchState.MULTIPLE;
        else if (liveBranches > 0)
            ms = Equation.MatchState.SINGLE;
    }
}
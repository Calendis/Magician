namespace Magician.Symbols;

internal class OperLayers
{
    public List<List<Oper>> OpTree = new();
    public Dictionary<Oper, OperInfo> OpInfoMap = new();
    public OperLayers(Oper o)
    {
        OpTree = new() {new() {o}};
        GatherInfo(o);
    }

    void GatherInfo(Oper o, int depth=0, List<List<Oper>>? opTree=null)
    {
        if (opTree is null)
            opTree = OpTree;
        OperInfo i = new OperInfo(o);

        if (opTree.Count > depth)
        {
            OpTree[depth].Add(o);
            OpInfoMap.TryAdd(o, i);
        }
        else if (OpTree.Count == depth)
        {
            OpTree.Add(new List<Oper>{o});
            OpInfoMap.TryAdd(o, i);
        }
        else
        {
            throw Scribe.Issue("this can't happen");
        }
        foreach (Oper a in o.AllArgs)
        {
            GatherInfo(a, depth+1, opTree);
        }
    }
    
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
}

internal readonly struct OperInfo
{
    readonly List<Variable> eventContains;
    readonly double deg;
    readonly bool empty;
    public OperInfo(Oper o)
    {
        eventContains = new();
        deg = -1;
        empty = true;
    }
}
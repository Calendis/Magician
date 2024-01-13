namespace Magician.Symbols;
using Core;

public class Multivalue : Variable
{
    public IVal Principal => principal;
    public IVal[] Remaining => remaining;
    readonly IVal principal;
    readonly IVal[] remaining;
    public Multivalue(params double[] vs) : this(vs.Select(d => new Val(d)).ToArray()) {}
    public Multivalue(params IVal[] vs) : this("multivalue", vs) {}
    public Multivalue(string n, params IVal[] vs) : base(n, vs[0])
    {
        if (vs.Length == 0)
            throw Scribe.Error("Cannot create empty multivalue");
        principal = vs[0];
        remaining = vs.Skip(1).ToArray();
    }
    // TODO: arithmetic methods for Multivalues
}
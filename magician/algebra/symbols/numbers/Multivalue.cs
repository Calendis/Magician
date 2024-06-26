namespace Magician.Alg.Symbols;
using Core;

public class Multivalue : Variable
{
    public IVal Principal => Value;
    public IVal[] All => new List<IVal>{Value}.Concat(remaining).ToArray();
    public int Solutions => remaining.Length + 1;
    //IVal principal;
    IVal[] remaining;
    //public Multivalue(params double[] vs) : this(vs.Select(d => new Val(d)).ToArray()) {}
    public Multivalue(params IVal[] vs) : this("multivalue", vs) {}
    public Multivalue(string n, params IVal[] vs) : base(n, vs[0])
    {
        if (vs.Length == 0)
            throw Scribe.Error("Cannot create empty multivalue");
        Value.Set(vs[0]);
        remaining = vs.Skip(1).ToArray();
    }

    public override void Set(List<IVal> vs)
    {
        if (vs.Count == 0)
            throw Scribe.Error("Cannot create empty multivalue");
        Value.Set(vs[0].Values);
        remaining = vs.Skip(1).ToArray();
    }

    public override Multivalue Copy()
    {
        return new(Name, All);
    }

    public override string ToString()
    {
        if (Found)
            return Solutions == 1 ? $"{Value.Get()}" : $"{Value.Get()}"+", "+remaining.Aggregate("", (a, b) => a += $", {b}")[2..];
        return base.ToString();
    }
    // TODO: arithmetic methods for Multivalues
}
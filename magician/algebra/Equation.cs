namespace Magician.Alg;

using Magician.Core.Maps;
using Symbols;

public partial class Equation
{
    public List<Variable> Unknowns { get; }  // all unknowns
    public List<Variable> Constants => Unknowns.Where(v => v.Found).ToList();
    public Oper LHS { get; private set; }
    public Oper RHS { get; private set; }
    public Fulcrum Fulcrum => fulcrum;
    Fulcrum fulcrum;
    Solver? solver = null;
    public Equation(Oper o0, Fulcrum f, Oper o1)
    {
        fulcrum = f;
        LHS = o0;
        RHS = o1;

        List<Variable> initialIsolates = new();
        if (o0 is Variable v && !v.Found)
        {
            initialIsolates.Add(v);
        }
        if (o1 is Variable v2 && !v2.Found)
        {
            initialIsolates.Add(v2);
        }
        OperLayers lhs = new(LHS, Variable.Undefined);
        OperLayers rhs = new(RHS, Variable.Undefined);
        Unknowns = lhs.GetInfo(0, 0).assocArgs.Concat(rhs.GetInfo(0, 0).assocArgs).Union(initialIsolates).ToList();
    }

    // Re-arrange and reconstruct the equation in terms of a certain variable
    public IRelation Solved(Variable? v = null)
    {
        solver = new(LHS.Copy(), RHS.Copy(), v);
        solver.Run();
        return solver.Relation;
    }
    public Equation Rearranged(Oper targ)
    {
        return Rearranged_old(targ);
        //solver = new(LHS, RHS, targ);
        //solver.Run();
        //return new(solver.LHS, solver.FULCRUM, solver.RHS);
    }

    public override string ToString()
    {
        string fulcrumString = "";
        switch (fulcrum)
        {
            case Fulcrum.EQUALS:
                fulcrumString = "=";
                break;
            case Fulcrum.LESSTHAN:
                fulcrumString = "<";
                break;
            case Fulcrum.GREATERTHAN:
                fulcrumString = ">";
                break;
            case Fulcrum.LSTHANOREQTO:
                fulcrumString = "<=";
                break;
            case Fulcrum.GRTHANOREQTO:
                fulcrumString = ">=";
                break;
        }
        return $"{LHS} {fulcrumString} {RHS}";
    }
}
// The fulcrum is the =, >, <, etc.
// TODO: actually support inequalities
public enum Fulcrum
{
    EQUALS, LESSTHAN, GREATERTHAN, LSTHANOREQTO, GRTHANOREQTO
}
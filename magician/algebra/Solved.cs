namespace Magician.Alg;
using Symbols;
using Core;
using Core.Maps;

public class SolvedEquation : IRelation
{
    public Variable Chosen => chosenRoot;
    public Equation Equation => eq;
    public Oper Opposite => eq.RHS;
    public int Ins => eq.Unknowns.Count - eq.Sliders.Count - 1;
    public IVal Cache {get; private set;}
    readonly Equation eq;
    readonly Variable chosenRoot;
    public SolvedEquation(Variable chosenRoot, Fulcrum fulc, Oper oppositeRoot)
    {
        eq = new(chosenRoot, fulc, oppositeRoot);
        this.chosenRoot = chosenRoot;
        Cache = new Val(0);
    }

    public IVal Evaluate(params double[] vals)
    {
        if (vals.Length != Ins)
        {
            throw Scribe.Error($"Equation expected {Ins} arguments, got {vals.Length}");
        }

        List<Variable> unknowns = Opposite.AssociatedVars.ToList();
        unknowns.Remove(Chosen);
        unknowns = unknowns.Except(eq.Sliders).ToList();
        // Sort the arguments so you don't get inconsistent behaviour
        unknowns = unknowns.OrderBy(v => v.Name).ToList();
        // Set the values
        int counter = 0;
        foreach(double val in vals)
        {
            unknowns[counter++].Set(val);
        }
        Cache.Set(Opposite.Sol());
        unknowns.ForEach(v => v.Reset());
        return Cache;
    }

    public override string ToString()
    {
        return eq.ToString();
    }
}
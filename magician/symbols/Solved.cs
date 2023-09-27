namespace Magician.Symbols;
using Magician.Maps;

public class SolvedEquation : InverseParamMap
{
    Equation eq;
    Variable solvedVar;
    Oper chosen, opposite;
    public Equation Eq => eq;
    public Variable SolvedVar => solvedVar;

    // Constructs a solved equation from sides
    public SolvedEquation(Oper chosenRoot, Fulcrum fulc, Oper oppositeRoot, Variable v, int ins) : base(null, ins)
    {
        eq = new(chosenRoot, fulc, oppositeRoot);
        chosen = chosenRoot;
        opposite = oppositeRoot;
        solvedVar = v;
        map = MapFromIPFunc(Evaluate);
    }

    public override double Evaluate(params double[] vals)
    {
        if (vals.Length != eq.Unknowns.Length - 1)
        {
            throw Scribe.Error($"Equation expected {eq.Unknowns.Length - 1} arguments, got {vals.Length}");
        }
        int counter = 0;
        List<Variable> knowns = eq.Unknowns.ToList();
        knowns.Remove(solvedVar);
        
        foreach (Variable x in knowns)
        {
            x.Val = vals[counter++];
        }
        
        Variable result = opposite.Solution();
        Array.ForEach(eq.Unknowns, v => v.Reset());
        Array.ForEach<Variable>(knowns.ToArray(), v => v.Reset());
        return result.Val;
    }

    public override string ToString()
    {
        return eq.ToString();
    }
}
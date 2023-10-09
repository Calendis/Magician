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
    public Multi Plot(params (Variable, PlotOptions)[] varPairedOptions)
    {
        // Every Unknown not paired with an axis becomes a slider
        List<Variable> pairedUnknowns = varPairedOptions.Select(t => t.Item1).ToList();
        eq.Sliders = eq.Unknowns.Except(pairedUnknowns.Append(solvedVar)).ToList();
        // The map can temporarily be considered to have fewer inputs
        ins -= eq.Sliders.Count;
        Multi plot = base.Plot(varPairedOptions.Select(t => t.Item2).ToArray());
        // Restore the proper number of inputs
        ins += eq.Sliders.Count;
        eq.Sliders.Clear();
        return plot;
    }

    public override double Evaluate(params double[] vals)
    {
        int expectedArgs = eq.Unknowns.Count - eq.Sliders.Count - 1;
        if (vals.Length != expectedArgs)
        {
            throw Scribe.Error($"Equation expected {expectedArgs} arguments, got {vals.Length}");
        }
        int counter = 0;
        List<Variable> unknowns = eq.Unknowns.ToList();
        unknowns.Remove(solvedVar);
        unknowns = unknowns.Except(eq.Sliders).ToList();
        
        // Set the values
        foreach (Variable x in unknowns)
        {
            x.Val = vals[counter++];
        }
        
        Variable result = opposite.Solution();
        unknowns.ForEach(v => v.Reset());
        return result.Val;
    }

    public override string ToString()
    {
        return eq.ToString();
    }
}
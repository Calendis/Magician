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
    public SolvedEquation(Oper chosenRoot, Fulcrum fulc, Oper oppositeRoot, Variable v, int ins) : base(oppositeRoot.Evaluate, ins)
    {
        eq = new(chosenRoot, fulc, oppositeRoot);
        chosen = chosenRoot;
        opposite = oppositeRoot;
        solvedVar = v;

        Ins = ins;
        //map = MapFromIPFunc(Evaluate);
    }
    public Multi Plot(AxisSpecifier outAxis, params (Variable, PlotOptions)[] varPairedOptions)
    {
        // Every Unknown not paired with an axis becomes a slider
        List<Variable> pairedUnknowns = varPairedOptions.Select(t => t.Item1).ToList();
        eq.Sliders = eq.Unknowns.Except(pairedUnknowns.Append(solvedVar)).ToList();
        // The map can temporarily be considered to have fewer inputs
        Ins -= eq.Sliders.Count;
        //Multi plot = new InverseParamMap(solvedSide == 0 ? eq.LHS.Evaluate : eq.RHS.Evaluate, eq.Ins).Plot(varPairedOptions.Select(t => t.Item2).ToArray());
        Multi plot = base.Plot(outAxis, varPairedOptions.Select(t => t.Item2).ToArray());
        // Restore the proper number of inputs
        Ins += eq.Sliders.Count;
        eq.Sliders.Clear();
        return plot;
    }

    public new double Evaluate(params double[] vals)
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
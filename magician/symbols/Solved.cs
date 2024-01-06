namespace Magician.Symbols;
using Magician.Maps;

public class SolvedEquation : InverseParamMap
{
    Equation eq;
    Variable solvedVar;
    public Oper chosen, opposite;
    public Equation Eq => eq;
    public Variable SolvedVar => solvedVar;
    // Constructs a solved equation from sides
    // TODO: chosenRoot and v are redundant. We can make chosenRoot a Variable and eliminate v
    public SolvedEquation(Oper chosenRoot, Fulcrum fulc, Oper oppositeRoot, Variable v, int ins) : base(oppositeRoot.Evaluate, ins)
    {
        eq = new(chosenRoot, fulc, oppositeRoot);
        chosen = chosenRoot;
        opposite = oppositeRoot;
        //opposite.AssociatedVars.Sort((v0, v1) => v0.Name[0] < v1.Name[0] ? 1 : v0.Name[0] > v1.Name[0] ? -1 : 0);
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

    public new IVal Evaluate(params double[] vals)
    {
        int expectedArgs = eq.Unknowns.Count - eq.Sliders.Count - 1;
        if (vals.Length != expectedArgs)
        {
            throw Scribe.Error($"Equation expected {expectedArgs} arguments, got {vals.Length}");
        }
        int counter = 0;
        //List<Variable> unknowns = eq.Unknowns.ToList();
        List<Variable> unknowns = opposite.AssociatedVars.ToList();

        unknowns.Remove(solvedVar);
        unknowns = unknowns.Except(eq.Sliders).ToList();

        // Sort the arguments so you don't get inconsistent behaviour
        unknowns = unknowns.OrderBy(v => v.Name).ToList();
        // Set the values
        //Scribe.Warn($"Got vals: {Scribe.Expand<List<double>, double>(vals.ToList())}, for vars {Scribe.Expand<List<Variable>, Variable>(unknowns)}");
        foreach (Variable x in unknowns)
        {
            x.Set(vals[counter++]);
        }

        Variable result = opposite.Sol();
        unknowns.ForEach(v => v.Reset());
        return result;
    }

    public override string ToString()
    {
        return eq.ToString();
    }
}
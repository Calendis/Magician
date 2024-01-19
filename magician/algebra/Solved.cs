namespace Magician.Alg;
using Symbols;
using Core;
using Core.Maps;
using Geo;

public class SolvedEquation : Relational
{
    Equation eq;
    Variable solvedVar;
    public Oper chosen, opposite;
    public Equation Eq => eq;
    public Variable SolvedVar => solvedVar;
    // Constructs a solved equation from sides
    // TODO: chosenRoot and v are redundant. We can make chosenRoot a Variable and eliminate v
    public SolvedEquation(Oper chosenRoot, Fulcrum fulc, Oper oppositeRoot, Variable v, int ins) : base(args => oppositeRoot.Evaluate(args).Values.ToArray(), ins)
    {
        eq = new(chosenRoot, fulc, oppositeRoot);
        chosen = chosenRoot;
        opposite = oppositeRoot;
        solvedVar = v;
        Ins = ins;
    }
    public Node Plot(AxisSpecifier outAxis, params (Variable, PlotOptions)[] varPairedOptions)
    {
        // Every Unknown not paired with an axis becomes a slider
        List<Variable> pairedUnknowns = varPairedOptions.Select(t => t.Item1).ToList();
        eq.Sliders = eq.Unknowns.Except(pairedUnknowns.Append(solvedVar)).ToList();
        // The map can temporarily be considered to have fewer inputs
        Ins -= eq.Sliders.Count;
        //Multi plot = new InverseParamMap(solvedSide == 0 ? eq.LHS.Evaluate : eq.RHS.Evaluate, eq.Ins).Plot(varPairedOptions.Select(t => t.Item2).ToArray());
        Node plot = base.Plot(outAxis, varPairedOptions.Select(t => t.Item2).ToArray());
        // Restore the proper number of inputs
        Ins += eq.Sliders.Count;
        eq.Sliders.Clear();
        return plot;
    }

    public override string ToString()
    {
        return eq.ToString();
    }
}
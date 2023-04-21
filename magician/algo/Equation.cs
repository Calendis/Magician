namespace Magician.Algo;

// Re-write of IMap
public class Equation
{
    int knowns;
    int unknowns;
    int isolates;
    public Equation(Oper o0, Fulcrum f, Oper o1)
    {
        List<Variable> vars0 = new(), vars1 = new();
        int knowns0 = 0, knowns1 = 0, unknowns0 = 0, unknowns1 = 0;
        o0.CollectVariables(ref vars0, ref knowns0, ref unknowns0);
        o1.CollectVariables(ref vars1, ref knowns1, ref unknowns1);

        knowns = knowns0 + knowns1;
        unknowns = unknowns0 + unknowns1;
        Scribe.Info($"Creating new equation with {unknowns} unknowns and {knowns} knowns");
        Variable[] vars = new Variable[unknowns];
        
        //o0.Commute(0, 1);
        Scribe.Info($"Collected {vars0.Count} symbols from the left-hand side, and {vars1.Count} from the right-hand side");
        foreach (Variable v in vars0)
        {
            Scribe.Info(v);
        }
    }

    public enum AxisSpecifier
    {
        X, Y, Z, HUE, TIME
    }
}

// The fulcrum is the =, >, <, etc.
public enum Fulcrum
{
    EQUALS, LESSTHAN, GREATERTHAN, LSTHANOREQTO, GRTHANOREQTO
}
namespace Magician.Algo;

// Re-write of IMap
public class Equation
{
    int knowns;
    int unknowns;
    int isolates;
    public Equation(Oper o0, Fulcrum f, Oper o1)
    {
        unknowns = o0.hasUnknownVars + o1.hasUnknownVars;
        knowns = o0.numArgs + o1.numArgs - unknowns;
        Scribe.Info($"Creating new equation with {unknowns} unknowns and {knowns} knowns");
        Variable[] vars = new Variable[unknowns];
        
        // Detect isolates
        Variable[] unknownVars0, unknownVars1;
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
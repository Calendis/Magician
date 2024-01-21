namespace Magician.Demos.Tests;
using Core.Caster;
using static Alg.Notate;
using Alg.Symbols;
using Magician.Geo;
using Magician.Alg;

public class ImplicitGeometry : Spell
{
    public override void PreLoop()
    {
        Scribe.Info("Generalized expression rendering...");

        // Expression we want to render
        //Oper oper = Var("x").Divide(Var("z").Pow(Val(2)).Plus(Val(1)));
        Equation eq = new(
        new SumDiff(new ExpLog(new List<Oper>{Var("x"), Val(2)}, new List<Oper>{}), Val(0), new ExpLog(new List<Oper>{Var("y"), Val(2)}, new List<Oper>{}), Val(0), new ExpLog(new List<Oper>{Var("z"), Val(2)}, new List<Oper>{})),
            Fulcrum.EQUALS,
            new Fraction(Val(1), Val(0.00000135)).Plus(Val(30000))
        );
        SolvedEquation se = eq.Solved();

        // Create geometry from expression
        Geo.Implicit geo = new(se, 0, 0, 0, 1, (5, 0.01), (5, 0.01));
        
        // Add our geometry to the scene
        Origin["myGeo"] = geo;

    }
    public override void Loop()
    {
        //
    }
}
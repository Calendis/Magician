using Magician.Algo;
using Magician.Library;
using static Magician.Algo.Algebra;

namespace Magician.Demos.Tests;

public class Plots : Spell
{
    public override void PreLoop()
    {
        Origin["plot0"] = ((IMap)new CustomMap(x => 60 * Math.Sin(x/6))).Plot(0, 0, 0, 96*Math.PI, 1, HSLA.RandomVisible());
        Origin["plot1"] = new IOMap(1, t => 60*Math.Sin(t), t => 60*Math.Cos(t/5)).Plot(-300, -300, 0, 96*Math.PI, 1, HSLA.RandomVisible());
        //Origin["plot2"] = new IOMap(2, (x, y) => x + Math.Sin(y)).Plot();
        //Origin["plot2"] = new IOMap(2, x => x + Math.Sin(x)).Plot();

        // Algebra testing
        Oper hyperbola = new Mult(Let("x"), Let("y"));
        Oper scale = N(1);
        Equation.Fulcrum equalsSign = Equation.Fulcrum.EQUALS;
        //Equation e = new(hyperbola, equalsSign, scale);

        //Scribe.Info("### CLEARING CACHE ###");
        MathCache.freeVars.Clear();

        //Oper leftHand = new Plus(Let("y"), N(3));
        //Oper rightHand = new Plus(Let("x"), N(4));
        //Equation f = new(leftHand, Equation.Fulcrum.EQUALS, rightHand);
        //Scribe.Info(f);
        //Scribe.Info("Solving for x...");
        //Equation solved = f.Solve(Let("x"));
        //Scribe.Info(solved);
        //Scribe.Info("Solving for y...");
        //solved = f.Solve(Let("y"));
        //Scribe.Info(solved);

        //Oper leftHand = new Plus(new Minus(Let("x"), N(5)), N(9));
        //Oper rightHand = new Plus(Let("y"), N(4));
        //Equation f = new(leftHand, Equation.Fulcrum.EQUALS, rightHand);
        //Scribe.Info(f);
        //Scribe.Info("Solving for x...");
        //Equation solved = f.Solve(Let("x"));
        //Scribe.Info("Done.");
        //Scribe.Info(solved);

        Oper leftHand =  new Minus(Let("x"), N(5)); 
        Oper rightHand = new Minus(new Plus(Let("y"), N(4)), N(9));
        Equation f = new(leftHand, Equation.Fulcrum.EQUALS, rightHand);
        Scribe.Info(f);
        Scribe.Info("Solving for x...");
        Equation solved = f.Solve(Let("x"));
        Scribe.Info("Done");
        Scribe.Info(solved);
    }

    public override void Loop()
    {
        //
    }
}
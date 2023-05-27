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
        Oper lhs = new Fraction(new SumDiff(Let("x"), N(0), N(1)), new SumDiff(N(4), Let("y")));
        Oper rhs = new Fraction(N(3), N(1), Let("z"));
        Equation e = new(lhs, Equation.Fulcrum.EQUALS, rhs);
        
        Scribe.Info("Equation:");
        Scribe.Info(e);
        
        //Scribe.Info("Solving for x...");
        //Equation sx = e.Solve(Let("x"));
        //Scribe.Info(sx);
        
        //Scribe.Info("Solving for y...");
        //Equation sy = e.Solve(Let("y"));
        //Scribe.Info(sy);
        
        Scribe.Info("Solving for z...");
        Equation sz = e.Solve(Let("z"));
        Scribe.Info(sz);
    }

    public override void Loop()
    {
        //
    }
}
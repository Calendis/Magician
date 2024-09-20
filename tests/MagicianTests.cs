namespace Magician.Tests;
using Core;
using Alg;
using Alg.Symbols;
using Alg.Symbols.Commonfuncs;
using static Alg.Notate;
using NUnit.Framework;
using Magician.Core.Maps;

public class SimpleAlgebraCases
{
    [Test]
    public void Subsumption()
    {
        Scribe.Info(Variable.Undefined);
        Oper o0 = new Abs(new Max(new Fraction(new Abs(new SumDiff(new Abs(Val(4096)))))));
        Scribe.Info(o0);
        o0.Reduce();
        Scribe.Info(o0);
        Assert.That(o0.Like(new Abs(Val(4096))));
    }

    [Test]
    public void SumDiffBasic()
    {
        SumDiff sd = new(Val(5), Val(2));
        Scribe.Info($"sd: {sd}, sol: {sd.Sol()}");
        Assert.That(sd.Sol().Like(Val(3)));
    }

    [Test]
    public void AddingOrSubtractingAnOperFromItself()
    {
        Oper o = new Fraction(
            Var("x"),
            new SumDiff(
                Val(1), new ExpLog(new List<Oper> { Var("x"), Val(2) }, new List<Oper> { })
            )
        );

        SumDiff doubl = new(new List<Oper> { o.Copy(), o.Copy() }, new List<Oper> { });
        SumDiff nothing = new(o.Copy(), o.Copy());
        Scribe.Info(doubl);
        doubl.Combine(Var("x"));
        Scribe.Info(doubl);
        doubl.Reduce();
        nothing.ReduceOuter();

        Oper doublManual = new Fraction(new List<Oper> { Val(2), o }, new List<Oper> { });
        doublManual.Commute(); doubl.Commute(); doublManual.Associate();
        Scribe.Info($"  Got {doubl.GetType().Name} {doubl}, need {doublManual}");
        Assert.That(doubl.Trim().Like(doublManual));
        Assert.That(nothing.Trim().Like(Val(0)));
    }

    [Test]
    public void BasicFracSimp()
    {
        Scribe.Info($"Start of test...");
        Fraction f = new Fraction(new List<Oper> { Val(3), Var("x"), Var("x") }, new List<Oper> { });
        Scribe.Info($"Created f");
        Fraction need = new Fraction(new List<Oper> { Val(3), new ExpLog(new List<Oper> { Var("x"), Val(2) }, new List<Oper> { }) }, new List<Oper> { });

        Scribe.Info($"{f.Canonical()} vs. {need.Canonical()}");
        Assert.That(f.Canonical().Like(need.Canonical()));
    }
    [Test]
    public void NextFracSimp()
    {
        Fraction f = new(new List<Oper> { Val(3), Var("x"), new ExpLog(new List<Oper> { Var("x"), Val(2) }, new List<Oper> { }) }, new List<Oper> { });
        Fraction need = new(new List<Oper> { Val(3), new ExpLog(new List<Oper> { Var("x"), Val(3) }, new List<Oper> { }) }, new List<Oper> { });

        Scribe.Info($"{f} will become {f.Canonical()}, which should be equal to {need}");
        Assert.That(f.Canonical().Like(need.Canonical()));
    }
    [Test]
    public void Reciprocals()
    {
        Fraction r0 = new Fraction(Val(1), Var("x"));
        Oper r1a = new Fraction(Var("x"), Var("x").Pow(Val(2)));
        Oper r1b = new Fraction(Var("x"), Var("x").Mult(Var("x")));
        Oper r2a = Val(1).Divide(Var("x").Pow(Val(2)));
        Oper r2b = Val(1).Divide(Var("x").Mult(Var("x")));
        Scribe.Info($"{r0}, {r1a}, {r1b}, {r2a}, {r2b}");
        r0.Combine();
        r1a.Combine(); r1b.Combine();
        r2a.Combine(); r2b.Combine();
        Scribe.Info($"{r0}, {r1a}, {r1b}, {r2a}, {r2b}");

        Assert.That(r0.Like(Val(1).Divide(Var("x"))));
        Assert.That(r1a.Like(r1b));
        Assert.That(r1a.Like(new Fraction(Var("x").Pow(Val(-1)))));
        Assert.That(r2a.Like(new Fraction(Val(1), Var("x").Pow(Val(2)))));
        Assert.That(r2b.Like(new Fraction(new List<Oper> { Val(1) }, new List<Oper> { Var("x"), Var("x") })));

    }
    [Test]
    public void XTripleYdouble()
    {
        SumDiff need = new(
            new List<Oper>{new Fraction(new List<Oper>{Var("x"), Val(3)}, new List<Oper>{}),
            new Fraction(new List<Oper>{Var("y"), Val(2)}, new List<Oper>{})},
            new List<Oper> { }
        );
        need.Commute();

        SumDiff f = new(
            new List<Oper> { Var("x"), Var("x"), Var("x"), Var("y"), Var("y") },
            new List<Oper> { }
        );
        f.CombineAll();
        f.Commute();
        need.CombineAll();

        Scribe.Info($"got: {f}, need {need}");
        Assert.That(f.Like(need));
    }

    [Test]
    public void Degrees()
    {
        Oper o0 = new ExpLog(new List<Oper> { Var("x"), Val(1) }, new List<Oper> { });
        Oper o1 = Var("x");
        Oper o2 = new ExpLog(new List<Oper> { Var("x") }, new List<Oper> { });
        Oper o3 = new ExpLog(new List<Oper> { Var("x") }, new List<Oper> { });
        Oper o4 = new ExpLog(new List<Oper> { Var("x") }, new List<Oper> { });

        Assert.That(o0.Degree().Sol().Like(Val(1)));
        Assert.That(o0.Degree().Sol().Like(o0.Degree(Var("x"))));
        Assert.That(o0.Degree().Like(o1.Degree()));
        Assert.That(o1.Degree().Sol().Like(o1.Degree(Var("x"))));
    }
    [Test]
    public void Factors()
    {
        Oper a0 = Var("x").Mult(Var("x"));
        Oper a1 = Var("x").Pow(Val(2));
        Scribe.Info($"{a0.Factors().ToFraction()}, {a1.Factors().ToFraction()}");
        Assert.That(a0.Factors().ToFraction().Like(a1.Factors().ToFraction()));
        Oper b0 = Var("x").Mult(Var("x")).Mult(Var("x"));
        Oper b1 = new Fraction(new List<Oper> { Var("x"), Var("x"), Var("x") }, new List<Oper> { });
        Oper b2 = Var("x").Pow(Val(3));
        Scribe.Info($"{b0.Factors().ToFraction()}, {b1.Factors().ToFraction()}, {b2.Factors().ToFraction()}");
        Assert.That(b0.Factors().ToFraction().Like(b1.Factors().ToFraction()));
        Assert.That(b0.Factors().ToFraction().Like(b2.Factors().ToFraction()));
        Assert.That(b0.Factors().ToFraction().Like(new Fraction(Var("x").Pow(Val(3)))));
        Oper c0 = Val(1).Divide(Var("x").Mult(Var("x")).Mult(Var("x")));
        Oper c1 = new Fraction(new List<Oper> { }, new List<Oper> { Var("x"), Var("x"), Var("x") });
        Oper c2 = Val(1).Divide(Var("x").Pow(Val(3)));
        Scribe.Info($"{c0.Factors().ToFraction()}, {c1.Factors().ToFraction()}, {c2.Factors().ToFraction()}");
        Assert.That(c0.Factors().ToFraction().Like(c1.Factors().ToFraction()));
        Assert.That(c0.Factors().ToFraction().Like(c2.Factors().ToFraction()));
        Assert.That(c0.Factors().ToFraction().Like(new Fraction(Val(1), Var("x").Pow(Val(3)))));
    }

    [Test]
    public void CommonFactors()
    {
        Oper i0, i1, cf;

        // Trivial 1 intersection
        i0 = Var("x");
        i1 = Val(242141);
        Scribe.Info($"  {i0} CF {i1}: {i0.CommonFactors(i1)}");
        Assert.That(i0.CommonFactors(i1).Trim().Like(Val(1)));
        // single x intersection
        i0 = Var("x");
        i1 = Var("x");
        Scribe.Info($"  {i0} CF {i1}: {i0.CommonFactors(i1)}");
        Assert.That(i0.CommonFactors(i1).Trim().Like(Var("x")));
        // double x intersection
        i0 = new Fraction(new List<Oper> { Val(2), Var("x") }, new List<Oper> { });
        i1 = Var("x");
        cf = i0.CommonFactors(i1);
        cf.Reduce();
        Scribe.Info($"  {i0} CF {i1}: {cf}");
        Assert.That(cf.Trim().Like(Var("x")));
        // sumdiff null intersection
        i0 = new SumDiff(new List<Oper>{
            new Fraction(new List<Oper>{Val(3), Var("y")}, new List<Oper>{}),
            new Fraction(new List<Oper>{Var("x"), Var("y")}, new List<Oper>{})
            }, new List<Oper> { });
        i1 = Var("y");
        cf = i0.CommonFactors(i1);
        cf.Reduce();
        Scribe.Info($"  {i0} CF {i1}: {cf}");
        Assert.That(cf.Trim().Like(Val(1)));
        // quadratic-x intersection
        i0 = Var("x");
        i1 = new ExpLog(new List<Oper> { Var("x"), Val(2) }, new List<Oper> { });
        cf = i0.CommonFactors(i1);
        cf.Reduce();
        Scribe.Info($"  {i0} CF {i1}: {cf}");
        Assert.That(cf.Trim().Like(Var("x")));
        // quad-quad intersection
        i0 = new ExpLog(new List<Oper> { Var("x"), Val(2) }, new List<Oper> { });
        i1 = new ExpLog(new List<Oper> { Var("x"), Val(2) }, new List<Oper> { });
        cf = i0.CommonFactors(i1);
        cf.Reduce();
        Scribe.Info($"  {i0} CF {i1}: {cf}");
        Assert.That(cf.Trim().Like(Var("x").Pow(Val(2))));
        // quadratic-1/x intersection
        i0 = new Fraction(Val(1), Var("x"));
        i1 = new ExpLog(new List<Oper> { Var("x"), Val(2) }, new List<Oper> { });
        cf = i0.CommonFactors(i1);
        cf.Reduce();
        Scribe.Info($"  {i0} CF {i1}: {cf} [{i0.Factors().ToFraction()}], [{i0.Factors().Common(i1.Factors()).ToFraction()}, {i1.Factors().Common(i0.Factors()).ToFraction()}]");
        Assert.That(cf.Trim().Like(Val(1).Divide(Var("x"))));
        // alternate quadratic-1/x intersection
        i0 = new ExpLog(new List<Oper> { Var("x"), Val(-1) }, new List<Oper> { });
        i1 = new ExpLog(new List<Oper> { Var("x"), Val(2) }, new List<Oper> { });
        cf = i0.CommonFactors(i1);
        cf.Reduce();
        Scribe.Info($"  {i0} CF {i1}: {cf} [{i0.Factors().ToFraction()}], [{i0.Factors().Common(i1.Factors()).ToFraction()}, {i1.Factors().Common(i0.Factors()).ToFraction()}]");
        Assert.That(cf.Trim().Like(Val(1).Divide(Var("x"))));
        // quad-invquad intersection
        i0 = Val(1).Divide(Var("x").Pow(Val(2)));
        i1 = Var("x").Pow(Val(2));
        cf = i0.CommonFactors(i1);
        cf.Reduce();
        Scribe.Info($"  {i0} CF {i1}: {cf} [{i0.Factors().ToFraction()}], [{i0.Factors().Common(i1.Factors()).ToFraction()}, {i1.Factors().Common(i0.Factors()).ToFraction()}]");
        //Assert.That(LegacyForm.Shed(cf).Like(Val(1).Divide(Var("x"))));
        // alternate quad-invquad intersection
        i0 = Var("x").Pow(Val(-2));
        i1 = Var("x").Pow(Val(2));
        cf = i0.CommonFactors(i1);
        cf.Reduce();
        Scribe.Info($"  {i0} CF {i1}: {cf} [{i0.Factors().ToFraction()}], [{i0.Factors().Common(i1.Factors()).ToFraction()}, {i1.Factors().Common(i0.Factors()).ToFraction()}]");
        Assert.That(cf.Like(Val(1).Divide(Var("x").Pow(Val(2)))));
        // alternate quad-invquad intersection
        i0 = Val(1).Divide(Var("x").Mult(Var("x")));
        i1 = Var("x").Pow(Val(2));
        cf = i0.CommonFactors(i1);
        cf.Reduce();
        Scribe.Info($"  {i0} CF {i1}: {cf} [{i0.Factors().ToFraction()}], [{i1.Factors().ToFraction()}], [{i0.Factors().Common(i1.Factors()).ToFraction()}, {i1.Factors().Common(i0.Factors()).ToFraction()}]");
        Assert.That(cf.Like(Val(1).Divide(Var("x").Pow(Val(2)))));
        //general intersection
        i0 = new ExpLog(new List<Oper> { Var("x"), Var("n") }, new List<Oper> { });
        i1 = new ExpLog(new List<Oper> { Var("x"), Var("k") }, new List<Oper> { });
        cf = i0.CommonFactors(i1);
        cf.Reduce();
        Scribe.Info($"  {i0} CF {i1}: {cf}");
        Assert.That(cf.Trim().Like(Var("x").Pow(new Min(Var("k"), Var("n")))));
    }

    [Test]
    public void StableReduce()
    {
        SumDiff sd = new(
            new List<Oper>{
                Var("x"),
                Var("z"),
                new Fraction(Var("z"), Val(1), Var("y")),
                Var("x"),
                new Fraction(Val(3.3), Val(1), Var("z")),
                new Fraction(Var("x"), Val(1), Var("y")),
                new Fraction(Var("x"), Val(1), Var("z")),
                new Fraction(Val(0.4), Val(1), Var("x"), Val(1), Var("y"), Val(1), Var("z"))
            },
            new List<Oper> { }
        );
        // Reduce a bunch times initially
        for (int i = 0; i < 10; i++)
            sd.ReduceOuter();

        for (int i = 0; i < 10; i++)
        {
            Oper c = sd.Copy();
            sd.ReduceOuter();
            Assert.That(sd.Like(c));
        }

    }

    [Test]
    public void StableSimplify()
    {
        SumDiff sd = new(
            new List<Oper>{
                Var("x"),
                Var("z"),
                new Fraction(Var("z"), Val(1), Var("y")),
                Var("x"),
                new Fraction(Val(3.3), Val(1), Var("z")),
                new Fraction(Var("x"), Val(1), Var("y")),
                new Fraction(Var("x"), Val(1), Var("z")),
                new Fraction(Val(0.4), Val(1), Var("x"), Val(1), Var("y"), Val(1), Var("z"))
            },
            new List<Oper> { }
        );
        // Reduce a bunch times initially
        sd.CombineAll();

        for (int i = 0; i < 10; i++)
        {
            Oper c = sd.Copy();
            sd.CombineAll();
            Scribe.Info($"sd {sd} vs. c {c}");
            Assert.Multiple(() =>
            {
                Assert.That(sd.Like(c));
                Assert.That(sd.Ord(), Is.EqualTo(c.Ord()));
            });
        }
    }
    [Test]
    public void SdStableSimplify()
    {
        Oper o0 = new SumDiff(Val(-1));
        o0.Combine();
        Scribe.Info($"{o0.Name} {o0}");
        Assert.That(o0.Like(new SumDiff(Val(-1))));

        Oper o1 = new SumDiff(new SumDiff(Val(0)), Val(1));
        Scribe.Info(o1);
        o1.Combine();
        Scribe.Info(o1);
        Assert.That(o1.Like(new SumDiff(Val(-1))));
    }
    [Test]
    public void PatchedStableCanonical()
    {
        Oper o0 = new Fraction(new List<Oper> { new SumDiff(new List<Oper> { Val(0), Val(0) }, new List<Oper> { Val(4) }), Val(3) }, new List<Oper> { });
        Scribe.Info(o0);
        Assert.That(o0.Sol().Value.Get(), Is.EqualTo(-12));
        Oper o0Canon = o0.Canonical();
        Scribe.Info(o0Canon);
        Assert.That(o0Canon.Like(Val(-12)));

        Oper o2 = new Fraction(new List<Oper> { new SumDiff(new List<Oper> { Val(0), Var("y") }, new List<Oper> { Val(3) }), Val(8) }, new List<Oper> { });
        Scribe.Info($"{o2.Name} {o2}");
        Oper o2canon = o2.Canonical();
        Scribe.Info($"{o2canon.Name} {o2canon}");
        double x1 = o2.Evaluate(4.5).Get();
        double x2 = o2canon.Evaluate(4.5).Get();
        Scribe.Info($"{x1} vs. {x2}");

        Var("y").Set(4.5);
        Scribe.Info($"{o2}: {o2.Sol()} vs. {o2canon}: {o2canon.Sol()}");

        Assert.That(x2, Is.EqualTo(x1));
    }
    [Test]
    public void PatchedSimplify()
    {
        Oper o0 = new Fraction(new List<Oper> { new SumDiff(Val(1), Val(3)), Val(4) }, new List<Oper> { });
        Scribe.Info($"{o0} = {o0.Sol()}");
        o0.Combine();
        Scribe.Info(o0);
        Assert.That(o0.Like(new Fraction(Val(-8))));

        Oper o1 = new SumDiff(new Fraction(Val(0)), Val(1));
        Scribe.Info($"{o1} = {o1.Sol()}");
        o1.Combine();
        Scribe.Info(o1);
        Assert.That(o1.Like(new SumDiff(Val(-1))));
    }

    [Test]
    public void XSquared()
    {
        Fraction xsq = new(new List<Oper> { Var("x"), Var("x") }, new List<Oper> { });
        Scribe.Info($"have: {xsq}");
        xsq.Combine();
        Scribe.Info($"Got {xsq}, need {new Fraction(new ExpLog(new List<Oper> { Var("x"), Val(2) }, new List<Oper> { }))}");
        Assert.That(xsq.Like(new Fraction(new ExpLog(new List<Oper> { Var("x"), Val(2) }, new List<Oper> { }))));
    }
    [Test]
    public void XXSquared()
    {
        Fraction xxsq = new(new List<Oper> { Var("x"), Var("x").Pow(Val(2)) }, new List<Oper> { });
        Scribe.Info($"have: {xxsq}, {xxsq.Factors().ToFraction().Canonical()}");
        //xxsq.Commute();
        xxsq.Combine();

        Fraction xxsqWorking = new(new List<Oper> { Var("x"), Var("x").Pow(Val(2)) }, new List<Oper> { });
        xxsqWorking.Commute();
        xxsqWorking.Combine();

        Scribe.Info($"Got {xxsq}, need {new Fraction(new ExpLog(new List<Oper> { Var("x"), Val(3) }, new List<Oper> { }))}");
        Assert.That(xxsq.Like(new Fraction(new ExpLog(new List<Oper> { Var("x"), Val(3) }, new List<Oper> { }))));
    }
    [Test]
    public void XCubed()
    {
        Fraction xcb = new(new List<Oper> { Var("x"), Var("x"), Var("x") }, new List<Oper> { });
        Scribe.Info($"have: {xcb}");
        xcb.CombineAll();
        Scribe.Info($"Got {xcb}, need {new Fraction(new ExpLog(new List<Oper> { Var("x"), Val(3) }, new List<Oper> { }))}");
        Assert.That(xcb.Like(new Fraction(new ExpLog(new List<Oper> { Var("x"), Val(3) }, new List<Oper> { }))));
    }
    [Test]
    public void XCubeYsquare()
    {
        Var("y").Reset();
        Fraction need = new(
            new List<Oper>{new ExpLog(new List<Oper>{Var("x"), Val(3)}, new List<Oper>{}),
            new ExpLog(new List<Oper>{Var("y"), Val(2)}, new List<Oper>{})},
            new List<Oper> { }
        );
        need.CombineAll();

        Fraction f = new(
            new List<Oper> { Var("x"), Var("x"), Var("x"), Var("y"), Var("y") },
            new List<Oper> { }
        );
        f.CombineAll();
        f.Commute();

        Scribe.Info($"got {f} need {need}");
        Assert.That(f.Like(need));
    }
}

public class IntermediateAlgebraCases
{
    [Test]
    public void MultiplyTwoAndTen()
    {
        Oper twoTimesTen = new Fraction(Val(2), Val(1), Val(10));
        Variable result = twoTimesTen.Sol();
        Assert.That(result.Value.Get(), Is.EqualTo(20));
    }

    [Test]
    public void AssociateSumDiffs()
    {
        SumDiff sd = new(
            new List<Oper> { new SumDiff(Val(2)), new SumDiff(Val(1), Val(2), new SumDiff(Val(3)), Val(4)), Val(1), new SumDiff(Val(3)) },
            new List<Oper> { Val(5), new SumDiff(Val(1), new SumDiff(Val(5), new SumDiff(Val(3)))) }
        );
        Assert.That(sd.Sol().Value.Get(), Is.EqualTo(0));
        sd.Associate();
        Assert.That(sd.Sol().Value.Get(), Is.EqualTo(0));
    }

    [Test]
    public void CombineConstants()
    {
        SumDiff sd = new(Val(10), Val(6));
        sd.Combine();
        Scribe.Info(sd);
        Assert.That(sd.Like(new SumDiff(Val(4))));

        ExpLog pt = new(new List<Oper> { Val(2), Val(3) }, new List<Oper> { });
        pt.ReduceOuter();
        Scribe.Info(pt);
        Assert.That(pt.Like(new ExpLog(Val(8))));
    }

    [Test]
    public void SolveAddLike()
    {
        Equation unsolved = new(
            new SumDiff(
                new Fraction(Var("x"), Val(1), Var("y")),
                new Fraction(Var("x"), Val(1), Var("y"), Val(1), Var("x")),
                new Fraction(Var("y"), Val(1), Var("z")),
                new Fraction(Var("y"), Val(1), Var("z"), Val(1), Var("z"))
            ),
            Fulcrum.EQUALS,
            Val(4096)
        );
        IRelation solved = unsolved.Solved();
        SolvedEquation manuallySolved = new(
            Var("y"),
            Fulcrum.EQUALS,
            new Fraction(
                Val(4096),
                new SumDiff(
                    Var("x"),
                    new Fraction(Var("x"), Val(1), Var("x")),
                    Var("z"),
                    new Fraction(Var("z"), Val(1), Var("z"))
                )
            )
        );
        double one, two;
        one = solved.Evaluate(10.5, -30).Get();
        two = manuallySolved.Evaluate(10.5, -30).Get();
        Scribe.Info($"1,2: {one},{two}");
        Assert.That(one, Is.EqualTo(two));
    }

    [Test]
    public void SolveAddLike2()
    {
        Equation unsolved = new(
            new SumDiff(
                new List<Oper>{
                    Var("x"),
                    Var("z"),
                    new Fraction(Var("z"), Val(1), Var("y")),
                    Var("x"),
                    new Fraction(Val(3.3), Val(1), Var("z")),
                    new Fraction(Var("x"), Val(1), Var("y")),
                    new Fraction(Var("x"), Val(1), Var("z")),
                    new Fraction(Val(0.4), Val(1), Var("x"), Val(1), Var("y"), Val(1), Var("z"))
                },
                new List<Oper> { }
            ),
            Fulcrum.EQUALS,
            new Fraction(Val(2), Val(1), Var("y"), Val(1), Var("x"), Val(1), Var("z"))
        );
        IRelation s = unsolved.Solved(Var("x"));
        SolvedEquation manual = new(
            Var("x"),
            Fulcrum.EQUALS,
            new Fraction(
                new Fraction(new List<Oper>
                {
                    new SumDiff(new List<Oper>{Val(3.3), Val(1), Var("y")}, new List<Oper>{}),
                    Var("z")
                }, new List<Oper> { }),
                new SumDiff(
                    new List<Oper>
                    {
                        new SumDiff(new List<Oper>{Var("y"), Var("z"), Val(2)}, new List<Oper>{}),
                        new Fraction(new List<Oper>{new SumDiff(Val(0.4), Val(2)), Var("y"), Var("z")}, new List<Oper>{}),
                    },
                    new List<Oper> { }
                ), Val(-1)
            )
        );
        //s.Opposite.AssociatedVars.Sort((v0, v1) => v0.Name[0] < v1.Name[0] ? -1 : v0.Name[0] > v1.Name[0] ? 1 : 0);
        //manual.Opposite.AssociatedVars.Sort((v0, v1) => v0.Name[0] < v1.Name[0] ? -1 : v0.Name[0] > v1.Name[0] ? 1 : 0);
        // Chosen arbitrarily
        double[] args = new[] { 2d, 1 };
        Assert.That(s.Evaluate(args).Get(), Is.EqualTo(manual.Evaluate(args).Get()));
    }

    [Test]
    public void SolveSaddle()
    {
        Equation plotTest3d = new(
            new SumDiff(new Fraction(
                Var("y"),
                Val(0.2 / 6)
            ),
            Val(0), Var("y"), Val(0), new Fraction(Var("y"), Val(1), Val(0.2))),
            Fulcrum.EQUALS,
            new Fraction(
                new SumDiff(
                    new Fraction(Var("x"), Val(230), Var("x")),
                    new Fraction(Var("z"), Val(230), Var("z"))
                )
            )
        );
        plotTest3d.Solved();
    }
    [Test]
    public void SolveSaddle2()
    {
        Equation plotTest3d = new(
            new Fraction(
                new SumDiff(Var("y"), Val(1)),
                Var("time")),
            Fulcrum.EQUALS,
            new Fraction(
                new SumDiff(
                    new Fraction(Var("x"), Val(230), Var("x")),
                    new Fraction(Var("z"), Val(230), Var("z"))
                )
            )
        );
        plotTest3d.Solved();
    }

    [Test]
    public void Expand1()
    {
        SumDiff sd = new(
            new List<Oper>{
                Var("x"),
                Var("z"),
                new Fraction(Var("z"), Val(1), Var("y")),
                Var("x"),
                new Fraction(Val(3.3), Val(1), Var("z")),
                new Fraction(Var("x"), Val(1), Var("y")),
                new Fraction(Var("x"), Val(1), Var("z")),
                new Fraction(Val(0.4), Val(1), Var("x"), Val(1), Var("y"), Val(1), Var("z"))
            },
            new List<Oper> { }
        );

        Var("x").Set(20.13535);
        Var("y").Set(0.13585);
        Var("z").Set(-0.27164);
        double sol0 = sd.Sol().Value.Get();
        Var("x").Reset();
        Var("y").Reset();
        Var("z").Reset();

        Var("x").Set(20.13535);
        Var("y").Set(0.13585);
        Var("z").Set(-0.27164);
        double sol1 = sd.Sol().Value.Get();
        Var("x").Reset();
        Var("y").Reset();
        Var("z").Reset();
        Scribe.Info($"{sol0} == {sol1}?");
        Assert.That(sol0, Is.EqualTo(sol1));
    }

    [Test]
    public void SolveSlider()
    {
        Equation e = new
        (
            new Fraction(Var("y"), Var("time")),
            Fulcrum.EQUALS,
            new SumDiff(Var("x"), Val(10))
        );
        IRelation s = e.Solved(Var("y"));
        double res = s.Evaluate(10.5, 2).Get();
        Assert.That(res, Is.EqualTo(-84));
    }

    [Test]
    public void OperAsIFunction()
    {
        Oper decr = new SumDiff(Var("x"), Val(1));
        Assert.That(decr.Evaluate(0).Get(), Is.EqualTo(-1));

        Oper offsetSquares = new SumDiff
        (
            new Fraction(Var("x"), Val(1), Var("x")),
            Var("y")
        );
        List<IVal> ossNums = Enumerable.Range(0, 5).Select(n => offsetSquares.Evaluate(n + 1, n)).ToList();
        Assert.Multiple(() =>
        {
            Assert.That(ossNums[0].Get(), Is.EqualTo(1));
            Assert.That(ossNums[1].Get(), Is.EqualTo(3));
            Assert.That(ossNums[2].Get(), Is.EqualTo(7));
            Assert.That(ossNums[3].Get(), Is.EqualTo(13));
            Assert.That(ossNums[4].Get(), Is.EqualTo(21));
        });

        Oper triangleNumbers = new Fraction(
            new List<Oper> { Var("x"), new SumDiff(Var("x"), Val(0), Val(1)) },
            new List<Oper> { Val(2) }
        );
        List<IVal> triNums = Enumerable.Range(0, 5).Select(n => triangleNumbers.Evaluate(n)).ToList();
        Assert.Multiple(() =>
        {
            Assert.That(triNums[0].Get(), Is.EqualTo(0));
            Assert.That(triNums[1].Get(), Is.EqualTo(1));
            Assert.That(triNums[2].Get(), Is.EqualTo(3));
            Assert.That(triNums[3].Get(), Is.EqualTo(6));
            Assert.That(triNums[4].Get(), Is.EqualTo(10));
        });
    }

    [Test]
    public void SolveFoil()
    {
        //throw Scribe.Issue("implement this test");
    }

    [Test]
    public void SolveParasingle()
    {
        Equation eq = new(
            Var("x"),
            Fulcrum.EQUALS,
            new SumDiff(Var("y"), new Fraction(Var("x"), Val(3)))
        );
        IRelation s = eq.Solved(Var("y"));
        SolvedEquation manual = new(Var("y"), Fulcrum.EQUALS, new SumDiff(new Fraction(Var("x"), Val(3)), Val(0), Var("x")));
        Assert.That(s.Evaluate(4.31).Get(), Is.EqualTo(manual.Evaluate(4.31).Get()));
    }
    [Test]
    public void SolveParamultipleFracSimp()
    {
        Equation eq = new(
            Var("x"),
            Fulcrum.EQUALS,
            new SumDiff(Var("y"), new Fraction(Var("x"), Val(3)), Var("x"), new Fraction(new SumDiff(Var("x"), Val(1)), Val(4)), Var("y"))
        );
        IRelation s = eq.Solved(Var("y"));
        SolvedEquation manual = new(Var("y"), Fulcrum.EQUALS, new Fraction(new SumDiff(Var("x"), new Fraction(Var("x"), Val(1.5)), new Fraction(new SumDiff(Var("x"), Val(1)), Val(4))), Val(2)));
        Scribe.Info($"Need {manual}");
        Scribe.Info($"Got {s}");
        Assert.That(s.Evaluate(3.1).Get(), Is.EqualTo(manual.Evaluate(3.1).Get()));
    }
    [Test]
    public void SolveParamultiple()
    {
        Equation eq = new(
            Var("x"),
            Fulcrum.EQUALS,
            new SumDiff(Var("y"), new Fraction(Var("x"), Val(3)), new SumDiff(new SumDiff(Var("x"), Var("y")), Val(4)))
        );
        IRelation se = eq.Solved();
        Assert.That(se.Evaluate().Get(), Is.EqualTo(-12));
        Assert.That(se.Evaluate().Get(), Is.EqualTo(-12));
        Assert.That(se.Evaluate().Get(), Is.EqualTo(-12));
    }
    [Test]
    public void SolveImbalanced()
    {
        Equation eq = new(
            new Fraction(Var("x"), Val(3)),
            Fulcrum.EQUALS,
            new SumDiff(Var("x"), Var("y"), new Fraction(Var("x"), Val(8)))
        );
        IRelation se = eq.Solved(Var("x"));
        Assert.That(se.Evaluate(99).Get(), Is.EqualTo(125.05263157894736d));
    }
    [Test]
    public void SolveImbalanced2()
    {
        Equation eq = new(
            new SumDiff(Var("x"), Val(3)),
            Fulcrum.EQUALS,
            new SumDiff(Var("x"), Var("y"), new Fraction(Var("x"), Val(8)))
        );
        IRelation se = eq.Solved();
        Assert.That(se.Evaluate(1).Get(), Is.EqualTo(-16));
        Assert.That(se.Evaluate(2).Get(), Is.EqualTo(-8));
    }
    [Test]
    public void SolveDual()
    {
        Equation eq = new(
            new SumDiff(Var("x"), Val(3), Var("y")),
            Fulcrum.EQUALS,
            new SumDiff(Val(1), new Fraction(Var("x"), Val(3)))
        );
        IRelation se = eq.Solved();
        Assert.That(se.Evaluate(2).Get(), Is.EqualTo(1.5));
        Assert.That(se.Evaluate(4).Get(), Is.EqualTo(0));
    }
    [Test]
    public void SolveFluid()
    {
        Equation eq = new(
            new SumDiff(Var("x"), Val(3), Var("y"), new SumDiff(Var("x"), Val(1))),
            Fulcrum.EQUALS,
            new SumDiff(Val(1), new Fraction(Var("x"), Val(3)), new Fraction(Var("x"), Val(5), Var("y")))
        );
        IRelation s = eq.Solved(Var("x"));
        SolvedEquation manual = new(Var("x"), Fulcrum.EQUALS,
            new Fraction(
                Val(15),
                new SumDiff(new Fraction(Val(3), Val(1), Var("y")), Val(5)),
                new SumDiff(Var("y"), Val(3))
            ));
        Assert.That(s.Evaluate(15).Get(), Is.EqualTo(manual.Evaluate(15).Get()));
    }
}

public class AdvancedAlgebraCases
{
    [Test]
    public void SqrtX()
    {
        Oper parabola = new ExpLog(new List<Oper> { Var("x"), Val(2) }, new List<Oper> { });
        Equation eq = new(
            Var("y"),
            Fulcrum.EQUALS,
            parabola
        );
        IRelation s = eq.Solved(Var("x"));

        SolvedEquation manual = new(
            Var("x2"),
            Fulcrum.EQUALS,
            Var("y2").Root(Val(2))
        );
        // Make sure basic root functionality is working
        Assert.That(manual.Evaluate(2.8989).Get(), Is.EqualTo(Math.Sqrt(2.8989)));

        // Make sure that the algebra machine can invert exponents/powers correctly
        Scribe.Info($"got {s}=>{s.Evaluate(2).Get()}, need {manual}=>{manual.Evaluate(2).Get()}");
        Assert.That(s.Evaluate(2).Get(), Is.EqualTo(manual.Evaluate(2).Get()));
    }
    [Test]
    public void LogBase2()
    {
        Oper base2Exp = new ExpLog(new List<Oper> { Val(2), Var("x") }, new List<Oper> { });
        Equation eq = new(
            Var("y"),
            Fulcrum.EQUALS,
            base2Exp
        );
        IRelation s = eq.Solved(Var("x"));
        SolvedEquation manual = new(
            Var("x"),
            Fulcrum.EQUALS,
            Var("y").Log(Val(2))
        );
        // Make sure basic root functionality is working
        Assert.That(manual.Evaluate(2.8989).Get(), Is.EqualTo(Math.Log(2.8989, 2)));

        // Make sure that the algebra machine can invert exponents/powers correctly
        Scribe.Info($"got {s.Evaluate(2)}, need {manual.Evaluate(2)}");
        Assert.That(manual.Evaluate(2).Get(), Is.EqualTo(s.Evaluate(2).Get()));
    }

    [Test]
    public void LogBase2Then3()
    {
        ExpLog log23 = new(new List<Oper> { Var("x") }, new List<Oper> { Val(2), Val(3) });
        Scribe.Info(log23);
        Assert.That(log23.Evaluate(4096).Get(), Is.EqualTo(Math.Log(12, 3)));
    }
    [Test]
    public void Root2Then3()
    {
        ExpLog root23 = new(new List<Oper>() { Var("x"), new Fraction(Val(1), Val(2), Val(1), Val(3)) }, new List<Oper> { });
        Scribe.Info(root23);
        Assert.That(root23.Evaluate(4097).Get(), Is.EqualTo(4.0001627438622904));
    }
    [Test]
    public void Root3ThenLogBase2()
    {
        ExpLog root3log2 = new(new List<Oper> { Var("x"), Val(1d / 3) }, new List<Oper>() { Val(2) });
        Scribe.Info(root3log2);
        Assert.That(root3log2.Evaluate(8192).Get(), Is.EqualTo(4.333333333333333333333333333));
    }

    [Test]
    public void ThreePow()
    {
        ExpLog ptrl = new(new List<Oper> { Var("a"), Var("x"), Var("b") }, new List<Oper> { });
        Equation eq = new(Var("y"), Fulcrum.EQUALS, ptrl);
        IRelation sa = eq.Solved(Var("a"));
        IRelation sx = eq.Solved(Var("x"));
        IRelation sb = eq.Solved(Var("b"));
        Assert.Multiple(() =>
        {
            // Basic exponenet functionality
            Assert.That(ptrl.Evaluate(1.2, 1.4, 1.3).Get(), Is.EqualTo(Math.Pow(1.2, Math.Pow(1.3, 1.4))));

            // Inverting and solving for exponents
            Assert.That(sa.Evaluate(1.2, 1.3, 1.4).Get(), Is.EqualTo(Math.Pow(1.4, Math.Pow(Math.Pow(1.3, 1.2), -1))));
            Assert.That(sx.Evaluate(1.2, 1.3, 1.4).Get(), Is.EqualTo(Math.Pow(Math.Log(1.4, 1.2), Math.Pow(1.3, -1))));
            Assert.That(sb.Evaluate(1.2, 1.3, 1.4).Get(), Is.EqualTo(Math.Log(Math.Log(1.4, 1.2), 1.3)));
        });
    }

    [Test]
    public void NestedLogInverse()
    {
        ExpLog ptrl = new(new List<Oper> { Var("x") }, new List<Oper> { Var("b"), Var("a") });
        Equation eq = new(Var("y"), Fulcrum.EQUALS, ptrl);
        IRelation sa = eq.Solved(Var("a"));
        IRelation sx = eq.Solved(Var("x"));
        IRelation sb = eq.Solved(Var("b"));
        // Basic functionality of logarithms
        Assert.That(ptrl.Evaluate(1.2, 1.3, 1.4).Get(), Is.EqualTo(Math.Log(Math.Log(1.4, 1.3), 1.2)));

        // Inverting and solving for nested logarithms
        Assert.That(sa.Evaluate(1.2, 1.3, 1.4).Get(), Is.EqualTo(Math.Pow(Math.Log(1.3, 1.2), 1d / 1.4)));
        Assert.That(sx.Evaluate(1.2, 1.3, 1.4).Get(), Is.EqualTo(Math.Pow(1.3, Math.Pow(1.2, 1.4))));
        Assert.That(sb.Evaluate(1.2, 1.3, 1.4).Get(), Is.EqualTo(Math.Pow(1.3, 1d / Math.Pow(1.2, 1.4))));
    }


    /* This is my favourite test */
    [Test]
    public void PTRLBig()
    {
        ExpLog ptrl = new(
            new List<Oper> { Var("a"), Var("b"), Var("c"), Var("d"), Var("e") },
            new List<Oper> { Var("A"), Var("B"), Var("C"), Var("D"), Var("E") }
        );
        Equation eq = new(Var("y"), Fulcrum.EQUALS, ptrl);

        double a, b, c, d, e, A, B, C, D, E;
        a = 1.05; b = 2; c = 2.15; d = 1.2; e = 2.25;
        A = 1.3; B = 1.35; C = 1.4; D = 1.45; E = 1.5;

        Assert.That(
            ptrl.Evaluate(a, A, b, B, c, C, d, D, e, E).Get(), Is.EqualTo(
            Math.Log(Math.Log(Math.Log(Math.Log(Math.Log(Math.Pow(a, Math.Pow(b, Math.Pow(c, Math.Pow(d, e)))), A), B), C), D), E)
        ));

        IRelation sa = eq.Solved(Var("a")); IRelation sA = eq.Solved(Var("A"));
        IRelation sb = eq.Solved(Var("b")); IRelation sB = eq.Solved(Var("B"));
        IRelation sc = eq.Solved(Var("c")); IRelation sC = eq.Solved(Var("C"));
        IRelation sd = eq.Solved(Var("d")); IRelation sD = eq.Solved(Var("D"));
        IRelation se = eq.Solved(Var("e")); IRelation sE = eq.Solved(Var("E"));
        double y = 2.2;

        Assert.That(sE.Evaluate(a, A, b, B, c, C, d, D, e, y).Get(), Is.EqualTo(Math.Pow(Math.Log(Math.Log(Math.Log(Math.Log(Math.Pow(a, Math.Pow(b, Math.Pow(c, Math.Pow(d, e)))), A), B), C), D), 1d / y)));
        Assert.That(sD.Evaluate(a, A, b, B, c, C, d, e, E, y).Get(), Is.EqualTo(Math.Pow(Math.Log(Math.Log(Math.Log(Math.Pow(a, Math.Pow(b, Math.Pow(c, Math.Pow(d, e)))), A), B), C), 1d / Math.Pow(E, y))));
        Assert.That(sC.Evaluate(a, A, b, B, c, d, D, e, E, y).Get(), Is.EqualTo(Math.Pow(Math.Log(Math.Log(Math.Pow(a, Math.Pow(b, Math.Pow(c, Math.Pow(d, e)))), A), B), 1d / Math.Pow(D, Math.Pow(E, y)))));
        Assert.That(sB.Evaluate(a, A, b, c, C, d, D, e, E, y).Get(), Is.EqualTo(Math.Pow(Math.Log(Math.Pow(a, Math.Pow(b, Math.Pow(c, Math.Pow(d, e)))), A), 1d / Math.Pow(C, Math.Pow(D, Math.Pow(E, y))))));
        Assert.That(sA.Evaluate(a, b, B, c, C, d, D, e, E, y).Get(), Is.EqualTo(Math.Pow(Math.Pow(a, Math.Pow(b, Math.Pow(c, Math.Pow(d, e)))), 1d / Math.Pow(B, Math.Pow(C, Math.Pow(D, Math.Pow(E, y)))))));
        Assert.That(se.Evaluate(a, A, b, B, c, C, d, D, E, y).Get(), Is.EqualTo(Math.Log(Math.Log(Math.Log(Math.Log(Math.Pow(A, Math.Pow(B, Math.Pow(C, Math.Pow(D, Math.Pow(E, y))))), a), b), c), d)));
        Assert.That(sd.Evaluate(a, A, b, B, c, C, D, e, E, y).Get(), Is.EqualTo(Math.Pow(Math.Log(Math.Log(Math.Log(Math.Pow(A, Math.Pow(B, Math.Pow(C, Math.Pow(D, Math.Pow(E, y))))), a), b), c), 1d / e)));
        Assert.That(sc.Evaluate(a, A, b, B, C, d, D, e, E, y).Get(), Is.EqualTo(Math.Pow(Math.Log(Math.Log(Math.Pow(A, Math.Pow(B, Math.Pow(C, Math.Pow(D, Math.Pow(E, y))))), a), b), 1d / Math.Pow(d, e))));
        Assert.That(sb.Evaluate(a, A, B, c, C, d, D, e, E, y).Get(), Is.EqualTo(Math.Pow(Math.Log(Math.Pow(A, Math.Pow(B, Math.Pow(C, Math.Pow(D, Math.Pow(E, y))))), a), 1d / Math.Pow(c, Math.Pow(d, e)))));
        Assert.That(sa.Evaluate(A, b, B, c, C, d, D, e, E, y).Get(), Is.EqualTo(Math.Pow(Math.Pow(A, Math.Pow(B, Math.Pow(C, Math.Pow(D, Math.Pow(E, y))))), 1d / Math.Pow(b, Math.Pow(c, Math.Pow(d, e))))));
    }

    // We may want to solve for some Oper, rather than just a Variable
    [Test]
    public void SolveForOpers()
    {
        Oper lhs = Var("x").Mult(Val(3)).Pow(Val(2)).Plus(Var("y").Pow(Val(2)));
        Equation eq = new(
            lhs, Fulcrum.EQUALS, Val(1)
        );
        Equation rearr = eq.Rearranged(lhs.posArgs[0].posArgs[0]);
        Assert.That(rearr.LHS.Like(Var("x").Mult(Val(3))));
    }

    [Test]
    public void PolynomialZeroes()
    {
        Oper binom0 = Var("x").Plus(Val(1));
        Oper binom1 = Var("y").Minus(Val(1));
        Oper fz = binom0.Mult(binom1);
        Equation factorZeroes = new(
            fz, Fulcrum.EQUALS,
            Val(0)
        );
        IRelation s = factorZeroes.Solved(Var("x"));
        Scribe.Info(s);
        //Scribe.Info(s.Evaluate());
        //Scribe.Info(s.Evaluate(1));
        //Scribe.Info(s.Evaluate(0));
    }
}

public class ComplexAndMultivalued
{
    [Test]
    public void SquareRootMinus1()
    {
        Variable v;

        // Principal root (i)
        v = Val(-1).Pow(Val(0.5)).Sol();
        Scribe.Info(v);
        Assert.That(v.Like(new Variable(0, 1)));

        // Both solutions (+/-i)
        v = Val(-1).Root(Val(2)).Sol();
        Scribe.Info(v);
        Assert.That(v.Like(new Variable(0, 1)));
        Assert.That(((Multivalue)v).All[1].EqValue(new Val(0, -1)));

        // Both solutions (+/-i)
        v = Val(-1).Root(new Rational(2)).Sol();
        Scribe.Info(v);
        Assert.That(v.Like(new Variable(0, 1)));
        Assert.That(((Multivalue)v).All[1].EqValue(new Val(0, -1)));

        // Both solutions (+/-i)
        v = Val(-1).Pow(new Rational(1, 2)).Sol();
        Scribe.Info(v);
        Assert.That(v.Like(new Variable(0, 1)));
        Assert.That(((Multivalue)v).All[1].EqValue(new Val(0, -1)));

    }
    [Test]
    public void SquareRootsOf2()
    {
        Variable v;

        // Principal root
        v = Val(2).Pow(Val(0.5)).Sol();
        Scribe.Info(v);
        Assert.That(v.Value.Get(), Is.EqualTo(Math.Sqrt(2)));

        // Both solutions
        v = Val(2).Root(Val(2)).Sol();
        //Scribe.Info(v);
        Assert.That(v.Value.Get(), Is.EqualTo(Math.Sqrt(2)));
        Assert.That(((Multivalue)v).All[1].Get(), Is.EqualTo(-Math.Sqrt(2)));

        // Both solutions
        v = Val(2).Root(new Rational(2)).Sol();
        //Scribe.Info(v);
        Assert.That(v.Value.Get(), Is.EqualTo(Math.Sqrt(2)));
        Assert.That(((Multivalue)v).All[1].Get(), Is.EqualTo(-Math.Sqrt(2)));

        // Both solutions
        v = Val(2).Pow(new Rational(1, 2)).Sol();
        Scribe.Info(v);
        Assert.That(v.Value.Get(), Is.EqualTo(Math.Sqrt(2)));
        Assert.That(((Multivalue)v).All[1].Get(), Is.EqualTo(-Math.Sqrt(2)));
    }
    [Test]
    public void ComplexExponents()
    {
        Variable v;

        // 2 ^ (3-8i)
        v = new Variable(2).Pow(new Variable(3, -8)).Sol();
        //Scribe.Info($"2 ^ (3-8i) = {v}");
        Assert.That(v.Value.EqValue(new Val(5.9184829327522435, 5.382523550781771)));

        // (1 + 2i) ^ (3 + 4i)
        v = new Variable(1, 2).Pow(new Variable(3, 4)).Sol();
        Scribe.Info($"(1 + 2i) ^ (3 + 4i) = {v}");
        Variable rounded = new(double.Round(v.Value.Get(0), 5), double.Round(v.Value.Get(1), 5));
        Assert.That(rounded.Value.EqValue(new Val(0.12901, 0.03392)));

        // (3 - 4i)^2
        v = new Variable(3, -4).Pow(Val(5)).Sol();
        Scribe.Info($"(3 - 4i)^5 = {v}");
        Assert.That(v.Value.EqValue(new Val(-237, 3116)));

        v = new Variable(0, 1);
        Assert.That(v.Value.EqValue(new Val(0, 1)));
        v = new Variable(0, 1).Mult(new Variable(0, 1)).Sol();
        Assert.That(v.Value.EqValue(new Val(-1)));
        v = new Variable(0, 1).Mult(new Variable(0, 1)).Mult(new Variable(0, 1)).Sol();
        Assert.That(v.Value.EqValue(new Val(0, -1)));
        v = new Variable(0, 1).Mult(new Variable(0, 1)).Mult(new Variable(0, 1)).Mult(new Variable(0, 1)).Sol();
        Assert.That(v.Value.EqValue(new Val(1)));
    }

    [Test]
    public void NthRootsOfUnity()
    {
        for (int n = 1; n < 100; n++)
        {
            Variable rootsOfUnity = Val(1).Root(Val(n)).Sol();
            switch (n)
            {
                case 1:
                    Assert.That(rootsOfUnity.Value.Get(), Is.EqualTo(1));
                    Assert.That(rootsOfUnity is not Multivalue);
                    break;
                default:
                    Assert.That(rootsOfUnity is Multivalue);
                    Multivalue m = (Multivalue)rootsOfUnity;
                    List<Val> roots = Enumerable.Range(0, n).Select(k =>
                        new Val(Alg.Numeric.Trig.Cos(2 * Math.PI * k / n), Alg.Numeric.Trig.Sin(2 * Math.PI * k / n))
                    ).ToList();
                    Assert.That(roots, Has.Count.EqualTo(m.All.Length));
                    // compare the kth nth roots of unity to our multivalue
                    for (int k = 0; k < roots.Count; k++)
                    {
                        Assert.That(m.All[k].EqValue(roots[k]));
                    }
                    break;
            }
        }
    }
}

public class OrdAndData
{
    [Test]
    public void IValLikeness()
    {
        Variable v0 = new(1);
        Variable v1 = new(1, 0);
        Variable v2 = new(0, 1);
        Scribe.Info($"{v0}, {v1}, {v2}");
        Scribe.Info($"{v0.Ord()}, {v1.Ord()}, {v2.Ord()}");
        Assert.That(v0.Like(v1));
        Assert.That(v0.Ord(), Is.EqualTo(v1.Ord()));
        Assert.That(v0.Ord(), Is.Not.EqualTo(v2.Ord()));
        Assert.That(!v1.Like(v2));
    }
    [Test]
    public void PlusMinusBinomial()
    {
        Oper binom0 = Var("x").Plus(Val(1));
        Oper binom1 = Var("x").Minus(Val(1));
        Assert.That(!binom0.Like(binom1));
    }
}

public class Calculus
{
    [Test]
    public void DPolynomials()
    {
        Oper o0 = new Derivative(Val(100), Var("x"));
        Oper o1 = new Derivative(Var("x"), Var("x"));
        Oper o2 = new Derivative(Var("x").Pow(Val(2)), Var("x"));
        Oper o3 = new Derivative(Var("x").Pow(Val(3)).Plus(Var("x").Mult(Val(4))).Minus(Val(100)), Var("x"));

        Scribe.Info($"{o0}");
        Scribe.Info($"{o0.Canonical()}");
        Assert.That(o0.Canonical().Like(Val(0)));
        Scribe.Info($"{o1}");
        Scribe.Info($"{o1.Canonical()}");
        Assert.That(o1.Canonical().Like(Val(1)));
        Scribe.Info($"{o2}");
        Scribe.Info($"{o2.Canonical()}");
        Assert.That(o2.Canonical().Like(Val(2).Mult(Var("x"))));
        Scribe.Info($"{o3}");
        Scribe.Info($"{o3.Canonical()}");
        Assert.That(o3.Canonical().Like(Val(3).Mult(Var("x").Pow(Val(2))).Plus(Val(4)).Canonical()));
    }
    [Test]
    public void DBasicTranscendentals()
    {
        Oper o0 = new Derivative(Val(Math.E).Pow(Var("x")), Var("x"));
        Oper o1 = new Derivative(Val(2).Pow(Var("x")), Var("x"));
        Assert.That(o0.Canonical().Like(Val(Math.E).Pow(Var("x"))));
        Assert.That(o1.Canonical().Like(Val(2).Pow(Var("x")).Mult(Val(2).Log(Val(Math.E)).Sol())));
    }
    [Test]
    public void DProductRule()
    {
        Oper f = Var("x").Pow(Val(2)).Minus(Val(1)).Mult(Var("x"));
        Oper g = Val(Math.E).Pow(Var("x").Mult(Val(0.5)));
        Oper dfg = new Derivative(f.Mult(g), Var("x")).Canonical();
        Oper manual = Var("x").Pow(Val(2)).Minus(Val(1)).Plus(Val(2).Mult(Var("x").Pow(Val(2)))).Mult(Val(Math.E).Pow(Var("x").Mult(Val(0.5)))).Plus(Var("x").Mult(Var("x").Pow(Val(2)).Minus(Val(1))).Mult(Val(Math.E).Pow(Var("x").Mult(Val(0.5)))).Mult(Val(0.5)));
        Assert.That(dfg.Like(manual.Canonical()));
    }
    [Test]
    public void DProductRule2()
    {
        Oper f = Var("x");
        Oper g = Var("x").Pow(Val(2)).Minus(Val(1));
        Oper dfg = new Derivative(f.Mult(g), Var("x")).Canonical();
        Oper manual = Val(3).Mult(Var("x").Pow(Val(2))).Minus(Val(1));
        Assert.That(dfg.Like(manual.Canonical()));
    }
    [Test]
    public void Approximate()
    {
        Equation tanglecube = new(
            Var("x").Pow(Val(4)).Plus(Var("y").Pow(Val(4))).Plus(Var("z").Pow(Val(4))).Divide(Val(2)).Plus(Val(60)),
            Fulcrum.EQUALS,
            Var("x").Pow(Val(2)).Plus(Var("y").Pow(Val(2))).Plus(Var("z").Pow(Val(2))).Mult(Val(8))
        );
        // As long as we don't crash, good enough
        IRelation tcs = tanglecube.Solved();
        Scribe.Info((Approx)tcs);
        Scribe.Info(tcs.Evaluate(1, 2));
    }
    [Test]
    public void ImplicitDifferentiation()
    {
        // TODO: write assertions
        //Oper d0 = new Derivative(Var("x"), Var("t"));
        //Oper d1 = new Derivative(Var("x"), Var("t"), DerivativeKind.IMPLICIT);
        //Scribe.Info($"{d0}, {d1}");
        //Scribe.Info($"{d0.Canonical()}, {d1.Canonical()}");

        Oper diffMe = new ExpLog(Var("x"), Val(2));
        Oper der0 = new Derivative(diffMe, Var("y"), DerivativeKind.IMPLICIT);
        Oper der1 = new Derivative(diffMe, Var("y"));
        Scribe.Info($"{der0.Canonical()}, {der1.Canonical()}");
    }
}

public class Parser
{
    [Test]
    public void ParseSimple()
    {
        Oper o0 = Parse("3+5");
        Oper o1 = Parse("1+2+3");
        Oper o2 = Parse("3+5*2");
        Oper o3 = Parse("2*5+3");
        Oper o4 = Parse("x+1");
        Oper o5 = Parse("1+2+3+4*5+6*7+8*9");

    }

    [Test]
    public void ParseMalformed()
    {
        Oper o0 = Parse("  3 + 5    + 7   ");
        Oper o1 = Parse("+3 5");
        Oper o2 = Parse("+1+2+3");
    }
}
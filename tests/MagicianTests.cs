namespace Magician.Tests;
using Magician.Symbols;
using Magician.Symbols.Funcs;
using static Magician.Symbols.Notate;
using NUnit.Framework;

public class BasicCases2
{
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
        for (int i = 0; i < 10; i++)
            sd.SimplifyOnce();

        for (int i = 0; i < 10; i++)
        {
            Oper c = sd.Copy();
            c.Commute();
            sd.SimplifyOnce();
            sd.Commute();
            Assert.Multiple(() =>
            {
                Assert.That(sd.Like(c));
                Assert.That(sd.Ord(), Is.EqualTo(c.Ord()));
            });
        }
    }

    [Test]
    public void Subsumption()
    {
        Oper o0 = new Abs(new Max(new Fraction(new Abs(new SumDiff(new Abs(Val(4096)))))));
        Scribe.Info(o0);
        o0.Reduce();
        Scribe.Info(o0);
        Assert.That(o0.Like(new Abs(Val(4096))));
    }

    [Test]
    public void AddingOrSubtractingAnOperFromItself()
    {
        Oper o = new Fraction(
            Var("x"),
            new SumDiff(
                Val(1), new PowTowRootLog(new List<Oper> { Var("x"), Val(2) }, new List<Oper> { })
            )
        );

        SumDiff doubl = new(new List<Oper> { o.Copy(), o.Copy() }, new List<Oper> { });
        SumDiff nothing = new(o.Copy(), o.Copy());
        doubl.SimplifyOnce(Var("x"));
        doubl.Reduce();
        nothing.ReduceOuter();

        Oper doublManual = new Fraction(new List<Oper> { Val(2), o }, new List<Oper> { });
        doublManual.Commute(); doubl.Commute(); doublManual.Associate();
        Scribe.Info($"  Got {doubl}, need {doublManual}");
        Assert.That(LegacyForm.Shed(doubl).Like(doublManual));
        Assert.That(LegacyForm.Shed(nothing).Like(Val(0)));
    }

    [Test]
    public void XCubeYsquare()
    {
        Fraction need = new(
            new List<Oper>{new PowTowRootLog(new List<Oper>{Var("x"), Val(3)}, new List<Oper>{}),
            new PowTowRootLog(new List<Oper>{Var("y"), Val(2)}, new List<Oper>{})},
            new List<Oper> { }
        );
        need.Commute();

        Fraction f = new(
            new List<Oper> { Var("x"), Var("x"), Var("x"), Var("y"), Var("y") },
            new List<Oper> { }
        );
        f.Simplify();
        f.Commute();

        Scribe.Info($"got {f} need {need}");
        Assert.That(f.Like(need));
    }
    [Test]
    public void BasicFracSimp()
    {
        Fraction f = new Fraction(new List<Oper> { Val(3), Var("x"), Var("x") }, new List<Oper> { });
        Fraction need = new Fraction(new List<Oper> { Val(3), new PowTowRootLog(new List<Oper> { Var("x"), Val(2) }, new List<Oper> { }) }, new List<Oper> { });

        Scribe.Info($"{LegacyForm.Canonical(f)} vs. {LegacyForm.Canonical(need)}");
        Assert.That(LegacyForm.Canonical(f).Like(LegacyForm.Canonical(need)));
    }
    [Test]
    public void NextFracSimp()
    {
        Fraction f = new(new List<Oper> { Val(3), Var("x"), new PowTowRootLog(new List<Oper> { Var("x"), Val(2) }, new List<Oper> { }) }, new List<Oper> { });
        Fraction need = new(new List<Oper> { Val(3), new PowTowRootLog(new List<Oper> { Var("x"), Val(3) }, new List<Oper> { }) }, new List<Oper> { });

        Scribe.Info($"{f} will become {LegacyForm.Canonical(f)}, which should be equal to {need}");
        Assert.That(LegacyForm.Canonical(f).Like(LegacyForm.Canonical(need)));
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
        f.Simplify();
        f.Commute();

        Scribe.Info($"got: {f}, need {need}");
        Assert.That(f.Like(need));
    }

    [Test]
    public void CommonFactors()
    {
        Oper i0, i1;

        // Trivial 1 intersection
        i0 = Var("x");
        i1 = Val(242141);
        Scribe.Info($"  {i0} CF {i1}: {i0.CommonFactors(i1)}");
        Assert.That(LegacyForm.Shed(i0.CommonFactors(i1)).Like(Val(1)));

        // single x intersection
        i0 = Var("x");
        i1 = Var("x");
        Scribe.Info($"  {i0} CF {i1}: {i0.CommonFactors(i1)}");
        Assert.That(LegacyForm.Shed(i0.CommonFactors(i1)).Like(Var("x")));

        // double x intersection
        i0 = new Fraction(new List<Oper> { Val(2), Var("x") }, new List<Oper> { });
        i1 = Var("x");
        Oper cf = i0.CommonFactors(i1);
        cf.Reduce();
        Scribe.Info($"  {i0} CF {i1}: {cf}");
        Assert.That(LegacyForm.Shed(cf).Like(Var("x")));

        // sumdiff null intersection
        i0 = new SumDiff(new List<Oper>{
            new Fraction(new List<Oper>{Val(3), Var("y")}, new List<Oper>{}),
            new Fraction(new List<Oper>{Var("x"), Var("y")}, new List<Oper>{})
            }, new List<Oper> { });
        i1 = Var("y");
        cf = i0.CommonFactors(i1);
        cf.Reduce();
        Scribe.Info($"  {i0} CF {i1}: {cf}");
        Assert.That(LegacyForm.Shed(cf).Like(Val(1)));

        //Assert.That(Oper.Intersect(i0, i1).Like(Var("x")));
    }

    [Test]
    public void XSquared()
    {
        Fraction xsq = new(new List<Oper> { Var("x"), Var("x") }, new List<Oper> { });
        Scribe.Info($"have: {xsq}");
        xsq.SimplifyOnce();
        Scribe.Info($"Got {xsq}, need {new Fraction(new PowTowRootLog(new List<Oper> { Var("x"), Val(2) }, new List<Oper> { }))}");
        Assert.That(xsq.Like(new Fraction(new PowTowRootLog(new List<Oper> { Var("x"), Val(2) }, new List<Oper> { }))));
    }
    [Test]
    public void XXSquared()
    {
        Fraction xxsq = new(new List<Oper> { Var("x"), Var("x").Pow(Val(2)) }, new List<Oper> { });
        Scribe.Info($"have: {xxsq}");
        xxsq.SimplifyOnce();
        Scribe.Info($"Got {xxsq}, need {new Fraction(new PowTowRootLog(new List<Oper> { Var("x"), Val(3) }, new List<Oper> { }))}");
        Assert.That(xxsq.Like(new Fraction(new PowTowRootLog(new List<Oper> { Var("x"), Val(3) }, new List<Oper> { }))));
    }
    [Test]
    public void XCubed()
    {
        Fraction xcb = new(new List<Oper> { Var("x"), Var("x"), Var("x") }, new List<Oper> { });
        Scribe.Info($"have: {xcb}");
        xcb.Simplify();
        Scribe.Info($"Got {xcb}, need {new Fraction(new PowTowRootLog(new List<Oper> { Var("x"), Val(3) }, new List<Oper> { }))}");
        Assert.That(xcb.Like(new Fraction(new PowTowRootLog(new List<Oper> { Var("x"), Val(3) }, new List<Oper> { }))));
    }
}

public class BasicAlgebraCases
{
    [Test]
    public void MultiplyTwoAndTen()
    {
        Oper twoTimesTen = new Fraction(Val(2), Val(1), Val(10));
        Variable result = twoTimesTen.Solution();
        Assert.That(result.Val, Is.EqualTo(20));
    }

    [Test]
    public void AssociateSumDiffs()
    {
        SumDiff sd = new(
            new List<Oper> { new SumDiff(Val(2)), new SumDiff(Val(1), Val(2), new SumDiff(Val(3)), Val(4)), Val(1), new SumDiff(Val(3)) },
            new List<Oper> { Val(5), new SumDiff(Val(1), new SumDiff(Val(5), new SumDiff(Val(3)))) }
        );
        Assert.That(sd.Solution().Val, Is.EqualTo(0));
        sd.Associate();
        Assert.That(sd.Solution().Val, Is.EqualTo(0));
    }

    [Test]
    public void CombineConstants()
    {
        SumDiff sd = new(Val(10), Val(6));
        sd.ReduceOuter();
        Assert.That(sd.Like(new SumDiff(Val(4))));

        PowTowRootLog pt = new(new List<Oper> { Val(2), Val(3) }, new List<Oper> { });
        pt.ReduceOuter();
        Assert.That(pt.Like(new PowTowRootLog(Val(8))));
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
            Val(1)
        );
        SolvedEquation solved = unsolved.Solved();
        SolvedEquation manuallySolved = new(
            Var("y"),
            Fulcrum.EQUALS,
            new Fraction(
                Val(1),
                new SumDiff(
                    Var("x"),
                    new Fraction(Var("x"), Val(1), Var("x")),
                    Var("z"),
                    new Fraction(Var("z"), Val(1), Var("z"))
                )
            ),
            Var("y"),
            2
        );
        double one, two;
        one = solved.Evaluate(10.5, -30);
        two = manuallySolved.Evaluate(10.5, -30);
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
        SolvedEquation s = unsolved.Solved(Var("x"));
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
            ), Var("x"), 2
        );
        s.opposite.AssociatedVars.Sort((v0, v1) => v0.Name[0] < v1.Name[0] ? -1 : v0.Name[0] > v1.Name[0] ? 1 : 0);
        manual.opposite.AssociatedVars.Sort((v0, v1) => v0.Name[0] < v1.Name[0] ? -1 : v0.Name[0] > v1.Name[0] ? 1 : 0);
        // Chosen arbitrarily
        double[] args = new[] { 2d, 1 };
        Assert.That(s.Evaluate(args), Is.EqualTo(manual.Evaluate(args)));
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

        Var("x").Val = 20.13535;
        Var("y").Val = 0.13585;
        Var("z").Val = -0.27164;
        double sol0 = sd.Solution().Val;
        Var("x").Reset();
        Var("y").Reset();
        Var("z").Reset();

        Var("x").Val = 20.13535;
        Var("y").Val = 0.13585;
        Var("z").Val = -0.27164;
        double sol1 = sd.Solution().Val;
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
        SolvedEquation s = e.Solved(Var("y"));
        double res = s.Evaluate(10.5, 2);
        Assert.That(res, Is.EqualTo(-84));
    }

    [Test]
    public void OperAsIFunction()
    {
        Oper decr = new SumDiff(Var("x"), Val(1));
        Assert.That(decr.Evaluate(0), Is.EqualTo(-1));

        Oper offsetSquares = new SumDiff
        (
            new Fraction(Var("x"), Val(1), Var("x")),
            Var("y")
        );
        List<double> ossNums = Enumerable.Range(0, 5).Select(n => offsetSquares.Evaluate(n + 1, n)).ToList();
        Assert.Multiple(() =>
        {
            Assert.That(ossNums[0], Is.EqualTo(1));
            Assert.That(ossNums[1], Is.EqualTo(3));
            Assert.That(ossNums[2], Is.EqualTo(7));
            Assert.That(ossNums[3], Is.EqualTo(13));
            Assert.That(ossNums[4], Is.EqualTo(21));
        });

        Oper triangleNumbers = new Fraction(
            new List<Oper> { Var("x"), new SumDiff(Var("x"), Val(0), Val(1)) },
            new List<Oper> { Val(2) }
        );
        List<double> triNums = Enumerable.Range(0, 5).Select(n => triangleNumbers.Evaluate(n)).ToList();
        Assert.Multiple(() =>
        {
            Assert.That(triNums[0], Is.EqualTo(0));
            Assert.That(triNums[1], Is.EqualTo(1));
            Assert.That(triNums[2], Is.EqualTo(3));
            Assert.That(triNums[3], Is.EqualTo(6));
            Assert.That(triNums[4], Is.EqualTo(10));
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
        SolvedEquation s = eq.Solved();
        SolvedEquation manual = new(Var("y"), Fulcrum.EQUALS, new SumDiff(new Fraction(Var("x"), Val(3)), Val(0), Var("x")), Var("y"), 1);
        Assert.That(s.Evaluate(4.31), Is.EqualTo(manual.Evaluate(4.31)));
    }
    [Test]
    public void SolveParamultipleFracSimp()
    {
        Equation eq = new(
            Var("x"),
            Fulcrum.EQUALS,
            new SumDiff(Var("y"), new Fraction(Var("x"), Val(3)), Var("x"), new Fraction(new SumDiff(Var("x"), Val(1)), Val(4)), Var("y"))
        );
        SolvedEquation s = eq.Solved();
        SolvedEquation manual = new(Var("y"), Fulcrum.EQUALS, new Fraction(new SumDiff(Var("x"), new Fraction(Var("x"), Val(1.5)), new Fraction(new SumDiff(Var("x"), Val(1)), Val(4))), Val(2)), Var("y"), 1);
        Assert.That(s.Evaluate(3.1), Is.EqualTo(manual.Evaluate(3.1)));
    }
    [Test]
    public void SolveParamultiple()
    {
        Equation eq = new(
            Var("x"),
            Fulcrum.EQUALS,
            new SumDiff(Var("y"), new Fraction(Var("x"), Val(3)), new SumDiff(new SumDiff(Var("x"), Var("y")), Val(4)))
        );
        Assert.That(eq.Solved(Var("x")).Evaluate(), Is.EqualTo(-12));
        Assert.That(eq.Solved(Var("x")).Evaluate(), Is.EqualTo(-12));
        Assert.That(eq.Solved(Var("x")).Evaluate(), Is.EqualTo(-12));
    }
    [Test]
    public void SolveImbalanced()
    {
        Equation eq = new(
            new Fraction(Var("x"), Val(3)),
            Fulcrum.EQUALS,
            new SumDiff(Var("x"), Var("y"), new Fraction(Var("x"), Val(8)))
        );
        eq.Solved(Var("x"));
    }
    [Test]
    public void SolveImbalanced2()
    {
        Equation eq = new(
            new SumDiff(Var("x"), Val(3)),
            Fulcrum.EQUALS,
            new SumDiff(Var("x"), Var("y"), new Fraction(Var("x"), Val(8)))
        );
        Assert.That(eq.Solved(Var("x")).Evaluate(1), Is.EqualTo(-16));
        Assert.That(eq.Solved(Var("x")).Evaluate(2), Is.EqualTo(-8));
    }
    [Test]
    public void SolveDual()
    {
        Equation eq = new(
            new SumDiff(Var("x"), Val(3), Var("y")),
            Fulcrum.EQUALS,
            new SumDiff(Val(1), new Fraction(Var("x"), Val(3)))
        );
        eq.Solved();
        Assert.That(eq.Solved(Var("x")).Evaluate(2), Is.EqualTo(1.5));
        Assert.That(eq.Solved(Var("x")).Evaluate(4), Is.EqualTo(0));
    }
    [Test]
    public void SolveFluid()
    {
        Equation eq = new(
            new SumDiff(Var("x"), Val(3), Var("y"), new SumDiff(Var("x"), Val(1))),
            Fulcrum.EQUALS,
            new SumDiff(Val(1), new Fraction(Var("x"), Val(3)), new Fraction(Var("x"), Val(5), Var("y")))
        );
        SolvedEquation s = eq.Solved(Var("x"));
        SolvedEquation manual = new(Var("x"), Fulcrum.EQUALS,
            new Fraction(
                Val(15),
                new SumDiff(new Fraction(Val(3), Val(1), Var("y")), Val(5)),
                new SumDiff(Var("y"), Val(3))
            ),
        Var("x"), 1);
        Assert.That(s.Evaluate(15), Is.EqualTo(manual.Evaluate(15)));
    }
}
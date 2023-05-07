using NUnit.Framework;
using Magician.Algo;
using static Magician.Algo.Algebra;
using static Magician.Algo.Equation.Fulcrum;

namespace Magician.Tests;

public class Tests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void AddOneAndOne()
    {
        Oper onePlusOne = new Plus(N(1), N(1));
        Variable result = (Variable)onePlusOne.Eval();
        Assert.That(result.Val, Is.EqualTo(2));
    }


    [Test]
    public void MultiplyTwoAndTen()
    {
        Oper twoTimesTen = new Mult(N(2), N(10));
        Variable result = twoTimesTen.Eval();
        Assert.That(result.Val, Is.EqualTo(20));
    }
}
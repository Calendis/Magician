
namespace Magician.Tests;

/*
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

    [Test]
    public void SolveEquationWithSum()
    {
        Equation equation = new Equation(new Plus(Let("x"), N(2)), Equation.Fulcrum.EQUALS, N(10));
        Assert.That(equation.ToString(), Is.EqualTo(@"Plus(Variable(x`ariable(2)) = Variable(10)"));

        Equation solved = equation.Solve(Let("x"));
        Assert.That(solved.ToString(), Is.EqualTo(@"Variable(x) = Minus(Variable(10), Variable(2))"));
    }
}
*/
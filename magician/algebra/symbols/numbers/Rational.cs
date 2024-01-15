namespace Magician.Algebra.Symbols;

public class Rational : Variable
{
    int num;
    int denom;
    public int Numerator => num;
    public int Denominator => denom;
    public Rational(string n, int i, int j) : base(n, (double)i/j)
    {
        num = i;
        denom = j;
    }
    public Rational(int i, int j=1) : this("rational", i, j) {}
    public void Set(int i, int j)
    {
        num = i;
        denom = j;
        Set((double)i/j);
    }
    // TODO: arithmetic methods for rationals. This will allow for arbitrary-precision calculations
}
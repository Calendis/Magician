namespace Magician.Alg;
using Magician.Alg.Symbols;

internal struct OperBuilder
{
    Oper? current;
    Notate.Token.TokenKind k;
    public Oper Current => current != null ? current : throw Scribe.Error($"Cannot get null Oper from {this}");
    public Notate.Token.TokenKind Kind { get { _ = Current; return k; } }
    public bool IsEmpty => current == null;
    public OperBuilder(Oper o, Notate.Token.TokenKind tk)
    {
        current = o;
        k = tk;
    }

    public void AddArg(Oper o)
    {
        if (o.GetType() == Current.GetType())
        {
            switch (k)
            {
                case Notate.Token.TokenKind.MINUS:
                case Notate.Token.TokenKind.DIVIDEDBY:
                    Current.negArgs.AddRange(o.negArgs);
                    break;
                default:
                    Current.posArgs.AddRange(o.posArgs);
                    break;
            }
        }
        else
        {
            switch (k)
            {
                case Notate.Token.TokenKind.MINUS:
                case Notate.Token.TokenKind.DIVIDEDBY:
                    Current.negArgs.Add(o);
                    break;
                default:
                    Current.posArgs.Add(o);
                    break;
            }
        }
    }

    public Oper PopArg()
    {
        switch (k)
        {
            case Notate.Token.TokenKind.MINUS:
            case Notate.Token.TokenKind.DIVIDEDBY:
                Oper n = Current.negArgs[Current.negArgs.Count-1];
                Current.negArgs.RemoveAt(Current.negArgs.Count-1);
                return n;
            default:
                Oper p = Current.posArgs[Current.posArgs.Count-1];
                Current.posArgs.RemoveAt(Current.posArgs.Count-1);
                return p;
        }
    }


    public override string ToString()
    {
        return $"({current},{k})";
    }

    public static Oper FromToken(Notate.Token.TokenKind tk, string data)
    {
        switch (tk)
        {
            case Notate.Token.TokenKind.SYMBOL:
                return new Variable(data);
            case Notate.Token.TokenKind.NUMBER:
                double num;
                bool isInt = int.TryParse(data, out int n);
                bool isDouble = double.TryParse(data, out num);
                if (isInt)
                    num = n;
                return new Variable(num);
            case Notate.Token.TokenKind.PLUS:
                return new SumDiff();
            case Notate.Token.TokenKind.MINUS:
                return new SumDiff();
            case Notate.Token.TokenKind.TIMES:
                return new Fraction();
            case Notate.Token.TokenKind.DIVIDEDBY:
                return new Fraction();
            case Notate.Token.TokenKind.EXPONENT:
                return new ExpLog();
            case Notate.Token.TokenKind.LEFTPAREN:
                throw Scribe.Issue($"Unresolved leftparen");
            case Notate.Token.TokenKind.RIGHTPAREN:
                throw Scribe.Issue($"Unresolved rightparen");
            default:
                throw Scribe.Issue($"Unresolved token in parser");
        }
    }
}
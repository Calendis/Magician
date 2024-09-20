namespace Magician.Alg;
using Symbols;

using static MathCache;

public static class Notate
{
    // Denote and access variables with a string
    public static Variable Var(string name)
    {
        if (freeVars.ContainsKey(name))
        {
            return freeVars[name];
        }
        Variable v = new Variable(name);
        freeVars.Add(name, v);
        return freeVars[name];
    }

    // Denote a number
    public static Variable Val(double v)
    {
        return new Variable(v);
    }

    public static Oper Parse(string s)
    {
        List<Token> tokens = Tokenize(s);
        Oper o = ParseExpression(tokens.ToArray());
        return o;
    }

    internal class Token
    {
        internal enum TokenKind
        {
            PLUS, MINUS,
            TIMES, DIVIDEDBY,
            EXPONENT,
            LEFTPAREN, RIGHTPAREN,
            SYMBOL, NUMBER
        }
        internal enum RunningTokenState
        {
            FALSE, NUMBER, SYMBOL
        }
        internal TokenKind kind;
        internal string name;

        internal Token(TokenKind tk, string s)
        {
            kind = tk;
            name = s;
        }
        public override string ToString()
        {
            return $"{kind}\"{name}\"";
        }
    }

    internal static bool IsNumeric(char c)
    {
        return c >= 48 && c <= 57;
    }
    internal static bool IsAlphabet(char c)
    {
        return (c >= 65 && c <= 90) || (c >= 97 && c <= 122);
    }

    internal static readonly Dictionary<char, Token.TokenKind> charToKind = new()
    {
        { '+', Token.TokenKind.PLUS },
        { '-', Token.TokenKind.MINUS },
        { '*', Token.TokenKind.TIMES },
        { '/', Token.TokenKind.DIVIDEDBY },
        { '^', Token.TokenKind.EXPONENT },
        { '(', Token.TokenKind.LEFTPAREN },
        { ')', Token.TokenKind.RIGHTPAREN },
    };
    internal static readonly Dictionary<Token.TokenKind, int> kindToPrecedence = new()
    {
        {Token.TokenKind.NUMBER, 0 },
        {Token.TokenKind.SYMBOL, 0 },
        {Token.TokenKind.PLUS, 1 },
        {Token.TokenKind.MINUS, 1 },
        {Token.TokenKind.TIMES, 2 },
        {Token.TokenKind.DIVIDEDBY, 2 },
        {Token.TokenKind.EXPONENT, 3 },
        {Token.TokenKind.LEFTPAREN, 4 },
        {Token.TokenKind.RIGHTPAREN, 4 },
    };

    internal static List<Token> Tokenize(string input)
    {

        List<Token> tokens = new();
        List<char> runningToken = new();
        Token.RunningTokenState multiChar = 0;
        int i = 0;
        while (i < input.Length)
        {
            char currentChar = input[i];
            if (charToKind.ContainsKey(currentChar))
            {
                // Operator tokens break out of symbols and numbers
                if (multiChar > 0)
                {
                    string s = new(runningToken.ToArray());
                    if (multiChar == Token.RunningTokenState.NUMBER)
                        tokens.Add(new(Token.TokenKind.NUMBER, s));
                    else
                        tokens.Add(new(Token.TokenKind.SYMBOL, s));
                    runningToken.Clear();
                    multiChar = 0;
                }
                tokens.Add(new(charToKind[currentChar], currentChar.ToString()));
            }
            else if (IsNumeric(currentChar))
            {
                if (multiChar == 0)
                {
                    multiChar = Token.RunningTokenState.NUMBER;
                }
                runningToken.Add(currentChar);
            }
            else if (IsAlphabet(currentChar))
            {
                multiChar = Token.RunningTokenState.SYMBOL;
                runningToken.Add(currentChar);
            }
            else if (currentChar == ' ')
            {
                i++; continue;
            }
            else
            {
                throw Scribe.Error($"Unknown character '{currentChar}'");
            }
            i++;
        }
        // Collect the final token
        if (runningToken.Count > 0)
        {
            string s = new(runningToken.ToArray());
            if (multiChar == Token.RunningTokenState.NUMBER)
                tokens.Add(new(Token.TokenKind.NUMBER, s));
            else if (multiChar == Token.RunningTokenState.SYMBOL)
                tokens.Add(new(Token.TokenKind.SYMBOL, s));
            else
                throw Scribe.Issue("Tokenizer finished in invalid state");
        }
        return tokens;
    }

    internal static Oper ParseExpression(Token[] tokens)
    {
        Stack<OperBuilder> branches = new();
        bool primed = false;  // Primed to create an Oper object
        Token? arg = null;
        Token? op = null;
        for (int i = 0; i < tokens.Length; i++)
        {
            bool climb = false;
            Token t = tokens[i];
            switch (t.kind)
            {
                case Token.TokenKind.SYMBOL:
                case Token.TokenKind.NUMBER:
                case Token.TokenKind.PLUS:
                case Token.TokenKind.MINUS:
                case Token.TokenKind.TIMES:
                case Token.TokenKind.DIVIDEDBY:
                case Token.TokenKind.EXPONENT:

                    if (primed)
                    {
                        if (op == null)
                            throw Scribe.Issue($"null operator in parser");

                        OperBuilder newOp = new(OperBuilder.FromToken(op.kind, op.name), op.kind);
                        //OperBuilder? lastOp = branches.Count > 0 ? branches.Peek() : null;

                        // If this is the first operator, just push it
                        if (branches.Count == 0)
                        {
                            if (arg == null)
                            {
                                throw Scribe.Issue($"Null argument in initial op");
                            }
                            branches.Push(newOp);
                            branches.Peek().AddArg(OperBuilder.FromToken(arg.kind, arg.name));
                            op = null;
                        }
                        // Precedence drop, eat the argument and become the current branch
                        else if (kindToPrecedence[branches.Peek().Kind] > kindToPrecedence[op.kind])
                        {
                            if (arg == null)
                            {
                                throw Scribe.Issue($"Null argument in drop");
                            }
                            branches.Peek().AddArg(OperBuilder.FromToken(arg.kind, arg.name));
                            List<OperBuilder> brl = branches.ToList();
                            newOp.AddArg(brl[brl.Count - 1].Current);
                            branches.Clear();
                            branches.Push(newOp);
                            op = null;
                        }
                        // Precedece climb, append and push
                        else if (kindToPrecedence[branches.Peek().Kind] < kindToPrecedence[op.kind])
                        {
                            if (arg == null)
                            {
                                throw Scribe.Issue($"Null argument in climb");
                            }
                            newOp.AddArg(OperBuilder.FromToken(arg.kind, arg.name));
                            branches.Peek().AddArg(newOp.Current);
                            branches.Push(newOp);
                            op = null;
                            climb = true;
                        }
                        else
                        {
                            if (arg == null)
                            {
                                throw Scribe.Issue($"Null argument in climb");
                            }
                            newOp.AddArg(OperBuilder.FromToken(arg.kind, arg.name));
                            branches.Peek().AddArg(newOp.Current);
                            op = null;
                        }

                        if (kindToPrecedence[t.kind] == 0)
                        {
                            //newOp.AddArg(OperBuilder.FromToken(t.kind, t.name));
                            arg = t;
                        }
                        else
                        {
                            //throw Scribe.Error($"Expected symbol or number, got {t}");
                            op = t;
                            arg = null;
                        }
                    }
                    else
                    {
                        if (kindToPrecedence[t.kind] > 0)
                        {
                            op = t;
                        }
                        else if (arg == null && !climb)
                        {
                            arg = t;
                        }
                        else
                        {
                            throw Scribe.Issue($"Unsupported operation");
                        }
                    }

                    primed = op != null && arg != null;
                    //last = t;
                    break;
                case Token.TokenKind.LEFTPAREN:
                    throw Scribe.Issue($"Unresolved leftparen in parser");
                case Token.TokenKind.RIGHTPAREN:
                    throw Scribe.Issue($"Unresolved rightparen in parser");
                default:
                    throw Scribe.Issue($"Unresolved token in parser");
            }
        }

        if (branches.Count < 1)
        {
            throw Scribe.Issue($"Parser finished in unresolved state {branches.Count}");
        }
        // Absorb the final argument
        if (arg != null)
            branches.Peek().AddArg(OperBuilder.FromToken(arg.kind, arg.name));

        List<OperBuilder> bl = branches.ToList();
        return bl[bl.Count - 1].Current;
    }
}

static class MathCache
{
    public static Dictionary<string, Variable> freeVars = new();
    public static void Clear()
    {
        freeVars.Clear();
    }
}

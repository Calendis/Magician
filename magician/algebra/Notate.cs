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
        // A sparse array of tokens for every precedence level, called the precedence "tape"
        Dictionary<int, Dictionary<int, Token>> precedenceTape = new();
        for (int i = 0; i < tokens.Length; i++)
        {
            Token current = tokens[i];
            int p = kindToPrecedence[current.kind];
            precedenceTape.TryAdd(p, new Dictionary<int, Token>());
            precedenceTape[p].Add(i, current);
        }
        // Divide the tape tracks into labeled regions
        List<(Token, (int, int))>[] precedenceToRegions = new List<(Token, (int, int))>[precedenceTape.Keys.Count];
        int[] precedenceToLast = new int[precedenceTape.Keys.Count];
        Token? rightHandRegion = null;
        for (int i = 0; i < tokens.Length; i++)
        {
            Token current = tokens[i];
            int p = kindToPrecedence[current.kind];
            // Create a region from the current token
            precedenceToRegions[p].Add((rightHandRegion == null ? current : (Token)rightHandRegion, (precedenceToLast[p], precedenceToLast[p] + i)));
            precedenceToLast[p] = precedenceToLast[p] + i;  // is this right?
            rightHandRegion = current;
        }
        // Combine the tracks
        int[] trackIndices = new int[precedenceTape.Keys.Count];
        OperBuilder branches = new OperBuilder();
        for (int i = 0; i < tokens.Length; i++)
        {
            int j = 0;
            List<(Token, (int, int))> currentRegions = trackIndices.Select(trkIdx => precedenceToRegions[j++][trkIdx]).ToList();
            //OperBuilder[] branches = new OperBuilder[precedenceTape.Keys.Count];
            //OperBuilder branches = new OperBuilder();//[precedenceTape.Keys.Count];
            Stack<(Token t, int precedence)> precedences = new();
            foreach ((Token t, (int start, int end) span) region in currentRegions)
            {
                if (i >= region.span.start && i <= region.span.end)
                {
                    trackIndices[kindToPrecedence[region.t.kind]]++;
                    precedences.Push((region.t, kindToPrecedence[region.t.kind]));
                }
            }
            // The lower precedences encompass the higher ones
            OperBuilder previousBranch = new OperBuilder();
            for (int k = 0; k < precedences.Count; k++)
            {
                (Token t, int p) = precedences.Pop();
                switch (t.kind)
                {
                    case Token.TokenKind.SYMBOL:
                    case Token.TokenKind.NUMBER:
                    case Token.TokenKind.PLUS:
                    case Token.TokenKind.MINUS:
                    case Token.TokenKind.TIMES:
                    case Token.TokenKind.DIVIDEDBY:
                    case Token.TokenKind.EXPONENT:
                        Oper newOp = OperBuilder.FromToken(t.kind, t.name);
                        if (branches.IsEmpty)
                        {
                            branches = new(newOp, t.kind);
                        }
                        else
                        {
                            // Add the symbol to the current operator
                            if (kindToPrecedence[t.kind] < kindToPrecedence[previousBranch.Kind])
                            {
                                branches.AddArg(newOp);
                            }
                            // Supercede this branch
                            else
                            {
                                OperBuilder ob = new(newOp, t.kind);
                                ob.AddArg(branches.Current);
                                branches = ob;
                            }
                        }
                        previousBranch = branches;
                        break;
                    case Token.TokenKind.LEFTPAREN:
                        throw Scribe.Issue($"Unresolved leftparen in parser");
                    case Token.TokenKind.RIGHTPAREN:
                        throw Scribe.Issue($"Unresolved rightparen in parser");
                    default:
                        throw Scribe.Issue($"Unresolved token in parser");
                }
            }
        }

        return branches.Current;
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

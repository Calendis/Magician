namespace Magician;
/* Scribe is the logger */
public static class Scribe
{
    static int counter = 0;
    static int total = 0;
    public static void Info(string? s)
    {
        Console.Write($"INFO: {s}\n");
    }
    public static void Info(object? o)
    {
        if (o == null)
        {
            Info("NULL");
            return;
        }
        Info(o.ToString());
        return;
    }
    public static void Info<T, U>(T os) where T : IEnumerable<U>
    {
        Scribe.Info(Expand<T, U>(os));
    }
    public static string Expand<T, U>(T os) where T : IEnumerable<U>
    {
        string s = "";
        foreach (U o in os)
        {
            s += o is null ? "" : o.ToString();
            s += ", ";
        }
        if (s.Length < 2)
            s = "--";
        return s[..^2];
    }

    public static void Warn(string s)
    {
        Console.Write($"WARNING: {s}\n");
    }
    public static void Warn(object? o)
    {
        if (o == null)
        {
            Warn("NULL");
            return;
        }
        Warn(o.ToString()!);
        return;
    }
    public static void WarnIf(bool condition, string s)
    {
        if (condition)
            Warn(s);
    }

    public static void Peek()
    {
        Info(total);
    }
    public static void Tick()
    {
        counter += 1;
    }
    public static void Flush()
    {
        Info(counter);
        total += counter;
        counter = 0;
    }
    public static void Dump(bool flush=true)
    {
        if (flush)
            total += counter;
        counter = 0;
        Info(total);
        total = 0;
    }

    /// <summary><exception>
    /// bruhException
    /// </exception></summary>
    public static MagicianError Error(string s)
    {
        Console.Write($"ERROR: {s}\n");
        return new MagicianError(s);
    }
    public static MagicianError Issue(string s)
    {
        Console.WriteLine($"ERROR: {s}\nPlease file an issue at https://github.com/Calendis");
        return new MagicianError(s);
    }

    public class MagicianError : Exception
    {
        public MagicianError(string s) : base(s) { }
    }
}
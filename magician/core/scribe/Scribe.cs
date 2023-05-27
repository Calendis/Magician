namespace Magician;
/* Scribe is the logger */
public static class Scribe
{
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
        Warn(o.ToString());
        return;
    }
    public static void WarnIf(bool condition, string s)
    {
        if (condition)
            Warn(s);
    }
    public static void List<T>(List<T> l)
    {
        if (l == null)
        {
            throw Scribe.Error($"Could not print null list");
        }
        int c = 0;
        foreach (T o in l)
        {
            Console.WriteLine($"LIST {c++}: {o}");
        }
    }

    public static void List<T>(params T[] l)
    {
        List(l.ToList());
    }

    /// <summary><exception>
    /// bruhException
    /// </exception></summary>
    public static Exception Error(string s)
    {
        Console.Write($"ERROR: {s}\n");
        return new Exception(s);
    }
    public static Exception Issue(string s)
    {
        Console.WriteLine($"ERROR: {s}\nPlease file an issue at https://github.com/Calendis");
        return new Exception(s);
    }
}
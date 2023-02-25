namespace Magician
{
    /* Scribe is the logger */
    public static class Scribe
    {
        public static void Info(string s)
        {
            Console.Write($"INFO: {s}\n");
        }
        
        public static void Warn(string s)
        {
            Console.Write($"WARNING: {s}\n");
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
}
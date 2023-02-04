namespace Magician.UI.Presets
{
    public static class Graph
    {
        public static UI.Grid Cartesian(double spacing = 100)
        {
            return new UI.Grid(spacing, Math.Sqrt(spacing), spacing, Math.Sqrt(spacing));
        }
    }

    public static class State
    {
        //
    }
}
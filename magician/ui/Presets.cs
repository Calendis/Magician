namespace Magician.UI.Presets
{
    public static class Graph
    {
        public static Multi Cartesian(double spacing=100)
        {
            return new UI.Grid(spacing, Math.Sqrt(spacing), spacing, Math.Sqrt(spacing)).Render();
        }
    }

    public static class State
    {
        //
    }
}
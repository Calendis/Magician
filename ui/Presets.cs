namespace Magician.UI.Presets
{
    public static class Graph
    {
        public static void Cartesian(int spacing=100)
        {
            Geo.Origin.Add(
                new UI.Grid(spacing, Math.Sqrt(spacing), spacing, Math.Sqrt(spacing)).Render()
            );
        }
    }

    public static class State
    {
        //
    }
}
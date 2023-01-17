namespace Magician.UI.Presets
{
    public static class Graph
    {
        public static void Cartesian()
        {
            Geo.Origin.Add(
                new UI.Grid(100, 10, 100, 10).Render()
            );
        }
    }

    public static class State
    {
        //
    }
}
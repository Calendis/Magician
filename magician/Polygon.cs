namespace Magician
{
    public class Polygon : Multi
    {
        public Polygon(params Point[] points) : base(points)
        {
            //
        }

        public Polygon(Color c, params Point[] points) : base(points)
        {
            foreach (Point p in points)
            {
                p.Col = c;
            }
        }
    }
}
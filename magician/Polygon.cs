using static SDL2.SDL;

namespace Magician
{
    public class Polygon : Multi
    {
        public Polygon(params Point[] points) : base(points) {}

        public Polygon(Color c, params Point[] points) : base(points)
        {
            foreach (Point p in points)
            {
                p.Col = c;
            }
        }

        public override void Draw(ref IntPtr renderer, double xOffset = 0, double yOffset = 0)
        {
            
            for (int i = 0; i < constituents.Count-1; i++)
            {
                Point p0 = (Point)constituents[i];
                Point p1 = (Point)constituents[i+1];
                SDL_SetRenderDrawColor(renderer, p0.Col.R, p0.Col.G, p0.Col.B, 255);
                SDL_RenderDrawLine(renderer,
                (int)p0.XCartesian(pos[0]+xOffset), (int)p0.YCartesian(pos[1]+yOffset),
                (int)p1.XCartesian(pos[0]+xOffset), (int)p1.YCartesian(pos[1]+yOffset));
            }
            
            Point pLast = (Point)constituents[constituents.Count-1];
            Point pFirst = (Point)constituents[0];
            SDL_SetRenderDrawColor(renderer, pLast.Col.R, pLast.Col.G, pLast.Col.B, 255);
            SDL_RenderDrawLine(renderer,
            (int)pLast.XCartesian(pos[0]+xOffset), (int)pLast.YCartesian(pos[1]+yOffset),
            (int)pFirst.XCartesian(pos[0]+xOffset), (int)pFirst.YCartesian(pos[1]+yOffset));
        }

        public void SetCol(Color c)
        {
            foreach (Point p in constituents)
            {
                p.Col = c;
            }
        }
    }

    public class NonIntersectPolygon : Polygon
    {
        public NonIntersectPolygon(params Point[] points) : base(points) {}
        public NonIntersectPolygon(Color c, params Point[] points) : base(c, points) {}
        
        public override void Draw(ref IntPtr renderer, double xOffset = 0, double yOffset = 0)
        {
            // Make a copy of constituents to avoid scrambling the order
            List<Multi> constituentsCopy = constituents;
            
            // Order by phase to avoid intersecting lines
            constituents = constituentsCopy.OrderBy(c => c.Phase).ToList<Multi>();
            
            base.Draw(ref renderer, xOffset, yOffset);
            
            // Reset the order
            constituents = constituentsCopy;
        }
    }

    public class RegularPolygon : Polygon
    {
        public RegularPolygon(int sides, double magnitude)
        {
            List<Point> ps = new List<Point>();
            double angle = 360d / (double)sides;
            for (int i = 0; i < sides; i++)
            {
                double x = magnitude*Math.Cos(angle*i/180*Math.PI);
                double y = magnitude*Math.Sin(angle*i/180*Math.PI);
                ps.Add(new Point(x, y));
            }
            constituents.AddRange(ps);
        }
    }
}
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
                SDL_RenderDrawLine(renderer, (int)p0.X, (int)p0.Y, (int)p1.X, (int)p1.Y);
            }
            
            Point pLast = (Point)constituents[constituents.Count-1];
            Point pFirst = (Point)constituents[0];
            SDL_SetRenderDrawColor(renderer, pLast.Col.R, pLast.Col.G, pLast.Col.B, 255);
            SDL_RenderDrawLine(renderer, (int)pLast.X, (int)pLast.Y, (int)pFirst.X, (int)pFirst.Y);
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
}
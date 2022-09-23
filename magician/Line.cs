/*
    A line is defined by two points, and extends infinitely in either
*/
using static SDL2.SDL;

namespace Magician
{
    public class Line : Plot
    {
        private Point p0;
        private Point p1;
        public Point P0 {
            get => P0;
        }
        public Point P1 {
            get => P1;
        }
        
        public Line(Point p0, Point p1)
        {
            this.p0 = p0;
            this.p1 = p1;
            filled = true;
        }
        public Line(Point p0, Point p1, Color c)
        {
            this.p0 = p0;
            this.p1 = p1;
            filled = true;
            col = c;
        }

        public override void Draw(ref IntPtr renderer, double xOffset=0, double yOffset=0)
        {
            SDL_SetRenderDrawColor(renderer, col.R, col.G, col.B, 255);
            if (filled)
            {
                SDL_RenderDrawLine(renderer,
                (int)p0.XCartesian(xOffset), (int)p0.YCartesian(yOffset),
                (int)p1.XCartesian(xOffset), (int)p1.YCartesian(yOffset));
                return;
            }

            p0.Draw(ref renderer, xOffset, yOffset);
            p1.Draw(ref renderer, xOffset, yOffset);
        }
    }
}
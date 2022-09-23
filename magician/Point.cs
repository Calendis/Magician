using static SDL2.SDL;

namespace Magician
{
    public class Point : Plot
    {
        public Point(double x, double y)
        {
            pos[0] = x;
            pos[1] = y;
        }
        public Point(double x, double y, Color c)
        {
            pos[0] = x;
            pos[1] = y;
            col = c;
        }

        public Point(double[] pos)
        {
            this.pos[0] = pos[0];
            this.pos[1] = pos[1];
        }

        public override void Draw(ref IntPtr renderer, double xOffset=0, double yOffset=0)
        {
            SDL_SetRenderDrawColor(renderer, col.R, col.G, col.B, 255);
            // TODO: try SDL_RenderDrawPointF. How does it differ?
            SDL_RenderDrawPoint(renderer, (int)X, (int)Y);
        }
    }
}
using static SDL2.SDL;

namespace Magician
{
    public class Point : Multi
    {
        public Point(double x, double y, bool phaseMag=false)
        {
            if (!phaseMag)
            {
                pos[0] = x;
                pos[1] = y;
            }
            else
            {
                double phase = x;
                double mag = y;
                pos[0] = Math.Cos(phase) * mag;
                pos[1] = Math.Sin(phase) * mag;
            }
            
        }
        public Point(double x, double y, Color c) : this(new double[] {x, y})
        {
            col = c;
        }

        public Point(double[] pos)
        {
            this.pos[0] = pos[0];
            this.pos[1] = pos[1];
        }

        public new Point Copy()
        {
            return new Point(pos[0], pos[1]);
        }

        public override void Draw(ref IntPtr renderer, double xOffset=0, double yOffset=0)
        {
            SDL_SetRenderDrawColor(renderer, col.R, col.G, col.B, 255);
            // TODO: try SDL_RenderDrawPointF. How does it differ?
            SDL_RenderDrawPoint(renderer, (int)XCartesian(xOffset), (int)YCartesian(yOffset));
        }

        public override string ToString()
        {
            return $"Point({XCartesian(0)}, {YCartesian(0)})";
        }
    }
}
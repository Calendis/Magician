using static SDL2.SDL;

namespace Magician
{
    public class Point : Multi
    {
        static Point origin = new Point(0, 0, parent: null);
        
        public Point(double x, double y, Drawable? parent=null)
        {
            pos[0] = x;
            pos[1] = y;
            this.parent = parent;
        }
        public Point(double x, double y, Color c) : this(new double[] {x, y})
        {
            col = c;
        }

        public Point(double[] pos) : this(pos[0], pos[1]) {}

        public double XAbsolute(double offset)
        {
            return pos[0] + offset;
        }
        public double YAbsolute(double offset)
        {
            return pos[1] + offset;
        }

        public new void Draw(ref IntPtr renderer, double xOffset=0, double yOffset=0)
        {
            SDL_SetRenderDrawColor(renderer, col.R, col.G, col.B, col.A);
            // TODO: try SDL_RenderDrawPointF. How does it differ?
            SDL_RenderDrawPoint(renderer, (int)((Drawable)this).XCartesian(xOffset), (int)((Drawable)this).YCartesian(yOffset));
        }

        public override string ToString()
        {
            return $"Point({((Drawable)this).XCartesian(0)}, {((Drawable)this).YCartesian(0)})";
        }

        public static Point Origin
        {
            get => origin;
            set
            {
                ((Drawable)origin).SetX(value.XAbsolute(0));
                ((Drawable)origin).SetY(value.YAbsolute(0));
            }
        }
    }
}
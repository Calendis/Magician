/*
*  A Point is actually a Multi, so for Point-related operations, see Multi
*/
namespace Magician
{
    public class Point : Multi
    {
        static Point origin = new Point(0, 0);
        
        public Point(double x, double y, Color c, Drawable? parent=null) : base(x, y, c, false, false, true)
        {
            if (parent is null)
            {
                this.parent = origin;
            }
            else
            {
                this.parent = parent;
            }
        }
        public Point(double x, double y, Color c) : this(x, y, c, null) {}

        public Point(double[] pos, Color c) : this(pos[0], pos[1], c) {}
        public Point(double x, double y, Drawable? parent=null) : this(x, y, Globals.fgCol, parent:parent) {}

        /*
        public new void Draw(ref IntPtr renderer, double xOffset=0, double yOffset=0)
        {
            Console.WriteLine("drawing point");
            SDL_SetRenderDrawColor(renderer, col.R, col.G, col.B, col.A);
            // TODO: try SDL_RenderDrawPointF. How does it differ?
            SDL_RenderDrawPoint(renderer, (int)((Drawable)this).XCartesian(xOffset), (int)((Drawable)this).YCartesian(yOffset));
        }
        */

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
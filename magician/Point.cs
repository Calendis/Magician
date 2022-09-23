using static SDL2.SDL;

namespace Magician
{
    public class Point : Single
    {
        public Point(double x, double y)
        {
            pos[0] = x;
            pos[1] = y;
        }

        public Point(double[] pos)
        {
            this.pos[0] = pos[0];
            this.pos[1] = pos[1];
        }

        public override void Draw(ref IntPtr renderer, double xOffset=0, double yOffset=0)
        {
            //
        }
    }
}
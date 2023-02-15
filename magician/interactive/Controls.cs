using System;

namespace Magician.Interactive
{
    public interface IControl
    {
        //
    }

    public class Button : Multi, IControl
    {
        IMap mo;
        bool hovered = false;
        Color hoverCol = HSLA.RandomVisible();
        Color tempCol;
        public Button(double x, double y, double width, double height) : base(x, y)
        {
            Become(Geo.Create.Rect(x, y, width, height));
            mo = Sensor.MouseOver(this);
            tempCol = col;
        }

        public override void Update()
        {
            hovered = mo.Evaluate() > 0;

            if (hovered)
            {
                col = hoverCol;
            }
            else
            {
                col = tempCol;
            }

            if (hovered && Sensor.Click.Evaluate()>0)
            {
                col = HSLA.RandomVisible();
            }
        }
    }

    public class Draggable : Multi, IControl
    {
        //
    }
}

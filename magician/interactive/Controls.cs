using System;

namespace Magician.Interactive
{
    public abstract class Control : Multi
    {
        public Action ControlAction {get; set;}
        protected Control(double x, double y, Action? a=null) : base(x, y)
        {
            ControlAction = a ?? new Action(() => {});
        }
    }

    public abstract class Clickable : Control
    {
        protected IMap mo;
        protected bool hovered;
        protected Clickable(double x, double y, Action? a) : base(x, y, a)
        {
            hovered = false;
            mo = Sensor.MouseOver(this);
        }
    }

    public class Button : Clickable
    {

        Color hoverCol = HSLA.RandomVisible();
        Color tempCol;
        public Button(double x, double y, double width, double height, Action? a=null) : base(x, y, a)
        {
            tempCol = col;
            Become(Geo.Create.Rect(x, y, width, height));
        }

        public override void Update()
        {
            // Hover state
            hovered = mo.Evaluate() > 0;
            if (hovered)
            {
                col = hoverCol;
            }
            else
            {
                col = tempCol;
            }

            // Button is clicked
            if (hovered && Sensor.Click.Evaluate() > 0)
            {
                ControlAction.Invoke();
            }
        }
    }

    // A menu is a multi whose constituents are all Buttons
    // It's just a convenient way to group buttons together
    public class Menu : Multi
    {
        public Menu(double x=0, double y=0, params Button[] buttons) : base(x, y, Data.Col.UIDefault.FG, DrawMode.INVISIBLE, buttons)
        {
            //
        }

        public Action ControlAction { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        // Custom draw behaviour for menus
        public new void Draw(double xOffset, double yOffset)
        {
            base.Draw(xOffset, yOffset);
        }
    }

    public abstract class Draggable : Control
    {
        protected Draggable(double x, double y, Action? a) : base(x, y, a)
        {
            //
        }
    }
}

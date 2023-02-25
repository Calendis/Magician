using System;

namespace Magician.Interactive
{
    public abstract class Control : Multi
    {
        public Action ControlAction { get; set; }
        protected Control(double x, double y, Action? a = null) : base(x, y)
        {
            ControlAction = a ?? new Action(() => { });
        }
    }

    public abstract class InteractiveControl : Control
    {
        protected IMap controlSensor;
        public InteractiveControl(double x, double y, Func<Multi, IMap> sensor, Action? a = null) : base(x, y, a)
        {
            controlSensor = sensor.Invoke(this);
        }
    }

    public abstract class Clickable : InteractiveControl
    {
        protected bool hovered = false;
        protected Clickable(double x, double y, Action? a) : base(x, y, Sensor.MouseOver, a) { }
        public override void Update()
        {
            hovered = controlSensor.Evaluate() > 0;
        }
    }

    public class Button : Clickable
    {

        Color hoverCol = HSLA.RandomVisible();
        Color tempCol;
        public Button(double x, double y, double width, double height, Action? a = null) : base(x, y, a)
        {
            tempCol = col;
            Become(Geo.Create.Rect(x, y, width, height));
        }

        public override void Update()
        {
            base.Update();
            // Hover state
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
    public class Menu1D : Multi
    {
        public Menu1D(double x = 0, double y = 0, params Button[] buttons) : base(x, y, Data.Col.UIDefault.FG, DrawMode.INVISIBLE, buttons)
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

    public abstract class Draggable : Clickable
    {
        protected Draggable(double x, double y, Action? a) : base(x, y, a)
        {
            //
        }

        public override void Update()
        {
            if (hovered)
            {
                //
            }
        }
    }
}

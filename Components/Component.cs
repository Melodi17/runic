namespace runic;

using System.Drawing;

public abstract class Component
{
    public int X, Y, Width, Height;
    public abstract void Render(Graphics graphics, Context context);
}
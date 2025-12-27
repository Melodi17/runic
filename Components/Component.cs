namespace runic.Components;

using Models;
using SixLabors.ImageSharp.Processing;

public abstract class Component
{
    public int X, Y, Width, Height;
    public bool Visible = true;
    public abstract void Render(IImageProcessingContext graphics, Context context);
}
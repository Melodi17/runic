namespace runic;

using System.Drawing;
using SixLabors.ImageSharp.Processing;

public abstract class Component
{
    public const bool DebugMode = false; // Set to true for debug mode
    public int X, Y, Width, Height;
    public bool Visible = true;
    public abstract void Render(IImageProcessingContext graphics, Context context);
}
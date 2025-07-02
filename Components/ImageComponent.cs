using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Memory;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.Processing.Processors.Transforms;
using System.Collections.Generic;
using System;
using runic;

public class ImageComponent : Component
{
    public string ImagePath;
    public string SizeMode = "fit"; // Default to fit

    private static Dictionary<string, Image<Rgba32>> imageCache = new();

    public override void Render(IImageProcessingContext graphics, Context context)
    {
        if (string.IsNullOrEmpty(this.ImagePath))
            return;

        string resolvedImage = Helpers.ResolveVariables(this.ImagePath, context);

        Image<Rgba32> img;
        if (!imageCache.TryGetValue(resolvedImage, out img))
        {
            try
            {
                img = Image.Load<Rgba32>(resolvedImage);
                imageCache[resolvedImage] = img;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading image '{resolvedImage}': {ex.Message}");
                return;
            }
        }

        // Clone the image so we can safely manipulate transparency
        using var clonedImage = img.Clone();

        // Determine destination rectangle
        Rectangle destRect = CalculateDestinationRect(clonedImage.Width, clonedImage.Height, Width, Height, SizeMode);

        // Resize the image based on size mode
        clonedImage.Mutate(x =>
        {
            if (SizeMode == "stretch")
            {
                x.Resize(Width, Height);
            }
            else if (SizeMode == "fit" || SizeMode == "zoom" || SizeMode == "crop")
            {
                x.Resize(new ResizeOptions
                {
                    Size = new Size(destRect.Width, destRect.Height),
                    Mode = SizeMode switch
                    {
                        "fit" => ResizeMode.Max,
                        "zoom" => ResizeMode.Pad,
                        "crop" => ResizeMode.Crop,
                        _ => ResizeMode.Max
                    },
                    Position = AnchorPositionMode.Center
                });
            }
        });

        // Draw the image into the rendering context
        graphics.DrawImage(clonedImage, new Point(destRect.X, destRect.Y), 1f);
    }

    private Rectangle CalculateDestinationRect(int imgW, int imgH, int targetW, int targetH, string mode)
    {
        float aspect = (float)imgW / imgH;
        if (mode == "stretch" || mode == "fit")
            return new Rectangle(X, Y, targetW, targetH);

        if (mode == "zoom")
        {
            if (targetW / (float)targetH > aspect)
            {
                int newHeight = (int)(targetW / aspect);
                return new Rectangle(X, Y + (targetH - newHeight) / 2, targetW, newHeight);
            }
            else
            {
                int newWidth = (int)(targetH * aspect);
                return new Rectangle(X + (targetW - newWidth) / 2, Y, newWidth, targetH);
            }
        }

        if (mode == "crop")
        {
            if (targetW / (float)targetH > aspect)
            {
                int newWidth = (int)(targetH * aspect);
                return new Rectangle(X + (targetW - newWidth) / 2, Y, newWidth, targetH);
            }
            else
            {
                int newHeight = (int)(targetW / aspect);
                return new Rectangle(X, Y + (targetH - newHeight) / 2, targetW, newHeight);
            }
        }

        return new Rectangle(X, Y, targetW, targetH); // Default fallback
    }
}
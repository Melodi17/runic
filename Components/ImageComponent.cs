using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace runic.Components;

using Models;

public class ImageComponent : Component
{
    public string ImagePath;
    public ImageSizeMode SizeMode = ImageSizeMode.Fit;

    private static Dictionary<string, Image<Rgba32>> imageCache = new();

    public override void Render(IImageProcessingContext graphics, Context context)
    {
        if (string.IsNullOrEmpty(this.ImagePath))
            return;

        string resolvedImage = Helpers.ResolveVariables(this.ImagePath, context);

        Image<Rgba32>? img = TryLoadImage(resolvedImage);
        if (img == null)
        {
            Console.WriteLine($"Error loading image '{resolvedImage}'");
            return;
        }

        // Clone the image so we can safely manipulate transparency
        using var clonedImage = img.Clone();

        Rectangle destRect = this.CalculateDestinationRect(
            clonedImage.Width,
            clonedImage.Height,
            this.Width,
            this.Height,
            this.SizeMode);

        ApplySizeMode(clonedImage, destRect, this.SizeMode, this.Width, this.Height);

        graphics.DrawImage(clonedImage, new Point(destRect.X, destRect.Y), 1f);
    }
    
    private static Image<Rgba32>? TryLoadImage(string path)
    {
        if (imageCache.TryGetValue(path, out Image<Rgba32>? img))
            return img;

        try
        {
            img = Image.Load<Rgba32>(path);
            imageCache[path] = img;
            return img;
        }
        catch
        {
            return null;
        }
    }
    
    private static void ApplySizeMode(Image<Rgba32> image, Rectangle destRect, ImageSizeMode sizeMode, int width, int height)
    {
        image.Mutate(x =>
        {
            switch (sizeMode)
            {
                case ImageSizeMode.Stretch:
                    x.Resize(width, height);
                    break;
                
                case ImageSizeMode.Fit:
                case ImageSizeMode.Zoom:
                case ImageSizeMode.Crop:
                    x.Resize(
                        new ResizeOptions
                        {
                            Size = new Size(destRect.Width, destRect.Height),
                            Mode = sizeMode switch
                            {
                                ImageSizeMode.Fit  => ResizeMode.Max,
                                ImageSizeMode.Zoom => ResizeMode.Pad,
                                ImageSizeMode.Crop => ResizeMode.Crop,
                                _                  => ResizeMode.Max
                            },
                            Position = AnchorPositionMode.Center
                        });
                    break;
            }
        });
    }

    private Rectangle CalculateDestinationRect(
        int imgW,
        int imgH,
        int targetW,
        int targetH,
        ImageSizeMode mode)
    {
        float aspect = (float) imgW / imgH;
        if (mode == ImageSizeMode.Stretch || mode == ImageSizeMode.Fit)
            return new Rectangle(this.X, this.Y, targetW, targetH);

        if (mode == ImageSizeMode.Zoom)
        {
            if (targetW / (float) targetH > aspect)
            {
                int newHeight = (int) (targetW / aspect);
                return new Rectangle(this.X, this.Y + (targetH - newHeight) / 2, targetW, newHeight);
            }
            else
            {
                int newWidth = (int) (targetH * aspect);
                return new Rectangle(this.X + (targetW - newWidth) / 2, this.Y, newWidth, targetH);
            }
        }

        if (mode == ImageSizeMode.Crop)
        {
            if (targetW / (float) targetH > aspect)
            {
                int newWidth = (int) (targetH * aspect);
                return new Rectangle(this.X + (targetW - newWidth) / 2, this.Y, newWidth, targetH);
            }
            else
            {
                int newHeight = (int) (targetW / aspect);
                return new Rectangle(this.X, this.Y + (targetH - newHeight) / 2, targetW, newHeight);
            }
        }

        return new Rectangle(this.X, this.Y, targetW, targetH); // Default fallback
    }
}
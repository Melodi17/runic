namespace runic.Components;

using System.Drawing;
using System.Drawing.Imaging;

public class ImageComponent : Component
{
    public string ImagePath;
    public int Transparency = 255; // Default to fully opaque
    public string SizeMode = "fit"; // Default to fit

    private static Dictionary<string, Image> imageCache = new();

    public override void Render(Graphics graphics, Context context)
    {
        if (string.IsNullOrEmpty(this.ImagePath))
            return;

        // Resolve variables in the image path
        string resolvedImage = Helpers.ResolveVariables(this.ImagePath, context);

        // Load the image
        Image img;
        if (imageCache.ContainsKey(resolvedImage))
        {
            img = imageCache[resolvedImage];
        }
        else
        {
            try
            {
                img = Image.FromFile(resolvedImage);
                imageCache[resolvedImage] = img;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading image '{resolvedImage}': {ex.Message}");
                return;
            }
        }

        // Apply transparency if needed
        if (Transparency < 255)
        {
            Bitmap bmp = new Bitmap(img.Width, img.Height);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                ColorMatrix matrix = new ColorMatrix();
                matrix.Matrix33 = Transparency / 255f; // Set alpha channel
                ImageAttributes attributes = new ImageAttributes();
                attributes.SetColorMatrix(matrix);
                g.DrawImage(img, new Rectangle(0, 0, img.Width, img.Height), 0, 0, img.Width, img.Height,
                    GraphicsUnit.Pixel, attributes);
                img.Dispose();
                img = bmp;
            }
        }

        // Determine size mode
        Rectangle destRect;
        if (SizeMode == "fit")
        {
            destRect = new Rectangle(X, Y, Width, Height);
        }
        else if (SizeMode == "stretch")
        {
            destRect = new Rectangle(X, Y, Width, Height);
        }
        else if (this.SizeMode == "zoom")
        {
            float aspectRatio = (float) img.Width / img.Height;
            if (Width / Height > aspectRatio)
            {
                int newHeight = (int) (Width / aspectRatio);
                destRect = new Rectangle(X, Y + (Height - newHeight) / 2, Width, newHeight);
            }
            else
            {
                int newWidth = (int) (Height * aspectRatio);
                destRect = new Rectangle(X + (Width - newWidth) / 2, Y, newWidth, Height);
            }
        }
        else // "crop"
        {
            float aspectRatio = (float) img.Width / img.Height;
            if (Width / Height > aspectRatio)
            {
                int newWidth = (int) (Height * aspectRatio);
                destRect = new Rectangle(X + (Width - newWidth) / 2, Y, newWidth, Height);
            }
            else
            {
                int newHeight = (int) (Width / aspectRatio);
                destRect = new Rectangle(X, Y + (Height - newHeight) / 2, Width, newHeight);
            }
        }

        // Draw the image
        graphics.DrawImage(img, destRect);
    }
}
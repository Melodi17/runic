using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Processing;
using SixLabors.Fonts;
using SixLabors.ImageSharp.PixelFormats;
using System;

namespace runic.Components
{
    public class TextComponent : Component
    {
        public string Text;
        public string Font = "Arial";
        public float Size = 12f;
        public string Variant = "regular";
        public string Color = "#000000";
        public string Alignment = "top,left"; // top, left, center, right, bottom
        public string? Case = null; // "upper", "lower", "title"

        private static FontCollection fontCollection = new();
        private static Dictionary<string, FontFamily> loadedFonts = new();

        public override void Render(IImageProcessingContext graphics, Context context)
        {
            if (string.IsNullOrEmpty(Text))
                return;

            string resolvedText = Helpers.ResolveVariables(Text, context);
            string resolvedFontName = Helpers.ResolveVariables(this.Font, context);
            string resolvedTextColor = Helpers.ResolveVariables(this.Color, context);
            string resolvedAlignment = Helpers.ResolveVariables(Alignment, context);

            // Handle case transformation
            if (Case != null)
            {
                switch (Case.ToLower())
                {
                    case "upper":
                        resolvedText = resolvedText.ToUpperInvariant();
                        break;
                    case "lower":
                        resolvedText = resolvedText.ToLowerInvariant();
                        break;
                    case "title":
                        resolvedText =
                            System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(resolvedText);
                        break;
                }
            }

            // Try to load or get the font
            FontFamily family;
            if (!loadedFonts.TryGetValue(resolvedFontName, out family))
            {
                try
                {
                    family = SystemFonts.Families.FirstOrDefault(f
                        => f.Name.Equals(resolvedFontName, StringComparison.OrdinalIgnoreCase));
                    if (family == null)
                    {
                        Console.WriteLine($"Font '{resolvedFontName}' not found. Falling back to Arial.");
                        family = SystemFonts.Families.First(f => f.Name == "Arial");
                    }
                    loadedFonts[resolvedFontName] = family;
                }
                catch
                {
                    Console.WriteLine($"Failed to load font '{resolvedFontName}', using default.");
                    family = SystemFonts.Families.First(f => f.Name == "Arial");
                }
            }

            Font font = family.CreateFont(this.Size,
                Enum.TryParse<FontStyle>(this.Variant, true, out var style) ? style : FontStyle.Regular);

            // Parse color
            Color color = SixLabors.ImageSharp.Color.ParseHex(resolvedTextColor);

            // Alignment
            HorizontalAlignment hAlign = resolvedAlignment.Contains("right")
                ? HorizontalAlignment.Right
                : resolvedAlignment.Contains("center")
                    ? HorizontalAlignment.Center
                    : HorizontalAlignment.Left;
            VerticalAlignment vAlign = resolvedAlignment.Contains("bottom")
                ? VerticalAlignment.Bottom
                : resolvedAlignment.Contains("center")
                    ? VerticalAlignment.Center
                    : VerticalAlignment.Top;

            RichTextOptions textOptions = new(font)
            {
                Origin = new PointF(X, Y),
                WrappingLength = Width,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                TextAlignment = hAlign switch
                {
                    HorizontalAlignment.Left => TextAlignment.Start,
                    HorizontalAlignment.Center => TextAlignment.Center,
                    HorizontalAlignment.Right => TextAlignment.End,
                }
            };

            if (Component.DebugMode)
                graphics.Draw(SixLabors.ImageSharp.Color.Aqua, 10, new RectangleF(X, Y, Width, Height));

            var measureOptions = new TextOptions(font)
            {
                WrappingLength = Width,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                TextAlignment = textOptions.TextAlignment,
            };
            var textSize = TextMeasurer.MeasureAdvance(resolvedText, measureOptions);

            // adjust origin based on alignment
            float originX = X;
            float originY = Y;

            // if (hAlign == HorizontalAlignment.Center)
            //     originX = X + (Width - textSize.Width) / 2;
            // else if (hAlign == HorizontalAlignment.Right)
            //     originX = X + (Width - textSize.Width);

            if (vAlign == VerticalAlignment.Center)
                originY = Y + (Height - textSize.Height) / 2;
            else if (vAlign == VerticalAlignment.Bottom)
                originY = Y + (Height - textSize.Height);

            if (Component.DebugMode)
            {
                graphics.Draw(SixLabors.ImageSharp.Color.Red, 10, new RectangleF(X - 5, Y - 5, 10, 10));
                graphics.Draw(SixLabors.ImageSharp.Color.Green, 10, new RectangleF(originX - 5, originY - 5, 10, 10));
            }

            textOptions.Origin = new PointF(originX, originY);

            graphics.DrawText(textOptions, resolvedText, color);
        }
    }
}
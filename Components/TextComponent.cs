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
        public string OutlineColor = "#000000";
        public float OutlineWidth = 0f; // 0 means no outline

        private static FontCollection fontCollection = new();
        private static Dictionary<string, FontFamily> loadedFonts = new();

        public override void Render(IImageProcessingContext graphics, Context context)
        {
            if (string.IsNullOrEmpty(this.Text))
                return;

            string resolvedText = Helpers.ResolveVariables(this.Text, context);
            string resolvedFont = Helpers.ResolveVariables(this.Font, context);
            string resolvedColor = Helpers.ResolveVariables(this.Color, context);
            string resolvedOutlineColor = Helpers.ResolveVariables(this.OutlineColor, context);
            string resolvedAlignment = Helpers.ResolveVariables(this.Alignment, context);

            // Handle case transformation
            if (this.Case != null)
            {
                switch (this.Case.ToLower())
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
            if (!TextComponent.loadedFonts.TryGetValue(resolvedFont, out family))
            {
                try
                {
                    family = SystemFonts.Families.FirstOrDefault(f
                        => f.Name.Equals(resolvedFont, StringComparison.OrdinalIgnoreCase));
                    if (family == null)
                    {
                        Console.WriteLine($"Font '{resolvedFont}' not found. Falling back to Arial.");
                        family = SystemFonts.Families.First(f => f.Name == "Arial");
                    }
                    TextComponent.loadedFonts[resolvedFont] = family;
                }
                catch
                {
                    Console.WriteLine($"Failed to load font '{resolvedFont}', using default.");
                    family = SystemFonts.Families.First(f => f.Name == "Arial");
                }
            }

            Font font = family.CreateFont(this.Size,
                Enum.TryParse<FontStyle>(this.Variant, true, out var style) ? style : FontStyle.Regular);

            // Parse color
            Color color = SixLabors.ImageSharp.Color.ParseHex(resolvedColor);
            Color outlineColor = SixLabors.ImageSharp.Color.ParseHex(resolvedOutlineColor);

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
                Origin = new PointF(this.X, this.Y),
                WrappingLength = this.Width,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                TextAlignment = hAlign switch
                {
                    HorizontalAlignment.Left => TextAlignment.Start,
                    HorizontalAlignment.Center => TextAlignment.Center,
                    HorizontalAlignment.Right => TextAlignment.End,
                }
            };

            if (context.Options.DebugMode)
                graphics.Draw(SixLabors.ImageSharp.Color.Aqua, 10,
                    new RectangleF(this.X, this.Y, this.Width, this.Height));

            var measureOptions = new TextOptions(font)
            {
                WrappingLength = this.Width,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                TextAlignment = textOptions.TextAlignment,
            };
            var textSize = TextMeasurer.MeasureAdvance(resolvedText, measureOptions);

            // adjust origin based on alignment
            float originX = this.X;
            float originY = this.Y;

            // if (hAlign == HorizontalAlignment.Center)
            //     originX = X + (Width - textSize.Width) / 2;
            // else if (hAlign == HorizontalAlignment.Right)
            //     originX = X + (Width - textSize.Width);

            if (vAlign == VerticalAlignment.Center)
                originY = this.Y + (this.Height - textSize.Height) / 2;
            else if (vAlign == VerticalAlignment.Bottom)
                originY = this.Y + (this.Height - textSize.Height);

            if (context.Options.DebugMode)
            {
                graphics.Draw(SixLabors.ImageSharp.Color.Red, 10, new RectangleF(this.X - 5, this.Y - 5, 10, 10));
                graphics.Draw(SixLabors.ImageSharp.Color.Green, 10, new RectangleF(originX - 5, originY - 5, 10, 10));
            }

            textOptions.Origin = new PointF(originX, originY);

            if (this.OutlineWidth == 0)
                graphics.DrawText(textOptions, resolvedText, new SolidBrush(color));
            else
                graphics.DrawText(textOptions, resolvedText, new SolidBrush(color),
                    new SolidPen(outlineColor, this.OutlineWidth));
        }
    }
}
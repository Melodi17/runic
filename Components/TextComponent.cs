using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Processing;
using SixLabors.Fonts;

namespace runic.Components;

using Models;

public class TextComponent : Component
{
    public string Text;
    public string Font = "Arial";
    public float Size = 12f;
    public FontStyle Variant = FontStyle.Regular;
    public string Color = "#000000";
    public string Alignment = "top,left"; // "top,left", "middle,center", "bottom,right", etc.
    public TextCase Case = TextCase.Normal;
    public string OutlineColor = "#000000";
    public float OutlineWidth = 0f; // 0 means no outline

    private static readonly Dictionary<string, FontFamily> _loadedFonts = new();

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
        resolvedText = TransformTextCase(resolvedText, this.Case);

        // Try to load or get the font
        Font font = TryLoadFont(resolvedFont, this.Size, this.Variant);

        Color color = SixLabors.ImageSharp.Color.ParseHex(resolvedColor);
        Color outlineColor = SixLabors.ImageSharp.Color.ParseHex(resolvedOutlineColor);

        if (!ParseAlignment(resolvedAlignment, out HorizontalAlignment? hAlign, out VerticalAlignment? vAlign))
        {
            Console.WriteLine($"Invalid alignment '{resolvedAlignment}', defaulting to top,left.");
            hAlign = HorizontalAlignment.Left;
            vAlign = VerticalAlignment.Top;
        }

        RichTextOptions textOptions = new(font)
        {
            Origin = new PointF(this.X, this.Y),
            WrappingLength = this.Width,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            TextAlignment = ConvertToTextAlignment(hAlign!.Value),
        };

        if (context.Options.DebugMode)
           DrawBoundingBox(
                graphics,
                this.X,
                this.Y,
                textOptions.Origin.X - this.X,
                textOptions.Origin.Y - this.Y);

        TextOptions measureOptions = new(font)
        {
            WrappingLength = this.Width,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            TextAlignment = textOptions.TextAlignment,
        };
        FontRectangle textSize = TextMeasurer.MeasureAdvance(resolvedText, measureOptions);

        // adjust origin based on alignment
        float originX = this.X;
        float originY = this.Y;

        if (vAlign == VerticalAlignment.Center)
            originY = this.Y + (this.Height - textSize.Height) / 2;
        else if (vAlign == VerticalAlignment.Bottom)
            originY = this.Y + (this.Height - textSize.Height);

        if (context.Options.DebugMode)
            DrawBoundingBox(
                graphics,
                this.X,
                this.Y,
                originX,
                originY);

        textOptions.Origin = new PointF(originX, originY);

        if (this.OutlineWidth == 0)
            graphics.DrawText(textOptions, resolvedText, new SolidBrush(color));
        else
            graphics.DrawText(
                textOptions,
                resolvedText,
                new SolidBrush(color),
                new SolidPen(outlineColor, this.OutlineWidth));
    }
    private static Font TryLoadFont(string resolvedFont, float size, FontStyle variant)
    {
        FontFamily family;
        if (!TextComponent._loadedFonts.TryGetValue(resolvedFont, out family))
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
                TextComponent._loadedFonts[resolvedFont] = family;
            }
            catch
            {
                Console.WriteLine($"Failed to load font '{resolvedFont}', using default.");
                family = SystemFonts.Families.First(f => f.Name == "Arial");
            }
        }

        Font font = family.CreateFont(size, variant);
        return font;
    }

    private string TransformTextCase(string text, TextCase textCase)
        => textCase switch
        {
            TextCase.Normal => text,
            TextCase.Upper  => text.ToUpper(),
            TextCase.Lower  => text.ToLower(),
            TextCase.Title  => System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(text.ToLower()),
            _               => throw new ArgumentOutOfRangeException(),
        };

    private static void DrawBoundingBox(
        IImageProcessingContext graphics,
        float x,
        float y,
        float width,
        float height)
    {
        graphics.Draw(
            Pens.Solid(SixLabors.ImageSharp.Color.Red, 1),
            new RectangleF(x, y, width, height));
    }

    private static bool ParseAlignment(
        string alignment,
        out HorizontalAlignment? hAlign,
        out VerticalAlignment? vAlign)
    {
        hAlign = null;
        vAlign = null;

        string[] parts = alignment.ToLower().Split(',');
        if (parts.Length != 2)
            return false;

        string hPart = parts[1].Trim();
        string vPart = parts[0].Trim();

        switch (hPart)
        {
            case "left":
                hAlign = HorizontalAlignment.Left;
                break;
            case "center":
                hAlign = HorizontalAlignment.Center;
                break;
            case "right":
                hAlign = HorizontalAlignment.Right;
                break;
            default:
                return false;
        }

        switch (vPart)
        {
            case "top":
                vAlign = VerticalAlignment.Top;
                break;
            case "middle":
                vAlign = VerticalAlignment.Center;
                break;
            case "bottom":
                vAlign = VerticalAlignment.Bottom;
                break;
            default:
                return false;
        }

        return true;
    }

    private static TextAlignment ConvertToTextAlignment(HorizontalAlignment hAlign)
    {
        return hAlign switch
        {
            HorizontalAlignment.Left   => TextAlignment.Start,
            HorizontalAlignment.Center => TextAlignment.Center,
            HorizontalAlignment.Right  => TextAlignment.End,
            _                          => throw new ArgumentOutOfRangeException(),
        };
    }
}
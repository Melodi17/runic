namespace runic.Components;

using System.Drawing;

public class TextComponent : Component
{
    public string Text;
    public string FontName = "Arial";
    public float FontSize = 12f;
    public string TextColor = "#000000";
    public string Alignment = "top,left"; // top, left, center, right, bottom
    
    public override void Render(Graphics graphics, Context context)
    {
        if (string.IsNullOrEmpty(Text))
            return;

        // Resolve variables in the text
        string resolvedText = Helpers.ResolveVariables(Text, context);
        string resolvedFontName = Helpers.ResolveVariables(FontName, context);
        string resolvedTextColor = Helpers.ResolveVariables(TextColor, context);
        string resolvedAlignment = Helpers.ResolveVariables(Alignment, context);

        // Create font and brush
        using Font font = new(resolvedFontName, FontSize);
        using Brush brush = new SolidBrush(ColorTranslator.FromHtml(resolvedTextColor));

        // Determine alignment
        StringFormat format = new StringFormat();
        if (resolvedAlignment.Contains("center"))
            format.Alignment = StringAlignment.Center;
        else if (resolvedAlignment.Contains("right"))
            format.Alignment = StringAlignment.Far;
        
        if (resolvedAlignment.Contains("middle"))
            format.LineAlignment = StringAlignment.Center;
        else if (resolvedAlignment.Contains("bottom"))
            format.LineAlignment = StringAlignment.Far;
        
        // Draw the text
        RectangleF rect = new RectangleF(X, Y, Width, Height);
        graphics.DrawString(resolvedText, font, brush, rect, format);
    }
}
namespace runic;

public static class Helpers
{
    public static string ResolveVariables(string input, Context context)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        bool substituted = false;
        // Replace variables in the format $variableName
        foreach (string var in context.DataSource.Headers.OrderBy(h => h.Length))
        {
            string placeholder = $"${var}";
            if (input.Contains(placeholder))
            {
                string value = context.CurrentRow[var];
                input = input.Replace(placeholder, value);
                substituted = true;
            }
        }

        foreach (KeyValuePair<string, string> var in context.Template.Variables.OrderBy(p => p.Key.Length))
        {
            string placeholder = $"${var.Key}";
            if (input.Contains(placeholder))
            {
                string value = var.Value;
                input = input.Replace(placeholder, value);
                substituted = true;
            }
        }

        // Replace row index variable
        if (input.Contains("$#"))
        {
            input = input.Replace("$#", context.CurrentRowIndex.ToString());
            substituted = true;
        }

        if (input.Contains("$") && substituted)
            input = Helpers.ResolveVariables(input, context);

        return input;
    }
}
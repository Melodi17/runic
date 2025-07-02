namespace runic;

public static class Helpers
{
    public static string ResolveVariables(string input, Context context)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        // Replace variables in the format $variableName
        foreach (string var in context.DataSource.Headers.OrderBy(h=> h.Length))
        {
            string placeholder = $"${var}";
            if (input.Contains(placeholder))
            {
                string value = context.CurrentRow[var];
                input = input.Replace(placeholder, value);
            }
        }
        
        // Replace row index variable
        if (input.Contains("$#"))
            input = input.Replace("$#", context.CurrentRowIndex.ToString());
        
        return input;
    }
}
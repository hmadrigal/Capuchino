using System;
using System.Text.RegularExpressions;

namespace Expresso.Ultrapico.Expresso
{
    public class RegexLibraryItem
    {
        public string Description;
        public string Expression;
        public RegexOptions Options;

        public RegexLibraryItem()
        {
            this.Description = "";
            this.Expression = "";
        }

        public RegexLibraryItem(string description, string expression, RegexOptions options)
        {
            this.Description = description;
            this.Expression = expression;
            this.Options = options;
        }

        public RegexLibraryItem(string description, string expression)
        {
            this.Description = description;
            this.Expression = expression;
            this.Options = RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace | RegexOptions.CultureInvariant;
        }
    }
}

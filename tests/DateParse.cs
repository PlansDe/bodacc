using System;
using bodacc;
using Xunit;
using System.Globalization;

namespace tests
{
    public class DateParseTests
    {
        [Fact]
        public void Test1()
        {
            var french = CultureInfo.GetCultureInfo("fr-FR");
            var styles = DateTimeStyles.AllowInnerWhite | DateTimeStyles.AllowLeadingWhite | DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.AllowTrailingWhite;
            var result = DateTime.TryParseExact("1 février 2022", "d MMMM yyyy", french, styles, out DateTime parsed_date);
            Assert.True(result);
        }
    }
}
using System;
using bodacc;
using Xunit;

namespace tests
{
    public class UnitesLegalesTests
    {
        [Fact]
        public void Test1()
        {
            const String line = "485179014,O,,2005-11-22,,F,FREDERIQUE,,,,FREDERIQUE,,,,,2017-05-20T09:16:35,3,PME,2018,2008-11-01,A,DENIAUD,,,,,,1000,70.10Z,NAFRev2,00025,,";
            var split = SireneCsvReader.ReadLine(line);
            Assert.Equal(33, split.Length);
            Assert.Equal("1000", split[27]);
        }
    }
}

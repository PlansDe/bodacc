using System;
using bodacc;
using Xunit;

namespace tests
{
    public class EtablissementsTests
    {
        [Fact]
        public void Test1()
        {
            const String line = "321569527,00062,32156952700062,O,2014-11-18,12,2018,,2020-11-20T08:33:40,false,3,,5,B,RUE,DE L ASILE POPINCOURT,75011,PARIS 11,,,75111,,,,,,,,,,,,,,,,,,,2020-11-04,F,ASIE INFINY,,,\"AUSTRALIE TOURS, NOUVELLE-ZELANDE VOYAGES, ALMA LATINA VOYAGES\",79.12Z,NAFRev2,O";
            var split = SireneCsvReader.ReadLine(line);
            Assert.Equal(48, split.Length);
            Assert.Equal("\"AUSTRALIE TOURS, NOUVELLE-ZELANDE VOYAGES, ALMA LATINA VOYAGES\"", split[44]);
        }
    }
}

using FluentAssertions;
using GTStarData;
using GTStarData.Lookups;
using NUnit.Framework;

namespace UnitTests
{
    [TestFixture]
    public class UwpLookupsTests
    {
        [TestCase("A", "Excellent Quality")]
        [TestCase("B", "Good Quality")]
        [TestCase("C", "Routine Quality")]
        [TestCase("D", "Poor Quality")]
        [TestCase("E", "Frontier Installation")]
        [TestCase("X", "No Starport")]
        public void Starport_Lookups_Return_Expected_ShortText(string code, string expected)
        {
            var lookup = UwpLookups.GetLookup(TextType.Starport, code);
            lookup.TextType.Should().Be(TextType.Starport);
            lookup.Code.Should().Be(code);
            lookup.ShortText.Should().Be(expected);
            lookup.LongText.Should().NotBeNullOrEmpty();
        }

        [TestCase("0", "Asteroid Belt")]
        [TestCase("7", "11,200 km (0.70-0.80G)")]
        [TestCase("8", "12,800 km (0.90-1.00G)")]
        [TestCase("A", "16,000 km (1.15-1.25G)")]
        public void PlanetSize_Lookups_Return_Expected_ShortText(string code, string expected)
        {
            var lookup = UwpLookups.GetLookup(TextType.PlanetSize, code);
            lookup.ShortText.Should().Be(expected);
        }

        [TestCase("0", "No Atmosphere")]
        [TestCase("6", "Standard")]
        [TestCase("8", "Dense")]
        [TestCase("A", "Exotic")]
        public void Atmosphere_Lookups_Return_Expected_ShortText(string code, string expected)
        {
            var lookup = UwpLookups.GetLookup(TextType.Atmosphere, code);
            lookup.ShortText.Should().Be(expected);
        }

        [TestCase("0", "0% Water (Desert World)")]
        [TestCase("7", "70% Water (Earth-like)")]
        [TestCase("A", "100% Water (Water World)")]
        public void Hydrosphere_Lookups_Return_Expected_ShortText(string code, string expected)
        {
            var lookup = UwpLookups.GetLookup(TextType.Hydrosphere, code);
            lookup.ShortText.Should().Be(expected);
        }

        [TestCase("0", "Unpopulated")]
        [TestCase("6", "Millions")]
        [TestCase("8", "Hundreds of Millions")]
        [TestCase("9", "Billions")]
        public void Population_Lookups_Return_Expected_ShortText(string code, string expected)
        {
            var lookup = UwpLookups.GetLookup(TextType.Population, code);
            lookup.ShortText.Should().Be(expected);
        }

        [TestCase("0", "None / Anarchy")]
        [TestCase("4", "Representative Democracy")]
        [TestCase("6", "Captive Government / Colony")]
        [TestCase("9", "Impersonal Bureaucracy")]
        public void Government_Lookups_Return_Expected_ShortText(string code, string expected)
        {
            var lookup = UwpLookups.GetLookup(TextType.Government, code);
            lookup.ShortText.Should().Be(expected);
        }

        [TestCase("0", "No Prohibitions")]
        [TestCase("6", "All firearms except shotguns banned")]
        [TestCase("9", "Weapon possession outside home banned")]
        [TestCase("A", "All weapons prohibited")]
        public void LawLevel_Lookups_Return_Expected_ShortText(string code, string expected)
        {
            var lookup = UwpLookups.GetLookup(TextType.LawLevel, code);
            lookup.ShortText.Should().Be(expected);
        }

        [TestCase("0", "TL 0 - Stone Age")]
        [TestCase("C", "TL 12 - Average Imperial")]
        [TestCase("F", "TL 15 - Imperial Maximum")]
        public void TechLevel_Lookups_Return_Expected_ShortText(string code, string expected)
        {
            var lookup = UwpLookups.GetLookup(TextType.TechLevel, code);
            lookup.ShortText.Should().Be(expected);
        }

        [TestCase("Im", "Third Imperium")]
        [TestCase("ImDd", "Third Imperium, Domain of Deneb")]
        [TestCase("ZhIN", "Zhodani Consulate, Iadr Nsobl Province")]
        [TestCase("SwCf", "Sword Worlds Confederation")]
        [TestCase("DaCf", "Darrian Confederation")]
        [TestCase("NaHu", "Non-Aligned, Human-dominated")]
        public void Allegiance_Lookups_Return_Expected_ShortText(string code, string expected)
        {
            var lookup = UwpLookups.GetLookup(TextType.Allegiance, code);
            lookup.ShortText.Should().Be(expected);
        }

        [TestCase("4", "Very Important (+4)")]
        [TestCase("0", "Ordinary (0)")]
        [TestCase("-2", "Very Unimportant (-2)")]
        public void Importance_Lookups_Return_Expected_ShortText(string code, string expected)
        {
            var lookup = UwpLookups.GetLookup(TextType.Importance, code);
            lookup.ShortText.Should().Be(expected);
        }

        [TestCase("Ag", "Agricultural")]
        [TestCase("Ri", "Rich")]
        [TestCase("Hi", "High Population")]
        [TestCase("Ht", "High Tech")]
        [TestCase("Cp", "Subsector Capital")]
        public void TradeCode_Lookups_Return_Expected_ShortText(string code, string expected)
        {
            var lookup = UwpLookups.GetLookup(TextType.TradeCode, code);
            lookup.ShortText.Should().Be(expected);
        }

        [Test]
        public void Economics_Extension_Parses_Correctly()
        {
            var lookup = UwpLookups.GetLookup(TextType.Ecomomics, "(D7E+5)");
            lookup.ShortText.Should().Contain("Vast Resources");
            lookup.ShortText.Should().Contain("Tens of Millions");
            lookup.ShortText.Should().Contain("Efficiency +5");
        }

        [Test]
        public void Culture_Extension_Parses_Correctly()
        {
            var lookup = UwpLookups.GetLookup(TextType.Culture, "[9C6D]");
            lookup.ShortText.Should().Contain("Pluralistic Society");
            lookup.ShortText.Should().Contain("Xenophilic");
            lookup.ShortText.Should().Contain("Distinct Local Quirks");
        }

        [Test]
        public void Unknown_Code_Returns_TextLookup_With_Code_And_Type()
        {
            var lookup = UwpLookups.GetLookup(TextType.Starport, "ZZZ");
            lookup.TextType.Should().Be(TextType.Starport);
            lookup.Code.Should().Be("ZZZ");
            lookup.ShortText.Should().BeNull();
        }
    }
}

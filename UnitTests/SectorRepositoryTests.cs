using FluentAssertions;
using GTStarData;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace UnitTests
{
    [TestFixture]
    public class SectorRepositoryTests
    {
        private GTStarDbContext _context;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<GTStarDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new GTStarDbContext(options);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Dispose();
        }

        [Test]
        public void FindTextLookup_Uses_Static_Lookups_For_Uwp_Types()
        {
            var starport = _context.FindTextLookup(TextType.Starport, "A");
            starport.Should().NotBeNull();
            starport.ShortText.Should().Be("Excellent Quality");

            var size = _context.FindTextLookup(TextType.PlanetSize, "7");
            size.Should().NotBeNull();
            size.ShortText.Should().Be("11,200 km (0.70-0.80G)");

            var pop = _context.FindTextLookup(TextType.Population, "8");
            pop.Should().NotBeNull();
            pop.ShortText.Should().Be("Hundreds of Millions");
        }

        [Test]
        public void FindTextLookup_Falls_Back_To_Database_For_Non_Static_Types()
        {
            // Seed a non-static lookup (e.g. Climate, Bases, TravelZone)
            var dbLookup = new TextLookup
            {
                Id = Guid.NewGuid(),
                TextType = TextType.TravelZone,
                Code = "A",
                ShortText = "Amber Zone Database",
                LongText = "Travel with caution"
            };
            _context.TextLookups.Add(dbLookup);
            _context.SaveChanges();

            var result = _context.FindTextLookup(TextType.TravelZone, "A");
            result.Should().NotBeNull();
            result.ShortText.Should().Be("Amber Zone Database");
            result.LongText.Should().Be("Travel with caution");
        }

        [Test]
        public void FindTradeCodeLookup_Combines_Multiple_Trade_Codes()
        {
            var result = _context.FindTradeCodeLookup("Ri Pa Ph");
            result.Code.Should().Be("Ri Pa Ph");
            result.ShortText.Should().Be("Rich. Pre-Agricultural. Pre-High Population.");
        }
    }
}

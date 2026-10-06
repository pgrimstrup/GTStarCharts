using FluentAssertions;
using GTStarData;
using NUnit.Framework;

namespace UnitTests
{
    [TestFixture]
    public class StellarDataTests
    {
        [TestCase("G1 V M1 V", 2)]
        [TestCase("F0 V", 1)]
        [TestCase("M2 V", 1)]
        [TestCase("G4 V M5 V", 2)]
        [TestCase("F2 V M2 V", 2)]
        [TestCase("K6 V M0 V", 2)]
        [TestCase("M0 V M2 V", 2)]
        [TestCase("M4 III M0 V", 2)]
        [TestCase("G2 V M5 V K1 V M3 V", 3)]
        public void Sample_Stellar_Data_Is_Parsed(string stellar, int expectedStars)
        {
            var data = StellarDataFactory.Create(stellar, 12800);

            data.Stars.Should().HaveCount(expectedStars);
            data.Stars.Should().OnlyContain(s => s.Temperature > 0 && s.Radius > 0 && s.Mass > 0 && s.Luminosity > 0);
            data.Stars.Should().OnlyContain(s => s.InnerHZ > 0 && s.OuterHZ > s.InnerHZ);
            data.Stars.Should().OnlyContain(s => s.EarthEquivalentOrbit > s.InnerHZ && s.EarthEquivalentOrbit < s.OuterHZ);
            data.StellarDescription.Should().NotBeNullOrEmpty();
            data.HabitableZoneDescription.Should().NotBeNullOrEmpty();
            data.OrbitalPeriod.Should().NotBeNullOrEmpty();
            data.SafeJumpTimes.Select(t => t.Acceleration).Should().Equal(1, 2, 3, 4, 5, 6);

            TestContext.Out.WriteLine($"[{stellar}]");
            TestContext.Out.WriteLine(data.StellarDescription);
            foreach (var star in data.Stars)
                TestContext.Out.WriteLine($"  {star.Role}: {star.Notes}");
            TestContext.Out.WriteLine(data.HabitableZoneDescription);
            TestContext.Out.WriteLine(data.OrbitalPeriod);
            TestContext.Out.WriteLine(data.TidalLockingDescription);
            TestContext.Out.WriteLine(data.SafeJumpWarning);
            TestContext.Out.WriteLine(data.SafeJumpDescription);
            foreach (var t in data.SafeJumpTimes)
                TestContext.Out.WriteLine($"  {t.Acceleration}G: {t.TimeText} / {t.TimeNoTurnoverText}");
        }

        [Test]
        public void Only_First_Three_Stars_Are_Considered()
        {
            var data = StellarDataFactory.Create("G2 V M5 V K1 V M3 V");

            data.Configuration.Should().Be(StellarConfiguration.Trinary);
            data.Stars.Select(s => s.Code).Should().Equal("G2 V", "M5 V", "K1 V");
            data.Stars.Select(s => s.Role).Should().Equal("Primary", "Secondary", "Tertiary");
        }

        [Test]
        public void Sun_Like_Star_Matches_Real_Values()
        {
            var data = StellarDataFactory.Create("G2 V", 12800);
            var sun = data.Primary;

            sun.CommonName.Should().Be("Yellow Dwarf");
            sun.Temperature.Should().BeApproximately(5772, 50);
            sun.Radius.Should().BeApproximately(1.0, 0.02);
            sun.Mass.Should().BeApproximately(1.0, 0.01);
            sun.Luminosity.Should().BeApproximately(1.0, 0.05);

            // Kopparapu et al. (2014) conservative habitable zone for the Sun: ~0.95 to ~1.68 AU
            sun.InnerHZ.Should().BeApproximately(0.95, 0.03);
            sun.OuterHZ.Should().BeApproximately(1.68, 0.03);
            sun.EarthEquivalentOrbit.Should().BeApproximately(1.0, 0.02);

            // One year at the Earth-equivalent orbit
            data.MeanOrbitalPeriod.Should().BeApproximately(365.25, 10);

            // Sun's jump shadow: 100 diameters is ~139 million km (~0.93 AU), inside Earth's orbit
            sun.JumpShadowKm.Should().BeApproximately(100 * 2 * 695700, 3e6);
            data.SafeJumpWarning.Should().BeNull();
            data.JumpLimitedByStar.Should().BeFalse();
            data.SafeJumpDistanceKm.Should().Be(1280000);
        }

        [Test]
        public void Safe_Jump_Times_Use_Standard_Traveller_Formula()
        {
            var data = StellarDataFactory.Create("G2 V", 12800);
            var oneG = data.SafeJumpTimes.Single(t => t.Acceleration == 1);

            // T = 2 x sqrt(D / A) = 2 x sqrt(1.28e9 m / 9.80665 m/s^2) ~ 22,850 s (~6.3 hours)
            oneG.Time.TotalSeconds.Should().BeApproximately(2 * Math.Sqrt(1.28e9 / 9.80665), 1);
            oneG.TimeNoTurnover.TotalSeconds.Should().BeApproximately(Math.Sqrt(2 * 1.28e9 / 9.80665), 1);
            oneG.TimeText.Should().Be("6 hours 21 minutes");

            var fourG = data.SafeJumpTimes.Single(t => t.Acceleration == 4);
            fourG.Time.TotalSeconds.Should().BeApproximately(oneG.Time.TotalSeconds / 2, 1);
        }

        [Test]
        public void Giant_Primary_Places_Main_World_Inside_Stellar_Jump_Shadow()
        {
            var data = StellarDataFactory.Create("M4 III M0 V", 8000);
            var giant = data.Primary;

            giant.CommonName.Should().Be("Red Giant");
            giant.Radius.Should().BeInRange(60, 150);
            giant.Luminosity.Should().BeInRange(500, 3000);

            data.JumpLimitedByStar.Should().BeTrue();
            data.SafeJumpWarning.Should().NotBeNull();
            data.SafeJumpDistanceKm.Should().BeApproximately(giant.JumpShadowKm - data.MainWorldOrbitAU * StellarTables.AuKm, 1);
            data.SafeJumpTimes.Single(t => t.Acceleration == 1).Time.TotalDays.Should().BeGreaterThan(5);
        }

        [Test]
        public void Red_Dwarf_Main_World_Is_Tidally_Locked()
        {
            var data = StellarDataFactory.Create("M2 V", 8000);

            data.Primary.CommonName.Should().Be("Red Dwarf");
            data.MainWorldOrbitAU.Should().BeLessThan(0.25);
            data.TidalLockingDescription.Should().Contain("tidally locked");
            data.MeanOrbitalPeriod.Should().BeLessThan(60);
        }

        [TestCase(SpectralType.O5, "Blue")]
        [TestCase(SpectralType.B9, "Blue-White")]
        [TestCase(SpectralType.A9, "White")]
        [TestCase(SpectralType.F9, "Yellow-White")]
        [TestCase(SpectralType.G9, "Yellow")]
        [TestCase(SpectralType.K9, "Orange")]
        [TestCase(SpectralType.M9, "Red")]
        [TestCase(SpectralType.L9, "Dark Red")]
        [TestCase(SpectralType.T2, "Magenta")]
        public void Every_Spectral_Subtype_Has_A_Color(SpectralType type, string expected)
        {
            StellarDataFactory.GetSpectralColorName(type).Should().Be(expected);
        }

        [Test]
        public void Every_Spectral_Type_Has_Main_Sequence_Properties()
        {
            foreach (var type in Enum.GetValues<SpectralType>().Where(t => t != SpectralType.Unknown))
            {
                var p = StellarTables.GetProperties(type, StellarClass.V);
                p.Temperature.Should().BeGreaterThan(0, type.ToString());
                p.Radius.Should().BeGreaterThan(0, type.ToString());
                p.Mass.Should().BeGreaterThan(0, type.ToString());
            }
        }

        [Test]
        public void Giants_Are_Larger_Than_Dwarfs_Of_The_Same_Type()
        {
            foreach (var type in new[] { SpectralType.B5, SpectralType.A0, SpectralType.F5, SpectralType.G2, SpectralType.K0, SpectralType.M2 })
            {
                double v = StellarFactoryRadius(StellarClass.V, type);
                double iv = StellarFactoryRadius(StellarClass.IV, type);
                double iii = StellarFactoryRadius(StellarClass.III, type);
                double ii = StellarFactoryRadius(StellarClass.II, type);
                double ib = StellarFactoryRadius(StellarClass.Ib, type);
                double ia = StellarFactoryRadius(StellarClass.Ia, type);

                iv.Should().BeGreaterThan(v, type.ToString());
                iii.Should().BeGreaterThan(iv, type.ToString());
                ii.Should().BeGreaterThan(iii, type.ToString());
                ib.Should().BeGreaterThan(ii, type.ToString());
                ia.Should().BeGreaterThan(ib, type.ToString());
            }
        }

        static double StellarFactoryRadius(StellarClass cls, SpectralType type) => StellarDataFactory.GetRadius(cls, type);

        [Test]
        public void Parses_Supergiants_White_Dwarfs_And_Brown_Dwarfs()
        {
            var data = StellarDataFactory.Create("M2 Ia D BD");

            data.Stars.Select(s => s.StarClass).Should().Equal(StellarClass.Ia, StellarClass.VII, StellarClass.BD);
            data.Stars[0].CommonName.Should().Be("Red Supergiant");
            data.Stars[1].CommonName.Should().Be("White Dwarf");
            data.Stars[2].CommonName.Should().Be("Brown Dwarf");
        }

        [Test]
        public void Ignores_Unrecognised_And_Numeric_Tokens()
        {
            var data = StellarDataFactory.Create("5 V G2 V");

            data.Stars.Should().ContainSingle().Which.SpectralType.Should().Be(SpectralType.G2);
        }

        [Test]
        public void Empty_Stellar_Data_Uses_World_Jump_Shadow_Only()
        {
            var data = StellarDataFactory.Create("", 6400);

            data.Stars.Should().BeEmpty();
            data.StellarDescription.Should().NotBeNullOrEmpty();
            data.SafeJumpDistanceKm.Should().Be(640000);
            data.SafeJumpTimes.Should().HaveCount(6);
        }

        [TestCase("A788899-C", 11200)]
        [TestCase("B000000-0", 0)]
        [TestCase("CA00000-0", 16000)]
        [TestCase("ES00000-0", 600)]
        public void Main_World_Diameter_From_Uwp(string uwp, double expected)
        {
            new SystemData { UWP = uwp }.MainWorldDiameterKm().Should().Be(expected);
        }
    }
}

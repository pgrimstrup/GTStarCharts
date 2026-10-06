using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace GTStarData
{
    public enum StellarConfiguration
    {
        Single,
        Binary,
        Trinary
    }

    public enum StellarClass
    {
        Unknown,
        O,   // Hypergiant (luminosity class 0 / Ia+)
        I,   // Super Giant (unspecified, or Iab)
        II,  // Bright Giant
        III, // Giant
        IV,  // Subgiant
        V,    // Main Sequence (Dwarf)
        VI,   // Subdwarf
        VII,  // White Dwarf (also written as "D")
        Ia,   // Luminous Supergiant
        Ib,   // Less Luminous Supergiant
        BD    // Brown Dwarf
    }

    public enum SpectralType
    {
        Unknown,
        O3, O4, O5, O6, O7, O8, O9,
        B0, B1, B2, B3, B4, B5, B6, B7, B8, B9,
        A0, A1, A2, A3, A4, A5, A6, A7, A8, A9,
        F0, F1, F2, F3, F4, F5, F6, F7, F8, F9,
        G0, G1, G2, G3, G4, G5, G6, G7, G8, G9,
        K0, K1, K2, K3, K4, K5, K6, K7, K8, K9,
        M0, M1, M2, M3, M4, M5, M6, M7, M8, M9,
        L0, L1, L2, L3, L4, L5, L6, L7, L8, L9,
        T2, T3, T4, T5, T6, T7, T8

    }

    /// <summary>
    /// Physical details of a single star within a system.
    /// </summary>
    public class StarDetails
    {
        /// <summary>Primary, Secondary or Tertiary.</summary>
        public string Role { get; set; }

        /// <summary>Stellar code as it appears in the system data, e.g. "G2 V".</summary>
        public string Code { get; set; }

        public SpectralType SpectralType { get; set; }
        public StellarClass StarClass { get; set; }

        public string ColorName { get; set; }
        public string ClassName { get; set; }

        /// <summary>Everyday name for the star, e.g. "Yellow Dwarf", "Red Giant".</summary>
        public string CommonName { get; set; }

        /// <summary>Effective surface temperature (K).</summary>
        public double Temperature { get; set; }

        /// <summary>Radius in solar radii.</summary>
        public double Radius { get; set; }

        /// <summary>Mass in solar masses.</summary>
        public double Mass { get; set; }

        /// <summary>Luminosity in solar luminosities.</summary>
        public double Luminosity { get; set; }

        /// <summary>Inner edge of the conservative habitable zone (AU).</summary>
        public double InnerHZ { get; set; }

        /// <summary>Outer edge of the conservative habitable zone (AU).</summary>
        public double OuterHZ { get; set; }

        /// <summary>Orbit at the same relative position in the habitable zone as Earth is around the Sun (AU).</summary>
        public double EarthEquivalentOrbit { get; set; }

        public double RadiusKm => Radius * StellarTables.SolarRadiusKm;
        public double DiameterKm => 2 * RadiusKm;

        /// <summary>Radius of the star's jump shadow (100 diameters) measured from its centre, in km.</summary>
        public double JumpShadowKm => 100 * DiameterKm;
        public double JumpShadowAU => JumpShadowKm / StellarTables.AuKm;

        /// <summary>Single line summary of the star.</summary>
        public string Description { get; set; }

        /// <summary>Real-life notes about this kind of star.</summary>
        public string Notes { get; set; }
    }

    /// <summary>
    /// Time to travel from the main world to the safe jump distance at a given acceleration.
    /// </summary>
    public class SafeJumpTime
    {
        /// <summary>Acceleration in G.</summary>
        public int Acceleration { get; set; }

        public double DistanceKm { get; set; }

        /// <summary>Accelerate to the midpoint, then decelerate (the standard Traveller formula T = 2 x sqrt(D/A)).</summary>
        public TimeSpan Time { get; set; }

        /// <summary>Constant acceleration all the way (no turnover; the ship arrives at speed), T = sqrt(2D/A).</summary>
        public TimeSpan TimeNoTurnover { get; set; }

        public string TimeText => StellarDataFactory.FormatDuration(Time);
        public string TimeNoTurnoverText => StellarDataFactory.FormatDuration(TimeNoTurnover);
    }

    public class StellarData
    {
        /// <summary>Maximum number of stellar entries considered for a system.</summary>
        public const int MaxStars = 3;

        public StellarConfiguration Configuration { get; set; }
        public StellarClass StarClass { get; set; }
        public SpectralType SpectralType { get; set; }
        public StellarClass StarClass2 { get; set; }
        public SpectralType SpectralType2 { get; set; }
        public StellarClass StarClass3 { get; set; }
        public SpectralType SpectralType3 { get; set; }

        /// <summary>The (up to 3) stars in the system, primary first.</summary>
        public List<StarDetails> Stars { get; } = new List<StarDetails>();
        public StarDetails Primary => Stars.FirstOrDefault();

        // Primary star details
        public double StellarTemp { get; set; }
        public double StellarRadius { get; set; }
        public double StellarMass { get; set; }
        public double Luminosity { get; set; }
        public string StellarDescription { get; set; }
        public string HabitableZoneDescription { get; set; }
        public string OrbitalPeriod { get; set; }
        public string TidalLockingDescription { get; set; }

        /// <summary>Set when the main world orbits inside the primary star's jump shadow.</summary>
        public string SafeJumpWarning { get; set; }

        public double InnerHZ { get; set; }
        public double OuterHZ { get; set; }

        /// <summary>Orbital periods (days) at the inner edge, outer edge and Earth-equivalent orbit of the habitable zone.</summary>
        public double MinOrbitalPeriod { get; set; }
        public double MaxOrbitalPeriod { get; set; }
        public double MeanOrbitalPeriod { get; set; }

        // Main world and jump details
        /// <summary>Assumed orbit of the main world around the primary (AU) - the Earth-equivalent orbit.</summary>
        public double MainWorldOrbitAU { get; set; }
        public double MainWorldDiameterKm { get; set; }

        /// <summary>100 diameters of the main world (km).</summary>
        public double WorldJumpShadowKm { get; set; }

        /// <summary>100 diameters of the primary star, measured from the star's centre (km).</summary>
        public double StarJumpShadowKm { get; set; }

        /// <summary>Distance the ship must travel from the main world to be clear of both jump shadows (km).</summary>
        public double SafeJumpDistanceKm { get; set; }

        /// <summary>True if the star's jump shadow, rather than the world's, sets the safe jump distance.</summary>
        public bool JumpLimitedByStar { get; set; }

        public string SafeJumpDescription { get; set; }
        public List<SafeJumpTime> SafeJumpTimes { get; } = new List<SafeJumpTime>();
    }

    public class StellarDataFactory
    {
        /// <summary>Starship accelerations included in the safe jump table.</summary>
        public static readonly int[] Accelerations = { 1, 2, 3, 4, 5, 6 };

        static readonly string[] Roles = { "Primary", "Secondary", "Tertiary" };

        static readonly Regex SpectralPattern = new Regex(@"^([OBAFGKMLT])(\d)(\.\d+)?$", RegexOptions.Compiled);
        static readonly Regex WhiteDwarfPattern = new Regex(@"^(D[ABOQZCX]?|WD)$", RegexOptions.Compiled);

        /// <summary>
        /// Create stellar data for a system.
        /// </summary>
        /// <param name="stellarInfo">Stellar codes, e.g. "G2 V M5 V". Only the first 3 stars are considered.</param>
        /// <param name="mainWorldDiameterKm">Diameter of the main world in km (0 for asteroid belts or if unknown).</param>
        public static StellarData Create(string stellarInfo, double mainWorldDiameterKm = 0)
        {
            StellarData data = new StellarData();
            data.MainWorldDiameterKm = Math.Max(0, mainWorldDiameterKm);

            foreach (var (type, cls, code) in Parse(stellarInfo).Take(StellarData.MaxStars))
                data.Stars.Add(CreateStar(type, cls, code, Roles[data.Stars.Count]));

            switch (data.Stars.Count)
            {
                case 0:
                    data.StellarDescription = string.IsNullOrWhiteSpace(stellarInfo)
                        ? "No stellar data available. "
                        : $"Unknown stellar configuration [{stellarInfo}]. ";
                    CalculateSafeJump(data);
                    return data;

                case 1:
                    data.Configuration = StellarConfiguration.Single;
                    data.StellarDescription = $"Single star system. {data.Stars[0].Description}";
                    break;

                case 2:
                    data.Configuration = StellarConfiguration.Binary;
                    data.StellarDescription = $"Binary star system. Primary {data.Stars[0].Description}Secondary {data.Stars[1].Description}";
                    break;

                default:
                    data.Configuration = StellarConfiguration.Trinary;
                    data.StellarDescription = $"Trinary star system. Primary {data.Stars[0].Description}Secondary {data.Stars[1].Description}Tertiary {data.Stars[2].Description}";
                    break;
            }

            // Flat properties (kept for compatibility)
            var primary = data.Stars[0];
            data.StarClass = primary.StarClass;
            data.SpectralType = primary.SpectralType;
            if (data.Stars.Count > 1)
            {
                data.StarClass2 = data.Stars[1].StarClass;
                data.SpectralType2 = data.Stars[1].SpectralType;
            }
            if (data.Stars.Count > 2)
            {
                data.StarClass3 = data.Stars[2].StarClass;
                data.SpectralType3 = data.Stars[2].SpectralType;
            }

            data.StellarTemp = primary.Temperature;
            data.StellarRadius = primary.Radius;
            data.StellarMass = primary.Mass;
            data.Luminosity = primary.Luminosity;
            data.InnerHZ = primary.InnerHZ;
            data.OuterHZ = primary.OuterHZ;
            data.MainWorldOrbitAU = primary.EarthEquivalentOrbit;
            data.MinOrbitalPeriod = GetOrbitalPeriod(primary.InnerHZ, primary.Mass);
            data.MaxOrbitalPeriod = GetOrbitalPeriod(primary.OuterHZ, primary.Mass);
            data.MeanOrbitalPeriod = GetOrbitalPeriod(primary.EarthEquivalentOrbit, primary.Mass);

            data.HabitableZoneDescription = DescribeHabitableZone(data);
            data.OrbitalPeriod = DescribeOrbitalPeriod(data);
            data.TidalLockingDescription = DescribeTidalLocking(data);

            CalculateSafeJump(data);

            return data;
        }

        /// <summary>
        /// Parse a stellar string into (spectral type, luminosity class, code) entries.
        /// Handles pairs such as "G2 V", supergiant classes "Ia"/"Ib"/"Iab", white dwarfs ("D", "DA", "M2 D")
        /// and brown dwarfs ("BD"). A spectral type with no luminosity class is assumed to be main sequence.
        /// Unrecognised tokens are ignored.
        /// </summary>
        public static IEnumerable<(SpectralType Type, StellarClass Class, string Code)> Parse(string stellarInfo)
        {
            if (string.IsNullOrWhiteSpace(stellarInfo))
                yield break;

            var tokens = stellarInfo.Split(new[] { ' ', '\t', ',' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < tokens.Length; i++)
            {
                var token = tokens[i];

                if (token.Equals("BD", StringComparison.OrdinalIgnoreCase))
                {
                    yield return (SpectralType.Unknown, StellarClass.BD, "BD");
                    continue;
                }

                if (WhiteDwarfPattern.IsMatch(token))
                {
                    yield return (SpectralType.Unknown, StellarClass.VII, token);
                    continue;
                }

                if (!TryParseSpectralType(token, out var type))
                    continue;

                string next = i + 1 < tokens.Length ? tokens[i + 1] : null;
                if (next != null && TryParseStellarClass(next, out var cls))
                {
                    yield return (type, cls, $"{token} {next}");
                    i++;
                }
                else if (next != null && next == "D")
                {
                    yield return (type, StellarClass.VII, $"{token} {next}");
                    i++;
                }
                else
                {
                    yield return (type, StellarClass.V, token);
                }
            }
        }

        public static bool TryParseSpectralType(string token, out SpectralType type)
        {
            type = SpectralType.Unknown;
            var match = SpectralPattern.Match(token ?? "");

            // Decimal subtypes (e.g. "G2.5") are truncated to the whole subtype
            return match.Success && Enum.TryParse(match.Groups[1].Value + match.Groups[2].Value, out type) && Enum.IsDefined(typeof(SpectralType), type);
        }

        public static bool TryParseStellarClass(string token, out StellarClass cls)
        {
            switch (token)
            {
                case "0":
                case "Ia0":
                case "Ia+":
                case "0-Ia":
                    cls = StellarClass.O;
                    return true;
                case "Ia":
                    cls = StellarClass.Ia;
                    return true;
                case "Iab":
                case "I":
                    cls = StellarClass.I;
                    return true;
                case "Ib":
                    cls = StellarClass.Ib;
                    return true;
                case "II":
                    cls = StellarClass.II;
                    return true;
                case "III":
                    cls = StellarClass.III;
                    return true;
                case "IV":
                    cls = StellarClass.IV;
                    return true;
                case "V":
                    cls = StellarClass.V;
                    return true;
                case "VI":
                    cls = StellarClass.VI;
                    return true;
                case "VII":
                    cls = StellarClass.VII;
                    return true;
                default:
                    cls = StellarClass.Unknown;
                    return false;
            }
        }

        public static StarDetails CreateStar(SpectralType type, StellarClass cls, string code, string role = "Primary")
        {
            var props = StellarTables.GetProperties(type, cls);

            var star = new StarDetails {
                Role = role,
                Code = code,
                SpectralType = type,
                StarClass = cls,
                Temperature = props.Temperature,
                Radius = props.Radius,
                Mass = props.Mass,
                Luminosity = GetLuminosity(props.Radius, props.Temperature),
                ColorName = GetSpectralColorName(type) ?? GetDefaultColorName(cls),
                ClassName = GetStellarClassName(cls),
                CommonName = GetCommonName(type, cls),
            };

            if (star.Luminosity > 0)
            {
                star.InnerHZ = Math.Sqrt(star.Luminosity / StellarTables.InnerHabitableZoneFlux(star.Temperature));
                star.OuterHZ = Math.Sqrt(star.Luminosity / StellarTables.OuterHabitableZoneFlux(star.Temperature));
                star.EarthEquivalentOrbit = Math.Sqrt(star.Luminosity / StellarTables.EarthEquivalentFlux(star.Temperature));
            }

            star.Description = DescribeStar(star);
            star.Notes = GetStarNotes(star);
            return star;
        }

        /// <summary>
        /// Calculate the distance a ship must travel from the main world to clear both the world's and the primary
        /// star's jump shadows (100 diameters), and the time to do so at 1G to 6G.
        /// The ship is assumed to depart radially away from the star.
        /// </summary>
        static void CalculateSafeJump(StellarData data)
        {
            data.WorldJumpShadowKm = 100 * data.MainWorldDiameterKm;

            var primary = data.Primary;
            if (primary != null)
            {
                double orbitKm = data.MainWorldOrbitAU * StellarTables.AuKm;
                data.StarJumpShadowKm = primary.JumpShadowKm;

                double clearOfStar = Math.Max(0, data.StarJumpShadowKm - orbitKm);
                data.JumpLimitedByStar = clearOfStar > data.WorldJumpShadowKm;
                data.SafeJumpDistanceKm = Math.Max(data.WorldJumpShadowKm, clearOfStar);

                if (clearOfStar > 0)
                {
                    data.SafeJumpWarning = $"WARNING: The main world orbits inside the jump shadow of its {primary.Code} primary star. " +
                        $"The star's 100-diameter limit extends {FormatDistance(data.StarJumpShadowKm)} from the star, " +
                        $"so ships must travel {FormatDistance(clearOfStar)} outward from the main world before jumping. ";
                }
            }
            else
            {
                data.SafeJumpDistanceKm = data.WorldJumpShadowKm;
            }

            data.SafeJumpDescription = DescribeSafeJump(data);

            foreach (int g in Accelerations)
            {
                data.SafeJumpTimes.Add(new SafeJumpTime {
                    Acceleration = g,
                    DistanceKm = data.SafeJumpDistanceKm,
                    Time = GetTravelTime(data.SafeJumpDistanceKm, g),
                    TimeNoTurnover = GetTravelTime(data.SafeJumpDistanceKm, g, turnover: false),
                });
            }
        }

        /// <summary>
        /// Travel time over a distance at a constant acceleration.
        /// </summary>
        /// <param name="distanceInKilometers">Distance to travel (km).</param>
        /// <param name="acceleration">Acceleration in G.</param>
        /// <param name="turnover">True to accelerate to the midpoint then decelerate, T = 2 x sqrt(D/A) (standard Traveller formula).
        /// False to accelerate all the way, T = sqrt(2D/A).</param>
        public static TimeSpan GetTravelTime(double distanceInKilometers, double acceleration, bool turnover = true)
        {
            if (distanceInKilometers <= 0 || acceleration <= 0)
                return TimeSpan.Zero;

            double metres = distanceInKilometers * 1000;
            double a = acceleration * StellarTables.StandardGravity;
            double seconds = turnover ? 2 * Math.Sqrt(metres / a) : Math.Sqrt(2 * metres / a);
            return TimeSpan.FromSeconds(seconds);
        }

        public static string TimeToTravel(double distanceInKilometers, int acceleration)
        {
            return FormatDuration(GetTravelTime(distanceInKilometers, acceleration));
        }

        public static string FormatDuration(TimeSpan time)
        {
            if (time <= TimeSpan.Zero)
                return "Immediate";

            if (time.TotalDays >= 1)
            {
                long hours = (long)Math.Round(time.TotalHours);
                if (hours % 24 == 0)
                    return Plural(hours / 24, "day");
                return $"{Plural(hours / 24, "day")} {Plural(hours % 24, "hour")}";
            }

            if (time.TotalHours >= 1)
            {
                long minutes = (long)Math.Round(time.TotalMinutes);
                if (minutes % 60 == 0)
                    return Plural(minutes / 60, "hour");
                return $"{Plural(minutes / 60, "hour")} {Plural(minutes % 60, "minute")}";
            }

            if (time.TotalMinutes >= 1)
                return Plural((long)Math.Round(time.TotalMinutes), "minute");

            return Plural(Math.Max(1, (long)Math.Round(time.TotalSeconds)), "second");
        }

        static string Plural(long value, string unit) => value == 1 ? $"1 {unit}" : $"{value:n0} {unit}s";

        /// <summary>
        /// Orbital period in days (Kepler's third law), for an orbit of the given radius (AU) around a star of the given mass (solar masses).
        /// </summary>
        public static double GetOrbitalPeriod(double radiusAU, double starMass)
        {
            if (radiusAU <= 0 || starMass <= 0)
                return 0;

            return 365.25 * Math.Sqrt(Math.Pow(radiusAU, 3) / starMass);
        }

        /// <summary>
        /// Approximate orbital distance (AU) within which an Earth-like world is likely to become tidally locked
        /// within a few billion years. Scales with the cube root of stellar mass (Peale 1977; Kasting et al. 1993).
        /// </summary>
        public static double GetTidalLockRadius(double starMass) => 0.4 * Math.Cbrt(Math.Max(0, starMass));

        public static double GetLuminosity(double stellarRadius, double stellarTemp)
        {
            return Math.Pow(stellarRadius, 2) * Math.Pow(stellarTemp / StellarTables.SolarTemperature, 4);
        }

        public static double GetRadius(StellarClass cls, SpectralType type)
        {
            return StellarTables.GetProperties(type, cls).Radius;
        }

        public static string DescribeStar(StarDetails star)
        {
            return $"{star.Code}: {star.CommonName} star " +
                $"({FormatNumber(star.Temperature, 0)} K, {FormatValue(star.Radius)} solar radii, " +
                $"{FormatValue(star.Mass)} solar masses, {FormatValue(star.Luminosity)} x Sun's luminosity). ";
        }

        public static string DescribeHabitableZone(StellarData data)
        {
            if (data.InnerHZ <= 0)
                return null;

            var who = data.Stars.Count > 1 ? "around the primary " : "";
            return $"Habitable zone {who}{FormatAU(data.InnerHZ)} to {FormatAU(data.OuterHZ)} AU " +
                $"({FormatDistance(data.InnerHZ * StellarTables.AuKm, false)} to {FormatDistance(data.OuterHZ * StellarTables.AuKm, false)}); " +
                $"Earth-equivalent orbit {FormatAU(data.MainWorldOrbitAU)} AU. ";
        }

        public static string DescribeOrbitalPeriod(StellarData data)
        {
            if (data.MeanOrbitalPeriod <= 0)
                return null;

            return $"Orbital period in the habitable zone {FormatPeriod(data.MinOrbitalPeriod)} to {FormatPeriod(data.MaxOrbitalPeriod)} " +
                $"(Earth-equivalent orbit {FormatPeriod(data.MeanOrbitalPeriod)}). ";
        }

        public static string DescribeTidalLocking(StellarData data)
        {
            var primary = data.Primary;
            if (primary == null || data.MainWorldOrbitAU <= 0)
                return null;

            double lockRadius = GetTidalLockRadius(primary.Mass);
            if (data.MainWorldOrbitAU <= lockRadius)
                return "A main world in the habitable zone is very likely tidally locked, with one hemisphere permanently facing the star. ";
            if (data.InnerHZ <= lockRadius)
                return "Worlds near the inner edge of the habitable zone may be tidally locked. ";

            return null;
        }

        public static string DescribeSafeJump(StellarData data)
        {
            var sb = new StringBuilder();

            if (data.Primary != null)
                sb.Append($"Main world assumed at the Earth-equivalent orbit of the primary ({FormatAU(data.MainWorldOrbitAU)} AU). ");

            if (data.MainWorldDiameterKm > 0)
                sb.Append($"World jump shadow (100 x {data.MainWorldDiameterKm:n0} km diameter): {FormatDistance(data.WorldJumpShadowKm)}. ");
            else
                sb.Append("World jump shadow: negligible. ");

            if (data.Primary != null)
                sb.Append($"Primary star jump shadow (100 x {FormatDistance(data.Primary.DiameterKm, false)} diameter): {FormatDistance(data.StarJumpShadowKm)} from the star. ");

            if (data.SafeJumpDistanceKm <= 0)
                sb.Append("Ships may jump immediately. ");
            else
                sb.Append($"Safe jump distance from the main world: {FormatDistance(data.SafeJumpDistanceKm)} " +
                    $"(set by the {(data.JumpLimitedByStar ? "star's" : "world's")} jump shadow). ");

            if (data.Stars.Count > 1)
                sb.Append("Companion star separations are not known; their jump shadows are assumed not to affect the main world. ");

            return sb.ToString();
        }

        public static string GetStellarClassName(StellarClass cls)
        {
            switch (cls)
            {
                case StellarClass.O:
                    return "Hypergiant";
                case StellarClass.Ia:
                    return "Luminous Supergiant";
                case StellarClass.I:
                    return "Supergiant";
                case StellarClass.Ib:
                    return "Less Luminous Supergiant";
                case StellarClass.II:
                    return "Bright Giant";
                case StellarClass.III:
                    return "Giant";
                case StellarClass.IV:
                    return "Subgiant";
                case StellarClass.V:
                    return "Main Sequence (Dwarf)";
                case StellarClass.VI:
                    return "Subdwarf";
                case StellarClass.VII:
                    return "White Dwarf";
                case StellarClass.BD:
                    return "Brown Dwarf";
                default:
                    return "Unknown";
            }
        }

        /// <summary>
        /// Conventional colour names for the Harvard spectral classes.
        /// </summary>
        public static string GetSpectralColorName(SpectralType type)
        {
            switch (StellarTables.SpectralLetter(type))
            {
                case 'O':
                    return "Blue";
                case 'B':
                    return "Blue-White";
                case 'A':
                    return "White";
                case 'F':
                    return "Yellow-White";
                case 'G':
                    return "Yellow";
                case 'K':
                    return "Orange";
                case 'M':
                    return "Red";
                case 'L':
                    return "Dark Red";
                case 'T':
                    return "Magenta";
                default:
                    return null;
            }
        }

        static string GetDefaultColorName(StellarClass cls)
        {
            switch (cls)
            {
                case StellarClass.VII:
                    return "White";
                case StellarClass.BD:
                    return "Dark Red";
                default:
                    return "Unknown";
            }
        }

        /// <summary>
        /// Everyday name for a star, e.g. "Yellow Dwarf", "Red Giant", "Blue Supergiant".
        /// </summary>
        public static string GetCommonName(SpectralType type, StellarClass cls)
        {
            char letter = StellarTables.SpectralLetter(type);
            string color = GetSpectralColorName(type) ?? GetDefaultColorName(cls);

            switch (cls)
            {
                case StellarClass.VII:
                    return "White Dwarf";
                case StellarClass.BD:
                    return "Brown Dwarf";

                case StellarClass.V:
                    if (letter == 'L' || letter == 'T')
                        return "Brown Dwarf";
                    if (letter == 'G' || letter == 'K' || letter == 'M')
                        return $"{color} Dwarf";
                    return $"{color} Main Sequence";

                case StellarClass.VI:
                    if (letter == 'O' || letter == 'B')
                        return "Hot Subdwarf";
                    return $"{color} Subdwarf";

                case StellarClass.O:
                    return $"{color} Hypergiant";
                case StellarClass.Ia:
                case StellarClass.I:
                case StellarClass.Ib:
                    return $"{color} Supergiant";
                case StellarClass.II:
                    return $"{color} Bright Giant";
                case StellarClass.III:
                    return $"{color} Giant";
                case StellarClass.IV:
                    return $"{color} Subgiant";
            }

            return "Unknown";
        }

        /// <summary>
        /// Real-life notes describing the nature of the star.
        /// </summary>
        public static string GetStarNotes(StarDetails star)
        {
            char letter = StellarTables.SpectralLetter(star.SpectralType);

            switch (star.StarClass)
            {
                case StellarClass.O:
                    return "One of the most luminous stars in the galaxy; unstable and shedding mass in violent outbursts. Native life is essentially impossible.";

                case StellarClass.Ia:
                case StellarClass.I:
                case StellarClass.Ib:
                    if (letter == 'K' || letter == 'M')
                        return "A red supergiant: a massive star near the end of its short life, destined to explode as a supernova. Native life is very unlikely.";
                    return "A massive, short-lived supergiant only a few million years old that will end its life as a supernova. Native life is very unlikely.";

                case StellarClass.II:
                    return "A bright giant: a massive evolved star, far more luminous than an ordinary giant. Native life is very unlikely.";

                case StellarClass.III:
                    if (letter == 'O' || letter == 'B')
                        return "A hot, massive giant only a few tens of millions of years old, with intense ultraviolet output.";
                    return "An evolved star that has exhausted the hydrogen in its core and swollen to many times its original size. " +
                        "Its habitable zone has moved outward as it brightened, so worlds there may only recently have thawed.";

                case StellarClass.IV:
                    return "A subgiant: core hydrogen is nearly exhausted and the star is slowly expanding and brightening as it leaves the main sequence.";

                case StellarClass.VI:
                    if (letter == 'O' || letter == 'B')
                        return "A hot subdwarf: the exposed helium-burning core of a star that lost its outer layers. Small, hot and rich in ultraviolet.";
                    return "A metal-poor subdwarf (an old Population II star); its worlds are likely to be poor in heavy elements.";

                case StellarClass.VII:
                    return "The dense, Earth-sized remnant of a dead star. Very dim, with a tiny habitable zone; any surviving worlds endured the star's red giant phase.";

                case StellarClass.BD:
                    return "A substellar object too small to sustain hydrogen fusion; it glows dimly in infrared and has no meaningful habitable zone.";

                case StellarClass.V:
                    {
                        string notes;
                        switch (letter)
                        {
                            case 'O':
                                notes = "An extremely hot, massive star with intense ultraviolet output.";
                                break;
                            case 'B':
                                notes = "A hot, luminous star with strong ultraviolet output; its short life makes native complex life unlikely.";
                                break;
                            case 'A':
                                notes = "A hot white star, often rapidly rotating, with elevated ultraviolet output.";
                                break;
                            case 'F':
                                notes = "Hotter and brighter than the Sun, with somewhat higher ultraviolet output; long-lived enough for life to evolve.";
                                break;
                            case 'G':
                                notes = "A Sun-like star with stable, long-lived output.";
                                break;
                            case 'K':
                                notes = "Cooler and dimmer than the Sun; very stable and long-lived, considered among the most favourable stars for life.";
                                break;
                            case 'M':
                                notes = "A cool, dim red dwarf. The habitable zone lies very close to the star, where worlds are usually tidally locked and exposed to frequent flares.";
                                break;
                            default:
                                return "A brown dwarf (or very low mass star) emitting mostly infrared light; no meaningful habitable zone.";
                        }

                        if (star.Luminosity > 0)
                            notes += $" Estimated main sequence lifetime: {FormatLifetime(10 * star.Mass / star.Luminosity)}.";
                        return notes;
                    }
            }

            return null;
        }

        static string FormatLifetime(double billionsOfYears)
        {
            // Simple M/L scaling underestimates the lifetime of the most massive stars, which is never less than a few million years
            billionsOfYears = Math.Max(billionsOfYears, 0.003);

            if (billionsOfYears >= 1000)
                return "over a trillion years";
            if (billionsOfYears >= 1)
                return $"{FormatValue(billionsOfYears)} billion years";
            return $"{FormatValue(billionsOfYears * 1000)} million years";
        }

        /// <summary>
        /// Format a period given in days, e.g. "3.1 days", "372 days", "17.4 years".
        /// </summary>
        public static string FormatPeriod(double days)
        {
            if (days < 1)
                return $"{days * 24:n1} hours";
            if (days < 10)
                return $"{days:n1} days";
            if (days <= 1000)
                return $"{days:n0} days";
            return $"{days / 365.25:n1} years";
        }

        /// <summary>
        /// Format a distance in km, optionally with the equivalent in AU for large distances.
        /// </summary>
        public static string FormatDistance(double km, bool includeAU = true)
        {
            string text;
            if (km >= 1e9)
                text = $"{km / 1e9:n2} billion km";
            else if (km >= 1e7)
                text = $"{km / 1e6:n1} million km";
            else
                text = $"{km:n0} km";

            double au = km / StellarTables.AuKm;
            if (includeAU && au >= 0.01)
                text += $" ({FormatAU(au)} AU)";

            return text;
        }

        public static string FormatAU(double au) => FormatValue(au);

        /// <summary>
        /// Format a value to approximately 3 significant figures.
        /// </summary>
        public static string FormatValue(double value)
        {
            if (value <= 0 || double.IsNaN(value) || double.IsInfinity(value))
                return "0";

            int decimals = Math.Clamp(2 - (int)Math.Floor(Math.Log10(value)), 0, 6);
            return FormatNumber(value, decimals);
        }

        static string FormatNumber(double value, int decimals) => value.ToString("n" + decimals);
    }
}

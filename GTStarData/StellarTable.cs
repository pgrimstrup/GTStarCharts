using System;
using System.Collections.Generic;

namespace GTStarData
{
    /// <summary>
    /// Basic physical properties of a star.
    /// </summary>
    /// <param name="Temperature">Effective surface temperature in Kelvin.</param>
    /// <param name="Radius">Radius in solar radii.</param>
    /// <param name="Mass">Mass in solar masses.</param>
    public readonly record struct StarProperties(double Temperature, double Radius, double Mass);

    /// <summary>
    /// Real-life approximations of stellar properties by spectral type and luminosity class.
    ///
    /// Sources / approximations:
    ///  * Main sequence (V): Pecaut &amp; Mamajek (2013) "A Modern Mean Dwarf Stellar Color and Effective
    ///    Temperature Sequence" (E. Mamajek's maintained table). L and T entries are typical field brown dwarfs.
    ///  * Giants, bright giants and supergiants: approximate values after Allen's Astrophysical Quantities
    ///    (4th ed.) cross-checked against well studied stars (e.g. Pollux, Arcturus, Aldebaran, Capella,
    ///    Rigel, Deneb, Canopus, Betelgeuse, Antares). These classes have a large natural spread, so the
    ///    values are representative only.
    ///  * Habitable zone: Kopparapu et al. (2014), conservative limits (Runaway Greenhouse to Maximum Greenhouse).
    /// </summary>
    public static class StellarTables
    {
        /// <summary>IAU 2015 nominal solar effective temperature (K).</summary>
        public const double SolarTemperature = 5772;

        /// <summary>IAU 2015 nominal solar radius (km).</summary>
        public const double SolarRadiusKm = 695700;

        /// <summary>IAU 2012 astronomical unit (km).</summary>
        public const double AuKm = 149597870.7;

        /// <summary>Standard gravity (m/s²), used for 1G of starship acceleration.</summary>
        public const double StandardGravity = 9.80665;

        // Kopparapu et al. (2014) coefficients: Seff = S0 + a.T + b.T^2 + c.T^3 + d.T^4, where T = Teff - 5780 K
        static readonly double[] RunawayGreenhouse = { 1.107, 1.332e-4, 1.580e-8, -8.308e-12, -1.931e-15 };
        static readonly double[] MaximumGreenhouse = { 0.356, 6.171e-5, 1.698e-9, -3.198e-12, -5.575e-16 };

        /// <summary>
        /// Earth's relative position within the Sun's conservative habitable zone, measured in stellar flux
        /// (0 = inner edge, 1 = outer edge). Earth receives 1.0 times the solar constant.
        /// </summary>
        public static readonly double EarthHabitableZoneFraction =
            (RunawayGreenhouse[0] - 1.0) / (RunawayGreenhouse[0] - MaximumGreenhouse[0]);

        /// <summary>Effective stellar flux (Earth = 1) at the inner edge of the conservative habitable zone.</summary>
        public static double InnerHabitableZoneFlux(double temperature) => EffectiveFlux(RunawayGreenhouse, temperature);

        /// <summary>Effective stellar flux (Earth = 1) at the outer edge of the conservative habitable zone.</summary>
        public static double OuterHabitableZoneFlux(double temperature) => EffectiveFlux(MaximumGreenhouse, temperature);

        /// <summary>Effective stellar flux at the same relative position in the habitable zone as Earth.</summary>
        public static double EarthEquivalentFlux(double temperature)
        {
            double inner = InnerHabitableZoneFlux(temperature);
            double outer = OuterHabitableZoneFlux(temperature);
            return inner - EarthHabitableZoneFraction * (inner - outer);
        }

        static double EffectiveFlux(double[] k, double temperature)
        {
            // The polynomial fit is only valid between 2,600 K and 7,200 K; clamp outside that range.
            double t = Math.Clamp(temperature, 2600, 7200) - 5780;
            return k[0] + k[1] * t + k[2] * t * t + k[3] * t * t * t + k[4] * t * t * t * t;
        }

        /// <summary>
        /// Returns approximate physical properties for a star of the given spectral type and luminosity class.
        /// </summary>
        public static StarProperties GetProperties(SpectralType type, StellarClass cls)
        {
            switch (cls)
            {
                case StellarClass.V:
                    return GetMainSequence(type);

                case StellarClass.VI:
                    {
                        var ms = GetMainSequence(type);
                        char letter = SpectralLetter(type);
                        if (letter == 'O' || letter == 'B')
                            return new StarProperties(ms.Temperature, 0.2, 0.5); // Hot subdwarf (sdO/sdB): stripped helium-burning core
                        return new StarProperties(ms.Temperature, ms.Radius * 0.7, ms.Mass * 0.7); // Metal-poor cool subdwarf
                    }

                case StellarClass.VII:
                    {
                        // White dwarf: roughly Earth sized, ~0.6 solar masses. Temperature reflects its colour (spectral type) if known.
                        double temp = type == SpectralType.Unknown ? 10000 : Math.Max(4000, GetMainSequence(type).Temperature);
                        return new StarProperties(temp, 0.012, 0.6);
                    }

                case StellarClass.BD:
                    if (type != SpectralType.Unknown && SpectralLetter(type) is 'L' or 'T')
                        return GetMainSequence(type);
                    return new StarProperties(1500, 0.09, 0.05); // Typical field brown dwarf near the L/T transition

                case StellarClass.IV:
                    return Interpolate(SubgiantAnchors, type);

                case StellarClass.III:
                    return Interpolate(GiantAnchors, type);

                case StellarClass.II:
                    return Interpolate(BrightGiantAnchors, type);

                case StellarClass.Ib:
                    return Interpolate(SupergiantIbAnchors, type);

                case StellarClass.Ia:
                    return Interpolate(SupergiantIaAnchors, type);

                case StellarClass.I:
                    {
                        // Unspecified supergiant (or Iab): midway between Ia and Ib
                        var a = Interpolate(SupergiantIaAnchors, type);
                        var b = Interpolate(SupergiantIbAnchors, type);
                        return new StarProperties((a.Temperature + b.Temperature) / 2, Math.Sqrt(a.Radius * b.Radius), Math.Sqrt(a.Mass * b.Mass));
                    }

                case StellarClass.O:
                    {
                        // Hypergiant (luminosity class 0 / Ia+): larger and more massive than a Ia supergiant
                        var a = Interpolate(SupergiantIaAnchors, type);
                        return new StarProperties(a.Temperature, a.Radius * 1.4, a.Mass * 1.2);
                    }
            }

            return default;
        }

        /// <summary>
        /// Spectral letter (O, B, A, F, G, K, M, L, T) for the type, or '\0' if unknown.
        /// </summary>
        public static char SpectralLetter(SpectralType type) =>
            type == SpectralType.Unknown ? '\0' : type.ToString()[0];

        /// <summary>
        /// A continuous numeric scale for spectral types: O0 = 0, B0 = 10, A0 = 20 ... M0 = 60, L0 = 70, T0 = 80.
        /// </summary>
        public static double SpectralCode(SpectralType type)
        {
            if (type == SpectralType.Unknown)
                return double.NaN;

            var name = type.ToString();
            int letter = "OBAFGKMLT".IndexOf(name[0]);
            return letter * 10 + (name[1] - '0');
        }

        static StarProperties GetMainSequence(SpectralType type)
        {
            return MainSequence.TryGetValue(type, out var props) ? props : default;
        }

        static StarProperties Interpolate((SpectralType Type, StarProperties Props)[] anchors, SpectralType type)
        {
            if (type == SpectralType.Unknown)
                return default;

            double code = SpectralCode(type);
            if (code <= SpectralCode(anchors[0].Type))
                return anchors[0].Props;

            for (int i = 0; i < anchors.Length - 1; i++)
            {
                double lo = SpectralCode(anchors[i].Type);
                double hi = SpectralCode(anchors[i + 1].Type);
                if (code >= lo && code <= hi)
                {
                    double f = (code - lo) / (hi - lo);
                    var a = anchors[i].Props;
                    var b = anchors[i + 1].Props;

                    // Temperature varies roughly linearly with subtype; radius and mass vary geometrically.
                    return new StarProperties(
                        a.Temperature + (b.Temperature - a.Temperature) * f,
                        a.Radius * Math.Pow(b.Radius / a.Radius, f),
                        a.Mass * Math.Pow(b.Mass / a.Mass, f));
                }
            }

            return anchors[anchors.Length - 1].Props;
        }

        // Main sequence dwarfs (V): Temperature (K), Radius (solar), Mass (solar)
        static readonly Dictionary<SpectralType, StarProperties> MainSequence = new Dictionary<SpectralType, StarProperties> {
            [SpectralType.O3] = new(44900, 13.43, 59.0),
            [SpectralType.O4] = new(42900, 12.13, 48.0),
            [SpectralType.O5] = new(41400, 11.45, 43.0),
            [SpectralType.O6] = new(38200, 10.71, 35.0),
            [SpectralType.O7] = new(35500, 9.40, 28.0),
            [SpectralType.O8] = new(33400, 8.80, 23.0),
            [SpectralType.O9] = new(32500, 7.70, 19.0),

            [SpectralType.B0] = new(31400, 7.16, 17.7),
            [SpectralType.B1] = new(26000, 5.71, 11.8),
            [SpectralType.B2] = new(20600, 4.06, 7.3),
            [SpectralType.B3] = new(17000, 3.61, 5.4),
            [SpectralType.B4] = new(16400, 3.46, 5.1),
            [SpectralType.B5] = new(15700, 3.36, 4.7),
            [SpectralType.B6] = new(14500, 3.27, 4.3),
            [SpectralType.B7] = new(14000, 2.94, 3.92),
            [SpectralType.B8] = new(12300, 2.86, 3.38),
            [SpectralType.B9] = new(10700, 2.49, 2.75),

            [SpectralType.A0] = new(9700, 2.193, 2.18),
            [SpectralType.A1] = new(9300, 2.136, 2.05),
            [SpectralType.A2] = new(8800, 2.117, 1.98),
            [SpectralType.A3] = new(8600, 1.861, 1.93),
            [SpectralType.A4] = new(8250, 1.794, 1.88),
            [SpectralType.A5] = new(8100, 1.785, 1.86),
            [SpectralType.A6] = new(7910, 1.775, 1.83),
            [SpectralType.A7] = new(7760, 1.750, 1.81),
            [SpectralType.A8] = new(7590, 1.747, 1.77),
            [SpectralType.A9] = new(7400, 1.758, 1.75),

            [SpectralType.F0] = new(7220, 1.728, 1.61),
            [SpectralType.F1] = new(7020, 1.679, 1.50),
            [SpectralType.F2] = new(6820, 1.622, 1.46),
            [SpectralType.F3] = new(6750, 1.578, 1.44),
            [SpectralType.F4] = new(6670, 1.533, 1.38),
            [SpectralType.F5] = new(6550, 1.473, 1.33),
            [SpectralType.F6] = new(6350, 1.359, 1.25),
            [SpectralType.F7] = new(6280, 1.324, 1.21),
            [SpectralType.F8] = new(6180, 1.221, 1.18),
            [SpectralType.F9] = new(6050, 1.167, 1.13),

            [SpectralType.G0] = new(5930, 1.100, 1.06),
            [SpectralType.G1] = new(5860, 1.060, 1.03),
            [SpectralType.G2] = new(5770, 1.012, 1.00),
            [SpectralType.G3] = new(5720, 1.002, 0.99),
            [SpectralType.G4] = new(5680, 0.991, 0.985),
            [SpectralType.G5] = new(5660, 0.977, 0.98),
            [SpectralType.G6] = new(5600, 0.949, 0.97),
            [SpectralType.G7] = new(5550, 0.927, 0.95),
            [SpectralType.G8] = new(5480, 0.914, 0.94),
            [SpectralType.G9] = new(5380, 0.853, 0.90),

            [SpectralType.K0] = new(5270, 0.813, 0.88),
            [SpectralType.K1] = new(5170, 0.797, 0.86),
            [SpectralType.K2] = new(5100, 0.783, 0.82),
            [SpectralType.K3] = new(4830, 0.755, 0.78),
            [SpectralType.K4] = new(4600, 0.713, 0.73),
            [SpectralType.K5] = new(4440, 0.701, 0.70),
            [SpectralType.K6] = new(4300, 0.669, 0.69),
            [SpectralType.K7] = new(4100, 0.630, 0.64),
            [SpectralType.K8] = new(3990, 0.615, 0.62),
            [SpectralType.K9] = new(3930, 0.608, 0.59),

            [SpectralType.M0] = new(3850, 0.588, 0.57),
            [SpectralType.M1] = new(3660, 0.501, 0.50),
            [SpectralType.M2] = new(3560, 0.446, 0.44),
            [SpectralType.M3] = new(3430, 0.361, 0.37),
            [SpectralType.M4] = new(3210, 0.274, 0.23),
            [SpectralType.M5] = new(3060, 0.196, 0.162),
            [SpectralType.M6] = new(2810, 0.137, 0.102),
            [SpectralType.M7] = new(2680, 0.120, 0.090),
            [SpectralType.M8] = new(2570, 0.114, 0.085),
            [SpectralType.M9] = new(2380, 0.102, 0.079),

            // L and T dwarfs are mostly brown dwarfs; radius stays close to Jupiter's (~0.1 solar) while they cool.
            [SpectralType.L0] = new(2250, 0.100, 0.077),
            [SpectralType.L1] = new(2100, 0.098, 0.075),
            [SpectralType.L2] = new(1960, 0.096, 0.072),
            [SpectralType.L3] = new(1830, 0.094, 0.070),
            [SpectralType.L4] = new(1700, 0.092, 0.068),
            [SpectralType.L5] = new(1590, 0.090, 0.065),
            [SpectralType.L6] = new(1490, 0.089, 0.062),
            [SpectralType.L7] = new(1410, 0.088, 0.060),
            [SpectralType.L8] = new(1350, 0.088, 0.058),
            [SpectralType.L9] = new(1300, 0.088, 0.055),

            [SpectralType.T2] = new(1200, 0.088, 0.050),
            [SpectralType.T3] = new(1180, 0.088, 0.048),
            [SpectralType.T4] = new(1160, 0.088, 0.045),
            [SpectralType.T5] = new(1100, 0.088, 0.042),
            [SpectralType.T6] = new(1000, 0.088, 0.040),
            [SpectralType.T7] = new(900, 0.088, 0.035),
            [SpectralType.T8] = new(750, 0.088, 0.030),
        };

        // Subgiants (IV)
        static readonly (SpectralType, StarProperties)[] SubgiantAnchors =
        {
            (SpectralType.O5, new(40000, 12.0, 45.0)),
            (SpectralType.B0, new(30000, 7.0, 17.0)),
            (SpectralType.B5, new(15500, 4.0, 5.5)),
            (SpectralType.A0, new(9900, 3.0, 3.0)),
            (SpectralType.A5, new(8100, 2.5, 2.2)),
            (SpectralType.F0, new(7150, 2.3, 1.9)),
            (SpectralType.F5, new(6500, 2.2, 1.6)),
            (SpectralType.G0, new(5900, 2.1, 1.4)),
            (SpectralType.G5, new(5450, 2.4, 1.2)),
            (SpectralType.K0, new(5000, 3.0, 1.2)),
            (SpectralType.K5, new(4300, 4.0, 1.1)),
            (SpectralType.M0, new(3900, 5.0, 1.0)),
            (SpectralType.M5, new(3400, 7.0, 1.0)),
            (SpectralType.M9, new(3000, 9.0, 1.0)),
        };

        // Giants (III)
        static readonly (SpectralType, StarProperties)[] GiantAnchors =
        {
            (SpectralType.O5, new(39000, 14.0, 45.0)),
            (SpectralType.B0, new(29000, 9.0, 18.0)),
            (SpectralType.B5, new(15000, 6.0, 6.0)),
            (SpectralType.A0, new(10000, 5.0, 4.0)),
            (SpectralType.A5, new(8100, 4.0, 2.5)),
            (SpectralType.F0, new(7150, 4.0, 2.2)),
            (SpectralType.F5, new(6500, 4.5, 1.8)),
            (SpectralType.G0, new(5800, 7.0, 2.0)),
            (SpectralType.G5, new(5150, 10.0, 1.8)),
            (SpectralType.K0, new(4750, 11.0, 1.6)),
            (SpectralType.K5, new(3950, 40.0, 1.2)),
            (SpectralType.M0, new(3850, 50.0, 1.2)),
            (SpectralType.M5, new(3400, 110.0, 1.2)),
            (SpectralType.M9, new(2900, 200.0, 1.0)),
        };

        // Bright giants (II)
        static readonly (SpectralType, StarProperties)[] BrightGiantAnchors =
        {
            (SpectralType.O5, new(39000, 15.0, 50.0)),
            (SpectralType.B0, new(28000, 15.0, 20.0)),
            (SpectralType.B5, new(14000, 20.0, 9.0)),
            (SpectralType.A0, new(9700, 25.0, 7.0)),
            (SpectralType.A5, new(8100, 28.0, 6.0)),
            (SpectralType.F0, new(7100, 30.0, 6.0)),
            (SpectralType.F5, new(6400, 32.0, 6.0)),
            (SpectralType.G0, new(5600, 35.0, 6.0)),
            (SpectralType.G5, new(5000, 40.0, 6.0)),
            (SpectralType.K0, new(4600, 50.0, 6.0)),
            (SpectralType.K5, new(4000, 90.0, 7.0)),
            (SpectralType.M0, new(3700, 150.0, 7.0)),
            (SpectralType.M5, new(3400, 250.0, 8.0)),
            (SpectralType.M9, new(3000, 350.0, 8.0)),
        };

        // Less luminous supergiants (Ib)
        static readonly (SpectralType, StarProperties)[] SupergiantIbAnchors =
        {
            (SpectralType.O5, new(37000, 16.0, 50.0)),
            (SpectralType.B0, new(26000, 22.0, 25.0)),
            (SpectralType.B5, new(14000, 35.0, 15.0)),
            (SpectralType.A0, new(9700, 60.0, 12.0)),
            (SpectralType.A5, new(8400, 70.0, 10.0)),
            (SpectralType.F0, new(7400, 70.0, 9.0)),
            (SpectralType.F5, new(6600, 80.0, 8.0)),
            (SpectralType.G0, new(5600, 75.0, 7.0)),
            (SpectralType.G5, new(5000, 100.0, 8.0)),
            (SpectralType.K0, new(4500, 150.0, 9.0)),
            (SpectralType.K5, new(3900, 350.0, 11.0)),
            (SpectralType.M0, new(3700, 450.0, 12.0)),
            (SpectralType.M5, new(3400, 700.0, 13.0)),
            (SpectralType.M9, new(3000, 900.0, 13.0)),
        };

        // Luminous supergiants (Ia)
        static readonly (SpectralType, StarProperties)[] SupergiantIaAnchors =
        {
            (SpectralType.O5, new(38000, 20.0, 60.0)),
            (SpectralType.B0, new(27000, 32.0, 40.0)),
            (SpectralType.B5, new(14000, 60.0, 25.0)),
            (SpectralType.A0, new(9700, 130.0, 20.0)),
            (SpectralType.A5, new(8500, 180.0, 18.0)),
            (SpectralType.F0, new(7700, 200.0, 16.0)),
            (SpectralType.F5, new(6900, 250.0, 15.0)),
            (SpectralType.G0, new(5550, 350.0, 16.0)),
            (SpectralType.G5, new(4850, 450.0, 16.0)),
            (SpectralType.K0, new(4420, 550.0, 17.0)),
            (SpectralType.K5, new(3850, 750.0, 17.0)),
            (SpectralType.M0, new(3650, 900.0, 18.0)),
            (SpectralType.M5, new(3300, 1300.0, 20.0)),
            (SpectralType.M9, new(3000, 1600.0, 20.0)),
        };
    }
}

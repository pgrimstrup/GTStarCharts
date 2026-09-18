using System;
using System.Collections.Generic;
using System.Text;

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
        O,   // Hypergiants
        I,   // Super Giant
        II,  // Bright Giant
        III, // Giant
        IV,  // Subgiant
        V,    // Main Sequence (Dwarf)
        VI,   // Subdwarf
        VII   // White Dwarf
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


    public class StellarData
    {
        public StellarConfiguration Configuration { get; set; }
        public StellarClass StarClass { get; set; }
        public SpectralType SpectralType { get; set; }
        public StellarClass StarClass2 { get; set; }
        public SpectralType SpectralType2 { get; set; }
        public StellarClass StarClass3 { get; set; }
        public SpectralType SpectralType3 { get; set; }

        public double StellarTemp { get; set; }
        public double StellarRadius { get; set; }
        public double Luminosity { get; set; }
        public string StellarDescription { get; set; }
        public string HabitableZoneDescription { get; set; }
        public string OrbitalPeriod { get; set; }
        public string SafeJumpWarning { get; set; }
        public string SafeJumpWarning2 { get; set; }

        public double InnerHZ { get; set; }
        public double OuterHZ { get; set; }
        public double MinOrbitalPeriod { get; set; }
        public double MaxOrbitalPeriod { get; set; }
        public double MeanOrbitalPeriod { get; set; }
        public double SafeJumpDistance { get; set; }
    }

    public class StellarDataFactory
    {
        const double StefanBoltzmannConstant = 5.670367e-8;
        const double SolarTemp = 5800;
        const double SolarRadiusInKilometers = 432170 * 1.6;
        const double AuInKilometers = 9.2956e+7 * 1.6;

        static readonly double[] StellarTemp = { 0,
            53000, 49000, 45000, 41000, 38000, 35000, 32000, // O3-O9
            29200,24000,21000,17600,16000,15200,14300,13500,12300,11400, // B0-B9
            10000,9330,9040,8750,8480,8300,8100,7850,7650, 7500, // A0-A9
            7350,7200,7050,6850,6750,6700,6550,6400,6300,6150, // F0-F9
            6050,5930,5800,5760,5710,5660,5580,5510,5440,5340, //G0-G9
            5240,5110,4960,4800,4600,4400,4200,4000,3870,3800, //K0-K9
            3750,3700,3600,3500,3400,3200,3100,2900,2800,2700, //M0-M9
            2600,2400,2300,2200,2100,2000,1900,1800,1600,1500, //L0-L9
            1400,1300,1200,1100,1000,900,800 //T2-T8
        };

        public static StellarData Create(string stellarInfo)
        {
            StellarData data = new StellarData();

            if (string.IsNullOrWhiteSpace(stellarInfo))
                return data;

            var bits = stellarInfo.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i + 1 < bits.Length; i += 2)
            {
                if (Enum.TryParse(bits[i], out SpectralType type) && Enum.TryParse(bits[i + 1], out StellarClass cls))
                {
                    switch (i)
                    {
                        case 0:
                            data.Configuration = StellarConfiguration.Single;
                            data.StarClass = cls;
                            data.SpectralType = type;
                            GetStellarDetails(data);
                            break;

                        case 2:
                            data.Configuration = StellarConfiguration.Binary;
                            data.StarClass2 = cls;
                            data.SpectralType2 = type;
                            break;

                        case 4:
                            data.Configuration = StellarConfiguration.Trinary;
                            data.StarClass3 = cls;
                            data.SpectralType3 = type;
                            break;
                    }
                }
            }

            switch (data.Configuration)
            {
                case StellarConfiguration.Single:
                    data.StellarDescription = DescribeStar(data.StarClass, data.SpectralType, data.StellarRadius);
                    data.HabitableZoneDescription = DescribeHabitableZone(data);
                    data.OrbitalPeriod = DescribeOrbitalPeriod(data);
                    break;

                case StellarConfiguration.Binary:
                    data.StellarDescription = $"Binary star system. Primary {DescribeStar(data.StarClass, data.SpectralType, data.StellarRadius)}Secondary {DescribeStar(data.StarClass2, data.SpectralType2)}";
                    data.HabitableZoneDescription = DescribeHabitableZone(data);
                    data.OrbitalPeriod = DescribeOrbitalPeriod(data);
                    break;

                case StellarConfiguration.Trinary:
                    data.StellarDescription = $"Trinary star system. Primary {DescribeStar(data.StarClass, data.SpectralType, data.StellarRadius)}Secondary {DescribeStar(data.StarClass2, data.SpectralType2)}Tertiary {DescribeStar(data.StarClass3, data.SpectralType3)}";
                    data.HabitableZoneDescription = DescribeHabitableZone(data);
                    data.OrbitalPeriod = DescribeOrbitalPeriod(data);
                    break;

                default:
                    data.StellarDescription = $"Unknown stellar configuration [{stellarInfo}]";
                    break;

            }

            double safejump = (data.SafeJumpDistance * SolarRadiusInKilometers);
            double extra = safejump - (data.InnerHZ * AuInKilometers);
            if (extra > 0)
            {
                data.SafeJumpWarning = $"WARNING: Minimum safe jump distance is {safejump:n0} km from primary star, {extra:n0} km from planetary surface. ";
                data.SafeJumpWarning2 = $"Travel Time at 2G={TimeToTravel(extra, 2)}, 4G={TimeToTravel(extra, 4)}, 6G={TimeToTravel(extra, 6)}. ";
            }


            return data;
        }

        public static string TimeToTravel(double distanceInKilometers, int acceleration)
        {
            var time = TimeSpan.FromSeconds(2 * Math.Sqrt((200 * distanceInKilometers) / acceleration));

            if (time.TotalDays > 1.5)
                return $"{time.TotalDays:n0} days, {time.Hours} hours";
            if (time.TotalHours > 1.5)
                return $"{time.TotalHours:n0} hours, {time.Minutes} minutes";
            if (time.TotalMinutes > 1.5)
                return $"{time.TotalMinutes:n0} minutes, {time.Seconds} seconds";

            return $"{time.TotalSeconds:n0} seconds";

        }

        public static void GetStellarDetails(StellarData data)
        {
            data.StellarTemp = GetStarTemp(data.SpectralType);
            data.StellarRadius = GetRadius(data.StarClass, data.SpectralType);
            data.Luminosity = GetLuminosity(data.StellarRadius, data.StellarTemp);
            data.InnerHZ = Math.Sqrt(data.Luminosity / 1.1);
            data.OuterHZ = Math.Sqrt(data.Luminosity / 0.53);
            data.MinOrbitalPeriod = GetOribitalPeriod(data.InnerHZ);
            data.MaxOrbitalPeriod = GetOribitalPeriod(data.OuterHZ);
            data.MeanOrbitalPeriod = GetOribitalPeriod(Math.Sqrt(data.Luminosity));
            data.SafeJumpDistance = 200 * data.StellarRadius;
        }

        private static double GetOribitalPeriod(double radius)
        {
            return 116.18 * Math.PI * radius;
        }

        private static double GetLuminosity(double stellarRadius, double stellarTemp)
        {
            double lumins = Math.Pow(stellarRadius, 2) * Math.Pow(stellarTemp / SolarTemp, 4);
            return lumins;
        }

        private static double GetStarTemp(SpectralType spectralType)
        {
            int index = (int)spectralType;
            if (index < 0 || index >= StellarTemp.Length)
                return 0;

            return StellarTemp[index];
        }

        public static double GetRadius(StellarClass cls, SpectralType type)
        {
            if (cls == StellarClass.VII)
                return GetRadius(StellarClass.V, type) * 0.5;

            if (cls == StellarClass.VI)
                return GetRadius(StellarClass.V, type) * 0.75;

            if (cls == StellarClass.V)
            {
                if (type >= SpectralType.O3 && type <= SpectralType.O9)
                    return (int)(SpectralType.O9 - type) * (15 - 6.7) / 7 + 6.7;

                if (type >= SpectralType.B0 && type <= SpectralType.B9)
                    return (int)(SpectralType.B9 - type) * (6.7 - 2.7) / 10 + 2.7;

                if (type >= SpectralType.A0 && type <= SpectralType.A9)
                    return (int)(SpectralType.A9 - type) * (2.4 - 1.55) / 10 + 1.55;

                if (type >= SpectralType.F0 && type <= SpectralType.F9)
                    return (int)(SpectralType.F9 - type) * (1.5 - 1.15) / 10 + 1.15;

                if (type >= SpectralType.G0 && type <= SpectralType.G9)
                    return (int)(SpectralType.G9 - type) * (1.1 - 0.86) / 10 + 0.86;

                if (type >= SpectralType.K0 && type <= SpectralType.K9)
                    return (int)(SpectralType.K9 - type) * (0.85 - 0.62) / 10 + 0.62;

                if (type >= SpectralType.M0 && type <= SpectralType.M9)
                    return (int)(SpectralType.M9 - type) * (0.6 - 0.15) / 10 + 0.15;

                if (type >= SpectralType.L0 && type <= SpectralType.L9)
                    return (int)(SpectralType.L9 - type) * (0.14 - 0.085) / 10 + 0.085;

                if (type >= SpectralType.T3 && type <= SpectralType.T8)
                    return (int)(SpectralType.T8 - type) * (0.08 - 0.06) / 5 + 0.08;
            }

            if (cls == StellarClass.IV)
                return GetRadius(StellarClass.V, type) * 1.5;

            if (cls == StellarClass.III)
                return GetRadius(StellarClass.V, type) * 2;

            if (cls == StellarClass.II)
                return GetRadius(StellarClass.V, type) * 3;

            if (cls == StellarClass.I)
                return GetRadius(StellarClass.V, type) * 4;


            return 0;
        }


        public static string DescribeStar(StellarClass cls, SpectralType type, double radius = 0)
        {
            if (radius <= 0)
                return $"{GetSpectralColorName(type)} {GetStellarClassName(cls)} star. ";

            if (radius > 10)
                return $"{GetSpectralColorName(type)} {GetStellarClassName(cls)} star ({radius:n0} solar radius). ";
            if (radius > 1)
                return $"{GetSpectralColorName(type)} {GetStellarClassName(cls)} star ({radius:n1} solar radius). ";
            if (radius > 0.1)
                return $"{GetSpectralColorName(type)} {GetStellarClassName(cls)} star ({radius:n2} solar radius). ";
            if (radius > 0.01)
                return $"{GetSpectralColorName(type)} {GetStellarClassName(cls)} star ({radius:n3} solar radius). ";
            if (radius > 0.001)
                return $"{GetSpectralColorName(type)} {GetStellarClassName(cls)} star ({radius:n4} solar radius). ";

            return $"{GetSpectralColorName(type)} {GetStellarClassName(cls)} star ({radius:n5} solar radius). ";
        }

        public static string DescribeHabitableZone(StellarData data)
        {
            if (data.InnerHZ > 10)
                return $"Habitable Zone {data.InnerHZ:n0} to {data.OuterHZ:n0} AU. ";
            if (data.InnerHZ > 1)
                return $"Habitable Zone {data.InnerHZ:n1} to {data.OuterHZ:n1} AU. ";
            if (data.InnerHZ > 0.1)
                return $"Habitable Zone {data.InnerHZ:n2} to {data.OuterHZ:n2} AU. ";
            if (data.InnerHZ > 0.01)
                return $"Habitable Zone {data.InnerHZ:n3} to {data.OuterHZ:n3} AU. ";
            if (data.InnerHZ > 0.001)
                return $"Habitable Zone {data.InnerHZ:n4} to {data.OuterHZ:n4} AU. ";

            return $"Habitable Zone {data.InnerHZ:n5} to {data.OuterHZ:n5} AU. ";
        }

        public static string DescribeOrbitalPeriod(StellarData data)
        {
            if (data.MeanOrbitalPeriod < 10)
            {
                return $"Orbital Period of {data.MinOrbitalPeriod:n1} to {data.MaxOrbitalPeriod:n1} days (mean= {data.MeanOrbitalPeriod:n1} days). Main planet is tidally locked. ";
            }
            else if (data.MeanOrbitalPeriod > 400)
            {
                return $"Orbital Period of {data.MinOrbitalPeriod:n0} to {data.MaxOrbitalPeriod:n0} days / {data.MinOrbitalPeriod / 365:n1} to {data.MaxOrbitalPeriod / 365:n1} years (mean= {data.MeanOrbitalPeriod:n0} days / {data.MeanOrbitalPeriod / 365:n1} years). ";
            }
            else
            {
                return $"Orbital Period of {data.MinOrbitalPeriod:n0} to {data.MaxOrbitalPeriod:n0} days (mean= {data.MeanOrbitalPeriod:n0} days). ";
            }
        }

        public static string GetStellarClassName(StellarClass cls)
        {
            switch (cls)
            {
                case StellarClass.O: return "Hypergiant";
                case StellarClass.I: return "Supergiant";
                case StellarClass.II: return "Bright Giant";
                case StellarClass.III: return "Giant";
                case StellarClass.IV: return "Subgiant";
                case StellarClass.V: return "Dwarf (Main Sequence)";
                case StellarClass.VI: return "Subdwarf";
                case StellarClass.VII: return "White Dwarf";
                default: return "Unknown";
            }
        }

        public static string GetSpectralColorName(SpectralType type)
        {
            if (type >= SpectralType.O3 && type <= SpectralType.O9)
                return "Blue";

            if (type >= SpectralType.B0 && type <= SpectralType.B9)
                return "Blue/White";

            if (type >= SpectralType.A0 && type <= SpectralType.A8)
                return "White";

            if (type >= SpectralType.F0 && type <= SpectralType.F8)
                return "Yellow/White";

            if (type >= SpectralType.G0 && type <= SpectralType.G8)
                return "Yellow";

            if (type >= SpectralType.K0 && type <= SpectralType.K8)
                return "Pale Orange";

            if (type >= SpectralType.M0 && type <= SpectralType.M8)
                return "Orange/Red";

            if (type >= SpectralType.L0 && type <= SpectralType.L8)
                return "Brown";

            if (type >= SpectralType.T3 && type <= SpectralType.T8)
                return "Brown";

            return null;
        }
    }
}

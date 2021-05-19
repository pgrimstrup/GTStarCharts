using GTStarData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GTStarCharts.Rules
{
    public static class FarTrader
    {
        public static void CalculateTradeFactors(this SystemData system, TextLookup lookup)
        {
            double uwtn = GetTLModifier(system) + GetPopulationModifier(system);
            double wtn = uwtn + GetPortModifier(system, uwtn);

        }

        private static double GetPortModifier(SystemData system, double uwtn)
        {
            int index = ((int)Math.Truncate(uwtn)).Min(0).Max(7);

            double[] lookup;
            switch (system.UWP.Subcode(UWP.Starport))
            {
                case "A": lookup = new double[] { 1.5, 1, 1, 0.5, 0.5, 0, 0, 0 }; break;
                case "B": lookup = new double[] { 1, 1, 0.5, 0.5, 0, 0, -0.5, -1 }; break;
                case "C": lookup = new double[] { 1, 0.5, 0.5, 0, 0, -0.5, -1, -1.5 }; break;
                case "D": lookup = new double[] { 0.5, 0.5, 0, 0, -0.5, -1, -1.5, -2 }; break;
                case "E": lookup = new double[] { 0.5, 0, 0, -0.5, -1, -1.5, -2, -2.5 }; break;
                default: lookup = new double[] { 0, 0, -2.5, -3, -3.5, -4, -4.5, -5 }; break;
            }

            return lookup[index];
        }

        private static double GetPopulationModifier(SystemData system)
        {
            return system.UWP.Subcode(UWP.Population).ToInt() * 0.5;
        }

        private static double GetTLModifier(SystemData system)
        {
            switch (system.UWP.Subcode(UWP.TechLevel).ToInt())
            {
                case 0:
                case 1:
                case 2: return -0.5;
                case 3:
                case 4:
                case 5: return 0;
                case 6:
                case 7:
                case 8: return 0.5;
                case 9:
                case 10:
                case 11: return 1;
                case 12:
                case 13: return 1.5;
                default: return 2.0;
            }
        }
    }
}

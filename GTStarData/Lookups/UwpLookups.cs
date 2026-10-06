using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace GTStarData.Lookups
{
    public static class UwpLookups
    {
        public static readonly HashSet<TextType> StaticTypes = new HashSet<TextType>
        {
            TextType.Starport,
            TextType.PlanetSize,
            TextType.Atmosphere,
            TextType.Hydrosphere,
            TextType.Population,
            TextType.Government,
            TextType.LawLevel,
            TextType.TechLevel,
            TextType.Allegiance,
            TextType.Importance,
            TextType.TradeCode,
            TextType.Ecomomics,
            TextType.Culture
        };

        public static bool IsStaticType(TextType type) => StaticTypes.Contains(type);

        public static TextLookup GetLookup(TextType type, string code)
        {
            var cleanCode = code?.Trim() ?? string.Empty;
            var lookup = new TextLookup
            {
                Id = Guid.Empty,
                TextType = type,
                Code = code
            };

            switch (type)
            {
                case TextType.Starport:
                    if (Starports.TryGetValue(cleanCode, out var sp))
                    {
                        lookup.ShortText = sp.ShortText;
                        lookup.LongText = sp.LongText;
                    }
                    break;

                case TextType.PlanetSize:
                    if (PlanetSizes.TryGetValue(cleanCode, out var ps))
                    {
                        lookup.ShortText = ps.ShortText;
                        lookup.LongText = ps.LongText;
                    }
                    break;

                case TextType.Atmosphere:
                    if (Atmospheres.TryGetValue(cleanCode, out var at))
                    {
                        lookup.ShortText = at.ShortText;
                        lookup.LongText = at.LongText;
                    }
                    break;

                case TextType.Hydrosphere:
                    if (Hydrospheres.TryGetValue(cleanCode, out var hy))
                    {
                        lookup.ShortText = hy.ShortText;
                        lookup.LongText = hy.LongText;
                    }
                    break;

                case TextType.Population:
                    if (Populations.TryGetValue(cleanCode, out var po))
                    {
                        lookup.ShortText = po.ShortText;
                        lookup.LongText = po.LongText;
                    }
                    break;

                case TextType.Government:
                    if (Governments.TryGetValue(cleanCode, out var go))
                    {
                        lookup.ShortText = go.ShortText;
                        lookup.LongText = go.LongText;
                    }
                    break;

                case TextType.LawLevel:
                    if (LawLevels.TryGetValue(cleanCode, out var ll))
                    {
                        lookup.ShortText = ll.ShortText;
                        lookup.LongText = ll.LongText;
                    }
                    break;

                case TextType.TechLevel:
                    if (TechLevels.TryGetValue(cleanCode, out var tl))
                    {
                        lookup.ShortText = tl.ShortText;
                        lookup.LongText = tl.LongText;
                    }
                    break;

                case TextType.Allegiance:
                    if (Allegiances.TryGetValue(cleanCode, out var al))
                    {
                        lookup.ShortText = al.ShortText;
                        lookup.LongText = al.LongText;
                    }
                    else if (!string.IsNullOrEmpty(cleanCode))
                    {
                        lookup.ShortText = cleanCode;
                    }
                    break;

                case TextType.Importance:
                    if (Importances.TryGetValue(cleanCode, out var im))
                    {
                        lookup.ShortText = im.ShortText;
                        lookup.LongText = im.LongText;
                    }
                    else if (int.TryParse(cleanCode, out var ixVal))
                    {
                        lookup.ShortText = $"Importance {ixVal}";
                    }
                    break;

                case TextType.TradeCode:
                    if (TradeCodes.TryGetValue(cleanCode, out var tc))
                    {
                        lookup.ShortText = tc.ShortText;
                        lookup.LongText = tc.LongText;
                    }
                    break;

                case TextType.Ecomomics:
                    GetEconomicsLookup(cleanCode, lookup);
                    break;

                case TextType.Culture:
                    GetCultureLookup(cleanCode, lookup);
                    break;
            }

            return lookup;
        }

        private static void GetEconomicsLookup(string code, TextLookup lookup)
        {
            if (string.IsNullOrEmpty(code)) return;

            // Pattern: (RLI+E) or (RLI-E) or RLI+E
            var m = Regex.Match(code.Trim(), @"\(?([0-9A-Z])([0-9A-Z])([0-9A-Z])([+-]\d+)\)?", RegexOptions.IgnoreCase);
            if (m.Success)
            {
                var r = m.Groups[1].Value.ToUpperInvariant();
                var l = m.Groups[2].Value.ToUpperInvariant();
                var i = m.Groups[3].Value.ToUpperInvariant();
                var e = m.Groups[4].Value;

                var rText = EconomicResources.TryGetValue(r, out var rt) ? rt : $"Resource {r}";
                var lText = EconomicLabor.TryGetValue(l, out var lt) ? lt : $"Labor {l}";
                var iText = EconomicInfrastructure.TryGetValue(i, out var it) ? it : $"Infrastructure {i}";
                var eText = $"Efficiency {e}";

                lookup.ShortText = $"{rText}, {lText}, {iText}, {eText}";
                lookup.LongText = $"Resources: {rText}. Labor: {lText}. Infrastructure: {iText}. Efficiency: {eText}.";
            }
            else if (EconomicResources.TryGetValue(code, out var desc))
            {
                lookup.ShortText = desc;
            }
            else
            {
                lookup.ShortText = code;
            }
        }

        private static void GetCultureLookup(string code, TextLookup lookup)
        {
            if (string.IsNullOrEmpty(code)) return;

            // Pattern: [HASS] or HASS (4 eHex digits)
            var m = Regex.Match(code.Trim(), @"\[?([0-9A-Z])([0-9A-Z])([0-9A-Z])([0-9A-Z])\]?", RegexOptions.IgnoreCase);
            if (m.Success)
            {
                var h = m.Groups[1].Value.ToUpperInvariant();
                var a = m.Groups[2].Value.ToUpperInvariant();
                var s = m.Groups[3].Value.ToUpperInvariant();
                var y = m.Groups[4].Value.ToUpperInvariant();

                var hText = CulturalHeterogeneity.TryGetValue(h, out var ht) ? ht : $"Heterogeneity {h}";
                var aText = CulturalAcceptance.TryGetValue(a, out var at) ? at : $"Acceptance {a}";
                var sText = CulturalStrangeness.TryGetValue(s, out var st) ? st : $"Strangeness {s}";
                var yText = CulturalSymbols.TryGetValue(y, out var yt) ? yt : $"Symbols {y}";

                lookup.ShortText = $"{hText}, {aText}, {sText}, {yText}";
                lookup.LongText = $"Heterogeneity: {hText}. Acceptance: {aText}. Strangeness: {stText(sText)}. Symbols: {yText}.";
            }
            else
            {
                lookup.ShortText = code;
            }

            static string stText(string s) => s;
        }

        #region Lookup Dictionaries

        // 1. Starport (Mongoose 2E / T5)
        public static readonly Dictionary<string, (string ShortText, string LongText)> Starports =
            new Dictionary<string, (string ShortText, string LongText)>(StringComparer.OrdinalIgnoreCase)
        {
            { "A", ("Excellent Quality", "Excellent quality installation. Refined fuel available. Annual maintenance overhaul available. Shipyard capable of constructing starships and non-starships present. Naval base and/or scout base may be present.") },
            { "B", ("Good Quality", "Good quality installation. Refined fuel available. Annual maintenance overhaul available. Shipyard capable of constructing non-starships present. Naval base and/or scout base may be present.") },
            { "C", ("Routine Quality", "Routine quality installation. Only unrefined fuel available. Reasonable repair facilities present. Scout base may be present.") },
            { "D", ("Poor Quality", "Poor quality installation. Only unrefined fuel available. No repair facilities present. Scout base may be present.") },
            { "E", ("Frontier Installation", "Frontier installation. Essentially a marked spot of bedrock with no fuel, facilities, or bases present.") },
            { "X", ("No Starport", "No starport. No provision is made for any ship landings.") },
            { "F", ("Good Spaceport", "Good spaceport. Minor system with orbital or surface port facilities capable of handling interplanetary craft.") },
            { "G", ("Poor Spaceport", "Poor spaceport. Minimal spaceport facility.") },
            { "H", ("Primitive Spaceport", "Primitive spaceport. Landing strip with minimal navigation aid.") },
            { "Y", ("No Spaceport", "No spaceport. Bare terrain with no facilities.") },
            { "?", ("Unknown", "Starport quality unknown.") }
        };

        // 2. Planet Size (Mongoose 2E / T5)
        public static readonly Dictionary<string, (string ShortText, string LongText)> PlanetSizes =
            new Dictionary<string, (string ShortText, string LongText)>(StringComparer.OrdinalIgnoreCase)
        {
            { "0", ("Asteroid Belt", "Asteroid belt or orbital complex with negligible diameter and microgravity (negligible G).") },
            { "1", ("1,600 km (0.05-0.09G)", "Diameter 1,600 km (1,000 miles). Surface gravity approx 0.05 to 0.09G. Escape velocity 1.35 km/s.") },
            { "2", ("3,200 km (0.10-0.17G)", "Diameter 3,200 km (2,000 miles). Surface gravity approx 0.10 to 0.17G (e.g. Luna, Triton).") },
            { "3", ("4,800 km (0.24-0.34G)", "Diameter 4,800 km (3,000 miles). Surface gravity approx 0.24 to 0.34G (e.g. Mercury, Ganymede).") },
            { "4", ("6,400 km (0.32-0.46G)", "Diameter 6,400 km (4,000 miles). Surface gravity approx 0.32 to 0.46G (e.g. Mars).") },
            { "5", ("8,000 km (0.40-0.57G)", "Diameter 8,000 km (5,000 miles). Surface gravity approx 0.40 to 0.57G.") },
            { "6", ("9,600 km (0.60-0.70G)", "Diameter 9,600 km (6,000 miles). Surface gravity approx 0.60 to 0.70G.") },
            { "7", ("11,200 km (0.70-0.80G)", "Diameter 11,200 km (7,000 miles). Surface gravity approx 0.70 to 0.80G.") },
            { "8", ("12,800 km (0.90-1.00G)", "Diameter 12,800 km (8,000 miles). Surface gravity approx 0.90 to 1.00G (Earth size).") },
            { "9", ("14,400 km (1.00-1.15G)", "Diameter 14,400 km (9,000 miles). Surface gravity approx 1.00 to 1.15G.") },
            { "A", ("16,000 km (1.15-1.25G)", "Diameter 16,000 km (10,000 miles). Surface gravity approx 1.15 to 1.25G.") },
            { "B", ("17,600 km (1.25-1.40G)", "Diameter 17,600 km (11,000 miles). Surface gravity approx 1.25 to 1.40G.") },
            { "C", ("19,200 km (1.35-1.50G)", "Diameter 19,200 km (12,000 miles). Surface gravity approx 1.35 to 1.50G.") },
            { "D", ("20,800 km (1.45-1.65G)", "Diameter 20,800 km (13,000 miles). Surface gravity approx 1.45 to 1.65G.") },
            { "E", ("22,400 km (1.55-1.75G)", "Diameter 22,400 km (14,000 miles). Surface gravity approx 1.55 to 1.75G.") },
            { "F", ("24,000 km (1.65-1.90G)", "Diameter 24,000 km (15,000 miles). Surface gravity approx 1.65 to 1.90G.") },
            { "?", ("Unknown", "Planet size unknown.") }
        };

        // 3. Atmosphere (Mongoose 2E / T5)
        public static readonly Dictionary<string, (string ShortText, string LongText)> Atmospheres =
            new Dictionary<string, (string ShortText, string LongText)>(StringComparer.OrdinalIgnoreCase)
        {
            { "0", ("No Atmosphere", "No atmosphere (vacuum, pressure 0.00). Requires vacc suit.") },
            { "1", ("Trace", "Trace atmosphere (pressure 0.001 to 0.09 atm). Requires vacc suit.") },
            { "2", ("Very Thin, Tainted", "Very thin tainted atmosphere (pressure 0.10 to 0.42 atm). Requires combination respirator and filter mask.") },
            { "3", ("Very Thin", "Very thin atmosphere (pressure 0.10 to 0.42 atm). Requires respirator.") },
            { "4", ("Thin, Tainted", "Thin tainted atmosphere (pressure 0.43 to 0.70 atm). Requires filter mask.") },
            { "5", ("Thin", "Thin breathable atmosphere (pressure 0.43 to 0.70 atm). Breathable.") },
            { "6", ("Standard", "Standard breathable atmosphere (pressure 0.71 to 1.49 atm). Breathable, shirt-sleeve environment.") },
            { "7", ("Standard, Tainted", "Standard tainted atmosphere (pressure 0.71 to 1.49 atm). Requires filter mask.") },
            { "8", ("Dense", "Dense breathable atmosphere (pressure 1.50 to 2.49 atm). Breathable.") },
            { "9", ("Dense, Tainted", "Dense tainted atmosphere (pressure 1.50 to 2.49 atm). Requires filter mask.") },
            { "A", ("Exotic", "Exotic atmosphere (no unbonded oxygen). Requires oxygen tanks and mask, or protective suit depending on toxicity.") },
            { "B", ("Corrosive", "Corrosive atmosphere (highly reactive chemical compounds). Requires full protective suit / HEV.") },
            { "C", ("Insidious", "Insidious atmosphere (caustic substances penetrate protective suits). Requires specialized hostile environment gear.") },
            { "D", ("Dense, High", "Dense high atmosphere. Breathable above a minimum altitude; sea level pressure is crushing or toxic.") },
            { "E", ("Thin, Low", "Thin low atmosphere. Breathable only in deep valleys and chasms; elsewhere unbreathable.") },
            { "F", ("Unusual", "Unusual atmosphere (elliptical orbits, unusual composition, variable conditions).") },
            { "?", ("Unknown", "Atmosphere unknown.") }
        };

        // 4. Hydrosphere (Mongoose 2E / T5)
        public static readonly Dictionary<string, (string ShortText, string LongText)> Hydrospheres =
            new Dictionary<string, (string ShortText, string LongText)>(StringComparer.OrdinalIgnoreCase)
        {
            { "0", ("0% Water (Desert World)", "Desert world. 0% to 5% surface free water or liquid.") },
            { "1", ("10% Water", "10% surface water (6% to 15%). Very dry world.") },
            { "2", ("20% Water", "20% surface water (16% to 25%). Dry world.") },
            { "3", ("30% Water", "30% surface water (26% to 35%). Small seas and scattered lakes.") },
            { "4", ("40% Water", "40% surface water (36% to 45%). Small seas and continents.") },
            { "5", ("50% Water", "50% surface water (46% to 55%). Balanced water and land area.") },
            { "6", ("60% Water", "60% surface water (56% to 65%). Large seas and oceans.") },
            { "7", ("70% Water (Earth-like)", "70% surface water (66% to 75%). Earth-like ocean coverage.") },
            { "8", ("80% Water", "80% surface water (76% to 85%). Extensive oceans, island continents.") },
            { "9", ("90% Water", "90% surface water (86% to 95%). World ocean with small archipelagoes.") },
            { "A", ("100% Water (Water World)", "100% surface water (96% to 100%). Water world with no major dry land masses.") },
            { "?", ("Unknown", "Hydrosphere percentage unknown.") }
        };

        // 5. Population (Mongoose 2E / T5)
        public static readonly Dictionary<string, (string ShortText, string LongText)> Populations =
            new Dictionary<string, (string ShortText, string LongText)>(StringComparer.OrdinalIgnoreCase)
        {
            { "0", ("Unpopulated", "Unpopulated world (0 inhabitants).") },
            { "1", ("Tens", "Tens of inhabitants (1 to 99 people).") },
            { "2", ("Hundreds", "Hundreds of inhabitants (100 to 999 people).") },
            { "3", ("Thousands", "Thousands of inhabitants (1,000 to 9,999 people).") },
            { "4", ("Tens of Thousands", "Tens of thousands of inhabitants (10,000 to 99,999 people).") },
            { "5", ("Hundreds of Thousands", "Hundreds of thousands of inhabitants (100,000 to 999,999 people).") },
            { "6", ("Millions", "Millions of inhabitants (1,000,000 to 9,999,999 people).") },
            { "7", ("Tens of Millions", "Tens of millions of inhabitants (10,000,000 to 99,999,999 people).") },
            { "8", ("Hundreds of Millions", "Hundreds of millions of inhabitants (100,000,000 to 999,999,999 people).") },
            { "9", ("Billions", "Billions of inhabitants (1,000,000,000 to 9,999,999,999 people).") },
            { "A", ("Tens of Billions", "Tens of billions of inhabitants (10,000,000,000 to 99,999,999,999 people).") },
            { "B", ("Hundreds of Billions", "Hundreds of billions of inhabitants (100,000,000,000 to 999,999,999,999 people).") },
            { "C", ("Trillions", "Trillions of inhabitants (1,000,000,000,000+ people).") },
            { "D", ("Tens of Trillions", "Tens of trillions of inhabitants.") },
            { "E", ("Hundreds of Trillions", "Hundreds of trillions of inhabitants.") },
            { "F", ("Quadrillions", "Quadrillions of inhabitants.") },
            { "?", ("Unknown", "Population unknown.") }
        };

        // 6. Government (Mongoose 2E / T5)
        public static readonly Dictionary<string, (string ShortText, string LongText)> Governments =
            new Dictionary<string, (string ShortText, string LongText)>(StringComparer.OrdinalIgnoreCase)
        {
            { "0", ("None / Anarchy", "No government structure. In family, clan, or individual anarchy.") },
            { "1", ("Company / Corporation", "Corporate rule. The company owns the world or colony and directs operations.") },
            { "2", ("Participating Democracy", "Participating democracy. Government by advice and consent of the entire citizen body.") },
            { "3", ("Self-Perpetuating Oligarchy", "Self-perpetuating oligarchy. Government by a restricted minority with autonomous succession.") },
            { "4", ("Representative Democracy", "Representative democracy. Government by elected representatives of the populace.") },
            { "5", ("Feudal Technocracy", "Feudal technocracy. Government by specific technological guilds or technical elite.") },
            { "6", ("Captive Government / Colony", "Captive government / colony. Subordinate to an outside power or mother world.") },
            { "7", ("Balkanization", "Balkanized. Multiple competing, independent governments on world.") },
            { "8", ("Civil Service Bureaucracy", "Civil service bureaucracy. Government by professional civil servants and departments.") },
            { "9", ("Impersonal Bureaucracy", "Impersonal bureaucracy. Government by isolated officials and complex administrative rules.") },
            { "A", ("Charismatic Dictator", "Charismatic dictator. Government by a single leader with massive popular devotion.") },
            { "B", ("Non-Charismatic Dictator", "Non-charismatic leader. Government by a military dictator or autocrat without popular support.") },
            { "C", ("Charismatic Oligarchy", "Charismatic oligarchy. Government by a small ruling coalition of popular leaders.") },
            { "D", ("Religious Dictatorship", "Religious dictatorship. Government by a religious hierarchy enforcing dogma.") },
            { "E", ("Religious Autocracy", "Religious autocracy. Government by an individual divine ruler or ecclesiastical head.") },
            { "F", ("Totalitarian Oligarchy", "Totalitarian oligarchy. All facets of society are strictly controlled by the ruling party.") },
            { "G", ("Small Station / Facility (Aslan)", "Small station or facility. Clan outpost or observation post.") },
            { "H", ("Split Clan Control (Aslan)", "Split clan control. Multiple Aslan clans share or contest territory.") },
            { "J", ("Single On-World Clan Control (Aslan)", "Single on-world clan control.") },
            { "K", ("Single Multi-World Clan Control (Aslan)", "Single multi-world clan control.") },
            { "L", ("Major Clan Control (Aslan)", "Major Aslan clan control.") },
            { "M", ("Vassal Clan Control (Aslan)", "Vassal clan control.") },
            { "N", ("Major Vassal Clan Control (Aslan)", "Major vassal clan control.") },
            { "P", ("Small Station or Facility (K'kree)", "Small station or military facility.") },
            { "Q", ("Off-World Steppelord Rule (K'kree)", "Krurruna or Krumanak rule for off-world steppelord.") },
            { "R", ("Steppelord On-World Rule (K'kree)", "Steppelord on-world rule.") },
            { "S", ("Sept (Hiver)", "Sept government structure.") },
            { "T", ("Unsupervised Anarchy (Hiver)", "Unsupervised anarchy.") },
            { "U", ("Supervised Anarchy (Hiver)", "Supervised anarchy.") },
            { "W", ("Committee (Hiver)", "Committee government.") },
            { "X", ("Droyne Hierarchy", "Droyne caste hierarchy.") },
            { "?", ("Unknown", "Government type unknown.") }
        };

        // 7. Law Level (Mongoose 2E / T5)
        public static readonly Dictionary<string, (string ShortText, string LongText)> LawLevels =
            new Dictionary<string, (string ShortText, string LongText)>(StringComparer.OrdinalIgnoreCase)
        {
            { "0", ("No Prohibitions", "No legal restrictions or prohibitions. Anything goes.") },
            { "1", ("Poison gas, explosives, body pistols banned", "Poison gas, explosives, and body pistols prohibited. Energy and military weapons permitted.") },
            { "2", ("Portable energy weapons banned", "Portable energy weapons (lasers, plasma) and high-power explosives prohibited.") },
            { "3", ("Heavy weapons banned", "Heavy weapons, machine guns, and automatic rifles prohibited.") },
            { "4", ("Light assault weapons banned", "Light assault weapons, SMGs, and full-automatic small arms prohibited.") },
            { "5", ("Personal concealable weapons banned", "Personal concealable weapons (pistols, revolvers) prohibited outside home or business.") },
            { "6", ("All firearms except shotguns banned", "All firearms except hunting shotguns and rifles banned. Carrying openly prohibited.") },
            { "7", ("Shotguns and hunting firearms banned", "Shotguns and hunting long arms prohibited. Only archaic or sporting gear permitted with license.") },
            { "8", ("Bladed weapons controlled", "Long bladed weapons controlled; open possession of swords and large blades prohibited.") },
            { "9", ("Weapon possession outside home banned", "Weapon possession outside private home strictly prohibited. Routine checks.") },
            { "A", ("All weapons prohibited", "Weapon possession completely prohibited anywhere on world.") },
            { "B", ("Rigid civilian movement control", "Rigid control of civilian movement. Passports, curfews, checkpoints required.") },
            { "C", ("Unrestricted invasion of privacy", "Unrestricted invasion of privacy. Warrants not required for search or seizure.") },
            { "D", ("Paramilitary law enforcement", "Paramilitary law enforcement. Armed patrols and military garrisons maintain order.") },
            { "E", ("Police state", "Full-fledged police state. Severe restrictions on travel, speech, and assembly.") },
            { "F", ("All facets of daily life controlled", "All facets of daily life regularly legislated, monitored, and rigidly controlled.") },
            { "G", ("Severe punishment for petty infractions", "Severe punishment for petty infractions. High surveillance and Draconian penalties.") },
            { "H", ("Legalized oppressive practices", "Legalized oppressive practices. Slavery, forced labor, or arbitrary detention.") },
            { "J", ("Routinely oppressive and restrictive", "Routinely oppressive and restrictive society.") },
            { "K", ("Excessively oppressive and restrictive", "Excessively oppressive and restrictive regime.") },
            { "L", ("Totally oppressive and restrictive", "Totally oppressive and restrictive system. Total subjugation.") },
            { "S", ("Special / Variable Situation", "Special or variable legal situation.") },
            { "?", ("Unknown", "Law level unknown.") }
        };

        // 8. Tech Level (Mongoose 2E / T5)
        public static readonly Dictionary<string, (string ShortText, string LongText)> TechLevels =
            new Dictionary<string, (string ShortText, string LongText)>(StringComparer.OrdinalIgnoreCase)
        {
            { "0", ("TL 0 - Stone Age", "Stone Age. Primitive tools, fire, stone, wood, bone; no metalworking.") },
            { "1", ("TL 1 - Bronze / Iron Age", "Bronze Age to Middle Ages (circa 3500 BC to 1400 AD). Metalworking, agriculture, sail.") },
            { "2", ("TL 2 - Renaissance", "Renaissance / Age of Sail (circa 1400 to 1700). Printing press, optics, early firearms, navigation.") },
            { "3", ("TL 3 - Industrial Revolution", "Basic science and early Industrial Revolution (circa 1700 to 1860). Steam power, telegraph, rifling.") },
            { "4", ("TL 4 - Mechanized Age", "Mechanized Age (circa 1860 to 1900). Internal combustion, electrification, ironclads, telephone.") },
            { "5", ("TL 5 - Broadcast Age", "Broadcast Age (circa 1900 to 1939). Radio, aircraft, mass production, early armor.") },
            { "6", ("TL 6 - Nuclear Age", "Nuclear Age (circa 1940 to 1969). Fission power, jet aircraft, rocketry, early electronics.") },
            { "7", ("TL 7 - Space Age", "Space Age (circa 1970 to 1979). Transistors, miniaturized electronics, interplanetary probes, orbiters.") },
            { "8", ("TL 8 - Information Age", "Information Age (circa 1980 to 2000). Microcomputers, global networks, personal computers, primitive bio-tech.") },
            { "9", ("TL 9 - Early Stellar", "Early Stellar (interplanetary development). Fusion power, gravitics basics, off-world colonies.") },
            { "A", ("TL 10 - Early Interstellar", "Early Interstellar. Jump-1 drive, Maneuver drive, laser carbines, orbital habitats.") },
            { "B", ("TL 11 - Average Interstellar", "Average Interstellar. Jump-2, combat armor, neural computers, grav vehicles.") },
            { "C", ("TL 12 - Average Imperial", "Average Imperial standard. Jump-3, plasma weapons, advanced cloning and cybernetics.") },
            { "D", ("TL 13 - Above Average Imperial", "Above Average Imperial. Jump-4, battle dress, fusion rifles, sophisticated antimatter research.") },
            { "E", ("TL 14 - Advanced Imperial", "Advanced Imperial. Jump-5, advanced fusion, meson weapons, artificial neural links.") },
            { "F", ("TL 15 - Imperial Maximum", "Technical Imperial Maximum. Jump-6, black globe generators, disintegrators, advanced psionics.") },
            { "G", ("TL 16 - Advanced Robots", "Post-Imperial development. Autonomous robotics, advanced matter alteration.") },
            { "H", ("TL 17 - Artificial Intelligence", "True sentient artificial intelligence, advanced antimatter manipulation.") },
            { "J", ("TL 18 - Personal Disintegrators", "Personal disintegrators, advanced gravitic transmutation.") },
            { "K", ("TL 19 - Plastic Metals", "Plastic metals, reality engineering.") },
            { "L", ("TL 20 - Technological Magic", "Comprehensible only as technological magic; manipulation of fundamental cosmic forces.") },
            { "?", ("Unknown", "Technological level unknown.") }
        };

        // 9. Allegiance (OTU Major Polities, T5SS Codes, Classic / MegaTraveller / TNE codes)
        public static readonly Dictionary<string, (string ShortText, string LongText)> Allegiances =
            new Dictionary<string, (string ShortText, string LongText)>(StringComparer.OrdinalIgnoreCase)
        {
            // Core Major Polities
            { "Im", ("Third Imperium", "The Third Imperium, a vast feudal empire of over 11,000 systems spanning 28 sectors.") },
            { "ImAp", ("Third Imperium, Amec Protectorate", "Third Imperium, Amec Protectorate.") },
            { "ImDa", ("Third Imperium, Domain of Antares", "Third Imperium, Domain of Antares under Archduke Brzk.") },
            { "ImDc", ("Third Imperium, Domain of Sylea", "Third Imperium, Domain of Sylea.") },
            { "ImDd", ("Third Imperium, Domain of Deneb", "Third Imperium, Domain of Deneb under Duke Norris.") },
            { "ImDg", ("Third Imperium, Domain of Gateway", "Third Imperium, Domain of Gateway.") },
            { "ImDi", ("Third Imperium, Domain of Ilelish", "Third Imperium, Domain of Ilelish under Archduke Dulinor.") },
            { "ImDs", ("Third Imperium, Domain of Sol", "Third Imperium, Domain of Sol.") },
            { "ImDv", ("Third Imperium, Domain of Vland", "Third Imperium, Domain of Vland.") },
            { "ImLa", ("Third Imperium, League of Antares", "Third Imperium, League of Antares.") },
            { "ImLu", ("Third Imperium, Lucan's Imperium", "Third Imperium loyal to Emperor Lucan.") },
            { "ImSy", ("Third Imperium, Sylean Worlds", "Third Imperium, Sylean Worlds.") },
            { "ImVl", ("Third Imperium, Vilani Domains", "Third Imperium, Vilani Domains.") },

            // Zhodani Consulate
            { "Zh", ("Zhodani Consulate", "Zhodani Consulate. Psionic human major power governed by the Nobility and Intendant class.") },
            { "ZhIN", ("Zhodani Consulate, Iadr Nsobl Province", "Zhodani Consulate, Iadr Nsobl Province in the Spinward Marches rimward frontier.") },
            { "ZhJp", ("Zhodani Consulate, Jadlhri Province", "Zhodani Consulate, Jadlhri Province.") },
            { "ZhOb", ("Zhodani Consulate, Obrefri Province", "Zhodani Consulate, Obrefri Province.") },
            { "ZhSh", ("Zhodani Consulate, Shtail Province", "Zhodani Consulate, Shtail Province.") },

            // Solomani Confederation
            { "So", ("Solomani Confederation", "Solomani Confederation. Democratic human state advocating Solomani racial primacy.") },
            { "SoCf", ("Solomani Confederation", "Solomani Confederation.") },
            { "SoFr", ("Solomani Confederation, Free Republic", "Solomani Confederation, Free Republic.") },
            { "SoKl", ("Solomani Confederation, Kolan Union", "Solomani Confederation, Kolan Union.") },

            // Aslan Hierate
            { "As", ("Aslan Hierate", "Aslan Hierate. Loose confederation of feline warrior clans governed by the Tlaukhu council.") },
            { "AsMw", ("Aslan Hierate, Clan Dominated", "Aslan Hierate, single multiple-world clan dominates.") },
            { "AsSc", ("Aslan Hierate, Split Clans", "Aslan Hierate, multiple clans split control.") },
            { "AsSF", ("Aslan Hierate, Facility", "Aslan Hierate, small facility or outpost.") },
            { "AsOf", ("Oleaiy'fte", "Aslan Hierate, Oleaiy'fte clan.") },
            { "AsIf", ("Iyeaao'fte", "Aslan Hierate, Iyeaao'fte clan.") },

            // Vargr Extents
            { "Va", ("Vargr Extents", "Vargr Extents. Collection of canine sophont nation-states, packs, and temporary coalitions.") },
            { "VaEx", ("Vargr Extents", "Vargr Extents.") },
            { "VDeP", ("Dzarrghhr Puen", "Dzarrghhr Puen (Vargr pack coalition).") },
            { "VEnA", ("Enclave of Antares", "Enclave of Antares (Vargr).") },
            { "VIng", ("Infinity League", "Infinity League (Vargr).") },
            { "VLoA", ("Logaksu Alliance", "Logaksu Alliance (Vargr).") },
            { "VNoG", ("Nation of Gryg", "Nation of Gryg (Vargr).") },
            { "VOpp", ("Opposition Alliance", "Opposition Alliance (Vargr).") },
            { "VRoP", ("Roriseine Pack", "Roriseine Pack (Vargr).") },
            { "VSeK", ("Sakhag Alliance", "Sakhag Alliance (Vargr).") },
            { "VThE", ("Thek'khor Enclave", "Thek'khor Enclave (Vargr).") },
            { "VTzE", ("Tozue' Vargr Enclave", "Tozue' Vargr Enclave.") },
            { "VUFa", ("United Followers of Augur", "United Followers of Augur.") },

            // Other Major Powers
            { "Kk", ("Two Thousand Worlds (K'kree)", "Two Thousand Worlds. Centauroid militantly herbivorous empire.") },
            { "Hv", ("Hive Federation", "Hive Federation. Egalitarian multispecies state led by the Hiver race.") },
            { "Dr", ("Droyne Worlds", "Droyne homeworlds and enclaves.") },

            // Minor Races and Independent Powers
            { "Da", ("Darrian Confederation", "Darrian Confederation. Solitary high-tech civilization in the Spinward Marches.") },
            { "DaCf", ("Darrian Confederation", "Darrian Confederation.") },
            { "Sw", ("Sword Worlds Confederation", "Sword Worlds Confederation. Germanic-heritage human colonies in the Spinward Marches.") },
            { "SwCf", ("Sword Worlds Confederation", "Sword Worlds Confederation.") },
            { "Fl", ("Florian League", "Florian League. Peaceful cooperative state in the Trojan Reach.") },
            { "FlLe", ("Florian League", "Florian League.") },
            { "DrFt", ("Droyne Worlds", "Droyne Worlds.") },
            { "GeAn", ("Gethan Annex", "Gethan Annex.") },
            { "GlEm", ("Glorious Empire", "Glorious Empire (Aslan breakaway slave empire).") },
            { "GrCo", ("Greater Colchis", "Greater Colchis.") },
            { "LDeA", ("League of Deneb and Antares", "League of Deneb and Antares.") },
            { "LuWl", ("Lunion Shield Worlds", "Lunion Shield Worlds.") },
            { "Na", ("Non-Aligned", "Non-Aligned world without formal imperial or confederate allegiance.") },
            { "NaHu", ("Non-Aligned, Human-dominated", "Non-Aligned, Human-dominated world.") },
            { "NaAs", ("Non-Aligned, Aslan-dominated", "Non-Aligned, Aslan-dominated world.") },
            { "NaVa", ("Non-Aligned, Vargr-dominated", "Non-Aligned, Vargr-dominated world.") },
            { "NaDr", ("Non-Aligned, Droyne-dominated", "Non-Aligned, Droyne-dominated world.") },
            { "NaXX", ("Non-Aligned, Unclaimed", "Non-Aligned, unclaimed or wilderness world.") },
            { "Cs", ("Client State", "Client state of a major interstellar empire.") },
            { "CsIm", ("Client State, Third Imperium", "Client state of the Third Imperium.") },
            { "CsZh", ("Client State, Zhodani Consulate", "Client state of the Zhodani Consulate.") },
            { "CsSo", ("Client State, Solomani Confederation", "Client state of the Solomani Confederation.") },
            { "CsAs", ("Client State, Aslan Hierate", "Client state of the Aslan Hierate.") },
            { "CsVa", ("Client State, Vargr", "Client state of a Vargr power.") },
            { "CsHv", ("Client State, Hive Federation", "Client state of the Hive Federation.") },

            // Rebellion / MegaTraveller Factions
            { "Dd", ("Domain of Deneb", "Domain of Deneb (MegaTraveller Rebellion era).") },
            { "Fd", ("Federation of Daibei", "Federation of Daibei (MegaTraveller Rebellion era).") },
            { "Fi", ("Federation of Ilelish", "Federation of Ilelish (MegaTraveller Rebellion era).") },
            { "La", ("League of Antares", "League of Antares (MegaTraveller Rebellion era).") },
            { "Li", ("Lucan's Imperium", "Lucan's Imperium (MegaTraveller Rebellion era).") },
            { "Ma", ("Margaret's Stronghold", "Margaret's Stronghold (MegaTraveller Rebellion era).") },
            { "Rv", ("Restored Vilani Empire", "Restored Vilani Empire / Ziru Sirka (MegaTraveller era).") },
            { "St", ("Strephon's Imperium", "Strephon's Imperium (MegaTraveller Rebellion era).") },
            { "Wi", ("Wilds", "The Wilds (Traveller: The New Era post-collapse systems).") },
            { "--", ("Unclaimed", "Empty or unclaimed star system.") },
            { "??", ("Unknown", "Allegiance unknown.") }
        };

        // 10. Importance ({ Ix } modifier, T5 / TravellerMap)
        public static readonly Dictionary<string, (string ShortText, string LongText)> Importances =
            new Dictionary<string, (string ShortText, string LongText)>(StringComparer.OrdinalIgnoreCase)
        {
            { "-3", ("Very Unimportant (-3)", "Extremely unimportant system with negligible interstellar trade, low population, and poor infrastructure.") },
            { "-2", ("Very Unimportant (-2)", "Very unimportant system with low economic activity and sparse traffic.") },
            { "-1", ("Unimportant (-1)", "Unimportant system off the main trade corridors.") },
            { "0", ("Ordinary (0)", "Ordinary star system of average regional importance.") },
            { "1", ("Notable (+1)", "Notable world with significant regional trade, population, or military presence.") },
            { "2", ("Significant (+2)", "Significant star system serving as a regional crossroad or manufacturing hub.") },
            { "3", ("Important (+3)", "Important star system with high economic output, capital world, or primary subsector hub.") },
            { "4", ("Very Important (+4)", "Very important sector capital, regional industrial nexus, or primary naval base.") },
            { "5", ("Extremely Important (+5)", "Extremely important premier system, domain or Imperial capital, core economic engine.") }
        };

        // 11. Trade Codes (Mongoose 2E / T5 canon)
        public static readonly Dictionary<string, (string ShortText, string LongText)> TradeCodes =
            new Dictionary<string, (string ShortText, string LongText)>(StringComparer.OrdinalIgnoreCase)
        {
            // Standard Planetary & Economic
            { "Ag", ("Agricultural", "Agricultural world. Produces abundant foodstuffs and organic raw materials for export.") },
            { "As", ("Asteroid", "Asteroid belt or cluster. High mineral yield, microgravity industry and mining.") },
            { "Ba", ("Barren", "Barren world. Unpopulated with no developed civilization or technology.") },
            { "De", ("Desert", "Desert world. Arid terrain, water is scarce and strictly managed.") },
            { "Di", ("Dieback", "Dieback world. Former high technology civilization collapsed; remnants or ghost cities.") },
            { "Fa", ("Farming", "Farming world dedicated to agricultural cultivation.") },
            { "Fl", ("Fluid Oceans", "Fluid oceans. Surface seas consist of hydrocarbons, ammonia, or exotic liquids.") },
            { "Ga", ("Garden", "Garden world. Earth-like environment, ideal climate and ecology for human habitation.") },
            { "He", ("Hellworld", "Hellworld. Hostile environment with toxic atmosphere, extremes of heat, cold, or volcanism.") },
            { "Hi", ("High Population", "High population world. Billions of inhabitants, massive market for imports.") },
            { "Ht", ("High Tech", "High technology world. Produces cutting-edge starships, weapons, electronics, and medical tech.") },
            { "Ic", ("Ice-Capped", "Ice-capped world. Glaciers and frozen poles cover much of the surface.") },
            { "In", ("Industrial", "Industrialized world. Massive manufacturing, refineries, heavy equipment, and vehicle exports.") },
            { "Lo", ("Low Population", "Low population world. Few inhabitants, minimal market, pioneer frontier.") },
            { "Lt", ("Low Tech", "Low technology world. Pre-industrial or primitive civilization lacking modern tech.") },
            { "Mi", ("Mining", "Mining world. Extensive excavation of ores, gems, radioactives, and rare minerals.") },
            { "Mr", ("Military Rule", "Military rule. Under military governor or marshal administration.") },
            { "Na", ("Non-Agricultural", "Non-agricultural world. Cannot produce sufficient food; dependent on food imports.") },
            { "Ni", ("Non-Industrial", "Non-industrial world. Minimal local manufacturing; dependent on manufactured imports.") },
            { "Oc", ("Ocean World", "Ocean world. Vast global seas covering almost the entire planetary surface.") },
            { "Pa", ("Pre-Agricultural", "Pre-agricultural world. Developing agriculture and biosphere potential.") },
            { "Pe", ("Penal Colony", "Penal colony. Facility for incarcerating or exiling convicts.") },
            { "Ph", ("Pre-High Population", "Pre-high population world. Rapidly growing population nearing high-pop threshold.") },
            { "Pi", ("Pre-Industrial", "Pre-industrial world. Developing mechanized production and heavy industry.") },
            { "Po", ("Poor", "Poor world. Low natural resources and harsh living conditions.") },
            { "Pr", ("Pre-Rich", "Pre-rich world. Strong economic fundamentals poised for high prosperity.") },
            { "Px", ("Prison World", "Prison or exile world operated by governmental or military authorities.") },
            { "Re", ("Reserve", "Preserve or nature reserve. Protected environment restricted from development.") },
            { "Ri", ("Rich", "Rich world. Wealthy market, high standard of living, valuable exports.") },
            { "Sa", ("Satellite", "Satellite. Main world is a moon orbiting a gas giant or large planet.") },
            { "Tr", ("Tropic", "Tropic world with warm climate and dense equatorial vegetation.") },
            { "Tu", ("Tundra", "Tundra world with cold climate and subarctic vegetation.") },
            { "Tz", ("Twilight Zone", "Twilight zone world. Tidally locked to its parent star with habitable terminator.") },
            { "Va", ("Vacuum", "Vacuum world. No atmosphere, vacc suits required at all times.") },
            { "Wa", ("Water World", "Water world. Completely covered in liquid water.") },

            // Political & Strategic
            { "Cp", ("Subsector Capital", "Subsector capital. Seat of the subsector duke or regional government.") },
            { "Cs", ("Sector Capital", "Sector capital. Seat of the archduke or sector government.") },
            { "Cx", ("Capital", "Capital world of an interstellar empire or polity.") },
            { "Cy", ("Colony", "Colony world dependent on a parent star system.") },
            { "Fo", ("Forbidden", "Forbidden system (Red Zone). Interdicted by Imperial or regional Navy.") },
            { "Pz", ("Puzzle", "Puzzle system (Amber Zone). Travellers are urged to exercise extreme caution.") },
            { "Da", ("Danger", "Danger zone (Amber Zone). Heightened hazard from lawlessness, plague, or conflict.") },
            { "Ab", ("Data Repository", "Data repository world or ancient library installation.") },
            { "An", ("Ancient Site", "Ancient site. Contains ruins, artifacts, or technology of the Ancients.") },
            { "Rs", ("Research Station", "Imperial or corporate scientific research station.") },
            { "RsA", ("Research Station Alpha", "Imperial Research Station Alpha.") },
            { "RsB", ("Research Station Beta", "Imperial Research Station Beta.") },
            { "RsG", ("Research Station Gamma", "Imperial Research Station Gamma.") },
            { "RsD", ("Research Station Delta", "Imperial Research Station Delta.") },
            { "RsE", ("Research Station Epsilon", "Imperial Research Station Epsilon.") },
            { "Xb", ("Xboat Station", "Express boat (Xboat) communication hub with tender and relay facilities.") },
            { "Ex", ("Exile Camp", "Exile camp or detention colony.") }
        };

        // 12. Economic Extension Components (T5SS (RLI+E))
        public static readonly Dictionary<string, string> EconomicResources =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "0", "Very Scarce Resources" },
            { "1", "Very Scarce Resources" },
            { "2", "Scarce Resources" },
            { "3", "Low Resources" },
            { "4", "Below Average Resources" },
            { "5", "Moderate Resources" },
            { "6", "Average Resources" },
            { "7", "Good Resources" },
            { "8", "Abundant Resources" },
            { "9", "Very Abundant Resources" },
            { "A", "Extremely Abundant Resources" },
            { "B", "Rich Resources" },
            { "C", "Very Rich Resources" },
            { "D", "Vast Resources" },
            { "E", "Near-Limitless Resources" },
            { "F", "Exceptional Resources" },
            { "G", "Exceptional Resources" },
            { "H", "Phenomenal Resources" },
            { "J", "Unbounded Resources" }
        };

        public static readonly Dictionary<string, string> EconomicLabor =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "0", "Negligible Workforce" },
            { "1", "Dozens in Workforce" },
            { "2", "Hundreds in Workforce" },
            { "3", "Thousands in Workforce" },
            { "4", "Tens of Thousands in Workforce" },
            { "5", "Hundreds of Thousands in Workforce" },
            { "6", "Millions in Workforce" },
            { "7", "Tens of Millions in Workforce" },
            { "8", "Hundreds of Millions in Workforce" },
            { "9", "Billions in Workforce" },
            { "A", "Tens of Billions in Workforce" },
            { "B", "Hundreds of Billions in Workforce" },
            { "C", "Trillions in Workforce" }
        };

        public static readonly Dictionary<string, string> EconomicInfrastructure =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "0", "Non-Existent Infrastructure" },
            { "1", "Extremely Primitive Infrastructure" },
            { "2", "Primitive Infrastructure" },
            { "3", "Basic Infrastructure" },
            { "4", "Limited Infrastructure" },
            { "5", "Developing Infrastructure" },
            { "6", "Standard Infrastructure" },
            { "7", "Above Average Infrastructure" },
            { "8", "Advanced Infrastructure" },
            { "9", "Comprehensive Infrastructure" },
            { "A", "Extensive Planetary Infrastructure" },
            { "B", "Highly Integrated Infrastructure" },
            { "C", "Superior Orbital & Surface Infrastructure" },
            { "D", "State-of-the-Art Infrastructure" },
            { "E", "Master-Planned Sector Infrastructure" },
            { "F", "Domain-Level Infrastructure" },
            { "G", "Imperial Maximum Infrastructure" },
            { "H", "Transcendent Infrastructure" }
        };

        // 13. Cultural Extension Components (T5SS [HASS])
        public static readonly Dictionary<string, string> CulturalHeterogeneity =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "0", "Unpopulated" },
            { "1", "Monolithic Culture" },
            { "2", "Harmonious Culture" },
            { "3", "Unified Culture" },
            { "4", "Homogeneous Culture" },
            { "5", "Predominant Mainstream Culture" },
            { "6", "Moderate Diversity Culture" },
            { "7", "Diverse Culture" },
            { "8", "Multi-Cultural Society" },
            { "9", "Pluralistic Society" },
            { "A", "Polarized Society" },
            { "B", "Fragmented Culture" },
            { "C", "Contentious Multi-Faction Society" },
            { "D", "Tribal Factional Culture" },
            { "E", "Sectarian Cultural Schism" },
            { "F", "Chaotic Cultural Diversity" },
            { "G", "Completely Fragmented Culture" }
        };

        public static readonly Dictionary<string, string> CulturalAcceptance =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "0", "Unpopulated" },
            { "1", "Extremely Xenophobic" },
            { "2", "Very Xenophobic" },
            { "3", "Xenophobic" },
            { "4", "Distrustful of Outsiders" },
            { "5", "Wary of Off-Worlders" },
            { "6", "Cautiously Tolerant" },
            { "7", "Tolerant" },
            { "8", "Accepting" },
            { "9", "Friendly to Strangers" },
            { "A", "Welcoming" },
            { "B", "Very Welcoming" },
            { "C", "Xenophilic" },
            { "D", "Very Xenophilic" },
            { "E", "Highly Philanthropic" },
            { "F", "Extremely Xenophilic" }
        };

        public static readonly Dictionary<string, string> CulturalStrangeness =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "0", "Unpopulated" },
            { "1", "Very Typical / Familiar" },
            { "2", "Typical" },
            { "3", "Familiar Everyday Custom" },
            { "4", "Mildly Unusual" },
            { "5", "Noticeably Distinct Customs" },
            { "6", "Distinct Local Quirks" },
            { "7", "Unusual Customs" },
            { "8", "Very Unusual Customs" },
            { "9", "Exotic Customs" },
            { "A", "Bizarre Customs" },
            { "B", "Profoundly Alien Concepts" },
            { "C", "Radically Alien Customs" },
            { "D", "Incomprehensible Mindset" }
        };

        public static readonly Dictionary<string, string> CulturalSymbols =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "0", "Unpopulated / Extremely Concrete" },
            { "1", "Literal / Concrete Symbols" },
            { "2", "Direct Visual Iconography" },
            { "3", "Pragmatic Signage" },
            { "4", "Representational Symbols" },
            { "5", "Traditional Symbolism" },
            { "6", "Nuanced Cultural Imagery" },
            { "7", "Abstract Symbology" },
            { "8", "Rich Metaphorical Art" },
            { "9", "Subtle Symbolic Codes" },
            { "A", "Dense Esoteric Metaphors" },
            { "B", "Cryptic Symbolism" },
            { "C", "Complex Abstract Iconography" },
            { "D", "Highly Surrealist Symbolism" },
            { "E", "Deeply Conceptual Allegories" },
            { "F", "Nearly Incomprehensible Symbols" },
            { "G", "Mystic Mathematical Abstractions" },
            { "H", "Hyper-Abstracted Symbols" },
            { "J", "Philosophical Ineffabilities" },
            { "K", "Esoteric Transcendent Codes" },
            { "L", "Incomprehensibly Abstract" }
        };

        #endregion
    }
}

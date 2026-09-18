using System;
using System.Collections.Generic;
using System.Text;

namespace GTStarData
{
    public enum EncounterType
    {
        NoEncounter,
        Traveller,
        SmallFreighter,
        MediumFreighter,
        HeavyFreighter,
        Convoy,
        Liner,
        UnusualVessel,
        NavalPatrol,
        SystemDefenceBoat

    }

    [Serializable]
    public class RandomEncounter
    {
        public int EncounterD66 { get; set; }
        public string EncounterName { get; set; }
        public string Description { get; set; }
        public string Quirks { get; set; }
        public int QuirksD66 { get; set; }
        public string Complications { get; set; }
        public int ComplicationsD66 { get; set; }
        public bool IsRichFreighter { get; set; }
        public EncounterType EncounterType { get; set; }

        public int EncounterDistance { get; set; }
        public string Direction { get; set; }

    }

    public static class RandomEncounterGenerator
    { 

        static Random r = new Random();

        public static RandomEncounter Space(SystemData data)
        {
            RandomEncounter encounter = new RandomEncounter();

            int d1mod = 0;
            int d2mod = 0;
            string starport = data.UWP.Subcode(UWP.Starport);
            int lawlevel = data.UWP.Subcode(UWP.LawLevel).ToInt();
            int techlevel = data.UWP.Subcode(UWP.TechLevel).ToInt();

            if (!data.Allegiance.StartsWith("Im") && !data.Allegiance.StartsWith("As") && (starport == "X" || starport == "E"))
                d1mod -= 1;

            if (data.Zone == "A" || data.Zone == "R" || lawlevel <= 3)
                d2mod -= 1;

            if (starport == "A" || starport == "B")
                if (data.Remarks.Contains("In") || data.Remarks.Contains("Ph") || data.Remarks.Contains("Hi") || data.Remarks.Contains("Ag") || data.Remarks.Contains("Ri") || techlevel > 11)
                    d1mod += 1;

            if (data.Bases != null)
                d2mod += 2;

            if (lawlevel >= 7)
                d2mod += 1;

            if (data.IX.Subcode(0).ToInt() > 1)
                d1mod += 2;

            encounter.EncounterDistance = r.D6(3) * data.AveragePlanetDiameter();

            SelectPreyEncounter(d1mod, d2mod, encounter);
            if (encounter.EncounterType != EncounterType.NoEncounter)
            {
                SelectPreyQuirks(encounter);
                SelectPreyComplications(encounter);
            }


            return encounter;
        }

        private static void SelectPreyComplications(RandomEncounter encounter)
        {
            encounter.ComplicationsD66 = r.D66();
            switch (encounter.ComplicationsD66)
            {
                case 11:
                    encounter.Complications =
@"Solar Flares: The system’s primary sun spits out huge
flares and high levels of radiation.All ships take 3D x
100 rads per hour."; break;

                case 41:
                    encounter.Complications =
@"Rapid Reaction: The security forces here respond very
quickly – apply DM+4 to the response time roll."; break;

                case 12:
                    encounter.Complications =
@"Debris Field: The encounter takes place in a debris
field.Pilot checks are needed to avoid floating
obstacles; on the bright side, there may be some
salvage here."; break;

                case 42:
                    encounter.Complications =
               @"Corrupt Cops: The security forces can be bribed to
ignore the attack."; break;
                case 13:
                    encounter.Complications =
           @"Ice Field: The planet’s surrounded by a ring of ice
particles, and the quarry takes refuge there.Direct - fire
weapons are limited to Short range."; break;
                case 43:
                    encounter.Complications =
@"Nearby Asteroid: There’s an asteroid close to the battle;
            the merchant can fly to the refuge and hide behind it.
The asteroid might even be inhabited."; break;
                case 14:
                    encounter.Complications =
@"Comms Jamming: Something in the system blocks
communications.The victim can’t call for help."; break;
                case 44:
                    encounter.Complications =
@"Sensor Jamming: Conditions in the system block
sensors."; break;
                case 15:
                    encounter.Complications =
@"Behind The Moon: There’s a nearby moon.What’s
lurking there ? Another pirate ? An interceptor ? An Aslan
spy ?"; break;
                case 45:
                    encounter.Complications =
@"Imperial Patrol: There’s an Imperial or Aslan patrol
in the system, hunting for pirates.They’re far enough
away that the Travellers might be able to complete the
attack before the first fighters arrive..."; break;
                case 16:
                    encounter.Complications =
@"Incoming Escort: The merchant has an escort, but they
haven’t jumped in yet.They’ll be here any minute."; break;
                case 46:
                    encounter.Complications =
@"Distress Call: The Travellers detect a distress call from
a stricken ship.Do they call off their attack ?"; break;

                case 21: encounter.Complications =
@"Rival Pirate: There’s another pirate after the same prize"; break;
                case 51: encounter.Complications =
@"High Guard: There’s an unexpected ship refuelling
at the system’s gas giant(or at another source of
hydrogen, like a lake). Why are they avoiding the
starport ?"; break;
                case 22:
                    encounter.Complications =
@"Slow Leak: The Travellers’ fuel tank has a slow leak;
they’re losing 1D tons of fuel per round."; break;
                case 52:
                    encounter.Complications =
@"Spy in the System: A spy in the system contacts the
Travellers by radio, offering them useful information
about traffic."; break;
                case 23:
                    encounter.Complications =
@"Out of Control: The quarry loses control of its attitude
thrusters and starts spinning wildly. It’s now easy to
catch but very hard to dock with."; break;
                case 53:
                    encounter.Complications =
@"Screamer: The merchant ship frantically warns
everyone who’ll listen about the pirates – not just in
this system, but in every other system the merchant
visits"; break;
                case 24:
                    encounter.Complications =
@"Cargo Spilled: In a panic, the merchant jettisons most
of its cargo, sending an expanding flock of canisters
into space."; break;
                case 54:
                    encounter.Complications =
@"Incoming!: The starport below launches ground -
to - space missiles. The first missile hits in 1D + 10
rounds..."; break;
                case 25:
                    encounter.Complications =
@"Collision Warning!Both ships nearly collide with a
small asteroid or other piece of space debris."; break;
                case 55:
                    encounter.Complications =
@"Tricky Calculation: The complex arrangement of moons
and planets in this system make jump calculations
harder. Apply DM-4 to any Astrogation checks."; break;
                case 26:
                    encounter.Complications =
@"Misjump: The first ship to jump out misjumps when
they flee"; break;
                case 56:
                    encounter.Complications =
@"Pull Up!: The merchant doesn’t slow down as it
approaches the planet – instead, they plan to use
aerobraking to slow their dissent."; break;
                case 31:
                    encounter.Complications =
@"Observer: There’s another ship nearby. They steer clear
of the dogfight, but they’re watching..."; break;
                case 61:
                    encounter.Complications =
@"The Black Signal: The pirates pick up the fabled ‘black
signal’ on the ship; a pattern of radiation burned into
the hull, denoting that this ship is an enemy of the
pirates of Theev."; break;
                case 32:
                    encounter.Complications =
@"Bad Jump: This was a bad jump – the pirates have
arrived well outside the travelled parts of the system."; break;
                case 62:
                    encounter.Complications =
@"Familiar Ship: The Travellers have encountered this
merchant ship before..."; break;
                case 33:
                    encounter.Complications =
@"Unfortunate Timing: Another ship jumps right into the
middle of the battle."; break;
                case 63:
                    encounter.Complications =
@"Aslan Raiders: Several Aslan raiders led by an
ambitious ihatei warlord arrive in the system."; break;
                case 34:
                    encounter.Complications =
@"Crew Dissent: One of the crew on board the Travellers’
ship is having problems that affect the battle. Perhaps
they object to this particular target, are drunk, or are
deliberately sabotaging the attack."; break;
                case 64:
                    encounter.Complications =
@"Under The Shield of the Sunburst: An Imperial patrol
jumps in; they’re not pirate hunting, they’re here to
enforce the Third Imperium’s will on the planetary
government."; break;
                case 35:
                    encounter.Complications =
@"System Failure: A key system fails on board the pirate
ship.Roll for a random critical hit with a Severity of
D3."; break;
                case 65:
                    encounter.Complications =
@"Didn’t Expect To Find You Here: A Contact(or Ally, or
Enemy) of a Traveller is on board the merchant."; break;
                case 36:
                    encounter.Complications =
@"Escape Pods: The merchant’s crew flee their ship in
escape pods and small craft.They could be carrying
treasure on board those pods – but the pirates have
time to only chase down one of them..."; break;
                case 66:
                    encounter.Complications =
@"Anomaly: The Travellers run into something unusual,
like a wrecked ship or a spatial anomaly."; break;
            }
        }

        private static void SelectPreyQuirks(RandomEncounter encounter)
        {
            encounter.QuirksD66 = r.D66();
            switch (encounter.QuirksD66)
            {
                case 11: encounter.Quirks = "Coward: Surrenders easily. Reduce starting MOR by 1D"; break;
                case 12: encounter.Quirks = "Deceitful: Pretends to surrender in order to lure the pirates into docking, then fights back at short range"; break;
                case 13: encounter.Quirks = "Smuggler: The really valuable cargo is hidden in a secret compartment"; break;
                case 14: encounter.Quirks = "Eccentric: The captain is insanse, drunk or otherwise eccentric"; break;
                case 15: encounter.Quirks = "No Surrender: The crew will not surrender under any circumstances. Ignore MOR"; break;
                case 16: encounter.Quirks = "Duel of Honour: The captain challenges one of the pirates to a rapier duel in vacc suits on the exterior hull of the ship"; break;
                case 21: encounter.Quirks = "Noble: There's a noble on board. If ransomed, she's worth considerably more than normal."; break;
                case 22: encounter.Quirks = "Alien: There's an exotic alien like a Hiver on board."; break;
                case 23: encounter.Quirks = "Family: The captain's family travel on board the ship"; break;
                case 24: encounter.Quirks = "Diplomat: There is an Imperial or Aslan diplomat on board, carrying a secret message"; break;
                case 25: encounter.Quirks = "Stowaway: Someone's hidden inside a cargo container the pirates just stole"; break;
                case 26: encounter.Quirks = "Prisoner: There's a criminal - perhaps a captured pirate - in the ship's brig"; break;
                case 31: encounter.Quirks = "Plague Ship: The crew are infected with a potentially fatal disease"; break;
                case 32: encounter.Quirks = "Dying Ship: The ship misjumped and is running low on food, oxygen or fuel"; break;
                case 33: encounter.Quirks = "Damaged Ship: The ship has sustained 1D critical hists, each of Severity D3 already"; break;
                case 34: encounter.Quirks = "Treasure Map: While looting the ship, the Travellers find a map pointing to a hidden supply cache, mineral deposit or other valuable treasure."; break;
                case 35: encounter.Quirks = "Important Document: The ship's safe contains the deeds to a property, a letter of marque, a corporate contract or some other valuable document"; break;
                case 36: encounter.Quirks = "Message Pos: The ship carries a 5-dton data drum containing mail. Decoding this data may reveal useful information"; break;
                case 41: encounter.Quirks = "Heavily Armed: The merchant ship is ready for a fight. Any hardpoints are equipped with turrets"; break;
                case 42: encounter.Quirks = "Berserker: One of the merchant crew is a trained marine equipped with battle dress or boarding vacc suit, and a heavy weapon"; break;
                case 43: encounter.Quirks = "Self Destruct: The captain would rather die than lose his ship. Unless the pirates can stop him, he'll scuttle his ship rather than lise the cargo"; break;
                case 44: encounter.Quirks = "Mission of Mercy: The ship is carrying vitally needed supplies, like medicine or food, to a troubled colony"; break;
                case 45: encounter.Quirks = "Die Hard: One of the merchant's crew hides when the ship is boarded, and sneaks onto the Traveller's ship to sabotage them"; break;
                case 46: encounter.Quirks = "Psionic Defender: One of the crew of the merchant ship is a psion"; break;
                case 51: encounter.Quirks = "Unlikely Cargo: The merchant ship is carryinf an unexpected cargo - what are they doing out here?"; break;
                case 52: encounter.Quirks = "Perishable Cargo: The merchant's cargo is valuable, but only if sold within the month"; break;
                case 53: encounter.Quirks = "Dangerous Cargo: The merchant's cargo is dangerous to have on board"; break;
                case 54: encounter.Quirks = "Living Cargo: The cargo is alive - animals, insects or event slaves"; break;
                case 55: encounter.Quirks = "Hot Cargo: The cargo was stolen - and the real owner wants it back"; break;
                case 56: encounter.Quirks = "Alien Cargo: The merchant is carrying something form a very distance part of space, or even an Ancient relic"; break;
                case 61: encounter.Quirks = "Traitor: One of the merchant's crew is willing to betray his shipmates for a large payoff"; break;
                case 62: encounter.Quirks = "Infestation: There's something alive on board the ship"; break;
                case 63: encounter.Quirks = "Ghost Ship: The ship has been drifting dead for centuries. The Travellers were attacked by automated weapons"; break;
                case 64: encounter.Quirks = "Strange Curio: There's a relic or other strange item in the captain's cabin"; break;
                case 65: encounter.Quirks = "It's a Trap: This 'merchant' is actually a disguised q-ship or pirate hunter"; break;
                case 66: encounter.Quirks = "Drinaxian on Board: One of the important Drinaxian NPCs is on board - what are they doing here?"; break;
            }
        }

        private static void SelectPreyEncounter(int d1mod, int d2mod, RandomEncounter encounter)
        {
            int d1 = (r.D6() + d1mod).Min(0).Max(7);
            int d2 = (r.D6() + d2mod).Min(0).Max(8);
            int d3 = r.D6();

            encounter.EncounterD66 = d1 * 10 + d2;
            switch (encounter.EncounterD66)
            {
                case 0:
                case 1:
                case 10:
                case 20:
                case 30:
                case 41:
                case 50:
                case 60:
                case 70:
                    encounter.EncounterName = "Traveller";
                    encounter.EncounterType = EncounterType.Traveller;
                    encounter.Description = "A vessel other than a cargo ship, such as a scout vessel, small military ship, fast courier or even another pirate. Travellers are poor targets for pirates, as they are unlikely to carry valuable cargoes. They can be looted for parts."; ;
                    break;

                case 4:
                case 14:
                case 23:
                case 31:
                case 40:
                case 52:
                case 62:
                case 71:
                    encounter.EncounterName = "Small Freighter";
                    encounter.EncounterType = EncounterType.SmallFreighter;
                    encounter.Description = @"A far trader, free trader or other 100-
300 ton trade vessel. Such vessels are ideal prey for
most pirates, as they can quickly be looted and are
unlikely to have significant defences.";
                    break;

                case 17:
                case 24:
                case 34:
                case 53:
                case 63:
                case 72:
                    encounter.EncounterName = "Medium Freighter";
                    encounter.EncounterType = EncounterType.MediumFreighter;
                    encounter.Description = @"A trader of 400-1,000 tons. Still a
good target for pirates, although sorting through the
cavernous cargo bay for the choicest items may take
more time than the pirate can afford.";
                    break;

                case 43:
                case 55:
                    encounter.EncounterName = "Heavy Freighter";
                    encounter.EncounterType = EncounterType.HeavyFreighter;
                    encounter.Description = @"This is a freighter of 1,000 tons or
more. Heavy freighters of this kind travel with escort
vessels or carry their own fighters or defensive weapons,
and may be more than the average pirate can handle.";
                    break;

                case 37:
                case 66:
                case 76:
                    switch (r.D6())
                    {
                        case 1:
                        case 2:
                        case 3:
                            encounter.EncounterName = "Rich Freighter (Small)";
                            encounter.EncounterType = EncounterType.SmallFreighter;
                            encounter.Description = @"This freighter is carrying
an especially valuable cargo; when rolling for a random
cargo, roll twice and take the most valuable result.";
                            break;

                        case 4:
                        case 5:
                            encounter.EncounterName = "Rich Freighter (Medium)";
                            encounter.EncounterType = EncounterType.MediumFreighter;
                            encounter.Description = @"This freighter is carrying
an especially valuable cargo; when rolling for a random
cargo, roll twice and take the most valuable result.";
                            break;

                        case 6:
                            encounter.EncounterName = "Rich Freighter (Heavy)";
                            encounter.EncounterType = EncounterType.HeavyFreighter;
                            encounter.Description = @"This freighter is carrying
an especially valuable cargo; when rolling for a random
cargo, roll twice and take the most valuable result.";
                            break;
                    }
                    break;

                case 32:
                case 42:
                case 51:
                case 61:
                case 65:
                case 73:
                    encounter.EncounterName = "Convoy";
                    encounter.EncounterType = EncounterType.Convoy;
                    encounter.Description = @"A convoy consists of 2-12 ships, split between
Heavy, Medium and Rich Freighters and armed escorts.";
                    break;

                case 46:
                case 56:
                case 64:
                case 75:
                    encounter.EncounterName = "Liner";
                    encounter.EncounterType = EncounterType.Liner;
                    encounter.Description = @"This is a passenger vessel, colony ship, troop
transport or tourist vessel. Such ships may have many
valuable items carried by the passengers, but the pirates
will be outnumbered if they try to storm on board.";
                    break;

                case 26:
                case 33:
                case 74:
                    encounter.EncounterName = "Unusual Vessel";
                    encounter.EncounterType = EncounterType.UnusualVessel;
                    encounter.Description = @"A noble’s yacht, science vessel, X-boat,
mercenary transport, survey ship or other rare type of
ship. The vessel may be utterly useless to the pirate or a
rich prize, depending on what is on board. It could even
be a rival pirate.";
                    break;

                case 8:
                case 18 when d3 >= 4:
                case 28 when d3 >= 4:
                case 38 when d3 >= 4:
                case 48 when d3 >= 4:
                case 58 when d3 >= 3:
                case 68:
                case 78:
                    encounter.EncounterName = "Naval Patrol";
                    encounter.EncounterType = EncounterType.NavalPatrol;
                    encounter.Description = @"The pirate has run into a heavily armed
navy ship, ranging from a small escort ship or pirate
hunter to a full-size warship.";
                    break;

                case 27:
                case 47:
                case 57:
                case 67:
                case 77:
                    switch (r.D6())
                    {
                        case 1:
                            encounter.EncounterName = "Small Freighter (Q-Ship)";
                            encounter.EncounterType = EncounterType.SystemDefenceBoat;
                            encounter.Description = @"This is a q-ship,
a small freighter that has been refitted with concealed
weapons as a honey trap for pirates.";
                            break;

                        default:
                            encounter.EncounterName = "System Defence Boat";
                            encounter.EncounterType = EncounterType.SystemDefenceBoat;
                            encounter.Description = @"This is a classic
SDB of some sort – a heavily armed, fast-moving
spaceship without a jump drive. ";
                            break;
                    }
                    break;

                default:
                    encounter.EncounterName = "No encounter";
                    encounter.EncounterType = EncounterType.NoEncounter;
                    break;
            }

            if (encounter.EncounterType != EncounterType.NoEncounter)
            {
                switch (r.D6())
                {
                    case 1:
                    case 2:
                    case 3:
                        encounter.Direction = "Inbound to main world";
                        break;

                    case 4:
                    case 5:
                        encounter.Direction = "Outbound from main world";
                        break;

                    case 6:
                        encounter.Direction = "Stationary, or heading to a moon or other planet";
                        break;
                }
            }
        }
    }
}

// A WEB5 STAR CelestialBody DNA file: the description STAR generates an OAPP (and, with HoloOASIS, its hApp) from.
// Each class deriving from ZomeDNA is a zome; each class inside it deriving from HolonDNA is an entry type, and its public
// properties become the entry's fields (string, int, long, bool, double, float, Guid, DateTime).
// (This file is data for STAR and HolochainHappGenerator.FromStarDnaFolder; it isn't compiled into the example.)

using System;
using NextGenSoftware.OASIS.STAR.DNA;

namespace SpaceGame.DNA
{
    public class Game : ZomeDNA
    {
        public class Ship : HolonDNA
        {
            public string Name { get; set; }
            public double Speed { get; set; }
            public int Crew { get; set; }
            public DateTime Launched { get; set; }
            public bool IsDocked { get; set; }
        }

        public class Planet : HolonDNA
        {
            public string Name { get; set; }
            public long Population { get; set; }
        }
    }
}

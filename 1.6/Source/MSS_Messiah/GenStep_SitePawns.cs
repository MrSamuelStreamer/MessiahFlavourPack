using System.Linq;
using RimWorld;
using RimWorld.BaseGen;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.AI.Group;

namespace MSS_Messiah;

public class GenStep_SitePawns : GenStep
{
    public override int SeedPart => 237483478;

    private static PawnGroupMakerParms GroupMakerParmsWorkers(
        PlanetTile tile,
        Faction faction,
        float points,
        int seed
    )
    {
        float a = points / 2f;
        PawnGroupMakerParms parms = new PawnGroupMakerParms
        {
            groupKind = PawnGroupKindDefOf.Settlement,
            tile = tile,
            faction = faction,
            inhabitants = true,
            seed = seed,
        };
        parms.points = Mathf.Max(
            a,
            faction.def.MinPointsToGeneratePawnGroup(parms.groupKind, parms)
        );
        return parms;
    }

    private static PawnGroupMakerParms GroupMakerParmsFighters(
        PlanetTile tile,
        Faction faction,
        float points,
        int seed
    )
    {
        PawnGroupKindDef groupKindDef = PawnGroupKindDefOf.Combat;
        if (!faction.def.pawnGroupMakers.Any(maker => maker.kindDef == PawnGroupKindDefOf.Combat))
            groupKindDef = PawnGroupKindDefOf.Settlement;
        PawnGroupMakerParms parms = new PawnGroupMakerParms
        {
            groupKind = groupKindDef,
            tile = tile,
            faction = faction,
            inhabitants = true,
            generateFightersOnly = true,
            seed = seed,
        };
        parms.points = Mathf.Max(
            points / 2f,
            faction.def.MinPointsToGeneratePawnGroup(parms.groupKind, parms)
        );
        return parms;
    }

    public override void Generate(Map map, GenStepParams parms)
    {
        IntVec3 baseCenter = map.Center;
        Faction faction = parms.sitePart.site.Faction;

        if (faction == null)
        {
            // Try to get hostile faction first, then any enemy faction as fallback
            faction = Find.FactionManager.AllFactionsListForReading.FirstOrDefault(f =>
                f != Faction.OfPlayer && !f.def.hidden && f.HostileTo(Faction.OfPlayer)
            );

            if (faction == null)
            {
                faction = Find.FactionManager.AllFactionsListForReading.FirstOrDefault(f =>
                    f != Faction.OfPlayer && !f.def.hidden
                );
            }

            if (faction == null)
            {
                return;
            }
        }

        if (parms.sitePart.parms.threatPoints <= 0)
        {
            parms.sitePart.parms.threatPoints = 100f;
        }

        Lord lord = LordMaker.MakeNewLord(
            faction,
            new LordJob_DefendBase(faction, baseCenter, 25000),
            map
        );
        TraverseParms traverseParms = TraverseParms.For(TraverseMode.PassDoors);
        ResolveParams settlerParams = new ResolveParams
        {
            faction = faction,
            singlePawnLord = lord,
            singlePawnSpawnCellExtraPredicate = x =>
                map.reachability.CanReachMapEdge(x, traverseParms),
        };
        int pawnGroupMakerSeed = OutpostSitePartUtility.GetPawnGroupMakerSeed(parms.sitePart.parms);
        settlerParams.pawnGroupMakerParams = GroupMakerParmsWorkers(
            map.Tile,
            faction,
            parms.sitePart.parms.threatPoints,
            pawnGroupMakerSeed
        );
        settlerParams.pawnGroupKindDef = settlerParams.pawnGroupMakerParams.groupKind;
        // Generate workers directly
        var workers = faction
            .def.pawnGroupMakers.Where(maker => maker.kindDef == PawnGroupKindDefOf.Settlement)
            .FirstOrDefault()
            ?.GeneratePawns(settlerParams.pawnGroupMakerParams);

        if (workers != null)
        {
            foreach (var pawn in workers)
            {
                if (pawn != null && !pawn.Destroyed)
                {
                    GenSpawn.Spawn(pawn, map.Center, map);
                    lord.AddPawn(pawn);
                }
            }
        }

        // Generate fighters directly
        var fighterParams = GroupMakerParmsFighters(
            map.Tile,
            faction,
            parms.sitePart.parms.threatPoints,
            pawnGroupMakerSeed
        );
        var fighters = faction
            .def.pawnGroupMakers.Where(maker => maker.kindDef == fighterParams.groupKind)
            .FirstOrDefault()
            ?.GeneratePawns(fighterParams);

        if (fighters != null)
        {
            foreach (var pawn in fighters)
            {
                if (pawn != null && !pawn.Destroyed)
                {
                    GenSpawn.Spawn(pawn, map.Center, map);
                    lord.AddPawn(pawn);
                }
            }
        }
    }
}

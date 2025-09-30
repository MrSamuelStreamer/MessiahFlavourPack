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
      seed = seed
    };
    parms.points = Mathf.Max(a, faction.def.MinPointsToGeneratePawnGroup(parms.groupKind, parms));
    return parms;
  }

  private static PawnGroupMakerParms GroupMakerParmsFighters(
    PlanetTile tile,
    Faction faction,
    float points,
    int seed)
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
      seed = seed
    };
    parms.points = Mathf.Max(points / 2f, faction.def.MinPointsToGeneratePawnGroup(parms.groupKind, parms));
    return parms;
  }

  public override void Generate(Map map, GenStepParams parms)
  {
    IntVec3 baseCenter = map.Center;
    Faction faction = parms.sitePart.site.Faction;
    Lord lord = LordMaker.MakeNewLord(faction, new LordJob_DefendBase(faction, baseCenter, 25000), map);
    TraverseParms traverseParms = TraverseParms.For(TraverseMode.PassDoors);
    ResolveParams settlerParams = new ResolveParams
    {
      faction = faction,
      singlePawnLord = lord,
      singlePawnSpawnCellExtraPredicate = x => map.reachability.CanReachMapEdge(x, traverseParms)
    };
    int pawnGroupMakerSeed = OutpostSitePartUtility.GetPawnGroupMakerSeed(parms.sitePart.parms);
    settlerParams.pawnGroupMakerParams = GroupMakerParmsWorkers(map.Tile, faction,
      parms.sitePart.parms.threatPoints, pawnGroupMakerSeed);
    settlerParams.pawnGroupKindDef = settlerParams.pawnGroupMakerParams.groupKind;
    BaseGen.symbolStack.Push("pawnGroup", settlerParams);
    ResolveParams fighterParams = settlerParams with
    {
      pawnGroupMakerParams =
      GroupMakerParmsFighters(map.Tile, faction, parms.sitePart.parms.threatPoints,
        pawnGroupMakerSeed)
    };
    fighterParams.pawnGroupKindDef = fighterParams.pawnGroupMakerParams.groupKind;
    BaseGen.symbolStack.Push("pawnGroup", fighterParams);
    BaseGen.globalSettings.map = map;
    BaseGen.Generate();
  }
}
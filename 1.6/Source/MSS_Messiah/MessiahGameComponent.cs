using RimWorld;
using RimWorld.Planet;
using Verse;

namespace MSS_Messiah;

public class MessiahGameComponent(Game game) : GameComponent
{
    public Game Game { get; } = game;

    public override void FinalizeInit()
    {
        bool hasSkullSites = Find.World.worldObjects.Sites.Any(s=>s.MainSitePartDef == MSS_MessiahDefOf.MSSFP_FallenSkyKnight);

        if (hasSkullSites) return;

        int sitesToGen = Rand.Range(1, 5);
        for(int i = 0; i < sitesToGen; i++)
        {
            Faction faction = Find.FactionManager.AllFactionsVisible.RandomElement();
            PlanetLayer layer = Find.WorldGrid.Surface;
            PlanetTile tile = TileFinder.RandomSettlementTileFor(layer, faction);
            ModLog.Debug($"Making site MSSFP_FallenSkyKnight at {tile}");
            Site site = SiteMaker.MakeSite(MSS_MessiahDefOf.MSSFP_FallenSkyKnight, tile, faction);
            Find.WorldObjects.Add(site);
        }
    }
}
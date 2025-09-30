using RimWorld;

namespace MSS_Messiah;

[DefOf]
public static class MSS_MessiahDefOf
{
    public static readonly SitePartDef MSSFP_FallenSkyKnight;
    
    static MSS_MessiahDefOf() => DefOfHelper.EnsureInitializedInCtor(typeof(MSS_MessiahDefOf));
}

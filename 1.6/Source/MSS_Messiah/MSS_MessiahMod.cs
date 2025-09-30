using HarmonyLib;
using Verse;

namespace MSS_Messiah;

[StaticConstructorOnStartup]
public class MSS_MessiahMod : Mod
{
    public MSS_MessiahMod(ModContentPack content)
        : base(content)
    {
        ModLog.Log("Loading the Mr Samuel Streamer Messiah Pack");

#if DEBUG
        Harmony.DEBUG = true;
#endif
        Harmony harmony = new Harmony("MrSamuelStreamer.rimworld.MSS_Messiah.main");
        harmony.PatchAll();
    }
}

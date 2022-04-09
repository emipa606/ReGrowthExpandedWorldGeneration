using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

namespace RGExpandedWorldGeneration;

[HarmonyPatch]
public static class RimWar_Patch
{
    private static MethodBase patch_Page_CreateWorldParams_DoWindowContents;

    public static float yOffset = 295;
    public static float xOffset = 495;

    public static bool Prepare()
    {
        patch_Page_CreateWorldParams_DoWindowContents =
            AccessTools.Method("RimWar.Harmony.RimWarMod+Patch_Page_CreateWorldParams_DoWindowContents:Postfix");
        return patch_Page_CreateWorldParams_DoWindowContents != null;
    }

    public static IEnumerable<MethodBase> TargetMethods()
    {
        yield return patch_Page_CreateWorldParams_DoWindowContents;
    }

    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var codes = instructions.ToList();

        for (var i = 0; i < codes.Count; i++)
        {
            var code = codes[i];
            if (codes[i].opcode == OpCodes.Ldc_R4)
            {
                if (codes[i].OperandIs(118))
                {
                    yield return new CodeInstruction(OpCodes.Ldsfld,
                        AccessTools.Field(typeof(RimWar_Patch), "yOffset"));
                }
                else if (codes[i].OperandIs(0))
                {
                    yield return new CodeInstruction(OpCodes.Ldsfld,
                        AccessTools.Field(typeof(RimWar_Patch), "xOffset"));
                }
                else
                {
                    yield return code;
                }
            }
            else
            {
                yield return code;
            }
        }
    }
}
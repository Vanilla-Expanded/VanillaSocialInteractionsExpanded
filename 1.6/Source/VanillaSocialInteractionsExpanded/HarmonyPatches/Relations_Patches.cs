using HarmonyLib;
using RimWorld;
using Verse;

namespace VanillaSocialInteractionsExpanded
{
    [HarmonyPatch(typeof(Pawn_RelationsTracker), "AddDirectRelation")]
    public class AddDirectRelation_Patch
    {
        private static void Prefix(Pawn ___pawn, PawnRelationDef def, Pawn otherPawn)
        {
            if (VanillaSocialInteractionsExpandedSettings.EnableMemories && Find.TickManager.gameStartAbsTick != 0)
            {
                if (def == PawnRelationDefOf.Lover || def == PawnRelationDefOf.Fiance || def == PawnRelationDefOf.Spouse)
                {
                    var mainSpouse = ___pawn;
                    var newLover = otherPawn;
                    var exLover1 = ___pawn.GetSpouseOrLoverOrFiance();
                    if (exLover1 is null)
                    {
                        mainSpouse = otherPawn;
                        newLover = ___pawn;
                        exLover1 = mainSpouse.GetSpouseOrLoverOrFiance();
                    }

                    if (ModsConfig.IdeologyActive && mainSpouse.Ideo != null)
                    {
                        foreach (var precept in mainSpouse.Ideo.PreceptsListForReading)
                        {
                            var spouses = mainSpouse.GetSpouses(false);

                            switch (precept.def.defName, mainSpouse.gender, spouses.Count)
                            {
                                case ("SpouseCount_Female_MaxTwo", Gender.Female, < 2):
                                case ("SpouseCount_Female_MaxThree", Gender.Female, < 3):
                                case ("SpouseCount_Female_MaxFour", Gender.Female, < 4):
                                case ("SpouseCount_Female_Unlimited", Gender.Female, _):
                                case ("SpouseCount_Male_MaxTwo", Gender.Male, < 2):
                                case ("SpouseCount_Male_MaxThree", Gender.Male, < 3):
                                case ("SpouseCount_Male_MaxFour", Gender.Male, < 4):
                                case ("SpouseCount_Male_Unlimited", Gender.Male, _):
                                    return;
                            }
                        }
                    }

                    if (exLover1 != null && !exLover1.Dead && exLover1 != newLover)
                    {
                        if (Rand.Chance(0.1f))
                        {
                            TaleRecorder.RecordTale(VSIE_DefOf.VSIE_StoleMyLover, exLover1, mainSpouse, newLover);
                        }
                    }

                    var exLover2 = newLover.GetSpouseOrLoverOrFiance();
                    if (exLover2 != null && !exLover2.Dead && exLover2 != newLover)
                    {
                        if (Rand.Chance(0.1f))
                        {
                            TaleRecorder.RecordTale(VSIE_DefOf.VSIE_StoleMyLover, exLover2, newLover, mainSpouse);
                        }
                    }
                }
            }
        }
    }
}
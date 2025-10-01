using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using Verse.Grammar;

namespace VanillaSocialInteractionsExpanded
{
    public class GatheringWorker_DoublePawn : GatheringWorker
    {
        public override bool CanExecute(Map map, Pawn organizer = null)
        {
            bool debug = false;
            if (organizer == null)
            {
                organizer = FindOrganizerCustom(map, out var companion);
                if (organizer is null)
                {
                    if (debug) Log.Warning("Could not find any organizer for gathering " + def.defName + " on map " + map);
                    return false;
                }
                else if (companion is null)
                {
                    if (debug) Log.Warning("Could not find any companion for gathering " + def.defName + " on map " + map + " with organizer " + organizer);
                    return false;
                }
            }
            if (!TryFindGatherSpot(organizer, out IntVec3 _))
            {
                if (debug) Log.Warning("Could not find any spot for gathering " + def.defName + " on map " + map + " with organizer " + organizer);
                return false;
            }
            if (!GatheringsUtility.PawnCanStartOrContinueGathering(organizer))
            {
                if (debug) Log.Warning("Organizer " + organizer + " cannot start or continue gathering " + def.defName + " on map " + map);
                return false;
            }
            else if (FindCompanion(organizer, this.def) is null)
            {
                if (debug) Log.Warning("Could not find any companion for gathering " + def.defName + " on map " + map + " with organizer " + organizer);
                return false;
            }
            else if (!ConditionsMeet(organizer))
            {
                if (debug) Log.Warning("Conditions to start gathering " + def.defName + " on map " + map + " with organizer " + organizer + " are not met");
                return false;
            }
            if (debug) Log.Message("Can execute gathering " + def.defName + " on map " + map + " with organizer " + organizer);
            return true;
        }

        public override bool TryExecute(Map map, Pawn organizer = null)
        {
            bool debug = false;
            Pawn companion = null;
            if (organizer == null)
            {
                organizer = FindOrganizerCustom(map, out companion);
            }

            if (organizer == null)
            {
                if (debug) Log.Warning("Could not find any organizer for gathering " + def.defName + " on map " + map);
                return false;
            }
            if (!TryFindGatherSpot(organizer, out IntVec3 spot))
            {
                if (debug) Log.Warning("Could not find any spot for gathering " + def.defName + " on map " + map + " with organizer " + organizer);
                return false;
            }
            if (!GatheringsUtility.PawnCanStartOrContinueGathering(organizer))
            {
                if (debug) Log.Warning("Organizer " + organizer + " cannot start or continue gathering " + def.defName + " on map " + map);
                return false;
            }
            if (organizer is null || companion is null || !ConditionsMeet(organizer))
            {
                if (debug) Log.Warning("Conditions to start gathering " + def.defName + " on map " + map + " with organizer " + organizer + " are not met");
                return false;
            }
            LordJob lordJob = CreateLordJobCustom(spot, organizer, companion);
            List<Pawn> pawns = new List<Pawn> { organizer, companion };
            LordMaker.MakeNewLord(organizer.Faction, lordJob, organizer.Map, pawns);
            SendLetterCustom(spot, organizer, companion);
            return true;
        }

        protected virtual void SendLetterCustom(IntVec3 spot, Pawn organizer, Pawn companion)
        {
            Find.LetterStack.ReceiveLetter(def.letterTitle, def.letterText.Formatted(organizer.Named("ORGANIZER"), companion.Named("COMPANION")), LetterDefOf.PositiveEvent,
                new List<Pawn> { organizer, companion });
        }
        private Pawn FindOrganizerCustom(Map map, out Pawn companion)
        {
            companion = null;
            if (map is null) return null;
            var organizer = FindRandomGatheringOrganizer(Faction.OfPlayer, map, def, out companion);
            if (organizer is null)
            {
                return null;
            }
            return organizer;
        }

        private bool BasePawnValidator(Pawn pawn, GatheringDef gatheringDef)
        {
            var value = pawn != null && pawn.Spawned && pawn.Downed is false && pawn.RaceProps.Humanlike && !pawn.InBed() && !pawn.InMentalState && pawn.GetLord() == null
            && GatheringsUtility.ShouldPawnKeepGathering(pawn, gatheringDef) && !pawn.Drafted && (gatheringDef.requiredTitleAny == null || gatheringDef.requiredTitleAny.Count == 0
            || (pawn.royalty != null && pawn.royalty.AllTitlesInEffectForReading.Any((RoyalTitle t) => gatheringDef.requiredTitleAny.Contains(t.def))));
            return value;
        }
        public Pawn FindRandomGatheringOrganizer(Faction faction, Map map, GatheringDef gatheringDef, out Pawn companion)
        {
            Predicate<Pawn> v = (Pawn organizer) => BasePawnValidator(organizer, gatheringDef) && MemberValidator(organizer) && FindCompanion(organizer, gatheringDef) != null;
            if (map.mapPawns.SpawnedPawnsInFaction(faction).Where(x => v(x)).TryRandomElement(out Pawn result))
            {
                companion = FindCompanion(result, gatheringDef);
                return result;
            }
            companion = null;
            return null;
        }

        protected Pawn FindCompanion(Pawn organizer, GatheringDef gatheringDef)
        {
            var candidates = organizer.Map.mapPawns.SpawnedPawnsInFaction(organizer.Faction).Where(candidate => candidate != organizer && BasePawnValidator(candidate, gatheringDef) && MemberValidator(candidate) && PawnsCanGatherTogether(organizer, candidate));
            if (candidates.Any() && candidates.TryRandomElementByWeight(x => SortCandidatesBy(organizer, x), out var companion))
            {
                return companion;
            }
            return null;
        }

        protected virtual float SortCandidatesBy(Pawn organizer, Pawn candidate)
        {
            return 0f;
        }
        protected virtual LordJob CreateLordJobCustom(IntVec3 spot, Pawn organizer, Pawn companion)
        {
            return null;
        }
        protected virtual bool MemberValidator(Pawn pawn)
        {
            return pawn.Spawned;
        }
        protected virtual bool PawnsCanGatherTogether(Pawn organizer, Pawn companion)
        {
            return organizer != companion && organizer is not null && companion is not null && organizer.Spawned && companion.Spawned;
        }
        protected virtual bool ConditionsMeet(Pawn organizer)
        {
            return true;
        }
        public override bool TryFindGatherSpot(Pawn organizer, out IntVec3 spot)
        {
            return RCellFinder.TryFindGatheringSpot(organizer, def, ignoreRequiredColonistCount: false, out spot);
        }

        public override LordJob CreateLordJob(IntVec3 spot, Pawn organizer) // we don't use it, CreateLordJobCustom is used instead with custom arguments;
        {
            throw new NotImplementedException();
        }
    }
}

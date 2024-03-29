﻿using RimWorld;
using Verse;

namespace VanillaSocialInteractionsExpanded
{
	public class GatheringWorker_Dating : GatheringWorker_DoublePawn
	{
		public override bool CanExecute(Map map, Pawn organizer = null)
		{
			if (!VanillaSocialInteractionsExpandedSettings.EnableDating)
			{
				return false;
			}
			return base.CanExecute(map, organizer);
		}
		protected override bool MemberValidator(Pawn pawn)
		{
			return !VSIE_Utils.workTags.Contains(pawn.mindState.lastJobTag);
		}
		protected override bool PawnsCanGatherTogether(Pawn organizer, Pawn companion)
		{
			return BasicLovePartnerRelationGenerationChance(organizer, companion) != 0f
				&& organizer.DevelopmentalStage == DevelopmentalStage.Adult && companion.DevelopmentalStage == DevelopmentalStage.Adult
				&& companion.relations.OpinionOf(organizer) >= 0 && organizer.relations.OpinionOf(companion) >= 0
				&& organizer.relations.CompatibilityWith(companion) >= 1f && companion.relations.CompatibilityWith(organizer) >= 1f
				&& (!organizer.GetSpouses(false).Contains(companion) || Rand.Chance(0.7f)); // we reduce the chance of married couples a bit
		}

		public float BasicLovePartnerRelationGenerationChance(Pawn generated, Pawn other)
		{
			if (generated.ageTracker.AgeBiologicalYearsFloat < 14f)
			{
				return 0f;
			}
			if (other.ageTracker.AgeBiologicalYearsFloat < 14f)
			{
				return 0f;
			}
			if (generated.gender == other.gender && (!other.story.traits.HasTrait(TraitDefOf.Gay)))
			{
				return 0f;
			}
			if (generated.gender != other.gender && other.story.traits.HasTrait(TraitDefOf.Gay))
			{
				return 0f;
			}
			return 1f;
		}
		protected override float SortCandidatesBy(Pawn organizer, Pawn candidate)
		{
			return organizer.relations.OpinionOf(candidate);
		}
	}
}
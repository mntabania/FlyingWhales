using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;
using UnityEngine.Assertions;
using UtilityScripts;

public class AbductorBehaviour : CharacterBehaviourComponent {
	public AbductorBehaviour() {
		priority = 30;
	}

	#region Overrides
	public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob) {
		TIME_IN_WORDS currentTimeInWords = GameManager.GetCurrentTimeInWordsOfTick();
		//if nest is blocked, destroy anything that is occupying it.
		if (character.behaviourComponent.IsNestBlocked(out var blocker)) {
			return character.jobComponent.TriggerDestroy(blocker, out producedJob);
		}
		if (character.behaviourComponent.AlreadyHasAbductedVictimAtNest(out var abductedCharacter)) {
			//if already has an abducted victim at nest
			if (abductedCharacter.isDead) {
				//check if target is alive, if not, create a job to move corpse to a nearby location.
				GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.MONSTER_ABDUCT,
					INTERACTION_TYPE.DROP, abductedCharacter, character);
				job.SetCannotBePushedBack(true);
				LocationGridTile targetTile = character.behaviourComponent.nest.GetNearestUnoccupiedTileFromThis();
				Assert.IsNotNull(targetTile);
				job.AddOtherData(INTERACTION_TYPE.DROP, new object[] {targetTile.structure, targetTile});
				producedJob = job;
				return true;
			} else {
				//else, check time of day
				//if it is around morning or night, check if character should eat
				bool willEat = false;
				switch (currentTimeInWords) {
					case TIME_IN_WORDS.MORNING:
					case TIME_IN_WORDS.AFTERNOON:
					case TIME_IN_WORDS.LUNCH_TIME:
						willEat = character.behaviourComponent.hasEatenInTheMorning == false;
						break;
					case TIME_IN_WORDS.EARLY_NIGHT:
					case TIME_IN_WORDS.LATE_NIGHT:
						willEat = character.behaviourComponent.hasEatenInTheNight == false;
						break;
				}
				if (willEat && GameUtilities.RollChance(40)) {
					//if character has not yet eaten for the current time, then perform eat alive action towards the abducted target
					return character.jobComponent.TriggerEatAlive(abductedCharacter, out producedJob);
				} else {
					//if character has already eaten, trigger roam around territory.
					return character.jobComponent.TriggerRoamAroundTerritory(out producedJob);
				}
			}
		} else {
			//if has no abducted victim at nest, check current time
			if (currentTimeInWords == TIME_IN_WORDS.AFTER_MIDNIGHT) {
				List<Character> validTargets = GetValidAbductTargets(character);
				if (validTargets != null) {
					//if it is after midnight pick a random villager or docile animal to abduct
					Character chosenTarget = CollectionUtilities.GetRandomElement(validTargets);
					return character.jobComponent.TriggerMonsterAbduct(chosenTarget, out producedJob, character.behaviourComponent.nest);
				} else {
					//if no target was found, trigger roam around territory
					return character.jobComponent.TriggerRoamAroundTerritory(out producedJob);
				}
			} else {
				//if it is NOT yet after midnight, trigger roam around territory
				return character.jobComponent.TriggerRoamAroundTerritory(out producedJob);
			}
		}
	}
	public override void OnAddBehaviourToCharacter(Character character) {
		base.OnAddBehaviourToCharacter(character);
		character.behaviourComponent.OnBecomeAbductor();
	}
	public override void OnRemoveBehaviourFromCharacter(Character character) {
		base.OnRemoveBehaviourFromCharacter(character);
		character.behaviourComponent.OnNoLongerAbductor();
	}
	#endregion
	
	private List<Character> GetValidAbductTargets(Character abductor) {
		List<Character> validTargets = null;
		for (int i = 0; i < abductor.currentRegion.charactersAtLocation.Count; i++) {
			Character character = abductor.currentRegion.charactersAtLocation[i];
			bool isValidTarget = character is Animal ||
			                     (character.isNormalCharacter && character.isDead == false);
			if (isValidTarget) {
				if (validTargets == null) {
					validTargets = new List<Character>();
				}
				validTargets.Add(character);
			}
		}
		return validTargets;
	}
}

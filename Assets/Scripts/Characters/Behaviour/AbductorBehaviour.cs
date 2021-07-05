using System.Collections.Generic;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using UnityEngine;
using UnityEngine.Assertions;
using UtilityScripts;

public class AbductorBehaviour : CharacterBehaviourComponent {
	public AbductorBehaviour() {
		priority = 30;
	}

	#region Overrides
	public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob) {
		TIME_IN_WORDS currentTimeInWords = GameManager.Instance.GetCurrentTimeInWordsOfTick();
		//if nest is blocked, destroy anything that is occupying it.
		if (character.behaviourComponent.IsNestBlocked(out var blocker)) {
			return character.jobComponent.TriggerDestroy(blocker, out producedJob);
		}
		if (character.behaviourComponent.AlreadyHasAbductedVictimAtNest(out var abductedCharacter)) {
			//if already has an abducted victim at nest
			if (abductedCharacter.isDead) {
				//check if target is alive, if not, create a job to move corpse to a nearby location.
				GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.MOVE_CHARACTER, INTERACTION_TYPE.DROP, abductedCharacter, character);
				job.SetCannotBePushedBack(true);
				LocationGridTile targetTile = character.behaviourComponent.nest.GetFirstNearestTileFromThisWithNoObject(exception: character.behaviourComponent.nest);
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
				Character validTarget = GetRandomValidAbductTarget(character);
				if (validTarget != null) {
					//if it is after midnight pick a random villager or docile animal to abduct
					return character.jobComponent.TriggerMonsterAbduct(validTarget, out producedJob, character.behaviourComponent.nest);
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
		character.movementComponent.SetEnableDigging(true);
		character.behaviourComponent.OnBecomeAbductor();
	}
	public override void OnRemoveBehaviourFromCharacter(Character character) {
		base.OnRemoveBehaviourFromCharacter(character);
		character.movementComponent.SetEnableDigging(false);
		character.behaviourComponent.OnNoLongerAbductor();
	}
	public override void OnLoadBehaviourToCharacter(Character character) {
		base.OnLoadBehaviourToCharacter(character);
		character.behaviourComponent.OnBecomeAbductor();
	}
	#endregion
	
	private Character GetRandomValidAbductTarget(Character abductor) {
		List<Character> validTargets = ObjectPoolManager.Instance.CreateNewCharactersList();
        Character chosenTarget = null;
        for (int i = 0; i < abductor.currentRegion.charactersAtLocation.Count; i++) {
			Character character = abductor.currentRegion.charactersAtLocation[i];
			bool isValidTarget = (character is Animal ||
			                     (character.isNormalCharacter && character.isDead == false && 
			                      character.traitContainer.HasTrait("Resting")) && character.currentStructure is Kennel == false)
								  && !character.isAlliedWithPlayer; //Those who are allied with player should not be targeted by abductors
			if (isValidTarget) {
				validTargets.Add(character);
			}
		}
        if(validTargets.Count > 0) {
            chosenTarget = CollectionUtilities.GetRandomElement(validTargets);
        }
        ObjectPoolManager.Instance.ReturnCharactersListToPool(validTargets);
        return chosenTarget;
	}
}

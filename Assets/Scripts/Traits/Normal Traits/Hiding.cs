using UnityEngine;
using Random = UnityEngine.Random;
namespace Traits {
	public class Hiding : Trait {

		private Character _owner;
		
		public Hiding() {
			name = "Hiding";
			description = "This character hiding from something.";
			type = TRAIT_TYPE.STATUS;
			effect = TRAIT_EFFECT.NEGATIVE;
			ticksDuration = GameManager.Instance.GetTicksBasedOnHour(12);
			isHidden = true;
		}

		#region Overrides
		public override void OnAddTrait(ITraitable addedTo) {
			base.OnAddTrait(addedTo);
			if (addedTo is Character character) {
				_owner = character;
				_owner.CancelAllJobs();
				(_owner.jobTriggerComponent as CharacterJobTriggerComponent).CreateHideAtHomeJob();
				// _owner.DecreaseCanWitness();
				Messenger.AddListener<Character, GoapPlanJob>(Signals.CHARACTER_FINISHED_JOB_SUCCESSFULLY, OnCharacterFinishedJobSuccessfully);
			}
		}
		private void OnCharacterFinishedJobSuccessfully(Character character, GoapPlanJob job) {
			if (character == _owner && job.jobType == JOB_TYPE.HIDE_AT_HOME) {
				character.logComponent.PrintLogIfActive($"{GameManager.Instance.TodayLogString()}{character.name} has successfully finished hide at home job! Will now start cowering check...");
				Messenger.RemoveListener<Character, GoapPlanJob>(Signals.CHARACTER_FINISHED_JOB_SUCCESSFULLY, OnCharacterFinishedJobSuccessfully);
				OnArriveAtHome();
			}
		}
		private void OnArriveAtHome() {
			Debug.Log($"{GameManager.Instance.TodayLogString()}{_owner.name} has arrived at {_owner.currentStructure.GetNameRelativeTo(_owner)}");
			// _owner.IncreaseCanWitness();
			_owner.trapStructure.SetForcedStructure(_owner.currentStructure);
			_owner.DecreaseCanTakeJobs();
			StartCheckingForCowering();
		}
		
		public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy) {
			base.OnRemoveTrait(removedFrom, removedBy);
			if (removedFrom is Character character) {
				StopCheckingForCowering();
				character.trapStructure.SetForcedStructure(null);
				_owner.IncreaseCanTakeJobs();
				character.needsComponent.CheckExtremeNeeds();
			}
		}
		#endregion

		#region Cowering
		private void StartCheckingForCowering() {	
			Messenger.AddListener(Signals.HOUR_STARTED, CoweringCheck);
		}
		private void CoweringCheck() {
			string summary = $"{GameManager.Instance.TodayLogString()}{_owner.name} is rolling for cower.";
			int roll = Random.Range(0, 100);
			int chance = 50;
			summary += $"\nRoll is {roll.ToString()}. Chance is {chance.ToString()}";
			if (roll < chance) {
				summary += $"\nChance met, triggering cowering interrupt";
				_owner.interruptComponent.TriggerInterrupt(INTERRUPT.Cowering, _owner);
			}
			Debug.Log(summary);
		}
		private void StopCheckingForCowering() {
			Messenger.RemoveListener(Signals.HOUR_STARTED, CoweringCheck);
		}
		#endregion
	}
}
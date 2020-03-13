using UnityEngine;
using Random = UnityEngine.Random;
namespace Traits {
	public class Hiding : Status {

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
				character.behaviourComponent.AddBehaviourComponent(typeof(DesiresIsolationBehaviour));
				StartCheckingForCowering();
			}
		}
		public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy) {
			base.OnRemoveTrait(removedFrom, removedBy);
			if (removedFrom is Character character) {
				StopCheckingForCowering();
				character.behaviourComponent.RemoveBehaviourComponent(typeof(DesiresIsolationBehaviour));
				character.needsComponent.CheckExtremeNeeds();
			}
		}
		#endregion

		#region Cowering
		private void StartCheckingForCowering() {	
			Messenger.AddListener(Signals.HOUR_STARTED, CoweringCheck);
		}
		private void CoweringCheck() {
			if (_owner.canPerform == false) { return; }
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
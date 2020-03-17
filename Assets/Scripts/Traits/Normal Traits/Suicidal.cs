using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Traits {
	public class Suicidal : Status {

		private Character _owner;
		
		public Suicidal() {
			name = "Suicidal";
			description = "This character wants to kill himself/herself.";
			type = TRAIT_TYPE.STATUS;
			effect = TRAIT_EFFECT.NEGATIVE;
			ticksDuration = GameManager.Instance.GetTicksBasedOnHour(24);
            AddTraitOverrideFunctionIdentifier(TraitManager.Tick_Started_Trait);
        }

        #region Overrides
        public override void OnAddTrait(ITraitable addedTo) {
			base.OnAddTrait(addedTo);
			if (addedTo is Character) {
				Character character = addedTo as Character;
				_owner = character;
				character.behaviourComponent.ReplaceBehaviourComponent(typeof(DefaultAtHome),
					typeof(SuicidalBehaviour));
			}
		}
		public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy) {
			base.OnRemoveTrait(removedFrom, removedBy);
			if (removedFrom is Character) {
				Character character = removedFrom as Character;
				character.behaviourComponent.ReplaceBehaviourComponent(typeof(SuicidalBehaviour),
					typeof(DefaultAtHome));
			}
		}
		public override void OnTickStarted() {
			base.OnTickStarted();
			if (_owner.currentActionNode != null && 
			    _owner.currentActionNode.associatedJobType == JOB_TYPE.COMMIT_SUICIDE) {
				CheckForChaosOrb();
			}
		}
		#endregion
		
		#region Chaos Orb
		private void CheckForChaosOrb() {
			string summary = $"{_owner.name} is rolling for chaos orb in suicidal trait";
			int roll = Random.Range(0, 100);
			int chance = 60;
			summary += $"\nRoll is {roll.ToString()}. Chance is {chance.ToString()}";
			if (roll < chance) {
				Messenger.Broadcast(Signals.CREATE_CHAOS_ORBS, _owner.marker.transform.position, 
					1, _owner.currentRegion.innerMap);
			}
			_owner.logComponent.PrintLogIfActive(summary);
		}
		#endregion
	}	
}


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Traits {
	public class Suicidal : Status {
		public Suicidal() {
			name = "Suicidal";
			description = "Might end up killing itself anytime soon.";
			type = TRAIT_TYPE.STATUS;
			effect = TRAIT_EFFECT.NEGATIVE;
			hindersSocials = true;
			ticksDuration = GameManager.Instance.GetTicksBasedOnHour(24);
            //AddTraitOverrideFunctionIdentifier(TraitManager.Tick_Started_Trait);
        }

        #region Overrides
        public override void OnAddTrait(ITraitable addedTo) {
			base.OnAddTrait(addedTo);
			if (addedTo is Character) {
				Character character = addedTo as Character;
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
		//public override void OnTickStarted() {
		//	base.OnTickStarted();
		//	if (_owner.currentActionNode != null && 
		//	    _owner.currentActionNode.associatedJobType == JOB_TYPE.COMMIT_SUICIDE) {
		//		CheckForChaosOrb();
		//	}
		//}
		#endregion
	}	
}


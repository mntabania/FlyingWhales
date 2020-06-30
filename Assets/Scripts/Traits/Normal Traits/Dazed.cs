using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Traits {
    public class Dazed : Status {
        // private List<CharacterBehaviourComponent> _behaviourComponentsBeforeDazed;
        public override bool isSingleton => true;

        public Dazed() {
            name = "Dazed";
            description = "This character will attack anyone at random and may destroy objects.";
            thoughtText = "This character is Dazed.";
            type = TRAIT_TYPE.STATUS;
            effect = TRAIT_EFFECT.NEGATIVE;
            ticksDuration = GameManager.Instance.GetTicksBasedOnHour(6);
            hindersWitness = true;
            // hindersPerform = true;
        }

        #region Overrides
        public override void OnAddTrait(ITraitable addedTo) {
            base.OnAddTrait(addedTo);
            if (addedTo is Character) {
                Character character = addedTo as Character;
                character.CancelAllJobs();
                // _behaviourComponentsBeforeDazed = new List<CharacterBehaviourComponent>(character.behaviourComponent.currentBehaviourComponents);
                // character.behaviourComponent.ReplaceBehaviourComponent(new List<CharacterBehaviourComponent>()
                //     {CharacterManager.Instance.GetCharacterBehaviourComponent(typeof(DazedBehaviour))});
                character.behaviourComponent.AddBehaviourComponent(typeof(DazedBehaviour));
            }
        }
        public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy) {
            base.OnRemoveTrait(removedFrom, removedBy);
            if (removedFrom is Character) {
                Character character = removedFrom as Character;
                // character.behaviourComponent.ReplaceBehaviourComponent(_behaviourComponentsBeforeDazed);
                // _behaviourComponentsBeforeDazed.Clear();
                character.behaviourComponent.RemoveBehaviourComponent(typeof(DazedBehaviour));
                character.needsComponent.CheckExtremeNeeds();
            }
        }
        #endregion
    }
}

﻿using System.Collections.Generic;
using System.Linq;
using Inner_Maps.Location_Structures;
using UtilityScripts;

namespace Traits {
    public class Cultist : Trait {
        
        public Cultist() {
            name = "Cultist";
            description = "This character is a cultist.";
            type = TRAIT_TYPE.FLAW;
            effect = TRAIT_EFFECT.NEGATIVE;
            ticksDuration = 0;
            canBeTriggered = true;
            AddTraitOverrideFunctionIdentifier(TraitManager.Death_Trait);
        }

        #region Overrides
        public override void OnAddTrait(ITraitable sourceCharacter) {
            base.OnAddTrait(sourceCharacter);
            if (sourceCharacter is Character character) {
                character.behaviourComponent.AddBehaviourComponent(typeof(CultistBehaviour));
                character.SetIsAlliedWithPlayer(true);
                character.AddItemAsInteresting("Cultist Kit");
            }
            
        }
        public override void OnRemoveTrait(ITraitable sourceCharacter, Character removedBy) {
            base.OnRemoveTrait(sourceCharacter, removedBy);
            if (sourceCharacter is Character character) {
                character.behaviourComponent.RemoveBehaviourComponent(typeof(CultistBehaviour));
                character.SetIsAlliedWithPlayer(false);
                character.RemoveItemAsInteresting("Cultist Kit");
            }
        }
        public override bool OnDeath(Character character) {
            character.traitContainer.RemoveTrait(character, this);
            return base.OnDeath(character);
        }
        public override string TriggerFlaw(Character character) {
            CultistBehaviour cultistBehaviour =
                CharacterManager.Instance.GetCharacterBehaviourComponent<CultistBehaviour>(typeof(CultistBehaviour));
            string log = string.Empty;
            if (cultistBehaviour.TryCreateCultistJob(character, ref log, out var producedJob)) {
                character.jobQueue.AddJobInQueue(producedJob);
                return base.TriggerFlaw(character);
            } else {
                return "no_job";
            }
        }
        #endregion
    }
}

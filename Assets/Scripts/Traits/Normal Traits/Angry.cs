using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UtilityScripts;
namespace Traits {
    public class Angry : Status {

        private Character owner;
        public Angry() {
            name = "Angry";
            description = "This character will often argue with others and may destroy objects.";
            type = TRAIT_TYPE.STATUS;
            effect = TRAIT_EFFECT.NEUTRAL;
            ticksDuration = GameManager.Instance.GetTicksBasedOnHour(8);
            moodEffect = -3;
            isStacking = true;
            stackLimit = 5;
            stackModifier = 0.5f;
            AddTraitOverrideFunctionIdentifier(TraitManager.See_Poi_Trait);
            //effects = new List<TraitEffect>();
        }

        #region Overrides
        public override void OnAddTrait(ITraitable addedTo) {
            base.OnAddTrait(addedTo);
            if (addedTo is Character character) {
                owner = character;
                character.marker.visionCollider.VoteToUnFilterVision();
                Messenger.AddListener(Signals.HOUR_STARTED, PerHourEffect);
            }
        }
        public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy) {
            base.OnRemoveTrait(removedFrom, removedBy);
            if (removedFrom is Character character) {
                owner = null;
                character.marker.visionCollider.VoteToFilterVision();
                Messenger.RemoveListener(Signals.HOUR_STARTED, PerHourEffect);
            }
        }
        public override bool OnSeePOI(IPointOfInterest targetPOI, Character characterThatWillDoJob) {
            //if (targetPOI is TileObject tileObject) { // || targetPOI is SpecialToken
            //    if (tileObject.mapObjectVisual.IsInvisible() == false && 
            //        tileObject.tileObjectType != TILE_OBJECT_TYPE.GENERIC_TILE_OBJECT && 
            //        Random.Range(0, 100) < 3) {
            //        return characterThatWillDoJob.jobComponent.TriggerDestroy(targetPOI);
            //    }
            //} else 
            if (targetPOI is Character) {
                Character targetCharacter = targetPOI as Character;
                if (Random.Range(0, 100) < 10 && characterThatWillDoJob.relationshipContainer.IsEnemiesWith(targetCharacter)
                    && !targetCharacter.traitContainer.HasTrait("Unconscious")) {
                    characterThatWillDoJob.combatComponent.Fight(targetCharacter, CombatManager.Anger, isLethal: false);
                }
            }
            return base.OnSeePOI(targetPOI, characterThatWillDoJob);
        }
        #endregion
        
        private void PerHourEffect() {
            if (owner.canPerform && owner.canMove && !owner.isDead && owner != null && owner.marker != null && owner.marker.inVisionTileObjects.Count > 0 && Random.Range(0, 100) < 8) {
                List<TileObject> choices = owner.marker.inVisionTileObjects
                    .Where(x => x.tileObjectType != TILE_OBJECT_TYPE.GENERIC_TILE_OBJECT).ToList();
                if (choices.Count > 0 && owner.jobQueue.HasJob(JOB_TYPE.DESTROY) == false) {
                    TileObject tileObject = CollectionUtilities.GetRandomElement(choices);
                    owner.jobComponent.TriggerDestroy(tileObject);
                }
            }
        }
    }
}


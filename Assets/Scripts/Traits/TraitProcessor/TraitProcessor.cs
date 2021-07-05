using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Traits {
    public abstract class TraitProcessor {
        public abstract void OnTraitAdded(ITraitable traitable, Trait trait, Character characterResponsible, int overrideDuration);
        public abstract void OnTraitRemoved(ITraitable traitable, Trait trait, Character removedBy = null);
        public abstract void OnStatusStacked(ITraitable traitable, Status status, Character characterResponsible, int overrideDuration);
        public abstract void OnStatusUnstack(ITraitable traitable, Status status, Character removedBy = null);

        protected void DefaultProcessOnAddTrait(ITraitable traitable, Trait trait, Character characterResponsible, int overrideDuration) {
            //trait.SetOnRemoveAction(onRemoveAction);
            trait.AddCharacterResponsibleForTrait(characterResponsible);
            ApplyPOITraitInteractions(traitable, trait);
            //traitable.traitContainer.SwitchOnTrait(trait.name);
            trait.OnAddTrait(traitable);
            
            int duration = overrideDuration;
            if (duration == -1) { duration = trait.ticksDuration; } //if no override duration was given(-1), then use the default trait duration
            GameDate removeDate = default;
            if (duration > 0) {
                //traitable.traitContainer.currentDurations.Add(trait, 0);
                removeDate = GameManager.Instance.Today();
                removeDate.AddTicks(duration);
                string ticket = SchedulingManager.Instance.AddEntry(removeDate, () => traitable.traitContainer.RemoveTraitOnSchedule(traitable, trait), this);
                traitable.traitContainer.AddScheduleTicket(trait.name, ticket, removeDate);
                //trait.SetExpiryTicket(traitable, ticket);
            }
            trait.ApplyMoodEffects(traitable, removeDate, characterResponsible);
            if(trait.traitOverrideFunctionIdentifiers != null && trait.traitOverrideFunctionIdentifiers.Count > 0) {
                for (int i = 0; i < trait.traitOverrideFunctionIdentifiers.Count; i++) {
                    string identifier = trait.traitOverrideFunctionIdentifiers[i];
                    traitable.traitContainer.AddTraitOverrideFunction(identifier, trait);
                }
            }
            //if (trait.hasOnCollideWith) {
            //    traitable.traitContainer.AddOnCollideWithTrait(trait);
            //}
            //if (trait.hasOnEnterGridTile) {
            //    traitable.traitContainer.AddOnEnterGridTileTrait(trait);
            //}
            if (traitable is Character character) {
                character.eventDispatcher.ExecuteCharacterGainedTrait(character, trait);
            } else if (traitable is TileObject tileObject) {
                tileObject.eventDispatcher.ExecuteTileObjectGainedTrait(tileObject, trait);
            }
            Messenger.Broadcast(TraitSignals.TRAITABLE_GAINED_TRAIT, traitable, trait);
        }
        protected void DefaultProcessOnRemoveTrait(ITraitable traitable, Trait trait, Character removedBy) {
            // traitable.traitContainer.RemoveScheduleTicket(trait.name, bySchedule);
            //trait.RemoveExpiryTicket(traitable);
            //traitable.traitContainer.SwitchOffTrait(trait.name);
            UnapplyPOITraitInteractions(traitable, trait);
            trait.OnRemoveTrait(traitable, removedBy);
            trait.UnapplyMoodEffects(traitable);
            if (trait.traitOverrideFunctionIdentifiers != null && trait.traitOverrideFunctionIdentifiers.Count > 0) {
                for (int i = 0; i < trait.traitOverrideFunctionIdentifiers.Count; i++) {
                    string identifier = trait.traitOverrideFunctionIdentifiers[i];
                    traitable.traitContainer.RemoveTraitOverrideFunction(identifier, trait);
                }
            }

            //if (trait.hasOnCollideWith) {
            //    traitable.traitContainer.RemoveOnCollideWithTrait(trait);
            //}
            //if (trait.hasOnEnterGridTile) {
            //    traitable.traitContainer.RemoveOnEnterGridTileTrait(trait);
            //}
            if (traitable is Character character) {
                character.eventDispatcher.ExecuteCharacterLostTrait(character, trait, removedBy);
            } else if (traitable is TileObject tileObject) {
                tileObject.eventDispatcher.ExecuteTileObjectLostTrait(tileObject, trait);
            }
            Messenger.Broadcast(TraitSignals.TRAITABLE_LOST_TRAIT, traitable, trait, removedBy);
        }
        protected bool DefaultProcessOnStackStatus(ITraitable traitable, Status status, Character characterResponsible, int overrideDuration) {
            int duration = overrideDuration;
            if(duration == -1) { duration = status.ticksDuration; }
            GameDate removeDate = default;
            if (duration > 0) {
                //traitable.traitContainer.currentDurations[trait] = 0;
                removeDate = GameManager.Instance.Today();
                removeDate.AddTicks(duration);
                string ticket = SchedulingManager.Instance.AddEntry(removeDate, () => traitable.traitContainer.RemoveTraitOnSchedule(traitable, status), this);
                traitable.traitContainer.AddScheduleTicket(status.name, ticket, removeDate);
                //trait.SetExpiryTicket(traitable, ticket);
            }
            if(traitable.traitContainer.stacks[status.name] <= status.stackLimit) {
                status.AddCharacterResponsibleForTrait(characterResponsible);
                status.OnStackStatus(traitable);
                status.ApplyStackedMoodEffect(traitable, removeDate, characterResponsible);
                return true;
            } else {
                status.OnStackStatusAddedButStackIsAtLimit(traitable);
            }
            return false;
        }
        protected void DefaultProcessOnUnstackStatus(ITraitable traitable, Status status, Character removedBy) {
            //trait.RemoveExpiryTicket(traitable);
            // traitable.traitContainer.RemoveScheduleTicket(trait.name, bySchedule);
            if (traitable.traitContainer.stacks[status.name] < status.stackLimit) {
                status.OnUnstackStatus(traitable);
                status.UnapplyStackedMoodEffect(traitable);
            }
        }
        private void ApplyPOITraitInteractions(ITraitable traitable, Trait trait) {
            if (trait.advertisedInteractions != null) {
                for (int i = 0; i < trait.advertisedInteractions.Count; i++) {
                    //NOTE: Did this to allow duplicates
                    //traitable.advertisedActions.Add(trait.advertisedInteractions[i]);
                    traitable.AddAdvertisedAction(trait.advertisedInteractions[i], true);
                }
            }
            if(traitable.advertisedActions != null && traitable.advertisedActions.Count > 0 && traitable is GenericTileObject genericObj) {
                UtilityScripts.LocationAwarenessUtility.AddToAwarenessList(genericObj, traitable.gridTileLocation);
                //removed by aaron for awareness update traitable.gridTileLocation.parentMap.region.AddPendingAwareness(genericObj);
            }
        }
        private void UnapplyPOITraitInteractions(ITraitable traitable, Trait trait) {
            if (trait.advertisedInteractions != null) {
                for (int i = 0; i < trait.advertisedInteractions.Count; i++) {
                    //traitable.advertisedActions.Remove(trait.advertisedInteractions[i]);
                    traitable.RemoveAdvertisedAction(trait.advertisedInteractions[i]);
                }
            }
            if ((traitable.advertisedActions == null || traitable.advertisedActions.Count <= 0) && traitable is GenericTileObject genericObj) {
                UtilityScripts.LocationAwarenessUtility.RemoveFromAwarenessList(genericObj);
                //removed by aaron aranas awareness update traitable.gridTileLocation.parentMap.region.RemovePendingAwareness(genericObj);
            }
        }
    }
}


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Traits {
    public abstract class TraitProcessor {
        public abstract void OnTraitAdded(ITraitable traitable, Trait trait, Character characterResponsible, ActualGoapNode gainedFromDoing, int overrideDuration);
        public abstract void OnTraitRemoved(ITraitable traitable, Trait trait, Character removedBy = null);
        public abstract void OnStatusStacked(ITraitable traitable, Status status, Character characterResponsible, ActualGoapNode gainedFromDoing, int overrideDuration);
        public abstract void OnStatusUnstack(ITraitable traitable, Status status, Character removedBy = null);

        protected void DefaultProcessOnAddTrait(ITraitable traitable, Trait trait, Character characterResponsible, ActualGoapNode gainedFromDoing, int overrideDuration) {
            trait.SetGainedFromDoing(gainedFromDoing);
            //trait.SetOnRemoveAction(onRemoveAction);
            trait.AddCharacterResponsibleForTrait(characterResponsible);
            ApplyPOITraitInteractions(traitable, trait);
            traitable.traitContainer.SwitchOnTrait(trait.name);
            trait.OnAddTrait(traitable);

            int duration = overrideDuration;
            if (duration == -1) { duration = trait.ticksDuration; }
            if (duration > 0) {
                //traitable.traitContainer.currentDurations.Add(trait, 0);
                GameDate removeDate = GameManager.Instance.Today();
                removeDate.AddTicks(duration);
                string ticket = SchedulingManager.Instance.AddEntry(removeDate, () => traitable.traitContainer.RemoveTraitOnSchedule(traitable, trait), this);
                traitable.traitContainer.AddScheduleTicket(trait.name, ticket, removeDate);
                //trait.SetExpiryTicket(traitable, ticket);
            }
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
            Messenger.Broadcast(Signals.TRAITABLE_GAINED_TRAIT, traitable, trait);
        }
        protected void DefaultProcessOnRemoveTrait(ITraitable traitable, Trait trait, Character removedBy) {
            // traitable.traitContainer.RemoveScheduleTicket(trait.name, bySchedule);
            //trait.RemoveExpiryTicket(traitable);
            //TODO: if (triggerOnRemove) {
            //    trait.OnRemoveTrait(this, removedBy);
            //}
            traitable.traitContainer.SwitchOffTrait(trait.name);
            UnapplyPOITraitInteractions(traitable, trait);
            trait.OnRemoveTrait(traitable, removedBy);

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
            Messenger.Broadcast(Signals.TRAITABLE_LOST_TRAIT, traitable, trait, removedBy);
        }
        protected bool DefaultProcessOnStackStatus(ITraitable traitable, Status status, Character characterResponsible, ActualGoapNode gainedFromDoing, int overrideDuration) {
            int duration = overrideDuration;
            if(duration == -1) { duration = status.ticksDuration; }
            if (duration > 0) {
                //traitable.traitContainer.currentDurations[trait] = 0;
                GameDate removeDate = GameManager.Instance.Today();
                removeDate.AddTicks(duration);
                string ticket = SchedulingManager.Instance.AddEntry(removeDate, () => traitable.traitContainer.RemoveTraitOnSchedule(traitable, status), this);
                traitable.traitContainer.AddScheduleTicket(status.name, ticket, removeDate);
                //trait.SetExpiryTicket(traitable, ticket);
            }
            if(traitable.traitContainer.stacks[status.name] <= status.stackLimit) {
                status.SetGainedFromDoing(gainedFromDoing);
                status.AddCharacterResponsibleForTrait(characterResponsible);
                status.OnStackStatus(traitable);
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
            }
        }
        private void ApplyPOITraitInteractions(ITraitable traitable, Trait trait) {
            if (trait.advertisedInteractions != null) {
                for (int i = 0; i < trait.advertisedInteractions.Count; i++) {
                    traitable.AddAdvertisedAction(trait.advertisedInteractions[i]);
                }
            }
        }
        private void UnapplyPOITraitInteractions(ITraitable traitable, Trait trait) {
            if (trait.advertisedInteractions != null) {
                for (int i = 0; i < trait.advertisedInteractions.Count; i++) {
                    traitable.RemoveAdvertisedAction(trait.advertisedInteractions[i]);
                }
            }
        }
    }
}


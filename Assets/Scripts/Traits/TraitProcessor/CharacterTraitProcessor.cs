using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
namespace Traits {
    /// <summary>
    /// Functions to be used to determine what happens when a trait is added/removed to a character
    /// </summary>
    public class CharacterTraitProcessor : TraitProcessor {
        public override void OnTraitAdded(ITraitable traitable, Trait trait, Character characterResponsible, int overrideDuration) {
            Character character = traitable as Character;
            Assert.IsNotNull(character);
            if(trait is Status status) {
                ApplyStatusEffects(character, status);
            }
            ApplyTraitEffects(character, trait);
            Messenger.Broadcast(JobSignals.CHECK_APPLICABILITY_OF_ALL_JOBS_TARGETING, character as IPointOfInterest);
            //ApplyPOITraitInteractions(character, trait);
            //character.currentAlterEgo.AddTrait(trait);

            if (GameManager.Instance.gameHasStarted) {
                if (trait.name == "Starving") {
                    character.needsComponent.PlanFullnessRecoveryActions();
                } else if (trait.name == "Sulking" || trait.name == "Lonely") {
                    // character.needsComponent.PlanHappinessRecoveryActions();
                } else if (trait.name == "Exhausted") {
                    character.needsComponent.PlanTirednessRecoveryActions();
                }
            }
            DefaultProcessOnAddTrait(traitable, trait, characterResponsible, overrideDuration);
            if (trait.affectsNameIcon) {
                character.bookmarkEventDispatcher.ExecuteBookmarkChangedNameOrElementsEvent(character);
                if (character.hasMarker) {
                    character.marker.UpdateName();
                }
            }
            Messenger.Broadcast(CharacterSignals.CHARACTER_TRAIT_ADDED, character, trait);
        }
        public override void OnTraitRemoved(ITraitable traitable, Trait trait, Character removedBy) {
            Character character = traitable as Character;
            if (trait is Status status) {
                UnapplyStatusEffects(character, status);
            }
            UnapplyTraitEffects(character, trait);
            Messenger.Broadcast(JobSignals.CHECK_APPLICABILITY_OF_ALL_JOBS_TARGETING, character as IPointOfInterest);
            //UnapplyPOITraitInteractions(character, trait);
            //character.currentAlterEgo.RemoveTrait(trait);

            DefaultProcessOnRemoveTrait(traitable, trait, removedBy);
            Messenger.Broadcast(CharacterSignals.CHARACTER_TRAIT_REMOVED, character, trait);
        }
        public override void OnStatusStacked(ITraitable traitable, Status status, Character characterResponsible, int overrideDuration) {
            Character character = traitable as Character;
            if(DefaultProcessOnStackStatus(traitable, status, characterResponsible, overrideDuration)) {
                Messenger.Broadcast(CharacterSignals.CHARACTER_TRAIT_STACKED, character, status.GetBase());
            }
        }
        public override void OnStatusUnstack(ITraitable traitable, Status status, Character removedBy = null) {
            Character character = traitable as Character;
            DefaultProcessOnUnstackStatus(traitable, status, removedBy);
            Messenger.Broadcast(CharacterSignals.CHARACTER_TRAIT_UNSTACKED, character, status.GetBase());
        }

        private void ApplyStatusEffects(Character character, Status status) {
            if (status.hindersWitness) {
                character.limiterComponent.DecreaseCanWitness();
            }
            if (status.hindersMovement) {
                character.limiterComponent.DecreaseCanMove();
            }
            if (status.hindersAttackTarget) {
                character.limiterComponent.DecreaseCanBeAttacked();
            }
            if (status.hindersPerform) {
                character.limiterComponent.DecreaseCanPerform();
            }
            if (status.hindersSocials) {
                character.limiterComponent.DecreaseSociable();
            }
            if (status.hindersFullnessRecovery) {
                character.limiterComponent.DecreaseCanDoFullnessRecovery();
            }
            if (status.hindersHappinessRecovery) {
                character.limiterComponent.DecreaseCanDoHappinessRecovery();
            }
            if (status.hindersTirednessRecovery) {
                character.limiterComponent.DecreaseCanDoTirednessRecovery();
            }
        }
        private void ApplyTraitEffects(Character character, Trait trait) {
            if (trait.resistancesType != null && trait.resistancesValue != null) {
                if (trait.resistancesType.Count != trait.resistancesValue.Count) {
                    Debug.LogError("Resistances does not match for " + trait.name);
                } else {
                    for (int i = 0; i < trait.resistancesType.Count; i++) {
                        character.piercingAndResistancesComponent.AdjustResistance(trait.resistancesType[i], trait.resistancesValue[i]);
                    }
                }
            }
            if (trait.name == "Abducted" || trait.name == "Restrained") {
                character.needsComponent.AdjustDoNotGetTired(1);
            } else if (trait.name == "Packaged" || trait.name == "Hibernating" || trait.name == "Resting" || trait.name == "Unconscious" || trait.name == "Recuperating") {
                character.needsComponent.AdjustDoNotGetTired(1);
                character.needsComponent.AdjustDoNotGetHungry(1);
                character.needsComponent.AdjustDoNotGetBored(1);
            } else if (trait.name == "Charmed") {
                character.needsComponent.AdjustDoNotGetBored(1);
            } else if (trait.name == "Eating") {
                character.needsComponent.AdjustDoNotGetHungry(1);
            } else if (trait.name == "Daydreaming") {
                character.needsComponent.AdjustDoNotGetTired(1);
                character.needsComponent.AdjustDoNotGetBored(1);
            }  else if (trait.name == "Optimist") {
                character.needsComponent.AdjustHappinessDecreaseRate(-(EditableValuesManager.Instance.baseHappinessDecreaseRate * 0.5f)); //Reference: https://trello.com/c/Aw8kIbB1/2654-optimist
            } else if (trait.name == "Pessimist") {
                character.needsComponent.AdjustHappinessDecreaseRate((EditableValuesManager.Instance.baseHappinessDecreaseRate * 0.5f)); //Reference: https://trello.com/c/lcen0P9l/2653-pessimist
            } else if (trait.name == "Fast") {
                character.movementComponent.AdjustSpeedModifier(0.25f); //Reference: https://trello.com/c/Gb3kfZEm/2658-fast
            }
        }

        private void UnapplyStatusEffects(Character character, Status status) {
            if (status.hindersWitness) {
                character.limiterComponent.IncreaseCanWitness();
            }
            if (status.hindersMovement) {
                character.limiterComponent.IncreaseCanMove();
            }
            if (status.hindersAttackTarget) {
                character.limiterComponent.IncreaseCanBeAttacked();
            }
            if (status.hindersPerform) {
                character.limiterComponent.IncreaseCanPerform();
            }
            if (status.hindersSocials) {
                character.limiterComponent.IncreaseSociable();
            }
            if (status.hindersFullnessRecovery) {
                character.limiterComponent.IncreaseCanDoFullnessRecovery();
            }
            if (status.hindersHappinessRecovery) {
                character.limiterComponent.IncreaseCanDoHappinessRecovery();
            }
            if (status.hindersTirednessRecovery) {
                character.limiterComponent.IncreaseCanDoTirednessRecovery();
            }
        }
        public void UnapplyTraitEffects(Character character, Trait trait) {
            if (trait.resistancesType != null && trait.resistancesValue != null) {
                if (trait.resistancesType.Count != trait.resistancesValue.Count) {
                    Debug.LogError("Resistances does not match for " + trait.name);
                } else {
                    for (int i = 0; i < trait.resistancesType.Count; i++) {
                        character.piercingAndResistancesComponent.AdjustResistance(trait.resistancesType[i], -trait.resistancesValue[i]);
                    }
                }
            }
            if (trait.name == "Abducted" || trait.name == "Restrained") {
                character.needsComponent.AdjustDoNotGetTired(-1);
            } else if (trait.name == "Packaged" || trait.name == "Hibernating" || trait.name == "Resting" || trait.name == "Unconscious") {
                character.needsComponent.AdjustDoNotGetTired(-1);
                character.needsComponent.AdjustDoNotGetHungry(-1);
                character.needsComponent.AdjustDoNotGetBored(-1);
            } else if (trait.name == "Charmed") {
                character.needsComponent.AdjustDoNotGetBored(-1);
            } else if (trait.name == "Eating") {
                character.needsComponent.AdjustDoNotGetHungry(-1);
            } else if (trait.name == "Daydreaming") {
                character.needsComponent.AdjustDoNotGetTired(-1);
                character.needsComponent.AdjustDoNotGetBored(-1);
            } else if (trait.name == "Optimist") {
                character.needsComponent.AdjustHappinessDecreaseRate((EditableValuesManager.Instance.baseHappinessDecreaseRate * 0.5f)); //Reference: https://trello.com/c/Aw8kIbB1/2654-optimist
            } else if (trait.name == "Pessimist") {
                character.needsComponent.AdjustHappinessDecreaseRate(-(EditableValuesManager.Instance.baseHappinessDecreaseRate * 0.5f)); //Reference: https://trello.com/c/lcen0P9l/2653-pessimist
            } else if (trait.name == "Fast") {
                character.movementComponent.AdjustSpeedModifier(-0.25f); //Reference: https://trello.com/c/Gb3kfZEm/2658-fast
            }             
        }
    }
}


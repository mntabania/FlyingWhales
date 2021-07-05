using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Traits {
    public class Edible : Trait {

        private IPointOfInterest owner;

        public Edible() {
            name = "Edible";
            description = "Yummy.";
            type = TRAIT_TYPE.NEUTRAL;
            effect = TRAIT_EFFECT.NEUTRAL;
            ticksDuration = 0;
            advertisedInteractions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.EAT, INTERACTION_TYPE.POISON };
            AddTraitOverrideFunctionIdentifier(TraitManager.Execute_Per_Tick_Effect_Trait);
            AddTraitOverrideFunctionIdentifier(TraitManager.Execute_After_Effect_Trait);
        }

        #region Overrides
        public override void OnAddTrait(ITraitable addedTo) {
            base.OnAddTrait(addedTo);
            if (addedTo is IPointOfInterest poi) {
                owner = poi;
            }
        }
        public override void LoadTraitOnLoadTraitContainer(ITraitable addTo) {
            base.LoadTraitOnLoadTraitContainer(addTo);
            if (addTo is IPointOfInterest poi) {
                owner = poi;
            }
        }
        public override void ExecuteActionPerTickEffects(INTERACTION_TYPE action, ActualGoapNode goapNode) {
            base.ExecuteActionPerTickEffects(action, goapNode);
            if (action == INTERACTION_TYPE.EAT) {
                goapNode.actor.needsComponent.AdjustFullness(5f);
                //goapNode.actor.needsComponent.AdjustStamina(2f);
                if(owner is Table) {
                    goapNode.actor.needsComponent.AdjustHappiness(0.83f);
                    owner.resourceStorageComponent.ReduceMainResourceUsingRandomSpecificResources(RESOURCE.FOOD, 1);
                } else if (owner is FoodPile foodPile) {
                    goapNode.actor.needsComponent.AdjustHappiness(-0.415f);
                    foodPile.AdjustResourceInPile(-1);
                }
            }
        }
        public override void ExecuteCostModification(INTERACTION_TYPE action, Character actor, IPointOfInterest poiTarget, OtherData[] otherData, ref int cost) {
            base.ExecuteCostModification(action, actor, poiTarget, otherData, ref cost);
            if (action == INTERACTION_TYPE.EAT) {
                string edibleType = GetEdibleType();
                if (edibleType == "Meat") {
                    if (actor.traitContainer.HasTrait("Carnivore")) {
                        cost = 25;
                    } else {
                        cost = 50;
                    }
                } else if (edibleType == "Plant") {
                    if (actor.traitContainer.HasTrait("Herbivore")) {
                        cost = 25;
                    } else {
                        cost = 50;
                    }
                } else if (edibleType == "Table") {
                    Table table = owner as Table;
                    if (table.structureLocation.isDwelling) {
                        if (table.structureLocation == actor.homeStructure) {
                            cost = 12;
                        } else {
                            if (table.structureLocation.HasPositiveRelationshipWithAnyResident(actor)) {
                                cost = 18;
                            } else if (!table.structureLocation.IsOccupied()) {
                                cost = 28;
                            }
                        }
                    } else {
                        cost = 28;
                    }
                }
            }
        }
        public override void ExecuteActionAfterEffects(INTERACTION_TYPE action, Character actor, IPointOfInterest target, ACTION_CATEGORY category, ref bool isRemoved) {
            base.ExecuteActionAfterEffects(action, actor, target, category, ref isRemoved);
            if (action == INTERACTION_TYPE.EAT) {
                if (owner is Crops crops) {
                    crops.SetGrowthState(Crops.Growth_State.Growing);
                }
            }
        }
        #endregion
        

        private string GetEdibleType() {
            if (owner is Crops) {
                return "Plant";
            } else if (owner is Table) {
                return "Table";
            } else {
                return "Meat";
            }
        }
    }
}


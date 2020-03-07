using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Traits {
    public class Edible : Status {

        private IPointOfInterest owner;
        //private int fullnessProvided;

        public Edible() {
            name = "Edible";
            description = "This can be eaten.";
            type = TRAIT_TYPE.STATUS;
            effect = TRAIT_EFFECT.NEUTRAL;
            ticksDuration = 0;
            advertisedInteractions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.EAT };
        }

        #region Overrides
        public override void OnAddTrait(ITraitable addedTo) {
            base.OnAddTrait(addedTo);
            if (addedTo is IPointOfInterest) {
                IPointOfInterest poi = addedTo as IPointOfInterest;
                owner = poi;
                //if (poi is Mushroom) {
                //    fullnessProvided = 520;
                //} else if (poi is EdiblePlant) {
                //    fullnessProvided = 520;
                //} else if (poi is Table) {
                //    fullnessProvided = 585;
                //} else if (poi is SmallAnimal) {
                //    fullnessProvided = 520;
                //}

            }
        }
        //public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy) {
        //    base.OnRemoveTrait(removedFrom, removedBy);
        //    if (removedFrom is IPointOfInterest) {
        //        IPointOfInterest poi = removedFrom as IPointOfInterest;
        //        poi.RemoveAdvertisedAction(INTERACTION_TYPE.EAT);
        //    }
        //}
        public override void ExecuteActionPreEffects(INTERACTION_TYPE action, ActualGoapNode goapNode) {
            base.ExecuteActionPreEffects(action, goapNode);
            if (action == INTERACTION_TYPE.EAT) {
                if (owner is Mushroom || owner is EdiblePlant || owner is SmallAnimal) {
                    owner.SetPOIState(POI_STATE.INACTIVE);
                }
            }
        }
        public override void ExecuteActionPerTickEffects(INTERACTION_TYPE action, ActualGoapNode goapNode) {
            base.ExecuteActionPerTickEffects(action, goapNode);
            if (action == INTERACTION_TYPE.EAT) {
                goapNode.actor.needsComponent.AdjustFullness(8.5f);
                goapNode.actor.needsComponent.AdjustComfort(2f);
                if(owner is Table || owner is FoodPile) {
                    owner.AdjustResource(RESOURCE.FOOD, -1);
                }
            }
        }
        //public override void ExecuteActionAfterEffects(INTERACTION_TYPE action, ActualGoapNode goapNode, ref bool isRemoved) {
        //    base.ExecuteActionAfterEffects(action, goapNode, ref isRemoved);
        //    if (action == INTERACTION_TYPE.EAT) {
        //        OnDoneEating(goapNode);
        //    }
        //}
        public override void ExecuteCostModification(INTERACTION_TYPE action, Character actor, IPointOfInterest poiTarget, object[] otherData, ref int cost) {
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
                            IDwelling dwelling = table.structureLocation as IDwelling;
                            if (dwelling.HasPositiveRelationshipWithAnyResident(actor)) {
                                cost = 18;
                            } else if (!dwelling.IsOccupied()) {
                                cost = 28;
                            }
                        }
                    } else {
                        cost = 28;
                    }
                    
                }
               
            }
        }
        #endregion

        //private void OnDoneEating(ActualGoapNode goapNode) {
        //    if (owner is Table) {
        //        //**Per Tick Effect 2**: Reduce Dwelling Table Food by 20/Duration
        //        Table table = owner as Table;
        //        table.AdjustFood(-(20 * goapNode.currentState.duration));
        //    }
        //}

        private string GetEdibleType() {
            if (owner is EdiblePlant || owner is Mushroom) {
                return "Plant";
            } else if (owner is Table) {
                return "Table";
            } else {
                return "Meat";
            }
        }
    }
}


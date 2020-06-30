using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;

namespace Traits {
    public class MusicLover : Trait {
        public override bool isSingleton => true;
        //private Character owner;

        public MusicLover() {
            name = "Music Lover";
            description = "Music Lovers enjoy playing instruments and listening to music.";
            type = TRAIT_TYPE.BUFF;
            effect = TRAIT_EFFECT.POSITIVE;
            ticksDuration = 0;
            mutuallyExclusive = new string[] { "Music Hater" };
        }

        #region Overrides
        //public override void OnSeePOI(IPointOfInterest targetPOI, Character character) {
        //    if (targetPOI is Character) {
        //        Character seenCharacter = targetPOI as Character;
        //        if (seenCharacter.currentActionNode != null && seenCharacter.currentActionNode.actionStatus == ACTION_STATUS.PERFORMING) {
        //            if (seenCharacter.currentActionNode.action.goapType == INTERACTION_TYPE.PLAY_GUITAR) {
        //                OnHearGuitarPlaying(seenCharacter);
        //            } else if (seenCharacter.currentActionNode.action.goapType == INTERACTION_TYPE.SING) {
        //                OnHearSinging(seenCharacter);
        //            }
        //        }
        //    }
        //}
        //public override void OnAddTrait(ITraitable addedTo) {
        //    base.OnAddTrait(addedTo);
        //    owner = addedTo as Character;
        //    //if (owner.marker) {
        //    //Messenger.AddListener<GoapAction, GoapActionState>(Signals.ACTION_STATE_SET, OnActionStateSet);
        //    //}
        //}
        //public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy) {
        //    base.OnRemoveTrait(removedFrom, removedBy);
        //    //Messenger.RemoveListener<GoapAction, GoapActionState>(Signals.ACTION_STATE_SET, OnActionStateSet);
        //}
        ////public override void OnDeath(Character character) {
        ////    base.OnDeath(character);
        ////    //Messenger.RemoveListener<GoapAction, GoapActionState>(Signals.ACTION_STATE_SET, OnActionStateSet);
        ////}
        //public override void OnReturnToLife(Character character) {
        //    base.OnReturnToLife(character);
        //    //Messenger.AddListener<GoapAction, GoapActionState>(Signals.ACTION_STATE_SET, OnActionStateSet);
        //}
        //public override void OnOwnerInitiallyPlaced(Character owner) {
        //    base.OnOwnerInitiallyPlaced(owner);
        //    //Messenger.AddListener<GoapAction, GoapActionState>(Signals.ACTION_STATE_SET, OnActionStateSet);
        //}
        //public override void ExecuteActionPerTickEffects(INTERACTION_TYPE action, ActualGoapNode goapNode) {
        //    if (action == INTERACTION_TYPE.SING && owner == goapNode.actor) {
        //        owner.needsComponent.AdjustHappiness(5f);
        //    } else if (action == INTERACTION_TYPE.PLAY_GUITAR && owner == goapNode.actor) {
        //        owner.needsComponent.AdjustHappiness(4f);
        //    }
        //}
        public override void ExecuteCostModification(INTERACTION_TYPE action, Character actor, IPointOfInterest poiTarget, object[] otherData, ref int cost) {
            if (action == INTERACTION_TYPE.SING) {
                //cost = Utilities.rng.Next(10, 27);
                cost += -15;
            } else if (action == INTERACTION_TYPE.PLAY_GUITAR) {
                cost += -15;
                //if (poiTarget.gridTileLocation != null) {
                //    LocationGridTile knownLoc = poiTarget.gridTileLocation;
                //    if (actor.homeStructure == knownLoc.structure) {
                //        //- Actor is resident of the Guitar's Dwelling: 15 - 26 (If Music Lover 5 - 12)
                //        cost = Utilities.rng.Next(5, 13);
                //    } else {
                //        if (knownLoc.structure.isDwelling) {
                //            IDwelling dwelling = knownLoc.structure as IDwelling;
                //            if (dwelling.IsOccupied()) {
                //                for (int i = 0; i < dwelling.residents.Count; i++) {
                //                    Character currResident = dwelling.residents[i];
                //                    if (currResident.RelationshipManager.GetRelationshipEffectWith(actor) == RELATIONSHIP_EFFECT.POSITIVE) {
                //                        //- Actor is not a resident but has a positive relationship with the Guitar's Dwelling resident: 20-36 (If music lover 10 - 26)
                //                        cost = Utilities.rng.Next(10, 27);
                //                    }
                //                }
                //                //the actor does NOT have any positive relations with any resident
                //                cost = 99999; //NOTE: Should never reach here since Requirement prevents this.
                //            }
                //        }
                //    }
                //}
                ////- Guitar Structure Has No Residents 40 - 56 (If Music Lover 25 - 46)
                //cost = Utilities.rng.Next(25, 47);
            }
        }
        #endregion

        //private void OnHearGuitarPlaying(Character guitarPlayer) {
        //    owner.traitContainer.AddTrait(owner, "Satisfied");
        //    owner.needsComponent.AdjustTiredness(10f);
        //    owner.needsComponent.AdjustHappiness(10f);
        //    owner.RelationshipManager.AdjustOpinion(guitarPlayer, "Base", 3);
        //    //Debug.Log(GameManager.Instance.TodayLogString() + owner.name + " heard " + guitarPlayer.name + " playing a guitar, and became happier.");
        //    Log log = new Log(GameManager.Instance.Today(), "Trait", "MusicLover", "heard_guitar");
        //    log.AddToFillers(owner, owner.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        //    log.AddToFillers(guitarPlayer, guitarPlayer.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        //    owner.RegisterLogAndShowNotifToThisCharacterOnly(log, onlyClickedCharacter: false);
        //    //log.AddLogToInvolvedObjects();
        //    //PlayerManager.Instance.player.ShowNotificationFrom(log, owner, guitarPlayer);
        //}
        //private void OnHearSinging(Character singer) {
        //    owner.traitContainer.AddTrait(owner, "Satisfied");
        //    owner.needsComponent.AdjustTiredness(10f);
        //    owner.needsComponent.AdjustHappiness(10f);
        //    owner.RelationshipManager.AdjustOpinion(singer, "Base", 3);
        //    //Debug.Log(GameManager.Instance.TodayLogString() + owner.name + " heard " + guitarPlayer.name + " playing a guitar, and became happier.");
        //    Log log = new Log(GameManager.Instance.Today(), "Trait", "MusicLover", "heard_sing");
        //    log.AddToFillers(owner, owner.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        //    log.AddToFillers(singer, singer.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        //    log.AddLogToInvolvedObjects();
        //    PlayerManager.Instance.player.ShowNotificationFrom(log, owner, singer);
        //}

    }

}

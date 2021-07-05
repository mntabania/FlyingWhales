using System.Collections;
using System.Collections.Generic;
using Logs;
using Object_Pools;
using UnityEngine;
using UtilityScripts;

namespace Interrupts {
    public class EvaluateCultistAffiliation : Interrupt {
        public EvaluateCultistAffiliation() : base(INTERRUPT.Evaluate_Cultist_Affiliation) {
            duration = 0;
            isSimulateneous = true;
            interruptIconString = GoapActionStateDB.No_Icon;
            logTags = new[] {LOG_TAG.Life_Changes};
        }

        #region Overrides
        public override bool ExecuteInterruptStartEffect(InterruptHolder interruptHolder, ref Log overrideEffectLog, ActualGoapNode goapNode = null) {
            Character actor = interruptHolder.actor;
            int chance = 0;
            if (!actor.traitContainer.HasTrait("Cultist")) {
                if (actor.traitContainer.IsBlessed()) {
                    chance = 100;
                } else {
                    if (actor.characterClass.className == "Hero") {
                        chance = 100;
                    } else {
                        if (actor.faction.leader is Character factionLeader) {
                            if (actor.relationshipContainer.IsFriendsWith(factionLeader)) {
                                Character loverOfFactionLeader = factionLeader.relationshipContainer.GetFirstCharacterWithRelationship(RELATIONSHIP_TYPE.LOVER);
                                bool isAffairOfFactionLeader = factionLeader.relationshipContainer.HasRelationshipWith(actor, RELATIONSHIP_TYPE.AFFAIR);
                                bool isFamilyMemberOfFactionLeader = actor.relationshipContainer.IsFamilyMember(factionLeader);

                                if (loverOfFactionLeader == actor) {
                                    chance = 0;
                                } else {
                                    Character lover = actor.relationshipContainer.GetFirstCharacterWithRelationship(RELATIONSHIP_TYPE.LOVER);

                                    bool hasNoLoverOrLoverIsNotFriendOrCloseFriend = lover == null || !actor.relationshipContainer.IsFriendsWith(lover);
                                    if (isAffairOfFactionLeader && hasNoLoverOrLoverIsNotFriendOrCloseFriend) {
                                        chance += 30;
                                    }
                                    if (isFamilyMemberOfFactionLeader) {
                                        chance += 20;
                                    }
                                    if(actor.relationshipContainer.GetOpinionLabel(factionLeader) == RelationshipManager.Close_Friend) {
                                        chance += 20;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            Faction prevFaction = actor.faction;
            if (GameUtilities.RollChance(chance)) {
                //Leave faction
                if (actor.ChangeFactionTo(FactionManager.Instance.vagrantFaction)) {
                    overrideEffectLog = GameManager.CreateNewLog(GameManager.Instance.Today(), "Interrupt", name, "leave", null, logTags);
                    overrideEffectLog.AddToFillers(actor, actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                    overrideEffectLog.AddToFillers(prevFaction, prevFaction.name, LOG_IDENTIFIER.FACTION_1);
                } else {
                    overrideEffectLog = GameManager.CreateNewLog(GameManager.Instance.Today(), "Interrupt", name, "stay", null, logTags);
                    overrideEffectLog.AddToFillers(actor, actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                    overrideEffectLog.AddToFillers(prevFaction, prevFaction.name, LOG_IDENTIFIER.FACTION_1);
                }
            } else {
                //Stay on faction
                if (!actor.traitContainer.HasTrait("Cultist")) {
                    actor.traitContainer.AddTrait(actor, "Cultist");
                }
                overrideEffectLog = GameManager.CreateNewLog(GameManager.Instance.Today(), "Interrupt", name, "stay", null, logTags);
                overrideEffectLog.AddToFillers(actor, actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                overrideEffectLog.AddToFillers(prevFaction, prevFaction.name, LOG_IDENTIFIER.FACTION_1);
            }
            return true;
        }
        #endregion
    }
}
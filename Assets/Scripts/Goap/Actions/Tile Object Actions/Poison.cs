using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using Traits;
using System.Linq;
using Inner_Maps;

public class Poison : GoapAction {

    public Poison() : base(INTERACTION_TYPE.POISON) {
        //this.goapName = "Poison Table";
        actionIconString = GoapActionStateDB.Hostile_Icon;
        //_isStealthAction = true;
        //SetIsStealth(true);
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.TILE_OBJECT };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.ELEMENTAL, RACE.KOBOLD };
        isNotificationAnIntel = true;
    }

    #region Overrides
    protected override void ConstructBasePreconditionsAndEffects() {
        AddPrecondition(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_POI, conditionKey = "Tool", target = GOAP_EFFECT_TARGET.ACTOR }, HasTool);
        //**Effect 1**: Table - Add Trait (Poisoned)
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_TRAIT, conditionKey = "Poisoned", target = GOAP_EFFECT_TARGET.TARGET });
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Poison Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, object[] otherData) {
        int cost = UtilityScripts.Utilities.rng.Next(80, 121);
        string costLog = $"\n{name} {target.nameWithID}: +{cost}(RNG)";
        actor.logComponent.AppendCostLog(costLog);
        return cost;
    }
    public override string ReactionToActor(Character witness, ActualGoapNode node, REACTION_STATUS status) {
        string response = base.ReactionToActor(witness, node, status);
        Character actor = node.actor;
        IPointOfInterest target = node.poiTarget;
        List<Character> targetObjectOwners = null;
        if (target is TileObject) {
            TileObject tileObject = target as TileObject;
            targetObjectOwners = tileObject.GetOwners();
        } 
        // else if (target is SpecialToken) {
        //     SpecialToken item = target as SpecialToken;
        //     if (item.characterOwner != null) {
        //         targetObjectOwners = new List<Character>() { item.characterOwner };
        //     }
        // }

        if (targetObjectOwners != null && targetObjectOwners.Contains(witness)) {
            if (witness.traitContainer.HasTrait("Coward")) {
                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Fear, witness, actor, status);
            } else {
                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Anger, witness, actor, status);
                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Threatened, witness, actor, status);
            }

            if (witness.relationshipContainer.IsFriendsWith(actor) || witness.relationshipContainer.HasRelationshipWith(actor, RELATIONSHIP_TYPE.LOVER, RELATIONSHIP_TYPE.AFFAIR, RELATIONSHIP_TYPE.RELATIVE)) {
                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Betrayal, witness, actor, status);
                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Shock, witness, actor, status);
            }
        } else {
            bool isTargetObjectOwnedByFriend = false;
            if(targetObjectOwners != null) {
                for (int i = 0; i < targetObjectOwners.Count; i++) {
                    Character objectOwner = targetObjectOwners[i];
                    if (witness.relationshipContainer.IsFriendsWith(objectOwner) || witness.relationshipContainer.HasRelationshipWith(objectOwner, RELATIONSHIP_TYPE.LOVER, RELATIONSHIP_TYPE.AFFAIR, RELATIONSHIP_TYPE.RELATIVE)) {
                        isTargetObjectOwnedByFriend = true;
                        break;
                    }
                }
            }
            if (isTargetObjectOwnedByFriend) {
                if (witness.traitContainer.HasTrait("Coward")) {
                    response += CharacterManager.Instance.TriggerEmotion(EMOTION.Fear, witness, actor, status);
                } else {
                    response += CharacterManager.Instance.TriggerEmotion(EMOTION.Shock, witness, actor, status);
                    response += CharacterManager.Instance.TriggerEmotion(EMOTION.Disapproval, witness, actor, status);
                    if (witness.relationshipContainer.IsFriendsWith(actor) || witness.relationshipContainer.HasRelationshipWith(actor, RELATIONSHIP_TYPE.LOVER, RELATIONSHIP_TYPE.AFFAIR, RELATIONSHIP_TYPE.RELATIVE)) {
                        response += CharacterManager.Instance.TriggerEmotion(EMOTION.Disappointment, witness, actor, status);
                    }
                }
            } else {
                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Disapproval, witness, actor, status);
                if (witness.relationshipContainer.IsFriendsWith(actor) || witness.relationshipContainer.HasRelationshipWith(actor, RELATIONSHIP_TYPE.LOVER, RELATIONSHIP_TYPE.AFFAIR, RELATIONSHIP_TYPE.RELATIVE)) {
                    response += CharacterManager.Instance.TriggerEmotion(EMOTION.Shock, witness, actor, status);
                    response += CharacterManager.Instance.TriggerEmotion(EMOTION.Disappointment, witness, actor, status);
                }
            }
        }
        CrimeManager.Instance.ReactToCrime(witness, actor, node, node.associatedJobType, CRIME_TYPE.MISDEMEANOR);
        return response;
    }
    #endregion

    #region State Effects
    public void PrePoisonSuccess(ActualGoapNode goapNode) {
        //**Effect 1**: Add Poisoned Trait to target table
        goapNode.poiTarget.traitContainer.AddTrait(goapNode.poiTarget, "Poisoned", goapNode.actor);
        //GoapActionState currentState = goapNode.action.states[goapNode.currentStateName];
        //goapNode.descriptionLog.AddToFillers(goapNode.poiTarget.gridTileLocation.structure.location, goapNode.poiTarget.gridTileLocation.structure.GetNameRelativeTo(goapNode.actor), LOG_IDENTIFIER.LANDMARK_1);
        //TODO: currentState.SetIntelReaction(PoisonSuccessReactions);
    }
    // public void AfterPoisonSuccess(ActualGoapNode goapNode) {
    //     
    // }
    #endregion

    #region Requirement
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData);
        if (satisfied) {
            if (!poiTarget.IsAvailable() || poiTarget.gridTileLocation == null) {
                return false;
            }
            LocationGridTile knownLoc = poiTarget.gridTileLocation;
            if (knownLoc.structure.isDwelling) {
                IDwelling d = knownLoc.structure as IDwelling;
                if (!d.IsOccupied()) {
                    return false;
                }
                Poisoned poisonedTrait = poiTarget.traitContainer.GetNormalTrait<Poisoned>("Poisoned");
                if (poisonedTrait != null && poisonedTrait.responsibleCharacters.Contains(actor)) {
                    return false; //to prevent poisoning a table that has been already poisoned by this character
                }
                return !d.IsResident(actor);
            }
        }
        return false;
    }
    #endregion

    #region Precondition
    private bool HasTool(Character character, IPointOfInterest poiTarget, object[] otherData) {
        return character.HasItem(TILE_OBJECT_TYPE.TOOL);
    }
    #endregion

    //#region Intel Reactions
    //private List<string> PoisonSuccessReactions(Character recipient, Intel sharedIntel, SHARE_INTEL_STATUS status) {
    //    List<string> reactions = new List<string>();

    //    //NOTE: If the eat at table action of the intel is null, nobody has eaten at this table yet.
    //    //NOTE: Poisoned trait has a list of characters that poisoned it. If the poisoned trait that is currently on the table has the actor of this action in it's list
    //    //this action is still valid for reactions where the table is currently poisoned.
    //    //PoisonTableIntel pti = sharedIntel as PoisonTableIntel;
    //    Poisoned poisonedTrait = poiTarget.traitContainer.GetNormalTrait<Trait>("Poisoned") as Poisoned;
    //    Dwelling targetDwelling = poiTarget.gridTileLocation.structure as Dwelling;

    //    if(poisonedTrait != null) {
    //        //- The witness should not eat at the table until the Poison has been removed
    //        //Add character to poisoned trait of table
    //        //and when getting the cost of eating at this table, check if the character knows about the poison, if he/she does, increase cost.
    //        poisonedTrait.AddAwareCharacter(recipient);
    //    }

    //    bool tableHasPoison = poisonedTrait != null && poisonedTrait.responsibleCharacters.Contains(actor);

    //    if (isOldNews) {
    //        reactions.Add("This is old news.");
    //        return reactions;
    //    }

    //    Character assumedTargetCharacter = null;
    //    if (targetDwelling.residents.Count > 0) {
    //        //TileObject table = poiTarget as TileObject;
    //        if (targetDwelling.IsResident(recipient)) {
    //            assumedTargetCharacter = recipient;
    //        } else {
    //            List<Character> positiveRelOwners = targetDwelling.residents.Where(x => recipient.relationshipContainer.GetRelationshipEffectWith(x) == RELATIONSHIP_EFFECT.POSITIVE).ToList();
    //            if(positiveRelOwners != null && positiveRelOwners.Count > 0) {
    //                assumedTargetCharacter = positiveRelOwners[UnityEngine.Random.Range(0, positiveRelOwners.Count)];
    //            } else {
    //                List<Character> negativeRelOwners = targetDwelling.residents.Where(x => recipient.relationshipContainer.GetRelationshipEffectWith(x) == RELATIONSHIP_EFFECT.NEGATIVE).ToList();
    //                if (negativeRelOwners != null && negativeRelOwners.Count > 0) {
    //                    assumedTargetCharacter = negativeRelOwners[UnityEngine.Random.Range(0, negativeRelOwners.Count)];
    //                } else {
    //                    List<Character> sameFactionOwners = targetDwelling.residents.Where(x => recipient.faction == x.faction).ToList();
    //                    if (sameFactionOwners != null && sameFactionOwners.Count > 0) {
    //                        assumedTargetCharacter = sameFactionOwners[UnityEngine.Random.Range(0, sameFactionOwners.Count)];
    //                    } else {
    //                        assumedTargetCharacter = targetDwelling.residents[UnityEngine.Random.Range(0, targetDwelling.residents.Count)];
    //                    }
    //                }
    //            }
    //        }
    //    }

    //    if(assumedTargetCharacter == null) {
    //        if (recipient == actor) {
    //            reactions.Add("What are you talking about?! I did not plan to poison anyone. How dare you accuse me?!");
    //            AddTraitTo(recipient, "Annoyed");
    //        } else {
    //            reactions.Add("I think it was a mistake.");
    //        }
    //    } else {
    //        Character targetCharacter = assumedTargetCharacter;
    //        Sick sickTrait = targetCharacter.traitContainer.GetNormalTrait<Trait>("Sick") as Sick;
    //        Dead deadTrait = targetCharacter.traitContainer.GetNormalTrait<Trait>("Dead") as Dead;
    //        bool targetIsSick = sickTrait != null && sickTrait.gainedFromDoing != null && sickTrait.gainedFromDoing.poiTarget == poiTarget;
    //        bool targetIsDead = deadTrait != null && deadTrait.gainedFromDoing != null && deadTrait.gainedFromDoing.poiTarget == poiTarget;

    //        if (awareCharactersOfThisAction.Contains(recipient)) {
    //            //- If Recipient is Aware
    //            if (recipient == actor) {
    //                reactions.Add("Yes, I did that.");
    //            } else {
    //                reactions.Add(string.Format("I already know that {0} poisoned {1}.", actor.name, poiTarget.name));
    //            }
    //        } else {
    //            //- If Recipient is Not Aware
    //            //- Recipient is Actor
    //            CHARACTER_MOOD recipientMood = recipient.currentMoodType;
    //            if (recipient == actor) {
    //                if (recipientMood == CHARACTER_MOOD.BAD || recipientMood == CHARACTER_MOOD.DARK) {
    //                    //- If Negative Mood: "Are you threatening me?!"
    //                    reactions.Add("Are you threatening me?!");
    //                } else {
    //                    //- If Positive Mood: "Yes I did that."
    //                    reactions.Add("Yes I did that.");
    //                }
    //            }
    //            //- Recipient is Target
    //            else if (recipient == targetCharacter) {
    //                if (recipient.faction == actor.faction) {
    //                    //- Same Faction
    //                    if (!recipient.relationshipContainer.HasRelationshipWith(actor.currentAlterEgo)) {
    //                        if (recipientMood == CHARACTER_MOOD.BAD || recipientMood == CHARACTER_MOOD.DARK) {
    //                            //- No Relationship (Negative Mood)
    //                            if (tableHasPoison) {
    //                                reactions.Add(string.Format("{0} wants to poison me?! {1} will get what {2} deserves!", actor.name, Utilities.GetPronounString(actor.gender, PRONOUN_TYPE.SUBJECTIVE, true), Utilities.GetPronounString(actor.gender, PRONOUN_TYPE.SUBJECTIVE, false)));
    //                                CreateRemovePoisonJob(recipient, assumedTargetCharacter);
    //                                recipient.CreateUndermineJobOnly(actor, "informed", status);
    //                            } else if (targetIsSick) {
    //                                reactions.Add(string.Format("{0} poisoned me?! {1} will get what {2} deserves!", actor.name, Utilities.GetPronounString(actor.gender, PRONOUN_TYPE.SUBJECTIVE, true), Utilities.GetPronounString(actor.gender, PRONOUN_TYPE.SUBJECTIVE, false)));
    //                                recipient.CreateUndermineJobOnly(actor, "informed", status);
    //                            } else {
    //                                reactions.Add(string.Format("{0} wants to poison me?", actor.name));
    //                            }
    //                            if (!hasCrimeBeenReported) {
    //                                recipient.ReactToCrime(committedCrime, this, actorAlterEgo, status);
    //                            }
    //                        } else {
    //                            //- No Relationship (Positive Mood)
    //                            if (tableHasPoison) {
    //                                reactions.Add(string.Format("{0} wants to poison me? I've got to do something about this.", actor.name));
    //                                CreateRemovePoisonJob(recipient, assumedTargetCharacter);
    //                            } else if (targetIsSick) {
    //                                reactions.Add(string.Format("{0} poisoned me?! Oh my. :(", actor.name));
    //                            } else {
    //                                reactions.Add(string.Format("{0} wants to poison me?", actor.name));
    //                            }
    //                            if (!hasCrimeBeenReported) {
    //                                recipient.ReactToCrime(committedCrime, this, actorAlterEgo, status);
    //                            }
    //                        }
    //                    } else if (recipient.relationshipContainer.GetRelationshipEffectWith(actor.currentAlterEgo) == RELATIONSHIP_EFFECT.NEGATIVE) {
    //                        //- Has Negative Relationship
    //                        if (tableHasPoison) {
    //                            reactions.Add(string.Format("That stupid {0} wants to poison me?! {1} will get what {2} deserves!", actor.name, Utilities.GetPronounString(actor.gender, PRONOUN_TYPE.SUBJECTIVE, true), Utilities.GetPronounString(actor.gender, PRONOUN_TYPE.SUBJECTIVE, false)));
    //                            CreateRemovePoisonJob(recipient, assumedTargetCharacter);
    //                            recipient.CreateUndermineJobOnly(actor, "informed", status);
    //                        } else if (targetIsSick) {
    //                            reactions.Add(string.Format("That stupid {0} poisoned me?! {1} will get what {2} deserves!", actor.name, Utilities.GetPronounString(actor.gender, PRONOUN_TYPE.SUBJECTIVE, true), Utilities.GetPronounString(actor.gender, PRONOUN_TYPE.SUBJECTIVE, false)));
    //                            recipient.CreateUndermineJobOnly(actor, "informed", status);
    //                        } else {
    //                            reactions.Add(string.Format("That stupid {0} wants to poison me?!", actor.name, Utilities.GetPronounString(actor.gender, PRONOUN_TYPE.SUBJECTIVE, true), Utilities.GetPronounString(actor.gender, PRONOUN_TYPE.SUBJECTIVE, false)));
    //                        }
    //                        if (!hasCrimeBeenReported) {
    //                            recipient.ReactToCrime(committedCrime, this, actorAlterEgo, status);
    //                        }
    //                    } else if (recipient.relationshipContainer.GetRelationshipEffectWith(actor.currentAlterEgo) == RELATIONSHIP_EFFECT.POSITIVE) {
    //                        //- Has Positive Relationship
    //                        if (tableHasPoison) {
    //                            if (!hasCrimeBeenReported) {
    //                                if (recipient.ReactToCrime(committedCrime, this, actorAlterEgo, status)) {
    //                                    reactions.Add(string.Format("Why does {0} want me dead? I've got to do something about this!", actor.name));
    //                                } else {
    //                                    reactions.Add("I just have to remove the poison and everything will go back to the way it was.");
    //                                }
    //                            }
    //                            CreateRemovePoisonJob(recipient, assumedTargetCharacter);
    //                        } else if (targetIsSick) {
    //                            if (!hasCrimeBeenReported) {
    //                                if (recipient.ReactToCrime(committedCrime, this, actorAlterEgo, status)) {
    //                                    reactions.Add(string.Format("Why does {0} want me dead? I've got to do something about this!", actor.name));
    //                                } else {
    //                                    reactions.Add("Relax. I didn't die. I just got sick. I'm sure I'll recover in no time.");
    //                                }
    //                            }
    //                        } else {
    //                            reactions.Add(string.Format("Why does {0} want me dead?", actor.name));
    //                        }
    //                    }
    //                } else {
    //                    //- Not Same Faction
    //                    if (recipient.relationshipContainer.GetRelationshipEffectWith(actor.currentAlterEgo) == RELATIONSHIP_EFFECT.POSITIVE) {
    //                        //- Has Positive Relationship
    //                        if (tableHasPoison) {
    //                            if (RelationshipManager.Instance.RelationshipDegradation(actor, recipient, this)) {
    //                                reactions.Add(string.Format("{0} wants to poison me?! {1} will not get away with this!", actor.name, Utilities.GetPronounString(actor.gender, PRONOUN_TYPE.SUBJECTIVE, true)));
    //                                recipient.CreateUndermineJobOnly(actor, "informed", status);
    //                            } else {
    //                                reactions.Add("I just have to remove the poison and everything will go back to the way it was.");
    //                            }
    //                            CreateRemovePoisonJob(recipient, assumedTargetCharacter);
    //                        } else if (targetIsSick) {
    //                            if (RelationshipManager.Instance.RelationshipDegradation(actor, recipient, this)) {
    //                                reactions.Add(string.Format("{0} poisoned me?! I will have my revenge!", actor.name));
    //                                recipient.CreateUndermineJobOnly(actor, "informed", status);
    //                            } else {
    //                                reactions.Add("Relax. I didn't die. I just got sick. I'm sure I'll recover in no time.");
    //                            }
    //                        } else {
    //                            reactions.Add(string.Format("{0} wants to poison me?! I knew those kind of people could never be trusted.", actor.name));
    //                        }
    //                    } else {
    //                        //- Has Negative/No Relationship
    //                        if (tableHasPoison) {
    //                            reactions.Add(string.Format("{0} will not get away with this!", actor.name));
    //                            CreateRemovePoisonJob(recipient, assumedTargetCharacter);
    //                            recipient.CreateUndermineJobOnly(actor, "informed", status);
    //                        } else if (targetIsSick) {
    //                            reactions.Add(string.Format("{0} will not get away with this!", actor.name));
    //                            recipient.CreateUndermineJobOnly(actor, "informed", status);
    //                        } else {
    //                            reactions.Add(string.Format("{0} will not get away with this! I knew those kind of people could never be trusted.", actor.name));
    //                        }
    //                    }
    //                }
    //            }
    //            //- Recipient Has Positive Relationship with Target
    //            else if (recipient.relationshipContainer.GetRelationshipEffectWith(targetCharacter.currentAlterEgo) == RELATIONSHIP_EFFECT.POSITIVE) {
    //                RELATIONSHIP_EFFECT relationshipWithActor = recipient.relationshipContainer.GetRelationshipEffectWith(actor.currentAlterEgo);
    //                if (relationshipWithActor == RELATIONSHIP_EFFECT.POSITIVE) {
    //                    if (tableHasPoison) {
    //                        if (!hasCrimeBeenReported) {
    //                            if (recipient.ReactToCrime(committedCrime, this, actorAlterEgo, status)) {
    //                                reactions.Add(string.Format("{0} wants to poison {1}? I've got to do something about this.", actor.name, targetCharacter.name));
    //                                recipient.CreateShareInformationJob(targetCharacter, this);
    //                            } else {
    //                                reactions.Add(string.Format("{0} wants to poison {1}? I don't believe that.", actor.name, targetCharacter.name));
    //                            }
    //                        }
    //                    } else if (targetIsSick) {
    //                        if (!hasCrimeBeenReported) {
    //                            if (recipient.ReactToCrime(committedCrime, this, actorAlterEgo, status)) {
    //                                reactions.Add(string.Format("{0} poisoned {1}? I've got to do something about this.", actor.name, targetCharacter.name));
    //                                recipient.CreateShareInformationJob(targetCharacter, this);
    //                            } else {
    //                                reactions.Add(string.Format("{0} poisoned {1}? I don't believe that.", actor.name, targetCharacter.name));
    //                            }
    //                        }
    //                    } else if (targetIsDead) {
    //                        if (!hasCrimeBeenReported) {
    //                            if (recipient.ReactToCrime(CRIME.MURDER, this, actorAlterEgo, status)) {
    //                                reactions.Add(string.Format("{0} poisoned {1}? I've got to do something about this.", actor.name, targetCharacter.name));
    //                            } else {
    //                                reactions.Add(string.Format("{0} poisoned {1}? I don't believe that.", actor.name, targetCharacter.name));
    //                            }
    //                        }
    //                    } else {
    //                        reactions.Add(string.Format("{0} wants to poison {1}?", actor.name, targetCharacter.name));
    //                    }
    //                } else if (relationshipWithActor == RELATIONSHIP_EFFECT.NEGATIVE) {
    //                    if (tableHasPoison) {
    //                        reactions.Add(string.Format("{0} wants to poison {1}? Why am I not surprised?", actor.name, targetCharacter.name));
    //                        if (!hasCrimeBeenReported) {
    //                            recipient.ReactToCrime(committedCrime, this, actorAlterEgo, status);
    //                        }
    //                        recipient.CreateShareInformationJob(targetCharacter, this);
    //                    } else if (targetIsSick) {
    //                        reactions.Add(string.Format("{0} poisoned {1}? Why am I not surprised?", actor.name, targetCharacter.name));
    //                        if (!hasCrimeBeenReported) {
    //                            recipient.ReactToCrime(committedCrime, this, actorAlterEgo, status);
    //                        }
    //                        recipient.CreateShareInformationJob(targetCharacter, this);
    //                    } else if (targetIsDead) {
    //                        reactions.Add(string.Format("{0} poisoned {1}? Why am I not surprised?", actor.name, targetCharacter.name));
    //                        if (!hasCrimeBeenReported) {
    //                            recipient.ReactToCrime(CRIME.MURDER, this, actorAlterEgo, status);
    //                        }
    //                    } else {
    //                        reactions.Add(string.Format("{0} wants to poison {1}? What a horrible person!", actor.name, targetCharacter.name));
    //                    }
    //                } else {
    //                    if (tableHasPoison) {
    //                        reactions.Add(string.Format("{0} could die. I've got to do something about this!", targetCharacter.name));
    //                        if (!hasCrimeBeenReported) {
    //                            recipient.ReactToCrime(committedCrime, this, actorAlterEgo, status);
    //                        }
    //                        recipient.CreateShareInformationJob(targetCharacter, this);
    //                    } else if (targetIsSick) {
    //                        reactions.Add(string.Format("{0} almost died. I've got to do something about this!", targetCharacter.name));
    //                        if (!hasCrimeBeenReported) {
    //                            recipient.ReactToCrime(committedCrime, this, actorAlterEgo, status);
    //                        }
    //                        recipient.CreateShareInformationJob(targetCharacter, this);
    //                    } else if (targetIsDead) {
    //                        reactions.Add(string.Format("{0} died. I've got to do something about this!", targetCharacter.name));
    //                        if (!hasCrimeBeenReported) {
    //                            recipient.ReactToCrime(CRIME.MURDER, this, actorAlterEgo, status);
    //                        }
    //                    } else {
    //                        reactions.Add(string.Format("{0} could die.", targetCharacter.name));
    //                    }
    //                }
    //            }
    //            //- Recipient Has Negative Relationship with Target
    //            else if (recipient.relationshipContainer.GetRelationshipEffectWith(targetCharacter.currentAlterEgo) == RELATIONSHIP_EFFECT.NEGATIVE) {
    //                RELATIONSHIP_EFFECT relationshipWithActor = recipient.relationshipContainer.GetRelationshipEffectWith(actor.currentAlterEgo);
    //                if (relationshipWithActor == RELATIONSHIP_EFFECT.POSITIVE) {
    //                    if (tableHasPoison) {
    //                        reactions.Add(string.Format("I hope {0} dies from that poison!", targetCharacter.name));
    //                    } else if (targetIsSick) {
    //                        reactions.Add(string.Format("{0} deserves worse but that will do.", targetCharacter.name));
    //                        AddTraitTo(recipient, "Satisfied");
    //                    } else if (targetIsDead) {
    //                        reactions.Add("Good riddance.");
    //                        AddTraitTo(recipient, "Satisfied");
    //                    } else {
    //                        reactions.Add(string.Format("I can't wait for {0} to die from that poison.", targetCharacter.name));
    //                    }
    //                } else if (relationshipWithActor == RELATIONSHIP_EFFECT.NEGATIVE) {
    //                    if (tableHasPoison) {
    //                        reactions.Add(string.Format("I hate both of them but I hope {0} dies from that poison!", targetCharacter.name));
    //                    } else if (targetIsSick) {
    //                        reactions.Add(string.Format("{0} deserves worse but that will do.", targetCharacter.name));
    //                        AddTraitTo(recipient, "Satisfied");
    //                    } else if (targetIsDead) {
    //                        reactions.Add(string.Format("Good riddance. I hope {0} is next.", actor.name));
    //                        AddTraitTo(recipient, "Satisfied");
    //                    } else {
    //                        reactions.Add(string.Format("I hate both of them but I can't wait for {0} to die from that poison.", targetCharacter.name));
    //                    }
    //                } else {
    //                    if (tableHasPoison) {
    //                        reactions.Add(string.Format("I hope {0} dies from that poison!", targetCharacter.name));
    //                    } else if (targetIsSick) {
    //                        reactions.Add(string.Format("{0} deserves worse but that will do.", targetCharacter.name));
    //                        AddTraitTo(recipient, "Satisfied");
    //                    } else if (targetIsDead) {
    //                        reactions.Add("Good riddance.");
    //                        AddTraitTo(recipient, "Satisfied");
    //                    } else {
    //                        reactions.Add(string.Format("I can't wait for {0} to die from that poison.", targetCharacter.name));
    //                    }
    //                }
    //            }
    //            //- Recipient Has No Relationship with Target
    //            else {
    //                RELATIONSHIP_EFFECT relationshipWithActor = recipient.relationshipContainer.GetRelationshipEffectWith(actor.currentAlterEgo);
    //                if (relationshipWithActor == RELATIONSHIP_EFFECT.POSITIVE) {
    //                    if (tableHasPoison) {
    //                        if (!hasCrimeBeenReported) {
    //                            if (recipient.ReactToCrime(committedCrime, this, actorAlterEgo, status)) {
    //                                reactions.Add(string.Format("{0} wants to poison {1}? This is unacceptable!", actor.name, targetCharacter.name));
    //                                recipient.CreateShareInformationJob(targetCharacter, this);
    //                            } else {
    //                                reactions.Add(string.Format("{0} wants to poison {1}? I don't believe that.", actor.name, targetCharacter.name));
    //                            }
    //                        }
    //                    } else if (targetIsSick) {
    //                        if (!hasCrimeBeenReported) {
    //                            if (recipient.ReactToCrime(committedCrime, this, actorAlterEgo, status)) {
    //                                reactions.Add(string.Format("{0} poisoned {1}? This is unacceptable!", actor.name, targetCharacter.name));
    //                                recipient.CreateShareInformationJob(targetCharacter, this);
    //                            } else {
    //                                reactions.Add(string.Format("{0} poisoned {1}? I don't believe that.", actor.name, targetCharacter.name));
    //                            }
    //                        }
    //                    } else if (targetIsDead) {
    //                        if (!hasCrimeBeenReported) {
    //                            if (recipient.ReactToCrime(CRIME.MURDER, this, actorAlterEgo, status)) {
    //                                reactions.Add(string.Format("{0} poisoned {1}? This is unacceptable!", actor.name, targetCharacter.name));
    //                            } else {
    //                                reactions.Add(string.Format("{0} poisoned {1}? I don't believe that.", actor.name, targetCharacter.name));
    //                            }
    //                        }
    //                    } else {
    //                        reactions.Add(string.Format("{0} wants to poison {1}? Why?!", actor.name, targetCharacter.name));
    //                    }
    //                } else if (relationshipWithActor == RELATIONSHIP_EFFECT.NEGATIVE) {
    //                    if (tableHasPoison) {
    //                        reactions.Add(string.Format("{0} wants to poison {1}? Why am I not surprised?", actor.name, targetCharacter.name));
    //                        if (!hasCrimeBeenReported) {
    //                            recipient.ReactToCrime(committedCrime, this, actorAlterEgo, status);
    //                        }
    //                        recipient.CreateShareInformationJob(targetCharacter, this);
    //                    } else if (targetIsSick) {
    //                        reactions.Add(string.Format("{0} poisoned {1}? Why am I not surprised?", actor.name, targetCharacter.name));
    //                        if (!hasCrimeBeenReported) {
    //                            recipient.ReactToCrime(committedCrime, this, actorAlterEgo, status);
    //                        }
    //                        recipient.CreateShareInformationJob(targetCharacter, this);
    //                    } else if (targetIsDead) {
    //                        reactions.Add(string.Format("{0} poisoned {1}? I can't let a killer loose.", actor.name, targetCharacter.name));
    //                        if (!hasCrimeBeenReported) {
    //                            recipient.ReactToCrime(CRIME.MURDER, this, actorAlterEgo, status);
    //                        }
    //                    } else {
    //                        reactions.Add(string.Format("{0} wants to poison {1}? So horrible!", actor.name, targetCharacter.name));
    //                    }
    //                } else {
    //                    if (tableHasPoison) {
    //                        reactions.Add(string.Format("{0} could die. I've got to do something about this!", targetCharacter.name));
    //                        if (!hasCrimeBeenReported) {
    //                            recipient.ReactToCrime(committedCrime, this, actorAlterEgo, status);
    //                        }
    //                        recipient.CreateShareInformationJob(targetCharacter, this);
    //                    } else if (targetIsSick) {
    //                        reactions.Add(string.Format("{0} almost died. I've got to do something about this!", targetCharacter.name));
    //                        if (!hasCrimeBeenReported) {
    //                            recipient.ReactToCrime(committedCrime, this, actorAlterEgo, status);
    //                        }
    //                        recipient.CreateShareInformationJob(targetCharacter, this);
    //                    } else if (targetIsDead) {
    //                        reactions.Add(string.Format("{0} died. I've got to do something about this!", targetCharacter.name));
    //                        if (!hasCrimeBeenReported) {
    //                            recipient.ReactToCrime(CRIME.MURDER, this, actorAlterEgo, status);
    //                        }
    //                    } else {
    //                        reactions.Add(string.Format("{0} could die.", targetCharacter.name));
    //                    }
    //                }
    //            }
    //        }
    //    }
    //    return reactions;
    //}
    //#endregion

    //private void CreateRemovePoisonJob(Character recipient, Character troubledCharacter) {
    //    if (recipient.role.roleType == CHARACTER_ROLE.CIVILIAN || recipient.role.roleType == CHARACTER_ROLE.ADVENTURER || recipient.role.roleType == CHARACTER_ROLE.SOLDIER || recipient.role.roleType == CHARACTER_ROLE.BANDIT || (recipient.role.roleType != CHARACTER_ROLE.BEAST && recipient.isFactionless)) {
    //        GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.REMOVE_POISON, new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.REMOVE_TRAIT, conditionKey = "Poisoned", targetPOI = poiTarget });
    //        recipient.jobQueue.AddJobInQueue(job);
    //    }
    //    //If Civilian, Noble or Faction Leader, create an Ask for Help Remove Poison Job.
    //    else if (recipient.role.roleType == CHARACTER_ROLE.NOBLE || recipient.role.roleType == CHARACTER_ROLE.LEADER) {
    //        recipient.CreateAskForHelpJob(troubledCharacter, INTERACTION_TYPE.REMOVE_POISON_TABLE, poiTarget);
    //    }
    //}
}

public class PoisonTableData : GoapActionData {
    public PoisonTableData() : base(INTERACTION_TYPE.POISON) {
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.ELEMENTAL, RACE.KOBOLD };
        requirementAction = Requirement;
    }

    private bool Requirement(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        if (!poiTarget.IsAvailable() || poiTarget.gridTileLocation == null) {
            return false;
        }
        LocationGridTile knownLoc = poiTarget.gridTileLocation;
        //LocationGridTile knownLoc = actor.GetAwareness(poiTarget).knownGridLocation;
        if (knownLoc.structure.isDwelling) {
            IDwelling d = knownLoc.structure as IDwelling;
            if (!d.IsOccupied()) {
                return false;
            }
            Poisoned poisonedTrait = poiTarget.traitContainer.GetNormalTrait<Poisoned>("Poisoned");
            if (poisonedTrait != null && poisonedTrait.responsibleCharacters.Contains(actor)) {
                return false; //to prevent poisoning a table that has been already poisoned by this character
            }
            return !d.IsResident(actor);
        }
        return false;
    }
}
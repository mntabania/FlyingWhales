using System.Collections;
using System.Collections.Generic;
using UtilityScripts;
using Inner_Maps.Location_Structures;
public class CultistKit : TileObject {
    
    public CultistKit() {
        Initialize(TILE_OBJECT_TYPE.CULTIST_KIT);
    }
    public CultistKit(SaveDataTileObject data) : base(data) {
        
    }

    public override string ToString() {
        return $"Cultist Kit {id.ToString()}";
    }

    #region Reactions
    public override void VillagerReactionToTileObject(Character actor, ref string debugLog) {
        base.VillagerReactionToTileObject(actor, ref debugLog);
        if (!IsOwnedBy(actor)) {
#if DEBUG_LOG
            debugLog = $"{debugLog}\n-Object is a cultist kit";
#endif
            if (gridTileLocation != null) {
                List<Character> validResidents = RuinarchListPool<Character>.Claim();
                if (structureLocation is ManMadeStructure &&
                    structureLocation.GetNumberOfResidentsAndPopulateListExcluding(validResidents, actor) > 0) {
#if DEBUG_LOG
                    debugLog = $"{debugLog}\n-Cultist kit is at structure with residents excluding the witness";
#endif
                    int chanceToCreateAssumption = 0;
                    if (actor.traitContainer.HasTrait("Suspicious") || actor.moodComponent.moodState == MOOD_STATE.Critical) {
                        chanceToCreateAssumption = 100;
                    } else if (actor.moodComponent.moodState == MOOD_STATE.Bad) {
                        chanceToCreateAssumption = 50;
                    } else {
                        chanceToCreateAssumption = 15;
                    }
#if DEBUG_LOG
                    debugLog = $"{debugLog}\n-Rolling for chance to create assumption";
#endif
                    if (GameUtilities.RollChance(chanceToCreateAssumption, ref debugLog)) {
                        actor.reactionComponent.assumptionSuspects.Clear();
                        if (validResidents != null) {
                            for (int i = 0; i < validResidents.Count; i++) {
                                Character resident = validResidents[i];
                                AWARENESS_STATE awarenessState = actor.relationshipContainer.GetAwarenessState(resident);
                                if (awarenessState == AWARENESS_STATE.Available) {
                                    actor.reactionComponent.assumptionSuspects.Add(resident);
                                } else if (awarenessState == AWARENESS_STATE.None) {
                                    if (!resident.isDead) {
                                        actor.reactionComponent.assumptionSuspects.Add(resident);
                                    }
                                }
                            }
                        }
                        Character chosenTarget = CollectionUtilities.GetRandomElement(actor.reactionComponent.assumptionSuspects);
                        if (chosenTarget != null && CrimeManager.Instance.IsConsideredACrimeByCharacter(actor, chosenTarget, this, CRIME_TYPE.Demon_Worship)) {
                            actor.assumptionComponent.CreateAndReactToNewAssumption(chosenTarget, this, INTERACTION_TYPE.IS_CULTIST, REACTION_STATUS.WITNESSED);
                        }
                    }
                }
                RuinarchListPool<Character>.Release(validResidents);
            }
        }
    }
    #endregion
}
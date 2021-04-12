using System.Collections.Generic;
using Inner_Maps.Location_Structures;
using UtilityScripts;

public class KleptomaniacBehaviour : CharacterBehaviourComponent {

    public KleptomaniacBehaviour() {
        priority = 20;
    }

    #region Overrides
    public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob) {
        if (character.homeSettlement == null) {
            producedJob = null;
            return false;
        }
        PLAYER_SKILL_TYPE playerSkillType = PLAYER_SKILL_TYPE.KLEPTOMANIA;
        if (character.afflictionsSkillsInflictedByPlayer.Contains(playerSkillType) && !character.IsInventoryAtFullCapacity()) {
            //affliction was applied by player
            PlayerSkillData playerSkillData = PlayerSkillManager.Instance.GetScriptableObjPlayerSkillData<PlayerSkillData>(playerSkillType);
            SkillData skillData = PlayerSkillManager.Instance.GetSkillData(playerSkillType);
            bool canRobAnyHouse = playerSkillData.afflictionUpgradeData.HasAddedBehaviourForLevel(AFFLICTION_SPECIFIC_BEHAVIOUR.Rob_From_House, skillData.currentLevel);
            bool canRobAnyPlace = playerSkillData.afflictionUpgradeData.HasAddedBehaviourForLevel(AFFLICTION_SPECIFIC_BEHAVIOUR.Rob_Any_Place, skillData.currentLevel);
            if (canRobAnyHouse || canRobAnyPlace) {
                bool chanceMet = ChanceData.RollChance(canRobAnyPlace ? CHANCE_TYPE.Kleptomania_Rob_Any_Place : CHANCE_TYPE.Kleptomania_Rob_Other_House);
                if (chanceMet) {
                    List<LocationStructure> robChoices = RuinarchListPool<LocationStructure>.Claim();
                    if (canRobAnyPlace) {
                        robChoices.AddRange(character.homeSettlement.allStructures);
                        if (character.homeStructure != null) {
                            robChoices.Remove(character.homeStructure);    
                        }
                    } else {
                        List<LocationStructure> dwellings = character.homeSettlement.GetStructuresOfType(STRUCTURE_TYPE.DWELLING);
                        if (dwellings != null) {
                            robChoices.AddRange(dwellings);    
                        }
                        if (character.homeStructure != null) {
                            robChoices.Remove(character.homeStructure);    
                        }
                    }
                    if (robChoices.Count > 0) {
                        LocationStructure targetStructure = CollectionUtilities.GetRandomElement(robChoices);
                        return character.jobComponent.TriggerRobLocation(targetStructure, INTERACTION_TYPE.STEAL_ANYTHING, out producedJob);
                    }
                    RuinarchListPool<LocationStructure>.Release(robChoices);
                }
            }
        }
        producedJob = null;
        return false;
    }
    #endregion
}

using System.Collections;
using System.Collections.Generic;
using Traits;
using UnityEngine;
using UtilityScripts;

public class DefaultFactionRelated : CharacterBehaviourComponent {
    public DefaultFactionRelated() {
        priority = 26;
        attributes = new BEHAVIOUR_COMPONENT_ATTRIBUTE[] { BEHAVIOUR_COMPONENT_ATTRIBUTE.DO_NOT_SKIP_PROCESSING/*, BEHAVIOUR_COMPONENT_ATTRIBUTE.ONCE_PER_DAY*/ };
    }

    public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob) {
        producedJob = null;
        if (character.isFriendlyFactionless) {
            if(UnityEngine.Random.Range(0, 100) < 15) {
                log += $"\n-{character.name} is factionless, 15% chance to join faction";
                Faction chosenFaction = character.JoinFactionProcessing();
                if (chosenFaction != null) {
                    log += $"\n-Chosen faction to join: {chosenFaction.name}";
                } else {
                    log += "\n-No available faction that the character fits the ideology";
                }
                return true;
            } else {
                log += $"\nDid not meet chance to join faction. Checking if can create faction...";
                int villagerFactionCount = FactionManager.Instance.GetActiveVillagerFactionCount();
                log += $"\nActive villager faction count is {villagerFactionCount.ToString()} and all regions count is {GridMap.Instance.allRegions.Length.ToString()}";
                if (villagerFactionCount < GridMap.Instance.allRegions.Length) {
                    log += $"\nActive villager factions is less than number of regions, rolling chances";
                    int createChance = 3;
                    if (character.traitContainer.HasTrait("Inspiring", "Ambitious")) {
                        createChance = 15;
                    }
                    if (GameUtilities.RollChance(createChance, ref log)) {
                        log += $"\nChance met, creating new faction";
                        character.interruptComponent.TriggerInterrupt(INTERRUPT.Create_Faction, character);
                        return true;
                    }
                }
            }
        }
        return false;
    }
}

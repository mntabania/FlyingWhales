﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionManager : MonoBehaviour {

    public static InteractionManager Instance = null;

    public static readonly string Supply_Cache_Reward_1 = "SupplyCacheReward1";
    public static readonly string Mana_Cache_Reward_1 = "ManaCacheReward1";
    public static readonly string Mana_Cache_Reward_2 = "ManaCacheReward2";
    public static readonly string Exp_Reward_1 = "ExpReward1";
    public static readonly string Exp_Reward_2 = "ExpReward2";

    public Dictionary<string, RewardConfig> rewardConfig = new Dictionary<string, RewardConfig>(){
        { Supply_Cache_Reward_1, new RewardConfig(){ rewardType = REWARD.SUPPLY, lowerRange = 50, higherRange = 250 } },
        { Mana_Cache_Reward_1, new RewardConfig(){ rewardType = REWARD.MANA, lowerRange = 5, higherRange = 30 } },
        { Mana_Cache_Reward_2, new RewardConfig(){ rewardType = REWARD.MANA, lowerRange = 30, higherRange = 50 } },
        { Exp_Reward_1, new RewardConfig(){ rewardType = REWARD.EXP, lowerRange = 40, higherRange = 40 } },
        { Exp_Reward_2, new RewardConfig(){ rewardType = REWARD.EXP, lowerRange = 80, higherRange = 80 } },
    };

    private void Awake() {
        Instance = this;
    }

    public Interaction CreateNewInteraction(INTERACTION_TYPE interactionType, IInteractable interactable) {
        Interaction createdInteraction = null;
        switch (interactionType) {
            case INTERACTION_TYPE.BANDIT_RAID:
                createdInteraction = new BanditRaid(interactable);
                break;
            case INTERACTION_TYPE.INVESTIGATE:
                createdInteraction = new InvestigateInteraction(interactable);
                break;
            case INTERACTION_TYPE.POI_1:
                createdInteraction = new PointOfInterest1(interactable);
                break;
            case INTERACTION_TYPE.POI_2:
                createdInteraction = new PointOfInterest2(interactable);
                break;
            case INTERACTION_TYPE.HARVEST_SEASON:
                createdInteraction = new HarvestSeason(interactable);
                break;
            case INTERACTION_TYPE.SPIDER_QUEEN:
                createdInteraction = new TheSpiderQueen(interactable);
                break;
            case INTERACTION_TYPE.BANDIT_REINFORCEMENT:
                createdInteraction = new BanditReinforcement(interactable);
                break;
            case INTERACTION_TYPE.MYSTERY_HUM:
                createdInteraction = new MysteryHum(interactable);
                break;
            case INTERACTION_TYPE.ARMY_UNIT_TRAINING:
                createdInteraction = new ArmyUnitTraining(interactable);
                break;
            case INTERACTION_TYPE.ARMY_MOBILIZATION:
                createdInteraction = new ArmyMobilization(interactable);
                break;
            case INTERACTION_TYPE.UNFINISHED_CURSE:
                createdInteraction = new UnfinishedCurse(interactable);
                break;
            case INTERACTION_TYPE.ARMY_ATTACKS:
                createdInteraction = new ArmyAttacks(interactable);
                break;
            case INTERACTION_TYPE.SUSPICIOUS_SOLDIER_MEETING:
                createdInteraction = new SuspiciousSoldierMeeting(interactable);
                break;
            case INTERACTION_TYPE.KILLER_ON_THE_LOOSE:
                createdInteraction = new KillerOnTheLoose(interactable);
                break;
            case INTERACTION_TYPE.MYSTERIOUS_SARCOPHAGUS:
                createdInteraction = new MysteriousSarcophagus(interactable);
                break;
        }
        return createdInteraction;
    }
    public bool CanCreateInteraction(INTERACTION_TYPE interactionType, BaseLandmark landmark) {
        switch (interactionType) {
            case INTERACTION_TYPE.BANDIT_REINFORCEMENT:
                //Random event that may show up in Bandit Camp tiles. 
                //Only triggers if the Bandit Camp has an empty Tile Defender Slot
                if (landmark.HasEmptyDefenderSlot()) {
                    return true;
                }
                return false;
            case INTERACTION_TYPE.POI_1:
            case INTERACTION_TYPE.POI_2:
            case INTERACTION_TYPE.SPIDER_QUEEN:
            case INTERACTION_TYPE.MYSTERY_HUM:
            case INTERACTION_TYPE.UNFINISHED_CURSE:
                //Requires actively Investigating Imp.
                return landmark.isBeingInspected;
            default:
                return true;
        }
    }
    public Reward GetReward(string rewardName) {
        if (rewardConfig.ContainsKey(rewardName)) {
            RewardConfig config = rewardConfig[rewardName];
            return new Reward { rewardType = config.rewardType, amount = Random.Range(config.lowerRange, config.higherRange) };
        }
        throw new System.Exception("There is no reward configuration with name " + rewardName);
    }
}

public struct RewardConfig {
    public REWARD rewardType;
    public int lowerRange;
    public int higherRange;
}
public struct Reward {
    public REWARD rewardType;
    public int amount;
}
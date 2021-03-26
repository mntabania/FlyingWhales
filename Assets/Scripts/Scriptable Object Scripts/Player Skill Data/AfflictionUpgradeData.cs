using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AfflictionUpgradeData
{
    [HideInInspector]
    public List<AFFLICTION_UPGRADE_BONUS> bonuses = new List<AFFLICTION_UPGRADE_BONUS>();
    [HideInInspector]
    public List<int> cooldown = new List<int>();
    [HideInInspector]
    public List<float> additionalPiercePerLevel = new List<float>();
    [HideInInspector]
    public List<int> rateChance = new List<int>();
    [HideInInspector]
    public List<int> crowdNumber = new List<int>();
    [HideInInspector]
    public List<int> napsPercent = new List<int>();
    [HideInInspector]
    public List<int> numberOfCriteria = new List<int>();
    [HideInInspector]
    public List<OPINIONS> opinionTrigger = new List<OPINIONS>();
    [HideInInspector]
    public List<AFFLICTION_SPECIFIC_BEHAVIOUR> addedBehaviour = new List<AFFLICTION_SPECIFIC_BEHAVIOUR>();
    [HideInInspector]
    public List<LIST_OF_CRITERIA> listOfCriteria = new List<LIST_OF_CRITERIA>();

    public int GetCoolDownPerLevel(int p_currentLevel) {
        if (cooldown == null || cooldown.Count <= 0) {
            return -1;
        }
        if (p_currentLevel >= cooldown.Count) {
            return cooldown[cooldown.Count - 1];
        }
        return cooldown[p_currentLevel];
    }

    public float GetPiercePerLevel(int p_currentLevel) {
        if (additionalPiercePerLevel == null || additionalPiercePerLevel.Count <= 0) {
            return -1;
        }
        if (p_currentLevel >= additionalPiercePerLevel.Count) {
            return additionalPiercePerLevel[additionalPiercePerLevel.Count - 1];
        }
        return additionalPiercePerLevel[p_currentLevel];
    }

    public float GetRateChancePerLevel(int p_currentLevel) {
        if (rateChance == null || rateChance.Count <= 0) {
            return -1;
        }
        if (p_currentLevel >= rateChance.Count) {
            return rateChance[rateChance.Count - 1];
        }
        return rateChance[p_currentLevel];
    }

    public int GetCrowdNumberPerLevel(int p_currentLevel) {
        if (crowdNumber == null || crowdNumber.Count <= 0) {
            return -1;
        }
        if (p_currentLevel >= crowdNumber.Count) {
            return crowdNumber[crowdNumber.Count - 1];
        }
        return crowdNumber[p_currentLevel];
    }

    public int GetNapsPercentagePerLevel(int p_currentLevel) {
        if (napsPercent == null || napsPercent.Count <= 0) {
            return -1;
        }
        if (p_currentLevel >= napsPercent.Count) {
            return crowdNumber[napsPercent.Count - 1];
        }
        return napsPercent[p_currentLevel];
    }

    public OPINIONS GetOpinionTriggerPerLevel(int p_currentLevel) {
        if (opinionTrigger == null || opinionTrigger.Count <= 0) {
            return OPINIONS.NoOne;
        }
        if (p_currentLevel >= napsPercent.Count) {
            return opinionTrigger[napsPercent.Count - 1];
        }
        return opinionTrigger[p_currentLevel];
    }

    public List<OPINIONS> GetAllOpinionsTrigger() {
        return opinionTrigger;
    }

    public AFFLICTION_SPECIFIC_BEHAVIOUR GetAddedBehaviourPerLevel(int p_currentLevel) {
        if (addedBehaviour == null || addedBehaviour.Count <= 0) {
            return AFFLICTION_SPECIFIC_BEHAVIOUR.None;
        }
        if (p_currentLevel >= opinionTrigger.Count) {
            return addedBehaviour[addedBehaviour.Count - 1];
        }
        return addedBehaviour[p_currentLevel];
    }

    public List<AFFLICTION_SPECIFIC_BEHAVIOUR> GetAllAddedBehaviour() {
        return addedBehaviour;
    }
}
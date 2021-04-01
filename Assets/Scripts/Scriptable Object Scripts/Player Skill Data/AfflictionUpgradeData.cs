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
    public List<int> napsDuration = new List<int>();
    [HideInInspector]
    public List<int> numberOfCriteria = new List<int>();
    [HideInInspector]
    public List<int> hungerRate = new List<int>();
    [HideInInspector]
    public List<float> duration = new List<float>();
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

    public int GetHungerRatePerLevel(int p_currentLevel) {
        if (hungerRate == null || hungerRate.Count <= 0) {
            return -1;
        }
        if (p_currentLevel >= hungerRate.Count) {
            return hungerRate[hungerRate.Count - 1];
        }
        return hungerRate[p_currentLevel];
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

    public float GetNapsDurationPerLevel(int p_currentLevel) {
        if (napsDuration == null || napsDuration.Count <= 0) {
            return -1;
        }
        if (p_currentLevel >= napsDuration.Count) {
            return napsDuration[napsDuration.Count - 1];
        }
        return napsDuration[p_currentLevel];
    }

    public float GetDurationPerLevel(int p_currentLevel) {
        if (duration == null || duration.Count <= 0) {
            return -1;
        }
        if (p_currentLevel >= duration.Count) {
            return duration[duration.Count - 1];
        }
        return duration[p_currentLevel];
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

    public OPINIONS GetOpinionTriggerPerLevel(int p_currentLevel) {
        if (opinionTrigger == null || opinionTrigger.Count <= 0) {
            return OPINIONS.NoOne;
        }
        if (p_currentLevel >= opinionTrigger.Count) {
            return opinionTrigger[opinionTrigger.Count - 1];
        }
        return opinionTrigger[p_currentLevel];
    }

    public List<OPINIONS> GetAllOpinionsTrigger() {
        return opinionTrigger;
    }
    public bool HasOpinionTriggerForLevel(OPINIONS p_opinion, int p_level) {
        return opinionTrigger.HasValueInListUntilIndex(p_level, p_opinion);
    }
    public void PopulateOpinionTriggerForLevel(List<OPINIONS> p_opinions, int p_level) {
        for (int i = 0; i <= p_level; i++) {
            OPINIONS opinions = opinionTrigger[i];
            p_opinions.Add(opinions);
        }
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
    public bool HasAddedBehaviourForLevel(AFFLICTION_SPECIFIC_BEHAVIOUR p_behaviour, int p_level) {
        return addedBehaviour.HasValueInListUntilIndex(p_level, p_behaviour);
    }
    public void PopulateAddedBehavioursForLevelNoDuplicates(List<AFFLICTION_SPECIFIC_BEHAVIOUR> behaviours, int p_level) {
        for (int i = 0; i <= p_level; i++) {
            AFFLICTION_SPECIFIC_BEHAVIOUR specificBehaviour = addedBehaviour[i];
            if (!behaviours.Contains(specificBehaviour)) {
                behaviours.Add(specificBehaviour);    
            }
        }
    }
    
    public void PopulateAvailableCriteriaForLevel(List<LIST_OF_CRITERIA> p_criteria, int p_level) {
        for (int i = 0; i <= p_level; i++) {
            LIST_OF_CRITERIA criteria = listOfCriteria[i];
            p_criteria.Add(criteria);
        }
    }
}
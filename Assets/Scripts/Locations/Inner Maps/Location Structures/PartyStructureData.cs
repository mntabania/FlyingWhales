using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PartyStructureData
{
    //summon list
    public List<Character> deployedSummons = new List<Character>();
    public List<MonsterAndDemonUnderlingCharges> deployedSummonUnderlings = new List<MonsterAndDemonUnderlingCharges>();

    //minion list
    public List<Character> deployedMinions = new List<Character>();
    public List<MonsterAndDemonUnderlingCharges> deployedMinionUnderlings = new List<MonsterAndDemonUnderlingCharges>();

    //targets
    public List<IStoredTarget> deployedTargets = new List<IStoredTarget>();

    public int deployedSummonCount => deployedSummons.Count;
    public int deployedMinionCount => deployedMinions.Count;

    public int deployedTargetCount => deployedTargets.Count;

    public int maxSummonLimitDeployCount = 5;
    public int readyForDeploySummonCount;
    public int readyForDeployMinionCount;
    public int readyForDeployTargetCount;

    public void ClearAllData() {
        deployedSummons.Clear();
        deployedSummonUnderlings.Clear();
        deployedMinionUnderlings.Clear();
        deployedMinions.Clear();
        deployedTargets.Clear();
        readyForDeployMinionCount = 0;
        readyForDeploySummonCount = 0;
        readyForDeployTargetCount = 0;
    }

    public void ResetAllReadyCounts() {
        readyForDeployMinionCount = 0;
        readyForDeploySummonCount = 0;
        readyForDeployTargetCount = 0;
    }
}

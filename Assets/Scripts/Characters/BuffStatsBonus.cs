using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuffStatsBonus
{
    public int addedGitatedAttackBonus;
    public int addedGitatedHpBonus;

    public void Reset() {
        addedGitatedAttackBonus = 0;
        addedGitatedHpBonus = 0;
    }

    public BuffStatsBonus(SaveDataBuffStatsBonus p_copy) {
        addedGitatedHpBonus = p_copy.addedGitatedHpBonus;
        addedGitatedAttackBonus = p_copy.addedGitatedAttackBonus;
    }

    public BuffStatsBonus(BuffStatsBonus p_copy) {
        addedGitatedHpBonus = p_copy.addedGitatedHpBonus;
        addedGitatedAttackBonus = p_copy.addedGitatedAttackBonus;
    }

    public BuffStatsBonus() {
        addedGitatedHpBonus = 0;
        addedGitatedAttackBonus = 0;
    }

    public void LoadReferences(SaveDataBuffStatsBonus data) {
        addedGitatedAttackBonus = data.addedGitatedAttackBonus;
        addedGitatedHpBonus = data.addedGitatedHpBonus;
    }
}

[System.Serializable]
public class SaveDataBuffStatsBonus : SaveData<BuffStatsBonus> {
    public int addedGitatedAttackBonus;
    public int addedGitatedHpBonus;

    #region Overrides
    public override void Save(BuffStatsBonus data) {
        addedGitatedAttackBonus = data.addedGitatedAttackBonus;
        addedGitatedHpBonus = data.addedGitatedHpBonus;
    }

    public override BuffStatsBonus Load() {
        BuffStatsBonus component = new BuffStatsBonus(this);
        return component;
    }
    #endregion
}

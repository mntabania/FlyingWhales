using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuffStatsBonus
{
    public int originalAttack;
    public int originalHP;

    public void Reset() {
        originalAttack = 0;
        originalHP = 0;
    }

    public BuffStatsBonus(SaveDataBuffStatsBonus p_copy) {
        originalHP = p_copy.addedGitatedHpBonus;
        originalAttack = p_copy.addedGitatedAttackBonus;
    }

    public BuffStatsBonus(BuffStatsBonus p_copy) {
        originalHP = p_copy.originalHP;
        originalAttack = p_copy.originalAttack;
    }

    public BuffStatsBonus() {
        originalHP = 0;
        originalAttack = 0;
    }

    public void LoadReferences(SaveDataBuffStatsBonus data) {
        originalAttack = data.addedGitatedAttackBonus;
        originalHP = data.addedGitatedHpBonus;
    }
}

[System.Serializable]
public class SaveDataBuffStatsBonus : SaveData<BuffStatsBonus> {
    public int addedGitatedAttackBonus;
    public int addedGitatedHpBonus;

    #region Overrides
    public override void Save(BuffStatsBonus data) {
        addedGitatedAttackBonus = data.originalAttack;
        addedGitatedHpBonus = data.originalHP;
    }

    public override BuffStatsBonus Load() {
        BuffStatsBonus component = new BuffStatsBonus(this);
        return component;
    }
    #endregion
}

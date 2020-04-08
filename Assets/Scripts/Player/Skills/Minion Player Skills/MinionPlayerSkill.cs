using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;

public class MinionPlayerSkill : SpellData {
    public override SPELL_CATEGORY category { get { return SPELL_CATEGORY.MINION; } }
    public string className { get; protected set; }

    public MinionPlayerSkill() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.TILE };
    }

    public override void ActivateAbility(LocationGridTile targetTile, ref Character spawnedCharacter) {
        Minion minion = PlayerManager.Instance.player.CreateNewMinion(className, RACE.DEMON, false);
        minion.SetCombatAbility(COMBAT_ABILITY.FLAMESTRIKE);
        PlayerManager.Instance.player.AddMinion(minion);
        minion.Summon(targetTile);
        spawnedCharacter = minion.character;
        base.ActivateAbility(targetTile, ref spawnedCharacter);
    }
}

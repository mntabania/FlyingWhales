using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;

public class MinionPlayerSkill : SpellData {
    public override SPELL_CATEGORY category { get { return SPELL_CATEGORY.MINION; } }
    public RACE race { get; protected set; }
    public string className { get; protected set; }
    //public int spawnCooldown { get; protected set; }

    //private int _currentSpawnCooldown;

    public MinionPlayerSkill() : base() {
        race = RACE.DEMON;
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.TILE };
        //spawnCooldown = 48;
    }

    public override void ActivateAbility(LocationGridTile targetTile, ref Character spawnedCharacter) {
        Minion minion = PlayerManager.Instance.player.CreateNewMinion(className, RACE.DEMON, false);
        minion.SetCombatAbility(COMBAT_ABILITY.FLAMESTRIKE);
        //PlayerManager.Instance.player.AddMinion(minion);
        minion.Summon(targetTile);
        minion.SetMinionPlayerSkillType(type);
        spawnedCharacter = minion.character;
        base.ActivateAbility(targetTile, ref spawnedCharacter);
    }

    //public void StartCooldown() {
    //    _currentSpawnCooldown = 0;
    //    Messenger.AddListener(Signals.TICK_STARTED, PerTickCooldown);
    //}

    //private void PerTickCooldown() {
    //    _currentSpawnCooldown++;
    //    if (_currentSpawnCooldown == spawnCooldown) {
    //        SetCharges(1);
    //        Messenger.RemoveListener(Signals.TICK_STARTED, PerTickCooldown);
    //    }
    //}
}

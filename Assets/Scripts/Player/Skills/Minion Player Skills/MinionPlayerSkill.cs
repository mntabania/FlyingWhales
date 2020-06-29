using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;

public class MinionPlayerSkill : SpellData {
    public override SPELL_CATEGORY category { get { return SPELL_CATEGORY.MINION; } }
    public RACE race { get; protected set; }
    public string className { get; protected set; }
    public int spawnCooldown { get; protected set; }

    public override bool isInCooldown => _currentSpawnCooldown < spawnCooldown;
    
    private int _currentSpawnCooldown;

    public MinionPlayerSkill() : base() {
        race = RACE.DEMON;
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.TILE };
        spawnCooldown = 48;
    }
    public override void ActivateAbility(LocationGridTile targetTile) {
        Minion minion = PlayerManager.Instance.player.CreateNewMinion(className, RACE.DEMON, false);
        minion.SetCombatAbility(COMBAT_ABILITY.FLAMESTRIKE);
        minion.Summon(targetTile);
        minion.SetMinionPlayerSkillType(type);
        base.ActivateAbility(targetTile);
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
    public override void HighlightAffectedTiles(LocationGridTile tile) {
        TileHighlighter.Instance.PositionHighlight(0, tile);
    }
    public void StartCooldown() {
        _currentSpawnCooldown = 0;
        Messenger.AddListener(Signals.TICK_STARTED, PerTickCooldown);
        Messenger.Broadcast(Signals.SPELL_COOLDOWN_STARTED, this as SpellData);
    }

    private void PerTickCooldown() {
        _currentSpawnCooldown++;
        if (_currentSpawnCooldown == spawnCooldown) {
            SetCharges(1);
            Messenger.RemoveListener(Signals.TICK_STARTED, PerTickCooldown);
            Messenger.Broadcast(Signals.SPELL_COOLDOWN_FINISHED, this as SpellData);
        }
    }
}

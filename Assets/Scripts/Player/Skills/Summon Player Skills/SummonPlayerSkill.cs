using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps;

public class SummonPlayerSkill : SpellData {
    public override SPELL_CATEGORY category { get { return SPELL_CATEGORY.SUMMON; } }
    public RACE race { get; protected set; }
    public string className { get; protected set; }
    public SUMMON_TYPE summonType { get; protected set; }

    public SummonPlayerSkill() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.TILE };
    }

    public override void ActivateAbility(LocationGridTile targetTile, ref Character spawnedCharacter) {
        Summon summon = CharacterManager.Instance.CreateNewSummon(summonType, PlayerManager.Instance.player.playerFaction, homeRegion: targetTile.parentMap.region as Region, className: className);
        CharacterManager.Instance.PlaceSummon(summon, targetTile);
        //summon.behaviourComponent.AddBehaviourComponent(typeof(DefaultMinion));
        spawnedCharacter = summon;
        Messenger.Broadcast(Signals.PLAYER_PLACED_SUMMON, summon);
        base.ActivateAbility(targetTile, ref spawnedCharacter);
    }
}
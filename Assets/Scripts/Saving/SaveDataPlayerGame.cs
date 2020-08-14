using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveDataPlayerGame : SaveData<Player> {
    //TODO: Player Faction, Player Settlement

    public int mana;

    public int portalTileXCoordinate;
    public int portalTileYCoordinate;

    //TODO: Minions, Summons

    //Threat
    public int threat;

    //Skills
    public List<SaveDataPlayerSkill> skills;
    //public bool canTriggerFlaw;
    //public bool canRemoveTraits;


    #region Overrides
    public override void Save() {
        base.Save();
        Player player = PlayerManager.Instance.player;
        mana = player.mana;
        portalTileXCoordinate = player.portalTile.data.xCoordinate;
        portalTileYCoordinate = player.portalTile.data.yCoordinate;

        threat = player.threatComponent.threat;

        //canTriggerFlaw = player.playerSkillComponent.canTriggerFlaw;
        //canRemoveTraits = player.playerSkillComponent.canRemoveTraits;

        skills = new List<SaveDataPlayerSkill>();
        for (int i = 0; i < player.playerSkillComponent.spells.Count; i++) {
            SpellData spell = PlayerSkillManager.Instance.GetSpellData(player.playerSkillComponent.spells[i]);
            SaveDataPlayerSkill dataPlayerSkill = new SaveDataPlayerSkill();
            dataPlayerSkill.Save(spell);
            skills.Add(dataPlayerSkill);
        }
        for (int i = 0; i < player.playerSkillComponent.afflictions.Count; i++) {
            SpellData spell = PlayerSkillManager.Instance.GetAfflictionData(player.playerSkillComponent.afflictions[i]);
            SaveDataPlayerSkill dataPlayerSkill = new SaveDataPlayerSkill();
            dataPlayerSkill.Save(spell);
            skills.Add(dataPlayerSkill);
        }
        for (int i = 0; i < player.playerSkillComponent.playerActions.Count; i++) {
            PlayerAction spell = PlayerSkillManager.Instance.GetPlayerActionData(player.playerSkillComponent.playerActions[i]);
            SaveDataPlayerSkill dataPlayerSkill = new SaveDataPlayerSkill();
            dataPlayerSkill.Save(spell);
            skills.Add(dataPlayerSkill);
        }
        for (int i = 0; i < player.playerSkillComponent.demonicStructuresSkills.Count; i++) {
            DemonicStructurePlayerSkill spell = PlayerSkillManager.Instance.GetDemonicStructureSkillData(player.playerSkillComponent.demonicStructuresSkills[i]);
            SaveDataPlayerSkill dataPlayerSkill = new SaveDataPlayerSkill();
            dataPlayerSkill.Save(spell);
            skills.Add(dataPlayerSkill);
        }
        for (int i = 0; i < player.playerSkillComponent.minionsSkills.Count; i++) {
            MinionPlayerSkill spell = PlayerSkillManager.Instance.GetMinionPlayerSkillData(player.playerSkillComponent.minionsSkills[i]);
            SaveDataPlayerSkill dataPlayerSkill = new SaveDataPlayerSkill();
            dataPlayerSkill.Save(spell);
            skills.Add(dataPlayerSkill);
        }
        for (int i = 0; i < player.playerSkillComponent.summonsSkills.Count; i++) {
            SummonPlayerSkill spell = PlayerSkillManager.Instance.GetSummonPlayerSkillData(player.playerSkillComponent.summonsSkills[i]);
            SaveDataPlayerSkill dataPlayerSkill = new SaveDataPlayerSkill();
            dataPlayerSkill.Save(spell);
            skills.Add(dataPlayerSkill);
        }
    }
    public override Player Load() {
        Player player = new Player(this);
        return player;
    }
    #endregion
}

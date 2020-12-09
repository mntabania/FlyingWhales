﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps.Location_Structures;
using Logs;

public class InstigateWarData : SchemeData {
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.INSTIGATE_WAR;
    public override string name => "Instigate War";
    public override string description => "Instigate War";
    public override PLAYER_SKILL_CATEGORY category => PLAYER_SKILL_CATEGORY.SCHEME;

    public InstigateWarData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.CHARACTER };
    }

    #region Overrides
    public override void ActivateAbility(IPointOfInterest targetPOI) {
        if (targetPOI is Character sourceCharacter) {
            Faction sourceFaction = sourceCharacter.faction;
            List<Faction> choices = ObjectPoolManager.Instance.CreateNewFactionList();
            for (int i = 0; i < FactionManager.Instance.allFactions.Count; i++) {
                Faction faction = FactionManager.Instance.allFactions[i];
                if(faction != sourceFaction && faction.factionType.type != FACTION_TYPE.Vagrants && faction.factionType.type != FACTION_TYPE.Demons
                    && faction.HasMemberThatMeetCriteria(c => !c.isDead)) {
                    choices.Add(faction);
                }
            }
            UIManager.Instance.ShowClickableObjectPicker(choices, o => OnChooseFaction(o, sourceCharacter), validityChecker: t => CanInstigateWar(sourceFaction, t), onHoverAction: t => OnHoverEnter(sourceFaction, t), onHoverExitAction: OnHoverExit, showCover: true,
                shouldShowConfirmationWindowOnPick: false, layer: 25);
            ObjectPoolManager.Instance.ReturnFactionListToPool(choices);
        }
    }
    public override bool IsValid(IPlayerActionTarget target) {
        if (target is Character character) {
            return character.isFactionLeader;
        }
        return base.IsValid(target);
    }
    protected override void OnSuccessScheme(Character character, object target) {
        base.OnSuccessScheme(character, target);
        if(target is Faction targetFaction) {
            Character targetFactionMember = targetFaction.characters[0];
            character.interruptComponent.TriggerInterrupt(INTERRUPT.Declare_War, targetFactionMember);
        }
    }
    protected override void PopulateSchemeConversation(List<ConversationData> conversationList, Character character, object target, bool isSuccessful) {
        if (target is Faction targetFaction) {
            ConversationData data = ObjectPoolManager.Instance.CreateNewConversationData($"I need you to declare war on {targetFaction.name}.", null, DialogItem.Position.Right);
            conversationList.Add(data);
        }
        base.PopulateSchemeConversation(conversationList, character, target, isSuccessful);
    }
    #endregion

    private bool CanInstigateWar(Faction source, Faction target) {
        if (source.HasRelationshipStatusWith(FACTION_RELATIONSHIP_STATUS.Hostile, target)) {
            return false;
        }
        return true;
    }
    private void OnHoverEnter(Faction source, Faction target) {
        if (source.HasRelationshipStatusWith(FACTION_RELATIONSHIP_STATUS.Hostile, target)) {
            UIManager.Instance.ShowSmallInfo(UtilityScripts.Utilities.InvalidColorize("Already at war."));
        }
    }
    private void OnHoverExit(Faction source) {
        UIManager.Instance.HideSmallInfo();
    }
    private void OnChooseFaction(object obj, Character source) {
        if (obj is Faction targetFaction) {
            UIManager.Instance.HideObjectPicker();

            //Show Scheme UI
        }
    }
}

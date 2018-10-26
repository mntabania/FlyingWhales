﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;

public class MysteriousSarcophagus : Interaction {

    public MysteriousSarcophagus(IInteractable interactable) : base(interactable, INTERACTION_TYPE.MYSTERIOUS_SARCOPHAGUS) {
        _name = "Mysterious Sarcophagus";
    }

    #region Overrides
    public override void CreateStates() {
        InteractionState startState = new InteractionState("Start", this);
        InteractionState cursedState = new InteractionState("Cursed", this);
        InteractionState accessoryUpgradeState = new InteractionState("Accessory Upgrade", this);
        InteractionState gainManaState = new InteractionState("Gain Mana", this);
        InteractionState gainSuppliesState = new InteractionState("Gain Supplies", this);
        InteractionState awakenUndeadHeroState = new InteractionState("Awaken Undead Hero", this);
        InteractionState gainPositiveState = new InteractionState("Gain Positive Trait", this);

        string startStateDesc = "Our Imp has discovered an unopened sarcophagus in a hidden alcove. Should we open it?";
        startState.SetDescription(startStateDesc);
        CreateActionOptions(startState);

        cursedState.SetEndEffect(() => CursedRewardEffect(cursedState));
        accessoryUpgradeState.SetEndEffect(() => AccessoryUpgradeRewardEffect(accessoryUpgradeState));
        gainManaState.SetEndEffect(() => GainManaRewardEffect(gainManaState));
        gainSuppliesState.SetEndEffect(() => GainSuppliesRewardEffect(gainSuppliesState));
        awakenUndeadHeroState.SetEndEffect(() => AwakenUndeadHeroRewardEffect(awakenUndeadHeroState));
        gainPositiveState.SetEndEffect(() => GainPositiveTraitRewardEffect(gainPositiveState));

        _states.Add(startState.name, startState);
        _states.Add(cursedState.name, cursedState);
        _states.Add(accessoryUpgradeState.name, accessoryUpgradeState);
        _states.Add(gainManaState.name, gainManaState);
        _states.Add(gainSuppliesState.name, gainSuppliesState);
        _states.Add(awakenUndeadHeroState.name, awakenUndeadHeroState);
        _states.Add(gainPositiveState.name, gainPositiveState);

        SetCurrentState(startState);
    }
    public override void CreateActionOptions(InteractionState state) {
        if (state.name == "Start") {
            ActionOption ofCourseOption = new ActionOption {
                interactionState = state,
                cost = new ActionOptionCost { amount = 20, currency = CURRENCY.SUPPLY },
                name = "Of course.",
                description = "We have sent %minion% to open the sarcophagus.",
                duration = 6,
                needsMinion = true,
                neededObjects = new List<System.Type>() { typeof(Minion) },
                effect = () => OfCourseOption(state),
            };
            ActionOption ofCourseNotOption = new ActionOption {
                interactionState = state,
                cost = new ActionOptionCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Of couse not.",
                duration = 0,
                needsMinion = false,
                effect = () => LeaveAloneEffect(state),
            };

            state.AddActionOption(ofCourseOption);
            state.AddActionOption(ofCourseNotOption);
        }
    }
    #endregion

    private void OfCourseOption(InteractionState state) {
        WeightedDictionary<string> effectWeights = new WeightedDictionary<string>();
        effectWeights.AddElement("Cursed", 5);
        effectWeights.AddElement("Accessory Upgrade", 10);
        effectWeights.AddElement("Gain Mana", 15);
        effectWeights.AddElement("Gain Supplies", 5);
        effectWeights.AddElement("Awaken Undead Hero", 5);
        effectWeights.AddElement("Gain Positive Trait", 5);


        string chosenEffect = effectWeights.PickRandomElementGivenWeights();
        if (chosenEffect == "Cursed") {
            CursedRewardState(state, chosenEffect);
        } else if (chosenEffect == "Accessory Upgrade") {
            AccessoryUpgradeRewardState(state, chosenEffect);
        } else if (chosenEffect == "Gain Mana") {
            GainManaRewardState(state, chosenEffect);
        } else if (chosenEffect == "Gain Supplies") {
            GainSuppliesRewardState(state, chosenEffect);
        } else if (chosenEffect == "Awaken Undead Hero") {
            AwakenUndeadHeroRewardState(state, chosenEffect);
        } else if (chosenEffect == "Gain Positive Trait") {
            GainPositiveTraitRewardState(state, chosenEffect);
        }
    }

    #region States
    private void CursedRewardState(InteractionState state, string stateName) {
        _states[stateName].SetDescription(state.chosenOption.assignedMinion.icharacter.name + " opened the sarcophagus and found nothing inside. But everyone felt a strange heaviness in the air and suddenly all of your followers have been cursed with a strange affliction.");
        SetCurrentState(_states[stateName]);
    }
    private void AccessoryUpgradeRewardState(InteractionState state, string stateName) {
        _states[stateName].SetDescription(state.chosenOption.assignedMinion.icharacter.name + " opened the sarcophagus and found a powerful accessory that it can use.");
        SetCurrentState(_states[stateName]);
    }
    private void GainManaRewardState(InteractionState state, string stateName) {
        _states[stateName].SetDescription(state.chosenOption.assignedMinion.icharacter.name + " opened the sarcophagus and found a small magical source. We have converted it into an ample amount of mana.");
        SetCurrentState(_states[stateName]);
    }
    private void GainSuppliesRewardState(InteractionState state, string stateName) {
        _states[stateName].SetDescription(state.chosenOption.assignedMinion.icharacter.name + " opened the sarcophagus and found a small amount of ancient treasures that we have added to our Supplies.");
        SetCurrentState(_states[stateName]);
    }
    private void AwakenUndeadHeroRewardState(InteractionState state, string stateName) {
        _states[stateName].SetDescription(state.chosenOption.assignedMinion.icharacter.name + " opened the sarcophagus and unleashed a powerful mummy. The mummy now guards " + _interactable.specificLocation.thisName + ". " + state.chosenOption.assignedMinion.icharacter.name + " managed to escape");
        SetCurrentState(_states[stateName]);
    }
    private void GainPositiveTraitRewardState(InteractionState state, string stateName) {
        _states[stateName].SetDescription(state.chosenOption.assignedMinion.icharacter.name + " opened the sarcophagus and found it empty. As " + state.chosenOption.assignedMinion.icharacter.name + " leaves, a wisp of sand blows through him and a strange power awakened within.");
        SetCurrentState(_states[stateName]);
    }
    #endregion

    #region State Effects
    private void CursedRewardEffect(InteractionState state) {
        state.assignedMinion.AdjustExp(1);
        //TODO: all of your units gain a random negative Trait (same negative Trait for all)
    }
    private void AccessoryUpgradeRewardEffect(InteractionState state) {
        state.assignedMinion.AdjustExp(1);
        if(state.assignedMinion.icharacter.equippedAccessory != null) {
            state.assignedMinion.icharacter.UpgradeAccessory();
        }
    }
    private void GainManaRewardEffect(InteractionState state) {
        state.assignedMinion.AdjustExp(1);
        PlayerManager.Instance.player.AdjustCurrency(CURRENCY.MANA, 50);
    }
    private void GainSuppliesRewardEffect(InteractionState state) {
        state.assignedMinion.AdjustExp(1);
        PlayerManager.Instance.player.AdjustCurrency(CURRENCY.SUPPLY, 50);
    }
    private void AwakenUndeadHeroRewardEffect(InteractionState state) {
        if(_interactable is BaseLandmark) {
            BaseLandmark landmark = _interactable as BaseLandmark;
            Character zombieEarthbinderHero = CharacterManager.Instance.CreateNewCharacter(CHARACTER_ROLE.HERO, "Earthbinder", RACE.ZOMBIE, GENDER.MALE, PlayerManager.Instance.player.playerFaction, PlayerManager.Instance.player.demonicPortal, false);
            zombieEarthbinderHero.LevelUp(15);
            landmark.AddDefender(zombieEarthbinderHero);
        }
    }
    private void GainPositiveTraitRewardEffect(InteractionState state) {
        state.assignedMinion.AdjustExp(1);
        //TODO: Positive Trait Reward 1
    }
    #endregion
}

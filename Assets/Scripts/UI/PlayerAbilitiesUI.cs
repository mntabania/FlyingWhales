﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;

public class PlayerAbilitiesUI : MonoBehaviour {
    public static PlayerAbilitiesUI Instance;

    public GameObject abilitiesGO;
    public GameObject playerAbilityButtonPrefab;

    private IInteractable _currentlySelectedInteractable;
    private List<PlayerAbilityButton> _playerAbilityButtons;

    #region getters/setters
    public IInteractable currentlySelectedInteractable {
        get { return _currentlySelectedInteractable; }
    }
    #endregion

    void Awake() {
        Instance = this;
    }
	
    public void ShowPlayerAbilitiesUI(IInteractable interactable) {
        _currentlySelectedInteractable = interactable;
        ShowAbilitiesOf(interactable);
        abilitiesGO.SetActive(true);
    }
    public void HidePlayerAbilitiesUI() {
        abilitiesGO.SetActive(false);
    }

    public void ConstructAbilityButtons() {
        _playerAbilityButtons = new List<PlayerAbilityButton>();
        for (int i = 0; i < PlayerManager.Instance.player.allAbilities.Count; i++) {
            PlayerAbility playerAbility = PlayerManager.Instance.player.allAbilities[i];
            GameObject go = GameObject.Instantiate(playerAbilityButtonPrefab, abilitiesGO.transform);
            PlayerAbilityButton playerAbilityButton = go.GetComponent<PlayerAbilityButton>();
            playerAbilityButton.SetPlayerAbility(playerAbility);
            playerAbility.SetPlayerAbilityButton(playerAbilityButton);
            _playerAbilityButtons.Add(playerAbilityButton);
            if(playerAbility.type != ABILITY_TYPE.ALL) {
                go.SetActive(false);
            }
        }
    }

    public void ShowAbilitiesOf(IInteractable interactable) {
        ABILITY_TYPE type = ABILITY_TYPE.CHARACTER;
        if (interactable is BaseLandmark) {
            type = ABILITY_TYPE.STRUCTURE;
        }
        for (int i = 0; i < _playerAbilityButtons.Count; i++) {
            PlayerAbilityButton playerAbilityButton = _playerAbilityButtons[i];
            if(playerAbilityButton.playerAbility.type == ABILITY_TYPE.ALL) {
                continue;
            }
            if(playerAbilityButton.playerAbility.type == type) {
                playerAbilityButton.gameObject.SetActive(true);
            } else {
                playerAbilityButton.gameObject.SetActive(false);
            }
        }
    }
}

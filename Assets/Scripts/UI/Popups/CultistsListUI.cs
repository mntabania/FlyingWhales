using System.Collections.Generic;
using Traits;
using UnityEngine;
using UnityEngine.UI;

public class CultistsListUI : PopupMenuBase {

    [SerializeField] private ScrollRect listScrollRect;
    [SerializeField] private GameObject listItemPrefab;
    [SerializeField] private Toggle cultistsToggle;

    public void Initialize() {
        //check if player can build a defiler,
        if (PlayerManager.Instance.player.playerSkillComponent.CanBuildDemonicStructure(SPELL_TYPE.DEFILER)) {
            //if they can activate cultist toggle
            cultistsToggle.gameObject.SetActive(true);
            Messenger.AddListener<Character, Trait>(Signals.CHARACTER_TRAIT_ADDED, OnCharacterGainedTrait);
            Messenger.AddListener<Character, Trait>(Signals.CHARACTER_TRAIT_REMOVED, OnCharacterRemovedTrait);
        }
        else {
            //else deactivate it
            cultistsToggle.gameObject.SetActive(false);
        }
    }

    #region General
    public void ToggleList(bool isOn) {
        if (isOn) {
            Open();
        } else {
            Close();
        }
    }
    public override void Close() {
        base.Close();
        cultistsToggle.SetIsOnWithoutNotify(false);
    }
    public override void Open() {
        base.Open();
        cultistsToggle.SetIsOnWithoutNotify(true);
    }
    #endregion
    
    #region Listeners
    private void OnCharacterGainedTrait(Character character, Trait trait) {
        if (trait is Cultist) {
            CreateNewItemFor(character);
        }
    }
    private void OnCharacterRemovedTrait(Character character, Trait trait) {
        if (trait is Cultist) {
            RemoveItemOf(character);
        }
    }
    #endregion

    #region List Management
    private void CreateNewItemFor(Character character) {
        GameObject nameplateGO = ObjectPoolManager.Instance.InstantiateObjectFromPool(listItemPrefab.name, Vector3.zero,
            Quaternion.identity, listScrollRect.content);
        CharacterNameplateItem item = nameplateGO.GetComponent<CharacterNameplateItem>();
        item.SetObject(character);
    }
    private void RemoveItemOf(Character character) {
        CharacterNameplateItem[] items =
            UtilityScripts.GameUtilities.GetComponentsInDirectChildren<CharacterNameplateItem>(listScrollRect.content
                .gameObject);
        for (int i = 0; i < items.Length; i++) {
            CharacterNameplateItem item = items[i];
            if (item.character == character) {
                ObjectPoolManager.Instance.DestroyObject(item);
                break;
            }
        }
    }
    #endregion
    
}
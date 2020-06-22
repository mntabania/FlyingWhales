using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Ruinarch;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

public class SkillTreeSelector : MonoBehaviour {

    [SerializeField] private HorizontalScrollSnap _horizontalScrollSnap;
    [SerializeField] private Button continueBtn;

    [SerializeField] private Toggle[] archetypeToggles;

    public PlayerSkillLoadoutUI[] playerLoadoutUI;

    public void Show() {
        for (int i = 0; i < playerLoadoutUI.Length; i++) {
            playerLoadoutUI[i].Initialize();
        }
        this.gameObject.SetActive(true);
        _horizontalScrollSnap.GoToScreen(0);
    }

    public void Hide() {
        this.gameObject.SetActive(false);
    }

    public void OnClickContinue() {
        // Hide();
        continueBtn.interactable = false;
        for (int i = 0; i < archetypeToggles.Length; i++) {
            if (archetypeToggles[i].isOn) {
                PlayerSkillManager.Instance.SetSelectedArchetype((PLAYER_ARCHETYPE) System.Enum.Parse(typeof(PLAYER_ARCHETYPE), UtilityScripts.Utilities.NotNormalizedConversionStringToEnum(archetypeToggles[i].gameObject.name)));
                break;
            }
        }
        Messenger.Broadcast(Signals.START_GAME_AFTER_LOADOUT_SELECT);
        MainMenuUI.Instance.StartNewGame();
    }

    private void OnScreenResolutionChanged() {
        StartCoroutine(_horizontalScrollSnap.UpdateLayoutCoroutine());
    }
    
    // #region Monobehaviours
    // private Vector2Int currentResolution;
    // private void Start () {
    //     currentResolution = new Vector2Int(Screen.width, Screen.height);
    // }
    //  
    // private void Update () {
    //     if (currentResolution.x != Screen.width || currentResolution.y != Screen.height) {
    //         //Do stuff
    //         currentResolution = new Vector2Int(Screen.width, Screen.height);
    //         OnScreenResolutionChanged();
    //     }
    // }
    // #endregion
}

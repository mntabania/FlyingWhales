using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class InitialWorldSetupMenu : MonoBehaviour  {

    [SerializeField] public SkillTreeSelector loadOutMenu;
    [SerializeField] public Button regenerateWorldBtn;
    [SerializeField] public Button configureLoadoutBtn;

    public void Initialize() {
        loadOutMenu.Initialize();
    }
    
    public void Show() {
        UIManager.Instance.SetSpeedTogglesState(false);

        regenerateWorldBtn.interactable =
            WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Custom;
        
        gameObject.SetActive(true);
    }
    public void Hide() {
        UIManager.Instance.SetSpeedTogglesState(true);
        gameObject.SetActive(false);
    }

    #region Reload World
    public void OnClickReloadWorld() {
        UIManager.Instance.ShowYesNoConfirmation("Regenerate World", 
            "Are you sure you want to regenerate a new World?", ReGenerateWorld);
    }
    private void ReGenerateWorld() {
        DOTween.Clear(true);
        Messenger.Cleanup();
        MainMenuManager.Instance.StartNewGame();
    }
    #endregion

    #region Loadout Menu
    public void OnClickConfigureLoadOut() {
        loadOutMenu.Show();
    }
    #endregion
}

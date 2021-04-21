using System;
using Ruinarch.Custom_UI;
using Ruinarch.MVCFramework;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

public class TutorialUIModel : MVCUIModel {

    public ScrollRect scrollRectTutorialItems;
    public RuinarchButton btnClose;
    public ToggleGroup toggleGroupTutorialItems;
    public HorizontalScrollSnap tutorialPagesScrollSnap;
    public RectTransform tutorialPaginationParent;
    public GameObject goTutorialPages;

    public System.Action onClickClose;
    
    private void OnEnable() {
        btnClose.onClick.AddListener(OnClickClose);
    }
    private void OnDisable() {
        btnClose.onClick.RemoveListener(OnClickClose);
    }

    #region Click Actions
    private void OnClickClose() {
        onClickClose?.Invoke();
    }
    #endregion
}

using UnityEngine.UI;
using UnityEngine;
using System;
using EZObjectPools;
using Ruinarch.Custom_UI;
using TMPro;

public class ContextMenuUIObject : PooledObject {
    public static Action<IContextMenuItem, bool, int> onMenuPress;
    public static Action<IContextMenuItem, bool, int> onHoverOverItem;
    public static Action<IContextMenuItem, bool, int> onHoverOutItem;
    
    public Image ImgIcon;
    public TextMeshProUGUI txtMenuName;
    public RuinarchButton btnActivate;
    public GameObject goArrow;
    public Image coverImg;

    public HoverHandler hoverHandler;

    private IContextMenuItem m_parentUIMenu;
    private bool m_isAction;
    private int m_menuColumn;

	private void OnEnable() {
        btnActivate.onClick.AddListener(ButtonClicked);
        hoverHandler.AddOnHoverOverAction(HoverOver);
        hoverHandler.AddOnHoverOutAction(HoverOut);
    }
    private void OnDisable() {
        btnActivate.onClick.RemoveListener(ButtonClicked);
        hoverHandler.RemoveOnHoverOverAction(HoverOver);
        hoverHandler.RemoveOnHoverOverAction(HoverOut);
    }
    private void Update() {
        if (m_parentUIMenu != null) {
            bool canBePicked = m_parentUIMenu.CanBePicked();
            coverImg.gameObject.SetActive(!canBePicked);
            btnActivate.interactable = canBePicked;
            if (coverImg.gameObject.activeSelf) {
                coverImg.fillAmount = m_parentUIMenu.GetCoverFillAmount();    
            }    
        }
    }
    public void SetMenuDetails(IContextMenuItem p_parentUIMenu) {
        ImgIcon.gameObject.SetActive(p_parentUIMenu.contextMenuIcon != null);
        btnActivate.name = p_parentUIMenu.contextMenuName;
        ImgIcon.sprite = p_parentUIMenu.contextMenuIcon;
        txtMenuName.text = p_parentUIMenu.contextMenuName;
        m_parentUIMenu = p_parentUIMenu;
        bool canBePicked = p_parentUIMenu.CanBePicked();
        btnActivate.interactable = canBePicked;
        coverImg.gameObject.SetActive(!canBePicked);
        if (coverImg.gameObject.activeSelf) {
            coverImg.fillAmount = p_parentUIMenu.GetCoverFillAmount();    
        }
        bool hasSubMenu = m_parentUIMenu.subMenus != null && m_parentUIMenu.subMenus.Count > 0;
        goArrow.gameObject.SetActive(hasSubMenu);
        if (!hasSubMenu) {
            m_isAction = true;
        }
        m_menuColumn = p_parentUIMenu.contextMenuColumn;
    }

    #region Interactions
    private void ButtonClicked() {
        onMenuPress?.Invoke(m_parentUIMenu, m_isAction, m_menuColumn);
    }
    private void HoverOver() {
        onHoverOverItem?.Invoke(m_parentUIMenu, m_isAction, m_menuColumn);
    }
    private void HoverOut() {
        onHoverOutItem?.Invoke(m_parentUIMenu, m_isAction, m_menuColumn);
    }
    #endregion

    #region Object Pool
    public override void Reset() {
        base.Reset();
        btnActivate.name = "ClickableButton";
        m_isAction = false;
    }
    #endregion
}

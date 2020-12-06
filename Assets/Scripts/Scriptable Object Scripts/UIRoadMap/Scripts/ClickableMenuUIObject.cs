using System.Collections;
using UnityEngine.UI;
using UnityEngine;
using System;

public class ClickableMenuUIObject : MonoBehaviour
{
    public static Action<ParentUIData, bool, int> onMenuPress;
    
    public Image ImgIcon;
    public Text txtMenuName;
    public Text txtCoolDown;
    public Button btnActivate;

    private ParentUIData m_parentUIMenu;

    private bool m_isAction;
    private int m_menuColumn;

	private void OnEnable()
	{
        btnActivate.onClick.AddListener(ButtonClicked);
    }
    private void OnDisable()
    {
        btnActivate.onClick.RemoveListener(ButtonClicked);
    }

    public void SetMenuDetails(ParentUIData p_parentUIMenu) 
    {
        ImgIcon.sprite = p_parentUIMenu.clickableMenuData.sprtIcon;
        txtMenuName.text = p_parentUIMenu.clickableMenuData.strMenuName;
        txtCoolDown.text = "60s";
        m_parentUIMenu = p_parentUIMenu;

        if (m_parentUIMenu.subMenu.Count <= 0) 
        {
            m_isAction = true;
        }

        m_menuColumn = p_parentUIMenu.clickableMenuData.column;
    }

    void ButtonClicked() 
    {
        onMenuPress?.Invoke(m_parentUIMenu, m_isAction, m_menuColumn);
    }
}

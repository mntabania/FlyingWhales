using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Ruinarch;
using UnityEngine;

public class ContextMenuTester : MonoBehaviour {
    [SerializeField] private List<ClickableMenuData> m_initialItems;
    [SerializeField] private ContextMenuUIController m_contextMenuController;
    private void Start() {
        ObjectPoolManager.Instance.InitializeObjectPools();
        m_contextMenuController.ShowContextMenu(m_initialItems.Select(x => x as IContextMenuItem).ToList(), new Vector3(0, Screen.height), "Test", InputManager.Cursor_Type.Default);
    }
    private void Update() {
        if (Input.GetMouseButtonDown(1)) {
            m_contextMenuController.ShowContextMenu(m_initialItems.Select(x => x as IContextMenuItem).ToList(), Input.mousePosition, "Test", InputManager.Cursor_Type.Default);
        }
    }
}

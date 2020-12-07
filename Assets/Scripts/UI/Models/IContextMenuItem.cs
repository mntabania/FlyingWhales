using System.Collections.Generic;
using UnityEngine;

public interface IContextMenuItem {
    Sprite contextMenuIcon { get; }
    string contextMenuName { get; }
    int contextMenuColumn { get; }
    List<IContextMenuItem> subMenus { get; }
    
    void OnPickAction();
    bool CanBePicked();
    float GetCoverFillAmount();
}

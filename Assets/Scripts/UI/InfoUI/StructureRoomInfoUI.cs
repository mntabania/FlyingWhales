using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using TMPro;
using UnityEngine;

public class StructureRoomInfoUI : InfoUIBase {

    [Space(10)]
    [Header("Basic Info")]
    [SerializeField] private TextMeshProUGUI nameLbl;

    public StructureRoom activeRoom { get; private set; }

    #region Overrides
    public override void CloseMenu() {
        base.CloseMenu();
        if(activeRoom != null) {
            Selector.Instance.Deselect();
        }
        activeRoom = null;
    }
    public override void OpenMenu() {
        activeRoom = _data as StructureRoom;
        // if(activeRoom.structureObj != null && activeRoom.structureObj.gameObject) {
        //     bool instantCenter = !InnerMapManager.Instance.IsShowingInnerMap(activeRoom.location);
        //     InnerMapCameraMove.Instance.CenterCameraOn(activeRoom.structureObj.gameObject, instantCenter);
        // }
        base.OpenMenu();
        Selector.Instance.Select(activeRoom);
        UpdateInfo();
    }
    #endregion

    public void UpdateInfo() {
        if(activeRoom == null) {
            return;
        }
        UpdateBasicInfo();
        //UpdateCharacters();
    }
    private void UpdateBasicInfo() {
        nameLbl.text = $"{activeRoom.name}";
    }
}

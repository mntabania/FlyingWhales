using UnityEngine;
using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using Pathfinding;
using System;

public class LandmarkVisual : MonoBehaviour {

    private BaseLandmark _landmark;

    [SerializeField] private TextMeshProUGUI landmarkLbl;
    [SerializeField] private SpriteRenderer topSprite;
    [SerializeField] private SpriteRenderer botSprite;
    [SerializeField] private GameObject exploredGO;
    [SerializeField] private GameObject landmarkNameplateGO;
    [SerializeField] private SpriteRenderer iconSprite;
    [SerializeField] private Slider hpProgressBar;
    public Canvas landmarkCanvas;
    public GameObject landmarkHPGO;

    #region getters/setters
    public BaseLandmark landmark {
        get { return _landmark; }
    }
    #endregion

    public void SetLandmark(BaseLandmark landmark) {
        _landmark = landmark;
        UpdateName();
        LandmarkData data = LandmarkManager.Instance.GetLandmarkData(_landmark.specificLandmarkType);
        if (data.landmarkObjectSprite != null) {
            iconSprite.sprite = data.landmarkObjectSprite;
        }
    }
    public void UpdateName() {
        if (landmarkLbl != null) {
            //Landmark object is an empty city
            if (!string.IsNullOrEmpty(_landmark.landmarkName)) {
                landmarkLbl.text = UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLetterOnly(_landmark.landmarkName);
            } else {
                landmarkLbl.text = UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLetterOnly(_landmark.specificLandmarkType.ToString());
            }
        }
    }
    public void SetIconState(bool state) {
        iconSprite.gameObject.SetActive(state);
    }
    public void UpdateProgressBar() {
        //hpProgressBar.value = (float) _landmark.landmarkObj.currentHP / (float) _landmark.landmarkObj.maxHP;
    }
    //public void OnCharacterEnteredLandmark(Party iparty) {
    //    //add character portrait to grid
    //    //CharacterPortrait portrait = iparty.icon.characterPortrait;
    //    //portrait.transform.SetParent(hoverContent);
    //    //portrait.ignoreInteractions = true;
    //    //UpdateCharCount();
    //    //portrait.transform.localScale = Vector3.one;
    //    //(portrait.transform as RectTransform).sizeDelta = new Vector2(64, 64);
    //    //ShowPartyPortrait(iparty);
    //    //iparty.icon.SetVisualState(false);
    //}
    //public void OnCharacterExitedLandmark(Party iparty) {
    //    //remove character portrait from grid
    //    //iparty.icon.gameObject.SetActive(true);
    //    //if (!iparty.icon.avatarVisual.activeSelf) {
    //        //iparty.icon.ReclaimPortrait();
    //        //UpdateCharCount();
    //        //iparty.icon.SetVisualState(true);
    //        //iparty.icon.characterPortrait.SetBorderState(false);
    //    //}
    //}
}

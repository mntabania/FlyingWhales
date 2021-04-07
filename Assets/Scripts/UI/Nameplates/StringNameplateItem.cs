using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StringNameplateItem : NameplateItem<string> {

    [SerializeField] private LocationPortrait _locationPortrait;
    [SerializeField] private TextMeshProUGUI additionalText;
    public Image img;

    public string identifier { get; private set; }
    public string str { get; private set; }

    public override void SetObject(string o) {
        base.SetObject(o);
        str = o;
        this.name = str;
        button.name = str;
        toggle.name = str;
        
        identifier = string.Empty;
        mainLbl.text = str;
        additionalText.text = string.Empty;
    }

    public void SetIdentifier(string id) {
        identifier = id;
        _locationPortrait.gameObject.SetActive(false);
        img.gameObject.SetActive(false);

        if (identifier == "Landmark") {
            _locationPortrait.gameObject.SetActive(true);
            string landmarkName = str.Replace(' ', '_');
            LANDMARK_TYPE landmark = (LANDMARK_TYPE)Enum.Parse(typeof(LANDMARK_TYPE), landmarkName.ToUpper());
            _locationPortrait.SetPortrait(landmark.GetStructureType());
            _locationPortrait.disableInteraction = true;
        } else if (identifier == "Intervention Ability") {
            img.gameObject.SetActive(true);
            img.sprite = PlayerManager.Instance.GetJobActionSprite(str);
        } else if (identifier == "player skill") {
            img.gameObject.SetActive(true);
            img.sprite = PlayerSkillManager.Instance.GetScriptableObjPlayerSkillData<PlayerSkillData>((PLAYER_SKILL_TYPE) System.Enum.Parse(typeof(PLAYER_SKILL_TYPE), UtilityScripts.Utilities.NotNormalizedConversionStringToEnum(str).ToUpper())).buttonSprite;
        } 
        // else if (identifier == "Trigger Flaw") {
        //     additionalText.text = $"{PlayerSkillManager.Instance.GetPlayerActionData(SPELL_TYPE.TRIGGER_FLAW).manaCost.ToString()} {UtilityScripts.Utilities.ManaIcon()}";
        // }
    }
    public override void Reset() {
        base.Reset();
        button.name = "Button";
        toggle.name = "Toggle";
    }
}

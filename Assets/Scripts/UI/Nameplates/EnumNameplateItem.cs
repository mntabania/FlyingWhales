using System;
using UnityEngine;
using UnityEngine.UI;

public class EnumNameplateItem : NameplateItem<Enum> {

    [SerializeField] private Image portrait;
    
    public override void SetObject(Enum o) {
        base.SetObject(o);
        mainLbl.text = UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLetters(o.ToString());
        subLbl.text = string.Empty;
    }
    public void SetPortrait(Sprite sprite) {
        portrait.sprite = sprite;
        portrait.gameObject.SetActive(portrait.sprite != null);
        
    }
}

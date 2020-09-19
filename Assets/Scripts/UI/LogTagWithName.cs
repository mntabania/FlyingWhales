using EZObjectPools;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LogTagWithName : PooledObject {

    [SerializeField] private Image tagImg;
    [SerializeField] private TextMeshProUGUI tagName;

    public void SetTag(LOG_TAG tag) {
        tagImg.sprite = UIManager.Instance.GetLogTagSprite(tag);
        tagName.text = UtilityScripts.Utilities.NotNormalizedConversionEnumToString(tag.ToString());
    }
}

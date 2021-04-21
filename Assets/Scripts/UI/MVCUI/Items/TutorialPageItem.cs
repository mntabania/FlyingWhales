using EZObjectPools;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TutorialPageItem : PooledObject {
    [SerializeField] private Image imgMain;
    [SerializeField] private TextMeshProUGUI lblDescription;

    public void Initialize(TutorialPage p_page) {
        imgMain.sprite = p_page.imgTutorial;
        lblDescription.text = p_page.description;
    }
    public override void Reset() {
        base.Reset();
        imgMain.sprite = null;
    }
}

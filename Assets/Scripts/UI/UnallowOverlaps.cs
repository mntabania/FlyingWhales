using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnallowOverlaps : MonoBehaviour
{
    public RectTransform rectTransform { get; private set; }
    public new OVERLAP_UI_TAG tag;

    private Vector3 _defaultLocalPosition;

    #region getters
    public Vector2 anchoredOffsetMin {
        get {
            Vector2 min = rectTransform.anchorMin;
            min.x *= Screen.width;
            min.y *= Screen.height;

            min += rectTransform.offsetMin;
            return min;
        }
    }
    public Vector2 anchoredOffsetMax {
        get {
            Vector2 max = rectTransform.anchorMax;
            max.x *= Screen.width;
            max.y *= Screen.height;

            max += rectTransform.offsetMax;
            return max;
        }
    }
    #endregion

    public void Initialize() {
        _defaultLocalPosition = rectTransform.anchoredPosition;
    }
    void OnEnable() {
        if(rectTransform == null) {
            rectTransform = gameObject.GetComponent<RectTransform>();
            UIManager.Instance.AddUnallowOverlapUI(this);
            Initialize();
        }
        UnallowOverlaps overlappedUI = UIManager.Instance.GetOverlappedUI(this);
        if(overlappedUI != null) {
            if (overlappedUI.tag == OVERLAP_UI_TAG.Top && tag == OVERLAP_UI_TAG.Bottom) {
                Reposition(overlappedUI);
            } else if(overlappedUI.tag == OVERLAP_UI_TAG.Bottom && tag == OVERLAP_UI_TAG.Top) {
                overlappedUI.Reposition(this);
            }
        } else {
            //When UI is shown and there is no overlapping UI always put it to the default position, left edge of the screen
            DefaultPosition();

            overlappedUI = UIManager.Instance.GetOverlappedUI(this);
            if (overlappedUI != null) {
                if (overlappedUI.tag == OVERLAP_UI_TAG.Top && tag == OVERLAP_UI_TAG.Bottom) {
                    Reposition(overlappedUI);
                } else if (overlappedUI.tag == OVERLAP_UI_TAG.Bottom && tag == OVERLAP_UI_TAG.Top) {
                    overlappedUI.Reposition(this);
                }
            }
        }
    }

    public void Reposition(UnallowOverlaps overlappedTop) {
        //Get the new x position for the bottom UI
        //If the difference between anchoredOffsetMin.x of the overlappedTop and the this bottom UI width is less than zero (i.e. overlappedTop.anchoredOffsetMin.x - rectTransform.rect.width < 0f) then it means that the bottom UI will go off screen to the left
        //meaning, that it is not fit to the left, it must be automatically be put to the right of the overlappedTop, the right of the overlappedTop is overlappedTop.anchoredOffsetMax.x

        float newXPos = overlappedTop.anchoredOffsetMin.x - rectTransform.rect.width;
        if(newXPos < 0f) {
            //Overshoot to the left, meaning it cannot fit to the left of overlappedTop because the UI will go offscreen
            //newXPos will be to the right of the overlappedTop
            newXPos = overlappedTop.anchoredOffsetMax.x;
        }
        transform.localPosition = new Vector3(newXPos, transform.localPosition.y, transform.localPosition.z);
        //string log = "offset min: " + anchoredOffsetMin + ", offset max: " + anchoredOffsetMax + ", width: " + rectTransform.rect.width + ", localPos: " + transform.localPosition + ", rect pos: + " + rectTransform.rect.position;
        //Debug.LogWarning(log);
    }
    private void DefaultPosition() {
        rectTransform.anchoredPosition = _defaultLocalPosition;
    }
}

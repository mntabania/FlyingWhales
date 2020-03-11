using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(ScrollRect))]
public class VerticalScroller : MonoBehaviour {

    private ScrollRect scrollView;
    // private TweenPosition tweener;
    [SerializeField] private Button scrollUpBtn;
    [SerializeField] private Button scrollDownBtn;
    [SerializeField] private float elementHeight;

    private void Awake() {
        scrollView = this.GetComponent<ScrollRect>();
        // tweener = scrollView.content.gameObject.GetComponent<TweenPosition>();
    }

    public void ScrollUp() {
        // tweener.from = scrollView.content.transform.localPosition;
        // tweener.to = scrollView.content.transform.localPosition;
        // tweener.to.y -= elementHeight;
        // tweener.ResetToBeginning();
        // tweener.enabled = true;
        // tweener.PlayForward();
    }

    public void ScrollDown() {
        // tweener.from = scrollView.content.transform.localPosition;
        // tweener.to = scrollView.content.transform.localPosition;
        // tweener.to.y += elementHeight;
        // tweener.ResetToBeginning();
        // tweener.enabled = true;
        // tweener.PlayForward();
    }

    public void OnScroll(Vector2 scrollPos) {
        if (scrollUpBtn != null && scrollDownBtn != null) {
            if (scrollView.content.rect.height <= scrollView.viewport.rect.height) {
                scrollDownBtn.gameObject.SetActive(false);
                scrollUpBtn.gameObject.SetActive(false);
                return;

            }
            if (Mathf.Approximately(scrollView.verticalNormalizedPosition, 0f)) { //bottom
                //disable scroll down btn
                scrollDownBtn.gameObject.SetActive(false);
            } else {
                scrollDownBtn.gameObject.SetActive(true);
            }
            if (Mathf.Approximately(scrollView.verticalNormalizedPosition, 1f)) { //top
                //disable scroll up btn
                scrollUpBtn.gameObject.SetActive(false);
            } else {
                scrollUpBtn.gameObject.SetActive(true);
            }

        }
    }

}

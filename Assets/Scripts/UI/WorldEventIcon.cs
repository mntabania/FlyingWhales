using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class WorldEventIcon : MonoBehaviour {

    private Region region;

    public void PlaceAt(Region region) {
        this.region = region;
        this.transform.position = region.coreTile.transform.position;
        this.transform.localScale = Vector3.one;
    }

    private void UpdatePosition() {
        if (region == null) {
            return;
        }
        //Vector2 originalPos = npcSettlement.coreTile.transform.position;
        //originalPos.y -= 1f;
        //Vector2 ScreenPosition = Camera.main.WorldToScreenPoint(npcSettlement.nameplatePos);
        this.transform.position = region.coreTile.transform.position;

        //Vector2 landmarkViewportPos = Camera.main.WorldToViewportPoint(landmark.tileLocation.transform.position);
        ////Vector2 cameraViewportPos = Camera.main.WorldToViewportPoint(this.transform.position);
        //if (landmarkViewportPos.x >= 0f && landmarkViewportPos.x <= 1f &&
        //    landmarkViewportPos.y >= 0f && landmarkViewportPos.y <= 1f) {
        //    //landmark is within camera view
        //    this.transform.position = landmark.tileLocation.transform.position;
        //}
        ////else if (cameraViewportPos.x >= 0f  && cameraViewportPos.x <= 1f &&
        ////    cameraViewportPos.y >= 0f && cameraViewportPos.y <= 1f) {
        ////    //point is in camera do nothing
        ////} 
        //else {
        //    //point is outside camera
        //    Vector3 newPos = landmarkViewportPos;
        //    float xAdjustment = 0f;
        //    float yAdjustment = 0f;
        //    if (landmarkViewportPos.x < 0f || landmarkViewportPos.x > 1f) {
        //        xAdjustment = landmarkViewportPos.x * -1f;
        //    }
        //    if (landmarkViewportPos.y < 0f || landmarkViewportPos.y > 1f) {
        //        yAdjustment = landmarkViewportPos.y * -1f;
        //    }
        //    if (xAdjustment != 0f) {
        //        newPos.x += xAdjustment;
        //    }
        //    if (yAdjustment != 0f) {
        //        newPos.y += yAdjustment;
        //    }
        //    newPos = Camera.main.ViewportToWorldPoint(newPos);
        //    newPos.z = 0f;
        //    this.transform.position = newPos;
        //    //Debug.Log("Point is outside camera: " + cameraViewportPos.ToString());
        //}
    }

    public void Update() {
        UpdatePosition();
    }

    public void OnClick(BaseEventData data) {
        UIManager.Instance.ShowRegionInfo(region);
    }
    public void OnHoverEnter(BaseEventData data) {
        region.coreTile.OnPointerEnter(data);
    }
    public void OnHoverExit(BaseEventData data) {
        region.coreTile.OnPointerExit(data);
    }
}

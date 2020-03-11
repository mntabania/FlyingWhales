using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class BezierCurveManager : MonoBehaviour {
    public static BezierCurveManager Instance;

    public int numOfInterpolations;
    public GameObject curvePrefab;
    public GameObject curveParentPrefab;
    public LineRenderer lineRenderer;
    public Transform startPoint;
    public Transform endPoint;
    public Transform controlPoint1;
    public Transform controlPoint2;

    private List<BezierCurveParent> _curveParents;
    private List<TravelLineParent> _travelLineParents;

    private void Awake() {
        Instance = this;
    }
    private void Start() {
        _curveParents = new List<BezierCurveParent>();
        _travelLineParents = new List<TravelLineParent>();
    }
    //private void Update() {
    //    DrawCubicCurveTesting(startPoint.position, endPoint.position);
    //}

    public BezierCurve DrawCubicCurve(Vector3 startPoint, Vector3 endPoint, int numOfTicks, DIRECTION direction) {
        Vector3 dir = endPoint - startPoint;

        if (dir == Vector3.zero) {
            return null;
        }
        GameObject go = GameObject.Instantiate(curvePrefab, GridMap.Instance.gameObject.transform);
        BezierCurve bezierCurve = go.GetComponent<BezierCurve>();
        bezierCurve.Initialize(startPoint, endPoint);
        int numOfPositions = numOfTicks;
        int numOfPositionsMultiplier = 2;
        while (numOfPositions < numOfInterpolations) {
            numOfPositions = numOfTicks * numOfPositionsMultiplier;
            numOfPositionsMultiplier++;
        }
        bezierCurve.SetProgressAmount(numOfPositionsMultiplier - 1);

        Vector3 normal = Vector3.Cross(Vector3.up, dir);
        if (startPoint.x != endPoint.x && direction == DIRECTION.DOWN) {
            normal = Vector3.Cross(Vector3.down, dir);
        }
        Vector3 normalUp = Vector3.Cross(dir, normal);

        normalUp = normalUp.normalized;
        float multiplier =  1f - (Vector3.Distance(startPoint, endPoint) * 0.1f);
        multiplier = Mathf.Clamp(multiplier, 0.5f, 1f);
        //normalUp *= dir.magnitude * 1f;
       
        //Vector3 controlPoint1 = new Vector3(normalUp.x + 0.5f, normalUp.y, normalUp.z);
        //Vector3 controlPoint2 = new Vector3(normalUp.x - 0.5f, normalUp.y, normalUp.z);
        //Vector3 curveSharpness = normalUp * dir.magnitude * multiplier;

        //Vector3 p1c = startPoint + curveSharpness;
        //Vector3 p2c = endPoint + curveSharpness;
        //controlPoint1.position = p1c;
        //controlPoint2.position = p2c;


        //float timeDivisor = (float) numOfPositions;
        //float t = 0;
        Vector3[] positions = new Vector3[numOfPositions];
        for (int i = 1; i <= numOfPositions; i++) {
            //t = i / timeDivisor;
            //Vector3 curvePoint = AstarSplines.CubicBezier(startPoint, p1c, p2c, endPoint, t);
            //positions[i - 1] = curvePoint;
        }
        bezierCurve.SetPositions(positions);
        return bezierCurve;
    }
    public void DrawCubicCurveTesting(Vector3 startPoint, Vector3 endPoint) {
        Vector3 dir = endPoint - startPoint;

        if (dir == Vector3.zero) {
            return;
        }
        lineRenderer.positionCount = numOfInterpolations;
        Vector3 normal = Vector3.Cross(Vector3.up, dir);
        Vector3 normalUp = Vector3.Cross(dir, normal);

        normalUp = normalUp.normalized;
        float multiplier = 1f - (Vector3.Distance(startPoint, endPoint) * 0.1f);
        multiplier = Mathf.Clamp(multiplier, 0.5f, 1f);
        //normalUp *= dir.magnitude * 1f;

        //Vector3 controlPoint1 = new Vector3(normalUp.x + 0.5f, normalUp.y, normalUp.z);
        //Vector3 controlPoint2 = new Vector3(normalUp.x - 0.5f, normalUp.y, normalUp.z);
        //Vector3 curveSharpness = normalUp * dir.magnitude * multiplier;

        //Vector3 p1c = startPoint + curveSharpness;
        //Vector3 p2c = endPoint + curveSharpness;
        //controlPoint1.position = p1c;
        //controlPoint2.position = p2c;


        //float timeDivisor = (float) numOfInterpolations;
        //float t = 0;
        //for (int i = 1; i <= numOfInterpolations; i++) {
        //    //t = i / timeDivisor;
        //    //Vector3 curvePoint = AstarSplines.CubicBezier(startPoint, p1c, p2c, endPoint, t);
        //    //lineRenderer.SetPosition(i - 1, curvePoint);
        //}
    }
    public void DrawCubicCurve(Vector3 startPoint, Vector3 endPoint, Vector3 controlPoint1, Vector3 controlPoint2, int numOfInterpolations) {
        lineRenderer.positionCount = numOfInterpolations;

        //float timeDivisor = (float) numOfInterpolations;
        //float t = 0;
        for (int i = 1; i <= numOfInterpolations; i++) {
            //t = i / timeDivisor;
            //Vector3 curvePoint = AstarSplines.CubicBezier(startPoint, controlPoint1, controlPoint2, endPoint, t);
            //lineRenderer.SetPosition(i - 1, curvePoint);
        }
    }
    public BezierCurveParent GetCurveParent(Vector3 startPos, Vector3 endPos) {
        for (int i = 0; i < _curveParents.Count; i++) {
            if(_curveParents[i].startPos == startPos && _curveParents[i].endPos == endPos) {
                return _curveParents[i];
            }
        }
        return null;
    }
    public BezierCurveParent CreateNewCurveParent(Vector3 startPos, Vector3 endPos) {
        GameObject go = GameObject.Instantiate(curveParentPrefab, GridMap.Instance.gameObject.transform);
        BezierCurveParent bezierCurveParent = go.GetComponent<BezierCurveParent>();
        bezierCurveParent.SetStartAndEndPositions(startPos, endPos);
        return bezierCurveParent;
    }
    public void AddCurveParent(BezierCurveParent curveParent) {
        _curveParents.Add(curveParent);
    }
    public void RemoveCurveParent(BezierCurveParent curveParent) {
        _curveParents.Remove(curveParent);
        GameObject.Destroy(curveParent.gameObject);
    }

    [ContextMenu("Get Variables")]
    public void GetVariables() {
        Vector3 dir = endPoint.position - startPoint.position;

        Vector3 normal = Vector3.Cross(Vector3.up, dir);
        Vector3 normalUp = Vector3.Cross(dir, normal);

        normalUp = normalUp.normalized;
        normalUp *= dir.magnitude * 0.1f;


        Debug.LogWarning($"Distance: {Vector3.Distance(startPoint.position, endPoint.position)}");
        Debug.LogWarning($"Dir: {dir}");
        Debug.LogWarning($"Up: {Vector3.up}");
        Debug.LogWarning($"Normal: {normal}");
        Debug.LogWarning($"NormalUp: {normalUp}");
    }

    #region Travel Lines
    public void AddTravelLineParent(TravelLineParent travelLineParent) {
        _travelLineParents.Add(travelLineParent);
    }
    public void RemoveTravelLineParent(TravelLineParent travelLineParent) {
        _travelLineParents.Remove(travelLineParent);
    }
    public TravelLineParent GetTravelLineParent(HexTile startTile, HexTile endTile) {
        for (int i = 0; i < _travelLineParents.Count; i++) {
            if (_travelLineParents[i].startPos == startTile && _travelLineParents[i].endPos == endTile) {
                return _travelLineParents[i];
            }
        }
        return null;
    }
    #endregion
}

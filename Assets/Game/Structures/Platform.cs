using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.U2D;

using Random = UnityEngine.Random;
using RandomParams = GameRules.RandomParams;

public class Platform : MonoBehaviour {

    [System.Serializable]
    public class PlatformSegment {

        public float width;
        public float length;
        public Vector3 midPoint;

        [HideInInspector] public Vector3 leftPointA;
        [HideInInspector] public Vector3 rightPointA;
        [HideInInspector] public Vector3 leftPointB;
        [HideInInspector] public Vector3 rightPointB;


        public void Randomize(RandomParams rWidth, RandomParams rLength, RandomParams rOffset, bool round) {
            if (!round) {
                width = rWidth.Get();
                length = rLength.Get();
                midPoint = rOffset.Get() * (Vector3)(Random.insideUnitCircle.normalized);
            }
            else {
                width = rWidth.GetRound();
                length = rLength.GetRound();
                midPoint = rOffset.GetRound() * (Vector3)(Random.insideUnitCircle.normalized);
            }
        }

        public void Set(Vector3 center, float width, float length) {
            this.width = width;
            this.length = length;
            midPoint = center;
        }

        public void Draw(bool debug, Color debugColor) {

            // Nodes are from the previous segment
            // Points are from this segment

            leftPointA = midPoint + new Vector3(-width / 2f, length / 2f, 0f);
            leftPointB = midPoint + new Vector3(-width / 2f, -length / 2f, 0f);
            rightPointA = midPoint + new Vector3(width / 2f, length / 2f, 0f);
            rightPointB = midPoint + new Vector3(width / 2f, -length / 2f, 0f);

            if (debug) {
                Debug.DrawLine(leftPointA , leftPointB , debugColor, Time.deltaTime, false);
                Debug.DrawLine(leftPointA , rightPointA , debugColor, Time.deltaTime, false);
                Debug.DrawLine(leftPointB , rightPointB , debugColor, Time.deltaTime, false);
                Debug.DrawLine(rightPointA , rightPointB , debugColor, Time.deltaTime, false);

            }

        }

    }


    public bool randomize;
    public bool round;
    public RandomParams randomWidth;
    public RandomParams randomLength;
    public RandomParams randomOffset;

    public int segmentCount;
    public PlatformSegment[] segments;
    public Vector3[] orderedPoints;
    public Vector3[] pathPoints;

    public bool render;
    public bool debug;
    public bool detail;

    void Update() {

        if (segmentCount != segments.Length) {
            PlatformSegment[] temp = new PlatformSegment[segmentCount];
            for (int i = 0; i < segmentCount; i++) {
                if (i < segments.Length) {
                    temp[i] = segments[i];
                }
                else {
                    temp[i] = new PlatformSegment();
                }
            }
            segments = temp;
        }

        if (randomize) {
            for (int i = 0; i < segments.Length; i++) {
                segments[i].Randomize(randomWidth, randomLength, randomOffset, round);
            }
            randomize = false;
        }

        for (int i = 0; i < segments.Length; i++) {
            segments[i].Draw(debug, Color.yellow);
        }

        if (render) {
            // Render(0, segments.Length);
            Render();
            // render = false;
        }

        if (detail) {
            AddDetail();
            Draw();
            detail = false;
        }

        if (orderedPoints != null) {

            Path();
            if (spawn) {
                StartCoroutine(IESpawn());
                spawn = false;
            }
            Node();
        }

    }

    public Tank tank;
    public int tankCount;
    public float tankSpawnInterval;
    public bool spawn;

    private IEnumerator IESpawn() {
        for (int i = 0; i < tankCount; i++) {
            Tank newTank = Instantiate(tank.gameObject, pathPoints[0], Quaternion.identity, null).GetComponent<Tank>();
            newTank.Init(pathPoints);
            yield return new WaitForSeconds(tankSpawnInterval);
        }
    }


    public float thresholdDistance;
    private void Path() {

        pathPoints = new Vector3[orderedPoints.Length];
        for (int i = 0; i < orderedPoints.Length; i++) {
            pathPoints[i] = orderedPoints[i] - (orderedPoints[i].normalized * 1f);
        }

        //List<Vector3> cleanPath = new List<Vector3>();
        //float dist = 0f;
        //cleanPath.Add(pathPoints[0]);
        //for (int i = 1; i < pathPoints.Length; i++) {
        //    dist += (pathPoints[i - 1] - pathPoints[i]).magnitude;
        //    print(dist);
        //    if (dist > thresholdDistance) {
        //        cleanPath.Add(pathPoints[i]);
        //        dist = dist - thresholdDistance;
        //    }
        //}
        //pathPoints = cleanPath.ToArray();

        for (int i = 1; i < pathPoints.Length; i++) {
            Debug.DrawLine(pathPoints[i - 1], pathPoints[i], Color.green, Time.deltaTime, false);
        }
        Debug.DrawLine(pathPoints[orderedPoints.Length - 1], pathPoints[0], Color.green, Time.deltaTime, false);


    }

    private void Render() {

        // Collect all the points
        List<Vector3> allPoints = new List<Vector3>();
        for (int i = 0; i < segments.Length; i++) {
            allPoints.Add(segments[i].leftPointA);
            allPoints.Add(segments[i].leftPointB);
            allPoints.Add(segments[i].rightPointA);
            allPoints.Add(segments[i].rightPointB);

        }

        // Get rid of all the points that are inside a box (using the original points list to avoid )
        List<Vector3> outerPoints = new List<Vector3>();
        for (int i = 0; i < allPoints.Count; i++) {
            Vector3 point = allPoints[i];
            bool inside = false;
            for (int j = 0; j < segments.Length; j++) {
                PlatformSegment box = segments[j];
                if (point.x > box.leftPointA.x && point.x < box.rightPointA.x && point.y < box.leftPointA.y && point.y > box.leftPointB.y) {
                    inside = true;
                    break;
                }
            }

            if (!inside) {
                outerPoints.Add(point);
            }
        }


        orderedPoints = outerPoints.ToArray();
        Array.Sort<Vector3>(orderedPoints, new Comparison<Vector3>((vectorA, vectorB) => Compare(vectorA, vectorB)));

        for (int i = 1; i < orderedPoints.Length; i++) {
            Debug.DrawLine(orderedPoints[i - 1], orderedPoints[i], Color.red, Time.deltaTime, false);
        }
        Debug.DrawLine(orderedPoints[orderedPoints.Length-1], orderedPoints[0], Color.red, Time.deltaTime, false);

        for (int i = 0; i < details.Count; i++) {


            bool detailIsContained = true;
            for (int j = 1; j < orderedPoints.Length; j++) {

                Vector2 lineB = orderedPoints[j] -orderedPoints[j - 1];
                bool a = CheckIntersect(details[i].leftPointA, details[i].leftPointB, orderedPoints[j], orderedPoints[j - 1]);
                bool b = CheckIntersect(details[i].leftPointA, details[i].rightPointA, orderedPoints[j], orderedPoints[j - 1]);
                bool c = CheckIntersect(details[i].leftPointB, details[i].rightPointB, orderedPoints[j], orderedPoints[j - 1]);
                bool d = CheckIntersect(details[i].rightPointA, details[i].rightPointB, orderedPoints[j], orderedPoints[j - 1]);

                if (a || b || c || d) {
                    detailIsContained = false;
                    break;
                }
            }

            bool a0 = CheckIntersect(details[i].leftPointA, details[i].leftPointB, orderedPoints[orderedPoints.Length-1], orderedPoints[0]);
            bool b0 = CheckIntersect(details[i].leftPointA, details[i].rightPointA, orderedPoints[orderedPoints.Length - 1], orderedPoints[0]);
            bool c0 = CheckIntersect(details[i].leftPointB, details[i].rightPointB, orderedPoints[orderedPoints.Length - 1], orderedPoints[0]);
            bool d0= CheckIntersect(details[i].rightPointA, details[i].rightPointB, orderedPoints[orderedPoints.Length - 1], orderedPoints[0]);

            if (a0 || b0 || c0 || d0) {
                detailIsContained = false;
            }

            if (detailIsContained) {
                details[i].Draw(true, Color.blue);
            }

        }


    }

    SpriteShapeController spriteShapeController;
    public float[] distances;
    private void Draw() {

        if (spriteShapeController == null) {
            spriteShapeController = GetComponent<SpriteShapeController>();
        }

        distances = new float[orderedPoints.Length];
        List<Vector3> cleanPath = new List<Vector3>();
        float dist = 0f;
        cleanPath.Add(orderedPoints[0]);
        for (int i = 1; i < orderedPoints.Length; i++) {
            dist += (orderedPoints[i - 1] - orderedPoints[i]).magnitude;
            distances[i] = (orderedPoints[i - 1] - orderedPoints[i]).magnitude;
            print(dist);
            if (dist > thresholdDistance) {
                cleanPath.Add(orderedPoints[i]);
                dist = dist - thresholdDistance;
            }
        }

        spriteShapeController.spline.Clear();
        for (int i = 0; i < cleanPath.Count; i++) {
            // spline.InsertPointAt(i, segments[i].leftPointA - transform.localPosition);
            spriteShapeController.spline.InsertPointAt(i, cleanPath[i] - transform.localPosition);
            spriteShapeController.spline.SetTangentMode(i, ShapeTangentMode.Continuous);
        }

    }

    public int Compare(Vector3 vA, Vector3 vB) {

        float angleA = Vector2.SignedAngle((Vector2)vA, Vector2.right);
        float angleB = Vector2.SignedAngle((Vector2)vB, Vector2.right);

        angleA = angleA < 0f ? angleA + 360f : angleA;
        angleB = angleB < 0f ? angleB + 360f : angleB;

        return angleA.CompareTo(angleB);

    }

    private List<PlatformSegment> details = new List<PlatformSegment>();

    private float leftMost;
    private float rightMost;
    private float bottomMost;
    private float topMost;

    private void AddDetail() {

        details = new List<PlatformSegment>();

        leftMost = Mathf.Infinity;// Get the left most
        rightMost = -Mathf.Infinity; ;// The right most
        bottomMost = Mathf.Infinity; ;// The bottom most
        topMost = -Mathf.Infinity; ;// The top most
        for (int i = 0; i < segments.Length; i++) {

            if (segments[i].leftPointA.x < leftMost) {
                leftMost = segments[i].leftPointA.x;
            }
            if (segments[i].rightPointA.x > rightMost) {
                rightMost = segments[i].rightPointA.x;
            }
            if (segments[i].leftPointA.y > topMost) {
                topMost = segments[i].leftPointA.y;
            }
            if (segments[i].leftPointB.y < bottomMost) {
                bottomMost = segments[i].leftPointB.y;
            }

        }

        Debug.DrawLine(new Vector3(leftMost, topMost, 0f), new Vector3(leftMost, bottomMost, 0f), Color.blue, Time.deltaTime, false);
        Debug.DrawLine(new Vector3(rightMost, topMost, 0f), new Vector3(rightMost, bottomMost, 0f), Color.blue, Time.deltaTime, false);
        Debug.DrawLine(new Vector3(leftMost, topMost, 0f), new Vector3(rightMost, topMost, 0f), Color.blue, Time.deltaTime, false);
        Debug.DrawLine(new Vector3(leftMost, bottomMost, 0f), new Vector3(rightMost, bottomMost, 0f), Color.blue, Time.deltaTime, false);

        // Generate all the possible squares
        for (int i = (int)Mathf.Floor(bottomMost); i < (int)Mathf.Ceil(topMost); i++) {

            for (int j = (int)Mathf.Floor(leftMost); j < (int)Mathf.Ceil(rightMost); j++) {

                float y = (float)i; float x = (float)j;

                Vector3 center = new Vector3(x + 0.5f, y + 0.5f, 0f);

                //Vector3 a = new Vector3(x, y, 0f);
                //Vector3 b = new Vector3(x + 1f, y, 0f);
                //Vector3 c = new Vector3(x, y + 1f, 0f);
                //Vector3 d = new Vector3(x + 1f, y + 1f, 0f);

                PlatformSegment newSegment = new PlatformSegment();
                newSegment.Set(center, 1f, 1f);
                details.Add(newSegment);

            }

        }

        // Generate the details points
        for (int i = 0; i < details.Count; i++) {
            details[i].Draw(true, Color.blue);
        }

    }

    public bool CheckIntersect(Vector2 A1, Vector2 A2, Vector2 B1, Vector2 B2) {

        float tmp = (B2.x - B1.x) * (A2.y - A1.y) - (B2.y - B1.y) * (A2.x - A1.x);

        bool found = true;
        if (tmp == 0) {
            // No solution!
            found = false;
        }

        float mu = ((A1.x - B1.x) * (A2.y - A1.y) - (A1.y - B1.y) * (A2.x - A1.x)) / tmp;

        Vector2 i = new Vector2(
            B1.x + (B2.x - B1.x) * mu,
            B1.y + (B2.y - B1.y) * mu
        );

        // check the intersection point is actually on both the lines
        float precision = 0.001f;

        bool A = (i - A1).magnitude + (i - A2).magnitude < (A1 - A2).magnitude + precision && (i - A1).magnitude + (i - A2).magnitude > (A1 - A2).magnitude - precision;
        bool B = (i - B1).magnitude + (i - B2).magnitude < (B1 - B2).magnitude + precision && (i - B1).magnitude + (i - B2).magnitude > (B1 - B2).magnitude - precision;
        return (found && A && B);
    }


    public Vector3 rightNode;
    public Vector3 leftNode;

    private void Node() {

        rightNode = new Vector3(rightMost - 2f, (topMost + bottomMost) / 2f, 0f);
        leftNode = new Vector3(leftMost + 2f, (topMost + bottomMost) / 2f, 0f);

        Debug.DrawLine(rightNode - Vector3.up * 10f, rightNode + Vector3.up * 10f, Color.yellow, Time.deltaTime, false);
        Debug.DrawLine(leftNode - Vector3.up * 10f, leftNode + Vector3.up * 10f, Color.yellow, Time.deltaTime, false);

    }

}

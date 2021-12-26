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


        public void Randomize(Vector3 position, RandomParams rWidth, RandomParams rLength, RandomParams rOffset, bool round) {
            if (!round) {
                width = rWidth.Get();
                length = rLength.Get();
                midPoint = position + rOffset.Get() * (Vector3)(Random.insideUnitCircle.normalized);
            }
            else {
                width = rWidth.GetRound();
                length = rLength.GetRound();
                midPoint = position + rOffset.GetRound() * (Vector3)(Random.insideUnitCircle.normalized);
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
                Debug.DrawLine(leftPointA, leftPointB, debugColor, Time.deltaTime, false);
                Debug.DrawLine(leftPointA, rightPointA, debugColor, Time.deltaTime, false);
                Debug.DrawLine(leftPointB, rightPointB, debugColor, Time.deltaTime, false);
                Debug.DrawLine(rightPointA, rightPointB, debugColor, Time.deltaTime, false);

            }

            leftPointA.z = GameRules.PlatformDepth;
            leftPointB.z = GameRules.PlatformDepth;
            rightPointA.z = GameRules.PlatformDepth;
            rightPointB.z = GameRules.PlatformDepth;

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

    public bool generate;
    public bool debugLines;
    public bool debugSquares;
    public float pathInset;

    public bool design;

    public Tank tank;
    public int tankCount;
    public float tankSpawnInterval;
    public bool spawnTanks;

    public int maxTurrets;
    public Turret turret;
    public bool spawnTurrets;

    public MeshFilter meshFilter;
    public LineRenderer outline;
    public LineRenderer pathline;
    public Color backgroundColor;

    private bool doneDetailing = false;

    void Update() {

        Move();

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
                segments[i].Randomize(transform.position, randomWidth, randomLength, randomOffset, round);
            }
            generate = true;
            randomize = false;
        }

        for (int i = 0; i < segments.Length; i++) {
            segments[i].Draw(debugSquares, Color.yellow);
        }

        if (generate) {
            CleanInside();
            if (spawnTurrets) {
                Detail();
            }
            generate = false;
        }

        if (orderedPoints != null && orderedPoints.Length > 0) {

            Node();

            if (spawnTanks) {
                Path();
                Render();
                StartCoroutine(IESpawn());
                spawnTanks = false;
            }

            if (details != null && details.Count > 0 && doneDetailing && spawnTurrets) {
                Design();
                SpawnTurrets();
                spawnTurrets = false;
            }

            if (debugLines) {
                DebugLines();
            }

        }

        if (transform.position.y < GameRules.MainCamera.transform.position.y - GameRules.ScreenPixelHeight / GameRules.PixelsPerUnit - 50f) {
            Destroy(gameObject);
        }

    }

    private void Move() {

        Vector3 diff = GameRules.ScrollSpeed * Time.deltaTime * Vector3.up * GameRules.GetParrallax(GameRules.PlatformDepth);
        //for (int i = 0; i < orderedPoints.Length; i++) {
        //    orderedPoints[]
        //}

        transform.position += diff;

        if (orderedPoints != null && orderedPoints.Length > 0) {
            for (int i = 0; i < orderedPoints.Length; i++) {
                orderedPoints[i] += diff;
            }

        }

        if (pathPoints != null && pathPoints.Length > 0) {
            for (int i = 0; i < pathPoints.Length; i++) {
                pathPoints[i] += diff;
            }
            RenderOutline();
        }

        if (details != null && details.Count > 0) {
            for (int i = 0; i < details.Count; i++) {
                details[i].midPoint += diff;
            }

        }

    }

    private IEnumerator IESpawn() {

        bool blue = Random.Range(0f, 1f) > 0.5f;
        int spawnIndex = Random.Range(0, pathPoints.Length);

        for (int i = 0; i < tankCount; i++) {
            Tank newTank = Instantiate(tank.gameObject, pathPoints[spawnIndex], Quaternion.identity, transform).GetComponent<Tank>();
            newTank.Init(pathPoints, blue);
            yield return new WaitForSeconds(tankSpawnInterval);
        }
    }

    private void SpawnTurrets() {

        List<int> indices = new List<int>();

        bool blue = Random.Range(0f, 1f) > 0.5f;

        for (int i = 0; i < details.Count; i++) {
            if (shiftRight) {
                if (details[i].midPoint.x < transform.position.x) {
                    indices.Add(i);
                }
            }
            else {
                if (details[i].midPoint.x > transform.position.x) {
                    indices.Add(i);
                }
            }

        }


        maxTurrets = (int)Mathf.Min(maxTurrets, indices.Count);
        for (int i = 0; i < maxTurrets; i++) {

            int index = Random.Range(0, indices.Count);
            Turret newTurret = Instantiate(turret.gameObject).GetComponent<Turret>();
            newTurret.transform.position = details[indices[index]].midPoint;
            newTurret.gameObject.SetActive(true);
            newTurret.Init(blue, details[indices[index]]);
            indices.RemoveAt(index);

        }

    }


    private void Path() {

        pathPoints = new Vector3[orderedPoints.Length];
        for (int i = 0; i < orderedPoints.Length; i++) {
            Vector3 direction = (Vector3)((Vector2)orderedPoints[i] - (Vector2)transform.position).normalized;
            pathPoints[i] = orderedPoints[i] - (direction * pathInset);
        }

    }

    private void DebugLines() {

        for (int i = 1; i < orderedPoints.Length; i++) {
            Debug.DrawLine(orderedPoints[i - 1], orderedPoints[i], Color.red, Time.deltaTime, false);
        }
        Debug.DrawLine(orderedPoints[orderedPoints.Length - 1], orderedPoints[0], Color.red, Time.deltaTime, false);

        if (pathPoints.Length > 0) {
            for (int i = 1; i < pathPoints.Length; i++) {
                Debug.DrawLine(pathPoints[i - 1], pathPoints[i], Color.green, Time.deltaTime, false);
            }
            Debug.DrawLine(pathPoints[pathPoints.Length - 1], pathPoints[0], Color.green, Time.deltaTime, false);
        }

        // Generate the details points
        if (details != null && details.Count > 0) {
            for (int i = 0; i < details.Count; i++) {
                details[i].Draw(true, Color.blue);
            }
            Debug.DrawLine(new Vector3(leftMost, topMost, 0f), new Vector3(leftMost, bottomMost, 0f), Color.blue, Time.deltaTime, false);
            Debug.DrawLine(new Vector3(rightMost, topMost, 0f), new Vector3(rightMost, bottomMost, 0f), Color.blue, Time.deltaTime, false);
            Debug.DrawLine(new Vector3(leftMost, topMost, 0f), new Vector3(rightMost, topMost, 0f), Color.blue, Time.deltaTime, false);
            Debug.DrawLine(new Vector3(leftMost, bottomMost, 0f), new Vector3(rightMost, bottomMost, 0f), Color.blue, Time.deltaTime, false);

        }

    }

    private void CleanInside() {
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
    }

    private void Render() {
        RenderOutline();
        RenderFace();
    }

    private void RenderFace() {
        if (meshFilter.mesh == null) {
            meshFilter.mesh = new Mesh();
        }

        List<Vector3> positions = new List<Vector3>();
        List<int> indices = new List<int>();
        List<Color> colors = new List<Color>();

        positions.Add(orderedPoints[0] - transform.localPosition);
        colors.Add(backgroundColor);

        for (int i = 1; i < orderedPoints.Length; i++) {

            positions.Add(orderedPoints[i] - transform.localPosition);

            indices.Add(orderedPoints.Length);
            indices.Add(i - 1);
            indices.Add(i);

            colors.Add(backgroundColor);

        }

        Vector3 center = transform.position;
        center.z = GameRules.PlatformDepth;
        positions.Add(center - transform.localPosition);

        indices.Add(positions.Count - 1);
        indices.Add(orderedPoints.Length - 1);
        indices.Add(0);

        colors.Add(backgroundColor);

        meshFilter.mesh.SetVertices(positions);
        meshFilter.mesh.SetIndices(indices.ToArray(), MeshTopology.Triangles, 0);
        meshFilter.mesh.colors = colors.ToArray();
    }

    private void RenderOutline() {
        outline.startWidth = 1f;
        outline.endWidth = 1f;
        outline.positionCount = orderedPoints.Length;
        outline.SetPositions(orderedPoints);

        List<Vector3> shadowLine = new List<Vector3>();
        for (int i = 0; i < orderedPoints.Length; i++) {
            Vector3 shadowPoint = orderedPoints[i] + Vector3.down * 1f;
            shadowPoint.z = GameRules.PlatformShadow;
            shadowLine.Add(shadowPoint);
        }

        pathline.startWidth = 1.75f;
        pathline.endWidth = 1.75f;
        pathline.positionCount = shadowLine.Count;
        pathline.SetPositions(shadowLine.ToArray());
    }

    public int Compare(Vector3 vA, Vector3 vB) {

        float angleA = Vector2.SignedAngle((Vector2)vA - (Vector2)transform.position, Vector2.right);
        float angleB = Vector2.SignedAngle((Vector2)vB - (Vector2)transform.position, Vector2.right);

        angleA = angleA < 0f ? angleA + 360f : angleA;
        angleB = angleB < 0f ? angleB + 360f : angleB;

        return angleA.CompareTo(angleB);

    }

    private List<PlatformSegment> details = new List<PlatformSegment>();

    private float leftMost;
    private float rightMost;
    private float bottomMost;
    private float topMost;

    private void Detail() {

        details = new List<PlatformSegment>();

        leftMost = Mathf.Infinity;// Get the left most
        rightMost = -Mathf.Infinity; ;// The right most
        bottomMost = Mathf.Infinity; ;// The bottom most
        topMost = -Mathf.Infinity; ;// The top most
        for (int i = 0; i < orderedPoints.Length; i++) {

            if (orderedPoints[i].x < leftMost) {
                leftMost = orderedPoints[i].x;
            }
            if (orderedPoints[i].x > rightMost) {
                rightMost = orderedPoints[i].x;
            }
            if (orderedPoints[i].y > topMost) {
                topMost = orderedPoints[i].y;
            }
            if (orderedPoints[i].y < bottomMost) {
                bottomMost = orderedPoints[i].y;
            }

        }

        // Generate all the possible squares
        int scale = 2;

        for (int i = (int)Mathf.Ceil(bottomMost); i < (int)Mathf.Floor(topMost); i += scale) {

            for (int j = (int)Mathf.Ceil(leftMost); j < (int)Mathf.Floor(rightMost); j += scale) {

                float _scale = (float)scale;
                float y = (float)i; float x = (float)j;
                Vector3 center = new Vector3(x + _scale / 2f, y + _scale / 2f, 0f);

                PlatformSegment newSegment = new PlatformSegment();
                newSegment.Set(center, _scale, _scale);
                details.Add(newSegment);

            }

        }

        // Generate the details points
        for (int i = 0; i < details.Count; i++) {
            details[i].Draw(true, Color.blue);
        }

        doneDetailing = true;
        // StartCoroutine(IEInteriorGrid());

    }

    private IEnumerator IEInteriorGrid() {

        doneDetailing = false;
        List<PlatformSegment> temp = new List<PlatformSegment>();
        for (int i = 0; i < details.Count; i++) {

            Vector3 midPoint;
            Vector3 direction;
            Vector3 n1;
            Vector3 n2;

            bool detailIsContained = true;
            for (int j = 1; j < orderedPoints.Length; j++) {
                // print(details[i].leftPointA.ToString() + ", " + details[i].leftPointB.ToString() + ", " + orderedPoints[j].ToString() + ", " + orderedPoints[j - 1].ToString());
                // return temp;
                midPoint = (orderedPoints[j] + orderedPoints[j - 1]) / 2f;
                direction = midPoint + (midPoint - transform.localPosition).normalized * 50f;

                // against the normals
                n1 = midPoint;
                n2 = direction;

                bool a = CheckIntersect(details[i].leftPointA, details[i].leftPointB, n1, n2);
                bool b = CheckIntersect(details[i].leftPointA, details[i].rightPointA, n1, n2);
                bool c = CheckIntersect(details[i].leftPointB, details[i].rightPointB, n1, n2);
                bool d = CheckIntersect(details[i].rightPointA, details[i].rightPointB, n1, n2);

                if (a || b || c || d) {
                    detailIsContained = false;
                    break;
                }

                // against the tangents
                n1 = orderedPoints[j];
                n2 = orderedPoints[j - 1];

                a = CheckIntersect(details[i].leftPointA, details[i].leftPointB, n1, n2);
                b = CheckIntersect(details[i].leftPointA, details[i].rightPointA, n1, n2);
                c = CheckIntersect(details[i].leftPointB, details[i].rightPointB, n1, n2);
                d = CheckIntersect(details[i].rightPointA, details[i].rightPointB, n1, n2);

                if (a || b || c || d) {
                    detailIsContained = false;
                    break;
                }

            }

            midPoint = (orderedPoints[orderedPoints.Length - 1] + orderedPoints[0]) / 2f;
            direction = midPoint + (midPoint - transform.localPosition).normalized * 50f;

            n1 = midPoint;
            n2 = direction;

            bool a0 = CheckIntersect(details[i].leftPointA, details[i].leftPointB, n1, n2);
            bool b0 = CheckIntersect(details[i].leftPointA, details[i].rightPointA, n1, n2);
            bool c0 = CheckIntersect(details[i].leftPointB, details[i].rightPointB, n1, n2);
            bool d0 = CheckIntersect(details[i].rightPointA, details[i].rightPointB, n1, n2);

            if (a0 || b0 || c0 || d0) {
                detailIsContained = false;
            }

            n1 = orderedPoints[orderedPoints.Length - 1];
            n2 = orderedPoints[0];

            a0 = CheckIntersect(details[i].leftPointA, details[i].leftPointB, n1, n2);
            b0 = CheckIntersect(details[i].leftPointA, details[i].rightPointA, n1, n2);
            c0 = CheckIntersect(details[i].leftPointB, details[i].rightPointB, n1, n2);
            d0 = CheckIntersect(details[i].rightPointA, details[i].rightPointB, n1, n2);

            if (a0 || b0 || c0 || d0) {
                detailIsContained = false;
            }

            if (detailIsContained) {
                temp.Add(details[i]);
            }

            // yield return new WaitForSeconds(0);
        }

        print(details.Count.ToString() + ", " + temp.Count.ToString());
        details = temp;

        // Generate the details points
        for (int i = 0; i < details.Count; i++) {
            details[i].Draw(true, Color.blue);
        }

        doneDetailing = true;

        yield return null;

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
        float precision = 0.05f;

        bool A = (i - A1).magnitude + (i - A2).magnitude < (A1 - A2).magnitude + precision && (i - A1).magnitude + (i - A2).magnitude > (A1 - A2).magnitude - precision;
        bool B = (i - B1).magnitude + (i - B2).magnitude < (B1 - B2).magnitude + precision && (i - B1).magnitude + (i - B2).magnitude > (B1 - B2).magnitude - precision;
        print(found.ToString() + ", " + A.ToString() + ", " + B.ToString());
        return (found && A && B);
    }


    public Vector3 rightNode;
    public Vector3 leftNode;
    [HideInInspector] public bool shift;
    [HideInInspector] public bool shiftRight;

    public bool attached = false;

    private void Node() {

        if (attached) { return; }

        rightNode = new Vector3(rightMost - 2f, (topMost + bottomMost) / 2f, 0f);
        leftNode = new Vector3(leftMost + 2f, (topMost + bottomMost) / 2f, 0f);

        Debug.DrawLine(rightNode - Vector3.up * 10f, rightNode + Vector3.up * 10f, Color.yellow, Time.deltaTime, false);
        Debug.DrawLine(leftNode - Vector3.up * 10f, leftNode + Vector3.up * 10f, Color.yellow, Time.deltaTime, false);

        Vector3 node;
        if (shift) {
            if (shiftRight) {
                node = rightNode;
            }
            else {
                node = leftNode;
            }

            for (int i = 0; i < orderedPoints.Length; i++) {

                orderedPoints[i] += node;

            }
        }

        attached = true;

    }

    public SpriteRenderer spriteBase;
    public Sprite[] designs;
    public Sprite outlineSprite;
    public float designDensity;
    private void Design() {
        for (int i = 0; i < details.Count; i++) {
            SpriteRenderer newSprite = Instantiate(spriteBase.gameObject, details[i].midPoint, Quaternion.identity, transform).GetComponent<SpriteRenderer>();
            if (Random.Range(0f, 1f) < designDensity) {
                newSprite.sprite = designs[Random.Range(0, designs.Length - 1)];
            }
            else {
                newSprite.sprite = designs[designs.Length - 1];
            }
            newSprite.transform.position = new Vector3(details[i].midPoint.x, details[i].midPoint.y, GameRules.PlatformDesigns);
            newSprite.gameObject.SetActive(true);

            SpriteRenderer outlineSprite = Instantiate(spriteBase.gameObject, details[i].midPoint, Quaternion.identity, transform).GetComponent<SpriteRenderer>();
            outlineSprite.sprite = this.outlineSprite;
            outlineSprite.transform.position = new Vector3(details[i].midPoint.x, details[i].midPoint.y, GameRules.PlatformDesigns + 1);
            outlineSprite.gameObject.SetActive(true);

        }
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

using RandomParams = GameRules.RandomParams;

public class Tunnel : MonoBehaviour {

    [System.Serializable]
    public class TunnelSegment {

        public float width;
        public float length;
        public float offset;
        [Range(-90f, 90f)] public float angle;

        public bool alreadyCheckedForBeam = false;

        [HideInInspector] public Vector3 leftPointA;
        [HideInInspector] public Vector3 rightPointA;
        [HideInInspector] public Vector3 leftPointB;
        [HideInInspector] public Vector3 rightPointB;

        public void Randomize(RandomParams rWidth, RandomParams rLength, RandomParams rOffset, RandomParams rAngle, bool round) {
            if (!round) {
                width = rWidth.Get();
                length = rLength.Get();
                offset = rOffset.Get();
                angle = rAngle.Get();
            }
            else {
                width = rWidth.GetRound();
                length = rLength.GetRound();
                offset = rOffset.GetRound();
                angle = rAngle.GetRound();
            }
        }

        public void Draw(Vector3 leftNode, Vector3 rightNode, bool continous) {

            // Nodes are from the previous segment
            // Points are from this segment

            Vector3 midPointA = offset * Vector3.right + (leftNode + rightNode) / 2f;

            if (continous) {
                leftPointA = leftNode;
                rightPointA = rightNode;
            }
            else {
                leftPointA = midPointA + Vector3.left * width / 2f;
                rightPointA = midPointA + Vector3.right * width / 2f;
            }

            Vector3 midPointB = midPointA + length * (Quaternion.Euler(0f, 0f, angle) * Vector3.up);
            leftPointB = midPointB + Vector3.left * width / 2f;
            rightPointB = midPointB + Vector3.right * width / 2f;

            // From left node to left point and right node to right point
            Debug.DrawLine(leftPointA, leftPointB, Color.yellow, Time.deltaTime, false);
            Debug.DrawLine(rightPointA, rightPointB, Color.yellow, Time.deltaTime, false);

            // From left point to right point
            Debug.DrawLine(leftPointA, rightPointA, Color.yellow, Time.deltaTime, false);
            Debug.DrawLine(leftPointB, rightPointB, Color.yellow, Time.deltaTime, false);

            leftPointA.z = GameRules.TunnelDepth;
            leftPointB.z = GameRules.TunnelDepth;
            rightPointA.z = GameRules.TunnelDepth;
            rightPointB.z = GameRules.TunnelDepth;

        }

    }

    public float entranceWidth;

    public bool randomize;
    public bool continous;
    public bool round;
    public RandomParams randomWidth;
    public RandomParams randomLength;
    public RandomParams randomOffset;
    public RandomParams randomAngle;

    public int segmentCount;
    public TunnelSegment[] segments;

    public bool render;
    public bool renderRange;
    public Color backgroundColor;
    public MeshFilter meshFilter;


    private Vector3 initialLeftNode;
    private Vector3 initialRightNode;

    void Start() {



        initialLeftNode = transform.localPosition + Vector3.left * entranceWidth / 2f;
        initialRightNode = transform.localPosition + Vector3.right * entranceWidth / 2f;

        //meshFilter = GetComponent<MeshFilter>();
        //meshFilter.mesh = new Mesh();

        StartCoroutine(IEProcedural());
    }

    static float ProceduralUpdateRate = 0.2f;

    public int cameraIndex;
    public int startIndex = 0;
    public int thresholdIndex = 25;

    private IEnumerator IEProcedural() {

        yield return new WaitForSeconds(ProceduralUpdateRate);

        // Get the camera index
        float cameraHeight = Camera.main.transform.position.y - (GameRules.ScreenPixelHeight / 2f / GameRules.PixelsPerUnit);
        cameraHeight -= 2f;
        for (int i = 0; i < segments.Length; i++) {
            if (segments[i].rightPointA.y > cameraHeight) {
                cameraIndex = i + startIndex;
                break;
            }
        }

        if (cameraIndex - startIndex > thresholdIndex) {
            // do stuff

            TunnelSegment[] temp = new TunnelSegment[segments.Length];
            for (int i = thresholdIndex; i < segments.Length; i++) {
                temp[i - thresholdIndex] = segments[i];
            }

            for (int i = segments.Length - thresholdIndex; i < segments.Length; i++) {

                temp[i] = new TunnelSegment();
                temp[i].Randomize(randomWidth, randomLength, randomOffset, randomAngle, round);

            }

            // transform.localPosition = Vector3.up * 2f * cameraIndex;
            initialLeftNode = segments[thresholdIndex].leftPointA;
            initialRightNode = segments[thresholdIndex].rightPointA;
            startIndex = cameraIndex;
            segments = temp;
            render = true;
        }



        StartCoroutine(IEProcedural());
        yield return null;


    }

    void Update() {

        if (segmentCount != segments.Length) {
            TunnelSegment[] temp = new TunnelSegment[segmentCount];
            for (int i = 0; i < segmentCount; i++) {
                if (i < segments.Length) {
                    temp[i] = segments[i];
                }
                else {
                    temp[i] = new TunnelSegment();
                }
            }
            segments = temp;
        }

        if (randomize) {
            for (int i = 0; i < segments.Length; i++) {
                segments[i].Randomize(randomWidth, randomLength, randomOffset, randomAngle, round);
            }
            randomize = false;
        }

        if (segments.Length > 0) {
            segments[0].Draw(initialLeftNode, initialRightNode, continous);
        }

        for (int i = 1; i < segments.Length; i++) {
            segments[i].Draw(segments[i - 1].leftPointB, segments[i - 1].rightPointB, continous);
        }

        if (render) {
            RenderLines();
            Beams();
            render = false;
        }

    }

    void FixedUpdate() {
    }

    public LineRenderer leftLine;
    public LineRenderer rightLine;

    private void RenderLines() {

        List<Vector3> leftPoints = new List<Vector3>();
        List<Vector3> rightPoints = new List<Vector3>();

        for (int i = 0; i < segments.Length; i++) {

            leftPoints.Add(segments[segments.Length - 1-i].leftPointA);
            rightPoints.Add(segments[i].rightPointA);

        }

        leftLine.startWidth = 1f;
        leftLine.endWidth = 1f;
        leftLine.positionCount = segments.Length;

        leftLine.SetPositions(leftPoints.ToArray());

        rightLine.startWidth = 1f;
        rightLine.endWidth = 1f;
        rightLine.positionCount = segments.Length;

        rightLine.SetPositions(rightPoints.ToArray());

    }

    public Beam beam;
    public List<int> check = new List<int>();

    private void BeamsB() {
        print("beams");
        for (int i = startIndex; i < startIndex + segments.Length; i++) {

            if (!check.Contains(i)) {
                if ((segments[i - startIndex].leftPointA - segments[i - startIndex].rightPointA).magnitude > 1f) {
                    if (Random.Range(0f, 1f) < 1f) {
                        Beam newBeam = Instantiate(beam.gameObject).GetComponent<Beam>();
                        newBeam.leftNode = segments[i - startIndex].leftPointA;
                        newBeam.rightNode = segments[i - startIndex].rightPointA;
                        newBeam.Init(Random.Range(0, 3));
                        newBeam.gameObject.SetActive(true);
                    }
                    check.Add(i);
                }
            }

        }
        

    }

    private void Beams() {
        print("beams");
        float deltaY = Camera.main.transform.position.y + GameRules.ScreenPixelHeight / GameRules.PixelsPerUnit * 1.5f;
        for (int i = 0; i < 3; i++) {

            deltaY += Random.Range(0f, 5f);
            Beam newBeam = Instantiate(beam.gameObject).GetComponent<Beam>();
            newBeam.leftNode = new Vector3( -50f, startIndex + deltaY, 0f);
            newBeam.rightNode = new Vector3(50f, startIndex + deltaY, 0f);
            newBeam.Init(Random.Range(0, 3));
            newBeam.gameObject.SetActive(true);

        }


    }

}

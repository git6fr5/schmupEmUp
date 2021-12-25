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

            leftPointA -= new Vector3(0f, 0f, leftPointA.z);
            leftPointB -= new Vector3(0f, 0f, leftPointB.z);
            rightPointA -= new Vector3(0f, 0f, rightPointA.z);
            rightPointB -= new Vector3(0f, 0f, rightPointB.z);

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
    private MeshFilter meshFilter;

    void Start() {

        //meshFilter = GetComponent<MeshFilter>();
        //meshFilter.mesh = new Mesh();


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
            segments[0].Draw(transform.localPosition + Vector3.left * entranceWidth / 2f, transform.localPosition + Vector3.right * entranceWidth / 2f, continous);
        }

        for (int i = 1; i < segments.Length; i++) {
            segments[i].Draw(segments[i - 1].leftPointB, segments[i - 1].rightPointB, continous);
        }

        if (render) {
            // Render(0, segments.Length);
            // CreateJaggedSpriteShape();
            CreateContinousSpriteShape();
            render = false;
        }        

        if (platforms) {
            CreatePlatforms();
            platforms = false;
        }
    }

    void FixedUpdate() {
        if (renderRange) {
            // Get the ones next to the player.
            // Assumes no angle
            // Assumes length is fixed (not random range).
            int segmentLength = (int)randomLength.GetRound();
            List<TunnelSegment> segmentsWithinScreen = new List<TunnelSegment>();
            float screenHeight = (float)(GameRules.ScreenPixelHeight / GameRules.PixelsPerUnit);
            float cameraHeight = Camera.main.transform.position.y;

            int startIndex = (int)(cameraHeight / segmentLength);
            startIndex = Mathf.Max(0, startIndex - 3);
            int finalIndex = Mathf.Min(segments.Length, startIndex + (int)Mathf.Ceil(screenHeight / segmentLength) + 3);
            // print(startIndex);

            float screenWidth = (float)(GameRules.ScreenPixelWidth / GameRules.PixelsPerUnit);
            int subdivisions = segmentLength * GameRules.PixelsPerUnit / 8;

            // Render(startIndex, startIndex + (int)Mathf.Ceil(screenHeight / segmentLength));
            DebugLines(startIndex, finalIndex, subdivisions, (float)segmentLength, screenWidth / 2f);
        }
    }

    void Render(int startIndex, int finalIndex) {

        List<Vector3> points = new List<Vector3>();
        List<Color> colors = new List<Color>();
        List<int> indices = new List<int>();

        int index = 0;
        for (int i = startIndex; i < finalIndex; i++) {

            points.Add(segments[i].leftPointA - transform.localPosition);
            points.Add(segments[i].rightPointA - transform.localPosition);
            points.Add(segments[i].leftPointB - transform.localPosition);
            points.Add(segments[i].rightPointB - transform.localPosition);

            indices.Add(4 * index + 0); // left 1
            indices.Add(4 * index + 3); // left 2
            indices.Add(4 * index + 1); // right 1

            indices.Add(4 * index + 0); // right 1
            indices.Add(4 * index + 2); // right 2
            indices.Add(4 * index + 3); // left 2

            colors.Add(backgroundColor);
            colors.Add(backgroundColor);
            colors.Add(backgroundColor);
            colors.Add(backgroundColor);

            index += 1;
        }

        meshFilter.mesh.SetVertices(points);
        meshFilter.mesh.SetIndices(indices.ToArray(), MeshTopology.Triangles, 0);
        meshFilter.mesh.colors = colors.ToArray();

    }

    void DebugLines(int startIndex, int finalIndex, int subdivisions, float length, float width) {
        for (int i = startIndex; i < finalIndex; i++) {
            for (int j = 0; j < subdivisions; j++) {
                Vector3 leftPoint = segments[i].leftPointA + Vector3.up * length * ((float)j) / ((float)subdivisions);
                Debug.DrawLine(leftPoint, leftPoint + Vector3.left * width, Color.red, Time.fixedDeltaTime, false);
                RaycastHit2D leftRay = Physics2D.Linecast(leftPoint, leftPoint + Vector3.left * width); 

                Vector3 rightPoint = segments[i].rightPointA + Vector3.up * length * ((float)j) / ((float)subdivisions);
                Debug.DrawLine(rightPoint, rightPoint + Vector3.right * width, Color.red, Time.fixedDeltaTime, false);
                RaycastHit2D rightRay = Physics2D.Linecast(rightPoint, rightPoint + Vector3.right * width);

                Player playerLeft = leftRay.collider?.GetComponent<Player>();
                Player playerRight = rightRay.collider?.GetComponent<Player>();
                if (playerLeft != null) {
                    print("HITTING A WALL!");
                    playerLeft.Respawn(false);
                }
                else if (playerRight != null) {
                    print("HITTING A WALL!");
                    playerRight.Respawn(false);
                }
            }
        }

    }

    // publci 
    public Spline spline;
    public SpriteShapeController spriteShapeController;

    public Platform platform;
    public bool platforms;

    private void CreatePlatforms() {

        bool right = true;

        for (int i = 10; i < segments.Length; i+=10) {

            // Create a platform.

            Platform newPlatform = Instantiate(platform.gameObject).GetComponent<Platform>();
            if (right) {
                newPlatform.transform.position = segments[i].rightPointA;
            }
            else {
                newPlatform.transform.position = segments[i].leftPointA;
            }


            newPlatform.gameObject.SetActive(true);
            right = !right;

        }

    }

    private void CreateContinousSpriteShape() {

        if (spriteShapeController == null) {
            spriteShapeController = GetComponent<SpriteShapeController>();
        }

        spriteShapeController.spline.Clear();
        for (int i = 0; i < segments.Length; i++) {
            // spline.InsertPointAt(i, segments[i].leftPointA - transform.localPosition);
            spriteShapeController.spline.InsertPointAt(i, segments[i].leftPointA - transform.localPosition);
            spriteShapeController.spline.SetTangentMode(i, ShapeTangentMode.Continuous);
        }

        spriteShapeController.spline.InsertPointAt(segments.Length, segments[segments.Length-1].leftPointB - transform.localPosition);
        spriteShapeController.spline.InsertPointAt(segments.Length + 1, segments[segments.Length-1].rightPointB - transform.localPosition);

        spriteShapeController.spline.SetTangentMode(segments.Length, ShapeTangentMode.Continuous);
        spriteShapeController.spline.SetTangentMode(segments.Length + 1, ShapeTangentMode.Continuous);


        for (int i = 0; i < segments.Length; i++) {
            // spline.InsertPointAt(i + segments.Length, segments[segments.Length - (i + 1)].rightPointA - transform.localPosition);
            spriteShapeController.spline.InsertPointAt(i + (segments.Length + 2), segments[segments.Length - (i+1)].rightPointA - transform.localPosition);
            spriteShapeController.spline.SetTangentMode(i + (segments.Length + 2), ShapeTangentMode.Continuous);

        }

    }

    private void CreateJaggedSpriteShape() {

        if (spriteShapeController == null) {
            spriteShapeController = GetComponent<SpriteShapeController>();
        }

        print("A");

        int index = 0;
        spriteShapeController.spline.Clear();
        for (int i = 0; i < segments.Length; i++) {

            if (i == 0) {
                spriteShapeController.spline.InsertPointAt(index, segments[i].leftPointA - transform.localPosition);
                index += 1;
            }
            else if (segments[i - 1].leftPointB != segments[i].leftPointA) {
                spriteShapeController.spline.InsertPointAt(index, segments[i].leftPointA - transform.localPosition);
                index += 1;
            }
            spriteShapeController.spline.InsertPointAt(index, segments[i].leftPointB - transform.localPosition);
            index += 1;

        }

        print("B");

        for (int i = segments.Length-1; i >= 0; i--) {

            if (i == segments.Length-1) {
                spriteShapeController.spline.InsertPointAt(index, segments[i].rightPointB - transform.localPosition);
                index += 1;
            }
            else if (segments[i + 1].rightPointA != segments[i].rightPointB) {
                spriteShapeController.spline.InsertPointAt(index, segments[i].rightPointB - transform.localPosition);
                index += 1;
            }
            spriteShapeController.spline.InsertPointAt(index, segments[i].rightPointA - transform.localPosition);
            index += 1;

        }

        print("C");

        for (int i = 0; i < index; i++) {
            spriteShapeController.spline.SetTangentMode(i, ShapeTangentMode.Continuous);
        }

    }

}

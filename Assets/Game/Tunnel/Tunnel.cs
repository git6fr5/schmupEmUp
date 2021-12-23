using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using RandomParams = GameRules.RandomParams;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class Tunnel : MonoBehaviour {

    [System.Serializable]
    public class TunnelSegment {

        public float width;
        public float length;
        // [Range(-90f, 90f)] public float angle;
        public float offset;

        [HideInInspector] public Vector3 leftPointA;
        [HideInInspector] public Vector3 rightPointA;
        [HideInInspector] public Vector3 leftPointB;
        [HideInInspector] public Vector3 rightPointB;

        public void Randomize(RandomParams rWidth, RandomParams rLength, RandomParams rOffset) {
            width = rWidth.Get();
            length = rLength.Get();
            offset = rOffset.Get();
        }

        public void Draw(Vector3 leftNode, Vector3 rightNode) {

            // Nodes are from the previous segment
            // Points are from this segment

            Vector3 midPointA = offset * Vector3.right + (leftNode + rightNode) / 2f;
            leftPointA = midPointA + Vector3.left * width / 2f;
            rightPointA = midPointA + Vector3.right * width / 2f;

            Vector3 midPointB = midPointA + length * Vector3.up;
            leftPointB = midPointB + Vector3.left * width / 2f;
            rightPointB = midPointB + Vector3.right * width / 2f;

            // From left node to left point and right node to right point
            Debug.DrawLine(leftPointA, leftPointB, Color.yellow, Time.deltaTime, false);
            Debug.DrawLine(rightPointA, rightPointB, Color.yellow, Time.deltaTime, false);

            // From left point to right point
            Debug.DrawLine(leftPointA, rightPointA, Color.yellow, Time.deltaTime, false);
            Debug.DrawLine(leftPointB, rightPointB, Color.yellow, Time.deltaTime, false);

        }

    }

    public float entranceWidth;

    public bool randomize;
    public RandomParams randomWidth;
    public RandomParams randomLength;
    public RandomParams randomAngle;

    public int segmentCount;
    public TunnelSegment[] segments;

    public bool render;
    public Color backgroundColor;
    private MeshFilter meshFilter;

    void Start() {

        meshFilter = GetComponent<MeshFilter>();
        meshFilter.mesh = new Mesh();
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
                segments[i].Randomize(randomWidth, randomLength, randomAngle);
            }
            randomize = false;
        }

        if (segments.Length > 0) {
            segments[0].Draw(transform.localPosition + Vector3.left * entranceWidth / 2f, transform.localPosition + Vector3.right * entranceWidth / 2f);
        }

        for (int i = 1; i < segments.Length; i++) {
            segments[i].Draw(segments[i-1].leftPointB, segments[i - 1].rightPointB);
        }

        if (render) {
            Render();
            render = false;
        }

    }

    void Render() {

        List<Vector3> points = new List<Vector3>();       
        List<Color> colors = new List<Color>();
        List<int> indices = new List<int>();

        for (int i = 0; i < segments.Length; i++) {

            points.Add(segments[i].leftPointA - transform.localPosition);
            points.Add(segments[i].rightPointA - transform.localPosition);
            points.Add(segments[i].leftPointB - transform.localPosition);
            points.Add(segments[i].rightPointB - transform.localPosition);

            indices.Add(4 * i + 0); // left 1
            indices.Add(4 * i + 3); // left 2
            indices.Add(4 * i + 1); // right 1

            indices.Add(4 * i + 0); // right 1
            indices.Add(4 * i + 2); // right 2
            indices.Add(4 * i + 3); // left 2

            colors.Add(backgroundColor);
            colors.Add(backgroundColor);
            colors.Add(backgroundColor);
            colors.Add(backgroundColor);

        }

        meshFilter.mesh.SetVertices(points);
        meshFilter.mesh.SetIndices(indices.ToArray(), MeshTopology.Triangles, 0);
        meshFilter.mesh.colors = colors.ToArray();

    }

}

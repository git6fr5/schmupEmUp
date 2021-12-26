using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class Cliff : MonoBehaviour {

    MeshFilter meshFilter;
    Tunnel tunnel;

    public Color backgroundColor;
    public bool render;

    public LineRenderer cliffSide;
    public LineRenderer cliffShadow;

    public bool right;
    bool skipFrame = true;

    void Start() {

        tunnel = (Tunnel)GameObject.FindObjectOfType(typeof(Tunnel));

        meshFilter = GetComponent<MeshFilter>();
        meshFilter.mesh = new Mesh();

        StartCoroutine(IERender());
    }

    void Update() {

        if (skipFrame) {
            skipFrame = false;
            return;
        }

        if (render && tunnel.segments != null && tunnel.segments.Length > 0) {
            RenderFace();
            RenderSide();
            render = false;
        }

    }

    private IEnumerator IERender() {
        while (true) {
            render = true;
            yield return new WaitForSeconds(1f);
        }
    }

    private void RenderSide() {

        List<Vector3> side = new List<Vector3>();
        List<Vector3> shadow = new List<Vector3>();

        for (int i = 0; i < tunnel.segments.Length; i++) {
            Vector3 point = Vector3.zero;
            Vector3 shadowPoint = Vector3.zero;
            if (right) {
                point = tunnel.segments[i].rightPointA + Vector3.left * 0.25f + Vector3.down * 1f;
                shadowPoint = point + Vector3.left * 1.5f;
            }
            else {
                point = tunnel.segments[tunnel.segments.Length - 1 - i].leftPointA + Vector3.right * 0.25f + Vector3.down * 1f;
                shadowPoint = point + Vector3.right * 1.5f;
            }

            point.z = GameRules.CliffSideDepth;
            shadowPoint.z = GameRules.CliffShadowDepth;

            side.Add(point);
            shadow.Add(shadowPoint);

        }

        cliffSide.startWidth = 2f;
        cliffSide.endWidth = 2f;
        cliffSide.positionCount = tunnel.segments.Length;
        cliffSide.SetPositions(side.ToArray());

        cliffShadow.startWidth = 3f;
        cliffShadow.endWidth = 3f;
        cliffShadow.positionCount = tunnel.segments.Length;
        cliffShadow.SetPositions(shadow.ToArray());

        List<Vector2> colliderPoints = new List<Vector2>();
        if (right) {
            colliderPoints.Add((Vector2)tunnel.segments[0].rightPointA + Vector2.right * 50f);
            for (int i = 0; i < tunnel.segments.Length; i++) {
                colliderPoints.Add((Vector2)tunnel.segments[i].rightPointA);
            }
            colliderPoints.Add((Vector2)tunnel.segments[tunnel.segments.Length - 1].rightPointA + Vector2.right * 50f);
        }
        else {
            colliderPoints.Add((Vector2)tunnel.segments[0].leftPointA + Vector2.left * 50f);
            for (int i = 0; i < tunnel.segments.Length; i++) {
                colliderPoints.Add((Vector2)tunnel.segments[i].leftPointA);
            }
            colliderPoints.Add((Vector2)tunnel.segments[tunnel.segments.Length - 1].leftPointA + Vector2.left * 50f);
        }

        GetComponent<PolygonCollider2D>().SetPath(0, colliderPoints);

    }

    void OnTriggerEnter2D(Collider2D collider) {

        if (collider.GetComponent<Player>() != null) {
            print("hello");
        }

    }

    void RenderFace() {

        List<Vector3> points = new List<Vector3>();
        List<Color> colors = new List<Color>();
        List<int> indices = new List<int>();

        float halfScreenWidth = GameRules.ScreenPixelWidth / GameRules.PixelsPerUnit / 2f;

        int index = 0;
        for (int i = 0; i < tunnel.segments.Length; i++) {

            if (right) {
                points.Add(tunnel.segments[i].rightPointA - tunnel.transform.localPosition);
                points.Add(points[points.Count - 1] + Vector3.right * halfScreenWidth);
                points.Add(tunnel.segments[i].rightPointB - tunnel.transform.localPosition);
                points.Add(points[points.Count - 1] + Vector3.right * halfScreenWidth);

                indices.Add(4 * index + 0); // left 1
                indices.Add(4 * index + 3); // left 2
                indices.Add(4 * index + 1); // right 1

                indices.Add(4 * index + 0); // right 1
                indices.Add(4 * index + 2); // right 2
                indices.Add(4 * index + 3); // left 2
            }
            else {
                points.Add(tunnel.segments[i].leftPointA - tunnel.transform.localPosition);
                points.Add(points[points.Count - 1] + Vector3.left * halfScreenWidth);
                points.Add(tunnel.segments[i].leftPointB - tunnel.transform.localPosition);
                points.Add(points[points.Count - 1] + Vector3.left * halfScreenWidth);

                indices.Add(4 * index + 0); // left 1
                indices.Add(4 * index + 1); // right 1
                indices.Add(4 * index + 3); // left 2

                indices.Add(4 * index + 0); // right 1
                indices.Add(4 * index + 3); // left 2
                indices.Add(4 * index + 2); // right 2
            }

            colors.Add(backgroundColor);
            colors.Add(backgroundColor);
            colors.Add(backgroundColor);
            colors.Add(backgroundColor);

            index += 1;
        }

        for (int i = 0; i < points.Count; i++) {
            points[i] = new Vector3(points[i].x, points[i].y, GameRules.CliffDepth);
        }

        meshFilter.mesh.SetVertices(points);
        meshFilter.mesh.SetIndices(indices.ToArray(), MeshTopology.Triangles, 0);
        meshFilter.mesh.colors = colors.ToArray();
        meshFilter.mesh.RecalculateBounds();

    }

}

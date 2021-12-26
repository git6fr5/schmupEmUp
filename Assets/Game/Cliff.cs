using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class Cliff : MonoBehaviour {

    MeshFilter meshFilter;

    public Tunnel tunnel;
    public Color backgroundColor;
    public bool render;

    public bool right;

    void Start() { 
        meshFilter = GetComponent<MeshFilter>();
        meshFilter.mesh = new Mesh();

        StartCoroutine(IERender());
    }

    bool skipFrame = true;
    void Update() {

        if (skipFrame) {
            skipFrame = false;
            return;
        }

        if (render && tunnel.segments != null && tunnel.segments.Length > 0) {
            Render(0, tunnel.segments.Length);
            render = false;
        }

    }

    private IEnumerator IERender() {
        while (true) {
            render = true;
            yield return new WaitForSeconds(1f);
        }
    }

    void Render(int startIndex, int finalIndex) {

        List<Vector3> points = new List<Vector3>();
        List<Color> colors = new List<Color>();
        List<int> indices = new List<int>();

        float halfScreenWidth = GameRules.ScreenPixelWidth / GameRules.PixelsPerUnit / 2f;

        int index = 0;
        for (int i = startIndex; i < finalIndex; i++) {

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

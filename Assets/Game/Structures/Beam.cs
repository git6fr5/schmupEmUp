using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public class Beam : MonoBehaviour {
    
    public Vector3 leftNode;
    public Vector3 rightNode;

    public LineRenderer lineRenderer;
    public Material[] beamMaterials;

    public float beamWidth;
    public float parrallaxFactor;

    public void Init(int index) {
        index = index % beamMaterials.Length;
        lineRenderer.materials = new Material[1] { beamMaterials[index] };
        leftNode.z = GameRules.BeamDepth + 10f * index;
        rightNode.z = GameRules.BeamDepth + 10f * index;
        parrallaxFactor = GameRules.GetParrallax(leftNode.z);

        Draw();
    }

    void Update() {
        Draw();

        if (leftNode.y < GameRules.MainCamera.transform.position.y - GameRules.ScreenPixelHeight / GameRules.PixelsPerUnit - 25f) {
            Destroy(gameObject);
        }
    }

    void Draw() {

        rightNode += parrallaxFactor * GameRules.ScrollSpeed * Vector3.up * Time.deltaTime;
        leftNode += parrallaxFactor * GameRules.ScrollSpeed * Vector3.up * Time.deltaTime;

        lineRenderer.startWidth = beamWidth;
        lineRenderer.endWidth = beamWidth;
        lineRenderer.positionCount = 2;
        lineRenderer.SetPositions(new Vector3[] { rightNode, leftNode });

    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public class Beam : MonoBehaviour {
    
    public Vector3 leftNode;
    public Vector3 rightNode;

    public SpriteShapeController spriteShapeController;
    public Material[] beamMaterials;

    public void Init(int index) {
        index = index % shapes.Length;
        spriteShapeController.spriteShape = shapes[index];
        leftNode.z = 0.01f;
        rightNode.z = 0.01f;
        Draw();
    }

    void Draw() {

        spriteShapeController.spline.Clear();
        spriteShapeController.spline.InsertPointAt(0, leftNode - transform.localPosition);
        spriteShapeController.spline.InsertPointAt(1, rightNode - transform.localPosition);
        spriteShapeController.spline.SetTangentMode(0, ShapeTangentMode.Continuous);
        spriteShapeController.spline.SetTangentMode(1, ShapeTangentMode.Continuous);

        //for (int i = 0; i < ropeSegments.Length; i++) {
        //    // spline.InsertPointAt(i, segments[i].leftPointA - transform.localPosition);
        //    tailShape.spline.InsertPointAt(i, ropeSegments[i] - transform.localPosition);
        //    tailShape.spline.SetTangentMode(i, ShapeTangentMode.Continuous);
        //}

    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tank : MonoBehaviour {

    public static float Speed = 2f;
    public Vector3[] path;

    public int pathIndex = 0;

    public void Init(Vector3[] path) {

        this.path = path;
        transform.position = path[0];
        pathIndex = 1;

        gameObject.SetActive(true);

    }

    void Update() {

        Move();

    }

    private void Move() {

        Vector3 target = path[pathIndex] - transform.position;
        transform.position += target.normalized * Speed * Time.deltaTime;
        if (target.magnitude < 0.05f) {
            pathIndex += 1;
            pathIndex = pathIndex % path.Length;
        }


    }

}

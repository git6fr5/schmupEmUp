using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Orbital : MonoBehaviour {

    public Player player;

    Vector3 displacement;
    Vector3 target;
    public float speed;
    public float orbitRadius = 1f;

    // public int count;

    void Update() {
        if (Player.MouseAimOrb) {
            // Aim();
        }
        else {
            // Move();
        }

        if (GameRules.Snake) {
            // gameObject.SetActive(false);
        }

        Attach();

    }

    public Vector3 orbitA;
    public Vector3 orbitB;

    public Gun[] lives;

    private void Attach() {

        transform.position = player.ropeSegments[1];
        // transform.eulerAngles = Vector3.forward * Vector2.SignedAngle(Vector2.down, ((Vector2)(player.ropeSegments[0] - player.ropeSegments[1])).normalized);

        int index = (int)Mathf.Floor(player.ropeSegments.Length / 3);
        orbitA = player.ropeSegments[index];
        
        index = (int)Mathf.Floor(2f * (float)player.ropeSegments.Length / 3f);
        orbitB = player.ropeSegments[index];

    }

    private void Aim() {

        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z -= mousePos.z;

        target = mousePos - player.transform.position;

        // Vector3 velocity = -(target - transform.position).normalized * speed;
        transform.localPosition = target.normalized * orbitRadius;

        //transform.position = player.transform.position + (transform.position - player.transform.position).normalized * orbitRadius;
        //transform.position += GameRules.ScrollSpeed * Vector3.up * Time.deltaTime;
    }

    private void Move() {


        ////// Move towards the player
        //Vector3 displacement = (player.transform.position - transform.position); // Clarity's sake.
        //float sqrMagnitude = displacement.sqrMagnitude; // Clarity's sake.
        //if (sqrMagnitude > orbitRadius * orbitRadius) {
        //    velocity += displacement.normalized * acceleration * sqrMagnitude   * Time.deltaTime;
        //}

        //velocity *= damping;
        //if (velocity.sqrMagnitude < threshold * threshold) {
        //    // velocity = Vector3.zero;
        //}

        //transform.position += velocity * Time.deltaTime;

        if (player.velocity != Vector2.zero) {
            displacement = (Vector3)player.velocity.normalized * (orbitRadius + player.velocity.magnitude / 5f);
        }
        target = player.transform.position - displacement;

        Vector3 velocity = -(target - transform.position).normalized * speed;
        // velocity.y *= Mathf.Sign(velocity.y);
        //Vector3 newPosition = transform.position + velocity * Time.deltaTime;
        //float angle = Vector2.SignedAngle(Vector2.down, newPosition);
        //angle = angle < 0f ? angle + 360f : angle;
        //if (angle < 270f && angle > 90f) {
        //    transform.position += velocity * Time.deltaTime;
        //}

        transform.position += velocity * Time.deltaTime;

        transform.position = player.transform.position + (transform.position - player.transform.position).normalized * orbitRadius;
        transform.position += GameRules.ScrollSpeed * Vector3.up * Time.deltaTime;

    }
}

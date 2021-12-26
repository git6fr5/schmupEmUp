using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Orbital : MonoBehaviour {

    public Player player;

    Vector3 displacement;
    Vector3 target;
    public float speed;
    public float orbitRadius = 1f;

    void Update() {

        Move();

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

        Vector3 velocity = (target - transform.position).normalized * speed;
        transform.position += velocity * Time.deltaTime;

        transform.position = player.transform.position + (transform.position - player.transform.position).normalized * orbitRadius;
        transform.position += GameRules.ScrollSpeed * Vector3.up * Time.deltaTime;

    }
}

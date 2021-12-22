using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(CircleCollider2D))]
public class Player : MonoBehaviour {

    public float maxSpeed;
    public float acceleration;
    [Range(0f, 1f)] public float damping;
    public Vector2 velocity;

    // Start is called before the first frame update
    void Start() {

    }

    // Update is called once per frame
    void Update() {
        Move();
    }

    // Movement mechanic
    void Move() {

        // Damping
        velocity *= damping;
        if (velocity.sqrMagnitude < GameRules.MovementPrecision * GameRules.MovementPrecision) {
            velocity = Vector2.zero;
        }

        // Input
        Vector2 movementVector = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        velocity += acceleration * movementVector;

        // Clamp
        float y = Mathf.Sign(velocity.y) * Mathf.Min(maxSpeed, Mathf.Abs(velocity.y));
        float x = Mathf.Sign(velocity.x) * Mathf.Min(maxSpeed, Mathf.Abs(velocity.x));
        velocity = new Vector2(x, y);

        // Move
        transform.position += (Vector3)velocity * Time.deltaTime;
    }
}

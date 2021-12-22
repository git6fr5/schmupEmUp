using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour {

    public static float defaultLifeTime = 3f;

    public enum Type {
        Red,
        Blue
    }

    public Type type;
    public Vector2 velocity;
    public Vector2 acceleration;

    // Run this to initialize the bullet
    public void Init(Vector2 origin, Type type, Vector2 velocity, Vector2 acceleration, float lifetime = -1) {
        transform.position = origin;
        this.type = type;
        this.velocity = velocity;
        this.acceleration = acceleration;

        if (lifetime == -1) {
            lifetime = defaultLifeTime;
        }
        Destroy(gameObject, lifetime);
        gameObject.SetActive(true);
    }

    // Update is called once per frame
    void Update() {
        if (acceleration.sqrMagnitude < 0.001f * 0.001f) {
            acceleration = Vector2.zero;
        }
        velocity += acceleration * Time.deltaTime;
        transform.position += (Vector3)velocity * Time.deltaTime;
    }
}

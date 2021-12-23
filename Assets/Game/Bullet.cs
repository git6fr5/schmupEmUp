using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Type = GameRules.Type;

[RequireComponent(typeof(CircleCollider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class Bullet : MonoBehaviour {

    SpriteRenderer spriteRenderer;
    CircleCollider2D hitbox;

    public static float defaultLifeTime = 3f;

    public Type type;
    public Vector2 velocity;
    public Vector2 acceleration;

    // Run this to initialize the bullet
    public void Init(Vector2 origin, Type type, Vector2 velocity, Vector2 acceleration, float lifetime = -1f) {

        transform.position = origin;
        this.type = type;
        this.velocity = velocity;
        this.acceleration = acceleration;

        if (lifetime == -1) {
            lifetime = defaultLifeTime;
        }
        Destroy(gameObject, lifetime);
        gameObject.SetActive(true);

        Color(); // Shouldn't need to do this in update.
        Collider();
    }

    // Update is called once per frame
    void Update() {
        if (acceleration.sqrMagnitude < 0.001f * 0.001f) {
            acceleration = Vector2.zero;
        }
        velocity += acceleration * Time.deltaTime;
        transform.position += (Vector3)velocity * Time.deltaTime;
        Scroll();
        Color();
    }

    // Color
    void Color() {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (type == Type.RedEnemy || type == Type.RedPlayer) {
            spriteRenderer.material = GameRules.RedMaterial;
        }
        else if (type == Type.BlueEnemy || type == Type.BluePlayer) {
            spriteRenderer.material = GameRules.BlueMaterial;
        }
    }

    void Collider() {
        hitbox = GetComponent<CircleCollider2D>();
        hitbox.isTrigger = true;
    }

    // The natural scrolling
    void Scroll() {
        transform.position += GameRules.ScrollSpeed * Vector3.up * Time.deltaTime;
    }
}

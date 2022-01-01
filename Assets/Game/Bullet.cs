using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Type = GameRules.Type;

[RequireComponent(typeof(CircleCollider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class Bullet : MonoBehaviour {

    public Sprite[] sprites;
    public int index;

    SpriteRenderer spriteRenderer;
    CircleCollider2D hitbox;

    public static float defaultLifeTime = 10f;

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

        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = sprites[index];

        Destroy(gameObject, lifetime);
        gameObject.SetActive(true);

        Color(); // Shouldn't need to do this in update.
        Collider();

        if (type == GameRules.Type.RedEnemy) {
            GameRules.PlayAnimation(transform.position, GameRules.RedFireAnimation, true, transform);
        }
        else {
            GameRules.PlayAnimation(transform.position, GameRules.BlueFireAnimation, true, transform);
        }
    }

    Player player;
    // Update is called once per frame
    void Update() {
        if (acceleration.sqrMagnitude < 0.001f * 0.001f) {
            acceleration = Vector2.zero;
        }

        if (player == null) {
            player = (Player)GameObject.FindObjectOfType(typeof(Player));
        }

        if (player != null) {
            if (player.type == (Type)((int)type + 2)) {
                spriteRenderer.material.SetFloat("_Opacity", 1f);
                transform.localScale = new Vector3(1f, 1f, 1f);
            }
            else {
                spriteRenderer.material.SetFloat("_Opacity", 0.5f);
                transform.localScale = new Vector3(0.5f, 0.5f, 1f);
            }
        }

        velocity += acceleration * Time.deltaTime;
        transform.position += (Vector3)velocity * Time.deltaTime;
        Scroll();
        // Color();
        Rotate();
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
        spriteRenderer.material.SetFloat("_Highlight", 1);
    }

    void Rotate() {
        float angle = Vector2.SignedAngle(Vector2.up, velocity);
        angle = angle < 0f ? angle + 360f : angle;
        transform.eulerAngles = new Vector3(0f, 0f, angle);
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

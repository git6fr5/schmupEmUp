using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Type = GameRules.Type;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(CircleCollider2D))]
public class Player : MonoBehaviour {

    public static KeyCode InputKey = KeyCode.Space;
    public static bool MouseAim = true;

    SpriteRenderer spriteRenderer;
    CircleCollider2D hitbox;

    public Type type;

    public float maxSpeed;
    public float acceleration;
    [Range(0f, 1f)] public float damping;
    public Vector2 velocity;

    private Camera viewCam;
    public Vector2 viewOffset;

    // Start is called before the first frame update
    void Start() {

        spriteRenderer = GetComponent<SpriteRenderer>();
        hitbox = GetComponent<CircleCollider2D>();
        hitbox.isTrigger = true;

        viewCam = Camera.main;
        viewCam.transform.position = new Vector3(transform.position.x + viewOffset.x, transform.position.y + viewOffset.y, viewCam.transform.position.z);
    }

    // Update is called once per frame
    void Update() {
        Move();
        Scroll();
        Color();
    }

    void FixedUpdate() {
        Collision();
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

    // The natural scrolling
    void Scroll() {
        transform.position += GameRules.ScrollSpeed * Vector3.up * Time.deltaTime;
        viewCam.transform.position += GameRules.ScrollSpeed * Vector3.up * Time.deltaTime;
        viewCam.transform.position = new Vector3(transform.position.x + viewOffset.x, viewCam.transform.position.y, viewCam.transform.position.z);
    }

    // Color
    void Color() {
        if (type == Type.RedPlayer) {
            spriteRenderer.material = GameRules.RedMaterial;
        }
        else if (type == Type.BluePlayer) {
            spriteRenderer.material = GameRules.BlueMaterial;
        }
    }

    void Collision() {
        print("hello");

        float radius = 0.3f;
        Collider2D[] collisions = Physics2D.OverlapCircleAll(transform.position, radius);

        for (int i = 0; i < collisions.Length; i++) {
            Bullet bullet = collisions[i].GetComponent<Bullet>();
            if (bullet != null) {

                int bulletType = (int)bullet.type;
                int playerType = (int)type - GameRules.ColorPaletteSize;
                // There's a smarter way to do this I'm sure.
                if (bulletType < GameRules.ColorPaletteSize) {
                    if (bulletType == playerType) {
                        //
                    }
                    else if (bulletType != playerType) {
                        type = (Type)(bulletType + GameRules.ColorPaletteSize);
                    }
                }

            }
        }
            
    }
}

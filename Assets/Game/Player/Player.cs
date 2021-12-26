using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Type = GameRules.Type;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(CircleCollider2D))]
public class Player : MonoBehaviour {

    public static float SegmentLength = 0.25f;
    public static int ConstraintDepth = 20;

    public static Vector3 ViewOffset;
    public static KeyCode InputKey = KeyCode.Space;
    public static bool MouseAim = false;

    [HideInInspector] public SpriteRenderer spriteRenderer;
    CircleCollider2D hitbox;

    public Type type;

    public int maxLives = 5;
    public int lives = 3;
    public bool invincible;
    public float explosionDuration;
    public float invincibilityDuration;

    public float maxSpeed;
    public float acceleration;
    [Range(0f, 1f)] public float damping;
    public Vector2 velocity;

    private Camera viewCam;
    public Vector2 viewOffset;

    public float ropeLength;
    public float weight;
    private int segmentCount; // The number of segments.
    public Vector3[] ropeSegments; // The current positions of the segments.
    protected Vector3[] prevRopeSegments; // The previous positions of the segments.

    // Start is called before the first frame update
    void Start() {

        spriteRenderer = GetComponent<SpriteRenderer>();
        hitbox = GetComponent<CircleCollider2D>();
        hitbox.isTrigger = true;

        viewCam = Camera.main;
        viewCam.transform.position = new Vector3(transform.position.x + viewOffset.x, transform.position.y + viewOffset.y, viewCam.transform.position.z);
        invincible = false;

        RopeSegments();
    }

    // Update is called once per frame
    void Update() {
        Move();
        Scroll();
        Shade();
    }

    void FixedUpdate() {
        Collision();
        // RenderRope();
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
        velocity += acceleration * (Vector2)(Quaternion.Euler(0f, 0f, viewCam.transform.eulerAngles.z) * (Vector3)movementVector);

        // Clamp
        float y = Mathf.Sign(velocity.y) * Mathf.Min(maxSpeed, Mathf.Abs(velocity.y));
        float x = Mathf.Sign(velocity.x) * Mathf.Min(maxSpeed, Mathf.Abs(velocity.x));
        velocity = new Vector2(x, y);

        // Move
        transform.position += (Vector3)velocity * Time.deltaTime;
    }

    // The natural scrolling
    void Scroll() {

        ViewOffset = viewOffset;

        transform.position += GameRules.ScrollSpeed * Vector3.up * Time.deltaTime;
        float deltaX = transform.position.x - (viewCam.transform.position.x + viewOffset.x);
        if (Mathf.Abs(deltaX) > 1f) {
            viewCam.transform.position += (Mathf.Sign(deltaX) * GameRules.ScrollSpeed * Time.deltaTime * Vector3.right);
        }
    }

    // Color
    void Shade() {
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
                        Respawn();
                    }
                    else if (bulletType != playerType) {
                        ChangeColor(bulletType);
                    }
                    Destroy(bullet.gameObject);

                }
            }
        }
            
    }

    private void ChangeColor(int bulletType) {
        type = (Type)(bulletType + GameRules.ColorPaletteSize);
    }

    public void Respawn(bool hitWall = false) {
        if (!invincible) {
            invincible = true;
            lives -= 1;
            if (lives <= 0) {
                StartCoroutine(IEReset(explosionDuration * 2f));
            }
            else {
                StartCoroutine(IEInvicibility(explosionDuration, invincibilityDuration, 0.1f, hitWall));

            }
        }
    }

    private IEnumerator IEReset(float explosionDelay) {

        invincible = true;
        Time.timeScale = 0.5f;
        ShakeUI.StartShake(1f, explosionDelay * Time.timeScale);
        yield return new WaitForSeconds(explosionDelay * Time.timeScale);

        Time.timeScale = 1f;
        RestartUI.Restart();

        yield return null;

    }

    private IEnumerator IEInvicibility(float explosionDelay, float invincibilityDelay, float flickerDelay, bool hitWall) {

        invincible = true;
        Time.timeScale = 0.5f;
        ShakeUI.StartShake(0.25f, explosionDelay * Time.timeScale);

        for (int i = 0; i < (int)(explosionDelay / flickerDelay); i++) {
            spriteRenderer.enabled = !spriteRenderer.enabled;
            yield return new WaitForSeconds(flickerDelay * Time.timeScale);
        }

        Time.timeScale = 1f;
        if (hitWall) {
            transform.position = viewCam.transform.position - (Vector3)viewOffset;
            transform.position -= transform.position.z * Vector3.forward;
        }

        for (int i = 0; i < (int)(invincibilityDelay/ flickerDelay); i++) {
            spriteRenderer.enabled = !spriteRenderer.enabled;
            yield return new WaitForSeconds(flickerDelay);
        }

        spriteRenderer.enabled = true;
        invincible = false;

        yield return null;
    }

    // Initalizes the rope segments.
    void RopeSegments() {
        // Get the number of segments for a rope of this length.
        segmentCount = (int)Mathf.Ceil(ropeLength / SegmentLength);

        // Initialize the rope segments.
        ropeSegments = new Vector3[segmentCount];
        prevRopeSegments = new Vector3[segmentCount];

        for (int i = 0; i < segmentCount; i++) {
            ropeSegments[i] = transform.position;
            prevRopeSegments[i] = ropeSegments[i];
        }
        ropeSegments[segmentCount - 1] += ropeLength * Vector3.down;
    }

    // Renders the rope
    void RenderRope() {
        Simulation();
        for (int i = 1; i < ropeSegments.Length; i++) {
            Debug.DrawLine(ropeSegments[i-1], ropeSegments[i], Color.red, Time.fixedDeltaTime, false);
        }

    }

    private float ticks;
    public float frequency;
    public float waveAmplitude;

    void Simulation() {
        ticks += Time.fixedDeltaTime;
        Vector3 scrollForce = new Vector3(0f, -weight * GameRules.ScrollSpeed, 0f);
        for (int i = 0; i < segmentCount; i++) {

            float ratio = (float)i; // ((float)i) / ((float)segmentCount);
            Vector3 waveFace = waveAmplitude * new Vector3(Mathf.Sin(2 * Mathf.PI * ticks * ratio * frequency), 0f, 0f); 

            Vector3 velocity = ropeSegments[i] - prevRopeSegments[i];
            prevRopeSegments[i] = ropeSegments[i];
            ropeSegments[i] += velocity;
            ropeSegments[i] += scrollForce * Time.fixedDeltaTime;
            ropeSegments[i] += waveFace * Time.fixedDeltaTime;
        }
        for (int i = 0; i < ConstraintDepth; i++) {
            Constraints();
        }
    }

    void Constraints() {
        ropeSegments[0] = transform.position;
        for (int i = 1; i < segmentCount; i++) {
            // Get the distance and direction between the segments.
            float newDist = (ropeSegments[i - 1] - ropeSegments[i]).magnitude;
            Vector3 direction = (ropeSegments[i - 1] - ropeSegments[i]).normalized;

            // Get the error term.
            float error = newDist - SegmentLength;
            Vector3 errorVector = direction * error;

            // Adjust the segments by the error term.
            if (i != 1) {
                ropeSegments[i - 1] -= errorVector * 0.5f;
            }
            ropeSegments[i] += errorVector * 0.5f;
        }
    }

}

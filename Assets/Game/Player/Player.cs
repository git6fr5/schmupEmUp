using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

using Type = GameRules.Type;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(CircleCollider2D))]
public class Player : MonoBehaviour {

    public int score;

    public static float SegmentLength = 0.25f;
    public static int ConstraintDepth = 20;

    public static Vector3 ViewOffset;
    public static KeyCode InputKey = KeyCode.Space;
    public static bool MouseAimOrb = true;
    public static bool MouseAim = true;

    [HideInInspector] public SpriteRenderer spriteRenderer;
    CircleCollider2D hitbox;

    public Type type;

    public int maxLives = 9;
    public int lives = 3;
    public bool invincible;
    public float explosionDuration;
    public float invincibilityDuration;

    public Orbital livesOrbital;

    public float maxSpeed;
    public float acceleration;
    [Range(0f, 1f)] public float damping;
    public Vector2 velocity;

    private Camera viewCam;
    public Vector2 viewOffset;

    bool knockback = false;

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
        Highlight();
        Orbs();
    }

    void FixedUpdate() {
        Collision();
        RenderRope();
    }

    private void Orbs() {

        if (lives > livesOrbital.lives.Length) {
            lives = livesOrbital.lives.Length;
        }

        for (int i = 0; i < livesOrbital.lives.Length; i++) {
            livesOrbital.lives[i].gameObject.SetActive(i < lives);
        }

    }

    // Movement mechanic
    void Move() {

        if (knockback) {
            transform.position += (Vector3)velocity * Time.deltaTime;
            return;
        }

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
            // viewCam.transform.position += (Mathf.Sign(deltaX) * GameRules.ScrollSpeed * Time.deltaTime * Vector3.right);
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
        // print("hello");

        float radius = 0.3f;
        Collider2D[] collisions = Physics2D.OverlapCircleAll(transform.position, radius);

        for (int i = 0; i < collisions.Length; i++) {
            Bullet bullet = collisions[i].GetComponent<Bullet>();
            if (bullet != null) {

                int bulletType = (int)bullet.type;
                int playerType = (int)type - GameRules.ColorPaletteSize;
                // There's a smarter way to do this I'm sure.
                if (bulletType < GameRules.ColorPaletteSize) {

                    if (bulletType != playerType) {
                        Respawn();
                    }
                    else if (bulletType == playerType) {
                        // lives += 1;
                    }

                    Destroy(bullet.gameObject);

                }
            }

            Cliff cliff = collisions[i].GetComponent<Cliff>();
            if (cliff != null) {
                print("HITTING CLIFF");
                Respawn();
                if (!knockback) {
                    StartCoroutine(IEKnockback());
                }
            }

            Change change = collisions[i].GetComponent<Change>();
            if (change != null) {

                int changeType = (int)change.type;
                int playerType = (int)type;

                if (changeType != playerType) {
                    type = (Type)(changeType);
                    lives += 1;
                    change.Eat();
                }
                else {
                    if (GameRules.AlternatingOrbs) {
                        Respawn();
                        lives -= 1;
                        Destroy(change.gameObject);
                    }
                    else {
                        lives += 1;
                        change.Eat();

                    }
                }
            }

        }
    }

    private IEnumerator IEKnockback() {
        velocity = Quaternion.Euler(0f, 0f, 180f) * velocity * 2f;
        knockback = true;
        yield return new WaitForSeconds(1f);
        knockback = false;
        yield return null;
    }

    private void ChangeColor(int bulletType) {
        type = (Type)(bulletType + GameRules.ColorPaletteSize);
    }

    public void Respawn(bool hitWall = false) {
        if (!invincible) {
            GameRules.PlayAnimation(transform.position, GameRules.ExplosionAnim);
            invincible = true;
            lives -= 1;
            if (lives <= 0) {
                StartCoroutine(IEReset(explosionDuration * 5f));
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
        for (int i = 0; i < 50; i++) {
            GameRules.PlayAnimation(transform.position + (Vector3)Random.insideUnitCircle * 15f, GameRules.ExplosionAnim);
            yield return new WaitForSeconds(explosionDelay * Time.timeScale / 50f);
        }

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

        for (int i = 0; i < (int)(invincibilityDelay / flickerDelay); i++) {
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
            ropeSegments[i] = transform.position + Vector3.down * i * SegmentLength;
            prevRopeSegments[i] = ropeSegments[i];
        }
        ropeSegments[segmentCount - 1] += ropeLength * Vector3.down;
    }

    // Renders the rope
    void RenderRope() {
        SimulationB();
        for (int i = 1; i < ropeSegments.Length; i++) {
            Debug.DrawLine(ropeSegments[i - 1], ropeSegments[i], Color.red, Time.fixedDeltaTime, false);
        }

    }

    private float ticks;
    public float frequency;
    public float waveAmplitude;

    void SimulationA() {
        ticks += Time.fixedDeltaTime;
        Vector3 scrollForce = new Vector3(0f, -weight * GameRules.ScrollSpeed, 0f);
        for (int i = 0; i < segmentCount; i++) {

            float ratio = (float)i; // ((float)i) / ((float)segmentCount);
            Vector3 waveFace = waveAmplitude * new Vector3(Mathf.Sin(2 * Mathf.PI * ticks * ratio * frequency), 0f, 0f);

            Vector3 velocity = ropeSegments[i] - prevRopeSegments[i];
            prevRopeSegments[i] = ropeSegments[i];
            ropeSegments[i] += GameRules.ScrollSpeed * Vector3.up * Time.deltaTime;
            ropeSegments[i] += velocity;
            ropeSegments[i] += scrollForce * Time.fixedDeltaTime;
            ropeSegments[i] += waveFace * Time.fixedDeltaTime;
        }
        for (int i = 0; i < ConstraintDepth; i++) {
            Constraints();
        }
    }

    public LineRenderer tailRenderer;
    public float tailStartWidth;
    public float tailEndWidth;

    void SimulationB() {

        // 0 => 1 but just the x?
        //Vector2 absVelocity = new Vector2(velocity.x, Mathf.Abs(velocity.y));
        //float angle = Vector2.SignedAngle(Vector2.up, absVelocity);
        //transform.eulerAngles = Vector3.forward * angle / 2f;
        //tailShape.transform.eulerAngles = Vector3.zero;

        ropeSegments[0] = transform.position;
        for (int i = 1; i < ropeSegments.Length; i++) {
            ropeSegments[i] += GameRules.ScrollSpeed / 20f * Vector3.up * Time.deltaTime;
            ropeSegments[i] += (Vector3)velocity / 20f * Time.deltaTime;
            if ((ropeSegments[i - 1] - ropeSegments[i]).magnitude > SegmentLength) {
                ropeSegments[i] += (ropeSegments[i - 1] - ropeSegments[i]).normalized * Time.deltaTime * 5f;
            }
        }

        for (int i = 0; i < ConstraintDepth; i++) {
            Constraints();
        }

        //for (int i = 1; i < ropeSegments.Length; i++) {
        //    if ((ropeSegments[i - 1] - ropeSegments[i]).magnitude < SegmentLength * 0.95f) {
        //        ropeSegments[i] = ropeSegments[i - 1] + (SegmentLength * 0.95f) * ((ropeSegments[i] - ropeSegments[i-1]).normalized);
        //    }
        //}

        tailRenderer.startWidth = tailStartWidth;
        tailRenderer.endWidth = tailEndWidth;
        tailRenderer.positionCount = ropeSegments.Length;
        tailRenderer.SetPositions(ropeSegments);

        if (type == Type.BluePlayer) {
            tailRenderer.materials[0].SetColor("_Color", GameRules.Blue);
        }
        else {
            tailRenderer.materials[0].SetColor("_Color", GameRules.Red);
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
        ropeSegments[0] = transform.position;
    }

    private float highlightTicks = 0f;
    private float highlightFrequency = 0.25f;
    private void Highlight() {

        highlightTicks += Time.deltaTime;
        if (highlightTicks > 1) {
            // highlightTicks -= 1;
        }

        float val = Mathf.Sin(2 * Mathf.PI * highlightTicks * highlightFrequency);
        val += 1f;
        val *= (0.5f * 0.5f * 0.5f);
        val += 0.75f;
        GetComponent<SpriteRenderer>().material.SetFloat("_Highlight", val);

    }

}

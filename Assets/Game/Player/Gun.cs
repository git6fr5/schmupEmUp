using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using BulletType = GameRules.Type;

[RequireComponent(typeof(SpriteRenderer))]
public class Gun : MonoBehaviour {

    public Player player;
    public Bullet bullet;
    public float bulletSpeed;

    SpriteRenderer spriteRenderer;

    // Start is called before the first frame update
    void Start() {
        spriteRenderer = GetComponent<SpriteRenderer>();
        transform.parent = null;
    }

    // Update is called once per frame
    void Update() {

        //
        Move();
        Fire();
        Color();
    }

    //private Vector3 velocity = Vector3.zero;
    //[Range(0f, 10f)] public float acceleration = 3f;
    //[Range(0.95f, 1f)] public float damping = 0.95f;
    //private float threshold = 0.001f;

    Vector3 displacement;
    Vector3 target;
    public float speed;
    public float orbitRadius = 1f;

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

    private void Fire() {

        // Vector3 direction = Vector3.up;
        Vector3 direction = (transform.position - player.transform.position).normalized;

        if (Player.MouseAim) {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            direction = ((Vector3)mousePos - transform.position).normalized;
            if (Input.GetMouseButtonDown(0)) {
                Fire(direction);
            }
            if (Input.GetMouseButton(0)) {
                Beam(direction);
            }

        }
        else {
            if (Input.GetKeyDown(Player.InputKey)) {
                Fire(direction);
            }
            if (Input.GetKey(Player.InputKey)) {
                Beam(direction);
            }
        }
    }

    private void Color() {
        spriteRenderer.material = player.spriteRenderer.material;
    }

    protected virtual void Fire(Vector2 direction) {
        Bullet newBullet = Instantiate(bullet.gameObject).GetComponent<Bullet>();
        BulletType type = ((Player)GameObject.FindObjectOfType(typeof(Player))).type;
        newBullet.Init(transform.position, type, bulletSpeed * direction, Vector2.zero);
    }

    protected virtual void Beam(Vector2 direction) {

    }
}

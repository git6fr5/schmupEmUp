using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using BulletType = GameRules.Type;

[RequireComponent(typeof(SpriteRenderer))]
public class Gun : MonoBehaviour {

    public Player player;
    public Orbital orbital;
    public Bullet bullet;
    public float bulletSpeed;

    SpriteRenderer spriteRenderer;

    // Start is called before the first frame update
    void Start() {
        x = ((float)count / MaxCount * 2 * Mathf.PI);
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update() {

        Move();
        Fire();
        Color();
    }

    Vector3 displacement;
    Vector3 target;
    public float speed;
    public float orbitRadius = 1f;

    float x;
    public float period;

    public static int MaxCount = 3;
    public int count;

    private void Move() {

        x += 2 * Mathf.PI * Time.deltaTime * period;

        float angle = Vector2.SignedAngle((Vector2)orbital.transform.localPosition, Vector2.up);
        angle = angle < 0f ? 360 + angle : angle;
        Vector3 newPosition = orbitRadius * new Vector3(Mathf.Cos(x), 0.5f * Mathf.Sin(x), 0f) + orbital.transform.position;
        newPosition = (Quaternion.Euler(0f, 0f, angle) * (newPosition-orbital.transform.position)) + orbital.transform.position;
        transform.position = newPosition;

        float scale = (2f + Mathf.Sin(x)) / 0.5f + 0.5f;
        transform.localScale = new Vector3(scale, scale, 0f);

    }

    private void Fire() {

        // Vector3 direction = Vector3.up;
        Vector3 direction = (orbital.transform.position - player.transform.position).normalized;

        if (Player.MouseAim) {
            // Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            // direction = ((Vector3)mousePos - transform.position).normalized;
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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using BulletType = GameRules.Type;

public class Gun : MonoBehaviour {

    public Bullet bullet;
    public float bulletSpeed;

    // Start is called before the first frame update
    void Start() {

    }

    // Update is called once per frame
    void Update() {

        Vector3 direction = Vector3.up;
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

    protected virtual void Fire(Vector2 direction) {
        Bullet newBullet = Instantiate(bullet.gameObject).GetComponent<Bullet>();
        BulletType type = ((Player)GameObject.FindObjectOfType(typeof(Player))).type;
        newBullet.Init(transform.position, type, bulletSpeed * direction, Vector2.zero);
    }

    protected virtual void Beam(Vector2 direction) {

    }
}

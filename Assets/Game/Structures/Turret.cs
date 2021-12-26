using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Turret : MonoBehaviour {

    public bool blue;
    Player player;
    public Bullet bulletBase;
    public SpriteRenderer turretBase;

    public static float Cooldown = 2f;
    public static float BulletSpeed = 7.5f;
    public static float FireRange = 10f;

    public void Init(bool blue) {

        this.blue = blue;

        if (blue) {
            GetComponent<SpriteRenderer>().material = GameRules.BlueMaterial;
        }
        else {
            GetComponent<SpriteRenderer>().material = GameRules.RedMaterial;
        }

        player = (Player)GameObject.FindObjectOfType(typeof(Player));
        StartCoroutine(IEFire());

        // turretBase.material = GetComponent<SpriteRenderer>().material;
        transform.position -= Vector3.forward;
        turretBase.transform.localPosition += Vector3.forward * 0.5f;

    }

    // Update is called once per frame
    void Update() {
        Point();
    }

    private void Point() {
        //
        if (!GameRules.OnScreen(transform.position)) { return; }

        Vector3 direction = (player.transform.position - transform.position).normalized;
        float angle = Vector2.SignedAngle(direction, Vector2.right);
        angle = angle < 0f ? angle + 360f : angle;

        transform.eulerAngles = new Vector3(0f, 0f, angle);
        turretBase.transform.eulerAngles = Vector3.zero;
    }

    private IEnumerator IEFire() {

        yield return new WaitForSeconds(Random.Range(0, Cooldown));

        while (player != null) {
            if (GameRules.OnScreen(transform.position)) {

                GameRules.Type type = blue ? GameRules.Type.BlueEnemy : GameRules.Type.RedEnemy;
                Bullet newBullet = Instantiate(bulletBase.gameObject).GetComponent<Bullet>();
                Vector3 bulletVelocity = BulletSpeed * (player.transform.position - transform.position).normalized;
                newBullet.Init(transform.position, type, bulletVelocity, Vector3.zero);

            }

            yield return new WaitForSeconds(Cooldown);
        }

        // yield return null;
    }

}

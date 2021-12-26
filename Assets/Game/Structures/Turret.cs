using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using EnemyType = GameRules.Type;

[RequireComponent(typeof(SpriteRenderer))]
public class Turret : MonoBehaviour {

    public bool blue;
    Player player;
    public Bullet bulletBase;
    public SpriteRenderer turretBase;

    public static float Cooldown = 2f;
    public static float BulletSpeed = 7.5f;
    public static float FireRange = 10f;

    public EnemyType type;
    public Platform.PlatformSegment attachPoint;

    public void Init(bool blue, Platform.PlatformSegment attachPoint) {

        this.blue = blue;
        this.attachPoint = attachPoint;

        if (blue) {
            type = EnemyType.BlueEnemy;
            GetComponent<SpriteRenderer>().material = GameRules.BlueMaterial;
        }
        else {
            type = EnemyType.RedEnemy;
            GetComponent<SpriteRenderer>().material = GameRules.RedMaterial;
        }

        player = (Player)GameObject.FindObjectOfType(typeof(Player));
        StartCoroutine(IEFire());

        transform.position = new Vector3(transform.position.x, transform.position.y, GameRules.PlatformEnemies);
        turretBase.transform.localPosition += Vector3.forward * 0.5f;
        turretBase.GetComponent<SpriteRenderer>().material = GetComponent<SpriteRenderer>().material;

    }

    // Update is called once per frame
    void Update() {
        transform.position = new Vector3(attachPoint.midPoint.x, attachPoint.midPoint.y, GameRules.PlatformEnemies);

        Point();
        if (transform.position.y < GameRules.MainCamera.transform.position.y - GameRules.ScreenPixelHeight / GameRules.PixelsPerUnit - 5f) {
            Destroy(gameObject);
        }
    }

    void FixedUpdate() {
        Collision();
    }

    private void Point() {
        
        if (player == null) { return; }
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
            if (GameRules.OnScreen(transform.position) && !insideCliff) {

                GameRules.Type type = blue ? GameRules.Type.BlueEnemy : GameRules.Type.RedEnemy;
                Bullet newBullet = Instantiate(bulletBase.gameObject).GetComponent<Bullet>();
                Vector3 bulletVelocity = BulletSpeed * ((Vector2)player.transform.position - (Vector2)transform.position).normalized;
                newBullet.Init(transform.position, type, bulletVelocity, Vector3.zero);

            }

            yield return new WaitForSeconds(Cooldown);
        }

        // yield return null;
    }

    bool insideCliff;

    void Collision() {

        insideCliff = false;
        float radius = 0.3f;
        Collider2D[] collisions = Physics2D.OverlapCircleAll(transform.position, radius);

        for (int i = 0; i < collisions.Length; i++) {
            Bullet bullet = collisions[i].GetComponent<Bullet>();
            if (bullet != null) {

                int bulletType = (int)bullet.type - GameRules.ColorPaletteSize;
                int enemyType = (int)type;
                // There's a smarter way to do this I'm sure.
                if (bulletType == enemyType) {
                    Destroy(gameObject);
                }
            }

            Cliff cliff = collisions[i].GetComponent<Cliff>();
            if (cliff != null) {
                insideCliff = true;
            }
        }

    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using EnemyType = GameRules.Type;


[RequireComponent(typeof(SpriteRenderer))]
public class Tank : MonoBehaviour {

    public bool blue;
    public Bullet bulletBase;

    public static float Speed = 2f;
    public Vector3[] path;

    public int pathIndex = 0;

    public static float Cooldown = 2f;
    public static float BulletSpeed = 7.5f;
    public static float FireRange = 10f;

    public Sprite[] sprites;

    public EnemyType type;


    public void Init(Vector3[] path, bool blue) {

        this.blue = blue;

        if (blue) {
            type = EnemyType.BlueEnemy;
            GetComponent<SpriteRenderer>().material = GameRules.BlueMaterial;
        }
        else {
            type = EnemyType.RedEnemy;
            GetComponent<SpriteRenderer>().material = GameRules.RedMaterial;
        }

        this.path = path;
        transform.position = new Vector3(path[0].x, path[0].y, GameRules.PlatformEnemies);
        pathIndex = 1;

        gameObject.SetActive(true);
        StartCoroutine(IEFire());


    }

    void Update() {
        Move();
        if (transform.position.y < GameRules.MainCamera.transform.position.y - GameRules.ScreenPixelHeight / GameRules.PixelsPerUnit - 5f) {
            Destroy(gameObject);
        }
    }

    void FixedUpdate() {
        Collision();
    }

    private void Move() {

        Vector3 target = path[pathIndex] - transform.position;
        target.z = 0f;

        transform.position += target.normalized * Speed * Time.deltaTime;
        if (target.magnitude < 0.05f) {
            pathIndex += 1;
            pathIndex = pathIndex % path.Length;
        }

        float angle = Vector2.SignedAngle(Vector2.right, target.normalized);
        Debug.DrawLine(transform.position, transform.position + target.normalized, Color.yellow, Time.deltaTime, false);
        angle = angle < 0f ? angle + 360f : angle;
        // print(angle);

        int index = (int)Mathf.Round(sprites.Length * (angle / 360f));
        index = index % sprites.Length;
        GetComponent<SpriteRenderer>().sprite = sprites[index];

    }

    private IEnumerator IEFire() {

        yield return new WaitForSeconds(Random.Range(0, Cooldown));

        while (true) {
            print("firing");
            if (GameRules.OnScreen(transform.position) && !insideCliff) {

                Vector3 target = path[pathIndex] - transform.position;
                target.z = 0f;

                GameRules.Type type = blue ? GameRules.Type.BlueEnemy : GameRules.Type.RedEnemy;
                Bullet newBullet = Instantiate(bulletBase.gameObject).GetComponent<Bullet>();
                Vector3 bulletVelocity = BulletSpeed * target.normalized;
                newBullet.Init(transform.position, type, bulletVelocity, Vector3.zero);

            }

            yield return new WaitForSeconds(Cooldown);
        }

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

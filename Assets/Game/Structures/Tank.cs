using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    public void Init(Vector3[] path, bool blue) {

        this.blue = blue;

        if (blue) {
            GetComponent<SpriteRenderer>().material = GameRules.BlueMaterial;
        }
        else {
            GetComponent<SpriteRenderer>().material = GameRules.RedMaterial;
        }

        this.path = path;
        transform.position = path[0];
        pathIndex = 1;

        gameObject.SetActive(true);
        StartCoroutine(IEFire());

        transform.position -= Vector3.forward;


    }

    void Update() {
        Move();
    }

    private void Move() {

        Vector3 target = path[pathIndex] - transform.position;
        transform.position += target.normalized * Speed * Time.deltaTime;
        if (target.magnitude < 0.05f) {
            pathIndex += 1;
            pathIndex = pathIndex % path.Length;
        }

        float angle = Vector2.SignedAngle(Vector2.right, target.normalized);
        Debug.DrawLine(transform.position, transform.position + target.normalized, Color.yellow, Time.deltaTime, false);
        angle = angle < 0f ? angle + 360f : angle;
        print(angle);

        int index = (int)Mathf.Round(sprites.Length * (angle / 360f));
        index = index % sprites.Length;
        GetComponent<SpriteRenderer>().sprite = sprites[index];

    }

    private IEnumerator IEFire() {

        yield return new WaitForSeconds(Random.Range(0, Cooldown));

        while (true) {
            print("firing");
            if (GameRules.OnScreen(transform.position)) {

                Vector3 target = path[pathIndex] - transform.position;

                GameRules.Type type = blue ? GameRules.Type.BlueEnemy : GameRules.Type.RedEnemy;
                Bullet newBullet = Instantiate(bulletBase.gameObject).GetComponent<Bullet>();
                Vector3 bulletVelocity = BulletSpeed * target.normalized;
                newBullet.Init(transform.position, type, bulletVelocity, Vector3.zero);

            }

            yield return new WaitForSeconds(Cooldown);
        }

    }

}

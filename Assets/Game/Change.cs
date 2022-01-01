using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Type = GameRules.Type;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(CircleCollider2D))]
public class Change : MonoBehaviour {


    public Player player;
    SpriteRenderer spriteRenderer;
    public GameRules.Type type;

    float ticks = 0f;
    public static float Frequency = 1f;

    public void Init(bool blue, Vector3 position) {

        type = blue ? Type.BluePlayer : Type.RedPlayer;
        Shade();

        transform.position = new Vector3(position.x, position.y, GameRules.OrbDepth);

        player = (Player)GameObject.FindObjectOfType(typeof(Player));

        gameObject.SetActive(true);
    }

    void Update() {

        ticks += Time.deltaTime;

        if (player == null) {
            player = (Player)GameObject.FindObjectOfType(typeof(Player));
        }

        if (player != null) {
            print("Hello");
            if (player.type != type) {
                float scale = 0.25f * Mathf.Sin(2 * Mathf.PI * ticks * Frequency);
                transform.localScale = new Vector3(1.25f + scale, 1.25f + scale, 1f);
                spriteRenderer.material.SetFloat("_Opacity", 1f);
            }
            else {
                transform.localScale = new Vector3(1f, 1f, 1f);
                spriteRenderer.material.SetFloat("_Opacity", 0.5f);
            }
        }

        transform.position += GameRules.ScrollSpeed * Time.deltaTime * Vector3.up * GameRules.GetParrallax(GameRules.BeamDepth);

    }

    // Color
    void Shade() {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (type == Type.RedPlayer) {
            spriteRenderer.material = GameRules.RedMaterial;
        }
        else if (type == Type.BluePlayer) {
            spriteRenderer.material = GameRules.BlueMaterial;
        }
        spriteRenderer.material.SetFloat("_Highlight", 1);
    }

    public void Eat() {

        GameRules.PlaySound(GameRules.EatSound);
        GameRules.PlayAnimation(transform.position, GameRules.EatAnim);
        Destroy(gameObject);

    }

}

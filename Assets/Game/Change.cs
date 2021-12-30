using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Type = GameRules.Type;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(CircleCollider2D))]
public class Change : MonoBehaviour {

    SpriteRenderer spriteRenderer;
    public GameRules.Type type;

    public void Init(bool blue, Vector3 position) {

        type = blue ? Type.BluePlayer : Type.RedPlayer;
        Shade();

        transform.position = new Vector3(position.x, position.y, GameRules.OrbDepth);

        gameObject.SetActive(true);
    }

    void Update() {

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
    }

    public void Eat() {

        GameRules.PlayAnimation(transform.position, GameRules.EatAnim);
        Destroy(gameObject);

    }

}

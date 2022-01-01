using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour {

    public Text scorebox;

    public Transform[] pieces;
    public SpriteRenderer[] instructions;

    float ticks = 0f;

    void Update() {

        if (Input.GetMouseButtonDown(0)) {
            SceneManager.LoadScene("Test");
        }

        scorebox.text = "Score: " + GameRules.Score;


        ticks += Time.deltaTime;
        for (int i = 0; i < pieces.Length; i++) {

            pieces[i].position += Vector3.up * 0.005f * Mathf.Sin(Mathf.PI * (2 * ticks * 0.5f + 2f * (float)i / (float)pieces.Length));

        }

        for (int i = 0; i < instructions.Length; i++) {

            float opacity = Mathf.Sin(Mathf.PI * (2 * ticks * 0.2f + i));
            instructions[i].color = new Color(1f, 1f, 1f, opacity);

        }

        // GetComponent<AudioSource>().Play();

    }
}

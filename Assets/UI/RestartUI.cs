using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RestartUI : MonoBehaviour {

    void Update() {
        if (Input.GetMouseButtonDown(0)) {
            // SceneManager.LoadScene(0);
        }
        if (Input.GetKeyDown(KeyCode.R)) {
            Restart();
        }
    }

    public static void Restart() {
        SceneManager.LoadScene(0);
    }

}

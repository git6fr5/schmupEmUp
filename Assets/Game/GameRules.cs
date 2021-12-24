using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameRules : MonoBehaviour {

    public static string Path = "Assets/Resources/";

    public static float MovementPrecision = 0.001f;
    public static float ScrollSpeed;

    public static Material RedMaterial;
    public static Material BlueMaterial;

    public float scrollSpeed = 5f;
    public float maxScrollSpeed = 10f;
    public float scrollAcceleration = 0.1f;

    public Material redMaterial;
    public Material blueMaterial;

    public static GameObject PlayerObject;
    public static GameObject RestartObject;

    // Should probably just grab this from the camera instead of hardcoding it here.
    public static int ScreenPixelHeight = 360;
    public static int ScreenPixelWidth = 640;
    public static int PixelsPerUnit = 16;

    public static int ColorPaletteSize = 2;
    public enum Type {
        //
        RedEnemy,
        BlueEnemy,
        //
        RedPlayer,
        BluePlayer
    }

    void Start() {
        ScrollSpeed = scrollSpeed;
        RedMaterial = redMaterial;
        BlueMaterial = blueMaterial;

        PlayerObject = ((Player)GameObject.FindObjectOfType(typeof(Player)))?.gameObject;
        RestartObject = ((RestartUI)GameObject.FindObjectOfType(typeof(RestartUI)))?.gameObject;

    }

    void Update() {
        if (scrollSpeed < maxScrollSpeed) {
            scrollSpeed += scrollAcceleration * Time.deltaTime;
        }
        else {
            scrollSpeed = maxScrollSpeed;
        }
        ScrollSpeed = scrollSpeed;

        // Restarting
        if (PlayerObject == null && RestartObject != null) {
            RestartObject.SetActive(true);
        }
        else {
            // RestartObject.SetActive(false);
        }
    }

    [System.Serializable]
    public class RandomParams {
        public float min;
        public float max;

        public float Get() {
            return Random.Range(min, max);
        }

        public float GetRound() {
            return Mathf.Round(Random.Range(min, max));
        }
    }

    [System.Serializable]
    public class SerializableVector3 {

        public float x;
        public float y;
        public float z;

        public SerializableVector3(Vector3 v) {
            this.x = v.x;
            this.y = v.y;
            this.z = v.z;
        }

        public Vector3 Deserialize() {
            return new Vector3(x, y, z);
        }
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShakeUI : MonoBehaviour {

    public static ShakeUI Instance;

    [Range(0f, 5f)] public float shakeStrength = 1f;
    public float shakeDuration = 0.5f;
    float elapsedTime = 0f;
    public bool startShake;
    private bool shake;
    public AnimationCurve curve;

    private Vector3 origin;

    void Start() {
        Instance = this;
    }

    void Update() {
        if (startShake) {
            shake = true;
            origin = transform.position;
            startShake = false;
        }

        // Shake the camera
        if (shake) {
            shake = Shake();
        }
    }

    public bool Shake() {
        elapsedTime += Time.deltaTime;
        if (elapsedTime >= shakeDuration) {
            elapsedTime = 0f;
            return false;
        }
        float strength = shakeStrength * curve.Evaluate(elapsedTime / shakeDuration);
        transform.position = origin + (Vector3)Random.insideUnitCircle * shakeStrength;
        origin += GameRules.ScrollSpeed * Vector3.up * Time.deltaTime;
        return true;
    }

    public static void StartShake(float strength, float duration) {
        if (Instance != null) {
            Instance.shakeDuration = duration;
            Instance.shakeStrength = strength;
            Instance.elapsedTime = 0f;
            Instance.startShake = true;
        }
    }

}
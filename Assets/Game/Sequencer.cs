using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using RandomParams = GameRules.RandomParams;

public class Sequencer : MonoBehaviour {

    public int difficulty = 0;

    public bool isTimed;
    public float duration;
    public float lull;

    [System.Serializable]
    public class SequenceComponent {

        public GameObject sequenceObject;
        public float delay;
        [HideInInspector] public bool isRunning;
        [HideInInspector] public bool isLoading;

        public void Run() {
            // Deattach
            sequenceObject.transform.parent = null;

            // Activate
            isLoading = false;
            sequenceObject.SetActive(true);
            isRunning = true;
        }

        // Check to turn off
        public void CheckEnd() {
            foreach (Transform child in sequenceObject.transform) {
                Wave wave = child.GetComponent<Wave>();
                // Can just turn it off after its complete right?
                // Don't need to wait till its cleared...
                if (wave != null && !wave.isCleared) {
                    return;
                }
            }
            sequenceObject.SetActive(false);
            isRunning = false;
        }

        public void Reset(int difficulty) {
            foreach (Transform child in sequenceObject.transform) {
                Wave wave = child.GetComponent<Wave>();
                // Can just turn it off after its complete right?
                // Don't need to wait till its cleared...
                if (wave != null) {
                    wave.reset = true;
                    wave.waves += difficulty;
                }
            }
        }

    }

    [System.Serializable]
    public class PlatformDistribution {

        public Platform platformBase;

        public RandomParams platformDistance;
        public float turretLikelihood;
        public float tankLikelihood;
        public float mechaLikelihood;

        int lastGeneratedIndex;
        int nextDistance;

        public void Check(Tunnel tunnel) {
            if (tunnel.cameraIndex - lastGeneratedIndex > nextDistance) {
                Generate(tunnel);
                lastGeneratedIndex = tunnel.cameraIndex;
            }
            nextDistance = (int)platformDistance.GetRound();
        }

        // Check to turn off
        private void Generate(Tunnel tunnel) {

            // Get the index.
            int increment = 50;
            int index = (tunnel.cameraIndex - tunnel.startIndex) + increment;
            if (index >= tunnel.segments.Length) {
                index = tunnel.segments.Length - 1;
            }

            bool right = Random.Range(0f, 1f) > 0.5f;

            Platform newPlatform = Instantiate(platformBase.gameObject).GetComponent<Platform>();
            if (right) {
                newPlatform.transform.position = tunnel.segments[index].rightPointA;
            }
            else {
                newPlatform.transform.position = tunnel.segments[index].leftPointA;
            }

            newPlatform.transform.position -= Vector3.forward * (GameRules.TunnelDepth - GameRules.PlatformDepth);
            newPlatform.shift = true;
            newPlatform.shiftRight = right;

            // The type of enemy.
            newPlatform.spawnTurrets = Random.Range(0f, 1f) < turretLikelihood;
            if (!newPlatform.spawnTurrets) {
                newPlatform.spawnTanks = true; // Random.Range(0f, 1f) < tankLikelihood;
            }

            newPlatform.gameObject.SetActive(true);

        }
    }

    public SequenceComponent[] sequenceComponents;
    public PlatformDistribution platformDistribution;
    Tunnel tunnel;

    private Camera trackCam;

    // Start is called before the first frame update
    void Start() {

        tunnel = (Tunnel)GameObject.FindObjectOfType(typeof(Tunnel));
        trackCam = Camera.main;

        for (int i = 0; i < sequenceComponents.Length; i++) {
            StartCoroutine(IESequence(sequenceComponents[i]));
        }

        platformDistribution.turretLikelihood /= (platformDistribution.turretLikelihood + platformDistribution.tankLikelihood);
        platformDistribution.tankLikelihood /= (platformDistribution.turretLikelihood + platformDistribution.tankLikelihood);

    }

    private IEnumerator IESequence(SequenceComponent component) {
        component.isLoading = true;
        yield return new WaitForSeconds(component.delay);
        component.Run();
        if (!isStarted) {
            isStarted = true;
        }
        yield return null;
    }

    public bool isStarted;
    public bool isFinished;
    // Update is called once per frame
    void Update() {

        Scroll();

        for (int i = 0; i < sequenceComponents.Length; i++) {
            if (sequenceComponents[i].isRunning) {
                sequenceComponents[i].CheckEnd();
            }
        }

        platformDistribution.Check(tunnel);

        // Check the whole thing is done.
        isFinished = true;
        if (!isStarted) {
            isFinished = false;
        }
        for (int i = 0; i < sequenceComponents.Length; i++) {
            if (sequenceComponents[i].isRunning || sequenceComponents[i].isLoading) {
                isFinished = false;
                return;
            }
        }
        isStarted = false;

    }

    // The natural scrolling
    void Scroll() {
        transform.position = trackCam.transform.position;
        transform.position -= transform.position.z * Vector3.forward;
    }

    public void Reset(int round) {
        difficulty = round;
        isStarted = false;
        for (int i = 0; i < sequenceComponents.Length; i++) {
            sequenceComponents[i].Reset(difficulty);
        }
    }

}

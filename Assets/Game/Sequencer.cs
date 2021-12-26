﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using RandomParams = GameRules.RandomParams;

public class Sequencer : MonoBehaviour {

    [System.Serializable]
    public class SequenceComponent {

        public GameObject sequenceObject;
        public float delay;
        [HideInInspector] public bool isRunning;

        public void Run() {
            // Deattach
            sequenceObject.transform.parent = null;

            // Activate
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
            int increment = 30;
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

            newPlatform.shift = true;
            newPlatform.shiftRight = right;

            // The type of enemy.
            newPlatform.spawnTurrets = Random.Range(0f, 1f) < turretLikelihood;
            newPlatform.spawnTanks = Random.Range(0f, 1f) < tankLikelihood;

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

    }

    private IEnumerator IESequence(SequenceComponent component) {
        yield return new WaitForSeconds(component.delay);
        component.Run();
        yield return null;
    }

    // Update is called once per frame
    void Update() {

        Scroll();
        for (int i = 0; i < sequenceComponents.Length; i++) {
            if (sequenceComponents[i].isRunning) {
                sequenceComponents[i].CheckEnd();
            }
        }

        platformDistribution.Check(tunnel);

    }

    // The natural scrolling
    void Scroll() {
        transform.position = trackCam.transform.position;
        transform.position -= transform.position.z * Vector3.forward;
    }
}

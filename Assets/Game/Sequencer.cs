using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    public SequenceComponent[] sequenceComponents;
    private Camera trackCam;

    // Start is called before the first frame update
    void Start() {

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

    }

    // The natural scrolling
    void Scroll() {
        transform.position = trackCam.transform.position;
        transform.position -= transform.position.z * Vector3.forward;
    }
}

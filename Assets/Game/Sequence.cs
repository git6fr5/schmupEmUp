using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sequence : MonoBehaviour {

    public Sequencer[] sequence;
    public bool currSequenceIsNotTimed = false;
    public int index = -1;

    public int round;

    void Start() {
        NextSequence();
    }

    private void NextSequence() {

        index += 1;
        if (index >= sequence.Length) {
            index = 0;
            round += 1;
        }

        sequence[index].Reset(round);
        sequence[index].gameObject.SetActive(true);
        sequence[index].isStarted = false;
        sequence[index].isFinished = false;

        if (sequence[index].isTimed) {
            StartCoroutine(IEDeactivateSequence(sequence[index].duration));
            currSequenceIsNotTimed = false;
        }
        else {
            currSequenceIsNotTimed = true;
        }
        
    }

    void Update() {

        if (currSequenceIsNotTimed) {

            if (sequence[index].isFinished) {
                sequence[index].isStarted = false;
                sequence[index].gameObject.SetActive(false);
                NextSequence();
            }
            // Check for sequence to be completed.

        }

    }

    private IEnumerator IEDeactivateSequence(float delay) {
        yield return new WaitForSeconds(delay);
        sequence[index].gameObject.SetActive(false);
        NextSequence();
    }

}

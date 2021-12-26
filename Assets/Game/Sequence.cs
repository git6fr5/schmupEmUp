using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sequence : MonoBehaviour {

    public Sequencer[] sequence;
    public bool currSequenceIsNotTimed = false;
    public int index = 0;

    void Start() {

        NextSequence();

    }

    private void NextSequence() {
        sequence[index].Reset();
        sequence[index].gameObject.SetActive(true);
        if (sequence[index].isTimed) {
            StartCoroutine(IEDeactivateSequence(sequence[index].duration));
            currSequenceIsNotTimed = false;
        }
        else {
            currSequenceIsNotTimed = true;
        }
        index += 1;
        index = index % sequence.Length;
    }

    void Update() {

        if (currSequenceIsNotTimed) {

            if (sequence[index].isFinished) {
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

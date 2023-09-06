using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteractor : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "StartRecording")
        {
            Recorder.Main.StartRecording();
        }
        else if (other.gameObject.tag == "StopRecording")
        {
            Recorder.Main.StopRecording();
        }
    }
}

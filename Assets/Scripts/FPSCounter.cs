using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPSCounter : MonoBehaviour
{
    int fps;
    float time;

    void Update()
    {
        fps++;

        if (Time.time > time + 1)
        {
            print("FPS: " + fps);

            time++;
            fps = 0;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FPS : MonoBehaviour
{
    [SerializeField] Transform _camera;
    [SerializeField] Vector3 _offset = new Vector3(0, 0.75f, 1.25f);

    [SerializeField] TMP_Text _text;

    int _counter;
    float _time;

    void Update()
    {
        transform.position = _camera.TransformPoint(_offset);
        transform.rotation = _camera.rotation;

        _counter++;

        if (Time.time >= _time + 1f)
        {
            _text.text = "FPS: " + _counter;

            _counter = 0;
            _time++;
        }
    }
}

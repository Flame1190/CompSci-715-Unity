using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Recenter : MonoBehaviour
{
    bool _prevLeftIndex;
    bool _prevRightIndex;

    float _threshold = 0.75f;

    float _slowRotate = 30f;

    void Update()
    {
        bool leftIndex = OVRInput.Get(OVRInput.RawAxis1D.LIndexTrigger) > _threshold || Input.GetKey("1");
        bool rightIndex = OVRInput.Get(OVRInput.RawAxis1D.RIndexTrigger) > _threshold || Input.GetKey("2");

        bool leftGrip = OVRInput.Get(OVRInput.RawAxis1D.LHandTrigger) > _threshold || Input.GetKey("3");
        bool rightGrip = OVRInput.Get(OVRInput.RawAxis1D.RHandTrigger) > _threshold || Input.GetKey("4");

        if (leftIndex && !_prevLeftIndex) transform.Rotate(Vector3.up, -90);
        if (rightIndex && !_prevRightIndex) transform.Rotate(Vector3.up, 90);

        if (leftGrip) transform.Rotate(Vector3.up, -_slowRotate * Time.deltaTime);
        if (rightGrip) transform.Rotate(Vector3.up, _slowRotate * Time.deltaTime);

        _prevLeftIndex = leftIndex;
        _prevRightIndex = rightIndex;
    }
}

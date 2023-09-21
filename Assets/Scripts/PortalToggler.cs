using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalToggler : MonoBehaviour
{
    [SerializeField] Transform _player;

    [SerializeField] GameObject[] _portals;

    static Vector2 _otherOffsetScale = new Vector2(3, 6) * 1f;
    [SerializeField] Vector2 _otherRoomDirection;

    private void FixedUpdate()
    {
        bool currentState = _portals[0].activeInHierarchy;
        bool newState = (Mathf.Abs(transform.position.x - _player.position.x) < 1.5f && 
                         Mathf.Abs(transform.position.z - _player.position.z) < 3f) ||
                        (Mathf.Abs(transform.position.x + (_otherRoomDirection.x * _otherOffsetScale.x) - _player.position.x) < 1.5f &&
                         Mathf.Abs(transform.position.z + (_otherRoomDirection.y * _otherOffsetScale.y) - _player.position.z) < 3f);

        if (currentState != newState)
        {
            foreach (GameObject portal in _portals) portal.SetActive(newState);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomToggleManager : MonoBehaviour
{
    [SerializeField] Transform _player;

    [System.Serializable]
    [SerializeField] struct RoomInfo
    {
        [SerializeField] Transform _target;
        public Transform Target { get { return _target; } }
        [SerializeField] GameObject[] _visible;
        public GameObject[] Visible { get { return _visible; } }
    }

    [SerializeField] RoomInfo[] _roomInfos;

    int _prevIndex;

    private void Start()
    {
        foreach(RoomInfo roomInfo in _roomInfos)
        {
            roomInfo.Target.gameObject.SetActive(false);
        }

        foreach (GameObject room in _roomInfos[0].Visible)
        {
            room.SetActive(true);
        }
    }

    private void Update()
    {
        for (int i = 0; i < _roomInfos.Length; i++)
        {
            if (Mathf.Abs(_player.position.x - _roomInfos[i].Target.position.x) < 3 / 2f &&
                Mathf.Abs(_player.position.z - _roomInfos[i].Target.position.z) < 6 / 2f)
            {
                if (i != _prevIndex)
                {
                    Dictionary<GameObject, bool> toggle = new Dictionary<GameObject, bool>();

                    foreach(GameObject room in _roomInfos[_prevIndex].Visible)
                    {
                        toggle[room] = false;
                    }

                    foreach (GameObject room in _roomInfos[i].Visible)
                    {
                        toggle[room] = true;
                    }

                    foreach (GameObject room in toggle.Keys)
                    {
                        room.SetActive(toggle[room]);
                    }

                    _prevIndex = i;
                }

                break;
            }
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Recorder : MonoBehaviour
{
    public static Recorder Main;

    bool _recording;
    int _id = -1;
    float _time;
    bool _saved;

    LogData data = new LogData();

    [SerializeField] Transform _target;
    [SerializeField] GameObject _canvas;
    [SerializeField] TMP_Text _text;
    AudioSource _audioSource;
    [SerializeField] AudioClip _failedAudioClip;

    private void Awake()
    {
        Main = this;

        _audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (_recording)
        {
            _time += Time.deltaTime;

            data.AddMotionInfo(_time, _target.position, _target.rotation);
        }

        if (OVRInput.GetDown(OVRInput.RawButton.X) || Input.GetKeyDown(KeyCode.X))
        {
            ToggleCanvas();
        }

        if (_canvas.activeInHierarchy)
        {
            if (OVRInput.GetDown(OVRInput.RawButton.A) || Input.GetKeyDown(KeyCode.A))
            {
                AddToID(-1);
            }
            if (OVRInput.GetDown(OVRInput.RawButton.B) || Input.GetKeyDown(KeyCode.B))
            {
                AddToID(1);
            }
            if ((OVRInput.GetDown(OVRInput.RawButton.Y) || Input.GetKeyDown(KeyCode.Y)) && _time != 0 && !_saved)
            {
                SaveData(true);
            }

            _canvas.transform.position = _target.position + _target.forward;
            _canvas.transform.rotation = _target.rotation;
        }
    }

    void AddToID(int add)
    {
        if (!_saved && !_recording)
        {
            _id += add;

            _text.text = "ID: " + _id;
        }
        else
        {
            _text.text = "ID: " + _id + "\n(Failed change)";
        }
    }

    void ToggleCanvas()
    {
        _canvas.SetActive(!_canvas.activeInHierarchy);
    }

    public void StartRecording()
    {
        if (_time == 0)
        {
            _recording = true;

            _text.text = "Recording...";

            if (_id < 0)
            {
                _audioSource.PlayOneShot(_failedAudioClip);
            }
            else
            {
                _audioSource.Play();
            }
        }
    }

    public void StopRecording()
    {
        if (_recording)
        {
            _recording = false;

            SaveData();

            _audioSource.Play();
        }
    }

    void SaveData(bool forced = false)
    {
        if (_id != -1)
        {
            SaveAndLoad.Save(_id, data);
            _saved = true;
            _text.text = "Saved to ID: " + _id;

            if (forced)
            {
                _text.text += "\n(Forced)";
            }
        }
        else
        {
            _text.text = "Failed to Save";
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NuclearCell
{
    public enum DeskGameState
    {
        PlugSelection,
        PlugGameplay,
        PlugWaitForBrick, // If there are two brick hanging already 
    }

    public enum SessionState
    {
        Idle = 0,
        Allocated,
        TaskFinished,
        Plugged,
        Finishing
    }

    public class PhoneSession
    {
        public Phone Phone;
        public Plug Plug;
        public Brick Brick;
        public Socket Socket;
        public SessionState State;
    }

    public class GameManager : SingletonBehaviour<GameManager>
    {
        public Camera DeskCam;
        public Camera WallCam;

        public Transform PlugAnchor;
        public Transform SelectedPlugAnchor;

        public List<Socket> SocketMounts;

        public Transform[] BrickSlotPositions;
        public Transform[] PhonePositions;

        private bool[] _brickSlots;

        private LevelData _levelData;
        private GameObject[] _plugObjects;
        private Vector3[] _plugTargetPositions;
        private Quaternion[] _plugTargetRotations;
        private GameObject[] _phonePrefabs;

        private DeskGameState _deskGameState;
        private PhoneSession[] _taskSessions;

        private int _plugSelection = 0;

        public void Start()
        {
            _levelData = LevelData.FromResource("LevelDesc");
            _plugObjects = new GameObject[_levelData.Phones.Length];
            _phonePrefabs = new GameObject[_levelData.Phones.Length];
            _plugTargetPositions = new Vector3[_levelData.Phones.Length];
            _plugTargetRotations = new Quaternion[_levelData.Phones.Length];
            _taskSessions = new PhoneSession[PhonePositions.Length];
            for (var i = 0; i < PhonePositions.Length; i++)
            {
                _taskSessions[i] = new PhoneSession();
            }

            _brickSlots = new bool[BrickSlotPositions.Length];

            for (var i = 0; i < _levelData.Phones.Length; i++)
            {
                var phone = _levelData.Phones[i];
                _phonePrefabs[i] = Resources.Load<GameObject>("Prefabs/Phones/" + phone.Data);
                var plugPrefab = Resources.Load<GameObject>("Prefabs/Plugs/" + phone.Data);
                _plugObjects[i] = Instantiate(plugPrefab);
                _plugObjects[i].GetComponent<Rigidbody>().isKinematic = true;
            }
            UpdatePlugTargetPosition();
            SyncPlugToTarget();
        }

        public void Update()
        {
            if (_deskGameState == DeskGameState.PlugSelection)
            {
                if (Input.GetKeyDown(KeyCode.D))
                {
                    _plugSelection++;
                    if (_plugSelection < 0) _plugSelection = 0;
                    if (_plugSelection >= _plugObjects.Length) _plugSelection = _plugObjects.Length - 1;
                    UpdatePlugTargetPosition();
                }
                else if (Input.GetKeyDown(KeyCode.A))
                {
                    _plugSelection--;
                    if (_plugSelection < 0) _plugSelection = 0;
                    if (_plugSelection >= _plugObjects.Length) _plugSelection = _plugObjects.Length - 1;
                    UpdatePlugTargetPosition();
                }
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    ConfirmSelection();
                }
            }

            for (var i = 0; i < _plugObjects.Length; i++)
            {
                _plugObjects[i].transform.position = Vector3.Lerp(_plugObjects[i].transform.position, _plugTargetPositions[i], 15.0f * Time.deltaTime);
                _plugObjects[i].transform.rotation = Quaternion.Lerp(_plugObjects[i].transform.rotation, _plugTargetRotations[i], 15.0f * Time.deltaTime);
            }
        }

        public void ConfirmSelection()
        {
            var newPlug = Instantiate(_plugObjects[_plugSelection]);
            var nwePlugScript = newPlug.GetComponent<Plug>();
            nwePlugScript.PreStart = true;
            nwePlugScript.StartingTransform = PlugAnchor;
            newPlug.GetComponent<Rigidbody>().isKinematic = false;
            _plugObjects[_plugSelection].SetActive(false);
            _deskGameState = DeskGameState.PlugGameplay;
            UpdatePlugTargetPosition();
        }

        public void UpdatePlugTargetPosition()
        {
            if (_deskGameState == DeskGameState.PlugSelection)
            {
                for (var i = 0; i < _plugTargetPositions.Length; i++)
                {
                    if (i == _plugSelection)
                    {
                        _plugTargetPositions[i] = SelectedPlugAnchor.position;
                        _plugTargetRotations[i] = SelectedPlugAnchor.rotation;
                    }
                    else
                    {
                        var anchorPos = PlugAnchor.position;
                        anchorPos.x += (i - _plugSelection) * 1.0f;
                        _plugTargetPositions[i] = anchorPos;
                        _plugTargetRotations[i] = Quaternion.identity;
                    }
                }
            }
            else
            {
                for (var i = 0; i < _plugTargetPositions.Length; i++)
                {
                    var anchorPos = PlugAnchor.position;
                    anchorPos.x += (i - _plugSelection) * 1.0f;
                    anchorPos.z -= 3.0f;
                    _plugTargetPositions[i] = anchorPos;
                    _plugTargetRotations[i] = Quaternion.identity;
                }
            }
        }

        public void SyncPlugToTarget()
        {
            for (var i = 0; i < _plugObjects.Length; i++)
            {
                _plugObjects[i].transform.position = _plugTargetPositions[i];
                _plugObjects[i].transform.rotation = _plugTargetRotations[i];
            }
        }

        public void SessionStart()
        {
            for (var i = 0; i < _taskSessions.Length; i++)
            {
                var session = _taskSessions[i];
                if (session.State == SessionState.Idle)
                {
                    Debug.Log(i);
                    session.State = SessionState.Allocated;
                    session.Phone = AllocateRandomPhone();
                    session.Plug = null;
                    session.Socket = null;

                    session.Phone.transform.position = PhonePositions[i].position + new Vector3(0, 0, 5);
                    session.Phone.TargetPosition = PhonePositions[i].position;
                    break;
                }
            }
        }

        public Phone AllocateRandomPhone()
        {
            // TODO: consider level
            var prefab = _phonePrefabs[Random.Range(0, _phonePrefabs.Length)];
            var gameObj = Instantiate(prefab);
            return gameObj.GetComponent<Phone>();
        }

        public void PlugSuccess(int rating)
        {
            var occupied = 0;
            var spawned = false;
            for (var i = 0; i < _brickSlots.Length; i++)
            {
                if (!_brickSlots[i])
                {
                    if (!spawned)
                    {
                        spawned = true;
                        occupied++;
                        SpawnBrick(i);
                    }
                }
                else
                {
                    occupied++;
                }
            }

            if (occupied < _brickSlots.Length)
            {
                _deskGameState = DeskGameState.PlugSelection;
                for (var i = 0; i < _plugObjects.Length; i++)
                    _plugObjects[i].SetActive(true);
                SyncPlugToTarget();
                UpdatePlugTargetPosition();
            }
            else
            {
                _deskGameState = DeskGameState.PlugWaitForBrick;
            }
        }

        public void SpawnBrick(int slot)
        {
            _brickSlots[slot] = true;
            // TODO!: spawn brick fr
        }
    }
}
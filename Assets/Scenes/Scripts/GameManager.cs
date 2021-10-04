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
        PluggedFaultily,
        Charging,
        Exploding,
    }

    public class PhoneSession
    {
        public Phone Phone;
        public Plug Plug;
        public Brick Brick;
        public Socket Socket;
        public SessionState State;
        public float ChargeTime;
        public float TimeLimit;
    }

    public class GameManager : SingletonBehaviour<GameManager>
    {
        public Camera DeskCam;
        public Camera WallCam;

        public Transform PlugAnchor;
        public Transform SelectedPlugAnchor;

        public Transform[] BrickSlotPositions;
        public Transform[] PhonePositions;

        private bool[] _brickSlots;

        private LevelData _levelData;
        private GameObject[] _plugObjects;
        private Vector3[] _plugTargetPositions;
        private Quaternion[] _plugTargetRotations;

        private GameObject[] _phonePrefabs;
        private GameObject[] _brickPrefabs;

        private DeskGameState _deskGameState;
        private PhoneSession[] _phoneSessions;

        private int _plugSelection = 0;

        public LevelData Level { get => _levelData; }

        public void Start()
        {
            _levelData = LevelData.FromResource("LevelDesc");
            _plugObjects = new GameObject[_levelData.Phones.Length];
            _plugTargetPositions = new Vector3[_levelData.Phones.Length];
            _plugTargetRotations = new Quaternion[_levelData.Phones.Length];
            _phoneSessions = new PhoneSession[PhonePositions.Length];

            _phonePrefabs = new GameObject[_levelData.Phones.Length];
            _brickPrefabs = new GameObject[_levelData.Bricks.Length];

            for (var i = 0; i < PhonePositions.Length; i++)
            {
                _phoneSessions[i] = new PhoneSession();
            }

            for (var i = 0; i < _levelData.Bricks.Length; i++)
            {
                var brick = _levelData.Bricks[i];
                _brickPrefabs[i] = Resources.Load<GameObject>("Prefabs/Bricks/" + brick.Data);
            }

            SocketManager.Singleton.InitSockets();

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

            SessionStart();
            SessionStart();
            SessionStart();
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
            var newPlugScript = newPlug.GetComponent<Plug>();
            newPlugScript.PreStart = true;
            newPlugScript.StartingTransform = PlugAnchor;
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
            for (var i = 0; i < _phoneSessions.Length; i++)
            {
                var session = _phoneSessions[i];
                if (session.State == SessionState.Idle)
                {
                    Debug.Log(i);
                    session.State = SessionState.Allocated;
                    session.Phone = AllocateRandomPhone();
                    session.Plug = null;
                    session.Socket = null;

                    session.Phone.transform.position = PhonePositions[i].position + new Vector3(0, 0, 5);
                    session.Phone.TargetPosition = PhonePositions[i].position;
                    session.Phone.Session = i;
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

        public void StartPlugSelection()
        {
            _deskGameState = DeskGameState.PlugSelection;
            for (var i = 0; i < _plugObjects.Length; i++)
                _plugObjects[i].SetActive(true);
            SyncPlugToTarget();
            UpdatePlugTargetPosition();
        }

        public void PlugSuccess(Plug which, int session, int rating)
        {
            var occupied = 0;
            var spawned = false;

            _phoneSessions[session].Plug = which;
            if (_phoneSessions[session].State != SessionState.TaskFinished)
            {
                _phoneSessions[session].State = SessionState.PluggedFaultily;
            }
            else
            {
                _phoneSessions[session].State = SessionState.Plugged;
            }

            for (var i = 0; i < _brickSlots.Length; i++)
            {
                if (!_brickSlots[i])
                {
                    if (!spawned)
                    {
                        spawned = true;
                        occupied++;
                        SpawnBrick(i, session);
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

        public int RandomBrick()
        {
            // TODO!: consider level and weight
            return Random.Range(0, _levelData.Bricks.Length);
        }

        public BrickData RandomBrickData()
        {
            return _levelData.Bricks[RandomBrick()];
        }

        public void SpawnBrick(int slot, int session)
        {
            var brick = RandomBrick();
            _phoneSessions[session].Brick = Instantiate(_brickPrefabs[brick]).GetComponent<Brick>();
            _phoneSessions[session].Brick.Slot = slot;
            SocketManager.Singleton.RegisterBrick(_phoneSessions[session].Brick.Type);
            SocketManager.Singleton.SetupSocketForType(_phoneSessions[session].Brick.Type, 3);

            // TODO!: spawn animation for brick 
            _phoneSessions[session].Brick.TargetPos = BrickSlotPositions[slot].position;
            _phoneSessions[session].Brick.Session = session;
            _phoneSessions[session].Brick.transform.position = BrickSlotPositions[slot].position + new Vector3(0, -2.5f, 0);
            _phoneSessions[session].Brick.transform.rotation = BrickSlotPositions[slot].rotation;
            _brickSlots[slot] = true;
        }

        public void ReleaseBrickSlot(int slot)
        {
            _brickSlots[slot] = false;
            if (_deskGameState == DeskGameState.PlugWaitForBrick)
                StartPlugSelection();
        }

        public void BrickMounted(int session, int rating)
        {
            if (_phoneSessions[session].State == SessionState.PluggedFaultily)
            {
                _phoneSessions[session].State = SessionState.Exploding;
                Debug.Log("Explode");
            }
            else
            {
                _phoneSessions[session].State = SessionState.Charging;
                Debug.Log("Charging");
            }
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NuclearCell
{
    public class SocketManager : SingletonBehaviour<SocketManager>
    {
        public Transform[] SocketSlots;

        public Socket[] Sockets
        {
            get => _sockets;
        }

        private Socket[] _sockets;
        private bool[] _socketRequired;
        private bool[] _socketJustSpawned;
        private GameObject[] _socketPrefabs;
        private SocketData[] _socketData;

        private List<int> _tempArr;
        private Dictionary<int, int> _tempDict;
        private Dictionary<int, int> _numBrickTypes;

        public void Start()
        {
            _socketRequired = new bool[SocketSlots.Length];
            _socketJustSpawned = new bool[SocketSlots.Length];
            _sockets = new Socket[SocketSlots.Length];
            _tempArr = new List<int>();
            _tempDict = new Dictionary<int, int>();
            _numBrickTypes = new Dictionary<int, int>();
        }

        public void InitSockets()
        {
            _socketData = GameManager.Singleton.Level.Sockets;
            _socketPrefabs = new GameObject[_socketData.Length];
            for (var i = 0; i < _socketData.Length; i++)
            {
                var socket = _socketData[i];
                _socketPrefabs[i] = Resources.Load<GameObject>("Prefabs/Sockets/" + socket.Data);
            }
        }

        private bool IsSocketFree(int i)
        {
            if (_sockets[i] != null && _sockets[i].Occupied) return false;
            return !_socketJustSpawned[i] && (_sockets[i] == null || !_socketRequired[i]);
        }

        public void Release(int slot)
        {
            _sockets[slot] = null;
            _socketJustSpawned[slot] = false;
            CalculateRequirement();
        }

        public void RegisterBrick(int type)
        {
            if (_numBrickTypes.ContainsKey(type))
                _numBrickTypes[type] += 1;
            else
                _numBrickTypes[type] = 1;
        }

        public void UnregisterBrick(int type)
        {
            if (_numBrickTypes.ContainsKey(type))
                _numBrickTypes[type] -= 1;
            else
                _numBrickTypes[type] = 0;
        }

        public void CalculateRequirement()
        {
            _tempDict.Clear();
            for (var i = 0; i < _socketRequired.Length; i++)
            {
                _socketRequired[i] = false;
            }

            for (var i = 0; i < _sockets.Length; i++)
            {
                var socket = _sockets[i];
                if (socket != null)
                {
                    foreach (var conf in socket.Configs)
                    {
                        if (!_tempDict.ContainsKey(conf.Type))
                            _tempDict[conf.Type] = 1;
                        else
                            _tempDict[conf.Type] += 1;
                    }
                }
            }

            for (var i = 0; i < _sockets.Length; i++)
            {
                var socket = _sockets[i];
                if (socket != null)
                {
                    foreach (var conf in socket.Configs)
                    {
                        var bricksOfType = _numBrickTypes.ContainsKey(conf.Type) ? _numBrickTypes[conf.Type] : 0;
                        if (_tempDict[conf.Type] <= bricksOfType)
                            _socketRequired[i] = true;
                    }
                }
            }
            Debug.Log(_socketRequired);
        }

        public void SpawnSocketOfType(int type, int slot, bool skipRequirement = false)
        {
            if (_sockets[slot])
                _sockets[slot].Release();

            _tempArr.Clear();

            for (var i = 0; i < _socketData.Length; i++)
            {
                var types = _socketData[i].Types;
                var isThisType = false;
                for (var j = 0; j < types.Length; j++)
                {
                    if (types[j] == type)
                    {
                        isThisType = true;
                        break;
                    }
                }
                if (isThisType)
                    _tempArr.Add(i);
            }

            var random = _tempArr[Random.Range(0, _tempArr.Count)];
            var newSocket = Instantiate(_socketPrefabs[random]);
            newSocket.transform.position = SocketSlots[slot].position + new Vector3(0, 0, 0.5f);
            _sockets[slot] = newSocket.GetComponent<Socket>();
            _sockets[slot].TargetPos = SocketSlots[slot].position;
            _sockets[slot].Wait = 0.2f;
            _sockets[slot].Slot = slot;
            _socketJustSpawned[slot] = true;
            if (!skipRequirement)
                CalculateRequirement();
        }

        public void SpawnSocketOfTypeRandomSlot(int type, bool skipRequirement = false)
        {
            int slot;
            var tries = -1;
            do
            {
                slot = Random.Range(0, SocketSlots.Length);
                tries++;
                if (tries > 12)
                {
                    Debug.LogWarning("Oops! Shouldn't happen");
                    break;
                }
            } while (!IsSocketFree(slot));
            SpawnSocketOfType(type, slot, skipRequirement);
        }

        public void SetupSocketForType(int type, int numOfSockets)
        {
            var typeFulfilled = false;
            for (var i = 0; i < _sockets.Length; i++)
            {
                var socket = _sockets[i];
                if (socket == null || _socketRequired[i]) continue;

                foreach (var conf in socket.Configs)
                {
                    if (conf.Type == type)
                    {
                        // 50% chance to reuse
                        if (Random.Range(0, 1) < 50)
                        {
                            numOfSockets--;
                            typeFulfilled = true;
                        }
                    }
                }
            }

            if (!typeFulfilled)
            {
                numOfSockets--;
                SpawnSocketOfTypeRandomSlot(type, true);
            }

            // TODO: do random bricks
            while (numOfSockets > 0)
            {
                numOfSockets--;
                var brick = GameManager.Singleton.RandomBrickData();
                SpawnSocketOfTypeRandomSlot(brick.Type, true);
            }

            CalculateRequirement();
            for (var i = 0; i < _socketJustSpawned.Length; i++)
            {
                _socketJustSpawned[i] = false;
            }
        }
    }
}
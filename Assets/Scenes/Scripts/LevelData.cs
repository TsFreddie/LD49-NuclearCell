using System;
using UnityEngine;

namespace NuclearCell
{
    [Serializable]
    public struct PhoneData
    {
        public int Level;
        public float Weight;
        public string Data;
    }

    [Serializable]
    public struct BrickData
    {
        public int Level;
        public float Weight;
        public string Data;
        public int Type;
    }

    [Serializable]
    public struct SocketData
    {
        public int Level;
        public float Weight;
        public string Data;
        public int[] Types;
    }

    public class LevelData
    {
        public PhoneData[] Phones;
        public BrickData[] Bricks;
        public SocketData[] Sockets;

        public static LevelData FromResource(string jsonFile)
        {
            var res = Resources.Load<TextAsset>(jsonFile);
            return JsonUtility.FromJson<LevelData>(res.text);
        }
    }
}
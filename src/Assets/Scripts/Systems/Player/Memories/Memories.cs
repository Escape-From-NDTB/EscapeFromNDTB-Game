using System;
using System.Collections.Generic;
using NDTB.Data;
using UnityEngine;

namespace NDTB.Systems.Player.Memories
{
    public class Memories : MonoBehaviour
    {
        public List<Memory> MemoriesList = new List<Memory>();

        public event Action<Memory> OnMemoryAdded;
        public event Action<Memory> OnMemoryRemoved;

        public void AddMemory(Memory memory)
        {
            if (!MemoriesList.Contains(memory))
            {
                MemoriesList.Add(memory);
                OnMemoryAdded?.Invoke(memory);
            }
        }

        public void RemoveMemory(Memory memory)
        {
            if (MemoriesList.Contains(memory))
            {
                MemoriesList.Remove(memory);
                OnMemoryRemoved?.Invoke(memory);
            }
        }
    }
}

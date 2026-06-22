using System;
using System.Collections.Generic;
using NDTB.Data;
using UnityEngine;

namespace NDTB.Systems.Player.Memories
{
    public class Memories : MonoBehaviour
    {
        public List<SO_Memory> MemoriesList = new List<SO_Memory>();

        public event Action<SO_Memory> OnMemoryAdded;
        public event Action<SO_Memory> OnMemoryRemoved;

        public void AddMemory(SO_Memory memory)
        {
            if (!MemoriesList.Contains(memory))
            {
                MemoriesList.Add(memory);
                OnMemoryAdded?.Invoke(memory);
            }
        }

        public void RemoveMemory(SO_Memory memory)
        {
            if (MemoriesList.Contains(memory))
            {
                MemoriesList.Remove(memory);
                OnMemoryRemoved?.Invoke(memory);
            }
        }
    }
}

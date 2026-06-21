using System.Collections.Generic;
using UnityEngine;

namespace NDTB.UI.Clues
{
    public class CluesController : MonoBehaviour
    {
        [SerializeField] private Clue clue_Prefab;
        [SerializeField] private Transform parent;

        private readonly List<Clue> active_Clues = new List<Clue>();

        public Clue Create(string text, Sprite sprite)
        {
            Clue clueInstance = Instantiate(clue_Prefab, parent);
            clueInstance.Initialize(text, sprite);
            active_Clues.Add(clueInstance);
            return clueInstance;
        }

        public void Remove(Clue clueInstance)
        {
            if (clueInstance != null && active_Clues.Contains(clueInstance))
            {
                active_Clues.Remove(clueInstance);
                Destroy(clueInstance.gameObject);
            }
        }

        public void ClearAll()
        {
            foreach (Clue clue in active_Clues)
            {
                if (clue != null)
                {
                    Destroy(clue.gameObject);
                }
            }
            active_Clues.Clear();
        }
    }
}

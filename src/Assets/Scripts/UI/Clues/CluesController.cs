using System.Collections.Generic;
using UnityEngine;
namespace NDTB.UI.Clues
{
    public class CluesController : MonoBehaviour
    {

        [SerializeField] private readonly Clue _cluePrefab;

        [SerializeField] private Transform _parent;

        private readonly List<Clue> _activeClues = new List<Clue>();

        public Clue Create(string text, Sprite sprite)
        {
            Clue clueInstance = Instantiate(_cluePrefab, _parent);
            clueInstance.Initialize(text, sprite);
            _activeClues.Add(clueInstance);
            return clueInstance;
        }

        public void Remove(Clue clueInstance)
        {
            if (clueInstance != null && _activeClues.Contains(clueInstance))
            {
                _activeClues.Remove(clueInstance);
                Destroy(clueInstance.gameObject);
            }
        }

        public void ClearAll()
        {
            foreach (Clue clue in _activeClues)
            {
                if (clue != null)
                {
                    Destroy(clue.gameObject);
                }
            }
            _activeClues.Clear();
        }
    }
}

using UnityEngine;
using UnityEngine.UI;
namespace NDTB.UI.Clues
{
    public class Clue : MonoBehaviour
    {

        [SerializeField] private Image _clueButtonImage;

        [SerializeField] private RectTransform _clueButtonTransform;

        [SerializeField] private Text _text;

        public void Initialize(string _text, Sprite sprite)
        {
            float width = (float)(sprite.rect.width * (120 / sprite.rect.height));
            _clueButtonTransform.sizeDelta = new Vector2(width, _clueButtonTransform.sizeDelta.y);
            if (width > 120)
            {
                Vector2 pos = _clueButtonTransform.anchoredPosition;
                pos.x = 150 - width;
                _clueButtonTransform.anchoredPosition = pos;
            }
            else
            {
                Vector2 pos = _clueButtonTransform.anchoredPosition;
                pos.x = 30;
                _clueButtonTransform.anchoredPosition = pos;
            }
            _clueButtonImage.sprite = sprite;
            this._text.text = _text;
        }
    }
}

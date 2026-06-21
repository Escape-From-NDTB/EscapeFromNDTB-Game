using UnityEngine;
using UnityEngine.UI;

namespace NDTB.UI.Clues
{
    public class Clue : MonoBehaviour
    {
        [SerializeField] private Image clue_Button_Image;
        [SerializeField] private RectTransform clue_Button_Transform;
        [SerializeField] private Text text;

        public void Initialize(string text, Sprite sprite)
        {
            float width = (float)(sprite.rect.width * 7.5);
            clue_Button_Transform.sizeDelta = new Vector2(width, clue_Button_Transform.sizeDelta.y);
            if (width > 120)
            {
                Vector2 pos = clue_Button_Transform.anchoredPosition;
                pos.x = 150 - width;
                clue_Button_Transform.anchoredPosition = pos;
            }
            else
            {
                Vector2 pos = clue_Button_Transform.anchoredPosition;
                pos.x = 30;
                clue_Button_Transform.anchoredPosition = pos;
            }
            clue_Button_Image.sprite = sprite;
            this.text.text = text;
        }
    }
}

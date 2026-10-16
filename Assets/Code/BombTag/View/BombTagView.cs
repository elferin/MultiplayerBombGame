using TMPro;
using UnityEngine;

namespace Code.BombTag {

    public class BombTagView : MonoBehaviour {

        [SerializeField] private TMP_Text centerText;

        public void Show(BombTagViewData data) {
            centerText.text = data.CenterText;
        }
    }
}

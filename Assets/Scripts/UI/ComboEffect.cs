using UnityEngine;
using TMPro;
using DG.Tweening;

namespace BlockBlast.UI
{
    /// <summary>
    /// Hiệu ứng hiển thị combo
    /// </summary>
    public class ComboEffect : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI comboText;
        [SerializeField] private float showDuration = 1.5f;
        [SerializeField] private float scaleAmount = 1.5f;
        [SerializeField] private AnimationCurve scaleCurve;

        public void ShowCombo(int combo)
        {
            if (comboText != null)
            {
                comboText.text = $"Combo x{combo}!";
                AnimateCombo();
            }
        }

        private void AnimateCombo()
        {
            // Reset state
            transform.localScale = Vector3.zero;
            comboText.alpha = 1f;
            gameObject.SetActive(true);

            // Tạo sequence animation với DOTween
            Sequence sequence = DOTween.Sequence();
            
            // Scale up with punch
            sequence.Append(transform.DOScale(scaleAmount, showDuration * 0.3f).SetEase(Ease.OutBack));
            
            // Hold
            sequence.AppendInterval(showDuration * 0.4f);
            
            // Scale down và fade out đồng thời
            sequence.Append(transform.DOScale(0f, showDuration * 0.3f).SetEase(Ease.InBack));
            sequence.Join(comboText.DOFade(0f, showDuration * 0.3f));

            sequence.OnComplete(() => gameObject.SetActive(false));
        }

        private void OnDestroy()
        {
            DOTween.Kill(this);
        }
    }
}

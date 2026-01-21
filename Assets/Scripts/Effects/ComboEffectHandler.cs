using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;

namespace BlockBlast.Effects
{
    /// <summary>
    /// Xử lý hiệu ứng combo xuất hiện tại vị trí đặt block
    /// </summary>
    public class ComboEffectHandler : MonoBehaviour
    {
        [Header("Combo Sprites")]
        [SerializeField] private Sprite comboSprite;          // combo.png
        [SerializeField] private Sprite xSprite;              // x sprite
        
        [Header("Number Bitmap Sprites")]
        [SerializeField] private Sprite[] numberSprites = new Sprite[10]; // Sprites cho số 0-9
        
        [Header("Layout Settings")]
        [SerializeField] private float comboScale = 1f;       // Scale cho chữ "Combo"
        [SerializeField] private float xScale = 0.8f;         // Scale cho chữ "X"
        [SerializeField] private float numberScale = 0.7f;    // Scale cho số
        [SerializeField] private float spacing = 0.15f;       // Khoảng cách giữa các phần tử
        [SerializeField] private float cameraPadding = 0.5f;  // Padding từ mép camera
        
        [Header("Animation Settings")]
        [SerializeField] private float shootDuration = 0.3f;    // Thời gian "bắn" ra
        [SerializeField] private float floatDistance = 2f;      // Khoảng cách bay lên
        [SerializeField] private float floatDuration = 0.8f;    // Thời gian bay lên
        [SerializeField] private float fadeDuration = 0.4f;     // Thời gian mờ dần
        
        private List<GameObject> activeEffects = new List<GameObject>();
        private Camera mainCamera;
        
        private void Awake()
        {
            mainCamera = Camera.main;
        }
        
        /// <summary>
        /// Hiển thị hiệu ứng combo tại vị trí world
        /// </summary>
        public void ShowComboAt(Vector3 worldPosition, int comboCount)
        {
            Debug.Log($"[ComboEffectHandler] ShowComboAt called! Position: {worldPosition}, Count: {comboCount}");
            
            if (comboSprite == null)
            {
                Debug.LogWarning("[ComboEffectHandler] Combo sprite chưa được assign!");
                return;
            }
            
            // Tính tổng chiều rộng và chiều cao trước để clamp vị trí
            float totalWidth = CalculateTotalWidth(comboCount);
            float totalHeight = CalculateTotalHeight();
            
            // Clamp vị trí để nằm trong camera view
            Vector3 clampedPosition = ClampPositionToCamera(worldPosition, totalWidth, totalHeight);
            
            // Tạo container chính
            GameObject effectObj = new GameObject("ComboEffect");
            effectObj.transform.position = clampedPosition;
            
            Debug.Log($"[ComboEffectHandler] Created effectObj at position: {effectObj.transform.position}");
            
            List<SpriteRenderer> allRenderers = new List<SpriteRenderer>();
            
            // 1. Tạo sprite "Combo"
            GameObject comboObj = new GameObject("Combo");
            comboObj.transform.SetParent(effectObj.transform);
            SpriteRenderer comboRenderer = comboObj.AddComponent<SpriteRenderer>();
            comboRenderer.sprite = comboSprite;
            comboRenderer.sortingOrder = 100;
            comboObj.transform.localScale = Vector3.one * comboScale;
            allRenderers.Add(comboRenderer);
            
            // 2. Tạo sprite "X"
            GameObject xObj = null;
            SpriteRenderer xRenderer = null;
            if (xSprite != null)
            {
                xObj = new GameObject("X");
                xObj.transform.SetParent(effectObj.transform);
                xRenderer = xObj.AddComponent<SpriteRenderer>();
                xRenderer.sprite = xSprite;
                xRenderer.sortingOrder = 100;
                xObj.transform.localScale = Vector3.one * xScale;
                allRenderers.Add(xRenderer);
            }
            
            // 3. Tạo sprites cho số
            List<GameObject> digitObjects = new List<GameObject>();
            string numberStr = comboCount.ToString();
            for (int i = 0; i < numberStr.Length; i++)
            {
                int digit = int.Parse(numberStr[i].ToString());
                if (numberSprites != null && digit < numberSprites.Length && numberSprites[digit] != null)
                {
                    GameObject digitObj = new GameObject($"Digit_{digit}");
                    digitObj.transform.SetParent(effectObj.transform);
                    SpriteRenderer digitRenderer = digitObj.AddComponent<SpriteRenderer>();
                    digitRenderer.sprite = numberSprites[digit];
                    digitRenderer.sortingOrder = 100;
                    digitObj.transform.localScale = Vector3.one * numberScale;
                    digitObjects.Add(digitObj);
                    allRenderers.Add(digitRenderer);
                }
            }
            
            // 4. Tính chiều rộng từng phần để đặt vị trí
            float comboWidth = comboSprite.bounds.size.x * comboScale;
            float xWidth = xSprite != null ? xSprite.bounds.size.x * xScale : 0f;
            float numbersWidth = CalculateTotalNumbersWidth(digitObjects);
            
            // 5. Đặt vị trí từ trái sang phải
            float currentX = -totalWidth / 2f;
            
            // Đặt Combo
            comboObj.transform.localPosition = new Vector3(currentX + comboWidth / 2f, 0, 0);
            currentX += comboWidth + spacing;
            
            // Đặt X
            if (xObj != null)
            {
                xObj.transform.localPosition = new Vector3(currentX + xWidth / 2f, 0, 0);
                currentX += xWidth + spacing;
            }
            
            // Đặt các số
            for (int i = 0; i < digitObjects.Count; i++)
            {
                float digitWidth = digitObjects[i].GetComponent<SpriteRenderer>().sprite.bounds.size.x * numberScale;
                digitObjects[i].transform.localPosition = new Vector3(currentX + digitWidth / 2f, 0, 0);
                currentX += digitWidth;
            }
            
            activeEffects.Add(effectObj);
            
            // Chạy animation
            AnimateComboEffect(effectObj, allRenderers, 1f);
        }
        
        /// <summary>
        /// Tính tổng chiều rộng của combo effect
        /// </summary>
        private float CalculateTotalWidth(int comboCount)
        {
            float comboWidth = comboSprite.bounds.size.x * comboScale;
            float xWidth = xSprite != null ? xSprite.bounds.size.x * xScale : 0f;
            
            // Tính chiều rộng của các số
            float numbersWidth = 0f;
            string numberStr = comboCount.ToString();
            for (int i = 0; i < numberStr.Length; i++)
            {
                int digit = int.Parse(numberStr[i].ToString());
                if (numberSprites != null && digit < numberSprites.Length && numberSprites[digit] != null)
                {
                    numbersWidth += numberSprites[digit].bounds.size.x * numberScale;
                }
            }
            
            // Tổng = combo + spacing + x + spacing + numbers
            float totalWidth = comboWidth;
            if (xSprite != null) totalWidth += spacing + xWidth;
            if (numbersWidth > 0) totalWidth += spacing + numbersWidth;
            
            return totalWidth;
        }
        
        /// <summary>
        /// Tính chiều cao của combo effect
        /// </summary>
        private float CalculateTotalHeight()
        {
            // Lấy chiều cao lớn nhất trong các sprite
            float maxHeight = comboSprite.bounds.size.y * comboScale;
            
            if (xSprite != null)
            {
                float xHeight = xSprite.bounds.size.y * xScale;
                if (xHeight > maxHeight) maxHeight = xHeight;
            }
            
            if (numberSprites != null && numberSprites.Length > 0 && numberSprites[0] != null)
            {
                float numberHeight = numberSprites[0].bounds.size.y * numberScale;
                if (numberHeight > maxHeight) maxHeight = numberHeight;
            }
            
            return maxHeight;
        }
        
        /// <summary>
        /// Clamp vị trí để combo nằm hoàn toàn trong camera view
        /// </summary>
        private Vector3 ClampPositionToCamera(Vector3 position, float width, float height)
        {
            if (mainCamera == null) return position;
            
            // Lấy bounds của camera trong world space
            float cameraHeight = mainCamera.orthographicSize * 2f;
            float cameraWidth = cameraHeight * mainCamera.aspect;
            
            Vector3 cameraCenter = mainCamera.transform.position;
            
            // Tính giới hạn với padding an toàn
            // minX: vị trí tối thiểu (bên trái nhất) để cả combo vẫn nằm trong camera
            float minX = cameraCenter.x - (cameraWidth / 2f) + (width / 2f) + cameraPadding;
            // maxX: vị trí tối đa (bên phải nhất)
            float maxX = cameraCenter.x + (cameraWidth / 2f) - (width / 2f) - cameraPadding;
            // minY: vị trí tối thiểu (bên dưới nhất)
            float minY = cameraCenter.y - (cameraHeight / 2f) + (height / 2f) + cameraPadding;
            // maxY: vị trí tối đa (bên trên nhất) - cộng thêm để tính cả khoảng bay lên
            float maxY = cameraCenter.y + (cameraHeight / 2f) - (height / 2f) - cameraPadding - floatDistance;
            
            // Clamp vị trí
            Vector3 clampedPos = position;
            clampedPos.x = Mathf.Clamp(position.x, minX, maxX);
            clampedPos.y = Mathf.Clamp(position.y, minY, maxY);
            clampedPos.z = position.z;
            
            Debug.Log($"[ComboEffectHandler] Original pos: {position}, Clamped: {clampedPos}, Width: {width}, CameraWidth: {cameraWidth}, MinX: {minX}, MaxX: {maxX}");
            
            return clampedPos;
        }
        
        /// <summary>
        /// Tính tổng chiều rộng của tất cả các số
        /// </summary>
        private float CalculateTotalNumbersWidth(List<GameObject> digitObjects)
        {
            float total = 0f;
            foreach (var digitObj in digitObjects)
            {
                SpriteRenderer sr = digitObj.GetComponent<SpriteRenderer>();
                if (sr != null && sr.sprite != null)
                {
                    total += sr.sprite.bounds.size.x * numberScale;
                }
            }
            return total;
        }
        
        private void AnimateComboEffect(GameObject effectObj, List<SpriteRenderer> allRenderers, float targetScale)
        {
            // Bắt đầu từ scale 0
            effectObj.transform.localScale = Vector3.zero;
            
            // Tạo sequence
            Sequence sequence = DOTween.Sequence();
            
            // Phase 1: "Bắn" ra với hiệu ứng nổ (shoot out)
            float shootScale = 1.2f;
            sequence.Append(effectObj.transform.DOScale(shootScale, shootDuration)
                .SetEase(Ease.OutBack));
                
            // Phase 2: Bay lên
            Vector3 targetPos = effectObj.transform.position + Vector3.up * floatDistance;
            
            sequence.Append(effectObj.transform.DOMove(targetPos, floatDuration)
                .SetEase(Ease.OutQuad));
                
            // Scale giảm về 80% khi bay lên
            sequence.Join(effectObj.transform.DOScale(0.8f, floatDuration)
                .SetEase(Ease.InQuad));
            
            // Phase 3: Fade out tất cả sprites cùng lúc
            float fadeStartTime = floatDuration - fadeDuration;
            
            // Fade tất cả renderers
            foreach (var renderer in allRenderers)
            {
                if (renderer != null)
                {
                    sequence.Insert(fadeStartTime, renderer.DOFade(0f, fadeDuration));
                }
            }
            
            // Cleanup
            sequence.OnComplete(() =>
            {
                activeEffects.Remove(effectObj);
                Destroy(effectObj);
            });
        }
        
        private void OnDestroy()
        {
            // Cleanup tất cả effects đang active
            foreach (var effect in activeEffects)
            {
                if (effect != null)
                    Destroy(effect);
            }
            activeEffects.Clear();
            
            DOTween.Kill(this);
        }
    }
}

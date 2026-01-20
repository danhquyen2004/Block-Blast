using UnityEngine;

namespace BlockBlast.Utils
{
    /// <summary>
    /// Các hàm tiện ích chung
    /// </summary>
    public static class GameUtils
    {
        public static Color GetRandomColor()
        {
            return new Color(
                Random.Range(0.3f, 1f),
                Random.Range(0.3f, 1f),
                Random.Range(0.3f, 1f)
            );
        }

        public static void ShuffleArray<T>(T[] array)
        {
            for (int i = array.Length - 1; i > 0; i--)
            {
                int randomIndex = Random.Range(0, i + 1);
                T temp = array[i];
                array[i] = array[randomIndex];
                array[randomIndex] = temp;
            }
        }

        public static Vector2Int WorldToGrid(Vector3 worldPos, Vector3 gridOrigin, float cellSize)
        {
            Vector3 localPos = worldPos - gridOrigin;
            int x = Mathf.RoundToInt(localPos.x / cellSize);
            int y = Mathf.RoundToInt(localPos.y / cellSize);
            return new Vector2Int(x, y);
        }

        public static Vector3 GridToWorld(Vector2Int gridPos, Vector3 gridOrigin, float cellSize)
        {
            return gridOrigin + new Vector3(
                gridPos.x * cellSize,
                gridPos.y * cellSize,
                0
            );
        }
    }
}

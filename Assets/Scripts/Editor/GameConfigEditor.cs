using UnityEngine;
using UnityEditor;
using BlockBlast.Data;

namespace BlockBlast.Editor
{
    /// <summary>
    /// Custom Editor cho GameConfig để dễ dàng assign sprites
    /// </summary>
    [CustomEditor(typeof(GameConfig))]
    public class GameConfigEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            GameConfig config = (GameConfig)target;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Quick Setup", EditorStyles.boldLabel);

            if (GUILayout.Button("Auto-Assign Stone Sprites from Graphics Folder"))
            {
                AutoAssignStoneSprites(config);
            }

            if (GUILayout.Button("Auto-Assign UI Sprites from Graphics Folder"))
            {
                AutoAssignUISprites(config);
            }
        }

        private void AutoAssignStoneSprites(GameConfig config)
        {
            string[] stoneNames = new string[]
            {
                "blueStone",
                "redStone",
                "greenStone",
                "yellowStone",
                "orangeStone",
                "purpleStone",
                "pinkStone",
                "brownStone",
                "creamStone",
                "lightBlueStone"
            };

            Sprite[] sprites = new Sprite[stoneNames.Length];
            int foundCount = 0;

            for (int i = 0; i < stoneNames.Length; i++)
            {
                Sprite sprite = LoadSpriteFromGraphics(stoneNames[i]);
                if (sprite != null)
                {
                    sprites[i] = sprite;
                    foundCount++;
                }
            }

            if (foundCount > 0)
            {
                Undo.RecordObject(config, "Auto-Assign Stone Sprites");
                config.blockStoneSprites = sprites;
                EditorUtility.SetDirty(config);
                Debug.Log($"Assigned {foundCount} stone sprites to GameConfig");
            }
            else
            {
                Debug.LogWarning("No stone sprites found in Assets/Assets/Graphics folder");
            }
        }

        private void AutoAssignUISprites(GameConfig config)
        {
            Sprite cellBg = LoadSpriteFromGraphics("gameplay_cell_mid");
            Sprite boardBg = LoadSpriteFromGraphics("gameplay_board_ground_1");
            Sprite empty = LoadSpriteFromGraphics("gameplay_cell_mid");

            bool changed = false;

            if (cellBg != null)
            {
                Undo.RecordObject(config, "Auto-Assign Cell Background Sprite");
                config.cellBackgroundSprite = cellBg;
                changed = true;
            }

            if (boardBg != null)
            {
                Undo.RecordObject(config, "Auto-Assign Board Background Sprite");
                config.boardBackgroundSprite = boardBg;
                changed = true;
            }

            if (empty != null)
            {
                Undo.RecordObject(config, "Auto-Assign Empty Sprite");
                config.emptyCellSprite = empty;
                changed = true;
            }

            if (changed)
            {
                EditorUtility.SetDirty(config);
                Debug.Log("Assigned UI sprites to GameConfig");
            }
            else
            {
                Debug.LogWarning("No UI sprites found in Assets/Assets/Graphics folder");
            }
        }

        private Sprite LoadSpriteFromGraphics(string spriteName)
        {
            // Tìm trong thư mục Graphics
            string[] guids = AssetDatabase.FindAssets($"{spriteName} t:Sprite", new[] { "Assets/Assets/Graphics" });
            
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                return AssetDatabase.LoadAssetAtPath<Sprite>(path);
            }

            return null;
        }
    }
}

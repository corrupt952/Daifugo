using UnityEngine;
using UnityEditor;
using System.IO;
using Daifugo.Data;

namespace Daifugo.Editor
{
    /// <summary>
    /// Editor utility to generate all 52 CardSO assets
    /// </summary>
    public class CardSOGenerator : EditorWindow
    {
        private const string CARDS_BASE_PATH = "Assets/_Project/ScriptableObjects/Cards";
        private const string SPRITES_PATH = "Assets/_Project/Art/Cards/Large";

        [MenuItem("Daifugo/Generate All Card Assets")]
        public static void GenerateAllCards()
        {
            int createdCount = 0;
            int skippedCount = 0;

            // Generate cards for each suit
            string[] suits = { "Spades", "Hearts", "Diamonds", "Clubs" };
            string[] suitNames = { "spades", "hearts", "diamonds", "clubs" };

            for (int suitIndex = 0; suitIndex < suits.Length; suitIndex++)
            {
                string suit = suits[suitIndex];
                string suitName = suitNames[suitIndex];
                CardSO.Suit suitEnum = (CardSO.Suit)suitIndex;

                // Generate all 13 ranks
                for (int rank = 1; rank <= 13; rank++)
                {
                    string rankStr = GetRankString(rank);
                    string fileName = $"Card_{suit}_{rankStr}.asset";
                    string assetPath = $"{CARDS_BASE_PATH}/{suit}/{fileName}";

                    // Skip if already exists
                    if (File.Exists(assetPath))
                    {
                        Debug.Log($"[CardSOGenerator] Skipped (already exists): {fileName}");
                        skippedCount++;
                        continue;
                    }

                    // Load sprite
                    string spriteName = GetSpriteFileName(suitName, rank);
                    string spritePath = $"{SPRITES_PATH}/{spriteName}";
                    Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);

                    if (sprite == null)
                    {
                        Debug.LogError($"[CardSOGenerator] Sprite not found: {spritePath}");
                        continue;
                    }

                    // Create CardSO
                    CardSO card = ScriptableObject.CreateInstance<CardSO>();

                    // Use reflection to set private fields (SerializedObject approach)
                    SerializedObject serializedCard = new SerializedObject(card);
                    serializedCard.FindProperty("suit").enumValueIndex = (int)suitEnum;
                    serializedCard.FindProperty("rank").intValue = rank;
                    serializedCard.FindProperty("cardSprite").objectReferenceValue = sprite;
                    serializedCard.ApplyModifiedProperties();

                    // Save asset
                    AssetDatabase.CreateAsset(card, assetPath);
                    createdCount++;

                    Debug.Log($"[CardSOGenerator] Created: {fileName} (Suit={suitEnum}, Rank={rank})");
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[CardSOGenerator] Complete! Created: {createdCount}, Skipped: {skippedCount}");
            EditorUtility.DisplayDialog("Card Generation Complete",
                $"Created: {createdCount} cards\nSkipped: {skippedCount} cards (already exist)",
                "OK");
        }

        /// <summary>
        /// Gets rank string for asset file name (A, 2, 3, ..., J, Q, K)
        /// </summary>
        private static string GetRankString(int rank)
        {
            switch (rank)
            {
                case 1: return "A";
                case 11: return "J";
                case 12: return "Q";
                case 13: return "K";
                default: return rank.ToString();
            }
        }

        /// <summary>
        /// Gets sprite file name for Kenney asset (card_spades_A.png, card_clubs_02.png, etc.)
        /// </summary>
        private static string GetSpriteFileName(string suit, int rank)
        {
            string rankStr;
            switch (rank)
            {
                case 1: rankStr = "A"; break;
                case 11: rankStr = "J"; break;
                case 12: rankStr = "Q"; break;
                case 13: rankStr = "K"; break;
                default: rankStr = rank.ToString("D2"); break; // 02-10 format
            }

            return $"card_{suit}_{rankStr}.png";
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using Inner_Maps;
using Ruinarch;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;
using Random = System.Random;
namespace UtilityScripts {
    public static class GameUtilities {

        private static Stopwatch stopwatch = new Stopwatch();
        private static List<LocationGridTile> listHolder = new List<LocationGridTile>();

        public static string Filtered_Object_Layer = "Filtered Object";
        public static string Unfiltered_Object_Layer = "Non Filtered Object";
        public static string Filtered_Vision_Layer = "Filtered Vision";
        public static string Unfiltered_Vision_Layer = "Non Filtered Vision";
        public static int Line_Of_Sight_Layer_Mask = LayerMask.GetMask("Unpassable", Filtered_Object_Layer, Unfiltered_Object_Layer);
        public static int Unpassable_Layer_Mask = LayerMask.GetMask("Unpassable");
        public static int Filtered_Layer_Mask = LayerMask.GetMask(Filtered_Object_Layer);

        public static WaitForSeconds waitFor1Second = new WaitForSeconds(1f);
        public static WaitForSeconds waitFor2Seconds = new WaitForSeconds(2f);
        public static WaitForSeconds waitFor3Seconds = new WaitForSeconds(3f);
        public static WaitForSeconds waitFor5Seconds = new WaitForSeconds(5f);
        
        private static readonly Color _grayedOutColor = new Color(106f / 255f, 106f / 255f, 128f / 255f);
        private static readonly Color _normalColor = new Color(248f / 255f, 225f / 255f, 169f / 255f);

        public static BIOMES[] customWorldBiomeChoices = new[] {BIOMES.GRASSLAND, BIOMES.FOREST, BIOMES.DESERT, BIOMES.SNOW};
        public static FACTION_TYPE[] customWorldFactionTypeChoices = new[] {FACTION_TYPE.Human_Empire, FACTION_TYPE.Elven_Kingdom, FACTION_TYPE.Demon_Cult, FACTION_TYPE.Vampire_Clan, FACTION_TYPE.Lycan_Clan};
        public static TILE_OBJECT_TYPE[] corruptionTileObjectChoices = new[]
            {TILE_OBJECT_TYPE.CORRUPTED_TENDRIL, TILE_OBJECT_TYPE.CORRUPTED_SPIKE, TILE_OBJECT_TYPE.DEMON_CIRCLE, TILE_OBJECT_TYPE.SPAWNING_PIT, TILE_OBJECT_TYPE.SIGIL, TILE_OBJECT_TYPE.SMALL_TREE_OBJECT};
        public static List<STRUCTURE_TYPE> skinnerStructures = new List<STRUCTURE_TYPE>() {
            STRUCTURE_TYPE.BOAR_DEN,
            STRUCTURE_TYPE.WOLF_DEN,
            STRUCTURE_TYPE.BEAR_DEN,
            STRUCTURE_TYPE.RABBIT_HOLE,
            STRUCTURE_TYPE.MINK_HOLE,
            STRUCTURE_TYPE.MOONCRAWLER_HOLE,
        };

        public static string GetNormalizedSingularRace(RACE race) {
            switch (race) {
                case RACE.HUMANS:
                    return "Human";
                case RACE.ELVES:
                    return "Elf";
                case RACE.MINGONS:
                    return "Mingon";
                case RACE.CROMADS:
                    return "Cromad";
                case RACE.GOBLIN:
                    return "Goblin";
                case RACE.TROLL:
                    return "Troll";
                case RACE.DRAGON:
                    return "Dragon";
                default:
                    return Utilities.NormalizeStringUpperCaseFirstLetterOnly(race.ToString());
            }
        }
        public static string GetNormalizedRaceAdjective(string race) {
            race = race.ToUpper();
            switch (race) {
                case "HUMANS":
                    return "Human";
                case "ELVES":
                    return "Elven";
                case "MINGONS":
                    return "Mingon";
                case "CROMADS":
                    return "Cromad";
                case "GOBLIN":
                    return "Goblin";
                case "TROLL":
                    return "Troll";
                case "DRAGON":
                    return "Dragon";
                default:
                    return Utilities.NormalizeStringUpperCaseFirstLetterOnly(race);
            }
        }
        public static string GetNormalizedRaceAdjective(RACE race) {
            switch (race) {
                case RACE.HUMANS:
                    return "Human";
                case RACE.ELVES:
                    return "Elven";
                case RACE.MINGONS:
                    return "Mingon";
                case RACE.CROMADS:
                    return "Cromad";
                case RACE.GOBLIN:
                    return "Goblin";
                case RACE.TROLL:
                    return "Troll";
                case RACE.DRAGON:
                    return "Dragon";
                default:
                    return Utilities.NormalizeStringUpperCaseFirstLetterOnly(race.ToString());
            }
        }
        public static Rect GetScreenRect(Vector3 screenPosition1, Vector3 screenPosition2) {
            // Move origin from bottom left to top left
            screenPosition1.y = Screen.height - screenPosition1.y;
            screenPosition2.y = Screen.height - screenPosition2.y;
            // Calculate corners
            var topLeft = Vector3.Min(screenPosition1, screenPosition2);
            var bottomRight = Vector3.Max(screenPosition1, screenPosition2);
            // Create Rect
            return Rect.MinMaxRect(topLeft.x, topLeft.y, bottomRight.x, bottomRight.y);
        }
        public static Bounds GetViewportBounds(Camera camera, Vector3 screenPosition1, Vector3 screenPosition2) {
            var v1 = Camera.main.ScreenToViewportPoint(screenPosition1);
            var v2 = Camera.main.ScreenToViewportPoint(screenPosition2);
            var min = Vector3.Min(v1, v2);
            var max = Vector3.Max(v1, v2);
            min.z = camera.nearClipPlane;
            max.z = camera.farClipPlane;

            var bounds = new Bounds();
            bounds.SetMinMax(min, max);
            return bounds;
        }
        public static bool IsVisibleFrom(Renderer renderer, Camera camera) {
            Plane[] planes = GeometryUtility.CalculateFrustumPlanes(camera);
            return GeometryUtility.TestPlanesAABB(planes, renderer.bounds);
        }
        public static int GetTicksInBetweenDates(GameDate date1, GameDate date2) {
            int yearDiff = Mathf.Abs(date1.year - date2.year);
            int monthDiff = Mathf.Abs(date1.month - date2.month);
            int daysDiff = Mathf.Abs(date1.day - date2.day);
            int ticksDiff = date2.tick - date1.tick;

            int totalTickDiff = yearDiff * ((GameManager.ticksPerDay * GameManager.daysPerMonth) * 12);
            totalTickDiff += monthDiff * (GameManager.ticksPerDay * GameManager.daysPerMonth);
            totalTickDiff += daysDiff * GameManager.ticksPerDay;
            totalTickDiff += ticksDiff;
        
            return totalTickDiff;
        }
        public static LocationGridTile GetCenterTile(List<LocationGridTile> tiles, LocationGridTile[,] map) {
            int minX = tiles.Min(t => t.localPlace.x);
            int maxX = tiles.Max(t => t.localPlace.x);
            int minY = tiles.Min(t => t.localPlace.y);
            int maxY = tiles.Max(t => t.localPlace.y);

            int differenceX = maxX - minX;
            int differenceY = maxY - minY;

            int centerX = minX + (differenceX / 2);
            int centerY = minY + (differenceY / 2);

            LocationGridTile centerTile = map[centerX, centerY]; 
        
            Assert.IsTrue(tiles.Contains(centerTile),
                $"Computed center is not in provided list. Center was {centerTile.ToString()}. Min X is {minX.ToString()}. Max X is {maxX.ToString()}. Min Y is {minY.ToString()}. Max Y is {maxY.ToString()}.");

            return centerTile;

        }
        public static string GetRespectiveBeastClassNameFromByRace(RACE race) {
            if(race == RACE.GOLEM) {
                return "Abomination";
            } else if(race == RACE.DRAGON) {
                return "Dragon";
            } else if (race == RACE.SPIDER) {
                return "Spinner";
            } else if (race == RACE.WOLF) {
                return "Ravager";
            }
            throw new Exception($"No beast class for {race} Race!");
        }
        private static readonly HashSet<RACE> _beastRaces = new HashSet<RACE>() {
            RACE.DRAGON,
            RACE.WOLF,
            //RACE.BEAST,
            RACE.SPIDER,
            RACE.GOLEM,
            RACE.SHEEP,
            RACE.PIG,
            RACE.CHICKEN,
            RACE.BEAST,
            RACE.RAT,
        };
        public static bool IsRaceBeast(RACE race) {
            return _beastRaces.Contains(race);
        }
        public static T[] GetComponentsInDirectChildren<T>(GameObject gameObject) {
            int indexer = 0;

            foreach (Transform transform in gameObject.transform) {
                if (transform.GetComponent<T>() != null) {
                    indexer++;
                }
            }

            T[] returnArray = new T[indexer];

            indexer = 0;

            foreach (Transform transform in gameObject.transform) {
                if (transform.GetComponent<T>() != null) {
                    returnArray[indexer++] = transform.GetComponent<T>();
                }
            }

            return returnArray;
        }

        public static int GetOptionIndex(Dropdown dropdown, string option) {
            for (int i = 0; i < dropdown.options.Count; i++) {
                if (dropdown.options[i].text.Equals(option)) {
                    return i;
                }
            }
            return -1;
        }
        public static int GetOptionIndex(TMP_Dropdown dropdown, string option) {
            for (int i = 0; i < dropdown.options.Count; i++) {
                if (dropdown.options[i].text.Equals(option)) {
                    return i;
                }
            }
            return -1;
        }
        public static GameObject FindParentWithTag(GameObject childObject, string tag) {
            Transform t = childObject.transform;
            while (t.parent != null) {
                if (t.parent.tag == tag) {
                    return t.parent.gameObject;
                }
                t = t.parent.transform;
            }
            return null; // Could not find a parent with given tag.
        }

        /// <summary>
        /// Get diamond tiles given a center and a radius.
        /// </summary>
        /// <param name="map">The inner map that the tile belongs to.</param>
        /// <param name="center">The center tile.</param>
        /// <param name="radius">Radius of diamond. NOTE this includes the center tile.</param>
        /// <returns>List of tiles included in diamond.</returns>
        public static List<LocationGridTile> GetDiamondTilesFromRadius(InnerTileMap map, Vector3Int center, int radius) {
            listHolder.Clear();
            int lowerBoundY = center.y - radius;
            int upperBoundY = center.y + radius;
            
            //from center to upwards
            int radiusModifier = 0;
            for (int y = center.y; y <= upperBoundY; y++) {
                int lowerBoundX = (center.x - radius) + radiusModifier;
                int upperBoundX = (center.x + radius) - radiusModifier;    
                for (int x = lowerBoundX; x <= upperBoundX; x++) {
                    if (Utilities.IsInRange(x, 0, map.width) 
                        && Utilities.IsInRange(y, 0, map.height)) {
                        LocationGridTile tile = map.map[x, y];
                        listHolder.Add(tile);
                    }
                }
                radiusModifier++;
            }
                    
            //from center downwards
            radiusModifier = 1;
            //-1 because center row tiles were already added above
            for (int y = center.y - 1; y >= lowerBoundY; y--) {
                int lowerBoundX = (center.x - radius) + radiusModifier;
                int upperBoundX = (center.x + radius) - radiusModifier;    
                for (int x = lowerBoundX; x <= upperBoundX; x++) {
                    if (Utilities.IsInRange(x, 0, map.width) 
                        && Utilities.IsInRange(y, 0, map.height)) {
                        LocationGridTile tile = map.map[x, y];
                        listHolder.Add(tile);
                    }
                }
                radiusModifier++;
            }
            return listHolder;
        }
        public static void HighlightTiles(List<LocationGridTile> tiles, Color color) {
            for (int i = 0; i < tiles.Count; i++) {
                LocationGridTile tile = tiles[i];
                tile.parentMap.groundTilemap.SetColor(tile.localPlace, color);
            }
        }
        public static Color InvertColor (Color color) {
            return new Color (1.0f-color.r, 1.0f-color.g, 1.0f-color.b);
        }
        public static Vector2 VectorSubtraction(Vector2 a, Vector2 b) {
            Vector3 result = a;
            result.x = a.x - b.x;
            result.y = a.y - b.y;
            return result;
        }
        public static int Roll() {
            int roll = UnityEngine.Random.Range(0, 100);
            return roll;
        }
        /// <summary>
        /// Roll a chance. This rolls from 0 - 100.
        /// </summary>
        /// <param name="chance">The chance for this to return true.</param>
        /// <param name="log">The log string to append this roll to.</param>
        /// <returns>Whether or not the given chance was met.</returns>
        public static bool RollChance(int chance, ref string log) {
            int roll = UnityEngine.Random.Range(0, 100);
#if DEBUG_LOG
            log += $"\nRoll is {roll.ToString()}. Chance is {chance.ToString()}";
#endif
            return roll < chance;
        }
        /// <summary>
        /// Roll a chance. This rolls from 0 - 100.
        /// </summary>
        /// <param name="chance">The chance for this to return true.</param>
        /// <returns>Whether or not the given chance was met.</returns>
        public static bool RollChance(int chance) {
            int roll = UnityEngine.Random.Range(0, 100);
            return roll < chance;
        }
        /// <summary>
        /// Roll a chance. This rolls from 0f - 1000f.
        /// </summary>
        /// <param name="chance">The chance for this to return true.</param>
        /// <returns>Whether or not the given chance was met.</returns>
        public static bool RollChance(float chance) {
            chance *= 100f;
            int roll = UnityEngine.Random.Range(0, 10000);
            return roll < chance;
        }
        /// <summary>
        /// Roll a chance. This rolls from 0f - 1000f.
        /// </summary>
        /// <param name="chance">The chance for this to return true.</param>
        /// <param name="log">The log string to append this roll to.</param>
        /// <returns>Whether or not the given chance was met.</returns>
        public static bool RollChance(float chance, ref string log) {
            chance *= 100f;
            int roll = UnityEngine.Random.Range(0, 10000);
            log += $"\nRoll is {roll.ToString()}. Chance is {chance.ToString()}";
            return roll < chance;
        }
        public static int RandomBetweenTwoNumbers(int p_min, int p_max) {
            //+1 because max range in Random.Range is exclusive
            int roll = UnityEngine.Random.Range(p_min, p_max + 1);
            return roll;
        }

        public static List<int> GetUniqueRandomNumbersInBetween(int p_min, int p_max, int p_count) {
            var sequence = Enumerable.Range(p_min, p_max).OrderBy(n => n * n + UnityEngine.Random.Range(p_min, p_max) * (new System.Random()).Next());

            var result = sequence.Distinct().Take(p_count);

            return result.ToList<int>();
        }
        public static List<Area> GetHexTilesGivenCoordinates(List<Point> coordinates, Area[,] map) {
            List<Area> tiles = new List<Area>();
            for (int i = 0; i < coordinates.Count; i++) {
                Point point = coordinates[i];
                Area tile = map[point.X, point.Y];
                tiles.Add(tile);
            }
            return tiles;
        }
        public static List<Area> GetHexTilesGivenCoordinates(Point[] coordinates, Area[,] map) {
            List<Area> tiles = new List<Area>();
            for (int i = 0; i < coordinates.Length; i++) {
                Point point = coordinates[i];
                Area tile = map[point.X, point.Y];
                tiles.Add(tile);
            }
            return tiles;
        }
        public static Area GetHexTileGivenCoordinates(Point point, Area[,] map) {
            Area tile = map[point.X, point.Y];
            return tile;
        }
        public static Color GetUpgradeButtonTextColor(bool p_interactable) {
            return p_interactable ? _normalColor : _grayedOutColor;
        }
        private static List<int> cornersOutside = new List<int>();
        private static Vector3[] corners = new Vector3[4];
        public static void PositionTooltip(Vector3 position, GameObject tooltipParent, RectTransform rtToReposition, RectTransform boundsRT, InputManager.Cursor_Type cursorType, RectTransform canvasRT) {
            var v3 = position;

            rtToReposition.pivot = new Vector2(0f, 1f);

            if (cursorType == InputManager.Cursor_Type.Cross || cursorType == InputManager.Cursor_Type.Check || cursorType == InputManager.Cursor_Type.Link) {
                v3.x += 100f;
                v3.y -= 32f;
            } else {
                v3.x += 25f;
                v3.y -= 25f;
            }
            
            rtToReposition.transform.position = v3;

            // if (rtToReposition.sizeDelta.y >= Screen.height) {
            //     return;
            // }

            // float currentMaxXPos = rtToReposition.anchoredPosition.x + rtToReposition.sizeDelta.x;
            // float currentMaxYPos = rtToReposition.anchoredPosition.y - rtToReposition.sizeDelta.y;
            //
            // if (currentMaxXPos > Screen.width || rtToReposition.anchoredPosition.x < 0) {
            //     float xPos = Mathf.Clamp(currentMaxXPos, 0, Screen.width - rtToReposition.sizeDelta.x);
            //     Vector3 newPos = new Vector3(xPos, rtToReposition.anchoredPosition.y);
            //     rtToReposition.anchoredPosition = newPos;
            // }
            // if (currentMaxYPos < -Screen.height) {
            //     float yPos = Mathf.Clamp(currentMaxYPos, -Screen.height + rtToReposition.sizeDelta.y, 0);
            //     Vector3 newPos = new Vector3(rtToReposition.anchoredPosition.x, yPos);
            //     rtToReposition.anchoredPosition = newPos;
            // }
            
            cornersOutside.Clear();
            boundsRT.GetWorldCorners(corners);
            for (int i = 0; i < 4; i++) {
                Vector3 corner = corners[i];
                Vector3 localSpacePoint = canvasRT.InverseTransformPoint(corner);
                // If parent (canvas) does not contain checked items any point
                if (!canvasRT.rect.Contains(localSpacePoint)) {
                    cornersOutside.Add(i);
                }
            }
            
            if (cornersOutside.Count != 0) {
                if (cornersOutside.Contains(2) && cornersOutside.Contains(3)) {
                    if (cornersOutside.Contains(0)) {
                        //bottom side and right side are outside, move anchor to bottom right
                        rtToReposition.pivot = new Vector2(1f, 0f);
                    } else {
                        //right side is outside, move anchor to top right side
                        rtToReposition.pivot = new Vector2(1f, 1f);
                    }
                } else if (cornersOutside.Contains(0) && cornersOutside.Contains(3)) {
                    //bottom side is outside, move anchor to bottom left
                    rtToReposition.pivot = new Vector2(0f, 0f);
                }    
            }
        }

        public static bool IsRectFullyInCanvas(RectTransform boundsRT, RectTransform canvasRT) {
            cornersOutside.Clear();
            boundsRT.GetWorldCorners(corners);
            for (int i = 0; i < 4; i++) {
                Vector3 corner = corners[i];
                Vector3 localSpacePoint = canvasRT.InverseTransformPoint(corner);
                // If parent (canvas) does not contain checked items any point
                if (!canvasRT.rect.Contains(localSpacePoint)) {
                    cornersOutside.Add(i);
                }
            }
            
            if (cornersOutside.Count != 0) {
                if (cornersOutside.Contains(2) && cornersOutside.Contains(3)) {
                    if (cornersOutside.Contains(0)) {
                        //bottom side and right side are outside, move anchor to bottom right
                        return false;
                    } else {
                        //right side is outside, move anchor to top right side
                        return false;
                    }
                } else if (cornersOutside.Contains(0) && cornersOutside.Contains(3)) {
                    //bottom side is outside, move anchor to bottom left
                    return false;
                }    
            }
            return true;
        }
        public static bool IsRectFullyInCanvas(RectTransform boundsRT, Rect canvasRT) {
            cornersOutside.Clear();
            boundsRT.GetWorldCorners(corners);
            for (int i = 0; i < 4; i++) {
                Vector3 corner = corners[i];
                // If parent (canvas) does not contain checked items any point
                if (!canvasRT.Contains(corner)) {
                    cornersOutside.Add(i);
                }
            }

            return cornersOutside.Count == 0;
        }
        public static Color GetValidTileHighlightColor() {
            Color color = Color.green;
            color.a = 0.3f;
            return color;
        }
        public static Color GetInvalidTileHighlightColor() {
            Color color = Color.red;
            color.a = 0.3f;
            return color;
        }
    }    
}


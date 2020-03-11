using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using Inner_Maps;
using Traits;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;
namespace UtilityScripts {
    public static class GameUtilities {

        private static Stopwatch stopwatch = new Stopwatch();
        private static List<LocationGridTile> listHolder = new List<LocationGridTile>();
        
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
        public static HexTile GetCenterTile(List<HexTile> tiles, HexTile[,] map, int width, int height) {
            int maxXCoordinate = tiles.Max(x => x.xCoordinate);
            int minXCoordinate = tiles.Min(x => x.xCoordinate);
            int maxYCoordinate = tiles.Max(x => x.yCoordinate);
            int minYCoordinate = tiles.Min(x => x.yCoordinate);

            int midPointX = (minXCoordinate + maxXCoordinate) / 2;
            int midPointY = (minYCoordinate + maxYCoordinate) / 2;

            if (width - 2 >= midPointX) {
                midPointX -= 2;
            }
            if (height - 2 >= midPointY) {
                midPointY -= 2;
            }
            if (midPointX >= 2) {
                midPointX += 2;
            }
            if (midPointY >= 2) {
                midPointY += 2;
            }
            midPointX = Mathf.Clamp(midPointX, 0, width - 1);
            midPointY = Mathf.Clamp(midPointY, 0, height - 1);

            try {
                HexTile newCenterOfMass = map[midPointX, midPointY];
                return newCenterOfMass;
            } catch {
                throw new Exception($"Cannot Recompute center. Computed new center is {midPointX}, {midPointY}");
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
        public static List<HexTile> GetTilesFromIDs(List<int> ids) {
            List<HexTile> tiles = new List<HexTile>();
            for (int i = 0; i < ids.Count; i++) {
                int currID = ids[i];
                HexTile tile = GridMap.Instance.GetHexTile(currID);
                tiles.Add(tile);
            }
            return tiles;
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
        public static List<ITraitable> GetTraitableDiamondTilesFromRadius(InnerTileMap map, Vector3Int center, int radius) {
            List<ITraitable> traitables = new List<ITraitable>();
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
                        traitables.AddRange(tile.GetTraitablesOnTile());
                    }
                }
                radiusModifier++;
            }
                    
            //from center downwards
            radiusModifier = 0;
            //-1 because center row tiles were already added above
            for (int y = center.y - 1; y >= lowerBoundY; y--) {
                int lowerBoundX = (center.x - radius) + radiusModifier;
                int upperBoundX = (center.x + radius) - radiusModifier;    
                for (int x = lowerBoundX; x <= upperBoundX; x++) {
                    if (Utilities.IsInRange(x, 0, map.width) 
                        && Utilities.IsInRange(y, 0, map.height)) {
                        LocationGridTile tile = map.map[x, y];
                        traitables.AddRange(tile.GetTraitablesOnTile());
                    }
                }
                radiusModifier++;
            }
            return traitables;
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
    }    
}


using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UtilityScripts;

public class WorldMapRegionGeneration : MapGenerationComponent {

	private List<WorldMapTemplate> worldMapTemplates = new List<WorldMapTemplate>() {
		new WorldMapTemplate() {
			regions = new Dictionary<int, RegionTemplate[]>() {
				{0, new[] {
						new RegionTemplate(5, 6),
						new RegionTemplate(5, 6),
					}
				},
				{1, new[] {
						new RegionTemplate(3, 6),
						new RegionTemplate(4, 6),
						new RegionTemplate(3, 6),
					}
				}
			}
		}, 
		new WorldMapTemplate() {
			regions = new Dictionary<int, RegionTemplate[]>() {
				{0, new[] {
						new RegionTemplate(3, 6),
						new RegionTemplate(4, 6),
						new RegionTemplate(3, 6),
					}
				},
				{1, new[] {
						new RegionTemplate(5, 6),
						new RegionTemplate(5, 6),
					}
				}
			}
		}, 
		new WorldMapTemplate() {
			regions = new Dictionary<int, RegionTemplate[]>() {
				{0, new[] {
						new RegionTemplate(6, 6),
						new RegionTemplate(4, 6),
					}
				},
				{1, new[] {
						new RegionTemplate(3, 6),
						new RegionTemplate(3, 6),
						new RegionTemplate(4, 6),
					}
				}
			}
		}
	};
	
	public override IEnumerator Execute(MapGenerationData data) {
		WorldMapTemplate chosenTemplate;
		if (WorldConfigManager.Instance.isDemoWorld) {
			chosenTemplate = new WorldMapTemplate() {
				regions = new Dictionary<int, RegionTemplate[]>() {
					{0, new [] {
						new RegionTemplate(8, 10), 
						}
					}	
				}
			};
		} else {
			chosenTemplate = CollectionUtilities.GetRandomElement(worldMapTemplates);
		}
		yield return MapGenerator.Instance.StartCoroutine(DivideToRegions(chosenTemplate, data));
	}
	private IEnumerator DivideToRegions(WorldMapTemplate mapTemplate, MapGenerationData data) {
		int lastX = 0;
		int lastY = 0;
		Region[] allRegions = new Region[data.regionCount];
		int regionIndex = 0;
		foreach (var mapTemplateRegion in mapTemplate.regions) {
			for (int i = 0; i < mapTemplateRegion.Value.Length; i++) {
				RegionTemplate regionTemplate = mapTemplateRegion.Value[i];
				Region region = CreateNewRegionFromTemplate(regionTemplate, lastX, lastY);

				lastX += regionTemplate.width;
				if (lastX == GridMap.Instance.width) {
					lastX = 0;
					lastY += regionTemplate.height;
				}
				allRegions[regionIndex] = region;
				regionIndex++;

			}
		}
		GridMap.Instance.SetRegions(allRegions);
		string summary = "Region Generation Summary: ";
		for (int i = 0; i < allRegions.Length; i++) {
			Region region = allRegions[i];
			summary += $"\n{region.name} - {region.tiles.Count.ToString()}";
		}
		Debug.Log(summary);
		
		yield return null;
	}

	private Region CreateNewRegionFromTemplate(RegionTemplate template, int startingX, int startingY) {
		int maxX = startingX + template.width;
		int maxY = startingY + template.height;
		
		int centerX = startingX + (template.width / 2);
		int centerY = startingY + (template.height / 2);
		HexTile center = GridMap.Instance.map[centerX, centerY];
		
		Region region = new Region(center);
		for (int x = startingX; x < maxX; x++) {
			for (int y = startingY; y < maxY; y++) {
				try {
					HexTile tile = GridMap.Instance.map[x, y];
					region.AddTile(tile);
				}
				catch (Exception e) {
					Console.WriteLine(e);
					throw;
				}
				
			}
		}

		return region;
	}
	
	
	
}

public struct WorldMapTemplate {
	public Dictionary<int, RegionTemplate[]> regions; //key is row
}
public struct RegionTemplate {
	public readonly int width;
	public readonly int height;

	public RegionTemplate(int width, int height) {
		this.width = width;
		this.height = height;
	}
}

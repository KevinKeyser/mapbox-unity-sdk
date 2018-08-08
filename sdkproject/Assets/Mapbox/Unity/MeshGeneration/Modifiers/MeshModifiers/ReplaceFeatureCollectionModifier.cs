﻿namespace Mapbox.Unity.MeshGeneration.Modifiers
{
	using UnityEngine;
	using System.Collections.Generic;
	using Mapbox.Unity.MeshGeneration.Data;
	using Mapbox.Unity.Utilities;
	using Mapbox.Unity.Map;
	using System;

	[System.Serializable]
	public class FeatureBundle
	{
		public bool active;

		public SpawnPrefabOptions spawnPrefabOptions;

		[Geocode]
		public List<string> _prefabLocations;

		public List<string> _explicitlyBlockedFeatureIds;

	}


	[System.Serializable]
	public class FeatureBundleList
	{
		public List<FeatureBundle> features = new List<FeatureBundle>();
	}

	/// <summary>
	/// ReplaceFeatureCollectionModifier aggregates multiple ReplaceFeatureModifier objects into one modifier.
	/// </summary>
	[CreateAssetMenu(menuName = "Mapbox/Modifiers/Replace Feature Collection Modifier")]
	public class ReplaceFeatureCollectionModifier : GameObjectModifier, IReplacementCriteria
	{
		public FeatureBundleList featureBundleList;
		public List<FeatureBundle> features = new List<FeatureBundle>();

		private List<ReplaceFeatureModifier> _replaceFeatureModifiers;

		public override void Initialize()
		{
			base.Initialize();
			_replaceFeatureModifiers = new List<ReplaceFeatureModifier>();
			foreach (FeatureBundle feature in features)
			{
				ReplaceFeatureModifier replaceFeatureModifier = ScriptableObject.CreateInstance<ReplaceFeatureModifier>();

				replaceFeatureModifier.Active = feature.active;
				replaceFeatureModifier.SpawnPrefabOptions = feature.spawnPrefabOptions;
				replaceFeatureModifier.PrefabLocations = new List<string>(feature._prefabLocations);
				replaceFeatureModifier.BlockedIds = new List<string>(feature._explicitlyBlockedFeatureIds);
				replaceFeatureModifier.Initialize();

				_replaceFeatureModifiers.Add(replaceFeatureModifier);
			}
		}

		public override void FeaturePreProcess(VectorFeatureUnity feature)
		{
			foreach (ReplaceFeatureModifier modifier in _replaceFeatureModifiers)
			{
				if (modifier == null)
				{
					continue;
				}
				modifier.FeaturePreProcess(feature);
			}
		}

		public override void SetProperties(ModifierProperties properties)
		{
			foreach (ReplaceFeatureModifier modifier in _replaceFeatureModifiers)
			{
				if (modifier == null)
				{
					continue;
				}
				modifier.SetProperties(properties);
			}
		}

		public bool ShouldReplaceFeature(VectorFeatureUnity feature)
		{
			foreach (ReplaceFeatureModifier modifier in _replaceFeatureModifiers)
			{
				if (modifier == null)
				{
					continue;
				}
				if(modifier.ShouldReplaceFeature(feature))
				{
					return true;
				}
			}
			return false;
		}

		public override void Run(VectorEntity ve, UnityTile tile)
		{
			foreach (ReplaceFeatureModifier modifier in _replaceFeatureModifiers)
			{
				modifier.Run(ve, tile);
			}
		}
	}
}

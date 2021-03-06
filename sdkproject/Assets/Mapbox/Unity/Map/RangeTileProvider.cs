namespace Mapbox.Unity.Map
{
	using UnityEngine;
	using Mapbox.Map;
	using System.Collections.Generic;

	public class RangeTileProvider : AbstractTileProvider
	{
		private RangeTileProviderOptions _rangeTileProviderOptions;
		private bool _initialized = false;

		private List<UnwrappedTileId> _toRemove;
		private HashSet<UnwrappedTileId> _tilesToRequest;

		public override void OnInitialized()
		{
			if (Options != null)
			{
				_rangeTileProviderOptions = (RangeTileProviderOptions)Options;
			}
			else
			{
				_rangeTileProviderOptions = new RangeTileProviderOptions();
			}

			_initialized = true;
			_toRemove = new List<UnwrappedTileId>((_rangeTileProviderOptions.east + _rangeTileProviderOptions.west) * (_rangeTileProviderOptions.north + _rangeTileProviderOptions.south));
			_tilesToRequest = new HashSet<UnwrappedTileId>();
		}

		protected virtual void Update()
		{
			if (!_initialized || Options == null)
			{
				return;
			}

			_tilesToRequest.Clear();
			_toRemove.Clear();
			var centerTile = TileCover.CoordinateToTileId(_map.CenterLatitudeLongitude, _map.AbsoluteZoom);
			_tilesToRequest.Add(new UnwrappedTileId(_map.AbsoluteZoom, centerTile.X, centerTile.Y));

			for (int x = (centerTile.X - _rangeTileProviderOptions.west); x <= (centerTile.X + _rangeTileProviderOptions.east); x++)
			{
				for (int y = (centerTile.Y - _rangeTileProviderOptions.north); y <= (centerTile.Y + _rangeTileProviderOptions.south); y++)
				{
					_tilesToRequest.Add(new UnwrappedTileId(_map.AbsoluteZoom, x, y));
				}
			}

			foreach (var item in _activeTiles)
			{
				if (!_tilesToRequest.Contains(item.Key))
				{
					_toRemove.Add(item.Key);
				}
			}

			foreach (var t2r in _toRemove)
			{
				RemoveTile(t2r);
			}

			foreach (var tile in _activeTiles)
			{
				// Reposition tiles in case we panned.
				RepositionTile(tile.Key);
			}

			foreach (var tile in _tilesToRequest)
			{
				if (!_activeTiles.ContainsKey(tile))
				{
					AddTile(tile);
				}
			}
		}
	}
}

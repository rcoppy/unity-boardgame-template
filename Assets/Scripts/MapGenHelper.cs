using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static Alex.BoardGame.AddressableDictionary;

namespace Alex.BoardGame
{
    public class MapGenHelper : MonoBehaviour
    {
        [SerializeField]
        [HideInInspector]
        AddressableDictionary _prefabMap;

        public AddressableDictionary PrefabMap
        {
            set { _prefabMap = value; }
        }

        // map an ascii character to a tile type
        Dictionary<char, PrefabReference> _charMap;

        [SerializeField]
        TextAsset _mapFile;

        Vector2Int _mapBounds;

        public Vector2Int MapBounds
        {
            get { return _mapBounds; }
        }

        List<(Vector2Int, GameObject)> _spawnGrid; 

        public List<(Vector2Int, GameObject)> SpawnGrid
        {
            get { return _spawnGrid; }
        }

        [Serializable]
        struct CharTilePair
        {
            [SerializeField]
            char character;

            [SerializeField]
            string tile; 

            public CharTilePair(char c, string t)
            {
                this.character = c;
                this.tile = t; 
            }
        }

        // guide to which chars map to which strings
        // (should be read-only this is sort of hacky
        [SerializeField]
        List<CharTilePair> _charTileKey; 

        const char NULL_CHAR = (char)47; 

        void InitCharMap()
        {
            _charMap = new Dictionary<char, PrefabReference>(); 

            for (int i = 0; i < _prefabMap.References.Count; i++)
            {
                _charMap[(char)(i + 48)] = _prefabMap.References[i]; 
            }

            _charMap[NULL_CHAR] = PrefabReference.Null();

            _charTileKey = new List<CharTilePair>(); 

            foreach (var kvp in _charMap.AsEnumerable())
            {
                var pair = new CharTilePair(kvp.Key, kvp.Value.Name);
                _charTileKey.Add(pair); 
            }
        }

        // callable from the board
        public void TriggerOnValidate()
        {
            OnValidate();
        }

        void OnValidate()
        {
            InitCharMap();
            ReadMapFile(); 
        }

        void OnEnable()
        {
            OnValidate(); 
        }

        void ReadMapFile()
        {
            int totalCount = 0;
            int lineCount = 0;

            int rowCount = 0;

            _spawnGrid = new List<(Vector2Int, GameObject)>();

            foreach (char c in _mapFile.text.Trim())
            {
                if (string.IsNullOrWhiteSpace("" + c))
                {
                    // certain null characters result in double count?
                    // double-check explicitly for newline

                    if (c == '\n')
                    {
                        rowCount = 0;
                        lineCount++;

                        // Debug.Log($"increment on character {c}; total: {totalCount}; linecount: {lineCount}");
                    }
                } else
                {
                    // Debug.Log($"adding at {rowCount}, {lineCount}");

                    Vector2Int coord = new Vector2Int(rowCount, lineCount);
                    (Vector2Int, GameObject) pair; 

                    if (!_charMap.ContainsKey(c))
                    {
                        pair = (coord, null);
                    } else
                    {
                        pair = (coord, _charMap[c].Prefab);
                    }

                    _spawnGrid.Add(pair);

                    rowCount++; 
                    totalCount++; 
                }
            }

            // last line won't have a trailing whitespace
            // so manually increment
            lineCount++; 

            int rowWidth = totalCount / (lineCount);

            // Debug.Log($"calc: {rowWidth} x {lineCount}; total: {totalCount}; last row count: {rowCount}");

            _mapBounds = new Vector2Int(rowWidth, lineCount);
        }
    }
}

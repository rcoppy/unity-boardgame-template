using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
namespace Alex.BoardGame
{
    [ExecuteAlways]
    [CreateAssetMenu(fileName = "BlockAtlasData", menuName = "Alex/BoardGame/Block Atlas Data", order = 1)]
    public class BlockAtlasData : ScriptableObject
    {
        [Serializable]
        public struct TileUVCoord
        {
            [SerializeField]
            string _name; 

            public string Name
            {
                get { return _name; }
            }

            [SerializeField]
            Vector2 _start;

            public Vector2 Start
            {
                get { return _start; }
            }

            [SerializeField]
            Vector2 _end;

            public Vector2 End
            {
                get { return _end; }
            }

            public TileUVCoord(string name, Vector2 start, Vector2 end)
            {
                _name = name;
                _start = start;
                _end = end; 
            }


            /* get a null struct
            public static PrefabReference Null()
            {
                var pr = new PrefabReference();
                pr._name = "null";
                pr._prefab = null;

                return pr;
            }*/

            /*public bool IsNull()
            {
                return _name.Equals("null") || _prefab == null;
            }*/
        }


        [SerializeField]
        TextAsset _atlasFile; 

        [SerializeField]
        List<TileUVCoord> _tileUVCoords;

        public List<TileUVCoord> TileUVCoords
        {
            get { return _tileUVCoords; }
        }

        // dictionaries don't serialize so serialize the pairs instead
        public Dictionary<string, (Vector2 Start, Vector2 End)> CoordsDict
        {
            get { return _coordsDict; }
        }

        Dictionary<string, (Vector2 Start, Vector2 End)> _coordsDict = null;


        void OnEnable()
        {
            DeserializeCoords(); 
        }

        public void DeserializeCoords()
        {
            _coordsDict = new Dictionary<string, (Vector2 Start, Vector2 End)>();

            foreach (var tile in _tileUVCoords)
            {
                _coordsDict[tile.Name] = (tile.Start, tile.End);
            }
        }

        public (Vector2 Start, Vector2 End) GetUVs(string name)
        {
            return _coordsDict[name];
        }

        void OnValidate()
        {
            _tileUVCoords = new List<TileUVCoord>(); 

            // parse in the values    
            var entries = _atlasFile.text.Split('\n');
            
            foreach (string entry in entries)
            {
                var values = entry.Split(' ');

                try
                {
                    string name = values[0];

                    // generated atlas origin is top-left
                    // but unity uvs are anchored bottom-left

                    float width = float.Parse(values[3]);
                    float height = float.Parse(values[4]);

                    float x1 = float.Parse(values[1]);
                    float y1 = 1f - float.Parse(values[2]) - height;


                    var tile = new TileUVCoord(name, new Vector2(x1, y1), new Vector2(x1 + width, y1 - height));

                    _tileUVCoords.Add(tile);
                } catch
                {
                    Debug.LogWarning("a parsed UV coord was malformed");
                }
            }

            DeserializeCoords(); 
        }
    }
}
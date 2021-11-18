using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Alex.BoardGame {

    [RequireComponent(typeof(MapGenHelper))]
    public class Board : MonoBehaviour
    {
        int _width;
        int _height;

        public Vector3 Origin
        {
            get { return transform.position; }
        }

        public Bounds BoardBounds
        { 
            get {
                float w = _tileWidth * _width;
                float h = _tileWidth * _height;
                
                // Vector3 leftUp = Origin - new Vector3(Origin.x - w, 0, Origin.z + h);
                // Vector3 rightDown = Origin - new Vector3(Origin.x + w, 0, Origin.z - h);

                return new Bounds(Origin, new Vector3(w, 0, h));  
            }
        }


        [SerializeField]
        float _tileWidth = 1f;

        public float TileWidth
        {
            get { return _tileWidth; }
        }

        [SerializeField]
        MapGenHelper _mapGenHelper; 

        public int Width { 
            get { return _width; }
            // set { _width = value;  } 
        }

        public int Height
        {
            get { return _height; }
            // set { _height = value; }
        }

        HashSet<TileEntity> _entities;

        public HashSet<TileEntity> Entities
        {
            get { return _entities; }
        }

        public AddressableDictionary PrefabList
        {
            get { return _prefabList; }
        }

        [SerializeField]
        AddressableDictionary _prefabList; 
        
        public TileEntity SpawnEntity(Vector2Int position, string name)
        {
            return SpawnEntity(position, _prefabList.GetPrefab(name));
        }

        public TileEntity SpawnEntity(Vector2Int position, GameObject entity)
        {
            var inst = Instantiate(entity);
            var tile = inst.GetComponent<TileEntity>();

            tile.OnDespawn += () => _entities.Remove(tile);
            tile.ParentBoard = this;

            // ensure instantly snaps to spawn position
            bool cachedTweenSetting = tile.ShouldTween;
            tile.ShouldTween = false; 

            // must call this *after* setting parent 
            // because setter depends on parent reference
            tile.BoardPosition = position;

            tile.ShouldTween = cachedTweenSetting;

            tile.transform.parent = transform; 

            _entities.Add(tile);

            return tile; 
        }

        // Start is called before the first frame update
        void Awake()
        {
            _entities = new HashSet<TileEntity>();

            // configures map helper
            OnValidate();

            foreach (var pair in _mapGenHelper.SpawnGrid)
            {
                SpawnEntity(pair.Item1, pair.Item2);
            }

        }

        public Vector3 BoardToWorld(int x, int y)
        {
            float worldX, worldZ;

            worldX = Origin.x - BoardBounds.extents.x + x * _tileWidth + _tileWidth / 2;
            worldZ = Origin.z + BoardBounds.extents.z - y * _tileWidth - _tileWidth / 2;

            return new Vector3(worldX, BoardBounds.center.y, worldZ); 
        }

        public Vector2Int WorldToBoard(Vector3 pos)
        {
            int boardX, boardY; 

            boardX = Mathf.RoundToInt((pos.x - _tileWidth / 2f - Origin.x + BoardBounds.extents.x) / (1f * _tileWidth));
            boardY = Mathf.RoundToInt((pos.z + _tileWidth / 2f - Origin.z - BoardBounds.extents.z) / (-1f * _tileWidth));

            return new Vector2Int(boardX, boardY);
        }


        void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(Origin, BoardBounds.size);

#if UNITY_EDITOR
            var color = new Color(1, 0.8f, 0.4f, 1);
            UnityEditor.Handles.color = color;

            // display object "value" in scene
            GUI.color = color;
            UnityEditor.Handles.Label(Origin, gameObject.name);

            // vertical lines
            for (int i = 1; i < _width; i++)
            {
                var top = BoardToWorld(i, 0);
                top.x -= _tileWidth / 2;
                top.z += _tileWidth / 2; 

                var bottom = BoardToWorld(i, _height);
                bottom.x -= _tileWidth / 2;
                bottom.z += _tileWidth / 2;

                UnityEditor.Handles.DrawDottedLine(top, bottom, 2); 
            }

            // horizontal lines
            for (int i = 1; i < _height; i++)
            {
                var left = BoardToWorld(0, i);
                left.x -= _tileWidth / 2;
                left.z += _tileWidth / 2;

                var right = BoardToWorld(_width, i);
                right.x -= _tileWidth / 2;
                right.z += _tileWidth / 2;

                UnityEditor.Handles.DrawDottedLine(left, right, 2);
            }

#endif
        }

        // Update is called once per frame
        void Update()
        {

        }

        void OnValidate()
        {
            _mapGenHelper = GetComponent<MapGenHelper>();

            _mapGenHelper.TriggerOnValidate();

            _width = _mapGenHelper.MapBounds.x;
            _height = _mapGenHelper.MapBounds.y;

            // synchronize with the helper component
            GetComponent<MapGenHelper>().PrefabMap = _prefabList;
        }

#if UNITY_EDITOR
        void OnEnable()
        {
            OnValidate();

            UnityEditor.EditorApplication.playModeStateChanged += ReactToPlayMode;
        }

        void OnDisable()
        {
            UnityEditor.EditorApplication.playModeStateChanged -= ReactToPlayMode;
        }

        // makes sure the gizmos draw when you exit play mode in the editor
        void ReactToPlayMode(UnityEditor.PlayModeStateChange stateChange)
        {
            if (stateChange == UnityEditor.PlayModeStateChange.ExitingPlayMode)
            {
                OnValidate();
                Debug.Log("exiting playmode");
            }
        }

        
#endif
    }
}
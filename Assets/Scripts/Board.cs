using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq; 

namespace Alex.BoardGame {
    [ExecuteAlways]
    [RequireComponent(typeof(MapGenHelper))]
    public class Board : MonoBehaviour
    {
        int _width;
        int _height;

        Queue<GameObject> _tileObjectPool;

        Dictionary<Vector2Int, HashSet<TileEntity>> _hashedEntityCoords; 


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
        bool _showBoardWhenNotActive = true;

        [SerializeField]
        float _tileWidth = 1f;

        public float TileWidth
        {
            get { return _tileWidth; }
        }

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
        

        public List<TileEntity> GetTilesAtPosition(Vector2Int pos) {
            
            if (_hashedEntityCoords != null && _hashedEntityCoords.ContainsKey(pos) && _hashedEntityCoords[pos].Count > 0)
            {
                return new List<TileEntity>(_hashedEntityCoords[pos]);
            }

            Debug.Log($"no tiles registered at position {pos.x}, {pos.y}"); 
            
            return null; 
        }


        public TileEntity SpawnEntity(Vector2Int position, string name)
        {
            return SpawnEntity(position, _prefabList.GetPrefab(name));
        }

        // callback for when a tile is destroyed

        void RegisterEntityDespawn(TileEntity tile)
        {
            tile.OnDespawn -= RegisterEntityDespawn;
            tile.OnPositionUpdate -= RegisterNewEntityCoord;

            if (_entities != null && _entities.Contains(tile))
            {
                _entities.Remove(tile);
            }

            if (_hashedEntityCoords != null && _hashedEntityCoords.ContainsKey(tile.BoardPosition))
            {
                _hashedEntityCoords[tile.BoardPosition].Remove(tile); 
            }
        }

        // callback used to keep coordinate hash synchronized 
        void RegisterNewEntityCoord(Vector2Int newPos, Vector2Int oldPos, TileEntity tile)
        {
            if (_hashedEntityCoords == null)
            {
                _hashedEntityCoords = new Dictionary<Vector2Int, HashSet<TileEntity>>(); 
            }

            if (_hashedEntityCoords.ContainsKey(oldPos))
            {
                _hashedEntityCoords[oldPos].Remove(tile);
            }

            if (!_hashedEntityCoords.ContainsKey(newPos))
            {
                _hashedEntityCoords[newPos] = new HashSet<TileEntity>(); 
            }

            _hashedEntityCoords[newPos].Add(tile); 
        }

        public TileEntity SpawnEntity(Vector2Int position, GameObject entity)
        {
            if (entity == null)
            {
                return null; 
            }

            if (_tileObjectPool == null)
            {
                _tileObjectPool = new Queue<GameObject>();
            }

            GameObject inst;

            if (_tileObjectPool.Count < 1)
            {

#if UNITY_EDITOR
                inst = UnityEditor.PrefabUtility.InstantiatePrefab(entity) as GameObject;
#else
            inst = Instantiate(entity);
#endif
            } else
            {
                inst = _tileObjectPool.Dequeue();
            }

            // TODO: pooling is tricky because tile objects don't necessarily have common children
            // come back to this 

            var tile = inst.GetComponent<TileEntity>();

            tile.OnDespawn += RegisterEntityDespawn;
            tile.OnPositionUpdate += RegisterNewEntityCoord;

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

        public void SpawnBoard()
        {
            _entities = new HashSet<TileEntity>();

            _hashedEntityCoords = new Dictionary<Vector2Int, HashSet<TileEntity>>(); 

            foreach (var pair in _mapGenHelper.SpawnGrid)
            {
                SpawnEntity(pair.Position, pair.Tile);
            }

            Debug.Log("Spawned board pieces.");
        }

        public void HardDespawn()
        {
            // destroy not just listed entities but all children of transform 
            // this only works with destroy immediate: while (transform.childCount > 0)

            // also apparently DestroyImmediate works in play mode without throwing errors? 

            // hacky, need to refactor

            if (transform.childCount > 0) {

                Transform child = transform.GetChild(0);

                while (child != null)
                {
                    var go = child.gameObject;

                    /*if (Application.isEditor)
                    {
                        DestroyImmediate(go);
                    }
                    else
                    {
                        Destroy(go);
                    }*/

                    DestroyImmediate(go);

                    try
                    {
                        child = transform.GetChild(0);
                    }
                    catch
                    {
                        child = null; 
                    }
                } 
            }
        }

        void DespawnBoard()
        {
            Debug.Log("Starting board despawn");

            if (_entities != null)
            {
                List<TileEntity> list = new List<TileEntity>(_entities);

                for (int i = 0; i < list.Count; i++)
                {
                    if (Application.isEditor)
                    {
                        DestroyImmediate(list[i].gameObject);
                    }
                    else
                    {
                        Destroy(list[i].gameObject);
                    }
                }

                _entities.Clear();

                Debug.Log("Despawned board pieces.");
            } else
            {
                Debug.LogWarning("spawnlist was null, nothing to despawn");
            }
        }

        // Start is called before the first frame update
        void Awake()
        {
            HardDespawn();
        }

        public Vector3 BoardToWorld(Vector2Int pos)
        {
            return BoardToWorld(pos.x, pos.y); 
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

        public bool IsPositionObstructed(Vector2Int pos)
        {
            var tiles = GetTilesAtPosition(pos);

            return tiles == null || tiles.Count == 0 || tiles.Where(t => t.IsObstruction).ToList().Count > 0;
        }

        void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(Origin, BoardBounds.size);

#if UNITY_EDITOR
            var color = new Color(1, 0.8f, 0.4f, 0.5f);
            UnityEditor.Handles.color = color;

            // display object "value" in scene
            GUI.color = color;
            // UnityEditor.Handles.Label(Origin, gameObject.name);

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

            // label the tiles
            if (_mapGenHelper.SpawnGrid != null && !_showBoardWhenNotActive)
            {
                foreach (var pair in _mapGenHelper.SpawnGrid)
                {
                    GUI.color = color;
                    UnityEditor.Handles.Label(BoardToWorld(pair.Position) - new Vector3(_tileWidth / 4, 0, _tileWidth / -4), pair.Tile.name.Split(' ')[0].Substring(0, 4));
                }
            }

#endif
        }

        /*private void Awake()
        {
            if (_mapGenHelper == null)
            {
                _mapGenHelper = GetComponent<MapGenHelper>();
            }
        }*/

        // Update is called once per frame
        void Update()
        {

        }

        /*private void OnValidate()
        {
            TriggerMapGen();
        }*/

        void ValidateMapGenHelper()
        {
            if (!_mapGenHelper)
            {
                _mapGenHelper = GetComponent<MapGenHelper>();
            }

            // check the null delegate
            if (_mapGenHelper.OnMapFileChanged == null)
            {
                _mapGenHelper.OnMapFileChanged += ReloadBoard;
            }
        }

        void TriggerMapGen()
        {
            ValidateMapGenHelper();

            _mapGenHelper.TriggerInitGenHelper();

            _width = _mapGenHelper.MapBounds.x;
            _height = _mapGenHelper.MapBounds.y;

            // synchronize with the helper component
            GetComponent<MapGenHelper>().PrefabMap = _prefabList;
        }

        void ReloadBoard()
        {
            DespawnBoard();
            TriggerMapGen();
            
            if (!Application.isEditor || _showBoardWhenNotActive)
            {
                SpawnBoard();
            }
            
        }

#if UNITY_EDITOR
        void OnEnable()
        {
            ReloadBoard(); 
            UnityEditor.EditorApplication.playModeStateChanged += ReactToPlayMode;
        }

        void OnDisable()
        {
            DespawnBoard();
            UnityEditor.EditorApplication.playModeStateChanged -= ReactToPlayMode;
        }

        // makes sure the gizmos draw when you exit play mode in the editor
        void ReactToPlayMode(UnityEditor.PlayModeStateChange stateChange)
        {
            if (stateChange == UnityEditor.PlayModeStateChange.ExitingEditMode)
            {
                DespawnBoard();
                Debug.Log("entering play mode");
            }
            else if (stateChange == UnityEditor.PlayModeStateChange.ExitingPlayMode)
            {
                Debug.Log("exited playmode");
                // ReloadBoard();
            }
        }

        // can't do this with simple OnValidate because
        // of contraints on when Destroy / DestroyImmediate 
        // can be called in editor
        public void CheckBoardReload()
        {
            if (_showBoardWhenNotActive)
            {
                SpawnBoard();
            } else
            {
                DespawnBoard();
            }
        }
#else
        void OnEnable() {
            ReloadBoard();
        }

        void OnDisable() {
            DespawnBoard(); 
        }
#endif
    }
}
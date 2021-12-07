using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine; 

namespace Alex.BoardGame
{
    public class GameManager : MonoBehaviour
    {
        HashSet<TileEntity> _players;

        [Serializable]
        struct SpawnData
        {
            public Vector2Int Position;
            public Color SpawnColor; 
        }

        [SerializeField]
        List<SpawnData> _playerSpawns;

        [SerializeField]
        Camera _camera; 

        [SerializeField]
        Board _board;

        TileEntity _activePlayer; 

        float _lastTime = 0f; 
        float _randomizerPeriod = 0.8f; 

        void RandomizeAllNumberBlocks()
        {
            foreach (var e in _board.Entities)
            {
                var state = e.GetComponentInChildren<NumberBlockState>(); 

                if (state)
                {
                    state.Digit = Mathf.RoundToInt(UnityEngine.Random.Range(0f, 9f)); 
                }
            }

            _lastTime = Time.time; 
        }

        void UpdateRandomizer()
        {
            if (Time.time >= _lastTime + _randomizerPeriod)
            {
                RandomizeAllNumberBlocks(); 
            }
        }

        void Start()
        {
            RandomizeAllNumberBlocks(); 

            _players = new HashSet<TileEntity>(); 

            foreach (var spawn in _playerSpawns)
            {
                var p = _board.SpawnEntity(spawn.Position, "Player");
                _players.Add(p);
            }

            _activePlayer = _players.FirstOrDefault(); 
        }
        
        void Update()
        {
            // TODO: remove, just a demo visual effect
            UpdateRandomizer(); 

            // TODO: replace with modern input hooks
            // jank tile selection
            if (Input.GetMouseButtonDown(0))
            {
                var coords = ScreenCoordToBoardCoord(Input.mousePosition); 
                
                if (coords != null)
                {
                    _activePlayer.BoardPosition = (Vector2Int)coords;
                }
                
            }

            // jank keyboard controls

            Vector2Int keyContribution = Vector2Int.zero;
            
            if (Input.GetKeyDown("left"))
            {
                keyContribution = Vector2Int.left;
            } else if (Input.GetKeyDown("right"))
            {
                keyContribution = Vector2Int.right; 
            } else if (Input.GetKeyDown("up"))
            {
                // board coords are inverted relative to world coords
                keyContribution = Vector2Int.down; 
            } else if (Input.GetKeyDown("down"))
            {
                keyContribution = Vector2Int.up; 
            }

            if (keyContribution != Vector2Int.zero)
            {
                // check if movement is valid

                var targetPos = _activePlayer.BoardPosition + keyContribution;

                var tiles = _board.GetTilesAtPosition(targetPos);

                if (tiles != null && !_board.IsPositionObstructed(targetPos))
                {
                    // position unobstructed, move player there
                    var here = _board.BoardToWorld(_activePlayer.BoardPosition);
                    var there = _board.BoardToWorld(targetPos);

                    // TODO: player rotation in motion direction
                    // _activePlayer.transform.rotation *= Quaternion.FromToRotation(here, there);
  
                    _activePlayer.BoardPosition = targetPos;

                    // Debug.LogWarning("should work");

                    foreach (var t in tiles)
                    {
                        t.OnNotifyOfNewOccupant?.Invoke(_activePlayer);
                    }
                } else if (tiles != null)
                {
                    foreach (var t in tiles.Where(t => t.IsInteractable))
                    {
                        t.OnTriggerInteraction?.Invoke(_activePlayer, t, TileEntity.InteractionType.Movement);  
                    } 
                }
            }
        }

        Vector2Int? ScreenCoordToBoardCoord(Vector3 screenCoord)
        {
            Vector3 mousePos = screenCoord;
            mousePos.z = _camera.nearClipPlane;

            var projection = _camera.ScreenToWorldPoint(mousePos) - _camera.transform.position;

            // can calculate distance to plane intersection
            Vector3 l0 = _camera.transform.position;
            Vector3 l = projection.normalized;
            Vector3 p0 = _board.Origin;
            Vector3 n = _board.transform.up;

            float denominator = Vector3.Dot(l, n);

            if (denominator != 0)
            {
                float d = Vector3.Dot(p0 - l0, n) / denominator;

                Vector3 worldBoardPoint = l0 + d * l;

                return _board.WorldToBoard(worldBoardPoint);

                // Debug.Log($"Click registered at {worldBoardPoint}: {clickCoords}");

                // _activePlayer.BoardPosition = clickCoords
            }

            return null; 
        }

        void OnDrawGizmos()
        {
            foreach (var spawn in _playerSpawns)
            {
                Gizmos.color = spawn.SpawnColor;
                Gizmos.DrawSphere(_board.BoardToWorld(spawn.Position.x, spawn.Position.y), _board.TileWidth * 0.18f); 
            }
        }
    }
}

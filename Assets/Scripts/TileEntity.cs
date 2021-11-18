using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using UnityEngine;

namespace Alex.BoardGame
{
    

    public class TileEntity : MonoBehaviour
    {
        public delegate void Despawn();
        public Despawn OnDespawn; 

        public enum EntityType { 
            Player,         // responds to user input
            Interactable,   // can be modified by player tiles
            Hazard,          // may or may not be static but can't be modified by player tiles
            Environment
        }

        [SerializeField]
        EntityType _myType; 

        public EntityType MyType
        {
            get { return _myType; }
        }

        [SerializeField]
        bool _isStatic = true; 

        public bool IsStatic { get { return _isStatic; } }

        [SerializeField]
        bool _isAlive = true;

        public bool IsAlive { get { return _isAlive; } }

        [SerializeField]
        Vector2Int _boardPosition; 

        Vector3 _targetWorldPosition;

        [SerializeField]
        bool _shouldTween = true; 

        public bool ShouldTween
        {
            get { return _shouldTween; } 
            set { _shouldTween = value; } 
        }

        // fires a callback on update
        public Vector2Int BoardPosition { 
            get { return _boardPosition;  } 
            set {
                var oldPos = _boardPosition; 
                _boardPosition = value;

                OnPositionUpdate(value, oldPos); 
            } 
        }

        public Board ParentBoard; 

        public delegate void PositionUpdate(Vector2Int newPos, Vector2Int oldPos);
        public PositionUpdate OnPositionUpdate; 

        void UpdateWorldTransform(Vector2Int newPos, Vector2Int oldPos) 
        {
            // TODO: Add tweening? 
            if (_shouldTween)
            {
                _targetWorldPosition = GetWorldCoords();
            } else
            {
                transform.position = GetWorldCoords();
                _targetWorldPosition = transform.position; 
            }
        }

        void Update()
        {
            if (_shouldTween && transform.position != _targetWorldPosition)
            {
                transform.position = Vector3.Lerp(transform.position, _targetWorldPosition, 0.08f);
            }
        }

        public Vector3 GetWorldCoords()
        {
            return ParentBoard.BoardToWorld(_boardPosition.x, _boardPosition.y);
        }

        void OnEnable()
        {
            OnPositionUpdate += UpdateWorldTransform; 
        }

        void OnDisable()
        {
            OnPositionUpdate -= UpdateWorldTransform; 
        }

        void OnDestroy()
        {
            OnDespawn.Invoke();
        }

    }
}

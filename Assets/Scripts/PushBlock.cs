using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine; 

namespace Alex.BoardGame
{
    [RequireComponent(typeof(TileEntity))]
    public class PushBlock : MonoBehaviour
    {
        TileEntity _tile; 

        private void Awake()
        {
            _tile = GetComponent<TileEntity>();     
        }
        void OnEnable()
        {
            _tile.OnTriggerInteraction += MoveSelf; 
        }

        private void OnDisable()
        {
            _tile.OnTriggerInteraction -= MoveSelf; 
        }

        void MoveSelf(TileEntity actor, TileEntity receiver, TileEntity.InteractionType interaction)
        {
            if (interaction == TileEntity.InteractionType.Movement)
            {
                Debug.Log("trying to push block");

                var direction = _tile.BoardPosition - actor.BoardPosition;

                if (!_tile.ParentBoard.IsPositionObstructed(_tile.BoardPosition + direction))
                {
                    actor.BoardPosition += direction;
                    _tile.BoardPosition += direction;
                } else
                {
                    Debug.Log("but position was obstructed");
                }
            }
        }


    }
}

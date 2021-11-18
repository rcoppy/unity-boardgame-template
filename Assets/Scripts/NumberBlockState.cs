using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine; 

namespace Alex.BoardGame
{
    [ExecuteAlways]
    public class NumberBlockState : MonoBehaviour
    {
        [SerializeField]
        int _digit;

        public int Digit {
            get { return _digit; }
            set {
                _digit = (int)Mathf.Clamp(value, 0f, 9f);
                UpdateNumberReference(); 
            }
        }

        BlockUVUpdater _uvUpdater;

        // TODO: these shouldn't be hardcoded, 
        // there should be a user-updateable scriptable object
        // to synchronize ints with number tags
        public Dictionary<int, string> DigitsToAtlasKeys = new Dictionary<int, string> { 
                                                                { 0, "Numb_0" },
                                                                { 1, "Numb_1" },
                                                                { 2, "Numb_2" },
                                                                { 3, "Numb_3" },
                                                                { 4, "Numb_4" },
                                                                { 5, "Numb_5" },
                                                                { 6, "Numb_6" },
                                                                { 7, "Numb_7" },
                                                                { 8, "Numb_8" },
                                                                { 9, "Numb_9" },
                                                            };

        private void OnEnable()
        {
            UpdateNumberReference();  
        }

        void UpdateNumberReference()
        {
            if (!_uvUpdater)
            {
                Debug.LogWarning("UV updater is missing");
                _uvUpdater = GetComponentInChildren<BlockUVUpdater>();
            } else
            {
                _uvUpdater.AtlasReference = DigitsToAtlasKeys[_digit]; 
            } 
        }


    }
}

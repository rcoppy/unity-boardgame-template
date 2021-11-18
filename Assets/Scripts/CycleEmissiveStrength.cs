using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Alex.BoardGame
{
    [ExecuteAlways]
    public class CycleEmissiveStrength : MonoBehaviour
    {
        [SerializeField]
        bool _shouldCycle = false; 

        public bool ShouldCycle
        {
            get { return _shouldCycle; }
            set { _shouldCycle = value; }
        }

        [SerializeField]
        float _minLevel = 0f;

        public float MinLevel
        {
            get { return _minLevel; }
            set { _minLevel = value; }
        }

        [SerializeField]
        float _maxLevel = 1f;

        public float MaxLevel
        {
            get { return _maxLevel; }
            set { _maxLevel = value; }
        }

        float _timePeriod = 1f; 

        public float TimePeriod
        {
            get { return _timePeriod; }
            set { _timePeriod = value; }
        }

        // float _timeElapsed = 0f;
        bool _isDecreasing = false;

        float _lastIntensity;
        Texture _originalTexture;

        Renderer _renderer; 

        void Start()
        {
            _renderer = GetComponent<Renderer>();

            _originalTexture = _renderer.material.GetTexture("_EmissionMap");
            _lastIntensity = 1f; 
        }

        void Update()
        {
            if (_shouldCycle)
            {
                UpdateEmissionStrength();
            }
        }

        private void UpdateEmissionStrength()
        {
            float target = _isDecreasing ? _minLevel : _maxLevel;

            _lastIntensity = Mathf.Lerp(_lastIntensity, target, Time.deltaTime / _timePeriod); 
 

            if (_lastIntensity <= _minLevel || _lastIntensity >= _maxLevel)
            {
                _isDecreasing = !_isDecreasing; 
            }

            // TODO this script is a no go for now
            // emission controlled by a texture, not a color, can't figure out where the intensity property is 
            // or if it just gets baked in 

            /*MaterialPropertyBlock props = new MaterialPropertyBlock();
            Texture tex = _originalTexture. * _lastIntensity; 

            props.SetColor("_EmissiveColor", newColor);

            _renderer.SetPropertyBlock(props);*/

        }
    }
}

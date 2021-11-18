using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Alex.BoardGame
{
    [ExecuteAlways]
    public class BlockUVUpdater : MonoBehaviour
    {
        [SerializeField]
        BlockAtlasData _atlasData;

        [SerializeField]
        string _atlasReference;

        public string AtlasReference
        {
            get { return _atlasReference; }
            set { 
                _atlasReference = value;
                UpdateTexture();
            }
        }

        /*private void Update()
        {
            UpdateTexture(); 
        }*/

        private void OnValidate()
        {
            UpdateTexture(); 
        }

        private void OnEnable()
        {
            UpdateTexture();
        }

        private void UpdateTexture()
        {
            if (_atlasData.CoordsDict == null || _atlasData.CoordsDict.Count < 1)
            {
                // deserialization might not have happened yet -- force it now
                _atlasData.DeserializeCoords();
            }

            // update UVs 
            try
            {
                var coords = _atlasData.CoordsDict[_atlasReference];

                var rend = GetComponent<Renderer>();

                MaterialPropertyBlock props = new MaterialPropertyBlock();

                var dif = coords.Start - coords.End; 

                float w, h;
                w = Mathf.Abs(dif.x);
                h = Mathf.Abs(dif.y);

                props.SetVector("_BaseMap_ST", new Vector4(w, h, coords.Start.x, coords.Start.y));
                props.SetVector("_EmissionMap_ST", new Vector4(w, h, coords.Start.x, coords.Start.y));

                rend.SetPropertyBlock(props); 

                // the below updates the material coords globally
                // what we want is the above, per instance via property block
                /*if (Application.isEditor)
                {
                    rend.sharedMaterial.SetTextureOffset("_BaseMap", coords.Start);
                    rend.sharedMaterial.SetTextureOffset("_EmissionMap", coords.Start);
                } else
                {
                    rend.material.SetTextureOffset("_BaseMap", coords.Start);
                    rend.material.SetTextureOffset("_EmissionMap", coords.Start);
                }*/
            }
            catch
            {
                Debug.LogError("Your atlas data isn't configured properly.");
            }
        }
    }
}

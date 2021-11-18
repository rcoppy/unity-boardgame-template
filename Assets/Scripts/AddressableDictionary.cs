using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine; 
namespace Alex.BoardGame
{
    [CreateAssetMenu(fileName = "AddressableDictionary", menuName = "Alex/BoardGame/Addressable Dictionary", order = 1)]
    public class AddressableDictionary : ScriptableObject
    {
        [Serializable]
        public struct PrefabReference
        {
            public string Name
            {
                get { return _name; }
            }

            [SerializeField]
            string _name; 

            public GameObject Prefab
            {
                get { return _prefab; }
            }

            [SerializeField]
            GameObject _prefab; 

            // get a null struct
            public static PrefabReference Null()
            {
                var pr = new PrefabReference(); 
                pr._name = "null";
                pr._prefab = null;

                return pr; 
            }

            public bool IsNull()
            {
                return _name.Equals("null") || _prefab == null; 
            }
        }   

        public List<PrefabReference> References
        {
            get { return _references; }
        }

        [SerializeField]
        List<PrefabReference> _references;

        // dictionaries don't serialize so serialize the pairs instead
        public Dictionary<string, GameObject> ReferenceDict
        {
            get { return _referenceDict; }
        }

        Dictionary<string, GameObject> _referenceDict = null; 


        void OnEnable()
        {
            _referenceDict = new Dictionary<string, GameObject>(); 

            foreach (var reference in _references)
            {
                _referenceDict[reference.Name] = reference.Prefab;
            }
        }

        public GameObject GetPrefab(string name)
        {
            return _referenceDict[name];
        }
    }
}
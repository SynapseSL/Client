using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SynapseClient.API
{
    public class CreditsHook : MonoBehaviour
    {
        public static CreditsHook Singleton;
        public CreditsHook(IntPtr intPtr) : base(intPtr) {}
        
        private bool _creditsReadyToUse;
        private bool _objectsCreated;
        public bool IsCreditsHookReadyToUse => _creditsReadyToUse;

        private static Dictionary<string, CreditsCategoryInfo> _categoryInfos = new Dictionary<string, CreditsCategoryInfo>();

        private GameObject _titleGameObject;
        private GameObject _roleGameObject;
        
        private void OnEnable()
        {
            Singleton = this;
            _creditsReadyToUse = false;
            foreach (CreditsCategoryInfo category in _categoryInfos.Values)
            {
                category.GameObjects = new CreditsCategoryGameObjects();
            }
            _objectsCreated = _categoryInfos.Values.Count == 0;
        }

        private void OnDisable()
        {
            foreach (CreditsCategoryInfo category in _categoryInfos.Values)
            {
                category.GameObjects = new CreditsCategoryGameObjects();
            }
        }

        private void Update()
        {
            if (!_creditsReadyToUse)
            {
                GameObject content = GameObject.Find("/New Main Menu/Credits/root/Content");
                if (content != null)
                {
                    if (content.transform.childCount >= 310)
                    {
                        _titleGameObject = GameObject.Find("/New Main Menu/Credits/root/Content/CreditsCategory(Clone)");
                        _roleGameObject = GameObject.Find("/New Main Menu/Credits/root/Content/CreditsElement with Role(Clone)/");
                        _creditsReadyToUse = true;
                        Events.InvokeCreateCreditsEvent(this);
                    }
                }
            }
            
            if (!_objectsCreated)
            {
                foreach (CreditsCategoryInfo cat in _categoryInfos.Values)
                {
                    cat.GameObjects.CategoryObject = GenerateCategoryObject(cat.CategoryName);
                    foreach (UserInfo user in cat.UserEntries)
                    {
                        cat.GameObjects.EntryObjects.Add(GenerateEntryObject(user.Username, user.Role, user.Color));
                    }
                }

                _objectsCreated = true;
                SortElements();
            }
        }

        private void SortElements()
        {
            int counter = 2;
            foreach (CreditsCategoryInfo cat in _categoryInfos.Values)
            {
                cat.GameObjects.CategoryObject.transform.SetSiblingIndex(counter++);
                foreach (GameObject entry in cat.GameObjects.EntryObjects)
                {
                    entry.transform.SetSiblingIndex(counter++);
                }
            }
        }
        
        public CreditsCategoryInfo GetCreditsCategory(string catName)
        {
            if (!_creditsReadyToUse)
                return null;

            return _categoryInfos.ContainsKey(catName) ? _categoryInfos[catName] : null;
        }

        public bool CreateCreditsCategory(string catName)
        {
            if (!_creditsReadyToUse)
                return false;

            if (_categoryInfos.ContainsKey(catName))
                return false;

            CreditsCategoryInfo catInfo = new CreditsCategoryInfo();
            catInfo.CategoryName = catName;
            catInfo.GameObjects.CategoryObject = GenerateCategoryObject(catName);
            
            _categoryInfos.Add(catName, catInfo);
            
            SortElements();
            
            return true;
        }
        
        private GameObject GenerateCategoryObject(string catName)
        {
            GameObject result = Instantiate(_titleGameObject, _titleGameObject.transform.parent, true);
            result.GetComponent<TMP_Text>().text = catName;

            return result;
        }

        public bool CreateCreditsEntry(string username, string role, string category, Color color)
        {
            if (!_creditsReadyToUse)
                return false;

            if (!_categoryInfos.ContainsKey(category))
                return false;

            CreditsCategoryInfo cat = _categoryInfos[category];

            cat.GameObjects.EntryObjects.Add(GenerateEntryObject(username, role, color));
            
            cat.UserEntries.Add(new UserInfo()
            {
                Username = username,
                Role = role,
                Color = color
            });
            
            SortElements();
            
            return true;
        }
        
        private GameObject GenerateEntryObject(string username, string role, Color color)
        {
            GameObject result = Instantiate(_roleGameObject, _roleGameObject.transform.parent, true);

            result.transform.GetChild(0).GetComponent<TMP_Text>().text = username;
            result.transform.GetChild(1).GetComponent<Image>().color = color;
            result.transform.GetChild(1).GetChild(0).GetComponent<TMP_Text>().text = role;

            return result;
        }
    }
    
    public class UserInfo
    {
        public string Username;
        public string Role;
        public Color Color;
    }

    public class CreditsCategoryGameObjects
    {
        public GameObject CategoryObject;
        public List<GameObject> EntryObjects = new List<GameObject>();
    }

    public class CreditsCategoryInfo
    {
        public string CategoryName;
        public List<UserInfo> UserEntries = new List<UserInfo>();
        public CreditsCategoryGameObjects GameObjects = new CreditsCategoryGameObjects();
    }
    
    public class CreditColors
    {
        // Gold
        public static readonly Color Gold = new Color(1, 0.8431f, 0, 1);
        // Yellow
        public static readonly Color Yellow100 = new Color(0.8471f, 0.5922f, 0.1882f, 1);
        public static readonly Color Yellow200 = new Color(1, 0.6902f, 0.2118f, 1);
        public static readonly Color Yellow300 = new Color(1, 0.7608f, 0.2863f, 1);
        // Beige
        public static readonly Color Beige100 = new Color(0.8f, 0.5882f, 0.3569f, 1);
        public static readonly Color Beige200 = new Color(1, 0.8235f, 0.5176f, 1);
        public static readonly Color Beige300 = new Color(0.8392f, 0.8392f, 0.6275f, 1);
        // Red
        public static readonly Color Red100 = new Color(0.4196f, 0.0667f, 0.1412f, 1);
        public static readonly Color Red200 = new Color(0.5294f, 0.0627f, 0.1529f, 1);
        public static readonly Color Red300 = new Color(0.6196f, 0.051f, 0.1843f, 1);
        public static readonly Color Red400 = new Color(0.7529f, 0.1294f, 0.2745f, 1);
        public static readonly Color Red500 = new Color(0.8627f, 0.0784f, 0.2353f, 1);
        public static readonly Color Red600 = new Color(0.8196f, 0.1333f, 0.1333f, 1);
        public static readonly Color Red700 = new Color(0.6784f, 0.2196f, 0.2196f, 1);
        public static readonly Color Red800 = new Color(0.8588f, 0.2078f, 0.2078f, 1);
        // Orange
        public static readonly Color Orange100 = new Color(0.6588f, 0.2627f, 0, 1);
        public static readonly Color Orange200 = new Color(0.7137f, 0.4706f, 0.1412f, 1);
        public static readonly Color Orange300 = new Color(0.7608f, 0.4863f, 0.0549f, 1);
        public static readonly Color Orange400 = new Color(1, 0.6314f, 0, 1);
        public static readonly Color Orange500 = new Color(0.9725f, 0.6471f, 0.0824f, 1);
        public static readonly Color Orange600 = new Color(1, 0.698f, 0.2902f, 1);
        // Lime
        public static readonly Color Lime100 = new Color(0.5255f, 1f, 0.5255f, 1);
        public static readonly Color Lime200 = new Color(0.1137f, 1f, 0.5686f, 1);
        public static readonly Color Lime300 = new Color(0.251f, 0.6196f, 0.251f, 1);
        // Green
        public static readonly Color Green100 = new Color(0f, 0.5882f, 0.2039f, 1);
        public static readonly Color Green200 = new Color(0, 0.5804f, 0.2431f, 1);
        public static readonly Color Green300 = new Color(0.0588f, 0.549f, 0.2353f, 1);
        public static readonly Color Green400 = new Color(0.2941f, 0.7451f, 0.3294f, 1);
        // Purple
        public static readonly Color Purple100 = new Color(0.098f, 0.0235f, 0.4275f, 1);
        public static readonly Color Purple200 = new Color(0.1725f, 0.0235f, 0.549f, 1);
        public static readonly Color Purple300 = new Color(0.1686f, 0.0471f, 0.6667f, 1);
        public static readonly Color Purple400 = new Color(0.3725f, 0.2f, 0.7725f, 1);
        public static readonly Color Purple500 = new Color(0.3922f, 0.1882f, 0.902f, 1);
        public static readonly Color Purple600 = new Color(0.6784f, 0.6941f, 1, 1);
        // Pink
        public static readonly Color Pink100 = new Color(0.7922f, 0.1059f, 0.2588f, 1);
        public static readonly Color Pink200 = new Color(0.8784f, 0.0667f, 0.3725f, 1);
        public static readonly Color Pink300 = new Color(0.7451f, 0.4431f, 0.5686f, 1);
        public static readonly Color Pink400 = new Color(0.9412f, 0.6745f, 0.8392f, 1);
        public static readonly Color CrabPink = new Color(0.9765f, 0.4078f, 0.3294f, 1);
        // Blue
        public static readonly Color Blue100 = new Color(0.2745f, 0.302f, 0.8549f, 1);
        public static readonly Color Blue200 = new Color(0.0588f, 0.8784f, 0.9804f, 1);
        // Turquoise
        public static readonly Color Turquoise100 = new Color(0.3137f, 0.7176f, 0.702f, 1);
        public static readonly Color Turquoise200 = new Color(0.1098f, 0.5686f, 0.549f, 1);
        public static readonly Color Turquoise300 = new Color(0.0157f, 0.4275f, 0.4039f, 1);
        // White and Gray
        public static readonly Color Gray = new Color(0.5529f, 0.5333f, 0.5333f, 1);
        public static readonly Color LightGray = new Color(1, 0.9294f, 0.949f, 1);
        public static readonly Color White = new Color(1, 1, 1, 1);
        // Magenta
        public static readonly Color Magenta100 = new Color(0.5608f, 0.0824f, 0.3059f, 1);
        public static readonly Color Magenta200 = new Color(0.5176f, 0.1137f, 0.702f, 1);
        public static readonly Color Magenta300 = new Color(0.6588f, 0.1216f, 0.7137f, 1);
    }
}
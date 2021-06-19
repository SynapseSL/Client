using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

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
        private GameObject _goldRoleGameObject;
        private GameObject _grayRoleGameObject;
        private GameObject _magentaRoleGameObject;
        private GameObject _orangeRoleGameObject;
        private GameObject _greenRoleGameObject;
        private GameObject _blueRoleGameObject;
        
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
                        _goldRoleGameObject = content.transform.GetChild(3).gameObject;
                        _grayRoleGameObject = content.transform.GetChild(4).gameObject;
                        _magentaRoleGameObject = content.transform.GetChild(5).gameObject;
                        _orangeRoleGameObject = content.transform.GetChild(6).gameObject;
                        _greenRoleGameObject = content.transform.GetChild(7).gameObject;
                        _blueRoleGameObject = content.transform.GetChild(8).gameObject;
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
            int _catCounter = 2;
            foreach (CreditsCategoryInfo cat in _categoryInfos.Values)
            {
                int _elementCounter = 0;
                
                cat.GameObjects.CategoryObject.transform.SetSiblingIndex(_catCounter++);
                foreach (GameObject entry in cat.GameObjects.EntryObjects)
                {
                    entry.transform.SetSiblingIndex(_catCounter + _elementCounter++);
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

        public bool CreateCreditsEntry(string username, string role, string category, CreditsColor color)
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
        
        private GameObject GenerateEntryObject(string username, string role, CreditsColor color)
        {
            GameObject result = null;
            switch (color)
            {
                case CreditsColor.Gold:
                    result = Instantiate(_goldRoleGameObject, _goldRoleGameObject.transform.parent, true);
                    break;

                case CreditsColor.Gray:
                    result = Instantiate(_grayRoleGameObject, _grayRoleGameObject.transform.parent, true);
                    break;

                case CreditsColor.Red:
                    result = Instantiate(_magentaRoleGameObject, _magentaRoleGameObject.transform.parent, true);
                    break;

                case CreditsColor.Orange:
                    result = Instantiate(_orangeRoleGameObject, _orangeRoleGameObject.transform.parent, true);
                    break;

                case CreditsColor.Green:
                    result = Instantiate(_greenRoleGameObject, _greenRoleGameObject.transform.parent, true);
                    break;

                case CreditsColor.Blue:
                    result = Instantiate(_blueRoleGameObject, _blueRoleGameObject.transform.parent, true);
                    break;

                default:
                    return null;
            }

            result.transform.GetChild(0).GetComponent<TMP_Text>().text = username;
            result.transform.GetChild(1).GetChild(0).GetComponent<TMP_Text>().text = role;

            return result;
        }
    }
    
    public class UserInfo
    {
        public string Username;
        public string Role;
        public CreditsColor Color;
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

    public enum CreditsColor
    {
        Gold = 0,
        Gray,
        Red,
        Orange,
        Green,
        Blue
    }
}
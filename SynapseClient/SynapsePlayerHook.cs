using System;
using SynapseClient.API;
using SynapseClient.Patches;
using UnityEngine;

namespace SynapseClient
{ 
    
    public class SynapsePlayerHook : MonoBehaviour
    {
        public static SynapsePlayerHook Singleton;
        public SynapsePlayerHook(IntPtr intPtr) : base(intPtr) {}

        public Camera Camera { get; set; }

        private GameObject _lookingAtCube;

        private int lastInvalidTraceId = 0;

        public GameObject LookingAt
        {
            get => _lookingAt;
            set => _lookingAt = value;
        }

        private GameObject _lookingAt;

        public void Awake()
        {
            Singleton = this;
            Logger.Info($"Awake SynapsePlayerHook in {gameObject.name}");
            _lookingAtCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            _lookingAtCube.transform.localScale = Vector3.one * 0.1f;
            Destroy(_lookingAtCube.GetComponent<BoxCollider>());
            CompleteAuth();
        }

        public void Update()
        {
            if (Camera == null) ResetCamera();
            
            RaycastHit hit;
            var mousePos = Input.mousePosition;
            var ray = Camera.ScreenPointToRay(mousePos);
            if (Physics.Raycast(ray, out hit)) {
                if (Input.GetKey(KeyCode.Keypad0)) _lookingAtCube.transform.position = hit.point;
                _lookingAt = hit.transform.gameObject;
                if (_lookingAt.GetInstanceID() != lastInvalidTraceId)
                {
                    Logger.Info($"Looking at object {_lookingAt.ToString()}");
                    var receiver = _lookingAt.GetComponent<LookReceiver>();
                    if (receiver == null)
                    {
                        lastInvalidTraceId = _lookingAt.GetInstanceID();
                    }
                    else
                    {
                        receiver.LookReceiveAction.Invoke(hit.point);
                    }
                }
            }
        }
        
        void OnGUI()
        {
            GUI.Label(new Rect(100, 10, 100, 100), ((int)(1.0f / Time.smoothDeltaTime) + " FPS").ToString());        
        }
        
        public RaycastHit? Raycast()
        {
            RaycastHit hit;
            var mousePos = Input.mousePosition;
            var ray = Camera.ScreenPointToRay(mousePos);
            if (Physics.Raycast(ray, out hit))
            {
                return hit;
            }
            return null;
        }

        public void OnDisable()
        {
            Logger.Error("Disable!!!");
            Events.InvokeRoundEnd();
        }

        private void ResetCamera()
        {
            var cameraObj = GameObject.Find($"{gameObject.name}/AllCameras (Recoil)/FirstPersonCharacter");
            Camera = cameraObj.GetComponent<Camera>();
        }
        
        private void CompleteAuth()
        {
            var roles = GetComponent<ServerRoles>();
            roles.CmdServerSignatureComplete("synapse-client-authentication", AuthPatches.synapseSessionToken, "", false);
        }
    }
}
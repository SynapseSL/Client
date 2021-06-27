using System;
using MelonLoader.Support;
using SynapseClient.API;
using SynapseClient.Patches;
using UnityEngine;

namespace SynapseClient
{ 
    
    public class LocalPlayer : MonoBehaviour
    {
        public static LocalPlayer Singleton;
        public LocalPlayer(IntPtr intPtr) : base(intPtr) {}

        public Camera Camera { get; internal set; }
        public AudioSource PlayerAudioSource { get; internal set; }

        private GameObject _lookingAtCube;

        private int lastInvalidTraceId = 0;

        public GameObject LookingAt
        {
            get => _lookingAt;
            set => _lookingAt = value;
        }

        private GameObject _lookingAt;
        
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

        public void PlayAudio(AudioClip clip)
        {
            PlayerAudioSource.PlayOneShot(clip);
        }
        
        private void Awake()
        {
            Singleton = this;
            PlayerAudioSource = GetComponentInChildren<AudioSource>();
            Logger.Info($"Awake SynapsePlayerHook in {gameObject.name}");
            _lookingAtCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            _lookingAtCube.transform.localScale = Vector3.one * 0.1f;
            Destroy(_lookingAtCube.GetComponent<BoxCollider>());
            CompleteAuth();
        }

        private void Update()
        {
            Coroutines.Process();
            Client.DoQueueTick();
            if (Camera == null) ResetCamera();
            RaycastHit hit;
            var mousePos = Input.mousePosition;
            var ray = Camera.ScreenPointToRay(mousePos);
            if (Physics.Raycast(ray, out hit)) {
                if (Input.GetKey(KeyCode.Keypad0)) _lookingAtCube.transform.position = hit.point;
                _lookingAt = hit.transform.gameObject;
                if (_lookingAt.GetInstanceID() != lastInvalidTraceId)
                { 
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

        private void FixedUpdate()
        {
            Coroutines.ProcessWaitForFixedUpdate();
        }

        private void LateUpdate()
        {
            Coroutines.ProcessWaitForEndOfFrame();
        }

        private void OnGUI()
        {
            GUI.Label(new Rect(100, 10, 100, 100), ((int)(1.0f / Time.smoothDeltaTime) + " FPS").ToString());        
        }
        
        private void OnDisable()
        {
            Logger.Info("Round has ended");
            Events.InvokeRoundEnd();
            Client.Singleton.ModLoader.ServerConnectionEnd();
        }

        private void ResetCamera()
        {
            Camera = GetComponentInChildren<Camera>();
            Logger.Info(Camera.gameObject.name);
        }
        
        private void CompleteAuth()
        {
            var roles = GetComponent<ServerRoles>();
            roles.CmdServerSignatureComplete("synapse-client-authentication", AuthPatches.synapseSessionToken, "", false);
        }
    }
}
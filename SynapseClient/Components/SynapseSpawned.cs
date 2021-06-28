using System;
using UnityEngine;

namespace SynapseClient.Components
{
    public class SynapseSpawned : MonoBehaviour
    {
        public SynapseSpawned(IntPtr intPtr) : base(intPtr) { }
        public Il2CppSystem.String Blueprint { get; internal set; }

        private Transform _transform;

        public void Awake()
        {
            _transform = transform;
        }

        public void Update()
        {

        }

        //Reserved
        public void TweenTo(Vector3 vector3, Quaternion quaternion)
        {
            _transform.position = vector3;
            _transform.rotation = quaternion;
        }

        public static SynapseSpawned ForObject(GameObject gameObject)
        {
            var ss = gameObject.GetComponent<SynapseSpawned>();
            if (ss == null)
            {
                throw new Exception("GameObject is not spawned via Synapse");
            }

            return ss;
        }
    }
}

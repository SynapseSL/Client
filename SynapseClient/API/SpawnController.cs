using System;
using System.Collections.Generic;
using System.Reflection;
using SynapseClient.Pipeline;
using SynapseClient.Pipeline.Packets;
using UnityEngine;

namespace SynapseClient.API
{
    public class SpawnController
    {
        //Maybe replace with just a string
        public List<GameObject> SpawnedObjects { get; internal set; } = new List<GameObject>();

        private Dictionary<string, SpawnHandler> Blueprints { get; set; } =
            new Dictionary<string, SpawnHandler>();

        internal void Subscribe()
        {
            ClientPipeline.DataReceivedEvent += delegate(PipelinePacket ev)
            {
                switch (ev.PacketId)
                {
                    case 10:
                    {
                        SpawnPacket.Decode(ev, out var pos, out var rot, out var name, out var blueprint);
                        Spawn(pos, rot, name, blueprint);
                        break;
                    }
                    case 11:
                    {
                        DestroyPacket.Decode(ev, out var name, out var blueprint);
                        var obj = GameObject.Find(name);
                        Destroy(obj, blueprint);
                        break;
                    }
                    case 12:
                    {
                        PositionPacket.Decode(ev, out var pos, out var rot, out var name);
                        var obj = GameObject.Find(name);
                        try
                        {
                            var spawned = SynapseSpawned.ForObject(obj);
                            spawned.TweenTo(pos, rot);
                        }
                        catch (Exception e)
                        {
                            var transform = obj.transform;
                            transform.position = pos;
                            transform.rotation = rot;   
                        }
                        break;
                    }
                }
            };
        }
        
        public void Register(SpawnHandler handler)
        {
            Blueprints[handler.GetBlueprint()] = handler;
        }

        public void Unregister(SpawnHandler handler)
        {
            Blueprints.Remove(handler.GetBlueprint());
        }
        
        public void Unregister(string blueprint)
        {
            Blueprints.Remove(blueprint);
        }

        public void Spawn(Vector3 pos, Quaternion rot, string name, string blueprint)
        {
            var handler = Blueprints[blueprint];
            var gameObject = handler.Spawn(pos, rot, name);
            var ss = gameObject.AddComponent<SynapseSpawned>();
            ss.Blueprint = blueprint;
            SpawnedObjects.Add(gameObject);
        }

        public void Destroy(GameObject gameObject, string blueprint)
        {
            SpawnedObjects.Remove(gameObject);
            var handler = Blueprints[blueprint];
            handler.Destroy(gameObject);
        }
    }

    public class SynapseSpawned : MonoBehaviour
    {
        public SynapseSpawned(IntPtr intPtr) : base(intPtr) {}
        public Il2CppSystem.String Blueprint { get; internal set; }

        private Transform _transform;

        private Vector3 _targetPos;
        private Quaternion _targetRot;

        public void Awake()
        {
            _transform = transform;
            _targetPos = _transform.position;
            _targetRot = _transform.rotation;
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

    public abstract class SpawnHandler
    {
        public abstract GameObject Spawn(Vector3 pos, Quaternion rot, string name);
        public abstract void Destroy(GameObject gameObject);

        public virtual string GetBlueprint()
        {
            var blueprint = GetType().GetCustomAttribute(typeof(Blueprint)) as Blueprint;
            if (blueprint == null)
            {
                throw new Exception("SpawnHandler subclass is not annotated with [Blueprint]");
            }
            return blueprint.Name;
        }
    }

    public class Blueprint : Attribute
    {
        public string Name { get; set; }
    }
}
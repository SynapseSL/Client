using System.Collections.Generic;

namespace SynapseClient.API
{
    public abstract class SpawnHandlerManager
    {
        public List<SpawnHandler> Handlers { get; private set; } = new List<SpawnHandler>();

        public abstract void RegisterAll();

        public void Register(SpawnHandler handler)
        {
            Handlers.Add(handler);
            Client.Singleton.SpawnController.Register(handler);
        }

        public void UnregisterAll()
        {
            foreach (var spawnHandler in Handlers)
            {
                Client.Singleton.SpawnController.Unregister(spawnHandler);
            }
            Handlers.Clear();
        }

    }
}
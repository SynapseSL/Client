using SynapseClient.Components;

namespace SynapseClient.API.Events
{
    public class EventHandlers
    {
        internal EventHandlers()
        {
            SynapseEvents.OnCreateCreditsEvent += CreateCredits;
        }

        private void CreateCredits(CreditsHook ev)
        {
            // Synapse Client Credits
            ev.CreateCreditsCategory("Synapse Client");
            ev.CreateCreditsEntry("Helight", "Maintainer", "Synapse Client", CreditColors.Red600);
            ev.CreateCreditsEntry("Dimenzio", "Developer", "Synapse Client", CreditColors.Blue100);
            ev.CreateCreditsEntry("Wholesome", "Developer", "Synapse Client", CreditColors.Blue100);
            ev.CreateCreditsEntry("Mika", "Developer", "Synapse Client", CreditColors.Blue100);
            ev.CreateCreditsEntry("Cubuzz", "Developer", "Synapse Client", CreditColors.Blue100);
            ev.CreateCreditsEntry("Flo0205", "Developer", "Synapse Client", CreditColors.Blue100);

            // Synapse Server Credits
            ev.CreateCreditsCategory(("Synapse Server"));
            ev.CreateCreditsEntry("Dimenzio", "Creator, Maintainer", "Synapse Server", CreditColors.Red600);
            ev.CreateCreditsEntry("Helight", "Maintainer", "Synapse Server", CreditColors.Red600);
            ev.CreateCreditsEntry("MineTech13", "Useless Sys Admin", "Synapse Server", CreditColors.Yellow300);
            ev.CreateCreditsEntry("moelrobi", "Former-Maintainer", "Synapse Server", CreditColors.Gray);
            ev.CreateCreditsEntry("Mika", "Contributor", "Synapse Server", CreditColors.Blue100);
            ev.CreateCreditsEntry("AlmightyLks", "Contributor", "Synapse Server", CreditColors.Blue100);
            ev.CreateCreditsEntry("TheVoidNebula", "Contributor", "Synapse Server", CreditColors.Blue100);
            ev.CreateCreditsEntry("PintTheDragon", "Contributor", "Synapse Server", CreditColors.Blue100);
        }
    }
}

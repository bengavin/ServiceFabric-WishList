using System;
using System.Collections.Generic;
using System.Fabric;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using WishList.Core.Models;
using WishList.Core.Services;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;

namespace WishList.PersonService
{
    /// <summary>
    /// An instance of this class is created for each service replica by the Service Fabric runtime.
    /// </summary>
    internal sealed class PersonService : StatefulService, IPersonService
    {
        private IReliableDictionary<Guid, Person> _personDictionary;
        private IReliableDictionary<string, Guid> _emailLookupDictionary;
        private IReliableDictionary<string, Guid> _twitterLookupDictionary;

        public PersonService(StatefulServiceContext context)
            : base(context)
        { }

        public async Task<bool> AddOrUpdateAsync(Person person)
        {
            using (var ctx = this.StateManager.CreateTransaction())
            {
                var personId = person.Id;
                var currentPerson = (Person)null;

                if (await _personDictionary.ContainsKeyAsync(ctx, personId))
                {
                    var personValue = await _personDictionary.TryGetValueAsync(ctx, personId);
                    if (personValue.HasValue) { currentPerson = personValue.Value; }
                }

                if (currentPerson == null)
                {
                    await _personDictionary.AddAsync(ctx, person.Id, person);
                    if (!String.IsNullOrWhiteSpace(person.EmailAddress))
                    {
                        await _emailLookupDictionary.AddAsync(ctx, person.EmailAddress, person.Id);
                    }

                    if (!String.IsNullOrWhiteSpace(person.TwitterHandle))
                    {
                        await _twitterLookupDictionary.AddAsync(ctx, person.TwitterHandle, person.Id);
                    }

                    await ctx.CommitAsync();

                    return true;
                }
                else
                {
                    if (await _personDictionary.TryUpdateAsync(ctx, person.Id, person, currentPerson))
                    {
                        await ctx.CommitAsync();

                        return true;
                    }
                }
            }

            return false;
        }

        public async Task<Person> GetByEmailOrTwitterAsync(string email, string twitter)
        {
            using (var ctx = this.StateManager.CreateTransaction())
            {
                var personId = Guid.Empty;
                if (!String.IsNullOrWhiteSpace(email) && await _emailLookupDictionary.ContainsKeyAsync(ctx, email.ToLower()))
                {
                    var personValue = await _emailLookupDictionary.TryGetValueAsync(ctx, email.ToLower());
                    if (personValue.HasValue) { personId = personValue.Value; }
                }

                if (personId == Guid.Empty && !String.IsNullOrWhiteSpace(twitter) && await _twitterLookupDictionary.ContainsKeyAsync(ctx, twitter.ToLower()))
                {
                    var personValue = await _twitterLookupDictionary.TryGetValueAsync(ctx, twitter.ToLower());
                    if (personValue.HasValue) { personId = personValue.Value; }
                }

                if (personId == Guid.Empty)
                {
                    return null;
                }

                var person = await _personDictionary.TryGetValueAsync(ctx, personId);

                return person.HasValue ? person.Value : null;
            }
        }

        /// <summary>
        /// Optional override to create listeners (e.g., HTTP, Service Remoting, WCF, etc.) for this service replica to handle client or user requests.
        /// </summary>
        /// <remarks>
        /// For more information on service communication, see https://aka.ms/servicefabricservicecommunication
        /// </remarks>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            return new ServiceReplicaListener[]
                {
                    new ServiceReplicaListener(context => this.CreateServiceRemotingListener(context))
                };
        }

        /// <summary>
        /// This is the main entry point for your service replica.
        /// This method executes when this replica of your service becomes primary and has write status.
        /// </summary>
        /// <param name="cancellationToken">Canceled when Service Fabric needs to shut down this service replica.</param>
        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            _personDictionary = await this.StateManager.GetOrAddAsync<IReliableDictionary<Guid, Person>>($"PersonServiceDictionary-{this.Partition}");
            _emailLookupDictionary = await this.StateManager.GetOrAddAsync<IReliableDictionary<string, Guid>>($"PersonServiceEmailLookupDictionary-{this.Partition}");
            _twitterLookupDictionary = await this.StateManager.GetOrAddAsync<IReliableDictionary<string, Guid>>($"PersonServiceTwitterLookupDictionary-{this.Partition}");
        }
    }
}

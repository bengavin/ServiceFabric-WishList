using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Remoting;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using System;
using System.Threading.Tasks;
using WishList.Core.Models;

namespace WishList.Core.Services
{
    public interface IPersonService : IService
    {
        Task<Person> GetByEmailOrTwitterAsync(string email, string twitter);
        Task<bool> AddOrUpdateAsync(Person person);
    }

    public interface IPersonServiceFactory
    {
        IPersonService Create(string lastName);
    }

    public class PersonServiceFactory : IPersonServiceFactory
    {
        public static readonly Uri FabricUri = new Uri("fabric:/WishList.ServiceFabric/PersonService");
        public static int PersonServicePartitionCount = 4;

        public IPersonService Create(string lastName)
        {
            if (String.IsNullOrWhiteSpace(lastName)) { throw new ArgumentException("Last Name is required, cannot create person service instance"); }

            var partitionId = (lastName.ToLower()[0] - 'a') % PersonServicePartitionCount;
            return ServiceProxy.Create<IPersonService>(FabricUri, new ServicePartitionKey(partitionId));
        }
    }
}

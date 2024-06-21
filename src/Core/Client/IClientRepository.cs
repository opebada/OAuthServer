using System;
using System.Collections.Generic;
using System.Threading.Tasks;
namespace Core.Client
{
    /// <summary>
    /// TODO: Research why we need DTOs
    /// </summary>
    public interface IClientRepository
    {
        Task<Client> Create(Client client);
        Task<Client> GetById(string id);
        Task<IEnumerable<Client>> GetAllClients();
        Task<Client> Update(Client client);
        Task<bool> Delete(string id);
    }
}


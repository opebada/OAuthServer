using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.Client;

public class ClientRepository : IClientRepository
{
    private readonly IDictionary<string, ClientApplication> _clients;

    public ClientRepository()
    {
        _clients = new Dictionary<string, ClientApplication>();
        _clients.Add("client1", new ClientApplication {
            Id = "client1",
            Name = "client1",
            Description = "client1",
            ClientType = ClientType.Confidential,    
            ClientSecret = "secret",
            RedirectUrls = new List<RedirectUrl> {
                new RedirectUrl {
                    Id = 1,
                    ClientId = "client1",
                    Value = "https://localhost:4000"
                }
            }
        });
    }

    public async Task<ClientApplication> Create(ClientApplication client)
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentException.ThrowIfNullOrWhiteSpace(client.Id);
        
        if (_clients.ContainsKey(client.Id))
            throw new ArgumentException($"Client with id {client.Id} already exists", nameof(client));

        _clients.Add(client.Id, client);

        return await Task.FromResult(client);
    }

    public async Task<bool> Delete(string id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);

        if (!_clients.ContainsKey(id))
            return await Task.FromResult(false);

        _clients.Remove(id);
        return await Task.FromResult(true);
    }

    public async Task<ICollection<ClientApplication>> GetAllClients()
    {
        return await Task.FromResult(_clients.Values);
    }

    public async Task<ClientApplication> GetById(string id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);

        if (!_clients.ContainsKey(id))
            return await Task.FromResult<ClientApplication>(null);

        return await Task.FromResult(_clients[id]);
    }

    public async Task<bool> Update(ClientApplication client)
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentException.ThrowIfNullOrWhiteSpace(client.Id);

        if (!_clients.ContainsKey(client.Id))
            return await Task.FromResult(false);;

        _clients[client.Id] = client;

        return await Task.FromResult(true);
    }
}

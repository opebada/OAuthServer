﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
namespace Core.Client
{
    /// <summary>
    /// </summary>
    public interface IClientRepository
    {
        Task<ClientApplication> Create(ClientApplication client);
        Task<ClientApplication> GetById(string id);
        Task<IEnumerable<ClientApplication>> GetAllClients();
        Task<ClientApplication> Update(ClientApplication client);
        Task<bool> Delete(string id);
    }
}


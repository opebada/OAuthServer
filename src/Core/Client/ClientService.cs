using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace Core.Client
{
    /// <summary>
    /// Put this all in the repository
    /// </summary>
    /// <param name="clientRepository"></param>
    internal class ClientService(IClientRepository clientRepository)
    {
        private const int stringLength = 32;

        public bool CreateClient(Client newClient)
        {

        }



        public void DeleteClient(Client newClient)
        {

        }
        private string CreateClientId()
        {
            string newClientId = RandomNumberGenerator.GetHexString(stringLength);

            // create 

            return newClientId;
        }

        private string CreateClientSecret(int stringLength)
        {
            string newClientSecret = RandomNumberGenerator.GetHexString(64);

            return $"private_{newClientSecret}";
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Scope
{
    public interface IScopeRepository
    {
        Task<Scope> Create(Scope scope);
        Task<Scope> GetByName(string name);
        Task<IEnumerable<Scope>> GetScopes(string[] scopes);
        Task<IEnumerable<Scope>> GetAllScopes();
        Task<bool> Update(Scope scope);
        Task<bool> Delete(string name);
    }
}

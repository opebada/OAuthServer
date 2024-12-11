using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Scope;

public class ScopeRepository : IScopeRepository
{
    private readonly IDictionary<string, Scope> _scopes;

    public ScopeRepository()
    {
        _scopes = new Dictionary<string, Scope>
        {
            { "read", new Scope { Name = "read", Description = "read" } }
        };
    }

    public async Task<Scope> Create(Scope scope)
    {
        ArgumentNullException.ThrowIfNull(scope);
        ArgumentException.ThrowIfNullOrWhiteSpace(scope.Name);

        if (_scopes.ContainsKey(scope.Name))
            throw new ArgumentException($"{scope.Name} Scope already exists", nameof(scope));

        _scopes.Add(scope.Name, scope);

        return await Task.FromResult(scope);
    }

    public async Task<bool> Delete(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        if (!_scopes.ContainsKey(name))
            return await Task.FromResult(false);

        _scopes.Remove(name);
        return await Task.FromResult(true);
    }

    public async Task<IEnumerable<Scope>> GetAllScopes()
    {
        return await Task.FromResult(_scopes.Values);
    }

    public async Task<Scope> GetByName(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        if (!_scopes.ContainsKey(name))
            return await Task.FromResult<Scope>(null);

        return await Task.FromResult(_scopes[name]);
    }

    public async Task<IEnumerable<Scope>> GetScopes(string[] scopes)
    {
        var requestedScopes = scopes.Where(x => _scopes.ContainsKey(x)).Select(y => _scopes[y]);

        return await Task.FromResult(requestedScopes);
    }

    public async Task<bool> Update(Scope scope)
    {
        ArgumentNullException.ThrowIfNull(scope);
        ArgumentException.ThrowIfNullOrWhiteSpace(scope.Name);

        if (!_scopes.ContainsKey(scope.Name))
            return await Task.FromResult(false);

        _scopes[scope.Name] = scope;

        return await Task.FromResult(true);
    }
}

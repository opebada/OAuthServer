using System;
using System.Collections.Generic;

namespace Core.Client;

public class ClientApplication
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public ClientType ClientType { get; set; }
    public IEnumerable<RedirectUrl>? RedirectUrls { get; set; }
    public string? ClientSecret { get; set; }
}


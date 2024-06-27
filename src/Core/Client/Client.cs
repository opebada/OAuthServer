using System;
using System.Collections.Generic;

namespace Core.Client;

public class Client
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public ClientType ClientType { get; set; }
    public required IEnumerable<RedirectUrl> RedirectUrls { get; set; }
    public string? ClientSecret { get; set; }
}


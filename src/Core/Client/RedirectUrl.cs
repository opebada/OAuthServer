using System;
namespace Core.Client
{
    public class RedirectUrl
    {
        public int Id { get; set; }
        public required string ClientId { get; set; }
        public required string Value { get; set; }
    }
}


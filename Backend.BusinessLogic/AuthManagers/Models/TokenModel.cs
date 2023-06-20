using System;

namespace Backend.ApiModels
{
    public class TokenModel
    {
        public string Token { get; set; }
        public DateTimeOffset ExpiredAt { get; set; }
    }
}
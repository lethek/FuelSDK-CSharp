using System;
using System.ComponentModel.DataAnnotations;

namespace FuelSDK;

public record FuelSettings
{
    [Required]
    public string AppSignature { get; init; }
    [Required]
    public string ClientId { get; init; }
    public string ClientSecret { get; init; }
    public string SoapEndPoint { get; init; }
    public string AuthEndPoint { get; init; }
    public string RestEndPoint { get; init; }
    public bool UseOAuth2Authentication { get; init; } = false;
    public string AccountId { get; init; } = "";
    public string Scope { get; init; } = "";
    public string ApplicationType { get; init; } = "server";
    public string AuthorizationCode { get; init; } = "";
    public string RedirectURI { get; init; } = "";
    public string Jwt { get; init; }
}

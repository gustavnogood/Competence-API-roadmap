using System;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System.Collections.Generic;
using System.Linq;

namespace RoadmapFunctionApp
{
    public class TokenValidator : ITokenValidator
    {
        public void ValidateToken(string accessToken)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var validationParameters = GetValidationParameters();

            try
            {
                tokenHandler.ValidateToken(accessToken, validationParameters, out _);
            }
            catch (Exception)
            {
                // Handle the exception with 401 unauthroized
                throw new UnauthorizedAccessException();

            }
        }

        private static TokenValidationParameters GetValidationParameters()
        {
            return new TokenValidationParameters
            {
                ValidIssuer = "https://login.microsoftonline.com/e719b1aa-cadf-4f70-a3fd-6a97e57b4e8b/v2.0",
                ValidAudience = "8dd0c61f-09c3-448b-81a2-26a25935ab3b",
                IssuerSigningKeys = GetSigningKeys()
            };
        }

        private static IEnumerable<SecurityKey> GetSigningKeys()
        {
            var configurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(
                "https://login.microsoftonline.com/e719b1aa-cadf-4f70-a3fd-6a97e57b4e8b/v2.0/.well-known/openid-configuration",
                new OpenIdConnectConfigurationRetriever());

            var configuration = configurationManager.GetConfigurationAsync().Result;
            return configuration.SigningKeys;
        }

        public UserInfo GetUserInfoFromToken(string accessToken)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(accessToken);

            // Extract user info from jwtToken.Claims and return it
            // This depends on how your access tokens are structured
            var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier);
            var emailClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Email);

            if (userIdClaim == null || emailClaim == null)
            {
                throw new UnauthorizedAccessException();
            }

            return new UserInfo
            {
                UserId = userIdClaim.Value,
                Email = emailClaim.Value
            };
        }
    }

    public class UserInfo
    {
        public string UserId { get; internal set; }
        public string Email { get; internal set; }
    }
}
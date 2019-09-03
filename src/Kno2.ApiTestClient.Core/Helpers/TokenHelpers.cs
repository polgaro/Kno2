using System;
using System.IO;
using System.Reflection;
using Kno2.ApiTestClient.Core.Extensions;
using Kno2.ApiTestClient.Core.Resources;

namespace Kno2.ApiTestClient.Core.Helpers
{
    public static class TokenHelpers
    {
        private const ConsoleColor RefreshTokenConsoleColor = ConsoleColor.DarkYellow;

        public static IToken GetRefreshToken(MediaType defaultMediaType = MediaType.json, string tokenFile = "access-token.json")
        {
            string path = tokenFile.AsAppPath(appNamePrefix: false);

            return File.Exists(path)
                ? ApiHelper.Deserialize<AuthResponse>(File.ReadAllText(path), defaultMediaType) 
                : new AuthResponse();
        }

        public static void Save(this IToken authResponse, MediaType defaultMediaType = MediaType.json, string tokenFile = "access-token.json")
        {
            string path = tokenFile.AsAppPath(appNamePrefix: false);

            ("   Saving Token to " + path).ToConsole();
            
            if (authResponse.AccessToken == null && !string.IsNullOrEmpty(authResponse.Error))
            {
                $"   X getting access token. Error: {authResponse.Error} - {authResponse.ErrorDescription}".ToConsole();
                throw new Exception(authResponse.Error);
            }

            ("   √ saving access token » " + authResponse.AccessToken.Substring(0, 15) + " ...").ToConsole();
            ("   √ saving refresh token » " + authResponse.RefreshToken).ToConsole();
            ("   √ saving expires » " + authResponse.Expires).ToConsole();

            File.WriteAllText(path, ApiHelper.Serialize(authResponse, defaultMediaType));
        }
    }
}
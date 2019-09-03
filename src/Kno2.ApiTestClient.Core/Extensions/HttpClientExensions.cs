using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using Kno2.ApiTestClient.Core.Helpers;
using Newtonsoft.Json.Linq;

namespace Kno2.ApiTestClient.Core.Extensions
{
    public static class HttpClientExensions
    {
        public async static void CheckStatus(this HttpResponseMessage httpResponseMessage)
        {
            if (!httpResponseMessage.IsSuccessStatusCode)
            {
                var errorColor = ConsoleColor.Red;
                StringBuilder errorMsg = new StringBuilder();
                errorMsg.AppendFormat("{0} {1}\r\n", httpResponseMessage.StatusCode, httpResponseMessage.ReasonPhrase);
                errorMsg.AppendFormat("{0} {1}\r\n", httpResponseMessage.RequestMessage.Method, httpResponseMessage.RequestMessage.RequestUri);

                if (httpResponseMessage.RequestMessage.Content != null)
                {
                    foreach (var header in httpResponseMessage.RequestMessage.Content.Headers)
                    {
                        errorMsg.AppendFormat(" - {0}: {1}\r\n", header.Key, string.Join((string) ",", (IEnumerable<string>) header.Value));
                    }
                }
                string messageBody = await httpResponseMessage.Content.ReadAsStringAsync();
                
                //Write the body if we have one
                if (!string.IsNullOrWhiteSpace(messageBody))
                {
                    if (httpResponseMessage.RequestMessage.Content != null && httpResponseMessage.Content.Headers.ContentType.MediaType.Contains("application/json"))
                    {
                        JObject jToken = JObject.Parse(messageBody);
                        foreach (var token in jToken)
                        {
                            errorMsg.AppendFormat(" - {0}: {1}\r\n", token.Key, token.Value);
                        }
                    }
                    else
                    {
                        //just output it to the screen
                        errorMsg.AppendFormat("Response: {0}\r\n", messageBody);
                    }
                }

                errorMsg.ToString().ToConsole();



                throw new Exception(errorMsg.ToString());
            }
        }
    }
}
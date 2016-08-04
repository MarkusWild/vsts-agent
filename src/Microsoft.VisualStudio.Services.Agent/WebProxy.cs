using Microsoft.VisualStudio.Services.Common;
using System;
using System.Net;
using System.IO;

namespace Microsoft.VisualStudio.Services.Agent
{
    public class WebProxy : IWebProxy
    {
        private static bool _proxySettingsApplied = false;

        public WebProxy(Uri proxyAddress)
        {
            if (proxyAddress == null)
            {
                throw new ArgumentNullException(nameof(proxyAddress));
            }

            ProxyAddress = proxyAddress;
        }

        public Uri ProxyAddress { get; private set; }

        public ICredentials Credentials { get; set; }

        public Uri GetProxy(Uri destination) => ProxyAddress;

        public bool IsBypassed(Uri uri)
        {
            return uri.IsLoopback;
        }

        public static void ApplyProxySettings(string proxyConfigFile)
        {
            if (_proxySettingsApplied)
            {
                return;
            }

            string proxy = Environment.GetEnvironmentVariable("VSTS_HTTP_PROXY");
            if (!string.IsNullOrEmpty(proxy))
            {
                string username = null;
                string password = null;
                if (!string.IsNullOrEmpty(proxyConfigFile) && File.Exists(proxyConfigFile))
                {
                    var proxyConfig = File.ReadAllLines(proxyConfigFile);
                    username = proxyConfig[0];
                    password = proxyConfig[1];
                }

                ICredentials cred = null;
                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                {
                    cred = CredentialCache.DefaultNetworkCredentials;
                }
                else
                {
                    cred = new NetworkCredential(username, password);
                }

                VssHttpMessageHandler.DefaultWebProxy = new WebProxy(new Uri(proxy))
                {
                    Credentials = cred
                };
            }

            _proxySettingsApplied = true;
        }
    }
}

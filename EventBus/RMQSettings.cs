using System;
using Microsoft.Extensions.Configuration;

namespace EventBus
{
    public class RMQSettings
    {
        public Uri Uri { get; set; }
        public int Port { get; set; }
        public bool IsSslEnabled { get; set; }
        public bool IsTraceEnabled { get; set; }
        public string VirtualHost { get; set; }
        public int PrefetchCount { get; set; }
        public string SslServerName { get; set; }
        public string CertificateThumbprint { get; set; }
        public TimeSpan RequestHeartBeat { get; set; }

        public IConfiguration _config;

        public RMQSettings(IConfiguration config)
        {
            _config = config;

            Port = Convert.ToInt32(_config.GetSection("Port").Value);
            SslServerName = _config.GetSection("SslServerName").Value;
            Uri = new Uri(_config.GetSection("Uri").Value);
            IsSslEnabled = Convert.ToBoolean(_config.GetSection("IsSslEnabled").Value);
            VirtualHost = _config.GetSection("VirtualHost").Value;
            PrefetchCount = Convert.ToInt32(_config.GetSection("PrefetchCount").Value);
            RequestHeartBeat = TimeSpan.FromSeconds(Convert.ToDouble(_config.GetSection("RequestHeartBeat").Value));
        }
    }
}

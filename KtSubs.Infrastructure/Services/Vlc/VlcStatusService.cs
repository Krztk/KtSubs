using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Text;
using System.Xml;

namespace KtSubs.Infrastructure.Services.Vlc
{
    public class VlcStatusService
    {
        private readonly HttpClient httpClient;
        private string statusUrl = string.Empty;
        private string pauseUrl = string.Empty;
        private string credentialsBase64 = string.Empty;

        public VlcStatusService(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public async Task<int> GetTimeMs()
        {
            var resultXML = await GetStatus();

            var length = int.Parse(resultXML.SelectSingleNode("/root/length").InnerText);
            var position = double.Parse(resultXML.SelectSingleNode("/root/position").InnerText, CultureInfo.InvariantCulture);
            var subtitledelayNode = resultXML.SelectSingleNode("/root/subtitledelay");
            var subtitleDelayMS = 0D;
            if (subtitledelayNode != null)
                subtitleDelayMS = double.Parse(subtitledelayNode.InnerText, CultureInfo.InvariantCulture) * 1000;

            int timeInMs = (int)(length * position * 1000 - subtitleDelayMS);
            return timeInMs;
        }

        public void SetAccessSettings(string password, int port)
        {
            statusUrl = $"http://127.0.0.1:{port}/requests/status.xml";
            pauseUrl = $"http://127.0.0.1:{port}/requests/status.xml?command=pl_pause";
            var username = string.Empty;
            var byteArray = Encoding.ASCII.GetBytes($"{username}:{password}");
            credentialsBase64 = Convert.ToBase64String(byteArray);
        }

        public async Task Play()
        {
            var status = await GetStatus();
            var state = status.SelectSingleNode("/root/state").InnerText;
            if (state == "playing" || state == "stopped")
                return;

            await TogglePlayPause();
        }

        /// <summary>
        /// Pauses VLC player via API
        /// </summary>
        /// <returns>true if the player was active and then stopped, otherwise false</returns>
        public async Task<bool> Pause()
        {
            var status = await GetStatus();
            var state = status.SelectSingleNode("/root/state").InnerText;
            if (state == "paused" || state == "stopped")
                return false;

            await TogglePlayPause();
            return true;
        }

        private async Task<XmlDocument> GetStatus()
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentialsBase64);
            return await GetVlcResponse(statusUrl);
        }

        private async Task<XmlDocument> TogglePlayPause()
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentialsBase64);
            return await GetVlcResponse(pauseUrl);
        }

        private async Task<XmlDocument> GetVlcResponse(string url)
        {
            try
            {
                HttpResponseMessage response = await httpClient.GetAsync(url);
                HttpContent content = response.Content;
                int numericStatusCode = (int)response.StatusCode;
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    var message = $"Something went wrong. Status Code: {numericStatusCode}";
                    if (response.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        message = "Unauthorized. Verify your credentials";
                    }

                    throw new VlcConnectionException(message);
                }
                string xmlString = await content.ReadAsStringAsync();
                var resultXML = new XmlDocument();
                resultXML.LoadXml(xmlString);
                return resultXML;
            }
            catch (HttpRequestException exception)
            {
                if (exception.InnerException is SocketException socketException)
                {
                    if (socketException.SocketErrorCode == SocketError.ConnectionRefused)
                    {
                        throw new VlcConnectionException("Check if your VLC media player and its HTTP interface are running. If you've just enabled the HTTP interface you should restart VLC media player.", exception);
                    }
                }

                throw new VlcConnectionException("Something went wrong.", exception);
            }
        }
    }
}
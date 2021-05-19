using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace GTStarCharts.TravellerMap
{
    public class MapAPI : IDisposable
    {
        WebClient JsonClient;
        WebClient ImageClient;

        public const string TravellerMap = "https://travellermap.com";

        public MapAPI()
        {
            JsonClient = new WebClient();
            JsonClient.Headers.Add(HttpRequestHeader.Accept, "application/json");

            ImageClient = new WebClient();
        }

        public void Dispose()
        {
            JsonClient.Dispose();
            ImageClient.Dispose();
        }

        public List<MapSector> GetSectors()
        {
            try
            {
                var result = JsonClient.DownloadString($"{TravellerMap}/data");
                var response = Newtonsoft.Json.JsonConvert.DeserializeObject<MapResponse>(result);
                return response.Sectors.ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                return null;
            }
        }

        public MapSectorDataResponse GetSectorData(string milieu, string name)
        {
            try
            {
                var result = JsonClient.DownloadString($"{TravellerMap}/data/{name}/metadata?milieu={milieu}");
                var response = Newtonsoft.Json.JsonConvert.DeserializeObject<MapSectorDataResponse>(result);
                return response;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                return null;
            }
        }

        public MapSystemData GetWorldData(string milieu, string sectorCode, string hex)
        {
            try
            {
                var result = JsonClient.DownloadString($"{TravellerMap}/data/{sectorCode}/{hex}?milieu={milieu}");
                var response = Newtonsoft.Json.JsonConvert.DeserializeObject<MapSystemData>(result);
                return response;
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                return null;
            }
        }

        public async Task<MapImageData> GetSectorThumbnail(string milieu, string sectorCode)
        {
            try
            {
                int scale = 4;
                byte[] imageData = await ImageClient.DownloadDataTaskAsync($"{TravellerMap}/api/poster?sector={sectorCode}&options=25&scale={scale}&milieu={milieu}");
                string contentType = ImageClient.ResponseHeaders[HttpRequestHeader.ContentType];

                return new MapImageData
                {
                    Scale = scale,
                    ImageData = imageData,
                    ContentType = contentType
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                return null;
            }
        }

        public async Task<MapImageData> GetSectorImage(string milieu, string sectorCode)
        {
            try
            {
                int scale = 48;
                byte[] imageData = await ImageClient.DownloadDataTaskAsync($"{TravellerMap}/api/poster?sector={sectorCode}&scale={scale}&milieu={milieu}&options=8618");
                string contentType = ImageClient.ResponseHeaders[HttpRequestHeader.ContentType];

                return new MapImageData
                {
                    Scale = scale,
                    ImageData = imageData,
                    ContentType = contentType
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                return null;
            }
        }

        public async Task<MapImageData> GetSubsectorImage(string milieu, string sectorCode, string subsectorCode)
        {
            try
            {
                int scale = 128;
                byte[] imageData = await ImageClient.DownloadDataTaskAsync($"{TravellerMap}/api/poster?sector={sectorCode}&subsector={subsectorCode}&scale={scale}&milieu={milieu}");
                string contentType = ImageClient.ResponseHeaders[HttpRequestHeader.ContentType];

                return new MapImageData
                {
                    Scale = scale,
                    ImageData = imageData,
                    ContentType = contentType
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                return null;
            }
        }

        public async Task<MapImageData> GetJumpImage(string milieu, string sectorCode, string hex)
        {
            try
            {
                int jump = 4;
                int scale = 64;
                byte[] imageData = await ImageClient.DownloadDataTaskAsync($"{TravellerMap}/api/jumpmap?sector={sectorCode}&jump={jump}&scale={scale}&milieu={milieu}&hex={hex}");
                string contentType = ImageClient.ResponseHeaders[HttpRequestHeader.ContentType];

                return new MapImageData
                {
                    Scale = scale,
                    ImageData = imageData,
                    ContentType = contentType
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                return null;
            }
        }

    }
}

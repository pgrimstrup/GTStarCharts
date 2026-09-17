using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTStarCharts.TravellerMap;
using GTStarData;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace GTStarCharts.Controllers
{
    [Route("api/Images")]
    public class ImagesController : ControllerBase
    {
        const int MaxCacheAge = 1000 * 24 * 60 * 60; // 1000 days
        static readonly TimeSpan RetryInterval = TimeSpan.FromHours(1);

        readonly GTStarDbContext Data;
        readonly MapAPI Api;
        readonly ILogger Logger;

        public ImagesController(ILogger<ImagesController> logger, GTStarDbContext data, MapAPI api)
        {
            Data = data;
            Api = api;
            Logger = logger;
        }

        /// <summary>
        /// Returns the thumbnail for the SubSector. Parameters: {id:guid}
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Route("{id:guid}/Thumbnail")]
        [ResponseCache(Duration = MaxCacheAge, Location = ResponseCacheLocation.Any)]
        public async Task<IActionResult> GetThumbnail(Guid id)
        {
            try
            {
                // Currently, only Sectors have thumbnails
                var sector = Data.FindSector(id);
                if (sector == null)
                    return this.NotFound();

                if (sector.ThumbnailData == null)
                {
                    // Only attempt to get the image once every 24 hours
                    if (sector.ThumbnailAttempt.HasValue && DateTimeOffset.Now.Subtract(sector.ThumbnailAttempt.Value) < RetryInterval)
                        return this.NotFound();

                    var image = await Api.GetSectorThumbnail(sector.Milieu, sector.Code);
                    if (image != null)
                    {
                        sector.ThumbnailData = image.ImageData;
                        sector.ThumbnailContentType = image.ContentType;
                        sector.ThumbnailAttempt = DateTimeOffset.Now;
                        Data.SaveChanges();
                    }
                }

                if (sector.ThumbnailData == null)
                {
                    sector.ThumbnailAttempt = DateTimeOffset.Now;
                    Data.SaveChanges();
                    return this.NotFound();
                }

                return this.File(sector.ThumbnailData, sector.ThumbnailContentType);
            }
            catch (Exception ex)
            {
                Logger.LogError(1, ex, $"Error while downloading Thumbnail for Sector {id}");
                return this.StatusCode(500);
            }
        }

        /// <summary>
        /// Returns the Image for the Sector, Sub-sector or System. Parameters {id:guid} {hex:string}
        /// </summary>
        /// <param name="id"></param>
        /// <param name="hex"></param>
        /// <returns></returns>
        [Route("{id:guid}")]
        [ResponseCache(Duration = MaxCacheAge, Location = ResponseCacheLocation.Any)]
        public async Task<IActionResult> GetImage(Guid id)
        {
            try
            {
                    var sector = Data.FindSector(id);
                    if (sector != null)
                        return await GetSectorImage(sector);

                    var subsector = Data.FindSubsector(id);
                    if (subsector != null)
                        return await GetSubSectorImage(subsector);

                    var system = Data.FindSystemData(id);
                    if (system != null)
                        return await GetSystemImage(system);

                return NotFound();
            }
            catch(Exception ex)
            {
                Logger.LogError(1, ex, $"Error while downloading Image for Sector/Subsector/System {id}");
                return this.StatusCode(500);

            }
        }

        private async Task<IActionResult> GetSectorImage(Sector sector)
        {
            if (sector.ImageData == null)
            {
                // Only attempt to get the image once every 24 hours
                if (sector.ImageAttempt.HasValue && DateTimeOffset.Now.Subtract(sector.ImageAttempt.Value) < TimeSpan.FromHours(24))
                    return this.NotFound();

                var image = await Api.GetSectorImage(sector.Milieu, sector.Code);
                if (image != null)
                {
                    sector.ImageScale = image.Scale;
                    sector.ImageData = image.ImageData;
                    sector.ImageContentType = image.ContentType;
                    sector.ImageAttempt = DateTimeOffset.Now;
                    Data.SaveChanges();
                }
            }

            if (sector.ImageData == null)
            {
                sector.ImageAttempt = DateTimeOffset.Now;
                Data.SaveChanges();
                return this.NotFound();
            }

            return this.File(sector.ImageData, sector.ImageContentType);
        }

        private async Task<IActionResult> GetSubSectorImage(Subsector subsector)
        {
            if (subsector.ImageData == null)
            {
                var sector = Data.FindSector(subsector.SectorId);
                if (sector == null)
                    return NotFound();

                // Only attempt to get the image once every 24 hours
                if (subsector.ImageAttempt.HasValue && DateTimeOffset.Now.Subtract(subsector.ImageAttempt.Value) < TimeSpan.FromHours(24))
                    return this.NotFound();

                var image = await Api.GetSubsectorImage(sector.Milieu, sector.Code, subsector.Code);
                if (image != null)
                {
                    subsector.ImageScale = image.Scale;
                    subsector.ImageData = image.ImageData;
                    subsector.ImageContentType = image.ContentType;
                    subsector.ImageAttempt = DateTimeOffset.Now;
                    Data.SaveChanges();
                }
            }

            if (subsector.ImageData == null)
            {
                subsector.ImageAttempt = DateTimeOffset.Now;
                Data.SaveChanges();
                return this.NotFound();
            }

            return this.File(subsector.ImageData, subsector.ImageContentType);
        }

        private async Task<IActionResult> GetSystemImage(SystemData system)
        {
            if (system.JumpImageData == null)
            {
                // Only attempt to get the image once every 24 hours
                if (system.JumpImageAttempt.HasValue && DateTimeOffset.Now.Subtract(system.JumpImageAttempt.Value) < TimeSpan.FromHours(24))
                    return this.NotFound();

                var sector = Data.Sectors.Find(system.SectorId);
                if (sector == null)
                    return this.NotFound();

                var image = await Api.GetJumpImage(sector.Milieu, sector.Code, system.Hex);
                if (image != null)
                {
                    system.JumpImageScale = image.Scale;
                    system.JumpImageData = image.ImageData;
                    system.JumpImageContentType = image.ContentType;
                    system.JumpImageAttempt = DateTimeOffset.Now;
                    Data.SaveChanges();
                }
            }

            if (system.JumpImageData == null)
            {
                system.JumpImageAttempt = DateTimeOffset.Now;
                Data.SaveChanges();
                return this.NotFound();
            }

            return this.File(system.JumpImageData, system.JumpImageContentType);
        }
    }
}

/*
 *  OblivionAPI :: ImageCacheService
 *
 *  This service is used to download and cache NFT images, as well as generate low res thumbnails for faster UI loading.
 * 
 */

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using OblivionAPI.Config;
using OblivionAPI.Objects;

namespace OblivionAPI.Services {
    public class ImageCacheService {
        private readonly ILogger<ImageCacheService> _logger;
        private readonly IHttpClientFactory _httpFactory;

        public ImageCacheService(ILogger<ImageCacheService> logger, IHttpClientFactory httpFactory) {
            _logger = logger;
            _httpFactory = httpFactory;
        }

        public async Task<ImageCacheDetails> ImageCache(ChainID chainID, string nft, string uri, bool clearExisting) {
            if (clearExisting) ClearCachedImages(nft);
            var details = new ImageCacheDetails();

            try {
                _logger.LogDebug("Performing image caching for {Nft} on {ChainID}", nft, chainID);
                
                uri = uri.Replace(Globals.IPFS_RAW_PREFIX, Globals.IPFS_HTTP_PREFIX);

                var highResFile = $"{nft}_high";
                var lowResFile = $"{nft}_low";

                details.HighResImage = await GetHighRes(uri, highResFile);

                if (!string.IsNullOrEmpty(details.HighResImage))
                    details.LowResImage = await ConvertLowRes(highResFile, lowResFile);
                
                return details;
            } catch (Exception error) {
                _logger.LogError(error, "An exception occured performing image caching for {Nft} on {ChainID}", nft, chainID);
                return details;
            }
        }

        private async Task<string> GetHighRes(string uri, string file) {
            _logger.LogDebug("Getting high res image from {Uri}", uri);
            
            try {
                var highResFile = Path.Combine(Globals.IMAGE_CACHE_DIR, file);
                if (File.Exists(highResFile)) return Globals.IMAGE_CACHE_PREFIX + file;
                var client = _httpFactory.CreateClient();
                var response = await client.GetAsync(uri);
                var content = await response.Content.ReadAsStreamAsync();

                await using var highRes = File.Create(highResFile);
                await content.CopyToAsync(highRes);

                highRes.Close();
                await highRes.DisposeAsync();
                content.Close();
                await content.DisposeAsync();
                
                return Globals.IMAGE_CACHE_PREFIX + file;
            }
            catch (Exception error) {
                _logger.LogError(error, "An exception occured while retrieving high res image from {Uri}", uri);
                return null;
            }
        }

        private async Task<string> ConvertLowRes(string highResFile, string name) {
            try {
                var lowResFile = Path.Combine(Globals.IMAGE_CACHE_DIR, name);
                if (File.Exists(lowResFile)) return Globals.IMAGE_CACHE_PREFIX + name;
                var highFile = Path.Combine(Globals.IMAGE_CACHE_DIR, highResFile);
                await using var file = File.Open(highFile, FileMode.Open);

                var reduced = Image.FromStream(file);
                var bitmap = new Bitmap(Globals.REDUCED_IMAGE_WIDTH, Globals.REDUCED_IMAGE_HEIGHT);
                var graphic = Graphics.FromImage(bitmap);

                graphic.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphic.SmoothingMode = SmoothingMode.HighQuality;
                graphic.PixelOffsetMode = PixelOffsetMode.HighQuality;
                graphic.CompositingQuality = CompositingQuality.HighQuality;
                graphic.DrawImage(reduced, 0, 0, Globals.REDUCED_IMAGE_WIDTH, Globals.REDUCED_IMAGE_HEIGHT);

                bitmap.Save(lowResFile);

                file.Close();
                await file.DisposeAsync();
                bitmap.Dispose();

                return Globals.IMAGE_CACHE_PREFIX + name;
            } catch (ArgumentException error) {
                if (error.Message == "Parameter is not valid.") 
                    _logger.LogWarning("Failed to generate low res image for {HighResFile} - Likely a movie image", highResFile);
                else _logger.LogCritical(error, "Error triggers: {Error}", error.Message);
                return null;
            } catch (Exception error) {
                _logger.LogError(error, "An exception occured converting low res image for {HighResFile}", highResFile);
                return null;
            }
        }
        
        private static void ClearCachedImages(string nft) {
            var highResFile = Path.Combine(Globals.IMAGE_CACHE_DIR, $"{nft}_high");
            var lowResFile = Path.Combine(Globals.IMAGE_CACHE_DIR, $"{nft}_low");
            
            if (File.Exists(highResFile)) File.Delete(highResFile);
            if (File.Exists(lowResFile)) File.Delete(lowResFile);
        }
    }
}

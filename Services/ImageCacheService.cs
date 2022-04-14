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

        public async Task<ImageCacheDetails> ImageCache(ChainID chainID, string nft, string uri) {
            var details = new ImageCacheDetails();
            
            try {
                _logger.LogInformation("Performing image caching for {NFT} on {ChainID}", nft, chainID);
                uri = uri.Replace(Globals.IPFS_RAW_PREFIX, Globals.IPFS_HTTP_PREFIX);
                var client = _httpFactory.CreateClient();
                
                var response = await client.GetAsync(uri);

                var highResFile = Globals.TEMP_DIR + nft + "_high";
                var lowResFile = Globals.TEMP_DIR + nft + "_low";

                var content = await response.Content.ReadAsStreamAsync();

                await using (var highRes = File.Create(highResFile)) 
                    await content.CopyToAsync(highRes);
                
                var reduced = Image.FromStream(content);
                var bitmap = new Bitmap(Globals.REDUCED_IMAGE_WIDTH, Globals.REDUCED_IMAGE_HEIGHT);
                var graphic = Graphics.FromImage(bitmap);
                    
                graphic.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphic.SmoothingMode = SmoothingMode.HighQuality;
                graphic.PixelOffsetMode = PixelOffsetMode.HighQuality;
                graphic.CompositingQuality = CompositingQuality.HighQuality;
                graphic.DrawImage(reduced, 0, 0, Globals.REDUCED_IMAGE_WIDTH, Globals.REDUCED_IMAGE_HEIGHT);
                    
                bitmap.Save(lowResFile);
                
                return details;
            } catch (Exception error) {
                _logger.LogError(error, "An exception occured performing image caching for {NFT} on {ChainID}", nft, chainID);
                return details;
            }
        }
    }
}

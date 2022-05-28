/*
 *  OblivionAPI :: ImageCacheService
 *
 *  This service is used to download and cache NFT images, as well as generate low res thumbnails for faster UI loading.
 * 
 */

using System.Net.Http;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Processing;

namespace OblivionAPI.Services; 

public class ImageCacheService {
    private readonly ILogger<ImageCacheService> _logger;
    private readonly IHttpClientFactory _httpFactory;

    public ImageCacheService(ILogger<ImageCacheService> logger, IHttpClientFactory httpFactory) {
        _logger = logger;
        _httpFactory = httpFactory;
    }

    public async Task<ImageCacheDetails> ImageCache(ChainID chainID, string nft, string uri, uint id, bool clearExisting) {
        if (clearExisting) ClearCachedImages(nft, id);
        var details = new ImageCacheDetails();

        try {
            _logger.LogDebug("Performing image caching for {Nft} on {ChainID}", nft, chainID);
                
            uri = uri.Replace(Globals.IPFS_RAW_PREFIX, Globals.IPFS_HTTP_PREFIX);

            var highResFile = $"{nft}_{id}_high";
            var lowResFile = $"{nft}_{id}_low";

            details.HighResImage = await GetHighRes(uri, highResFile);

            if (!string.IsNullOrEmpty(details.HighResImage)) {
                var info = new FileInfo(Path.Combine(Globals.IMAGE_CACHE_DIR, highResFile));
                details.LowResImage = info.Length > 1048576 ? await ConvertLowRes(highResFile, lowResFile) : details.HighResImage;
            }

            return details;
        } catch (Exception error) {
            _logger.LogError(error, "An exception occured performing image caching for {Nft} on {ChainID}", nft, chainID);
            return details;
        }
    }

    private async Task<string> GetHighRes(string uri, string file) {
        try {
            var highResFile = Path.Combine(Globals.IMAGE_CACHE_DIR, file);
            if (File.Exists(highResFile)) return Globals.IMAGE_CACHE_PREFIX + file;
            _logger.LogDebug("Getting high res image from {Uri}", uri);
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

            using var reader = new StreamReader(highFile);
            var image = await Image.LoadWithFormatAsync(reader.BaseStream);

            if (image.Format == GifFormat.Instance) {
                _logger.LogWarning("GIF detected for NFT {Address} - low res cache not generated", name);
                reader.Close();
                reader.Dispose();
                image.Image.Dispose();
                return null;
            }
            
            await using var outFile = File.Create(lowResFile);
            
            await image.Image.SaveAsJpegAsync(outFile, new JpegEncoder { Quality = 75 });
                
            outFile.Close();
            await outFile.DisposeAsync();
            image.Image.Dispose();
            reader.Close();
            reader.Dispose();
            return Globals.IMAGE_CACHE_PREFIX + name;
        } catch (UnknownImageFormatException error) {
            if (error.Message.Contains("Image cannot be loaded")) 
                _logger.LogWarning("Failed to generate low res image for {HighResFile} - Likely a movie image", highResFile);
            else _logger.LogCritical(error, "Error triggers: {Error}", error.Message);
            return null;
        } catch (Exception error) {
            _logger.LogError(error, "An exception occured converting low res image for {HighResFile}", highResFile);
            return null;
        }
    }
        
    private static void ClearCachedImages(string nft, uint id) {
        var highResFile = Path.Combine(Globals.IMAGE_CACHE_DIR, $"{nft}_{id}_high");
        var lowResFile = Path.Combine(Globals.IMAGE_CACHE_DIR, $"{nft}_{id}_low");
            
        if (File.Exists(highResFile)) File.Delete(highResFile);
        if (File.Exists(lowResFile)) File.Delete(lowResFile);
    }
}
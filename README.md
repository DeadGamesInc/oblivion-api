Oblivion API

This repository is for the API that powers the UI for Oblivion by managing an in memory database of all the contract and other details, in order to provide caching and improve performance of lookups that are required by the UI.

This is developed in C# .NET 5.0 using ASP .NET WebAPI framework.

Configuration is done via system environment variables, to facilitate easy configuration in any platform, as well as sets defaults if the value is not set. 

Includes a Dockerfile for building this into an image for easy deployment.

The API runs in HTTP only mode.

** Environment Variables **

PORT - The port the web service should listen on. Default: 5001

CACHE_TIME - The number of minutes before data is considered stale. Default: 2

REFRESH_TIME - The number of milliseconds between data refreshes. Default: 300000

THROTTLE_WAIT - The number of milliseconds to pause in certain blockchain operations to avoid getting throttled by the node. Default: 500

REDUCED_IMAGE_WIDTH - The width of the thumbnails generated for the cache. Default: 100

REDUCED_IMAGE_HEIGHT - The height of the thumbnails generated for the cache. Default: 100

IMAGE_CACHE_PREFIX - The image cache URL prefix. Default: TBD
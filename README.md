# Lidarr.Plugin.AppleMusic

A Lidarr plugin for downloading music from Apple Music with ALAC lossless and Dolby Atmos support.

## Prerequisites

This plugin requires the [apple-music-stack](https://github.com/space192/apple-music-stack) running as a backend. The stack handles Apple Music authentication, DRM decryption, and audio format conversion.

## Installation

1. Deploy the [apple-music-stack](https://github.com/space192/apple-music-stack) on your server
2. Open Lidarr → **System → Plugins**
3. Enter this repository URL and install

## Configuration

### Indexer
- Go to **Settings → Indexers → Add → Apple Music**
- Set **API Base URL** to your apple-music-bridge address (e.g., `http://apple-music-bridge:8000`)

### Download Client
- Go to **Settings → Download Clients → Add → Apple Music**
- Set **API Base URL** to the same bridge address
- Set **Download Path** to your music download directory
- Set **Preferred Codec** (ALAC, AAC, or Dolby Atmos)
- Set **Output Format** (FLAC, M4A, WAV, or MP3)

## Building from source

```bash
# Install .NET 8.0 SDK
curl -sSL https://dot.net/v1/dotnet-install.sh | bash -s -- --version 8.0.405 --install-dir $HOME/.dotnet

# Clone with Lidarr submodule
git clone --recursive https://github.com/space192/Lidarr.Plugin.AppleMusic.git
cd Lidarr.Plugin.AppleMusic

# Build
PATH="$HOME/.dotnet:$PATH" dotnet build /p:TreatWarningsAsErrors=false /p:NuGetAudit=false

# Output: _plugins/Lidarr.Plugin.AppleMusic/Lidarr.Plugin.AppleMusic.dll
```

## License

MIT

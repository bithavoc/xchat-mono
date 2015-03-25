X-Chat Managed Plugins. This plugin allow you create plugins for x-chat using Mono and  any Mono Enabled Compiler incluing C# and VB.NET.

## How it works ##

  1. Copy xchat-mono.so in your xchat plugin directory(e.g `cp xchat-mono.so /usr/lib/xchat-gnome/plugins/`)
  1. Copy xchat-mono.dll in your mono xchat directory(e.g `cp xchat-mono.dll $HOME/.config/xchat/mono/`):
  1. Copy your Plugins(.dll) in Xchat Mono Plugins(e.g `cp xchat-mono-banshee.dll $HOME/.config/xchat/mono/plugins/`):

## How to install ##

You can simplify the installation process running:

  1. `make`
  1. `sudo make install`
  1. `sudo make install-cil`
  1. `make install-plugins`

Note: you will need install libc development files and Mono development, in Ubuntu/Debian these packages are: 'libc-dev' and 'mono-devel'

## How to compile and use managed plugins ##

  1. Compile the plugin: `gmcs -r:xchat-mono.dll MyPlugin.cs -target:library -out:MyPlugin.dll`

  1. Install the plugin in $HOME/.config/xchat/mono/plugins/ directory

## Need help? ##

Please post your question in the Discussion group at http://groups.google.com/group/xchat-mono or email me: thepumpkin1979 at gmail dot com
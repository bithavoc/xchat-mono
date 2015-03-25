# Introduction #

This text help you to build your fist XChat Managed Plugin with C# and Mono 1.2.4. The plugin purpose is register a command and return responses when the command is invoked.

## The Source Code ##

Create a file named basicHello.cs and fill the content with this code:

```
using XChat;

[XChatPlugin("HelloPlugin","Hello Plugin Inteface",AutoActivate=true)]
public class HelloPlugin : PluginBase
{
        protected override void Init()
        {
                this.RegisterCommand("hello","Hello World plugin Demo",delegate
                {
                        this.Context.PrintLine("Hello World...");
                });
        }
}

```

## The Compilation ##

```
gmcs -r:xchat-mono.dll -target:library hello-plugin.cs
```

## Testing ##
Copy your plugin into your Managed Plugin Directory.

<sup>cp hello-plugin.dll $HOME/.config/xchat/mono/plugins/</sup>

Run your xchat-gnome and write /hello!
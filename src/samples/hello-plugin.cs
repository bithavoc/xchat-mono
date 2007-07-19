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

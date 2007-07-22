using System;
using XChat;

namespace XChatMessageLoggin
{
	[XChatPlugin("MessageLoggin","Message Loggin",Author="Johan Hernandez(thepumpkin)",AuthorEmail="thepumpkin1979@gmail.com",AutoActivate=true)]
	public class MessageLoggin : PluginBase
	{

		protected override void Init()
		{
			this.Context.ChannelMessageReceived+=delegate(ChatContext context,MessageReceivedEventArgs e)
			{
				Console.WriteLine("Loggin Recivio: {0}",e.Content);
			};
		}
	}
}
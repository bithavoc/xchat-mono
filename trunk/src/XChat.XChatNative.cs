using System;
using System.Runtime.CompilerServices;

namespace XChat
{
	public static class XChatNative
	{
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern static void SendCommand(string commandText);
			
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern static void PrintLine(string text);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]		
		public extern static string GetCurrentChannelName();

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern static string GetCurrentNickname();

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern static void RegisterCommand(string commandName,string commandDescription);
		
		public static void OnCommand(string commandName)
		{
			if(ExecutingCommand != null)
			{
				ExecutingCommand(commandName);
			}
		}
		
		internal delegate void OnCommandCallback(string commandName);
		internal static event OnCommandCallback ExecutingCommand;
		
		public static void OnJoin(string nickName,string channelName)
		{
			//new PluginManager;
			if(MemberJoin != null)
			{
				MemberJoin(new MemberJoinedEventArgs(nickName,channelName));
			}
		}//OnJoin
		
		internal delegate void OnJoinEventHandler(MemberJoinedEventArgs args);
		internal static OnJoinEventHandler MemberJoin;
			
		internal class MemberJoinedEventArgs : EventArgs
		{
			private string nickname,channel;
			public MemberJoinedEventArgs(string nickname,string channel)
			{
				this.nickname = nickname;
				this.channel = channel;
			}//.ctor
			public string Nickname
			{
				get
				{
					return this.nickname;
				}
			}//Nickname
			public string ChannelName
			{
				get
				{
					return this.channel;
				}
			}//ChannelName
		}//MemberJoinedEventArgs
	}//XChats
}//ns
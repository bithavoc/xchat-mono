/***************************************************************************
 *  XChat.XChatNative.cs
 *
 *  Written by Johan Hernandez <thepumpkin1979@gmail.com>
 *  Web: http://johansoft.blogpost.com
 ****************************************************************************/

/*  THIS FILE IS LICENSED UNDER THE MIT LICENSE AS OUTLINED IMMEDIATELY BELOW: 
 *
 *  Permission is hereby granted, free of charge, to any person obtaining a
 *  copy of this software and associated documentation files (the "Software"),  
 *  to deal in the Software without restriction, including without limitation  
 *  the rights to use, copy, modify, merge, publish, distribute, sublicense,  
 *  and/or sell copies of the Software, and to permit persons to whom the  
 *  Software is furnished to do so, subject to the following conditions:
 *
 *  The above copyright notice and this permission notice shall be included in 
 *  all copies or substantial portions of the Software.
 *
 *  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
 *  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
 *  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
 *  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
 *  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING 
 *  FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
 *  DEALINGS IN THE SOFTWARE.
 */
 
using System;
using System.Runtime.CompilerServices;

namespace XChat
{
	internal static class XChatNative
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

		public delegate void onMessageHandler(string nickname,string message);
		public static event onMessageHandler onMessage;
		private static void OnMessage(string nickname,string message)//External call
		{
			if(onMessage != null) onMessage(nickname,message);
		}
		
		public static void OnCommand(string commandName,string arg1)
		{
			Console.WriteLine("Recibido arg1:{0}",arg1);
			if(ExecutingCommand != null)
			{
				string[] args = null;
				if(!string.IsNullOrEmpty(arg1))
				{
					args = new string[]{arg1};
				}
				else
				{
					args = new string[0];
				}
				ExecutingCommand(commandName,args);
			}
		}
		
		internal delegate void OnCommandCallback(string commandName,string[] args);
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

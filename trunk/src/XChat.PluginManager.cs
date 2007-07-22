/***************************************************************************
 *  XChat.PluginManager.cs
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
using System.Reflection;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.IO;

namespace XChat
{
	public class PluginManager
	{
		public static void Main()
		{
			
		}
		private Dictionary<string,PluginBase> plugins = new Dictionary<string,PluginBase>();
		internal ChatContext context;
		string baseDir;
		public PluginManager(string baseDir)
		{
			this.baseDir = baseDir;
			//Console.WriteLine("baseDir plugin maanger={0}",baseDir);
			//Console.WriteLine("Plugin Manager Initialized");
			this.context = new ChatContext();
			XChatNative.RegisterCommand("mono-plugins-list","Show mono plugins list");
			XChatNative.ExecutingCommand +=delegate(string name,string[] args){
				if(name.Equals("mono-plugins-list"))
				{
					context.PrintLine("{0} plugins loaded",this.plugins.Count);
					if(this.plugins.Count > 0)
					{
						foreach(PluginBase plugin in this.plugins.Values)
						{
							context.PrintLine("{0}{1}",plugin.Id,plugin.IsActivated?"(Activated)":string.Empty);
							
						}//foreach
					}//if
				}
			};
			LoadUserPlugins();
		}

		public void RegisterPlugin(string id,PluginBase plugin)
		{
			//plugin.Init(this);
			//TODO: Already added plugin id?
			plugins.Add(id,plugin);
			if(plugin.AutoActivate)
			{
				Activate(id);
			}
		}

		public void Activate(string pluginId)
		{
			//TODO: does the pluginId exists?
			PluginBase p = plugins[pluginId];
			p._init(this);
			p.activate();
		}

		public void LoadPluginFile(string libraryPath)
		{
			Assembly asm = Assembly.LoadFile(libraryPath);
			Type[] types = asm.GetExportedTypes();
			foreach(Type type in types)
			{
				object[] atts = type.GetCustomAttributes(typeof(XChatPluginAttribute),true);
				if(atts.Length > 0)
				{
					XChatPluginAttribute att = atts[0] as XChatPluginAttribute;
					PluginBase pluginInstance = (PluginBase)Activator.CreateInstance(type,new Object[]{});
					pluginInstance.AutoActivate = att.AutoActivate;
					pluginInstance.Id = att.Id;
					//Console.WriteLine(att.Id);
					this.RegisterPlugin(att.Id,pluginInstance);
				}
			}
			types = null;
		}//LoadPluginFile

		public ChatContext Context
		{
			get
			{
				return this.context;
			}
		}

		public void LoadUserPlugins()
		{
			//string homeDir = g_get_home_dir();//Environment.GetEnvironmentVariable("HOME");
			
			//FileInfo homeFile = new FileInfo(homeDir);
			//homeDir = homeFile.Directory.FullName;
			string pluginDirName = "plugins/";
			string pluginDirPath = Path.Combine(this.baseDir,pluginDirName);
			Console.WriteLine("Plugin Idr path:{0}",pluginDirPath);
			if(Directory.Exists(pluginDirPath ))
			{
				DirectoryInfo pluginDir = new DirectoryInfo(pluginDirPath);
				FileInfo[] files = pluginDir.GetFiles("*.dll");
				Console.WriteLine("Pkugin files located count:{0}",files.Length);
				foreach(FileInfo file in files)
				{
					LoadPluginFile(file.FullName);
				}
			}
		}//LoadUserPlugins

	}//PluginManager
	
	public class ChatContext
	{
		public ChatContext()
		{
			XChatNative.onMessage +=delegate(string nickname,string message)
			{
				if(this.ChannelMessageReceived != null)
				{
					this.ChannelMessageReceived(this,new MessageReceivedEventArgs(nickname,message));
				}
			};
		}
		public void SendCommand(string command)
		{
			XChatNative.SendCommand(command);
		}
		public void SendCommand(string format,params object[] args)
		{
			XChatNative.SendCommand(string.Format(format,args));
		}
		public void PrintLine(string text)
		{
			XChatNative.PrintLine(text);
		}
		public void PrintLine(string format,params object[] args)
		{
			XChatNative.PrintLine(string.Format(format,args));
		}
		public string Nickname
		{
			get
			{
				return XChatNative.GetCurrentNickname();
			}
		}
		
		public delegate void MessageReceivedEventHandler(ChatContext context,MessageReceivedEventArgs e);
		public event MessageReceivedEventHandler ChannelMessageReceived;

	}//ChatContext
	public class MessageReceivedEventArgs
	{
		private string nick,content;
		public MessageReceivedEventArgs(string nickname,string content)
		{
			this.nick = nickname;
			this.content = content;
		}
		public string SenderNickname
		{
			get
			{
				return this.nick;
			}
		}
		public string Content
		{
			get
			{
				return this.content;
			}
		}
	}
	public abstract class PluginBase
	{
		private ChatContext context;
		private PluginManager manager;
		private bool isActivated;
		private bool autoActivate;
		private Dictionary<string,CommandEventHandler> onCommandList = new  Dictionary<string,CommandEventHandler>();
			private string id;
		internal void _init(PluginManager manager)
		{
			this.manager = manager;
			this.context = manager.context;
			XChatNative.ExecutingCommand +=delegate(string name,string[] args){
				if(this.onCommandList.ContainsKey(name))
				{
					onCommandList[name](this,new CommandEventArgs(name,args));//Execute the callback.
				}
			};
			Init();
		}
		protected abstract void Init();
			
		internal void activate()
		{
			this.isActivated = true;
		}
		/*internal void deactivate()
		{
			this.isActivated = false;
		}*/
		public string Id
		{
			get
			{
				return this.id;
			}
			internal set
			{
				this.id = value;
			}
		}
		public ChatContext Context
		{
			get
			{
				return this.context;
			}
		}
		protected PluginManager Manager
		{
			get
			{
				return this.manager;
			}
		}
		public bool IsActivated
		{
			get
			{
				return this.isActivated;
			}
		}
		
		public bool AutoActivate
		{
			get
			{
				return this.autoActivate;
			}
			set
			{
				this.autoActivate = value;
			}
		}
		public class CommandEventArgs
		{
			private string name;
			private string[] args;

			public CommandEventArgs(string command,string[] args)
			{
				this.name = command;
				this.args = args;
			}
			public string Name
			{
				get
				{
					return this.name;
				}
			}
			public string[] Arguments
			{
				get
				{
					return this.args;
				}
			}
		}
		public delegate void CommandEventHandler(PluginBase plugin,CommandEventArgs e);
		protected void RegisterCommand(string name,string description,CommandEventHandler callback)
		{
			if(callback == null)throw new Exception(string.Format("Command {0} can not be registered with a null callback",name));
			if(onCommandList.ContainsKey(name)) throw new Exception(string.Format("Command {0} already registered",name));
				onCommandList.Add(name,callback);
			XChatNative.RegisterCommand(name,description);
		}

	}//PluginBase
	
	[AttributeUsage(AttributeTargets.Class)]
	public class XChatPluginAttribute : Attribute
	{
		private string id, description, author, authorEmail;
		private bool autoActivate;
		public XChatPluginAttribute(string id,string description)
		{
			this.id = id;
			this.description = description;
			this.author = string.Empty;
			this.authorEmail = string.Empty;
		}
		public string Id
		{
			get
			{
				return this.id;
			}
		}
		public bool AutoActivate
		{
			get
			{
				return this.autoActivate;
			}
			set
			{
				this.autoActivate = value;
			}
		}
		public string Author
		{
			get
			{
				return this.author;
			}
			set
			{
				this.author = value;
			}
		}
		public string AuthorEmail
		{
			get
			{
				return this.authorEmail;
			}
			set
			{
				this.authorEmail = value;
			}
		}
	}//XChatPluginAttribute
}

using System;
using System.Reflection;
using System.Collections.Generic;
using System.IO;

namespace XChat
{
	public class PluginManager
	{
		private Dictionary<string,PluginBase> plugins = new Dictionary<string,PluginBase>();
		internal ChatContext context;
		public PluginManager()
		{
			Console.WriteLine("Plugin Manager Initialized");
			this.context = new ChatContext();
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
					Console.WriteLine(att.Id);
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
			string homeDir = Environment.GetEnvironmentVariable("HOME");
			
			FileInfo homeFile = new FileInfo(homeDir);
			homeDir = homeFile.Directory.FullName;
			string pluginDirName = "plugins/";
			string pluginDirPath = Path.Combine(homeDir,pluginDirName);
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
		}
	}//PluginManager
	
	public class ChatContext
	{
		public void SendCommand(string command)
		{
			XChatNative.SendCommand(command);
		}
		public void PrintLine(string text)
		{
			XChatNative.PrintLine(text);
		}
		public string Nickname
		{
			get
			{
				return XChatNative.GetCurrentNickname();
			}
		}
	}//ChatContext
	
	public abstract class PluginBase
	{
		private ChatContext context;
		private PluginManager manager;
		private bool isActivated;
		private bool autoActivate;
		private Dictionary<string,OnCommandEventHandler> onCommandList = new  Dictionary<string,OnCommandEventHandler>();
		internal void _init(PluginManager manager)
		{
			this.manager = manager;
			this.context = manager.context;
			XChatNative.ExecutingCommand +=delegate(string name){
				if(this.onCommandList.ContainsKey(name))
				{
					onCommandList[name](name);//Execute the callback.
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
		public delegate void OnCommandEventHandler(string commandName);
		protected void RegisterCommand(string name,string description,OnCommandEventHandler callback)
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
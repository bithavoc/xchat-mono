using System;
using NDesk.DBus;
using org.freedesktop.DBus;
using XChat;

namespace BansheeXChat
{
	[Interface("org.gnome.Banshee.Core")]
	public interface IPlayer
	{
	    void Shutdown();
	    void PresentWindow();
	    void ShowWindow();
	    void HideWindow();
	    void TogglePlaying();
	    void Play();
	    void Pause();
	    void Next();
	    void Previous();
	    void SelectAudioCd(string device);
	    void SelectDap(string device);
	    void EnqueueFiles(string [] files);
	    string GetPlayingArtist();
	    string GetPlayingAlbum();
	    string GetPlayingTitle();
	    string GetPlayingGenre();
	    string GetPlayingUri();
	    string GetPlayingCoverUri();
	    int GetPlayingDuration();
	    int GetPlayingPosition();
	    int GetPlayingRating();
	    int GetMaxRating();
	    int SetPlayingRating(int rating);
	    int GetPlayingStatus();
	    void SetVolume(int volume);
	    void IncreaseVolume();
	    void DecreaseVolume();
	    void SetPlayingPosition(int position);
	    void SkipForward();
	    void SkipBackward();
	}
    
	 
	[XChatPlugin("banshee","Banshee Music Player",Author="Johan Hernandez(thepumpkin)",AuthorEmail="thepumpkin1979@gmail.com",AutoActivate=true)]
	public class MusicChatPlugin : PluginBase
	{
		const string BUS_NAME = "org.gnome.Banshee";
    const string OBJECT_PATH = "/org/gnome/Banshee/Player";

 private static IPlayer banshee = null;
	 bool ensureBansheeConnection()
	{
		if(banshee == null)
		{
			try 
			{
				banshee = FindInstance();
				BusG.Init();
				return true;
			} catch(Exception) {
				this.PrintLine("Banshee is not running");
				//Environment.Exit(1);
			}
		}
		return false;
	}
	protected override void Init()
	{
		//Console.WriteLine("Banshee There");
		this.RegisterCommand("banshee","Banshee Current Track",delegate
		{
			if(ensureBansheeConnection())
			{
				QueryServer();
			}
		});
	}
	static IPlayer FindInstance()
	{
		if (!Bus.Session.NameHasOwner(BUS_NAME))
			throw new Exception(String.Format("Name {0} has no owner", BUS_NAME));
		return Bus.Session.GetObject<IPlayer>(BUS_NAME, new ObjectPath (OBJECT_PATH));
	}

	private static string last_uri = null;
	private bool QueryServer()
	    {
		int status = -1;
		
		try {
		    status = banshee.GetPlayingStatus();
		} catch(Exception) {
		    this.PrintLine("Lost connection to Banshee Server");
		    return false;
		}
		
		switch(status) {
		    case 0:
			//myMonoClass.sayHello("Paused");
			break;
		    case 1:
			//myMonoClass.sayHello("Playing");
			break;
		    case -1:
		    default: 
			//myMonoClass.sayHello("NO SONG LOADED");
			return true;
		}
		
		string uri = banshee.GetPlayingUri();
		
		      if(uri != last_uri) {
		    last_uri = uri;
		}
		XChatNative.SendCommand(
			string.Format("me is listening '{0}' - '{1}' (http://johansoft.blogspot.com/2007/07/x-chat-mono.html)", 
				banshee.GetPlayingTitle(),
				banshee.GetPlayingArtist(),
				XChatNative.GetCurrentNickname()));  
	       
		return true;
	    }
	}
}
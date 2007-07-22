/***************************************************************************
 *  BansheeXChat.cs
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
	void resetBanshee()
	{
		banshee = null;
		ensureBansheeConnection();
	}
	 bool ensureBansheeConnection()
	{
		if(banshee == null)
		{
			try 
			{
				banshee = FindInstance();
				BusG.Init();
				return true;
			} catch(Exception ex) {
				Console.WriteLine(ex.ToString());
				this.Context.PrintLine("Banshee is not running");
				return false;
			}
		}
		return true;
	}
	protected override void Init()
	{
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
	private void QueryServer()
	    {
		int status = -1;
		
		try {
		    status = banshee.GetPlayingStatus();
		} catch(Exception) {
		    this.Context.PrintLine("Lost connection to Banshee");
			resetBanshee();
		    return;
		}
		
		switch(status) {
		    case 0:
			this.Context.PrintLine("Banshee is paused");
		    return;
		}
		
		string uri = banshee.GetPlayingUri();
		
		      if(uri != last_uri) {
		    last_uri = uri;
		}
		this.Context.SendCommand(
			string.Format("me is listening '{0}' - '{1}' (xchat-mono Banshee Plugin)", 
				banshee.GetPlayingTitle(),
				banshee.GetPlayingArtist(),
				this.Context.Nickname));  
	       
		return;
	    }
	}
}
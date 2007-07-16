/***************************************************************************
 *  Mono Embedding Sample
 *  Copyright (C) 2007 Johan Hernandez
 *  Written by Johan Hernandez <thepumpkin1979@gmail.com> 
 *  Web: http://johansoft.blogspot.com
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
using NDesk.DBus;
using org.freedesktop.DBus;
using XChat;

class MainC
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
    const string BUS_NAME = "org.gnome.Banshee";
    const string OBJECT_PATH = "/org/gnome/Banshee/Player";

 private static IPlayer banshee = null;

    static IPlayer FindInstance()
    {
        if (!Bus.Session.NameHasOwner(BUS_NAME))
            throw new Exception(String.Format("Name {0} has no owner", BUS_NAME));

        return Bus.Session.GetObject<IPlayer>(BUS_NAME, new ObjectPath (OBJECT_PATH));
    }
	public static void Main()
	{
		 try {
	            banshee = FindInstance();
	        } catch(Exception) {
	            Console.Error.WriteLine("Could not locate Banshee on D-Bus. Perhaps it's not running?");
	            Environment.Exit(1);
	        }
	        
	        BusG.Init();
		QueryServer();
		XChatNative.MemberJoin+=delegate(XChatNative.MemberJoinedEventArgs args) {
			Console.WriteLine("CLR: {0} has just joined to {1}: ",args.Nickname,args.ChannelName);
		};
		//GLib.Timeout.Add(500, QueryServer);
//		myMonoClass.sayHello("Paso?");
		//Console.WriteLine("CLRCode: Hello World!");
//Console.ReadLine();
	}
  private static string last_uri = null;
 private static bool QueryServer()
    {
        int status = -1;
        
        try {
            status = banshee.GetPlayingStatus();
        } catch(Exception) {
            Console.Error.WriteLine("Lost connection to Banshee Server");
          
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
            
             //myMonoClass.sayHello(string.Format("Song Changed: {0}", uri));
     
//            SetField("Artist", banshee.GetPlayingArtist(), artist_label);
//            SetField("Album", banshee.GetPlayingAlbum(), album_label);
//            SetField("Title", banshee.GetPlayingTitle(), title_label);
        }
        XChatNative.SendCommand(
		string.Format("me is listening '{0}' - '{1}' (http://johansoft.blogspot.com/2007/07/x-chat-mono.html)", 
			banshee.GetPlayingTitle(),
			banshee.GetPlayingArtist(),
			XChatNative.GetCurrentNickname()));  
       
        return true;
    }
}


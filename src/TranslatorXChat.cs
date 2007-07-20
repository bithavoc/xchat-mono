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
using XChat;
using System.Net;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace XChatTranslator
{
	[XChatPlugin("translator","translator",Author="Johan Hernandez(thepumpkin)",AuthorEmail="thepumpkin1979@gmail.com",AutoActivate=true)]
	public class TranslatorPlugin : PluginBase
	{
		protected override void Init()
		{
			this.RegisterCommand("translate","Translate some text",delegate(PluginBase plugin,CommandEventArgs args)
			{
				if(args.Arguments.Length == 0)
				{
					Context.PrintLine("ok... y que vas a traducir?");
					return;
				}
				string toTranslate = args.Arguments[0];
							WebRequest request;
			Stream dataStream;
			request = WebRequest.Create("http://babelfish.altavista.com/tr");
			request.Method = "POST";
			
			string postData = string.Format("trtext={0},&lp={1}",toTranslate,"en_es");
			byte[] byteArray = Encoding.UTF8.GetBytes (postData);
			request.ContentType = "application/x-www-form-urlencoded";
			    request.ContentLength = byteArray.Length;
					
			    dataStream = request.GetRequestStream ();
			    dataStream.Write (byteArray, 0, byteArray.Length);
			    dataStream.Close ();

			    // Get the response.
			    WebResponse response = request.GetResponse ();

			    // Get the stream containing content returned by the server.
			    dataStream = response.GetResponseStream ();
			    // Open the stream using a StreamReader for easy access.
			    StreamReader reader = new StreamReader (dataStream);
			 
			       // Read the content.
			    string responseFromServer = reader.ReadToEnd ();
			 
			    // Clean up the streams.
			    reader.Close ();
			    dataStream.Close ();
			    response.Close ();
			
			string translatedText = getTranslatedText(responseFromServer);
				Context.SendCommand("me translates '{0}' to '{1}'",toTranslate,translatedText);
			});
		}
		private static string getTranslatedText(string serverResponse)
		{			
			Match m = Regex.Match(serverResponse, "<td bgcolor=white class=s><div style=padding:10px;>.*</div></td>");
				
			string aux = Regex.Replace(m.ToString(), "<.*?>", "");
			
			return aux;
		}
	}
}
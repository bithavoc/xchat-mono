/***************************************************************************
 *  xchat-mono.c
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
 
#include "xchat-plugin.h"
#include <stdio.h>
#include <string.h>
#include <mono/jit/jit.h>
#include <mono/metadata/mono-config.h>
#include <mono/metadata/assembly.h>
#include <mono/metadata/debug-helpers.h>
#include <mono/metadata/mono-debug.h>
#include <mono/metadata/metadata.h>
#include <stdlib.h>
#include <glib.h>
#include <stdlib.h>
#define PNAME "Mono"
#define PDESC "Mono Plugin Loader";
#define PVERSION "0.1"

xchat_plugin *ph;   /* plugin handle */
MonoDomain * dom ;
MonoAssembly *masm;
MonoImage  * mimg;

// CLI Internal Calls
void XChat_XChatNative_SendCommand(MonoString *); 
MonoString * XChat_XChatNative_GetCurrentChannelName();
MonoString * XChat_XChatNative_GetCurrentNickname();
void XChat_XChatNative_OnJoin(char * nickName,char * channelName);
void XChat_XChatNative_RegisterCommand(MonoString * commandName,MonoString * commandDesc);
int XChat_XChatNative_Command_GenericHook(char *word[], char *word_eol[], void *userdata);
void XChat_XChatNative_PrintLine(MonoString * text);
int XChat_XChatNative_OnMessage(char *word[], void *userdata);

// CIL Method Descriptors
MonoMethod * XChat_XChatNative_OnJoin_Method;
MonoClass * XChat_PluginManager_Class;
MonoObject * XChat_PluginManager_Object;
MonoMethod * XChat_XChatNative_OnCommand_Method;
MonoMethod * XChat_XChatNative_OnMessage_Method;

// Common Functions.
void Init_XChat_PluginManager_Object(const char * baseDir);
void InitializeMono();

static int join_cb(char *word[], void *userdata)
{
	
	XChat_XChatNative_OnJoin(word[1],word[2]);
	
	return XCHAT_EAT_NONE;  /* don't eat this event, xchat needs to see it! */
}

void xchat_plugin_get_info(char **name, char **desc, char **version, void **reserved)
{
	*name = PNAME;
	*desc = PDESC;
	*version = PVERSION;
}

int xchat_plugin_init(xchat_plugin *plugin_handle,
                      char **plugin_name,
                      char **plugin_desc,
                      char **plugin_version,
                      char *arg)
{
	ph = plugin_handle;

	*plugin_name = PNAME;
	*plugin_desc = PDESC;
	*plugin_version = PVERSION;

	xchat_hook_print(ph, "Join", XCHAT_PRI_NORM, join_cb, 0);
	xchat_hook_print(ph, "Channel Message", XCHAT_PRI_NORM, XChat_XChatNative_OnMessage, 0);
	printf("Initializing Mono\n");
	InitializeMono();
	return 1;       /* return 1 for success */
}

void InitializeMono()
{
		const char *home = g_get_home_dir ();

	const char * base_path = g_build_path ("/", home, ".config", "xchat", "mono", NULL);
		char * boot_assembly = g_build_path ("/",base_path,"xchat-mono.dll", NULL);

	//printf("boot assembly= %s",boot_assembly);	
	
	mono_config_parse ("/home/jhernandez/.config/xchat/mono/xchat-mono.dll.config");
		dom = mono_jit_init(boot_assembly);


	masm = mono_domain_assembly_open(dom,boot_assembly);
		if(!masm)
		{
			printf("Can not open mono assembly");
			exit(2);
		}
		
			char *argv [1];
		argv [0] = boot_assembly;
		argv[1] = NULL;
		mono_jit_exec (dom, masm, 1,argv);

		
		mono_add_internal_call("XChat.XChatNative::SendCommand",XChat_XChatNative_SendCommand );	
		mono_add_internal_call("XChat.XChatNative::GetCurrentChannelName",XChat_XChatNative_GetCurrentChannelName);	
		mono_add_internal_call("XChat.XChatNative::GetCurrentNickname",XChat_XChatNative_GetCurrentNickname);	
		mono_add_internal_call("XChat.XChatNative::RegisterCommand",XChat_XChatNative_RegisterCommand);
		mono_add_internal_call("XChat.XChatNative::PrintLine",XChat_XChatNative_PrintLine);

		mimg= mono_assembly_get_image  (masm);

		XChat_PluginManager_Class = mono_class_from_name (mimg,"XChat","PluginManager");
		if(!XChat_PluginManager_Class )printf("NO PLUGIN MANAGER LOCATED\n");
			
		
		Init_XChat_PluginManager_Object(base_path);
		MonoMethodDesc * desc;
		
		desc = mono_method_desc_new ("XChat.XChatNative:OnJoin", TRUE);

		XChat_XChatNative_OnJoin_Method = mono_method_desc_search_in_image (desc, mimg);
		mono_method_desc_free (desc);
		
		desc = mono_method_desc_new ("XChat.XChatNative:OnCommand", TRUE);
		XChat_XChatNative_OnCommand_Method = mono_method_desc_search_in_image (desc, mimg);
		mono_method_desc_free (desc);

		desc = mono_method_desc_new ("XChat.XChatNative:OnMessage", TRUE);
		XChat_XChatNative_OnMessage_Method = mono_method_desc_search_in_image (desc, mimg);
		mono_method_desc_free (desc);
		printf("Mono Initialized\n");
}
void Init_XChat_PluginManager_Object(const char * base_path)
{
		XChat_PluginManager_Object = mono_object_new(dom,XChat_PluginManager_Class);
		MonoMethodDesc * desc;
		desc = mono_method_desc_new ("XChat.PluginManager:.ctor", TRUE);
		MonoMethod * ctorMethod = mono_method_desc_search_in_image (desc, mimg);
		void *params [1];
		params[0] = mono_string_new(dom,base_path);
		mono_runtime_invoke (ctorMethod, XChat_PluginManager_Object, params, NULL);
		mono_method_desc_free (desc);
}
void XChat_XChatNative_SendCommand (MonoString * value) 
{
	xchat_command(ph, mono_string_to_utf8(value));
}//XChat_XChatNative_SendCommand

MonoString *  XChat_XChatNative_GetCurrentChannelName () 
{
	const char * channelName;
	channelName = xchat_get_info(ph,"channel");	
	return mono_string_new(dom,channelName);
}//XChat_XChatNative_GetCurrentChannelName

MonoString * XChat_XChatNative_GetCurrentNickname() 
{
	const char * nickName;
	nickName = xchat_get_info(ph,"nick");
	return mono_string_new(dom,nickName);
}//XChat_XChatNative_GetCurrentNickname

void XChat_XChatNative_RegisterCommand(MonoString * commandName,MonoString * commandDesc)
{
	xchat_hook_command(
		ph, 
		mono_string_to_utf8(commandName), 
		XCHAT_PRI_NORM, 
		XChat_XChatNative_Command_GenericHook, 
		mono_string_to_utf8(commandDesc), NULL);
}//XChat_XChatNative_RegisterCommand

int XChat_XChatNative_OnMessage(char *word[], void *userdata)
{
	if(!XChat_XChatNative_OnMessage_Method) return XCHAT_EAT_NONE;
	void *params [1];
	params[0] = mono_string_new(dom,word[1]);
	params[1] = mono_string_new(dom,word[2]);

	mono_runtime_invoke (XChat_XChatNative_OnMessage_Method, NULL, params, NULL);
	return XCHAT_EAT_NONE;
}//XChat_XChatNative_Command_HookGeneric

void XChat_XChatNative_OnJoin(char * nickName,char * channelName)
{
	if(XChat_XChatNative_OnJoin_Method == NULL) return;
	
	void *params [1];
	params [0] = mono_string_new (dom, nickName);
	params [1] = mono_string_new (dom, channelName);
	mono_runtime_invoke (XChat_XChatNative_OnJoin_Method, NULL, params, NULL);
}//XChat_XChatNative_OnJoin

int XChat_XChatNative_Command_GenericHook(char *word[], char *word_eol[], void *userdata)
{
	xchat_print(ph,word[2]);
	if(XChat_XChatNative_OnCommand_Method == NULL) return XCHAT_EAT_NONE;
	void *params [2];
	params [0] = mono_string_new (dom, word[1]); //commandName
	params [1] = mono_string_new (dom, word[2]); //arg1
	mono_runtime_invoke (XChat_XChatNative_OnCommand_Method, NULL, params, NULL);
	return XCHAT_EAT_ALL;
}//XChat_XChatNative_Command_HookGeneric

void XChat_XChatNative_PrintLine(MonoString * text)
{
	xchat_print(ph,mono_string_to_utf8(text));
}//XChat_XChatNative_PrintLine
#include "xchat-plugin.h"
#include <stdio.h>
#include <string.h>
#include <mono/jit/jit.h>
#include <mono/metadata/mono-config.h>
#include <mono/metadata/assembly.h>
#include <mono/metadata/debug-helpers.h>
#include <mono/metadata/mono-debug.h>
#include <stdlib.h>
#include <glib.h>
#include <stdlib.h>
#define PNAME "MONO"
#define PDESC "MONO CLR BASE";
#define PVERSION "0.1"

static xchat_plugin *ph;   /* plugin handle */
static MonoDomain * dom ;
static MonoAssembly *masm;
static MonoImage  * mimg;

// CLI Internal Calls
void XChat_XChatNative_SendCommand(MonoString *); 
MonoString * XChat_XChatNative_GetCurrentChannelName();
MonoString * XChat_XChatNative_GetCurrentNickname();
void XChat_XChatNative_OnJoin(char * nickName,char * channelName);
void XChat_XChatNative_RegisterCommand(MonoString * commandName,MonoString * commandDesc);
int XChat_XChatNative_Command_GenericHook(char *word[], char *word_eol[], void *userdata);
void XChat_XChatNative_PrintLine(MonoString * text);

// CIL Method Descriptors
MonoMethod * XChat_XChatNative_OnJoin_Method;
MonoClass * XChat_PluginManager_Class;
MonoObject * XChat_PluginManager_Object;
MonoMethod * XChat_XChatNative_OnCommand_Method;

// Common Functions.
void Init_XChat_PluginManager_Object();
void InitializeMono();

static int join_cb(char *word[], void *userdata)
{
	
	XChat_XChatNative_OnJoin(word[1],word[2]);
	
	return XCHAT_EAT_NONE;  /* don't eat this event, xchat needs to see it! */
}

/*static int autooptoggle_cb(char *word[], char *word_eol[], void *userdata)
{
	
	//printf("Word 0 = %s\n",);
	//printf("%s\n",(char*)userdata);
	
	return XCHAT_EAT_ALL;  
}*/

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
	//printf("Initiated\n");
	ph = plugin_handle;

	/* tell xchat our info */
	*plugin_name = PNAME;
	*plugin_desc = PDESC;
	*plugin_version = PVERSION;

	//xchat_hook_command(ph, "mono", XCHAT_PRI_NORM, autooptoggle_cb, "Usage: AUTOOPTOGGLE, Turns OFF/ON Auto Oping", "plugin:location");
	xchat_hook_print(ph, "Join", XCHAT_PRI_NORM, join_cb, 0);
	InitializeMono();
	return 1;       /* return 1 for success */
}

void InitializeMono()
{
		mono_config_parse (NULL);
		mono_set_dirs ("/usr/lib/",NULL);
		dom = mono_jit_init("Domain1");
		mono_debug_init (MONO_DEBUG_FORMAT_MONO);
		mono_debug_init_1 (dom);
	
		masm = mono_domain_assembly_open(dom,strcat(getenv("HOME"),"/.config/xchat/mono/xchat-mono.dll"));
		if(!masm)
		{
			//printf("\tFailed\n");
			printf("Can not open assembly");
			exit(2);
		}
		mono_add_internal_call("XChat.XChatNative::SendCommand",XChat_XChatNative_SendCommand );	
		mono_add_internal_call("XChat.XChatNative::GetCurrentChannelName",XChat_XChatNative_GetCurrentChannelName);	
		mono_add_internal_call("XChat.XChatNative::GetCurrentNickname",XChat_XChatNative_GetCurrentNickname);	
		mono_add_internal_call("XChat.XChatNative::RegisterCommand",XChat_XChatNative_RegisterCommand);
		mono_add_internal_call("XChat.XChatNative::PrintLine",XChat_XChatNative_PrintLine);

		mimg= mono_assembly_get_image  (masm);
		
		XChat_PluginManager_Class = mono_class_from_name (mimg,"XChat","PluginManager");
		if(!XChat_PluginManager_Class )printf("NO PLUGIN MANAGER LOCATED\n");
			
		
		Init_XChat_PluginManager_Object();
		MonoMethodDesc * desc;
		
		//if(!XChat_PluginManager_Object )printf("NO PLUGIN OBJECT LOCATED\n");
		
		desc = mono_method_desc_new ("XChat.XChatNative:OnJoin", TRUE);
		//if(desc == NULL) printf("CAN NOT LOCATE DESC\n");
		XChat_XChatNative_OnJoin_Method = mono_method_desc_search_in_image (desc, mimg);
		//if(XChat_XChatNative_OnJoin_Method == NULL) printf("CAN NOT mono_method_desc_search_in_image DESC\n");
		mono_method_desc_free (desc);
		
		desc = mono_method_desc_new ("XChat.XChatNative:OnCommand", TRUE);
		XChat_XChatNative_OnCommand_Method = mono_method_desc_search_in_image (desc, mimg);
		mono_method_desc_free (desc);
	/*char *argv [2];

	argv [0] = "";
	argv [1] = NULL;*/
	
	//mono_jit_exec (dom, masm,1,argv);

}
void Init_XChat_PluginManager_Object()
{
		XChat_PluginManager_Object = mono_object_new(dom,XChat_PluginManager_Class);
		MonoMethodDesc * desc;
		desc = mono_method_desc_new ("XChat.PluginManager:.ctor", TRUE);
		/*if(!desc) 
		{
			printf("CAN not locate CTORDESC\n");
			exit(2);
		}*/
		
		MonoMethod * ctorMethod = mono_method_desc_search_in_image (desc, mimg);
		/*if(!ctorMethod) 
		{
			printf("CAN not locate CTOR INSTANCE\n");
			exit(2);
		}
		else
		{
				printf("Instance\n");
		}*/
		mono_runtime_invoke (ctorMethod, XChat_PluginManager_Object, NULL, NULL);
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
	/*printf("Arg? %s\n",word[2]);
	printf("Arg? %s\n",word[3]);*/
	xchat_print(ph,word[2]);
	if(XChat_XChatNative_OnCommand_Method == NULL) return XCHAT_EAT_NONE;
	void *params [1];
	params [0] = mono_string_new (dom, word[1]); //commandName
	mono_runtime_invoke (XChat_XChatNative_OnCommand_Method, NULL, params, NULL);
	return XCHAT_EAT_ALL;
}//XChat_XChatNative_Command_HookGeneric

void XChat_XChatNative_PrintLine(MonoString * text)
{
	xchat_print(ph,mono_string_to_utf8(text));
}//XChat_XChatNative_PrintLine

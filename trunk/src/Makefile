all: xchat-mono.dll xchat-mono.so plugins

xchat-mono.dll: XChat.XChatNative.cs XChat.PluginManager.cs
	gmcs -target:library XChat.XChatNative.cs XChat.PluginManager.cs -out:xchat-mono.dll

xchat-mono.so: xchat-mono.c xchat-mono.dll
	gcc -Wl,--export-dynamic -Wall -O1 -shared -fPIC xchat-mono.c `pkg-config --cflags --libs mono` -o xchat-mono.so

xchat-mono-banshee.dll: xchat-mono.dll BansheeXChat.cs
	gmcs -target:library BansheeXChat.cs -pkg:ndesk-dbus-1.0 -pkg:ndesk-dbus-glib-1.0 -pkg:glib-sharp-2.0 -r:xchat-mono.dll -out:xchat-mono-banshee.dll

plugins: xchat-mono-banshee.dll
	

install: all
	-sudo rm /usr/lib/xchat-gnome/plugins/xchat-mono.so
	-sudo rm /usr/lib/xchat-gnome/plugins/xchat-mono.dll
	cp xchat-mono.so /usr/lib/xchat-gnome/plugins

install-cil: all
	-mkdir $(HOME)/.config
	-mkdir $(HOME)/.config/xchat
	-mkdir $(HOME)/.config/xchat/mono
	-mkdir $(HOME)/.config/xchat/mono/plugins
	cp xchat-mono.dll $(HOME)/.config/xchat/mono/

install-plugins: all
	cp xchat-mono-banshee.dll $(HOME)/.config/xchat/mono/plugins/xchat-mono-banshee.dll

clean:
	rm *.so
	rm *.dll

run:
	xchat-gnome

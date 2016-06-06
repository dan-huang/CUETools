HOST_APP_BUNDLE=$(shell echo $$BUILT_PRODUCTS_DIR/$$FULL_PRODUCT_NAME)
HOST_APP_RESOURCE_BUNDLE=$(HOST_APP_BUNDLE)/Resources
MONO_LIBRARY=/Library/Frameworks/Mono.framework/Libraries
DOT_NET_FRAMEWORK=$(MONO_LIBRARY)/mono/4.5
TARGETNAME=$(Tonal)

cuetools:
	xbuild ../Tonal/Tonal.sln
	cp ../Tonal/Tonal/bin/Debug/*.dll $(HOST_APP_RESOURCE_BUNDLE)
	cp $(DOT_NET_FRAMEWORK)/System.dll $(HOST_APP_RESOURCE_BUNDLE)
	cp $(DOT_NET_FRAMEWORK)/System.Xml.dll $(HOST_APP_RESOURCE_BUNDLE)
	cp $(DOT_NET_FRAMEWORK)/System.Core.dll $(HOST_APP_RESOURCE_BUNDLE)
	cp $(DOT_NET_FRAMEWORK)/System.Configuration.dll $(HOST_APP_RESOURCE_BUNDLE)
	cp $(DOT_NET_FRAMEWORK)/Mono.Security.dll $(HOST_APP_RESOURCE_BUNDLE)
	cp $(MONO_LIBRARY)/mono/4.5-api/mscorlib.dll $(HOST_APP_RESOURCE_BUNDLE)
	cp $(MONO_LIBRARY)/libmonosgen-2.0.1.dylib $(HOST_APP_RESOURCE_BUNDLE)
	cp Vendor/framework.config $(HOST_APP_RESOURCE_BUNDLE)
	install_name_tool -change /private/tmp/source-mono-4.4.0/bockbuild-mono-4.4.0-branch/profiles/mono-mac-xamarin/package-root/lib/libmonosgen-2.0.1.dylib $(HOST_APP_RESOURCE_BUNDLE)/libmonosgen-2.0.1.dylib $(HOST_APP_BUNDLE)/tonal_mac_cuetools

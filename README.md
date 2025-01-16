# cf-web-server

Basic web server. It serves static content and also dynamic content via PowerScript scripts.

The web server runs an internal website that is used for configuring websites. It can be managed via the
scripts in the Scripts folder.

# CFWebServerCommon (Class library)
This is a class library containing the web server components.

# CFWebServerConsole (Console app)
This is a console app for running the web server.

It can run in the following modes:
1) Run single website that has a site config. Uses /site-config-id command line parameter.
2) Run single website with no site config. Parameters are set on the command line.
3) Run all websites with a site config. Default if none of the above is set.

# CFWebServerMobile (.NET Maui app)
This is an .NET Maui app for running the web server. Currently it only run on Android.

# Scripts folder
This folder contains scripts for the following:
1) Get site configs.
2) Update site config.

# Command Line
Command line supports running only one website.

Parameter					Description
- /all-sites				Runs all websites with a site config
- /default-file				Default file for / request. E.g. Index.html
- /file-cache-compressed	Whether to store cache files as compressed. Default if not specified.
- /file-cache-expiry-secs	Number of secs since cache file last used after which it is expired. If file is
							frequently requested then it remains in cache and is removed N mins after it stops
							being requested. Default if not specified.
- /file-cache-disabled		File cache disabled. 
- /max-cached-file-size		Max size of file (Bytes) that will be cached. Default if not specified.
- /max-concurrent-requests	Max number of requests served concurrently. Default if not specified.
- /max-file-cache-size		Max total size of file cache (Bytes). Default if not specified.
- /root						Root folder where site is hosted.
							If "{process-path}\TestSite" is specified then the it will replace {process-path}
							with the path to the exe file.
- /site						Site to host. E.g. http://localhost:10010						
- /site-config-id			Use existing site config

Example to run website with a site config:
/site="http://localhost:10010"
/root="{process-path}\Sites\Test1" 
/default-file="Index.html"
/file-cache-compressed=true 
/file-cache-expiry-mins=30 
/max-concurrent-requests=15
/max-file-cache-size=10000024

Example to run a website for an existing site config:
/site-config-id="123456"

# Internal Web Site
An internal web site is hosted so that sites can be configured via HTTP.

# PowerShell Web Request Handler
The web server can handle dynamic content via PowerShell scripts.

See example in \Scripts\PowerShell Web Request Handler Sample
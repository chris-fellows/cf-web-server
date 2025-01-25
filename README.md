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
2) Run all enabled websites.

# CFWebServerMobile (.NET Maui app)
This is an .NET Maui app for running the web server. Currently it only run on Android.

# Scripts folder
This folder contains scripts for the following:
1) Get site configs.
2) Update site config.
3) PowerShell web request handler sample.

# Command Line
Parameter					Description
- /site-internal-site		Internal site for site config. E.g. http://localhost:10010/
- /site-config-id			Use existing site config. If not specified then all enabled sites are started.

Example to run a website for an existing site config:
/site-config-id="123456"

# Internal Web Site
An internal web site is hosted so that sites can be configured via HTTP.

# PowerShell Web Request Handler
The web server can handle dynamic content via PowerShell scripts. Each script must accept a standard set of
input parameters (Request, site parameters) and writes a standard set of output parameters (Response).

See example in \Scripts\PowerShell Web Request Handler Sample. It prompts the user for some parameters and
then posts the HTML Form to a PowerScript which returns an HTML page.
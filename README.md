# cf-web-server

Basic web server. Currently only serves static resources.

Each process logically supports multiple websites but the command line only supports parameters for a 
single website. It would just need changes to read the websites from a config file.

# Command Line
Command line supports running only one website.

Parameter					Description
/default-file				Default file for / request. E.g. Index.html
/file-cache-compressed		Whether to store cache files as compressed. Default if not specified.
/file-cache-expiry-secs	 	Number of secs since cache file last used after which it is expired. If file is
							frequently requested then it remains in cache and is removed N mins after it stops
							being requested. Default if not specified.
/file-cache-disabled		File cache disabled. 
/max-cached-file-size		Max size of file (Bytes) that will be cached. Default if not specified.
/max-concurrent-requests	Max number of requests served concurrently. Default if not specified.
/max-file-cache-size		Max total size of file cache (Bytes). Default if not specified.
/root						Root folder where site is hosted.
							If "{process-path}\TestSite" is specified then the it will replace {process-path}
							with the path to the exe file.
/site						Site to host. E.g. http://localhost:10010						

Example:
/site="http://localhost:10010"
/root="{process-path}\Sites\Test1" 
/default-file="Index.html"
/file-cache-compressed=true 
/file-cache-expiry-mins=30 
/max-concurrent-requests=15
/max-file-cache-size=10000024
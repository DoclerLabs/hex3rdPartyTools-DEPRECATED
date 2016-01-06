# GenereteImports from configuration xml files
This simple neko app helps you to create an Imports.hx file to include it on build.
This helps you to not import manually to your main class all the time the classes that you use in the configuration xml files.

# Usage
In FlashDevelop Project>Properties>Build>Pre-build command line add the following line (take care of the correct paths):
neko $(ProjectDir)\hex3rdPartyTools\utilities\xml\GenerateImports\bin\GenerateImports.n  -dir $(ProjectDir)\src -output $(ProjectDir)\src\Imports.hx
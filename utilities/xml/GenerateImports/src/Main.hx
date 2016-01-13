package;

import neko.Lib;
import sys.FileSystem;
import sys.io.File;
import sys.io.FileOutput;

/**
 * ...
 * @author 
 */
class Main 
{
	public static var keys = ["-dir", "-output", "-packagename"];
	
	var items:Array<String>;
	
	var dir:String;
	var output:String;
	var packagename:String = "";
	var outputFile:FileOutput;
	
	var typeMatcher:EReg = ~/(?:type|strategy|value)\s*?=\s*('|")([a-zA-Z0-9_.]+\.[a-zA-Z0-9_.]+)\1/gm;
	
	static function main() 
	{
		new Main();
	}
	
	public function new() 
	{
		if (parseArgs())
		{
		
			// make sure that the to directory exists
			//if (!FileSystem.exists(dir)) FileSystem.createDirectory(dir);
			
			items = [];
			recurse(dir);
			
			this.outputFile = File.write( this.output, false );
			
			this.outputFile.writeString( "package" + (this.packagename != null ? " " + this.packagename : "") + ";\n" );
			
			
			for (item in items)
			{
				doConversion(item);
			}
			
			var classMatcher = ~/(\w+(?:\.\w+)*)\.\w+$/i;
			classMatcher.match(this.output);
			
			this.outputFile.writeString( "class " + classMatcher.matched(1) + "{}" );
			
			this.outputFile.close();
			
		}
	}
	
	private function doConversion(file:String):Void
	{		
		var fromFile = file;
		
		var s = sys.io.File.getContent(fromFile);
		
		var results:Array<String> = [];
		
		this.typeMatcher.map(s, function(r:EReg):String {
			results.push( r.matched(2) );
			return r.matched(0);
		});
		
		var line:String;
		
		for ( result in results )
		{
			trace( result );
			this.outputFile.writeString( "import " + result + ";\n" );
		}
	}
	
	private function recurse(path:String)
	{
		var dir = FileSystem.readDirectory(path);
		
		for (item in dir)
		{
			var s = path + "/" + item;
			if (FileSystem.isDirectory(s))
			{
				recurse(s);
			}
			else
			{
				var exts = ["xml"];
				if(Lambda.has(exts, getExt(item)))
					items.push(s);
			}
		}
	}
	
	public function getExt(s:String)
	{
		return s.substr(s.lastIndexOf(".") + 1).toLowerCase();
	}
	
	private function parseArgs():Bool
	{
		// Parse args
		#if debug
		var args:Array<String> = ["-dir", "c:\\Work\\XMLPreProcessor\\bin", "-output", "c:\\Work\\XMLPreProcessor\\bin\\imports.hx", "-packagename", "com"];
		#else
		var args = Sys.args();
		#end
		for (i in 0...args.length)
		{
			if (Lambda.has(keys, args[i]))
				Reflect.setField(this, args[i].substr(1), args[i + 1]);
		}
			
		// Check to see if argument is missing
		if (dir == null) { Lib.println("Missing argument '-dir'"); return false; }
		if (output == null) { Lib.println("Missing argument '-output'"); return false; }
		
		return true;
	}
}
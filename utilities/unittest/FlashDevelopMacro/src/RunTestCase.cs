using ASCompletion.Context;
using PluginCore;
using PluginCore.Managers;
using System;
using System.IO;
using System.Threading;
using ProjectManager;
using ProjectManager.Projects.Haxe;
using ProjectManager.Actions;
using ProjectManager.Controls;
using ProjectManager.Controls.AS2;
using ProjectManager.Controls.TreeView;
using ProjectManager.Helpers;
using ProjectManager.Projects;


namespace Macros
{
    class TestClassRunner
    {
		private static string PREVIOUS_TEST_CLASS;
		
		private string origDocumentPath;
		
		private string tempDocumentClass = "src/TestMainTmp.hx";
		
		private string testDocumentTemplate = 
@"package;

class TestMainTmp
{
    static public function main() : Void
    {
		var emu : hex.unittest.runner.ExMachinaUnitCore = new hex.unittest.runner.ExMachinaUnitCore();
		
		#if js
			js.Browser.document.getElementById('console').style.display = 'block';
			//emu.addListener( new hex.unittest.notifier.BrowserUnitTestNotifier('console') );
			emu.addListener( new hex.unittest.notifier.WebSocketNotifier('ws://localhost:6660') );
		#elseif flash
			emu.addListener( new hex.unittest.notifier.FlashUnitTestNotifier(flash.Lib.current) );
		#end
		
		emu.$(testToRun);
        emu.run();
    }	
}";
		
		
		public static void Execute()
        {
			
			TestClassRunner runner = new TestClassRunner(); 
		}
		
		
		public TestClassRunner( )
		{
			if ( this.checkRunFromValidClass() )
			{
				this.notifyTestStart();
				
				HaxeProject project = (HaxeProject)PluginBase.CurrentProject;
				
				this.generateDocumentClassFromTemplate();
				
				this.storeOrigDocumentClass(project);
				
				this.setDocumentClass( project, Path.Combine( Path.GetDirectoryName(project.ProjectPath), this.tempDocumentClass ) );
				
				
				this.build();
				
				//TODO: figure out the build finished event
				Thread.Sleep(1000);
				
				this.setDocumentClass( project, this.origDocumentPath );
				
				this.cleanup();
			}
		}
		
		private bool checkRunFromValidClass( )
		{
			if ( ASContext.Context.CurrentClass.IsVoid() && PREVIOUS_TEST_CLASS == null )
			{
				TraceManager.Add("You should select a test class to RunTestCase!", 3);
				return false;
			}
			
			return true;
		}
		
		private void notifyTestStart( )
		{
			TraceManager.Add("Running unit test for " + ASContext.Context.CurrentClass.QualifiedName );
		}
		
		private void generateDocumentClassFromTemplate()
		{
			/*string testToRun = ASContext.Context.CurrentMember != null 
								? "addTestMethod(" + ASContext.Context.CurrentClass.QualifiedName + ", \"" + ASContext.Context.CurrentMember.Name + "\")"
								: "addTest(" + ASContext.Context.CurrentClass.QualifiedName + ")";*/
			
			string testToRun = this.getTestToRun();
			
			if ( !testToRun.EndsWith("Test") && PREVIOUS_TEST_CLASS != null )
			{
				testToRun = PREVIOUS_TEST_CLASS;
			}
			else
			{
				PREVIOUS_TEST_CLASS = testToRun;
			}
			
			string text = this.testDocumentTemplate;
			text = text.Replace("$(testToRun)", "addTest(" + testToRun + ")");
			File.WriteAllText(this.tempDocumentClass, text);
		}
		
		private string getTestToRun( )
		{
			/*string testToRun = ASContext.Context.CurrentMember != null 
								? "addTestMethod(" + ASContext.Context.CurrentClass.QualifiedName + ", \"" + ASContext.Context.CurrentMember.Name + "\")"
								: "addTest(" + ASContext.Context.CurrentClass.QualifiedName + ")";*/
			
			string testToRun = ASContext.Context.CurrentClass.QualifiedName;
			
			if ( !testToRun.EndsWith("Test") && PREVIOUS_TEST_CLASS != null )
			{
				return PREVIOUS_TEST_CLASS;
			}
			else
			{
				return testToRun;
			}
		}
		
		
		private void storeOrigDocumentClass(HaxeProject project)
		{
			string origMain = project.CompilerOptions.MainClass;
			string projectPath = Path.GetDirectoryName(project.ProjectPath);
			
			foreach (string cp in project.AbsoluteClasspaths)
			{
				if (File.Exists(Path.Combine(cp, origMain + ".hx")))
				{
					this.origDocumentPath = Path.Combine(cp, origMain + ".hx");
					break;
				}
			}
		}
		
		private void setDocumentClass( Project project, string path )
		{
			project.SetDocumentClass( path, true ); 
			project.Save();
		}
		
		private void build( )
		{
			DataEvent de1 = new DataEvent(EventType.Command, ProjectManagerCommands.BuildProject, null);
			EventManager.DispatchEvent(null, de1);
			
			/*if (!buildActions.Build(project, true, noTrace))
			{
				BroadcastBuildFailed(project);
			}*/
			
			DataEvent de2 = new DataEvent(EventType.Command, ProjectManagerCommands.TestMovie, null);
			EventManager.DispatchEvent(null, de2);
			
			//EventManager.AddEventHandler(null, "ProjectManager.BuildComplete")
		}
		
		private void cleanup()
		{
			File.Delete( this.tempDocumentClass );
		}
		
		/*public void HandleEvent(Object sender, NotifyEvent e, HandlingPriority priority)
        {
            switch (e.Type)
            {
                case EventType.Command:
                {
                    DataEvent de = (DataEvent)e;
                    if (de.Action == "ProjectManager.BuildComplete") 
                    {
                        TraceManager.Add("HELLOOOO");
                    }
                    break;
                }
            }
        }*/
	}
}
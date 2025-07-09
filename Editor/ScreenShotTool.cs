using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using Editor;
using Editor.ShaderGraph.Nodes;
using Sandbox;


[Dock( "Editor", "Screenshot Editor", "camera" )]
public class ScreenShotWindow : Widget
{
	public int width = 1920;
	public int height = 1080;
	public string fileName = "ScreenShot";
	public string folderName = "";

	public ScreenShotWindow( Widget parent ) : base( parent )
	{
		Layout = Layout.Column();
		Layout.Spacing = 10;
		Layout.Margin = 10;

		Rebuild();
	}

	[EditorEvent.Hotload]
	public void OnHotload()
	{
		Rebuild();
	}

	void Rebuild()
	{
		Layout?.Clear( true );

		var so = this.GetSerialized();

		var controlSheet = new ControlSheet();

		controlSheet.AddRow( so.GetProperty( "width" ) );
		controlSheet.AddRow( so.GetProperty( "height" ) );
		controlSheet.AddRow( so.GetProperty( "fileName" ) );
		controlSheet.AddRow( so.GetProperty( "folderName" ) );
		controlSheet.Margin = 0;

		Layout.Add( controlSheet );

		var col = Layout.AddColumn();
		col.Spacing = 5;
		col.Margin = 10;

		col.Add( new Label( "If the folder name is empty, the screenshot will be saved in the root assets folder." ) );
		col.Add( new Label( "You can use folder structures like \"this/another/folder\" and it will be saved within that folder." ) );

		var button = new Button();
		button.Text = "Take Screen Shot";
		button.Clicked += TakeScreenShot;
		button.FixedHeight = 30;

		Layout.Add( button );

		Layout.AddStretchCell();
	}

	public void TakeScreenShot()
	{
		var pixMap = new Pixmap( width, height );

		var camera = Game.IsPlaying ? Game.ActiveScene?.Camera : SceneEditorSession.Active.Scene?.Camera;

		if ( !camera.IsValid() )
		{
			Log.Error( "No active camera found. Please ensure a camera is present in the scene." );
			return;
		}

		var path = string.IsNullOrEmpty( folderName ) ? $"{Project.Current.GetRootPath().Replace( '\\', '/' )}/assets/{fileName}.png" : $"{Project.Current.GetRootPath().Replace( '\\', '/' )}/assets/{folderName}/{fileName}.png";

		var folderExists = Directory.Exists( Path.GetDirectoryName( path ) );

		if ( !folderExists && !string.IsNullOrWhiteSpace( path ) )
		{
			Directory.CreateDirectory( Path.GetDirectoryName( path ) );
			Log.Info( $"Created directory: {Path.GetDirectoryName( path )}" );
		}

		camera.RenderToPixmap( pixMap );
		pixMap.SavePng( path );

		Log.Warning( $"Screenshot saved to {path}" );
	}
}

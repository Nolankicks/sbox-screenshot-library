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
	public Vector2Int resolution { get; set; } = new Vector2Int( 1920, 1080 );
	public string fileName { get; set; } = "ScreenShot";
	public string folderName { get; set; } = "";
	public ScreenShotFileFormat fileFormat { get; set; } = ScreenShotFileFormat.PNG;

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

		controlSheet.AddRow( so.GetProperty( "resolution" ) );
		controlSheet.AddRow( so.GetProperty( "fileFormat" ) );
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
		var pixMap = new Pixmap( resolution.x, resolution.y );

		var camera = Game.IsPlaying ? Game.ActiveScene?.Camera : SceneEditorSession.Active.Scene?.Camera;

		if ( !camera.IsValid() )
		{
			Log.Error( "No active camera found. Please ensure a camera is present in the scene." );
			return;
		}

		var path = string.IsNullOrEmpty( folderName ) ? $"{Project.Current.GetRootPath().Replace( '\\', '/' )}/assets/{fileName}" : $"{Project.Current.GetRootPath().Replace( '\\', '/' )}/assets/{folderName}/{fileName}";

		var folderExists = Directory.Exists( Path.GetDirectoryName( path ) );

		if ( !folderExists && !string.IsNullOrWhiteSpace( path ) )
		{
			Directory.CreateDirectory( Path.GetDirectoryName( path ) );
			Log.Info( $"Created directory: {Path.GetDirectoryName( path )}" );
		}

		camera.RenderToPixmap( pixMap );

		switch ( fileFormat )
		{
			case ScreenShotFileFormat.JPG:
				if ( !path.EndsWith( ".jpg", System.StringComparison.OrdinalIgnoreCase ) )
					path += ".jpg";

				pixMap.SaveJpg( path );
				Log.Info( $"Screenshot saved as JPG to {path}" );
				break;
			case ScreenShotFileFormat.PNG:
				if ( !path.EndsWith( ".png", System.StringComparison.OrdinalIgnoreCase ) )
					path += ".png";

				pixMap.SavePng( path );
				Log.Info( $"Screenshot saved as PNG to {path}" );
				break;
		}
	}
}

public enum ScreenShotFileFormat
{
	PNG,
	JPG,
}

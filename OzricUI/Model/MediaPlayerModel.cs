using Blazor.Diagrams.Core.Geometry;
using OzricEngine.logic;

namespace OzricUI.Model;

public class MediaPlayerModel: EntityModel
{
    public const string ICON = "mdi:cast";

    public MediaPlayerModel(MediaPlayer mediaPlayer, Point? point = null): base(mediaPlayer, point)
    {
    }
    
    public override string Icon => ICON;
}
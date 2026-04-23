using Blazor.Diagrams.Core.Geometry;
using Ozric.Dashboard.Model;
using OzricEngine.Nodes;

namespace Ozric.Dashboard.Models;

public class DiagramMediaPlayer: DiagramEntity
{
    public const string ICON = "mdi:cast";

    public DiagramMediaPlayer(GraphMediaPlayer mediaPlayer, Point? point = null): base(mediaPlayer, point)
    {
    }
    
    public override string Icon => ICON;
}
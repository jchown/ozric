using System.Threading;
using System.Threading.Tasks;
using OzricEngine.Nodes;

namespace OzricEngine;

public abstract class CancellableTask: OzricObject
{
    private CancellationTokenSource _cancellationTokenSource = new();
    
    public void Start()
    {
        var cancellationToken = _cancellationTokenSource.Token;
        Task.Run(() => Run(cancellationToken), cancellationToken);
    }

    protected abstract Task Run(CancellationToken token);
    
    public void Cancel()
    {
        if (_cancellationTokenSource.IsCancellationRequested)
            return;
        
        _cancellationTokenSource.Cancel();
    }
}
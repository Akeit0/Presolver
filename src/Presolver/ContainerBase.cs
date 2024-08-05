﻿namespace Presolver;

public abstract class ContainerBase : IDisposable
{
    readonly CancellationTokenSource cancellationTokenSource = new();

    readonly object gate = new();

    List<IDisposable?> disposables = new();

    public bool IsDisposed { get; private set; }

    public CancellationToken CancellationToken => cancellationTokenSource.Token;

    public void Dispose()
    {
        if (IsDisposed) return;

        lock (gate)
        {
            if (IsDisposed) return;

            IsDisposed = true;
            DisposeFields();
            //InternalContainer?.Dispose();
            //Console.WriteLine("Dispose" +InternalContainer);
            foreach (var disposable in disposables) disposable?.Dispose();

            disposables.Clear();
            disposables = null!;

            cancellationTokenSource.Cancel();
        }
    }

    //protected abstract IInternalContainer? InternalContainer { get; } 


    // protected ContainerBase()
    // {
    //     // ReSharper disable once VirtualMemberCallInConstructor
    //     InternalContainer?.Initialize(this);
    //     //Console.WriteLine(GetType()+" "+InternalContainer.GetType());
    // }

    //protected object ParentResolver;

    protected abstract void DisposeFields();


    public void AddDisposable(IDisposable disposable)
    {
        lock (gate)
        {
            disposables.Add(disposable);
        }
    }

    public void ThrowIfDisposed()
    {
        if (IsDisposed) throw new ObjectDisposedException(GetType().Name);
    }
}
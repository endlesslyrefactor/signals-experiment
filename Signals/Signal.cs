namespace Signals;

public interface Cacheable
{
    void SetDirty();
}

public class Signal<T> : Cacheable
{
    private readonly Coordinator _coordinator;
    
    private Func<T> _func;

    private T _cachedValue;

    public bool IsDirty { get; private set; }

    public List<Cacheable> Sinks { get; } = new();
    
    public Signal(Coordinator coordinator, Func<T> func)
    {
        _coordinator = coordinator;
        _func = func;
        
        _cachedValue = _coordinator.ConfigureSources(this, func);
        IsDirty = false;
    }

    public void UpdateFunc(Func<T> newFunc)
    {
        this._func = newFunc;
        
        SetDirty();
    }
    
    public T Value
    {
        get
        {
            var coordinatorSink = _coordinator.GetCurrentSink();

            if (coordinatorSink != null)
                Sinks.Add(coordinatorSink);
            
            if (IsDirty)
            {
                _cachedValue = _func.Invoke();
                IsDirty = false;
            }

            return _cachedValue;
        }
    }

    public void SetDirty()
    {
        IsDirty = true;
        
        Sinks.ForEach(x => x.SetDirty());
    }
}

public class Coordinator()
{
    private Cacheable? _currentSink = null;
    
    public T ConfigureSources<T>(Cacheable sink, Func<T> func)
    {
        _currentSink = sink;
        var result = func();
        _currentSink = null;

        return result;
    }

    public Cacheable? GetCurrentSink()
    {
        return _currentSink!;
    }
}
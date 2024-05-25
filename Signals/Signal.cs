namespace Signals;

public interface Sink
{
    void SetDirty();
}

public interface Source
{
    void Deregister(Sink sink);
}

public class Signal<T> : Sink, Source
{
    private readonly Coordinator _coordinator;
    
    private Func<T> _func;

    private T _cachedValue;
    private T CachedValue
    {
        get => _cachedValue;
        set
        {
            _effects.ForEach(x => x(_cachedValue, value));
            _cachedValue = value;
        }
    }
    
    private List<Source> _sources;

    private List<Action<T, T>> _effects = new();

    public bool IsDirty { get; private set; }

    public List<Sink> Sinks { get; } = new();
    
    public Signal(Coordinator coordinator, Func<T> func)
    {
        _coordinator = coordinator;
        _func = func;
        
        (CachedValue, _sources) = _coordinator.ConfigureSources(this, func);
        IsDirty = false;
    }

    public void UpdateFunc(Func<T> newFunc)
    {
        this._func = newFunc;
        _sources.ForEach(x => x.Deregister(this));
        
        (CachedValue, _sources) = _coordinator.ConfigureSources(this, newFunc);
        
        SetDirty();
    }
    
    public T Value
    {
        get
        {
            var coordinatorSink = _coordinator.GetCurrentSink(this);

            if (coordinatorSink != null)
                Sinks.Add(coordinatorSink);
            
            if (IsDirty)
            {
                CachedValue = _func.Invoke();
                IsDirty = false;
            }

            return CachedValue;
        }
    }

    public void SetDirty()
    {
        IsDirty = true;
        
        Sinks.ForEach(x => x.SetDirty());
    }

    public void Deregister(Sink sink)
    {
        Sinks.Remove(sink);
    }
    
    public void AddEffect(Action<T, T> action)
    {
        _effects.Add(action);
    }
}

public class Coordinator()
{
    private Sink? _currentSink = null;

    private List<Source> _currentSources = new List<Source>();
    
    public (T, List<Source>) ConfigureSources<T>(Sink sink, Func<T> func)
    {
        _currentSources = new List<Source>();
        _currentSink = sink;
        var result = func();
        _currentSink = null;

        return (result, _currentSources);
    }

    public Sink? GetCurrentSink(Source source)
    {
        _currentSources.Add(source);
        return _currentSink!;
    }
}
namespace TestingLib.Internal.PatchImpl;

/// <summary>
/// Base class for a togglable patch.
/// </summary>
public abstract class TogglablePatch
{
    internal TogglablePatch()
    {
        Init();
    }
    /// <summary>
    /// State of the patch.
    /// </summary>
    public bool Enabled {
        get 
        { 
            return _enabled;
        }
        set 
        {
            _enabled = value;

            if (value) 
                OnEnable();
            else 
                OnDisable();
        }
    }
    private bool _enabled = false;
    internal virtual void Init() { }
    internal virtual void OnEnable() { }
    internal virtual void OnDisable() { }
}
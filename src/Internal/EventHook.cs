using System;
using System.Reflection;

namespace TestingLib {
    // I don't think such exists in Monomod, but I could be wrong.
    internal class EventHook
    {
        private EventInfo _EventInfo;
        private Delegate _Delegate;
        private bool _isApplied;
        internal EventHook(EventInfo _eventInfo, Delegate _delegate){
            _EventInfo = _eventInfo;
            _Delegate = Delegate.CreateDelegate(_eventInfo.EventHandlerType, _delegate.Method);
            _isApplied = false;
            Apply();
        }

        internal void Apply(){
            if(!_isApplied){
                _EventInfo.AddEventHandler(_Delegate.Target, _Delegate);
                _isApplied = true;
            }
        }
        
        internal void Undo(){
            if(_isApplied){
                _EventInfo.RemoveEventHandler(_Delegate.Target, _Delegate);
                _isApplied = false;
            }
        }

        public bool isApplied { get { return _isApplied; }}
    }
}
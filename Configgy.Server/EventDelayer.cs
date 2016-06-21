using System;
using System.Timers;

namespace Configgy.Server
{
    public class EventDelayer : IDisposable
    {
        private Timer _timer;
        private Action _action;
        private int _delayTime;
        private bool _resetWhenTriggering;
        private object _lock = new object();

        public EventDelayer(int timeInMilliseconds, bool resetWhenTriggering)
        {
            _delayTime = timeInMilliseconds;
            _resetWhenTriggering = resetWhenTriggering;

            _timer = new Timer(_delayTime);
            _timer.Elapsed += (_, __) => TriggerAction();
        }

        public void Trigger(Action action)
        {
            lock (_lock)
            {
                _action = action;
                _timer.Start();

                if (_resetWhenTriggering)
                {
                    _timer.Interval = _delayTime;
                }
            }
        }

        public void Dispose()
        {
            _timer.Dispose();
        }

        private void TriggerAction()
        {
            try
            {
                lock (_lock)
                {
                    if (_action == null) return;

                    try { _action(); }
                    catch { throw; }
                    finally
                    {
                        _action = null;
                        _timer.Stop();
                    }
                }
            }
            catch (Exception ex)
            {
                GenericExceptionHandler.Handle(ex);
            }
        }
    }
}

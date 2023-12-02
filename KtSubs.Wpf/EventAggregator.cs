//SOURCE: https://www.nichesoftware.co.nz/2015/08/16/wpf-event-aggregates.html

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace KtSubs.Wpf
{
    public class EventAggregator : IEventAggregator
    {
        private readonly List<Delegate> mHandlers = new List<Delegate>();
        private readonly SynchronizationContext? synchronizationContext;

        public EventAggregator()
        {
            synchronizationContext = SynchronizationContext.Current;
        }

        /// <summary>
        /// Send a message instance for immediate delivery
        /// </summary>
        /// <typeparam name="T">Type of the message</typeparam>
        /// <param name="message">Message to send</param>
        public void SendMessage<T>(T message)
        {
            if (message == null)
            {
                return;
            }

            if (synchronizationContext != null)
            {
                synchronizationContext.Send(
                    m => Dispatch((T?)m),
                    message);
            }
            else
            {
                Dispatch(message);
            }
        }

        /// <summary>
        /// Post a message instance for asynchronous delivery
        /// </summary>
        /// <typeparam name="T">Type of the message</typeparam>
        /// <param name="message">Message to send</param>
        public void PostMessage<T>(T message)
        {
            if (message == null)
            {
                return;
            }

            if (synchronizationContext != null)
            {
                synchronizationContext.Post(
                    m => Dispatch((T?)m),
                    message);
            }
            else
            {
                Dispatch(message);
            }
        }

        /// <summary>
        /// Register a message handler
        /// </summary>
        /// <param name="eventHandler">Message handler to add.</param>
        public Action<T> RegisterHandler<T>(Action<T> eventHandler)
        {
            if (eventHandler == null)
            {
                throw new ArgumentNullException(nameof(eventHandler));
            }

            mHandlers.Add(eventHandler);
            return eventHandler;
        }

        /// <summary>
        /// Unregister a message handler
        /// </summary>
        /// <param name="eventHandler">Message handler to remove.</param>
        public void UnregisterHandler<T>(Action<T> eventHandler)
        {
            if (eventHandler == null)
            {
                throw new ArgumentNullException(nameof(eventHandler));
            }

            mHandlers.Remove(eventHandler);
        }

        /// <summary>
        /// Dispatch a message to all appropriate handlers
        /// </summary>
        /// <typeparam name="T">Type of the message</typeparam>
        /// <param name="message">Message to dispatch to registered handlers</param>
        private void Dispatch<T>(T? message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            var compatibleHandlers
                = mHandlers.OfType<Action<T>>()
                    .ToList();

            foreach (var handler in compatibleHandlers)
            {
                handler(message);
            }
        }
    }
}
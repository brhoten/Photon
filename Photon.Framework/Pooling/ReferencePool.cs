﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Timers;

namespace Photon.Framework.Pooling
{
    public class ReferencePool<T> : IDisposable
        where T : IReferenceItem
    {
        private readonly ConcurrentDictionary<string, T> itemDictionary;
        private readonly Timer timer;

        public IEnumerable<T> Items => itemDictionary.Values;

        /// <summary>
        /// Gets or sets the amount of time in milliseconds between task pruning checks.
        /// </summary>
        public double PruneInterval {
            get => timer.Interval;
            set => timer.Interval = value;
        }


        public ReferencePool()
        {
            itemDictionary = new ConcurrentDictionary<string, T>(StringComparer.Ordinal);

            timer = new Timer {
                AutoReset = true,
            };
            timer.Elapsed += Timer_OnElapsed;

            PruneInterval = 60_000; // 1 minute
        }

        public void Dispose()
        {
            timer?.Dispose();
        }

        public void Start()
        {
            timer.Start();
        }

        public void Stop()
        {
            timer.Stop();
        }

        public void Add(T task)
        {
            itemDictionary[task.SessionId] = task;
        }

        public bool TryGet(string id, out T task)
        {
            return itemDictionary.TryGetValue(id, out task);
        }

        private void Timer_OnElapsed(object sender, ElapsedEventArgs e)
        {
            var keys = itemDictionary.Keys.ToArray();

            foreach (var id in keys) {
                if (!itemDictionary.TryGetValue(id, out var task))
                    continue;

                if (task.IsExpired()) {
                    if (itemDictionary.TryRemove(id, out _))
                        (task as IDisposable)?.Dispose();
                }
            }
        }
    }
}

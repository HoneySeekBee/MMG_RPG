using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerCore
{
    public class JobQueue
    {
        private Queue<Action> _jobs = new Queue<Action>();
        private object _lock = new object();

        public void Push(Action job)
        {
            lock (_lock)
            {
                _jobs.Enqueue(job);
            }
        }

        public void Flush()
        {
            while (true)
            {
                Action job = null;
                lock (_lock)
                {
                    if (_jobs.Count == 0)
                        break;

                    job = _jobs.Dequeue();
                }

                job?.Invoke();
            }
        }
    }
}

using System.Threading.Tasks;
using UnityEngine;

namespace Project.Scripts.Tools
{
    internal class WaitForTaskCompletion : CustomYieldInstruction
    {
        private readonly Task _task;
        public WaitForTaskCompletion(Task task) => _task = task;

        public override bool keepWaiting
        {
            get
            {
                if (!_task.IsCompleted) return true;
                if (!_task.IsFaulted) return false;
                if (_task.Exception != null)
                    Debug.LogError("WaitForTaskCompletion exception:" + _task.Exception.Message);
                return false;
            }
        }
    }
}
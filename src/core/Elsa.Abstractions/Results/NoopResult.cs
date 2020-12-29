using Elsa.Services;
using Elsa.Services.Models;

namespace Elsa.Results
{
    /// <summary>
    /// A result that does nothing.
    /// 什么也不处理
    /// </summary>
    public class NoopResult : ActivityExecutionResult
    {
        protected override void Execute(IWorkflowInvoker invoker, WorkflowExecutionContext workflowContext)
        {
            // Noop.无操作
        }
    }
}

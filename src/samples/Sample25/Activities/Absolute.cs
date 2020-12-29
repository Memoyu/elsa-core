using System;
using System.Threading;
using System.Threading.Tasks;
using Elsa.Expressions;
using Elsa.Models;
using Elsa.Results;
using Elsa.Services;
using Elsa.Services.Models;
using Newtonsoft.Json;

namespace Sample25.Activities
{
    public class Absolute : Activity
    {
        public WorkflowExpression<double> ValueExpression
        {
            get => GetState<WorkflowExpression<double>>();
            set => SetState(value);
        }

        protected override async Task<ActivityExecutionResult> OnExecuteAsync(WorkflowExecutionContext context, CancellationToken cancellationToken)
        {
            var value = await context.EvaluateAsync(ValueExpression, cancellationToken);
            var result = Math.Abs(value);
            //全局动态加参数
            context.Workflow.Input.Add("dyGloVar", new Variable("动态全局加参数"));

            //获取全局参数（流程启动定义）
            var val1 = context.Workflow.Input.GetVariable<string>("gloVar1");
            Console.WriteLine("全局参数-启动定义：" + val1?.ToString());

            //设置出参
            Output.SetVariable("Result", result);

            //获取全局参数（启动时配置）
            var val2 = context.GetVariable("glo");
            Console.WriteLine("全局参数-启动：" + val2?.ToString());

            //获取全局参数（节点设置）
            var val = context.GetVariable("luna");
            Console.WriteLine("全局参数-节点：" + val?.ToString());

            //全局动态修改参数
            context.Workflow.Input.SetVariable("glo", new Variable("动态全局修改参数"));


            //获取全局动态加参数（动态设置）
            var val3 = context.Workflow.Input.GetVariable("dyGloVar");
            Console.WriteLine("全局参数-动态：" + val3?.ToString());

            //获取全局动态修改参数（动态设置）
            var val4 = context.Workflow.Input.GetVariable("glo");
            Console.WriteLine("全局参数-动态：" + val4?.ToString());


            var json = JsonConvert.SerializeObject(context.Workflow.Definition);

            return Done();
        }
    }
}
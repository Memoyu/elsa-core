using System;
using System.Collections.Generic;
using System.Linq;
using Elsa.Models;
using Elsa.Services.Extensions;
using Elsa.Services.Models;
using Newtonsoft.Json.Linq;
using NodaTime;
using Connection = Elsa.Services.Models.Connection;

namespace Elsa.Services
{
    public class WorkflowFactory : IWorkflowFactory
    {
        private readonly IActivityResolver activityResolver;
        private readonly Func<IWorkflowBuilder> workflowBuilder;
        private readonly IClock clock;
        private readonly IIdGenerator idGenerator;

        public WorkflowFactory(
            IActivityResolver activityResolver,
            Func<IWorkflowBuilder> workflowBuilder,
            IClock clock,
            IIdGenerator idGenerator)
        {
            this.activityResolver = activityResolver;
            this.workflowBuilder = workflowBuilder;
            this.clock = clock;
            this.idGenerator = idGenerator;
        }

        public Workflow CreateWorkflow<T>(
            Variables input = default,
            WorkflowInstance workflowInstance = default,
            string correlationId = default) where T : IWorkflow, new()
        {
            var workflowDefinition = workflowBuilder().Build<T>();
            return CreateWorkflow(workflowDefinition, input, workflowInstance, correlationId);
        }
        /// <summary>
        /// 创建工作流
        /// </summary>
        /// <param name="definition"></param>
        /// <param name="input"></param>
        /// <param name="workflowInstance"></param>
        /// <param name="correlationId"></param>
        /// <returns></returns>
        public Workflow CreateWorkflow(
            WorkflowDefinitionVersion definition,
            Variables input = default,
            WorkflowInstance workflowInstance = default,
            string correlationId = default)
        {
            if(definition.IsDisabled)//工作流定义已经禁用
                throw new InvalidOperationException("Cannot instantiate disabled workflow definitions.");
            
            var activities = CreateActivities(definition.Activities).ToList();//创建节点
            var connections = CreateConnections(definition.Connections, activities);//创建连线
            var id = idGenerator.Generate();//生成Id
            var workflow = new Workflow(
                id,
                definition,
                clock.GetCurrentInstant(),
                activities,
                connections,
                input,
                correlationId);

            if (workflowInstance != default)
                workflow.Initialize(workflowInstance);

            return workflow;
        }
        /// <summary>
        /// 创建Activity连接线
        /// </summary>
        /// <param name="connectionBlueprints"></param>
        /// <param name="activities"></param>
        /// <returns></returns>
        private IEnumerable<Connection> CreateConnections(
            IEnumerable<ConnectionDefinition> connectionBlueprints,
            IEnumerable<IActivity> activities)
        {
            var activityDictionary = activities.ToDictionary(x => x.Id);//将Activities转换成Dictionary
            return connectionBlueprints.Select(x => CreateConnection(x, activityDictionary));
        }
        /// <summary>
        /// 创建Activities
        /// </summary>
        /// <param name="activityBlueprints"></param>
        /// <returns></returns>
        private IEnumerable<IActivity> CreateActivities(IEnumerable<ActivityDefinition> activityBlueprints)
        {
            return activityBlueprints.Select(CreateActivity);
        }

        /// <summary>
        /// 创建Activity操作
        /// </summary>
        /// <param name="definition"></param>
        /// <returns></returns>
        private IActivity CreateActivity(ActivityDefinition definition)
        {
            var activity = activityResolver.ResolveActivity(definition.Type);//获取Activity实例

            activity.State = new JObject(definition.State);
            activity.Id = definition.Id;

            return activity;
        }
        /// <summary>
        /// 创建连接线
        /// </summary>
        /// <param name="connectionDefinition"></param>
        /// <param name="activityDictionary"></param>
        /// <returns></returns>
        private Connection CreateConnection(
            ConnectionDefinition connectionDefinition,
            IDictionary<string, IActivity> activityDictionary)
        {
            var source = activityDictionary[connectionDefinition.SourceActivityId];
            var target = activityDictionary[connectionDefinition.DestinationActivityId];
            return new Connection(source, target, connectionDefinition.Outcome);
        }
    }
}
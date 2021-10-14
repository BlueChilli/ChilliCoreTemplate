using ChilliCoreTemplate.Models;
using ChilliCoreTemplate.Service;
using ChilliCoreTemplate.Web.Tasks;
using ChilliSource.Cloud.Core.Distributed;
using DryIoc;
using DryIoc.Microsoft.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;

namespace ChilliCoreTemplate.Web
{
    public static class TaskDescription
    {
        public static Guid ErrorLogTask_Id { get { return new Guid("D6713FDA-E468-4029-A83E-5D014DD08949"); } }
        public static Guid EmailTask_Id { get { return new Guid("D83D5A5A-D7A3-46BD-A9DA-3B4F518720DF"); } }
        public static Guid SmsTask_Id { get { return new Guid("2d2bbe82-51c9-45d0-8e89-4767cd22166a"); } }
        public static Guid CleanUpTask_Id { get { return new Guid("781569FE-A731-4995-8F38-44C52C396C14"); } }
        public static Guid WebhookTask_Id { get { return new Guid("8AE66995-49DD-45E9-B861-777A10D3ACA9"); } }
    }

    public class TaskConfig
    {
        IServiceProvider _parentProvider;
        public TaskConfig(IServiceProvider parentProvider)
        {
            _parentProvider = parentProvider;
        }

        public void RegisterTasks()
        {
            var manager = _parentProvider.GetRequiredService<ITaskManager>();

            manager.RegisterTaskType(typeof(EmailDeliveryTask), new TaskSettings(TaskDescription.EmailTask_Id));
            manager.EnqueueRecurrentTask<EmailDeliveryTask>((long)TimeSpan.FromSeconds(10).TotalMilliseconds);

            manager.RegisterTaskType(typeof(SmsDeliveryTask), new TaskSettings(TaskDescription.SmsTask_Id));
            manager.EnqueueRecurrentTask<SmsDeliveryTask>((long)TimeSpan.FromSeconds(20).TotalMilliseconds);

            manager.RegisterTaskType(typeof(CleanUpTask), new TaskSettings(TaskDescription.CleanUpTask_Id));
            manager.EnqueueRecurrentTask<CleanUpTask>((long)TimeSpan.FromHours(1).TotalMilliseconds);

            manager.RegisterTaskType(typeof(ErrorLogTask), new TaskSettings(TaskDescription.ErrorLogTask_Id));
            manager.EnqueueRecurrentTask<ErrorLogTask>((long)TimeSpan.FromSeconds(300).TotalMilliseconds);

            manager.RegisterTaskType(typeof(WebhookTask), new TaskSettings(TaskDescription.WebhookTask_Id));
            manager.EnqueueRecurrentTask<WebhookTask>((long)TimeSpan.FromSeconds(10).TotalMilliseconds);
        }

        public void StartListenner()
        {
            SetupScopeContextFactory();

            var manager = _parentProvider.GetRequiredService<ITaskManager>();

            manager.StartListener(10000);
        }

        private void SetupScopeContextFactory()
        {
            if (DryIocSetup.MvcContainer == null)
                throw new ApplicationException("DryIocSetup.MvcContainer not setup");

            var container = DryIocSetup.MvcContainer.CreateFacade(facadeKey: "TaskConfig");
            container.Register<IPrincipal, BackgroundTaskPrincipal>(reuse: Reuse.Transient);

            ScopeContextFactory.Instance = container.BuildServiceProvider();
        }
    }

    public class BackgroundTaskPrincipal : GenericPrincipal
    {
        public BackgroundTaskPrincipal()
            : base(new GenericIdentity("BackgroundTaskIdentity"), new string[] { AccountCommon.System })
        {
        }
    }
}

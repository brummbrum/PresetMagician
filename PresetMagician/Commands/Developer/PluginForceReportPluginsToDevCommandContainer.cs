﻿using System.Collections.Generic;
using System.Linq;
using Catel.IoC;
using Catel.MVVM;
using PresetMagician.Core.Models;

// ReSharper disable once CheckNamespace
namespace PresetMagician
{
    // ReSharper disable once UnusedMember.Global
    public class PluginForceReportPluginsToDevCommandContainer : AbstractReportPluginsCommandContainer
    {
        public PluginForceReportPluginsToDevCommandContainer(ICommandManager commandManager,
            IServiceLocator serviceLocator) : base(
            Commands.Plugin.ForceReportPluginsToDev, commandManager, serviceLocator)
        {
            ReportAll = true;
        }

        protected override bool CanExecute(object parameter)
        {
            return true;
        }

        protected override List<Plugin> GetPluginsToReport()
        {
            return (from plugin in GlobalService.Plugins
                where plugin.HasMetadata
                select plugin).ToList();
        }
    }
}
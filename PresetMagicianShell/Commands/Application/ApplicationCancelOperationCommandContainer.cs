﻿using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Catel;
using Catel.MVVM;
using Catel.Threading;
using PresetMagicianShell.Extensions;
using PresetMagicianShell.Models;
using PresetMagicianShell.Services.Interfaces;

// ReSharper disable once CheckNamespace
namespace PresetMagicianShell
{
    // ReSharper disable once UnusedMember.Global
    public class ApplicationCancelOperationCommandContainer : CommandContainerBase
    {
        private readonly IApplicationService _applicationService;
    

        public ApplicationCancelOperationCommandContainer(ICommandManager commandManager,
            IApplicationService applicationService)
            : base(Commands.Application.CancelOperation, commandManager)
        {
            Argument.IsNotNull(() => applicationService);

            _applicationService = applicationService;
        }

        protected override async Task ExecuteAsync(object parameter)
        {
            _applicationService.CancelApplicationOperation();
        }
    }
}
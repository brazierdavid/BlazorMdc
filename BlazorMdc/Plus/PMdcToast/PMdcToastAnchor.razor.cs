﻿using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace BlazorMdc
{
    /// <summary>
    /// An anchor component that displays toast notification that you display via
    /// <see cref="IPmdcToastService.ShowToast(PMdcToastLevel, string, string, PMdcToastCloseMethod?, string, string, IMdcIconFoundry?, bool?, uint?)"/>.
    /// Place this component at the top of either App.razor or MainLayout.razor.
    /// </summary>
    public partial class PMdcToastAnchor : ComponentBase
    {
        [Inject] private IPmdcToastService ToastService { get; set; }

        private List<ToastInstance> DisplayedToasts { get; set; } = new List<ToastInstance>();
        private Queue<ToastInstance> PendingToasts { get; set; } = new Queue<ToastInstance>();
        private readonly SemaphoreSlim displayedToastsSemaphore = new SemaphoreSlim(1);
        private readonly SemaphoreSlim pendingToastsSemaphore = new SemaphoreSlim(1);
        private string positionClass => $"bmdc-toast__{ToastService.Configuration.Position.ToString().ToLower()}";


        /// <inheritdoc/>
        protected override void OnInitialized()
        {
            ToastService.OnAdd += AddToast;
        }


        private void AddToast(PMdcToastLevel level, PMdcToastSettings settings)
        {
            InvokeAsync(async () =>
            {
                settings.Configuration = ToastService.Configuration;
                settings.Level = level;

                var toastInstance = new ToastInstance
                {
                    Id = Guid.NewGuid(),
                    TimeStamp = DateTime.Now,
                    Settings = settings
                };

                await pendingToastsSemaphore.WaitAsync();

                try
                {
                    PendingToasts.Enqueue(toastInstance);

                    await displayedToastsSemaphore.WaitAsync();

                    try
                    {
                        FlushPendingToasts();
                    }
                    finally
                    {
                        displayedToastsSemaphore.Release();
                    }
                }
                finally
                {
                    pendingToastsSemaphore.Release();
                }

                StateHasChanged();
            });
        }


        private void FlushPendingToasts()
        {
            bool FlushNext() => PendingToasts.Count() > 0 && (ToastService.Configuration.MaxToastsShowing <= 0 || DisplayedToasts.Where(t => t.Settings.Status != ToastStatus.Hide).Count() < ToastService.Configuration.MaxToastsShowing);

            while (FlushNext())
            {
                var toastInstance = PendingToasts.Dequeue();

                DisplayedToasts.Add(toastInstance);

                if (toastInstance.Settings.AppliedCloseMethod != PMdcToastCloseMethod.CloseButton)
                {
                    InvokeAsync(() =>
                    {
                        var timeout = toastInstance.Settings.AppliedTimeout;
                        var toastTimer = new System.Timers.Timer(toastInstance.Settings.AppliedTimeout);
                        toastTimer.Elapsed += (sender, args) => { CloseToast(toastInstance.Id); };
                        toastTimer.AutoReset = false;
                        toastTimer.Start();
                    });
                }
            }

            StateHasChanged();
        }


        public void CloseToast(Guid toastId)
        {
            InvokeAsync(async () =>
            {

                await displayedToastsSemaphore.WaitAsync();

                try
                {
                    var toastInstance = DisplayedToasts.SingleOrDefault(x => x.Id == toastId);

                    if (toastInstance is null)
                    {
                        return;
                    }

                    toastInstance.Settings.Status = ToastStatus.FadeOut;
                    StateHasChanged();
                }
                finally
                {
                    displayedToastsSemaphore.Release();
                }

                var toastTimer = new System.Timers.Timer(500);
                toastTimer.Elapsed += (sender, args) => { RemoveToast(toastId); };
                toastTimer.AutoReset = false;
                toastTimer.Start();

                StateHasChanged();
            });
        }


        private void RemoveToast(Guid toastId)
        {
            InvokeAsync(async () =>
            {
                await displayedToastsSemaphore.WaitAsync();

                try
                {
                    var toastInstance = DisplayedToasts.SingleOrDefault(x => x.Id == toastId);

                    if (toastInstance is null)
                    {
                        return;
                    }

                    toastInstance.Settings.Status = ToastStatus.Hide;

                    if (DisplayedToasts.Where(x => x.Settings.Status == ToastStatus.FadeOut).Count() == 0)
                    {
                        DisplayedToasts.RemoveAll(x => x.Settings.Status == ToastStatus.Hide);
                    }

                    StateHasChanged();

                    FlushPendingToasts();
                }
                finally
                {
                    displayedToastsSemaphore.Release();
                }
            });
        }
    }
}

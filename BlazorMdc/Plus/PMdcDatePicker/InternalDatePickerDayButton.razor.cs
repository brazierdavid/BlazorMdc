﻿using Microsoft.AspNetCore.Components;
using System;
using System.Threading.Tasks;

namespace BlazorMdc
{
    /// <summary>
    /// For BlazorMdc internal use only.
    /// </summary>
    public partial class InternalDatePickerDayButton : MdcComponentBase
    {
        /// <summary>
        /// The currently selected date.
        /// </summary>
        [Parameter] public DateTime CurrentDate { get; set; }


        /// <summary>
        /// The date being displayed by this button
        /// </summary>
        [Parameter] public DateTime DisplayDate { get; set; }


        /// <summary>
        /// First day of the month being displayed
        /// </summary>
        [Parameter] public DateTime StartOfDisplayMonth { get; set; }


        /// <summary>
        /// Callback returning the date if the button is clicked
        /// </summary>
        [Parameter] public EventCallback<DateTime> OnItemClickAsync { get; set; }


        /// <summary>
        /// Date selection criteria
        /// </summary>
        [Parameter] public PMdcDateSelectionCriteria? DateSelectionCriteria { get; set; }


        /// <summary>
        /// Minimum date set by the consumer
        /// </summary>
        [Parameter] public DateTime MinDate { get; set; }


        /// <summary>
        /// Maximum date set by the consumer
        /// </summary>
        [Parameter] public DateTime MaxDate { get; set; }


        private string Day => DisplayDate.Day.ToString();
        private MdcButtonStyle ButtonStyle => (DisplayDate == CurrentDate) ? MdcButtonStyle.ContainedUnelevated : MdcButtonStyle.Text;

        
        private bool ButtonDisabled
        {
            get
            {
                if (DisplayDate.Month != StartOfDisplayMonth.Month)
                {
                    return true;
                }

                var criteria = CascadingDefaults.AppliedDateSelectionCriteria(DateSelectionCriteria);

                switch (criteria)
                {
                    case PMdcDateSelectionCriteria.WeekendsOnly:
                        if ((DisplayDate.DayOfWeek != DayOfWeek.Sunday) && (DisplayDate.DayOfWeek != DayOfWeek.Saturday))
                        {
                            return true;
                        }
                        break;

                    case PMdcDateSelectionCriteria.WeekdaysOnly:
                        if ((DisplayDate.DayOfWeek == DayOfWeek.Sunday) || (DisplayDate.DayOfWeek == DayOfWeek.Saturday))
                        {
                            return true;
                        }
                        break;
                }

                return DisplayDate < MinDate || DisplayDate > MaxDate;
            }
        }


        private async Task OnClickAsync() => await OnItemClickAsync.InvokeAsync(DisplayDate);
    }
}

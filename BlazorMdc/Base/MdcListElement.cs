﻿using System;

namespace BlazorMdc
{
    /// <summary>
    /// A list item used by <see cref="MdcSelect{TItem}"/>, <see cref="PMdcRadioButtonGroup{TItem}"/> and <see cref="PMdcPagedDataList{TItem}"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class MdcListElement<T>
    {
        /// <summary>
        /// The value associated with the list element.
        /// </summary>
        public T SelectedValue { get; set; }


        /// <summary>
        /// The string label expressing the value.
        /// </summary>
        public string Label { get; set; }


        /// <summary>
        /// A CSS class used by <see cref="MdcRadioButton{TItem}"/> one div inside the outer <c>"mdc-radio"</c> div
        /// </summary>
        [Obsolete]
        public string ButtonContainerClass { get; set; }


        /// <summary>
        /// Determines whether the list item is to be disabled
        /// </summary>
        public bool Disabled { get; set; } = false;
    }
}

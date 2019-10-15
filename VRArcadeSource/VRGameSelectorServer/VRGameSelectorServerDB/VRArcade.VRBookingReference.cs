﻿//------------------------------------------------------------------------------
// This is auto-generated code.
//------------------------------------------------------------------------------
// This code was generated by Entity Developer tool using Telerik Data Access template.
// Code is generated on: 3/3/2019 8:16:36 PM
//
// Changes to this file may cause incorrect behavior and will be lost if
// the code is regenerated.
//------------------------------------------------------------------------------

using System;
using System.Data;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Data.Common;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Telerik.OpenAccess;
using Telerik.OpenAccess.Metadata;
using Telerik.OpenAccess.Data.Common;
using Telerik.OpenAccess.Metadata.Fluent;
using Telerik.OpenAccess.Metadata.Fluent.Advanced;

namespace VRGameSelectorServerDB
{
    public partial class VRBookingReference {

        public VRBookingReference()
        {
            this.WaiverLogs = new List<VRWaiverLog>();
            OnCreated();
        }

        public virtual int ID
        {
            get;
            set;
        }

        public virtual string Reference
        {
            get;
            set;
        }

        public virtual bool IsNonTimedTiming
        {
            get;
            set;
        }

        public virtual bool IsTimedTiming
        {
            get;
            set;
        }

        public virtual int Duration
        {
            get;
            set;
        }

        public virtual int NumberOfBookingTotal
        {
            get;
            set;
        }

        public virtual int NumberOfBookingLeft
        {
            get;
            set;
        }

        public virtual System.DateTime TimeStampCreate
        {
            get;
            set;
        }

        public virtual System.Nullable<System.DateTime> BookingChanged
        {
            get;
            set;
        }

        public virtual System.Nullable<System.DateTime> BookingDeleted
        {
            get;
            set;
        }

        public virtual System.Nullable<System.DateTime> BookingStartTime
        {
            get;
            set;
        }

        public virtual System.Nullable<System.DateTime> BookingEndTime
        {
            get;
            set;
        }

        public virtual IList<VRWaiverLog> WaiverLogs
        {
            get;
            set;
        }
    
        #region Extensibility Method Definitions

        partial void OnCreated();
        
        #endregion
    }

}

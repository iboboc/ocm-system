﻿using System;
using System.Collections.Generic;

namespace OCM.Core.Data
{
    public partial class Country
    {
        public Country()
        {
            AddressInfoes = new HashSet<AddressInfo>();
            Statistics = new HashSet<Statistic>();
            UserSubscriptions = new HashSet<UserSubscription>();
        }

        public int Id { get; set; }
        public string Title { get; set; }
        public string Isocode { get; set; }
        public string ContinentCode { get; set; }

        public virtual ICollection<AddressInfo> AddressInfoes { get; set; }
        public virtual ICollection<Statistic> Statistics { get; set; }
        public virtual ICollection<UserSubscription> UserSubscriptions { get; set; }
    }
}

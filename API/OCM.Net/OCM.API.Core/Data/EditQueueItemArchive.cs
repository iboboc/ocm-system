﻿using System;
using System.Collections.Generic;

namespace OCM.Core.Data
{
    public partial class EditQueueItemArchive
    {
        public int Id { get; set; }
        public int? UserId { get; set; }
        public string Comment { get; set; }
        public bool IsProcessed { get; set; }
        public int? ProcessedByUserId { get; set; }
        public DateTime DateSubmitted { get; set; }
        public DateTime? DateProcessed { get; set; }
        public string EditData { get; set; }
        public string PreviousData { get; set; }
        public int? EntityId { get; set; }
        public short? EntityTypeId { get; set; }
    }
}

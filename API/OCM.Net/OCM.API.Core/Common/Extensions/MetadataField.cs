﻿using System.Collections.Generic;

namespace OCM.API.Common.Model.Extensions
{
    public class MetadataField
    {
        public static Model.MetadataField FromDataModel(Core.Data.MetadataField source)
        {
            if (source == null) return null;

            var f = new Model.MetadataField
            {
                ID = source.Id,
                Title = source.Title,
                MetadataGroupID = source.MetadataGroupId,
                DataTypeID = source.DataTypeId
            };

            if (source.MetadataFieldOptions != null)
            {

                foreach (var o in source.MetadataFieldOptions)
                {
                    if (f.MetadataFieldOptions == null) f.MetadataFieldOptions = new List<Model.MetadataFieldOption>();
                    f.MetadataFieldOptions.Add(Extensions.MetadataFieldOption.FromDataModel(o));
                }
            }
            return f;
        }
    }
}
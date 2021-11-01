﻿namespace OCM.API.Common.Model.Extensions
{
    public class DataType
    {
        public static Model.DataType FromDataModel(Core.Data.DataType source)
        {
            if (source == null) return null;

            return new Model.DataType
            {
                ID = source.Id,
                Title = source.Title
            };
        }
    }
}
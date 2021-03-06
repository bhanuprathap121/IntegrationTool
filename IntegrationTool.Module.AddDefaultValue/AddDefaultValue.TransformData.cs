﻿using IntegrationTool.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegrationTool.Module.AddDefaultValue
{
    public partial class AddDefaultValue
    {
        public void TransformData(IConnection connection, SDK.Database.IDatabaseInterface databaseInterface, SDK.Database.IDatastore dataObject, ReportProgressMethod reportProgress)
        {
            foreach (var transformation in this.Configuration.DefaultValues)
            {
                reportProgress(new SimpleProgressReport("Start add default value column " + transformation.ColumnName + ". Value: " + transformation.Value));

                dataObject.AddColumnMetadata(new ColumnMetadata(dataObject.Metadata.Columns.Count, transformation.ColumnName));

                for (int i = 0; i < dataObject.Count; i++)
                {
                    dataObject.SetValue(i, (int)(dataObject.Metadata.Columns.Count - 1), transformation.Value);
                }

                reportProgress(new SimpleProgressReport("Finished add default value column " + transformation.ColumnName + ". Value: " + transformation.Value));
            }
        }
    }
}

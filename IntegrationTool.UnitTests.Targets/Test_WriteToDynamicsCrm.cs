﻿using IntegrationTool.DBAccess;
using IntegrationTool.Module.ConnectToDynamicsCrm;
using IntegrationTool.Module.WriteToDynamicsCrm;
using IntegrationTool.SDK;
using IntegrationTool.SDK.Database;
using IntegrationTool.UnitTests.Targets.Properties;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegrationTool.UnitTests.Targets
{
    [TestClass]
    public class Test_WriteToDynamicsCrm
    {
        private static Guid CRMCONNECTIONID = new Guid("4D3F2E27-71EC-4631-8E98-5915E99FCED2");
        private static IConnection connection;

        [ClassInitialize]
        public static void InitializeCrm2013Wrapper(TestContext context)
        {
            ConnectToDynamicsCrmConfiguration configuration = new ConnectToDynamicsCrmConfiguration();
            configuration.ConnectionString = Settings.Default.CrmConnectionString;

            connection = new ConnectToDynamicsCrm() { Configuration = configuration };
        }

        [TestMethod]
        public void Test_CorrectImport()
        {
            IntegrationTool.Module.WriteToDynamicsCrm.WriteToDynamicsCrmConfiguration writeToCrmConfig = new IntegrationTool.Module.WriteToDynamicsCrm.WriteToDynamicsCrmConfiguration();
            writeToCrmConfig.EntityName = "contact";
            writeToCrmConfig.PrimaryKeyAttributes.Add("new_id");
            writeToCrmConfig.ImportMode = Module.WriteToDynamicsCrm.SDK.Enums.ImportMode.All;
            writeToCrmConfig.MultipleFoundMode = Module.WriteToDynamicsCrm.SDK.Enums.MultipleFoundMode.All;
            writeToCrmConfig.Mapping.Add(new IntegrationTool.DataMappingControl.DataMapping() { Source = "ID", Target = "new_id" });
            writeToCrmConfig.Mapping.Add(new IntegrationTool.DataMappingControl.DataMapping() { Source = "FirstName", Target="firstname" });
            writeToCrmConfig.Mapping.Add(new IntegrationTool.DataMappingControl.DataMapping() { Source = "LastName", Target = "lastname" });
            writeToCrmConfig.Mapping.Add(new IntegrationTool.DataMappingControl.DataMapping() { Source = "Status", Target = "statuscode" });
            writeToCrmConfig.RelationMapping.Add(new Module.WriteToDynamicsCrm.SDK.RelationMapping()
                {
                    EntityName = "account",
                    LogicalName = "parentcustomerid",
                    Mapping = new List<DataMappingControl.DataMapping>() { new DataMappingControl.DataMapping()
                    {
                        Source ="CompanyName",
                        Target = "name"
                    } }
                });
            writeToCrmConfig.ConfigurationId = Guid.NewGuid();
            writeToCrmConfig.SelectedConnectionConfigurationId = CRMCONNECTIONID;
            writeToCrmConfig.PicklistMapping.Add(new Module.WriteToDynamicsCrm.SDK.PicklistMapping()
            {
                LogicalName = "statuscode",
                MappingType = Module.WriteToDynamicsCrm.SDK.Enums.PicklistMappingType.Manual,
                Mapping = new List<DataMappingControl.DataMapping>() 
                { 
                    new DataMappingControl.DataMapping() 
                    {
                        Source = "Active", Target = "1"
                    },
                    new DataMappingControl.DataMapping() 
                    {
                        Source = "Inactive", Target = "2"
                    }
                }
            });
            IDatastore dataObject = new IntegrationTool.SDK.DataObject();
            dataObject.AddColumnMetadata(new ColumnMetadata(0, "FirstName"));
            dataObject.AddColumnMetadata(new ColumnMetadata(1, "LastName"));
            dataObject.AddColumnMetadata(new ColumnMetadata(2, "City"));
            dataObject.AddColumnMetadata(new ColumnMetadata(3, "ID"));
            dataObject.AddColumnMetadata(new ColumnMetadata(4, "CompanyName"));
            dataObject.AddColumnMetadata(new ColumnMetadata(5, "Status"));

            dataObject.AddData(new object[] { "Peter", "Widmer", "Wettingen", 1001, "Best o' Things (sample)", "Active" });
            dataObject.AddData(new object[] { "Joachim 2", "Suter", "Dättwil", 1002, "Litware Inc. (sample)", "Inactive" });
            dataObject.AddData(new object[] { "James", "Brown", "London", 1003, null, "Active" });

            IModule module = Activator.CreateInstance(typeof(WriteToDynamicsCrm)) as IModule;
            module.SetConfiguration(writeToCrmConfig);

            ((IDataTarget)module).WriteData(connection, new DummyDatabaseInterface(), dataObject, ReportProgressMethod);
        }

        private void ReportProgressMethod(SimpleProgressReport progress)
        {
            
        }   
    }
}

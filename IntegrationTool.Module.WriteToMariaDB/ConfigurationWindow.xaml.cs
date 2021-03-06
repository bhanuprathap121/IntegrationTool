﻿using IntegrationTool.DBAccess;
using IntegrationTool.Module.DbCommons;
using IntegrationTool.Module.WriteToMySQL;
using IntegrationTool.Module.WriteToMySQL.UserControls;
using IntegrationTool.Module.WriteToMySQL.UserControls.Mapping;
using IntegrationTool.SDK;
using IntegrationTool.SDK.Controls.Generic;
using IntegrationTool.SDK.Database;
using IntegrationTool.SDK.GenericClasses;
using IntegrationTool.SDK.GenericControls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Odbc;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace IntegrationTool.Module.WriteToMySql
{
    /// <summary>
    /// Interaction logic for ConfigurationWindow.xaml
    /// </summary>
    public partial class ConfigurationWindow : UserControl, IConnectionChanged
    {
        private IDatastore dataObject;
        private WriteToMySQLConfiguration configuration;
        private IConnection connection;
        private MySQLDbMetadataProvider metadataProvider;

        // UserControls
        private AttributeMapping attributeMapping;
        private ImportSettings existingCheck;
        private RelationMapping relationMapping;

        public ConfigurationWindow(WriteToMySQLConfiguration configuration, IDatastore dataObject)
        {
            InitializeComponent();
            this.dataObject = dataObject;
            this.configuration = configuration;
        }

        public void ConnectionChanged(IConnection connection)
        {
            this.connection = connection;
            ConfigurationContent.Content = new LoadingControl();
            ddTargetTables.IsEnabled = false;

            BackgroundWorker bgwConnectionChanged = new BackgroundWorker();
            bgwConnectionChanged.DoWork += bgwConnectionChanged_DoWork;
            bgwConnectionChanged.RunWorkerCompleted += bgwConnectionChanged_RunWorkerCompleted;
            bgwConnectionChanged.RunWorkerAsync();     
        }

        void bgwConnectionChanged_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                MySQLDbMetadataProvider mySQLDbMetadataProvider = new MySQLDbMetadataProvider(this.connection);
                mySQLDbMetadataProvider.Initialize();

                e.Result = new object[] { mySQLDbMetadataProvider };
            }
            catch (Exception ex)
            {
                e.Result = new object[] { ex };
            }
        }

        void bgwConnectionChanged_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.metadataProvider = ((object[])e.Result)[0] as MySQLDbMetadataProvider;
            if (this.metadataProvider != null)
            {
                ConfigurationContent.Content = null;
                CommonDbInitializationHelper.InitializeTargetTable(this.metadataProvider, ddTargetTables, configuration.TargetTable);   
            }

            Exception error = ((object[])e.Result)[0] as Exception;
            if (error != null)
            {
                string message = error.Message + ((error.InnerException != null) ? "\n" + error.InnerException.Message : "");
                ConfigurationContent.Content = new MessageControl("An error occured:", message);
            }
        }        

        private void ddTargetTables_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(ddTargetTables.SelectedItem == null)
            {
                return;
            }

            DbMetadataTable selectedTable = ((ComboBoxItem)ddTargetTables.SelectedItem).Tag as DbMetadataTable;
            this.configuration.TargetTable = selectedTable.TableName;

            attributeMapping = CreateAttributeMappingWindow();
            existingCheck = CreateExistingCheckWindow();
            relationMapping = CreateRelationmappingWindow();

            ConfigurationContent.Content = new ConfigurationContent(attributeMapping, existingCheck, relationMapping);   
        }

        private ImportSettings CreateExistingCheckWindow()
        {
            ImportSettings existingCheck = new ImportSettings(this.configuration);
            foreach (var mapping in this.configuration.Mapping.OrderBy(t => t.Target))
            {
                existingCheck.AvailablePrimaryKeyAttributes.Add(new NameDisplayName(mapping.Target, mapping.Target));
            }
            return existingCheck;
        }

        private AttributeMapping CreateAttributeMappingWindow()
        {
            // TODO Implement
            return null;
        }

        private RelationMapping CreateRelationmappingWindow()
        {
            // TODO Implement
            return null;
        }
    }
}

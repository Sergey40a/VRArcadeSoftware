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
    public partial class VRArcadeDataAccessModelMetadataSource
    {
        public MappingConfiguration<VRClient> GetVRClientMappingConfiguration()
        {
            MappingConfiguration<VRClient> configuration = this.GetVRClientClassConfiguration();
            this.PrepareVRClientConfigurations(configuration);
            this.OnPrepareVRClientConfigurations(configuration);
            return configuration;
        }

        public MappingConfiguration<VRClient> GetVRClientClassConfiguration()
        {
            MappingConfiguration<VRClient> configuration = new MappingConfiguration<VRClient>();
            configuration.MapType(x => new { }).WithConcurencyControl(OptimisticConcurrencyControlStrategy.Changed).ToTable("VRClients");
            return configuration;
        }
	
        public void PrepareVRClientConfigurations(MappingConfiguration<VRClient> configuration)
        {
            configuration.HasProperty(x => x.ID).ToColumn(@"ID").IsIdentity(KeyGenerator.Autoinc).WithOpenAccessType(OpenAccessType.Int32).IsNotNullable().HasPrecision(11);
            configuration.HasProperty(x => x.MachineName).ToColumn(@"MachineName").WithOpenAccessType(OpenAccessType.Varchar).IsNotNullable().HasLength(255).IsUnicode();
            configuration.HasProperty(x => x.IPAddress).ToColumn(@"IPAddress").WithOpenAccessType(OpenAccessType.Varchar).IsNotNullable().HasLength(255).IsUnicode();
            configuration.HasProperty(x => x.DashboardModuleIP).ToColumn(@"DashboardModuleIP").WithOpenAccessType(OpenAccessType.Varchar).IsNullable().IsUnicode();
            configuration.HasProperty(x => x.TileConfigSetID).ToColumn(@"TileConfigSetID").WithOpenAccessType(OpenAccessType.Int32).IsNotNullable().HasPrecision(11);
            configuration.HasProperty(x => x.IsDeleted).ToColumn(@"IsDeleted").WithOpenAccessType(OpenAccessType.Boolean).IsNotNullable();
            configuration.HasAssociation<VRTileconfigset>(x => x.Tileconfigset).WithOpposite(op => op.Clients).HasConstraint((x, y) =>  x.TileConfigSetID == y.ID);
            configuration.HasAssociation<VRClienthistory>(x => x.VRClienthistories).WithOpposite(op => op.VRClient).HasConstraint((x, y) =>  x.ID == y.VRClientID);
            configuration.HasAssociation<VRTicket>(x => x.VRTickets).WithOpposite(op => op.VRClient).HasConstraint((x, y) =>  x.ID == y.VRClientID);
            configuration.HasAssociation<VRManageLog>(x => x.VRManageLogs).WithOpposite(op => op.VRClient).HasConstraint((x, y) =>  x.ID == y.VRClientID);
        }

        partial void OnPrepareVRClientConfigurations(MappingConfiguration<VRClient> configuration);
    }
}

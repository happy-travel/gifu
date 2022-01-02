﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#pragma warning disable 219, 612, 618
#nullable enable

namespace HappyTravel.Gifu.Data.CompiledModels
{
    public partial class GifuContextModel
    {
        partial void Initialize()
        {
            var amountChangesHistory = AmountChangesHistoryEntityType.Create(this);
            var ixarisScheduleLoad = IxarisScheduleLoadEntityType.Create(this);
            var vccDirectEditLog = VccDirectEditLogEntityType.Create(this);
            var vccIssue = VccIssueEntityType.Create(this);

            AmountChangesHistoryEntityType.CreateAnnotations(amountChangesHistory);
            IxarisScheduleLoadEntityType.CreateAnnotations(ixarisScheduleLoad);
            VccDirectEditLogEntityType.CreateAnnotations(vccDirectEditLog);
            VccIssueEntityType.CreateAnnotations(vccIssue);

            AddAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);
            AddAnnotation("ProductVersion", "6.0.1");
            AddAnnotation("Relational:MaxIdentifierLength", 63);
        }
    }
}

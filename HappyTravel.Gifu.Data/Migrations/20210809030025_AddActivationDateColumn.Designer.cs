﻿// <auto-generated />
using System;
using HappyTravel.Gifu.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace HappyTravel.Gifu.Data.Migrations
{
    [DbContext(typeof(GifuContext))]
    [Migration("20210809030025_AddActivationDateColumn")]
    partial class AddActivationDateColumn
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Relational:MaxIdentifierLength", 63)
                .HasAnnotation("ProductVersion", "5.0.6")
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            modelBuilder.Entity("HappyTravel.Gifu.Data.Models.VccIssue", b =>
                {
                    b.Property<string>("TransactionId")
                        .HasColumnType("text");

                    b.Property<DateTime>("ActivationDate")
                        .HasColumnType("timestamp without time zone");

                    b.Property<decimal>("Amount")
                        .HasColumnType("numeric");

                    b.Property<string>("ClientId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("Currency")
                        .HasColumnType("integer");

                    b.Property<DateTime>("DueDate")
                        .HasColumnType("timestamp without time zone");

                    b.Property<string>("ReferenceCode")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("TransactionId");

                    b.ToTable("VccIssues");
                });
#pragma warning restore 612, 618
        }
    }
}

﻿// <auto-generated />
using System;
using BeTiny.Api.Infra.Database.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BeTiny.Api.Infra.Database.Migrations
{
    [DbContext(typeof(BeTinyDbContext))]
    partial class BeTinyDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.16")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("BeTiny.Api.Domain.Entites.Url", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("text");

                    b.Property<long>("Accesses")
                        .HasColumnType("bigint")
                        .HasColumnName("Accesses");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("CreatedAt");

                    b.Property<string>("LongUrl")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)")
                        .HasColumnName("LongUrl");

                    b.HasKey("Id");

                    b.HasIndex("LongUrl")
                        .IsUnique();

                    b.ToTable("Urls", (string)null);
                });
#pragma warning restore 612, 618
        }
    }
}

﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using TrexLock.Persistence;

namespace TrexLock.Migrations
{
    [DbContext(typeof(LockDbContext))]
    [Migration("20201017185249_Initial")]
    partial class Initial
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "3.1.8");

            modelBuilder.Entity("TrexLock.Persistence.Dto.KeyDto", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("TEXT");

                    b.Property<string>("Value")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Keys");
                });

            modelBuilder.Entity("TrexLock.Persistence.Dto.LockDto", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("TEXT");

                    b.Property<int>("State")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime?>("Timeout")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Locks");
                });

            modelBuilder.Entity("TrexLock.Persistence.Dto.PinLogDto", b =>
                {
                    b.Property<int?>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("Pin")
                        .HasColumnType("INTEGER");

                    b.Property<int>("PinState")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Reason")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("Time")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("PinLogs");
                });

            modelBuilder.Entity("TrexLock.Persistence.Dto.SecuritylogDto", b =>
                {
                    b.Property<int?>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Body")
                        .HasColumnType("TEXT");

                    b.Property<string>("Method")
                        .HasColumnType("TEXT");

                    b.Property<string>("RequestOrigin")
                        .HasColumnType("TEXT");

                    b.Property<int>("ResponseCode")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Url")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Securitylogs");
                });
#pragma warning restore 612, 618
        }
    }
}
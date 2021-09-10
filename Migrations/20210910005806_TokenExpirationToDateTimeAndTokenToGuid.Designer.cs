﻿// <auto-generated />
using System;
using System.Collections.Generic;
using ForSakenBorders.Backend.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace ForSakenBorders.Backend.Migrations
{
    [DbContext(typeof(BackendContext))]
    [Migration("20210910005806_TokenExpirationToDateTimeAndTokenToGuid")]
    partial class TokenExpirationToDateTimeAndTokenToGuid
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Relational:MaxIdentifierLength", 63)
                .HasAnnotation("ProductVersion", "5.0.9")
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            modelBuilder.Entity("Kiki.Database.Note", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<string>("Content")
                        .IsRequired()
                        .HasMaxLength(10000)
                        .HasColumnType("character varying(10000)")
                        .HasColumnName("content");

                    b.Property<byte[]>("ContentHash")
                        .HasColumnType("bytea")
                        .HasColumnName("content_hash");

                    b.Property<bool>("IsPrivate")
                        .HasColumnType("boolean")
                        .HasColumnName("is_private");

                    b.Property<Guid?>("OwnerId")
                        .HasColumnType("uuid")
                        .HasColumnName("owner_id");

                    b.Property<List<string>>("Tags")
                        .HasMaxLength(10)
                        .HasColumnType("text[]")
                        .HasColumnName("tags");

                    b.Property<byte[]>("Thumbnail")
                        .HasColumnType("bytea")
                        .HasColumnName("thumbnail");

                    b.Property<byte[]>("ThumbnailHash")
                        .HasColumnType("bytea")
                        .HasColumnName("thumbnail_hash");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)")
                        .HasColumnName("title");

                    b.HasKey("Id")
                        .HasName("pk_notes");

                    b.HasIndex("OwnerId")
                        .HasDatabaseName("ix_notes_owner_id");

                    b.ToTable("notes");
                });

            modelBuilder.Entity("Kiki.Database.Role", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<string>("Description")
                        .HasColumnType("text")
                        .HasColumnName("description");

                    b.Property<byte[]>("Icon")
                        .HasColumnType("bytea")
                        .HasColumnName("icon");

                    b.Property<bool>("IsOfficial")
                        .HasColumnType("boolean")
                        .HasColumnName("is_official");

                    b.Property<string>("Name")
                        .HasColumnType("text")
                        .HasColumnName("name");

                    b.Property<byte>("NotePermissions")
                        .HasColumnType("smallint")
                        .HasColumnName("note_permissions");

                    b.Property<int>("Position")
                        .HasColumnType("integer")
                        .HasColumnName("position");

                    b.Property<Guid?>("UserId")
                        .HasColumnType("uuid")
                        .HasColumnName("user_id");

                    b.Property<byte>("UserPermissions")
                        .HasColumnType("smallint")
                        .HasColumnName("user_permissions");

                    b.HasKey("Id")
                        .HasName("pk_roles");

                    b.HasIndex("UserId")
                        .HasDatabaseName("ix_roles_user_id");

                    b.ToTable("roles");
                });

            modelBuilder.Entity("Kiki.Database.User", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<DateTime>("BanExpiration")
                        .HasColumnType("timestamp without time zone")
                        .HasColumnName("ban_expiration");

                    b.Property<string>("BanReason")
                        .HasColumnType("text")
                        .HasColumnName("ban_reason");

                    b.Property<string>("Email")
                        .HasMaxLength(320)
                        .HasColumnType("character varying(320)")
                        .HasColumnName("email");

                    b.Property<string>("FirstName")
                        .HasMaxLength(32)
                        .HasColumnType("character varying(32)")
                        .HasColumnName("first_name");

                    b.Property<bool>("IsBanned")
                        .HasColumnType("boolean")
                        .HasColumnName("is_banned");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("boolean")
                        .HasColumnName("is_deleted");

                    b.Property<bool>("IsVerified")
                        .HasColumnType("boolean")
                        .HasColumnName("is_verified");

                    b.Property<string>("LastName")
                        .HasMaxLength(32)
                        .HasColumnType("character varying(32)")
                        .HasColumnName("last_name");

                    b.Property<byte[]>("PasswordHash")
                        .HasColumnType("bytea")
                        .HasColumnName("password_hash");

                    b.Property<Guid>("Token")
                        .HasColumnType("uuid")
                        .HasColumnName("token");

                    b.Property<DateTime>("TokenExpiration")
                        .HasColumnType("timestamp without time zone")
                        .HasColumnName("token_expiration");

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("timestamp without time zone")
                        .HasColumnName("updated_at");

                    b.Property<string>("Username")
                        .HasMaxLength(32)
                        .HasColumnType("character varying(32)")
                        .HasColumnName("username");

                    b.HasKey("Id")
                        .HasName("pk_users");

                    b.HasIndex("Email")
                        .IsUnique()
                        .HasDatabaseName("ix_users_email");

                    b.ToTable("users");
                });

            modelBuilder.Entity("Kiki.Database.Note", b =>
                {
                    b.HasOne("Kiki.Database.User", "Owner")
                        .WithMany()
                        .HasForeignKey("OwnerId")
                        .HasConstraintName("fk_notes_users_owner_id");

                    b.Navigation("Owner");
                });

            modelBuilder.Entity("Kiki.Database.Role", b =>
                {
                    b.HasOne("Kiki.Database.User", null)
                        .WithMany("Roles")
                        .HasForeignKey("UserId")
                        .HasConstraintName("fk_roles_users_user_id");
                });

            modelBuilder.Entity("Kiki.Database.User", b =>
                {
                    b.Navigation("Roles");
                });
#pragma warning restore 612, 618
        }
    }
}

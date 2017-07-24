using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using RobloxScraper.DbModels;

namespace RobloxScraper.Migrations
{
    [DbContext(typeof(ForumsContext))]
    [Migration("20170724063530_InitialCreate")]
    partial class InitialCreate
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "1.1.2");

            modelBuilder.Entity("RobloxScraper.DbModels.Forum", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("id");

                    b.Property<int>("ForumGroupId")
                        .HasColumnName("forum_group_id");

                    b.Property<string>("Name")
                        .HasColumnName("name");

                    b.HasKey("Id");

                    b.HasIndex("ForumGroupId");

                    b.ToTable("Forums");
                });

            modelBuilder.Entity("RobloxScraper.DbModels.ForumGroup", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("id");

                    b.Property<string>("Name")
                        .HasColumnName("name");

                    b.HasKey("Id");

                    b.ToTable("ForumGroups");
                });

            modelBuilder.Entity("RobloxScraper.DbModels.Post", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("id");

                    b.Property<string>("Body")
                        .HasColumnName("body");

                    b.Property<int>("ThreadId")
                        .HasColumnName("thread_id");

                    b.Property<DateTime>("Timestamp")
                        .HasColumnName("timestamp");

                    b.Property<int>("UserId")
                        .HasColumnName("user_id");

                    b.HasKey("Id");

                    b.HasIndex("ThreadId");

                    b.HasIndex("UserId");

                    b.ToTable("Posts");
                });

            modelBuilder.Entity("RobloxScraper.DbModels.Thread", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("id");

                    b.Property<string>("Errors")
                        .HasColumnName("errors");

                    b.Property<int?>("ForumId")
                        .HasColumnName("forum");

                    b.Property<bool>("IsEmpty")
                        .HasColumnName("is_empty");

                    b.Property<string>("Title")
                        .HasColumnName("title");

                    b.HasKey("Id");

                    b.HasIndex("ForumId");

                    b.ToTable("Threads");
                });

            modelBuilder.Entity("RobloxScraper.DbModels.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("id");

                    b.Property<string>("Name")
                        .HasColumnName("name");

                    b.HasKey("Id");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("RobloxScraper.DbModels.Forum", b =>
                {
                    b.HasOne("RobloxScraper.DbModels.ForumGroup", "ForumGroup")
                        .WithMany("Forums")
                        .HasForeignKey("ForumGroupId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("RobloxScraper.DbModels.Post", b =>
                {
                    b.HasOne("RobloxScraper.DbModels.Thread", "Thread")
                        .WithMany("Posts")
                        .HasForeignKey("ThreadId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("RobloxScraper.DbModels.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("RobloxScraper.DbModels.Thread", b =>
                {
                    b.HasOne("RobloxScraper.DbModels.Forum", "Forum")
                        .WithMany()
                        .HasForeignKey("ForumId");
                });
        }
    }
}

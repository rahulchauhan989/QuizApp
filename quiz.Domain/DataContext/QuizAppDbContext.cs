using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using quiz.Domain.DataModels;

namespace quiz.Domain.DataContext;

public partial class QuiZappDbContext : DbContext
{
    public QuiZappDbContext()
    {
    }

    public QuiZappDbContext(DbContextOptions<QuiZappDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Option> Options { get; set; }

    public virtual DbSet<Question> Questions { get; set; }

    public virtual DbSet<Quiz> Quizzes { get; set; }

    public virtual DbSet<Quiztag> Quiztags { get; set; }

    public virtual DbSet<Tag> Tags { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<Useranswer> Useranswers { get; set; }

    public virtual DbSet<Userquizattempt> Userquizattempts { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseNpgsql("Host=localhost;Database=QuiZAppDb;Username=postgres;password=Tatva@123");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("uuid-ossp");

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("categories_pkey");

            entity.ToTable("categories");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Createdat)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("createdat");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Option>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("options_pkey");

            entity.ToTable("options");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Createdat)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("createdat");
            entity.Property(e => e.Iscorrect).HasColumnName("iscorrect");
            entity.Property(e => e.Questionid).HasColumnName("questionid");
            entity.Property(e => e.Text).HasColumnName("text");

            entity.HasOne(d => d.Question).WithMany(p => p.Options)
                .HasForeignKey(d => d.Questionid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_option_question");
        });

        modelBuilder.Entity<Question>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("questions_pkey");

            entity.ToTable("questions");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.Createdat)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("createdat");
            entity.Property(e => e.Difficulty)
                .HasMaxLength(20)
                .HasColumnName("difficulty");
            entity.Property(e => e.Isdeleted)
                .HasDefaultValueSql("false")
                .HasColumnName("isdeleted");
            entity.Property(e => e.Marks)
                .HasDefaultValueSql("1")
                .HasColumnName("marks");
            entity.Property(e => e.Quizid).HasColumnName("quizid");
            entity.Property(e => e.Text).HasColumnName("text");

            entity.HasOne(d => d.Category).WithMany(p => p.Questions)
                .HasForeignKey(d => d.CategoryId)
                .HasConstraintName("fk_categories");

            entity.HasOne(d => d.Quiz).WithMany(p => p.Questions)
                .HasForeignKey(d => d.Quizid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_question_quiz");
        });

        modelBuilder.Entity<Quiz>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("quizzes_pkey");

            entity.ToTable("quizzes");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Categoryid).HasColumnName("categoryid");
            entity.Property(e => e.Createdat)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("createdat");
            entity.Property(e => e.Createdby).HasColumnName("createdby");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Durationminutes)
                .HasDefaultValueSql("30")
                .HasColumnName("durationminutes");
            entity.Property(e => e.Enddate).HasColumnName("enddate");
            entity.Property(e => e.Isdeleted)
                .HasDefaultValueSql("false")
                .HasColumnName("isdeleted");
            entity.Property(e => e.Ispublic)
                .HasDefaultValueSql("true")
                .HasColumnName("ispublic");
            entity.Property(e => e.Modifiedat)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("modifiedat");
            entity.Property(e => e.Startdate).HasColumnName("startdate");
            entity.Property(e => e.Title)
                .HasMaxLength(150)
                .HasColumnName("title");
            entity.Property(e => e.Totalmarks).HasColumnName("totalmarks");

            entity.HasOne(d => d.Category).WithMany(p => p.Quizzes)
                .HasForeignKey(d => d.Categoryid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_quiz_category");

            entity.HasOne(d => d.CreatedbyNavigation).WithMany(p => p.Quizzes)
                .HasForeignKey(d => d.Createdby)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_quiz_user");
        });

        modelBuilder.Entity<Quiztag>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("quiztags_pkey");

            entity.ToTable("quiztags");

            entity.HasIndex(e => new { e.Quizid, e.Tagid }, "quiztags_quizid_tagid_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Quizid).HasColumnName("quizid");
            entity.Property(e => e.Tagid).HasColumnName("tagid");

            entity.HasOne(d => d.Quiz).WithMany(p => p.Quiztags)
                .HasForeignKey(d => d.Quizid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_quiztag_quiz");

            entity.HasOne(d => d.Tag).WithMany(p => p.Quiztags)
                .HasForeignKey(d => d.Tagid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_quiztag_tag");
        });

        modelBuilder.Entity<Tag>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("tags_pkey");

            entity.ToTable("tags");

            entity.HasIndex(e => e.Name, "tags_name_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("users_pkey");

            entity.ToTable("users");

            entity.HasIndex(e => e.Email, "users_email_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Createdat)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("createdat");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .HasColumnName("email");
            entity.Property(e => e.Fullname)
                .HasMaxLength(100)
                .HasColumnName("fullname");
            entity.Property(e => e.Isactive)
                .HasDefaultValueSql("true")
                .HasColumnName("isactive");
            entity.Property(e => e.Passwordhash)
                .HasMaxLength(255)
                .HasColumnName("passwordhash");
            entity.Property(e => e.Role)
                .HasMaxLength(50)
                .HasDefaultValueSql("'User'::character varying")
                .HasColumnName("role");
        });

        modelBuilder.Entity<Useranswer>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("useranswers_pkey");

            entity.ToTable("useranswers");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Attemptid).HasColumnName("attemptid");
            entity.Property(e => e.Iscorrect).HasColumnName("iscorrect");
            entity.Property(e => e.Optionid).HasColumnName("optionid");
            entity.Property(e => e.Questionid).HasColumnName("questionid");

            entity.HasOne(d => d.Attempt).WithMany(p => p.Useranswers)
                .HasForeignKey(d => d.Attemptid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_answer_attempt");

            entity.HasOne(d => d.Option).WithMany(p => p.Useranswers)
                .HasForeignKey(d => d.Optionid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_answer_option");

            entity.HasOne(d => d.Question).WithMany(p => p.Useranswers)
                .HasForeignKey(d => d.Questionid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_answer_question");
        });

        modelBuilder.Entity<Userquizattempt>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("userquizattempts_pkey");

            entity.ToTable("userquizattempts");

            entity.HasIndex(e => new { e.Userid, e.Quizid }, "userquizattempts_userid_quizid_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Categoryid).HasColumnName("categoryid");
            entity.Property(e => e.Endedat)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("endedat");
            entity.Property(e => e.Issubmitted)
                .HasDefaultValueSql("false")
                .HasColumnName("issubmitted");
            entity.Property(e => e.Quizid).HasColumnName("quizid");
            entity.Property(e => e.Score)
                .HasDefaultValueSql("0")
                .HasColumnName("score");
            entity.Property(e => e.Startedat)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("startedat");
            entity.Property(e => e.Timespent).HasColumnName("timespent");
            entity.Property(e => e.Userid).HasColumnName("userid");

            entity.HasOne(d => d.Category).WithMany(p => p.Userquizattempts)
                .HasForeignKey(d => d.Categoryid)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_category");

            entity.HasOne(d => d.Quiz).WithMany(p => p.Userquizattempts)
                .HasForeignKey(d => d.Quizid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_attempt_quiz");

            entity.HasOne(d => d.User).WithMany(p => p.Userquizattempts)
                .HasForeignKey(d => d.Userid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_attempt_user");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

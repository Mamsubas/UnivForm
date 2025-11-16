using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using UnivForm.Data; // AppUser ve AppRole için bu using gerekli olabilir
using UnivForm.Models;

namespace UnivForm.Data
{
    public class AppDbContext : IdentityDbContext<AppUser, AppRole, int>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Application> Applications { get; set; } = null!;
        public DbSet<ApplicationFileModel> ApplicationFileModels { get; set; } = null!;
        public DbSet<Category> Categories { get; set; }
        public DbSet<ForumThread> ForumThreads { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<PostLike> PostLikes { get; set; }
        public DbSet<Student> Students { get; set; } = null!;
        public DbSet<HighSchoolStudent> HighSchoolStudents { get; set; } = null!;
        public DbSet<UniversityStudent> UniversityStudents { get; set; } = null!;


        // -----------------------------------------------------------------
        // HATA ÇÖZÜMÜ İÇİN BU METODU EKLE
        // -----------------------------------------------------------------
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder); // Identity için bu satır ÇOK ÖNEMLİ, kalmalı

            // --- Hata Çözümü İçin Bu Kuralları Ekliyoruz ---

            // 1. Thread -> Post İlişkisi
            // Bir Konu (ForumThread) silinirse, içindeki tüm Yorumlar (Post) silinsin.
            builder.Entity<Post>()
                .HasOne(p => p.ForumThread)
                .WithMany(t => t.Posts) // ForumThread modelindeki 'Posts' koleksiyonu
                .HasForeignKey(p => p.ForumThreadId)
                .OnDelete(DeleteBehavior.Cascade); // Basamaklı silme AKTİF

            // 2. User -> Post İlişkisi (Hatanın Kaynağı)
            // Bir Kullanıcı (AppUser) silinirse, Yorumlarını (Post) doğrudan silmeye ÇALIŞMA.
            builder.Entity<Post>()
                .HasOne(p => p.Author)
                .WithMany() // AppUser'da 'Posts' koleksiyonu tanımlamadığımız için 'WithMany()' boş
                .HasForeignKey(p => p.AuthorId)
                .OnDelete(DeleteBehavior.Restrict); // Basamaklı silme YASAK (veya NoAction)

            // 3. User -> Thread İlişkisi
            // Bir Kullanıcı (AppUser) silinirse, tüm Konuları (ForumThread) silinsin.
            builder.Entity<ForumThread>()
                .HasOne(t => t.Author)
                .WithMany() // AppUser'da 'Threads' koleksiyonu tanımlamadığımız için 'WithMany()' boş
                .HasForeignKey(t => t.AuthorId)
                .OnDelete(DeleteBehavior.Cascade); // Basamaklı silme AKTİF

            builder.Entity<PostLike>()
            .HasOne(pl => pl.User)
            .WithMany() // AppUser'da 'Likes' koleksiyonu tanımlamadık
            .HasForeignKey(pl => pl.UserId)
            .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Post>()
            .HasOne(p => p.ParentPost)
            .WithMany(p => p.Replies)
            .HasForeignKey(p => p.ParentPostId)
            .OnDelete(DeleteBehavior.NoAction);

            // --- YENİ: AppUser - Student one-to-one relationship ---
            builder.Entity<Student>()
                .HasOne(s => s.AppUser)
                .WithOne(u => u.Student)
                .HasForeignKey<Student>(s => s.AppUserId)
                .OnDelete(DeleteBehavior.Cascade);

            // ExamScore decimal precision tanımlaması
            builder.Entity<Student>()
                .Property(s => s.ExamScore)
                .HasPrecision(5, 2); // max 999.99

            // --- YENİ: AppUser - HighSchoolStudent one-to-one relationship ---
            builder.Entity<HighSchoolStudent>()
                .HasOne(hs => hs.AppUser)
                .WithOne(u => u.HighSchoolStudent)
                .HasForeignKey<HighSchoolStudent>(hs => hs.AppUserId)
                .OnDelete(DeleteBehavior.Cascade);

            // HighSchoolStudent ExamScore decimal precision
            builder.Entity<HighSchoolStudent>()
                .Property(hs => hs.ExamScore)
                .HasPrecision(5, 2);

            // --- YENİ: AppUser - UniversityStudent one-to-one relationship ---
            builder.Entity<UniversityStudent>()
                .HasOne(us => us.AppUser)
                .WithOne(u => u.UniversityStudent)
                .HasForeignKey<UniversityStudent>(us => us.AppUserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
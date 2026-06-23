using Microsoft.EntityFrameworkCore;
using RentasCortas.Models;

namespace RentasCortas.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Inmueble> Inmuebles => Set<Inmueble>();
    public DbSet<PropertyImage> PropertyImages => Set<PropertyImage>();
    public DbSet<Reserva> Reservas => Set<Reserva>();
    public DbSet<ReservationStatusHistory> ReservationStatusHistory => Set<ReservationStatusHistory>();
    public DbSet<Favorito> Favoritos => Set<Favorito>();
    public DbSet<KycValidation> KycValidations => Set<KycValidation>();
    public DbSet<Notificacion> Notificaciones => Set<Notificacion>();
    public DbSet<Review> Reviews => Set<Review>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.ToTable("usuarios");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.Email).HasColumnName("email").HasMaxLength(255);
            entity.Property(e => e.PasswordHash).HasColumnName("password_hash").HasMaxLength(255);
            entity.Property(e => e.FullName).HasColumnName("full_name").HasMaxLength(255);
            entity.Property(e => e.Phone).HasColumnName("phone").HasMaxLength(20);
            entity.Property(e => e.Role).HasColumnName("role").HasMaxLength(10);
            entity.Property(e => e.KycStatus).HasColumnName("kyc_status").HasMaxLength(20).HasDefaultValue("pending");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
            entity.Property(e => e.DeletedAt).HasColumnName("deleted_at");
            entity.HasQueryFilter(e => e.DeletedAt == null);
        });

        modelBuilder.Entity<Inmueble>(entity =>
        {
            entity.ToTable("inmuebles");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.OwnerId).HasColumnName("owner_id");
            entity.Property(e => e.Title).HasColumnName("title").HasMaxLength(255);
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Location).HasColumnName("location").HasMaxLength(255);
            entity.Property(e => e.PricePerNight).HasColumnName("price_per_night").HasColumnType("decimal(10,2)");
            entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(20).HasDefaultValue("active");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
            entity.Property(e => e.DeletedAt).HasColumnName("deleted_at");
            entity.HasOne(e => e.Owner).WithMany().HasForeignKey(e => e.OwnerId);
            entity.HasMany(e => e.Images).WithOne(e => e.Inmueble).HasForeignKey(e => e.InmuebleId);
            entity.HasQueryFilter(e => e.DeletedAt == null);
        });

        modelBuilder.Entity<PropertyImage>(entity =>
        {
            entity.ToTable("property_images");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.InmuebleId).HasColumnName("inmueble_id");
            entity.Property(e => e.ImageUrl).HasColumnName("image_url").HasMaxLength(500);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
        });

        modelBuilder.Entity<Reserva>(entity =>
        {
            entity.ToTable("reservas");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.InmuebleId).HasColumnName("inmueble_id");
            entity.Property(e => e.GuestId).HasColumnName("guest_id");
            entity.Property(e => e.CheckIn).HasColumnName("check_in");
            entity.Property(e => e.CheckOut).HasColumnName("check_out");
            entity.Property(e => e.CheckInTime).HasColumnName("check_in_time").HasDefaultValue(new TimeOnly(14, 0));
            entity.Property(e => e.CheckOutTime).HasColumnName("check_out_time").HasDefaultValue(new TimeOnly(12, 0));
            entity.Property(e => e.TotalPrice).HasColumnName("total_price").HasColumnType("decimal(10,2)");
            entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(20).HasDefaultValue("pending");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
            entity.HasOne(e => e.Inmueble).WithMany().HasForeignKey(e => e.InmuebleId);
            entity.HasOne(e => e.Guest).WithMany().HasForeignKey(e => e.GuestId);
        });

        modelBuilder.Entity<ReservationStatusHistory>(entity =>
        {
            entity.ToTable("reservation_status_history");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.ReservaId).HasColumnName("reserva_id");
            entity.Property(e => e.PreviousStatus).HasColumnName("previous_status").HasMaxLength(20);
            entity.Property(e => e.NewStatus).HasColumnName("new_status").HasMaxLength(20);
            entity.Property(e => e.ChangedAt).HasColumnName("changed_at").HasDefaultValueSql("NOW()");
            entity.HasOne(e => e.Reserva).WithMany().HasForeignKey(e => e.ReservaId);
        });

        modelBuilder.Entity<Favorito>(entity =>
        {
            entity.ToTable("favoritos");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.GuestId).HasColumnName("guest_id");
            entity.Property(e => e.InmuebleId).HasColumnName("inmueble_id");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
            entity.HasIndex(e => new { e.GuestId, e.InmuebleId }).IsUnique();
            entity.HasOne(e => e.Guest).WithMany().HasForeignKey(e => e.GuestId);
            entity.HasOne(e => e.Inmueble).WithMany().HasForeignKey(e => e.InmuebleId);
        });

        modelBuilder.Entity<KycValidation>(entity =>
        {
            entity.ToTable("kyc_validations");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.ExtractedName).HasColumnName("extracted_name").HasMaxLength(255);
            entity.Property(e => e.ExtractedLastname).HasColumnName("extracted_lastname").HasMaxLength(255);
            entity.Property(e => e.ExtractedDocumentNumber).HasColumnName("extracted_document_number").HasMaxLength(50);
            entity.Property(e => e.ExtractedBirthdate).HasColumnName("extracted_birthdate");
            entity.Property(e => e.Verdict).HasColumnName("verdict").HasMaxLength(20);
            entity.Property(e => e.ProcessedAt).HasColumnName("processed_at");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
            entity.HasOne(e => e.User).WithMany().HasForeignKey(e => e.UserId);
        });

        modelBuilder.Entity<Notificacion>(entity =>
        {
            entity.ToTable("notificaciones");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.Type).HasColumnName("type").HasMaxLength(50);
            entity.Property(e => e.Message).HasColumnName("message");
            entity.Property(e => e.Channel).HasColumnName("channel").HasMaxLength(20);
            entity.Property(e => e.SentAt).HasColumnName("sent_at").HasDefaultValueSql("NOW()");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
            entity.HasOne(e => e.User).WithMany().HasForeignKey(e => e.UserId);
        });

        modelBuilder.Entity<Review>(entity =>
        {
            entity.ToTable("reviews");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.InmuebleId).HasColumnName("inmueble_id");
            entity.Property(e => e.GuestId).HasColumnName("guest_id");
            entity.Property(e => e.Rating).HasColumnName("rating");
            entity.Property(e => e.Comment).HasColumnName("comment");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
            entity.HasOne(e => e.Inmueble).WithMany().HasForeignKey(e => e.InmuebleId);
            entity.HasOne(e => e.Guest).WithMany().HasForeignKey(e => e.GuestId);
        });
    }
}
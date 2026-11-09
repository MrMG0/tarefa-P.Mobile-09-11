using ClubNet.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace ClubNet.API.Data;

public class ClubNetDbContext : DbContext
{
    public ClubNetDbContext(DbContextOptions<ClubNetDbContext> options) : base(options)
    {
    }

    public DbSet<Clube> Clubes => Set<Clube>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Associado> Associados => Set<Associado>();
    public DbSet<Dependente> Dependentes => Set<Dependente>();
    public DbSet<InteresseAssociado> InteressesAssociado => Set<InteresseAssociado>();
    public DbSet<Evento> Eventos => Set<Evento>();
    public DbSet<Mensalidade> Mensalidades => Set<Mensalidade>();
    public DbSet<Espaco> Espacos => Set<Espaco>();
    public DbSet<Reserva> Reservas => Set<Reserva>();
    public DbSet<Solicitacao> Solicitacoes => Set<Solicitacao>();
    public DbSet<Reclamacao> Reclamacoes => Set<Reclamacao>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Usuário
        modelBuilder.Entity<Usuario>()
            .HasIndex(u => u.Email)
            .IsUnique();

        // Associado
        modelBuilder.Entity<Associado>()
            .HasOne(a => a.Clube)
            .WithMany()
            .HasForeignKey(a => a.IdClube)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Associado>()
            .HasOne(a => a.Usuario)
            .WithMany()
            .HasForeignKey(a => a.IdUsuario)
            .OnDelete(DeleteBehavior.SetNull);

        // Dependente
        modelBuilder.Entity<Dependente>()
            .HasOne(d => d.Associado)
            .WithMany(a => a.Dependentes)
            .HasForeignKey(d => d.IdAssociado)
            .OnDelete(DeleteBehavior.Cascade);

        // Interesses
        modelBuilder.Entity<InteresseAssociado>()
            .HasOne(i => i.Associado)
            .WithMany(a => a.Interesses)
            .HasForeignKey(i => i.IdAssociado)
            .OnDelete(DeleteBehavior.Cascade);

        // Eventos
        modelBuilder.Entity<Evento>()
            .HasOne(e => e.Clube)
            .WithMany()
            .HasForeignKey(e => e.IdClube)
            .OnDelete(DeleteBehavior.Cascade);

        // Mensalidades
        modelBuilder.Entity<Mensalidade>()
            .HasOne(m => m.Clube)
            .WithMany()
            .HasForeignKey(m => m.IdClube)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Mensalidade>()
            .HasOne(m => m.Associado)
            .WithMany()
            .HasForeignKey(m => m.IdAssociado)
            .OnDelete(DeleteBehavior.Cascade);

        // Espaços
        modelBuilder.Entity<Espaco>()
            .HasOne(e => e.Clube)
            .WithMany()
            .HasForeignKey(e => e.IdClube)
            .OnDelete(DeleteBehavior.Cascade);

        // Reservas
        modelBuilder.Entity<Reserva>()
            .HasOne(r => r.Clube)
            .WithMany()
            .HasForeignKey(r => r.IdClube)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Reserva>()
            .HasOne(r => r.Espaco)
            .WithMany()
            .HasForeignKey(r => r.IdEspaco)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Reserva>()
            .HasOne(r => r.Associado)
            .WithMany()
            .HasForeignKey(r => r.IdAssociado)
            .OnDelete(DeleteBehavior.Cascade);

        // Solicitações
        modelBuilder.Entity<Solicitacao>()
            .HasOne(s => s.Clube)
            .WithMany()
            .HasForeignKey(s => s.IdClube)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Solicitacao>()
            .HasOne(s => s.Associado)
            .WithMany()
            .HasForeignKey(s => s.IdAssociado)
            .OnDelete(DeleteBehavior.SetNull);

        // Reclamações
        modelBuilder.Entity<Reclamacao>()
            .HasOne(rc => rc.Clube)
            .WithMany()
            .HasForeignKey(rc => rc.IdClube)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Reclamacao>()
            .HasOne(rc => rc.Associado)
            .WithMany()
            .HasForeignKey(rc => rc.IdAssociado)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

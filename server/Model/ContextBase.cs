using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace app.Model
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
        }

        public DbSet<Jwt> Jwts { get; set; }
        public DbSet<Petition> Petitions { get; set; }
        public DbSet<Point> Points { get; set; }
        public DbSet<PointChief> PointChiefs { get; set; }
        public DbSet<Unit> Units { get; set; }
        public DbSet<PetitionType> PetitionTypes { get; set; }
    }

    public class ApplicationUser : IdentityUser
    {

    }

    public class Jwt 
    {
        public Guid Id { get; set; } = new Guid();
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
        public string RefreshToken { get; set; }
        public DateTime ExpAtRef { get; set; }
        public string AccessToken { get; set; }
    }

    public class Petition
    {
        public Guid? Id { get; set; }
        public string Title { get; set; }
        public string Reason { get; set; }
        public string Vehicle { get; set; }
        public string Type { get; set; }
        public bool Checked { get; set; }
        public DateTime Date { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
        public List<Point> Points { get; set; } = new List<Point>();
        public List<Unit> Units { get; set; } = new List<Unit>();
        
    }

    public class Unit 
    {
        public Guid? Id { get; set; }
        public string FullName { get; set; }
        public string Position { get; set; }
        public string DocumentIdentity { get; set; }
        public DateTime BirthDay { get; set; }
        public string BirthPlace { get; set; }
        public string HomeAdress { get; set; }
        public List<Petition> Petitions { get; set; }
    }

    public class Point
    {
        public Guid? Id { get; set; }
        public string Title { get; set; }
        public List<Petition> Petitions { get; set; }
        public Guid PointChiefId { get; set; }
        public PointChief PointChief { get; set; }
    }

    public class PointChief
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string ChiefFullName { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
    }

    public class PetitionType 
    {
        public Guid? Id { get; set; }
        public string Title { get; set; }
    }
}

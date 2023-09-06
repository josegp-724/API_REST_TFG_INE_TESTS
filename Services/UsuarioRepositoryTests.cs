using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TFGv3Net7.Models;
using TFGv3Net7.Data;
using TFGv3Net7.Services;
using TFGv3Net7.Registros;
using Moq;
using Microsoft.Extensions.Configuration;

namespace TFGv3Net7.Tests.Services
{
    public class UsuarioRepositoryTests : IDisposable
    {
        private readonly TfgPrimeroContext _context;
        private readonly UsuarioRepository _repo;
        private readonly Mock<IConfiguration> _mockConfig;  // <-- Añadir esta línea

        public UsuarioRepositoryTests()
        {
            // Crear un nuevo proveedor de servicios y registrar el proveedor InMemory
            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .BuildServiceProvider();

            // Configurar las opciones para usar InMemory con el nuevo proveedor de servicios
            var options = new DbContextOptionsBuilder<TfgPrimeroContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .UseInternalServiceProvider(serviceProvider)
            .EnableSensitiveDataLogging()
            .Options;

            _context = new TfgPrimeroContext(options);

            // Crear mock de IConfiguration
            _mockConfig = new Mock<IConfiguration>();

            // Si necesitas configurar valores específicos para tu IConfiguration en el futuro, puedes hacerlo aquí.
            // Ejemplo: _mockConfig.Setup(c => c["SomeKey"]).Returns("SomeValue");

            _repo = new UsuarioRepository(_context, _mockConfig.Object);  // <-- Cambia esta línea
        }
        // metodo idisposable
        public void Dispose()
        {
            _context.Dispose();
        }

        [Fact]
        public async Task ObtenerTodosLosUsuariosAsync_ShouldReturnAllUsers()
        {
            // Arrange
            _context.Usuarios.Add(new Usuario { Email = "test1@test.com", BoolAdmin = 1, HashPassword = "sampleHashedPassword", MaxConsultas = 500 });
            _context.Usuarios.Add(new Usuario { Email = "test2@test.com", BoolAdmin = 0, HashPassword = "sampleHashedPassword", MaxConsultas = 250 });
            await _context.SaveChangesAsync();

            // Act
            var result = await _repo.ObtenerTodosLosUsuariosAsync();

            // Assert
            Assert.Equal(2, result.Count());
            Assert.Contains(result, u => u.Email == "test1@test.com");
            Assert.Contains(result, u => u.Email == "test2@test.com");
        }

        [Fact]
        public async Task ObtenerUsuarioPorIdAsync_ShouldReturnCorrectUser()
        {
            // Arrange
            var user = new Usuario { Email = "test1@test.com", HashPassword = "sampleHashedPassword", MaxConsultas = 500};
            _context.Usuarios.Add(user);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repo.ObtenerUsuarioPorIdAsync(user.IdUsuario);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("test1@test.com", result.Email);
        }

        [Fact]
        public async Task ObtenerNumeroMaximoConsultasAsync_ShouldReturnMaxConsultasValue()
        {
            // Arrange
            var user = new Usuario { Email = "test@test.com", HashPassword = "sampleHashedPassword", MaxConsultas = 500 };
            _context.Usuarios.Add(user);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repo.ObtenerNumeroMaximoConsultasAsync(user.IdUsuario);

            // Assert
            Assert.Equal(500, result);
        }

        [Fact]
        public async Task ObtenerIdUsuarioPorEmailAsync_ShouldReturnUserId()
        {
            // Arrange
            var user = new Usuario { Email = "test@test.com" , HashPassword = "sampleHashedPassword", MaxConsultas = 500};
            _context.Usuarios.Add(user);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repo.ObtenerIdUsuarioPorEmailAsync("test@test.com");

            // Assert
            Assert.Equal(user.IdUsuario, result);
        }

        [Fact]
        public async Task ObtenerUsuarioPorEmailAsync_ShouldReturnCorrectUser()
        {
            // Arrange
            var user = new Usuario { Email = "test1@test.com" , HashPassword = "sampleHashedPassword", MaxConsultas = 500};
            _context.Usuarios.Add(user);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repo.ObtenerUsuarioPorEmailAsync("test1@test.com");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("test1@test.com", result.Email);
        }


    }   
}

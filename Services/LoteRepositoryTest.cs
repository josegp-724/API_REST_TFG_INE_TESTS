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

namespace TFGv3Net7.Tests.Services
{
    public class LoteRepositoryTests : IDisposable
    {
        private readonly TfgPrimeroContext _context;
        private readonly LoteRepository _repo;

        public LoteRepositoryTests()
        {
            // Crear un nuevo proveedor de servicios y registrar el proveedor InMemory
            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .BuildServiceProvider();

            // Configurar las opciones para usar InMemory con el nuevo proveedor de servicios
            var options = new DbContextOptionsBuilder<TfgPrimeroContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .UseInternalServiceProvider(serviceProvider)
                .Options;

            _context = new TfgPrimeroContext(options);
            _repo = new LoteRepository(_context);
        }

        // metodo idisposable
        public void Dispose()
        {
            _context.Dispose();
        }



        [Fact]
        public async Task ObtenerTodosLosLotesAsync_ShouldReturnAllLotes()
        {
            // Arrange
            var lotes = new List<Lote>
            {
                new Lote { LoteId = 1 },
                new Lote { LoteId = 2 },
                new Lote { LoteId = 3 }
            };
            _context.Lotes.AddRange(lotes);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repo.ObtenerTodosLosLotesAsync();

            // Assert
            Assert.Equal(3, result.Count());
        }

        [Fact]
        public async Task ObtenerLotePorIdAsync_ShouldReturnCorrectLote()
        {
            // Arrange
            var lote = new Lote { LoteId = 1 };
            _context.Lotes.Add(lote);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repo.ObtenerLotePorIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.LoteId);
        }

        [Fact]
        public async Task ObtenerLotePorIdAsync_ShouldReturnNullForNonExistingLote()
        {
            // Act
            var result = await _repo.ObtenerLotePorIdAsync(999);

            // Assert
            Assert.Null(result);
        }

    // ... [Imports y configuración previa]

        [Fact]
        public async Task ObtenerTodosLosLotesAsync_ShouldReturnEmptyList_WhenNoLotesExist()
        {
            // Act
            var result = await _repo.ObtenerTodosLosLotesAsync();

            // Assert
            Assert.Empty(result);
        }


        [Theory]
        [InlineData(-1)]
        [InlineData(0)]
        public async Task ObtenerLotePorIdAsync_ShouldReturnNullForInvalidId(int invalidId)
        {
            // Act
            var result = await _repo.ObtenerLotePorIdAsync(invalidId);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task ContarConsultasDelDiaPorUsuarioAsync_ShouldReturnZero_WhenNoLotesExistForToday()
        {
            // Arrange
            var lote1 = new Lote 
            { 
                LoteId = 1, 
                IdUsuario = 1, 
                FechaConsulta = DateTime.Today.AddDays(-1) 
            };
            _context.Lotes.Add(lote1);
            await _context.SaveChangesAsync();

            // Act
            var count = await _repo.ContarConsultasDelDiaPorUsuarioAsync(1);

            // Assert
            Assert.Equal(0, count);
        }


    

    }
}

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
    public class ProvinciaRepositoryTests : IDisposable
    {
        private readonly TfgPrimeroContext _context;
        private readonly ProvinciaRepository _repo;

        public ProvinciaRepositoryTests()
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
            _repo = new ProvinciaRepository(_context);
        }

        // metodo idisposable
        public void Dispose()
        {
            _context.Dispose();
        }

        [Fact]
        public async Task ObtenerTodasLasProvinciasAsync_ReturnsAllProvincias()
        {
            // Arrange
            var provincias = new List<Provincium>
            {
                new Provincium { IdProvincia = "1", NombreProvincia = "Provincia1" },
                new Provincium { IdProvincia = "2", NombreProvincia = "Provincia2" }
            };
            _context.Provincia.AddRange(provincias);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repo.ObtenerTodasLasProvinciasAsync();

            // Assert
            Assert.Equal(2, result.Count());
            Assert.Contains("Provincia1", result);
            Assert.Contains("Provincia2", result);
        }

        [Fact]
        public async Task ObtenerProvinciaPorIdAsync_ReturnsCorrectProvincia()
        {
            // Arrange
            var provincia = new Provincium { IdProvincia = "1", NombreProvincia = "Provincia1" };
            await _context.Provincia.AddAsync(provincia);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repo.ObtenerProvinciaPorIdAsync("1");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Provincia1", result.NombreProvincia);
        }

        [Fact]
        public async Task ObtenerProvinciasNoCoincidentesPorLoteAsync_ReturnsNonMatchingProvincias()
        {
            // Arrange
            var provincias = new List<Provincium>
            {
                new Provincium { IdProvincia = "1", NombreProvincia = "Provincia1" }
            };
            _context.Provincia.AddRange(provincias);
            await _context.SaveChangesAsync();

            var registrosRenta = new List<RegistroRenta>
            {
                new RegistroRenta { ID_Provincia = "2", Nombre_Provincia = "Provincia2" },
                new RegistroRenta { ID_Provincia = "3", Nombre_Provincia = "Provincia3" }
            };

            // Act
            var result = await _repo.ObtenerProvinciasNoCoincidentesPorLoteAsync(registrosRenta);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Contains(result, p => p.IdProvincia == "2");
            Assert.Contains(result, p => p.IdProvincia == "3");
        }

        [Fact]
        public async Task ObtenerProvinciaPorIdAsync_WithNonExistentId_ReturnsNull()
        {
            // Act
            var result = await _repo.ObtenerProvinciaPorIdAsync("999");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task ObtenerTodasLasProvinciasAsync_WithEmptyDB_ReturnsEmptyList()
        {
            // Act
            var result = await _repo.ObtenerTodasLasProvinciasAsync();

            // Assert
            Assert.Empty(result);
        }


        [Fact]
        public async Task ObtenerProvinciasNoCoincidentesPorLoteAsync_AllMatch_ReturnsEmptyList()
        {
            // Arrange
            var provincias = new List<Provincium>
            {
                new Provincium { IdProvincia = "1", NombreProvincia = "Provincia1" },
                new Provincium { IdProvincia = "2", NombreProvincia = "Provincia2" }
            };
            _context.Provincia.AddRange(provincias);
            await _context.SaveChangesAsync();

            var registrosRenta = new List<RegistroRenta>
            {
                new RegistroRenta { ID_Provincia = "1", Nombre_Provincia = "Provincia1" },
                new RegistroRenta { ID_Provincia = "2", Nombre_Provincia = "Provincia2" }
            };

            // Act
            var result = await _repo.ObtenerProvinciasNoCoincidentesPorLoteAsync(registrosRenta);

            // Assert
            Assert.Empty(result);
        }
        [Fact]
        public async Task ObtenerProvinciasNoCoincidentesPorLoteAsync_NoRegistros_ReturnsEmptyList()
        {
            // Arrange
            var registrosRenta = new List<RegistroRenta>();

            // Act
            var result = await _repo.ObtenerProvinciasNoCoincidentesPorLoteAsync(registrosRenta);

            // Assert
            Assert.Empty(result);
        }

    }
}

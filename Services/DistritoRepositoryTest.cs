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
    public class DistritoRepositoryTests : IDisposable
    {
        private readonly TfgPrimeroContext _context;
        private readonly DistritoRepository _repo;

        public DistritoRepositoryTests()
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
            _repo = new DistritoRepository(_context);
        }

        // metodo idisposable
        public void Dispose()
        {
            _context.Dispose();
        }

        [Fact]
        public async Task ObtenerTodosLosDistritosAsync_ShouldReturnAllDistritos()
        {
            // Arrange
            var distritos = new List<Distrito>
            {
                new Distrito { IdDistrito = "1", IdMunicipio = "1", IdProvincia = "1" },
                new Distrito { IdDistrito = "2", IdMunicipio = "2", IdProvincia = "2" }
            };

            _context.Distritos.AddRange(distritos);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repo.ObtenerTodosLosDistritosAsync();

            // Assert
            Assert.Equal(distritos.Count, result.Count());
        }

        [Fact]
        public async Task ObtenerDistritoPorIdAsync_ShouldReturnCorrectDistrito()
        {
            // Arrange
            var distrito = new Distrito { IdDistrito = "3", IdMunicipio = "3", IdProvincia = "3" };
            _context.Distritos.Add(distrito);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repo.ObtenerDistritoPorIdAsync(distrito.IdDistrito, distrito.IdMunicipio, distrito.IdProvincia);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(distrito.IdDistrito, result.IdDistrito);
            Assert.Equal(distrito.IdMunicipio, result.IdMunicipio);
            Assert.Equal(distrito.IdProvincia, result.IdProvincia);
        }

        [Fact]
        public async Task ObtenerDistritosNoCoincidentesPorLoteAsync_ShouldReturnNonMatchingDistritos()
        {
            // Arrange
            var registrosRenta = new List<RegistroRenta>
            {
                new RegistroRenta { ID_Distrito = "4", ID_Municipio = "4", ID_Provincia = "4" },
                new RegistroRenta { ID_Distrito = "5", ID_Municipio = "5", ID_Provincia = "5" }
            };

            var distritos = new List<Distrito>
            {
                new Distrito { IdDistrito = "4", IdMunicipio = "4", IdProvincia = "4" }
            };

            _context.Distritos.AddRange(distritos);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repo.ObtenerDistritosNoCoincidentesPorLoteAsync(registrosRenta);

            // Assert
            Assert.Single(result); // Debería haber sólo 1 distrito no coincidente.
            Assert.Equal("5", result.First().IdDistrito);
            Assert.Equal("5", result.First().IdMunicipio);
            Assert.Equal("5", result.First().IdProvincia);
        }

        //... Resto del código de pruebas ...

        [Fact]
        public async Task ObtenerTodosLosDistritosAsync_ShouldReturnEmptyList_WhenNoDistritosExist()
        {
            // Act
            var result = await _repo.ObtenerTodosLosDistritosAsync();

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task ObtenerDistritoPorIdAsync_ShouldReturnNull_WhenDistritoDoesNotExist()
        {
            // Act
            var result = await _repo.ObtenerDistritoPorIdAsync("invalid", "invalid", "invalid");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task ObtenerDistritosNoCoincidentesPorLoteAsync_ShouldReturnEmptyList_WhenAllRegistrosMatch()
        {
            // Arrange
            var registrosRenta = new List<RegistroRenta>
            {
                new RegistroRenta { ID_Distrito = "1", ID_Municipio = "1", ID_Provincia = "1" },
                new RegistroRenta { ID_Distrito = "2", ID_Municipio = "2", ID_Provincia = "2" }
            };

            var distritos = new List<Distrito>
            {
                new Distrito { IdDistrito = "1", IdMunicipio = "1", IdProvincia = "1" },
                new Distrito { IdDistrito = "2", IdMunicipio = "2", IdProvincia = "2" }
            };

            _context.Distritos.AddRange(distritos);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repo.ObtenerDistritosNoCoincidentesPorLoteAsync(registrosRenta);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task ObtenerDistritosNoCoincidentesPorLoteAsync_ShouldReturnAllRegistros_WhenNoDistritosExist()
        {
            // Arrange
            var registrosRenta = new List<RegistroRenta>
            {
                new RegistroRenta { ID_Distrito = "1", ID_Municipio = "1", ID_Provincia = "1" },
                new RegistroRenta { ID_Distrito = "2", ID_Municipio = "2", ID_Provincia = "2" }
            };

            // Act
            var result = await _repo.ObtenerDistritosNoCoincidentesPorLoteAsync(registrosRenta);

            // Assert
            Assert.Equal(registrosRenta.Count, result.Count);
        }



    }
}

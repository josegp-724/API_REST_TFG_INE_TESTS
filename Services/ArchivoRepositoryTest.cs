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

namespace TFGv3Net7.Tests.Services
{
    public class ArchivoRepositoryTests : IDisposable
    {
        private readonly TestTfgPrimeroContext _context;
        private readonly ArchivoRepository _repo;

        public ArchivoRepositoryTests()
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

            _context = new TestTfgPrimeroContext(options);
            _repo = new ArchivoRepository(_context);
        }

        public void Dispose()
        {
            _context.Dispose();
        }

        [Fact]
        public async Task AgregarMultiplesArchivosAsync_ThrowsExceptionForInvalidTipo()
        {
            _context.Archivos.RemoveRange(_context.Archivos);  // Limpia la base de datos antes de la prueba
            await _context.SaveChangesAsync();

            Assert.Empty(_context.Archivos);  // Asegúrate de que la base de datos esté vacía al principio


            // Arrange
            var enlaces = new List<string> { "enlace1", "enlace2" };
            var invalidTipo = "INVALIDO";
            var years = new List<string> { "2020", "2021" };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _repo.AgregarMultiplesArchivosAsync(enlaces, invalidTipo, years));
        }

        [Fact]
        public async Task AgregarMultiplesArchivosAsync_AddsMultipleArchivos()
        {
            _context.Archivos.RemoveRange(_context.Archivos);  // Limpia la base de datos antes de la prueba
            await _context.SaveChangesAsync();

            Assert.Empty(_context.Archivos);  // Asegúrate de que la base de datos esté vacía al principio


            // Arrange
            var enlaces = new List<string> { "enlace1", "enlace2" };
            var tipo = "INE";
            var years = new List<string> { "2020", "2021" };

            // Act
            await _repo.AgregarMultiplesArchivosAsync(enlaces, tipo, years);

            // Assert
            Assert.Equal(4, _context.Archivos.Count());  // Verifica que se han agregado dos registros en la base de datos
        }

        [Fact]
        public async Task ObtenerEnlacesUnicosPorTipoYYearAsync_ReturnsUniqueEnlaces()
        {
            // Arrange
            _context.Archivos.AddRange(
                new Archivo { Enlace = "enlace1", Tipo = "INE", Year = "2020" },
                new Archivo { Enlace = "enlace2", Tipo = "INE", Year = "2020" },
                new Archivo { Enlace = "enlace3", Tipo = "INE", Year = "2021" }
            );
            await _context.SaveChangesAsync();

            // Act
            var enlaces = await _repo.ObtenerEnlacesUnicosPorTipoYYearAsync("INE", "2020");

            // Assert
            Assert.Equal(2, enlaces.Count);
            Assert.Contains("enlace1", enlaces);
            Assert.Contains("enlace2", enlaces);
        }

        [Fact]
        public async Task ObtenerEnlacesUnicosPorTipoYYearAsync_ThrowsForInvalidTipo()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _repo.ObtenerEnlacesUnicosPorTipoYYearAsync("INVALIDO", "2020"));
        }

        [Fact]
        public async Task ObtenerEnlacesUnicosPorTipoYYearAsync_ThrowsForInvalidYearFormat()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _repo.ObtenerEnlacesUnicosPorTipoYYearAsync("INE", "20AB"));
        }


        [Fact]
        public async Task EliminarArchivosPorTipoYYearAsync_DeletesCorrectRecords()
        {
            // Arrange
            _context.Archivos.AddRange(
                new Archivo { Enlace = "enlace1", Tipo = "INE", Year = "2020" },
                new Archivo { Enlace = "enlace2", Tipo = "INE", Year = "2021" },
                new Archivo { Enlace = "enlace3", Tipo = "CALLEJERO", Year = "2020" }
            );
            await _context.SaveChangesAsync();

            // Act
            await _repo.EliminarArchivosPorTipoYYearAsync("INE", "2020");

            // Assert
            var archivosRestantes = _context.Archivos.ToList();
            Assert.Equal(2, archivosRestantes.Count);
            Assert.DoesNotContain(archivosRestantes, a => a.Enlace == "enlace1");
        }

        [Fact]
        public async Task EliminarArchivosPorTipoYYearAsync_ThrowsForInvalidTipo()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _repo.EliminarArchivosPorTipoYYearAsync("INVALIDO", "2020"));
        }

        [Fact]
        public async Task EliminarArchivosPorTipoYYearAsync_ThrowsForInvalidYearFormat()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _repo.EliminarArchivosPorTipoYYearAsync("INE", "20AB"));
        }

        [Fact]
        public async Task EliminarArchivosPorTipoYYearAsync_DoesNotDeleteUnmatchingRecords()
        {
            // Arrange
            _context.Archivos.AddRange(
                new Archivo { Enlace = "enlace1", Tipo = "INE", Year = "2020" },
                new Archivo { Enlace = "enlace2", Tipo = "INE", Year = "2021" }
            );
            await _context.SaveChangesAsync();

            // Act
            await _repo.EliminarArchivosPorTipoYYearAsync("INE", "2019");  // Usando un año que no tiene registros

            // Assert
            var archivosRestantes = _context.Archivos.ToList();
            Assert.Equal(2, archivosRestantes.Count);  // Asegurando que ambos registros aún están presentes
        }


        [Fact]
        public async Task ObtenerTodosLosArchivosAsync_ReturnsAllArchivos()
        {
            // Arrange
            _context.Archivos.AddRange(
                new Archivo { Enlace = "enlace1", Tipo = "INE", Year = "2020" },
                new Archivo { Enlace = "enlace2", Tipo = "INE", Year = "2021" },
                new Archivo { Enlace = "enlace3", Tipo = "CALLEJERO", Year = "2020" }
            );
            await _context.SaveChangesAsync();

            // Act
            var result = await _repo.ObtenerTodosLosArchivosAsync();

            // Assert
            Assert.Equal(3, result.Count());
        }

        [Fact]
        public async Task ObtenerEnlacesUnicosPorTipoAsync_ReturnsUniqueEnlacesForTipo()
        {
            // Arrange
            _context.Archivos.AddRange(
                new Archivo { Enlace = "enlace1", Tipo = "INE", Year = "2020" },
                new Archivo { Enlace = "enlace1", Tipo = "INE", Year = "2021" },  // Mismo enlace, diferente año
                new Archivo { Enlace = "enlace2", Tipo = "INE", Year = "2021" },
                new Archivo { Enlace = "enlace3", Tipo = "CALLEJERO", Year = "2020" }
            );
            await _context.SaveChangesAsync();

            // Act
            var result = await _repo.ObtenerEnlacesUnicosPorTipoAsync("INE");

            // Assert
            Assert.Equal(2, result.Count());  // Solo dos enlaces únicos para el tipo "INE"
            Assert.Contains("enlace1", result);
            Assert.Contains("enlace2", result);
        }

        [Fact]
        public async Task ObtenerEnlacesUnicosPorTipoAsync_ThrowsForInvalidTipo()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _repo.ObtenerEnlacesUnicosPorTipoAsync("INVALIDO"));
        }

        [Fact]
        public async Task ObtenerEnlacesUnicosPorTipoAsync_ReturnsEmptyForNoMatchingTipo()
        {
            // Arrange: No agregamos ningún archivo para asegurarnos de que la base de datos está vacía

            // Act
            var result = await _repo.ObtenerEnlacesUnicosPorTipoAsync("INE");

            // Assert
            Assert.Empty(result);
        }



        [Fact]
        public async Task BorrarArchivosPorTipoAsync_RemovesFilesOfGivenTipo()
        {
            // Arrange
            _context.Archivos.AddRange(
                new Archivo { Enlace = "enlace1", Tipo = "INE", Year = "2020" },
                new Archivo { Enlace = "enlace2", Tipo = "INE", Year = "2021" },
                new Archivo { Enlace = "enlace3", Tipo = "CALLEJERO", Year = "2020" }
            );
            await _context.SaveChangesAsync();

            // Act
            await _repo.BorrarArchivosPorTipoAsync("INE");

            // Assert
            Assert.DoesNotContain(_context.Archivos, a => a.Tipo == "INE");
            Assert.Contains(_context.Archivos, a => a.Tipo == "CALLEJERO");
        }

        [Fact]
        public async Task BorrarArchivosPorTipoAsync_ThrowsForInvalidTipo()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _repo.BorrarArchivosPorTipoAsync("INVALIDO"));
        }

        [Fact]
        public async Task ObtenerEnlacesINEConRangoDeYearsAsync_ReturnsUniqueLinksAndYearRange()
        {
            // Arrange
            _context.Archivos.AddRange(
                new Archivo { Enlace = "enlace1", Tipo = "INE", Year = "2019" },
                new Archivo { Enlace = "enlace2", Tipo = "INE", Year = "2021" },
                new Archivo { Enlace = "enlace1", Tipo = "INE", Year = "2020" },
                new Archivo { Enlace = "enlace3", Tipo = "CALLEJERO", Year = "2020" }
            );
            await _context.SaveChangesAsync();

            // Act
            var (enlaces, añoMenor, añoMayor) = await _repo.ObtenerEnlacesINEConRangoDeYearsAsync();

            // Assert
            Assert.Equal(2, enlaces.Count());
            Assert.Contains("enlace1", enlaces);
            Assert.Contains("enlace2", enlaces);
            Assert.Equal("2019", añoMenor);
            Assert.Equal("2021", añoMayor);
        }

        [Fact]
        public async Task ObtenerEnlacesINEConRangoDeYearsAsync_ReturnsEmptyAndNullsForNoData()
        {
            // Arrange: No agregamos ningún archivo para asegurarnos de que la base de datos está vacía

            // Act
            var (enlaces, añoMenor, añoMayor) = await _repo.ObtenerEnlacesINEConRangoDeYearsAsync();

            // Assert
            Assert.Empty(enlaces);
            Assert.Null(añoMenor);
            Assert.Null(añoMayor);
        }

        [Fact]
        public async Task ObtenerEnlacesCallejeroConYearsAsync_ReturnsLinksAndTheirYears()
        {
            // Arrange
            _context.Archivos.AddRange(
                new Archivo { Enlace = "enlaceC1", Tipo = "CALLEJERO", Year = "2019" },
                new Archivo { Enlace = "enlaceC2", Tipo = "CALLEJERO", Year = "2021" },
                new Archivo { Enlace = "enlaceI1", Tipo = "INE", Year = "2020" }
            );
            await _context.SaveChangesAsync();

            // Act
            var enlacesConAños = await _repo.ObtenerEnlacesCallejeroConYearsAsync();

            // Assert
            Assert.Equal(2, enlacesConAños.Count);
            Assert.Equal("2019", enlacesConAños["enlaceC1"]);
            Assert.Equal("2021", enlacesConAños["enlaceC2"]);
            Assert.False(enlacesConAños.ContainsKey("enlaceI1"));
        }

        [Fact]
        public async Task ObtenerEnlacesCallejeroConYearsAsync_ReturnsEmptyDictionaryForNoData()
        {
            // Arrange: No agregamos ningún archivo para asegurarnos de que la base de datos está vacía

            // Act
            var enlacesConAños = await _repo.ObtenerEnlacesCallejeroConYearsAsync();

            // Assert
            Assert.Empty(enlacesConAños);
        }


    }
}

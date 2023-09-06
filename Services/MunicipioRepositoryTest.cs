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
    public class MunicipioRepositoryTests : IDisposable
    {
        private readonly TfgPrimeroContext _context;
        private readonly MunicipioRepository _repo;

        public MunicipioRepositoryTests()
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
            _repo = new MunicipioRepository(_context);
        }

        // metodo idisposable
        public void Dispose()
        {
            _context.Dispose();
        }

        
        [Fact]
        public async Task ObtenerTodosLosMunicipiosAsync_ReturnsAllMunicipios()
        {
            // Arrange
            var municipios = new List<Municipio>
            {
                new Municipio { IdMunicipio = "1", NombreMunicipio = "Municipio1", IdProvincia = "Prov1" },
                new Municipio { IdMunicipio = "2", NombreMunicipio = "Municipio2", IdProvincia = "Prov2" }
            };
            _context.Municipios.AddRange(municipios);
            _context.SaveChanges();

            // Act
            var result = await _repo.ObtenerTodosLosMunicipiosAsync();

            // Assert
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task ObtenerNombresMunicipiosPorNombreProvinciaAsync_ReturnsCorrectMunicipios()
        {
            // Arrange
            var provincias = new List<Provincium>
            {
                new Provincium { IdProvincia = "Prov1", NombreProvincia = "Prov1" },
                new Provincium { IdProvincia = "Prov2", NombreProvincia = "Prov2" }
            };
            _context.Provincia.AddRange(provincias);

            var municipios = new List<Municipio>
            {
                new Municipio { IdMunicipio = "1", NombreMunicipio = "Municipio1", IdProvincia = "Prov1" },
                new Municipio { IdMunicipio = "2", NombreMunicipio = "Municipio2", IdProvincia = "Prov2" },
                new Municipio { IdMunicipio = "3", NombreMunicipio = "Municipio3", IdProvincia = "Prov1" }
            };
            _context.Municipios.AddRange(municipios);
            _context.SaveChanges();

            // Act
            var result = await _repo.ObtenerNombresMunicipiosPorNombreProvinciaAsync("Prov1");

            // Assert
            Assert.Equal(2, result.Count());
            Assert.Contains("Municipio1", result);
            Assert.Contains("Municipio3", result);
        }

        [Fact]
        public async Task ObtenerMunicipioPorIdAsync_ReturnsCorrectMunicipio()
        {
            // Arrange
            var municipio = new Municipio { IdMunicipio = "1", NombreMunicipio = "Municipio1", IdProvincia = "Prov1" };
            _context.Municipios.Add(municipio);
            _context.SaveChanges();

            // Act
            var result = await _repo.ObtenerMunicipioPorIdAsync("1", "Prov1");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Municipio1", result.NombreMunicipio);
        }

        [Fact]
        public async Task ObtenerMunicipiosNoCoincidentesPorLoteAsync_ReturnsNonMatchingMunicipios()
        {
            // Arrange
            var registrosRenta = new List<RegistroRenta>
            {
                new RegistroRenta { ID_Municipio = "1", Nombre_Municipio = "Municipio1", ID_Provincia = "Prov1" },
                new RegistroRenta { ID_Municipio = "2", Nombre_Municipio = "Municipio2", ID_Provincia = "Prov2" },
                new RegistroRenta { ID_Municipio = "3", Nombre_Municipio = "Municipio3", ID_Provincia = "Prov3" }
            };

            var municipios = new List<Municipio>
            {
                new Municipio { IdMunicipio = "1", NombreMunicipio = "Municipio1", IdProvincia = "Prov1" }
            };
            _context.Municipios.AddRange(municipios);
            _context.SaveChanges();

            // Act
            var result = await _repo.ObtenerMunicipiosNoCoincidentesPorLoteAsync(registrosRenta);

            // Assert
            Assert.Equal(2, result.Count());
            Assert.Contains(result, m => m.NombreMunicipio == "Municipio2");
            Assert.Contains(result, m => m.NombreMunicipio == "Municipio3");
        }

    }
}

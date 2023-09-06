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
    public class SeccionCensalRepositoryTests : IDisposable
    {
        private readonly TfgPrimeroContext _context;
        private readonly SeccionCensalRepository _repo;

        public SeccionCensalRepositoryTests()
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
            _repo = new SeccionCensalRepository(_context);
        }

        // metodo idisposable
        public void Dispose()
        {
            _context.Dispose();
        }


        [Fact]
        public async Task ObtenerTodasLasSeccionesCensalesAsync_ShouldReturnAllSecciones()
        {
            // Añadir datos ficticios
            _context.SeccionCensals.AddRange(
                new SeccionCensal { IdSeccion = "1", IdDistrito = "D1", IdMunicipio = "M1", IdProvincia = "P1" },
                new SeccionCensal { IdSeccion = "2", IdDistrito = "D2", IdMunicipio = "M2", IdProvincia = "P2" }
            );
            await _context.SaveChangesAsync();

            var result = await _repo.ObtenerTodasLasSeccionesCensalesAsync();

            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task ObtenerSeccionCensalPorIdAsync_ShouldReturnCorrectSeccion()
        {
            // Añadir datos ficticios
            _context.SeccionCensals.Add(
                new SeccionCensal { IdSeccion = "1", IdDistrito = "D1", IdMunicipio = "M1", IdProvincia = "P1" }
            );
            await _context.SaveChangesAsync();

            var result = await _repo.ObtenerSeccionCensalPorIdAsync("1", "D1", "M1", "P1");

            Assert.NotNull(result);
            Assert.Equal("1", result.IdSeccion);
            Assert.Equal("D1", result.IdDistrito);
            Assert.Equal("M1", result.IdMunicipio);
            Assert.Equal("P1", result.IdProvincia);
        }

        [Fact]
        public async Task ObtenerSeccionCensalPorIdAsync_ShouldReturnNullForNotFound()
        {
            var result = await _repo.ObtenerSeccionCensalPorIdAsync("999", "D999", "M999", "P999"); // ID que no existe

            Assert.Null(result);
        }



        [Fact]
        public async Task ObtenerSeccionesCensalesNoCoincidentesPorLoteAsync_ShouldReturnNonMatchingSecciones()
        {
            // Añadir datos ficticios
            _context.SeccionCensals.Add(
                new SeccionCensal { IdSeccion = "1", IdDistrito = "D1", IdMunicipio = "M1", IdProvincia = "P1" }
            );
            await _context.SaveChangesAsync();

            var registrosRenta = new List<RegistroRenta>
            {
                new RegistroRenta { ID_Seccion = "2", ID_Distrito = "D2", ID_Municipio = "M2", ID_Provincia = "P2" }
            };

            var result = await _repo.ObtenerSeccionesCensalesNoCoincidentesPorLoteAsync(registrosRenta);

            Assert.Single(result);
            Assert.Equal("2", result[0].IdSeccion);
        }

    }   
}

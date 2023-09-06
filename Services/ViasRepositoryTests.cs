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
    public class ViasRepositoryTests : IDisposable
    {
        private readonly TfgPrimeroContext _context;
        private readonly ViasRepository _repo;

        public ViasRepositoryTests()
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
            _repo = new ViasRepository(_context);
        }

        // metodo idisposable
        public void Dispose()
        {
            _context.Dispose();
        }


        [Fact]
        public async Task ObtenerTodasLasViasAsync_ReturnsAllVias()
        {
            // Arrange
            _context.Via.AddRange(
                new Vium {CodVia = "1", FechaIdFecha = 2023, IdMunicipio = "A01", IdProvincia = "01", NombVia = "Via1", NombCortoVia = "V1", TipoVia = "Residential"},
                new Vium {CodVia = "2", FechaIdFecha = 2023, IdMunicipio = "A02", IdProvincia = "01", NombVia = "Via2", NombCortoVia = "V2", TipoVia = "Commercial"}
            );
            await _context.SaveChangesAsync();

            // Act
            var result = await _repo.ObtenerTodasLasViasAsync();

            // Assert
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task ObtenerTiposViasAsync_ReturnsDistinctVias()
        {
            // Arrange
            _context.Via.AddRange(
                new Vium {CodVia = "1", FechaIdFecha = 2023, IdMunicipio = "A01", IdProvincia = "01", TipoVia = "Residential", NombVia = "Via1", NombCortoVia = "V1"},
                new Vium {CodVia = "2", FechaIdFecha = 2023, IdMunicipio = "A02", IdProvincia = "01", TipoVia = "Commercial", NombVia = "Via2", NombCortoVia = "V2"},
                new Vium {CodVia = "3", FechaIdFecha = 2023, IdMunicipio = "A03", IdProvincia = "01", TipoVia = "Residential", NombVia = "Via3", NombCortoVia = "V3"}
            );
            await _context.SaveChangesAsync();

            // Act
            var result = await _repo.ObtenerTiposViasAsync();

            // Assert
            Assert.Equal(2, result.Count);
        }






    }   
}

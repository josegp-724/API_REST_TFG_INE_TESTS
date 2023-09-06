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
    public class TramosRepositoryTests : IDisposable
    {
        private readonly TfgPrimeroContext _context;
        private readonly TramosRepository _repo;

        public TramosRepositoryTests()
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
            _repo = new TramosRepository(_context);
        }

        // metodo idisposable
        public void Dispose()
        {
            _context.Dispose();
        }


        [Fact]
        public async Task ObtenerTodosLosTramosAsync_ReturnsAllTramos()
        {
            // Arrange
            var tramo1 = CreateSampleTramo();
            _context.Tramos.Add(tramo1);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repo.ObtenerTodosLosTramosAsync();

            // Assert
            Assert.Equal(1, result.Count());
        }

        private Tramo CreateSampleTramo()
        {
            return new Tramo 
            {
                TipoNumeracion = "A",
                ExtrInfNav = 1,
                CalExtrInfNav = "B",
                ExtrSupNav = 2,
                CalExtrSupNav = "C",
                CodPostal = "12345",
                CodUp = "1234567",
                IdSeccion = "001",
                IdDistrito = "01",
                IdMunicipio = "001",
                IdProvincia = "01",
                CodVia = "12345",
                FechaIdFecha = DateTime.Now.Year
            };
        }


        [Fact]
        public void RepoStartsEmpty()
        {
            // Act
            var result = _context.Tramos.Count();

            // Assert
            Assert.Equal(0, result);
        }





    }   
}

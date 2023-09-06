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
    public class SexoRepositoryTests : IDisposable
    {
        private readonly TfgPrimeroContext _context;
        private readonly SexoRepository _repo;

        public SexoRepositoryTests()
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
            _repo = new SexoRepository(_context);
        }

        // metodo idisposable
        public void Dispose()
        {
            _context.Dispose();
        }


        [Fact]
        public async Task ObtenerValoresSexoAsync_ShouldReturnAllSexoValues()
        {
            // Arrange: Add dummy data
            var expected = new List<Sexo>
            {
                new Sexo { ValorSexo = "Masculino" },
                new Sexo { ValorSexo = "Femenino" }
            };

            await _context.AddRangeAsync(expected);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repo.ObtenerValoresSexoAsync();

            // Assert
            Assert.Equal(expected.Select(s => s.ValorSexo), result);
        }

        [Fact]
        public async Task ObtenerTodosLosSexosAsync_ShouldReturnAllSexos()
        {
            // Arrange: Add dummy data
            var expected = new List<Sexo>
            {
                new Sexo { ValorSexo = "Masculino" },
                new Sexo { ValorSexo = "Femenino" }
            };

            await _context.AddRangeAsync(expected);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repo.ObtenerTodosLosSexosAsync();

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public async Task ObtenerSexoPorIdAsync_ShouldReturnCorrectSexo()
        {
            // Arrange
            var sexo = new Sexo { ValorSexo = "Masculino" };
            await _context.Sexos.AddAsync(sexo);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repo.ObtenerSexoPorIdAsync(sexo.IdSexo);

            // Assert
            Assert.Equal(sexo, result);
        }

        [Fact]
        public async Task ExisteSexoConNombreAsync_ShouldReturnCorrectSexo()
        {
            // Arrange
            var sexo = new Sexo { ValorSexo = "Masculino" };
            await _context.Sexos.AddAsync(sexo);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repo.ExisteSexoConNombreAsync(sexo.ValorSexo);

            // Assert
            Assert.Equal(sexo, result);
        }




    }   
}

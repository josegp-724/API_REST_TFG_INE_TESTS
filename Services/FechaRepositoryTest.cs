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
    public class FechaRepositoryTests : IDisposable
    {
        private readonly TfgPrimeroContext _context;
        private readonly FechaRepository _repo;

        public FechaRepositoryTests()
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
            _repo = new FechaRepository(_context);
        }

        // metodo idisposable
        public void Dispose()
        {
            _context.Dispose();
        }



        [Fact]
        public async Task ObtenerAniosAsync_ShouldReturn_ListOfYears()
        {
            var fechas = new List<Fecha>
            {
                new Fecha { Anyo = "2021" },
                new Fecha { Anyo = "2022" }
            };

            await _context.Fechas.AddRangeAsync(fechas);
            await _context.SaveChangesAsync();

            var result = await _repo.ObtenerAniosAsync();

            Assert.Equal(2, result.Count);
            Assert.Contains("2021", result);
            Assert.Contains("2022", result);
        }

        [Fact]
        public async Task ObtenerTodasLasFechasAsync_ShouldReturn_AllDates()
        {
            var fechas = new List<Fecha>
            {
                new Fecha { Anyo = "2021" },
                new Fecha { Anyo = "2022" }
            };

            await _context.Fechas.AddRangeAsync(fechas);
            await _context.SaveChangesAsync();

            var result = await _repo.ObtenerTodasLasFechasAsync();

            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task ObtenerFechaPorIdAsync_ShouldReturn_CorrectDate()
        {
            var fecha = new Fecha { Anyo = "2021" };
            await _context.Fechas.AddAsync(fecha);
            await _context.SaveChangesAsync();

            var result = await _repo.ObtenerFechaPorIdAsync(fecha.IdFecha);

            Assert.Equal("2021", result.Anyo);
        }

        [Fact]
        public async Task ExisteFechaConAnyoAsync_ShouldReturn_TrueIfExists()
        {
            var fecha = new Fecha { Anyo = "2021" };
            await _context.Fechas.AddAsync(fecha);
            await _context.SaveChangesAsync();

            var result = await _repo.ExisteFechaConAnyoAsync("2021");

            Assert.NotNull(result);
            Assert.Equal("2021", result.Anyo);
        }


        
        [Fact]
        public async Task ObtenerAniosAsync_ShouldReturn_EmptyList_WhenNoDatesExist()
        {
            var result = await _repo.ObtenerAniosAsync();

            Assert.Empty(result);
        }

        [Fact]
        public async Task ObtenerTodasLasFechasAsync_ShouldReturn_Empty_WhenNoDatesExist()
        {
            var result = await _repo.ObtenerTodasLasFechasAsync();

            Assert.Empty(result);
        }

        [Fact]
        public async Task ObtenerFechaPorIdAsync_ShouldReturn_Null_WhenDateDoesNotExist()
        {
            var result = await _repo.ObtenerFechaPorIdAsync(999); // Un ID que probablemente no exista

            Assert.Null(result);
        }

        [Fact]
        public async Task ExisteFechaConAnyoAsync_ShouldReturn_Null_WhenYearDoesNotExist()
        {
            var result = await _repo.ExisteFechaConAnyoAsync("2099"); // Un año que probablemente no exista

            Assert.Null(result);
        }

        [Fact]
        public async Task ExisteFechaConAnyoAsync_ShouldHandle_EmptyString()
        {
            await Assert.ThrowsAsync<ArgumentException>(() => _repo.ExisteFechaConAnyoAsync(""));
        }

        [Fact]
        public async Task ExisteFechaConAnyoAsync_ShouldHandle_NullString()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _repo.ExisteFechaConAnyoAsync(null));
        }


    }
}

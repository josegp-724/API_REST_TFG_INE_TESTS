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
    public class ConsultaRepositoryTests : IDisposable
    {
        private readonly TfgPrimeroContext _context;
        private readonly ConsultaRepository _repo;

        public ConsultaRepositoryTests()
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
            _repo = new ConsultaRepository(_context);
        }

        public void Dispose()
        {
            _context.Dispose();
        }


        // Aquí puedes agregar tus tests unitarios para las funciones del repositorio

        [Fact]
        public async Task ObtenerTodasLasConsultasAsync_ReturnsAllConsultas()
        {
            var consulta1 = new Consultum 
            { 
                IdConsulta = 1, 
                NombVia = "Calle A", 
                CodPostal = "12345",
                NombMunicipio = "Municipio A",
                NombProvincia = "Provincia A",
                TipoVia = "Tipo A"
            };
            var consulta2 = new Consultum 
            { 
                IdConsulta = 2, 
                NombVia = "Calle B", 
                CodPostal = "67890",
                NombMunicipio = "Municipio B",
                NombProvincia = "Provincia B",
                TipoVia = "Tipo B"
            };
            _context.Consulta.AddRange(consulta1, consulta2);
            await _context.SaveChangesAsync();

            var result = await _repo.ObtenerTodasLasConsultasAsync();

            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task ObtenerConsultaPorIdAsync_ReturnsCorrectConsulta()
        {
            var consulta1 = new Consultum 
            { 
                IdConsulta = 1, 
                NombVia = "Calle A", 
                CodPostal = "12345",
                NombMunicipio = "Municipio A",
                NombProvincia = "Provincia A",
                TipoVia = "Tipo A"
            };

            _context.Consulta.Add(consulta1);
            await _context.SaveChangesAsync();

            var result = await _repo.ObtenerConsultaPorIdAsync(1);

            Assert.Equal(1, result.IdConsulta);
            Assert.Equal("Calle A", result.NombVia);
        }

        [Fact]
        public async Task ObtenerConsultaPorIdAsync_ReturnsNullForNonexistentId()
        {
            var consulta1 = new Consultum 
            { 
                IdConsulta = 1, 
                NombVia = "Calle A", 
                CodPostal = "12345",
                NombMunicipio = "Municipio A",
                NombProvincia = "Provincia A",
                TipoVia = "Tipo A"
            };
            _context.Consulta.Add(consulta1);
            await _context.SaveChangesAsync();

            var result = await _repo.ObtenerConsultaPorIdAsync(99);

            Assert.Null(result);
        }

        [Fact]
        public async Task AgregarConsultasAsync_AddsConsultasToDb()
        {
            var consulta1 = new Consultum 
            { 
                IdConsulta = 1, 
                NombVia = "Calle A", 
                CodPostal = "12345",
                NombMunicipio = "Municipio A",
                NombProvincia = "Provincia A",
                TipoVia = "Tipo A"
            };
            var consulta2 = new Consultum 
            { 
                IdConsulta = 2, 
                NombVia = "Calle B", 
                CodPostal = "67890",
                NombMunicipio = "Municipio B",
                NombProvincia = "Provincia B",
                TipoVia = "Tipo B"
            };

            await _repo.AgregarConsultasAsync(new List<Consultum> { consulta1, consulta2 });

            Assert.Equal(2, _context.Consulta.Count());
        }


    [Fact]
    public async Task ActualizarConsultaAsync_UpdatesExistingConsulta()
    {
        // Preparar
        var consulta = new Consultum 
        { 
            IdConsulta = 1, 
            NombVia = "Calle A", 
            CodPostal = "12345",
            NombMunicipio = "Municipio A",
            NombProvincia = "Provincia A",
            TipoVia = "Tipo A"
        };

        _context.Consulta.Add(consulta);
        await _context.SaveChangesAsync();

        consulta.NombVia = "Calle B";

        // Actuar
        await _repo.ActualizarConsultaAsync(consulta);

        // Verificar
        var consultaActualizada = await _context.Consulta.FindAsync(1);
        Assert.Equal("Calle B", consultaActualizada.NombVia);
    }

    [Fact]
    public async Task EliminarConsultaAsync_RemovesConsultaFromDb()
    {
        // Preparar
        var consulta = new Consultum 
        { 
            IdConsulta = 1, 
            NombVia = "Calle A", 
            CodPostal = "12345",
            NombMunicipio = "Municipio A",
            NombProvincia = "Provincia A",
            TipoVia = "Tipo A"
        };

        _context.Consulta.Add(consulta);
        await _context.SaveChangesAsync();

        // Actuar
        await _repo.EliminarConsultaAsync(1);

        // Verificar
        var consultaEliminada = await _context.Consulta.FindAsync(1);
        Assert.Null(consultaEliminada);
    }


    }
}

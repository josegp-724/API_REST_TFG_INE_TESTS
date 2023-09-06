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
    public class AtributoRepositoryTests : IDisposable
    {
        private readonly TestTfgPrimeroContext _context;
        private readonly AtributoRepository _repo;

        public AtributoRepositoryTests()
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
            _repo = new AtributoRepository(_context);
        }

        public void Dispose()
        {
            _context.Dispose();
        }

        [Fact]
        public async void ObtenerNombresAtributosAsync_ReturnsEmptyListIfNoAtributos()
        {
            var result = await _repo.ObtenerNombresAtributosAsync();
            Assert.Empty(result);
        }

        [Fact]
        public async void ObtenerNombresAtributosAsync_ReturnsCorrectNames()
        {
            // Arrange
            var atributos = new List<Atributo>
            {
                new Atributo { Nombre = "Atributo1" },
                new Atributo { Nombre = "Atributo2" }
            };
            
            _context.Atributos.AddRange(atributos);
            _context.SaveChanges();

            // Act
            var result = await _repo.ObtenerNombresAtributosAsync();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Contains("Atributo1", result);
            Assert.Contains("Atributo2", result);
        }

        [Fact]
        public async void ObtenerNombresAtributosAsync_ReturnsNamesWithoutDuplicates()
        {
            // Arrange
            var atributos = new List<Atributo>
            {
                new Atributo { Nombre = "Atributo1" },
                new Atributo { Nombre = "Atributo1" }
            };
            
            _context.Atributos.AddRange(atributos);
            _context.SaveChanges();

            // Act
            var result = await _repo.ObtenerNombresAtributosAsync();

            // Assert
            Assert.Single(result);
            Assert.Contains("Atributo1", result);
        }

        [Fact]
        public async void ObtenerTodosLosAtributosAsync_ReturnsEmptyListIfNoAtributos()
        {
            var result = await _repo.ObtenerTodosLosAtributosAsync();
            Assert.Empty(result);
        }

        [Fact]
        public async void ObtenerTodosLosAtributosAsync_ReturnsAllAtributos()
        {
            // Arrange
            var atributos = new List<Atributo>
            {
                new Atributo { Nombre = "Atributo1" },
                new Atributo { Nombre = "Atributo2" }
            };
            
            _context.Atributos.AddRange(atributos);
            _context.SaveChanges();

            // Act
            var result = await _repo.ObtenerTodosLosAtributosAsync();

            // Assert
            Assert.Equal(2, result.Count());
            Assert.Contains(atributos[0], result);
            Assert.Contains(atributos[1], result);
        }



        [Fact]
        public async Task ObtenerAtributoPorIdAsync_ReturnsAtributo_WhenIdExists()
        {
            // Arrange
            var atributo = new Atributo { Nombre = "AtributoTest" };
            _context.Atributos.Add(atributo);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repo.ObtenerAtributoPorIdAsync(atributo.IdAtributo);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(atributo.IdAtributo, result.IdAtributo);
            Assert.Equal(atributo.Nombre, result.Nombre);
        }


        [Fact]
        public async Task ObtenerAtributoPorIdAsync_ReturnsNull_WhenIdDoesNotExist()
        {
            // Act
            var result = await _repo.ObtenerAtributoPorIdAsync(99999); // Un ID que no existe

            // Assert
            Assert.Null(result);
        }


        [Fact]
        public async Task ExisteAtributoConNombreAsync_ReturnsAtributo_WhenNameExists()
        {
            // Arrange
            var atributo = new Atributo { Nombre = "AtributoExistente" };
            _context.Atributos.Add(atributo);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repo.ExisteAtributoConNombreAsync(atributo.Nombre);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(atributo.Nombre, result.Nombre);
        }

        [Fact]
        public async Task ExisteAtributoConNombreAsync_ReturnsNull_WhenNameDoesNotExist()
        {
            // Act
            var result = await _repo.ExisteAtributoConNombreAsync("NombreNoExistente");

            // Assert
            Assert.Null(result);
        }


        [Fact]
        public async Task ObtenerAtributosNoCoincidentesPorLoteAsync_ReturnsEmptyList_WhenAllMatch()
        {
            // Arrange
            var atributos = new List<Atributo>
            {
                new Atributo { Nombre = "Atributo1" },
                new Atributo { Nombre = "Atributo2" }
            };

            _context.Atributos.AddRange(atributos);
            await _context.SaveChangesAsync();

            var registrosRenta = new List<RegistroRenta>
            {
                new RegistroRenta { Atributo = "Atributo1" },
                new RegistroRenta { Atributo = "Atributo2" }
            };

            // Act
            var result = await _repo.ObtenerAtributosNoCoincidentesPorLoteAsync(registrosRenta);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task ObtenerAtributosNoCoincidentesPorLoteAsync_ReturnsList_WhenNotAllMatch()
        {
            // Arrange
            var atributos = new List<Atributo>
            {
                new Atributo { Nombre = "Atributo1" }
            };

            _context.Atributos.AddRange(atributos);
            await _context.SaveChangesAsync();

            var registrosRenta = new List<RegistroRenta>
            {
                new RegistroRenta { Atributo = "Atributo1" },
                new RegistroRenta { Atributo = "Atributo2" }
            };

            // Act
            var result = await _repo.ObtenerAtributosNoCoincidentesPorLoteAsync(registrosRenta);

            // Assert
            Assert.Single(result);
            Assert.Equal("Atributo2", result.First().Nombre);
        }

        [Fact]
        public async Task ObtenerAtributosNoCoincidentesPorLoteAsync_ReturnsAll_WhenNoneMatch()
        {
            // Arrange
            var registrosRenta = new List<RegistroRenta>
            {
                new RegistroRenta { Atributo = "Atributo1" },
                new RegistroRenta { Atributo = "Atributo2" }
            };

            // Act
            var result = await _repo.ObtenerAtributosNoCoincidentesPorLoteAsync(registrosRenta);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Contains(result, a => a.Nombre == "Atributo1");
            Assert.Contains(result, a => a.Nombre == "Atributo2");
        }



        [Fact]
        public async Task AnadirAtributoAsync_AddsSingleAttribute()
        {
            // Arrange
            var atributo = new Atributo { Nombre = "Atributo1" };

            // Act
            await _repo.AñadirAtributoAsync(atributo);

            // Assert
            var result = _context.Atributos.FirstOrDefault();
            Assert.NotNull(result);
            Assert.Equal("Atributo1", result.Nombre);
        }

        [Fact]
        public async Task ActualizarAtributoAsync_UpdatesExistingAttribute()
        {
            // Arrange
            var atributo = new Atributo { Nombre = "Atributo1" };
            _context.Atributos.Add(atributo);
            await _context.SaveChangesAsync();

            atributo.Nombre = "AtributoModificado";

            // Act
            await _repo.ActualizarAtributoAsync(atributo);

            // Assert
            var result = _context.Atributos.FirstOrDefault();
            Assert.NotNull(result);
            Assert.Equal("AtributoModificado", result.Nombre);
        }

        [Fact]
        public async Task EliminaratributoAsync_DeletesExistingAttribute()
        {
            // Arrange
            var atributo = new Atributo { Nombre = "Atributo1" };
            _context.Atributos.Add(atributo);
            await _context.SaveChangesAsync();

            // Act
            await _repo.EliminaratributoAsync(atributo.IdAtributo);

            // Assert
            var result = _context.Atributos.FirstOrDefault();
            Assert.Null(result);
        }
        [Fact]
        public async Task EliminaratributoAsync_DoesNothingIfAttributeDoesNotExist()
        {
            // Arrange
            // No añadir ningún atributo al contexto.

            // Act
            await _repo.EliminaratributoAsync(999); // ID inexistente.

            // Assert
            var result = _context.Atributos.ToList();
            Assert.Empty(result);
        }


    }
}

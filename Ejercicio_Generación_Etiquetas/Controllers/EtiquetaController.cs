using Ejercicio_Generación_Etiquetas.Models;
using Ejercicio_Generación_Etiquetas.Repositories;
using Ejercicio_Generación_Etiquetas.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Ejercicio_Generación_Etiquetas.Controllers
{
    public class EtiquetaController : Controller
    {
        private readonly IEtiquetaRepository _repo;

        private static readonly string[] Impresoras = ["Impresora-01", "Impresora-02", "Impresora-03"];

        public EtiquetaController(IEtiquetaRepository repo)
        {
            _repo = repo;
        }

        public IActionResult Dashboard()
        {
            var todas = _repo.ObtenerTodas().ToList();
            var hoy = DateTime.Today;

            var dashboard = new DashboardEtiquetaViewModel
            {
                TotalEtiquetas = todas.Count,
                Pendientes = todas.Count(e => e.Estado == EstadoEtiqueta.Pendiente),
                Procesando = todas.Count(e => e.Estado == EstadoEtiqueta.Procesando),
                Impresas = todas.Count(e => e.Estado == EstadoEtiqueta.Impresa),
                Errores = todas.Count(e => e.Estado == EstadoEtiqueta.Error),
                Canceladas = todas.Count(e => e.Estado == EstadoEtiqueta.Cancelada),
                ProduccionHoy = todas
                    .Where(e => e.FechaSolicitud.Date == hoy)
                    .Sum(e => e.CantidadGenerada),
                PorcentajeGlobal = _repo.CalcularAvanceGlobal(),
                FechaServidor = DateTime.Now.ToString("dd/MM/yyyy HH:mm"),
                UltimasSolicitudes = todas
                    .OrderByDescending(e => e.FechaSolicitud)
                    .Take(5)
                    .Select(EtiquetaViewModel.DesdeModelo)
                    .ToList()
            };

            ViewData["TotalEtiquetas"] = dashboard.TotalEtiquetas;
            ViewData["Pendientes"] = dashboard.Pendientes;
            ViewData["Impresas"] = dashboard.Impresas;
            ViewData["Errores"] = dashboard.Errores;
            ViewData["ProduccionHoy"] = dashboard.ProduccionHoy;
            ViewData["FechaServidor"] = dashboard.FechaServidor;

            return View(dashboard);
        }

        public IActionResult Index(string? termino, EstadoEtiqueta? estado, PrioridadEtiqueta? prioridad, string? impresora)
        {
            var etiquetas = !string.IsNullOrWhiteSpace(termino)
                ? _repo.Buscar(termino)
                : _repo.ObtenerTodas();

            if (estado.HasValue)
            {
                etiquetas = etiquetas.Where(e => e.Estado == estado.Value);
            }

            if (prioridad.HasValue)
            {
                etiquetas = etiquetas.Where(e => e.Prioridad == prioridad.Value);
            }

            if (!string.IsNullOrWhiteSpace(impresora))
            {
                etiquetas = etiquetas.Where(e =>
                    e.Impresora.Equals(impresora, StringComparison.OrdinalIgnoreCase));
            }

            var lista = etiquetas.Select(EtiquetaViewModel.DesdeModelo).ToList();

            ViewBag.Termino = termino;
            ViewBag.Estado = estado;
            ViewBag.Prioridad = prioridad;
            ViewBag.Impresora = impresora;
            ViewBag.Estados = new SelectList(Enum.GetValues<EstadoEtiqueta>(), estado);
            ViewBag.Prioridades = new SelectList(Enum.GetValues<PrioridadEtiqueta>(), prioridad);
            ViewBag.Impresoras = new SelectList(Impresoras, impresora);

            return View(lista);
        }

        public IActionResult Detail(int id)
        {

            var etiqueta = _repo.ObtenerPorId(id);

            if (etiqueta is null)
            {
                return NotFound();
            }

            var viewmodel = EtiquetaViewModel.DesdeModelo(etiqueta);
            return View(viewmodel);
        }


        private void CargarListasFormulario()
        {
            ViewBag.Modelos = new SelectList(new[]
            {
          "Camisa", "Pantalón", "Chaqueta", "Vestido", "Sudadera"
            });

            ViewBag.Colores = new SelectList(new[]
            {
          "Negro", "Blanco", "Azul", "Rojo", "Verde"
             });

            ViewBag.Tallas = new SelectList(new[]
            {
          "S", "M", "L", "XL"
             });

            ViewBag.Impresoras = new SelectList(Impresoras);

            ViewBag.Prioridades = new SelectList(
                Enum.GetValues<PrioridadEtiqueta>());

            ViewBag.Estados = new SelectList(
                Enum.GetValues<EstadoEtiqueta>());
        }
        [HttpGet]
        public IActionResult Create()
        {
            CargarListasFormulario();

            return View(new EtiquetaViewModel
            {
                Estado = EstadoEtiqueta.Pendiente,
                Prioridad = PrioridadEtiqueta.Normal,
                CantidadGenerada = 0
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(EtiquetaViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                CargarListasFormulario();
                return View(viewModel);
            }

            try
            {
                _repo.Agregar(viewModel.ToModelo());

                TempData["Exito"] = "La solicitud se creó correctamente.";

                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);

                CargarListasFormulario();
                return View(viewModel);
            }
        }


        public IActionResult Edit(int id)
        {
            var etiqueta = _repo.ObtenerPorId(id);

            if (etiqueta is null)
            {
                return NotFound();
            }

            CargarListasFormulario();

            return View(EtiquetaViewModel.DesdeModelo(etiqueta));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, EtiquetaViewModel viewModel)
        {
            if (id != viewModel.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                CargarListasFormulario();
                return View(viewModel);
            }

            try
            {
                _repo.Actualizar(viewModel.ToModelo());

                TempData["Exito"] = "La solicitud se actualizó correctamente.";

                return RedirectToAction(nameof(Detail), new { id });
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);

                CargarListasFormulario();
                return View(viewModel);
            }
        }

        public IActionResult Delete(int id)
        {
            var etiqueta = _repo.ObtenerPorId(id);

            if (etiqueta is null)
            {
                return NotFound();
            }

            return View(EtiquetaViewModel.DesdeModelo(etiqueta));
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var etiqueta = _repo.ObtenerPorId(id);

            if (etiqueta is null)
            {
                return NotFound();
            }

            if (!_repo.Eliminar(id))
            {
                TempData["Error"] = "No se puede eliminar una etiqueta que ya fue impresa.";
                return RedirectToAction(nameof(Detail), new { id });
            }

            TempData["Exito"] = "La solicitud se eliminó correctamente.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Generar(int id, int cantidad)
        {
            try
            {
                _repo.GenerarEtiquetas(id, cantidad);
                TempData["Exito"] = "Las etiquetas se generaron correctamente.";
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Detail), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Cancelar(int id)
        {
            try
            {
                _repo.Cancelar(id);
                TempData["Exito"] = "La solicitud fue cancelada correctamente.";
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Detail), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CambiarEstado(int id, EstadoEtiqueta nuevoEstado)
        {
            try
            {
                _repo.CambiarEstado(id, nuevoEstado);
                TempData["Exito"] = "El estado de la solicitud se actualizó correctamente.";
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Detail), new { id });
        }
    }
}

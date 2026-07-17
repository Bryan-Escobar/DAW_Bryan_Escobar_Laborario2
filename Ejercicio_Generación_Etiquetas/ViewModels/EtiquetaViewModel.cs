using Ejercicio_Generación_Etiquetas.Models;
using System.ComponentModel.DataAnnotations;

namespace Ejercicio_Generación_Etiquetas.ViewModels
{
    public class EtiquetaViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El número de etiqueta es obligatorio.")]
        [Display(Name = "Número de etiqueta")]
        public string NumeroEtiqueta { get; set; } = string.Empty;

        [Required(ErrorMessage = "La orden es obligatoria.")]
        [Display(Name = "Orden")]
        public string Orden { get; set; } = string.Empty;

        [Required(ErrorMessage = "El modelo es obligatorio.")]
        [Display(Name = "Modelo")]
        public string Modelo { get; set; } = string.Empty;

        [Required(ErrorMessage = "El color es obligatorio.")]
        [Display(Name = "Color")]
        public string Color { get; set; } = string.Empty;

        [Required(ErrorMessage = "La talla es obligatoria.")]
        [Display(Name = "Talla")]
        public string Talla { get; set; } = string.Empty;

        [Required(ErrorMessage = "El usuario es obligatorio.")]
        [Display(Name = "Usuario")]
        public string Usuario { get; set; } = string.Empty;

        [Required(ErrorMessage = "La impresora es obligatoria.")]
        [Display(Name = "Impresora")]
        public string Impresora { get; set; } = string.Empty;

        [Range(1, int.MaxValue, ErrorMessage = "La cantidad solicitada debe ser mayor a cero.")]
        [Display(Name = "Cantidad solicitada")]
        public int CantidadSolicitada { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "La cantidad generada no puede ser negativa.")]
        [Display(Name = "Cantidad generada")]
        public int CantidadGenerada { get; set; }

        [Display(Name = "Estado")]
        public EstadoEtiqueta Estado { get; set; } = EstadoEtiqueta.Pendiente;

        [Display(Name = "Prioridad")]
        public PrioridadEtiqueta Prioridad { get; set; } = PrioridadEtiqueta.Normal;

        [Display(Name = "Fecha de solicitud")]
        public DateTime FechaSolicitud { get; set; } = DateTime.Now;

        [Display(Name = "Cantidad pendiente")]
        public int CantidadPendiente { get; set; }

        [Display(Name = "Porcentaje de avance")]
        public double PorcentajeAvance { get; set; }

        [Display(Name = "Tiempo transcurrido")]
        public string TiempoTranscurrido { get; set; } = string.Empty;

        public string ClaseBootstrapEstado { get; set; } = "secondary";

        public static EtiquetaViewModel DesdeModelo(EtiquetaProduccion etiqueta)
        {
            var transcurrido = DateTime.Now - etiqueta.FechaSolicitud;

            return new EtiquetaViewModel
            {
                Id = etiqueta.Id,
                NumeroEtiqueta = etiqueta.NumeroEtiqueta,
                Orden = etiqueta.Orden,
                Modelo = etiqueta.Modelo,
                Color = etiqueta.Color,
                Talla = etiqueta.Talla,
                Usuario = etiqueta.Usuario,
                Impresora = etiqueta.Impresora,
                CantidadSolicitada = etiqueta.CantidadSolicitada,
                CantidadGenerada = etiqueta.CantidadGenerada,
                Estado = etiqueta.Estado,
                Prioridad = etiqueta.Prioridad,
                FechaSolicitud = etiqueta.FechaSolicitud,
                CantidadPendiente = etiqueta.CantidadPendiente,
                PorcentajeAvance = etiqueta.PorcentajeAvance,
                TiempoTranscurrido = FormatearTiempo(transcurrido),
                ClaseBootstrapEstado = etiqueta.ClaseBootstrapEstado
            };
        }

        public EtiquetaProduccion ToModelo()
        {
            return new EtiquetaProduccion
            {
                Id = Id,
                NumeroEtiqueta = NumeroEtiqueta,
                Orden = Orden,
                Modelo = Modelo,
                Color = Color,
                Talla = Talla,
                Usuario = Usuario,
                Impresora = Impresora,
                CantidadSolicitada = CantidadSolicitada,
                CantidadGenerada = CantidadGenerada,
                Estado = Estado,
                Prioridad = Prioridad,
                FechaSolicitud = FechaSolicitud
            };
        }

        private static string FormatearTiempo(TimeSpan tiempo)
        {
            if (tiempo.TotalDays >= 1)
            {
                return $"{(int)tiempo.TotalDays}d {tiempo.Hours}h";
            }

            if (tiempo.TotalHours >= 1)
            {
                return $"{(int)tiempo.TotalHours}h {tiempo.Minutes}m";
            }

            return $"{tiempo.Minutes}m";
        }
    }
}

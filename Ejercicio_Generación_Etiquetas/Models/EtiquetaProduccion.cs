using System.ComponentModel.DataAnnotations;

namespace Ejercicio_Generación_Etiquetas.Models
{
    public class EtiquetaProduccion
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
        public int CantidadPendiente => CantidadSolicitada - CantidadGenerada;

        [Display(Name = "Porcentaje de avance")]
        public double PorcentajeAvance =>
            CantidadSolicitada == 0 ? 0 : (double)CantidadGenerada / CantidadSolicitada * 100;

        public string ClaseBootstrapEstado => Estado switch
        {
            EstadoEtiqueta.Pendiente => "secondary",
            EstadoEtiqueta.Procesando => "primary",
            EstadoEtiqueta.Impresa => "success",
            EstadoEtiqueta.Error => "danger",
            EstadoEtiqueta.Cancelada => "warning",
            _ => "secondary"
        };
    }
}

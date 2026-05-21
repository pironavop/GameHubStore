using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace GameHubStore.Models.ViewModels
{
    public class GameViewModel
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Range(1, 999999)]
        public decimal Price { get; set; }

        public decimal? DiscountPrice { get; set; }

        [Required]
        public DateTime ReleaseDate { get; set; }

        public string? CoverImageUrl { get; set; }
        public IFormFile? CoverImageFile { get; set; }

        public string? TrailerUrl { get; set; }

        [Range(0, 99999)]
        public int StockQuantity { get; set; }

        public bool IsActive { get; set; } = true;

        [Required]
        public int CategoryId { get; set; }

        public List<int> SelectedPlatformIds { get; set; } = new List<int>();

        public List<SelectListItem> Categories { get; set; } = new List<SelectListItem>();

        public List<SelectListItem> Platforms { get; set; } = new List<SelectListItem>();
    }
}
﻿using System.ComponentModel.DataAnnotations;

namespace Eticaret.Models
{
	public class Genre
	{
		public int Id { get; set; }
		[StringLength(30)]
		public string Name { get; set; }
		[StringLength(50)]
		public string? FotoPath { get; set; }

		public List<Album> Albums { get; set; }//o turden birden fazla album olabilir
	}
}

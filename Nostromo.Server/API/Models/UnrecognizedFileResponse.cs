
using System;
using System.Collections.Generic;
namespace Nostromo.Models;

public class UnrecognizedFileResponse
{
    public int VideoID { get; set; }
    public string FileName { get; set; }
    public DateTime? Timestamp { get; set; }
    public List<TmdbMovieResponse> Guesses { get; set; }
}

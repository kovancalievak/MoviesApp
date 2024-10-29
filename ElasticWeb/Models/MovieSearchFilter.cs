using System;
namespace ElasticWeb.Models
{
	public class MovieSearchFilter
	{
        public string MovieName { get; set; }

        public string GenreName { get; set; }

        public string CastName { get; set; }
    }

    public class MovieSearchResult
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public string PosterPath { get; set; }

        public DateTime ReleaseDate { get; set; }

        public string Genres { get; set; }

        public string Cast { get; set; }

        public double VoteAverage { get; set; }
    }
}


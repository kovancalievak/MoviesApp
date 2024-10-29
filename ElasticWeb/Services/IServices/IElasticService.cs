using System;
using ElasticWeb.Models;

namespace ElasticWeb.Services.IServices
{
	public interface IElasticService
	{
		public long GetMoviesCount();

		public IEnumerable<Movie> GetTop50Movies();

		public Movie GetMovieDetails(string movieId);

		public IEnumerable<Movie> GetTop50MoviesByRevenue();

		public IEnumerable<ChartKeyValue> GetMoviesByDecade();

		public IEnumerable<ChartKeyValue> GetMoviesCountByRating();

		public IEnumerable<MovieSearchResult> SearchMovies(MovieSearchFilter sf);
	}
}


using System;
using ElasticWeb.Models;
using ElasticWeb.Services.IServices;
using Nest;

namespace ElasticWeb.Services
{
	public class ElasticService : IElasticService
	{
        private readonly IElasticClient _elastic;

        public ElasticService(IElasticClient elastic)
        {
            _elastic = elastic;
        }

        public IEnumerable<Movie> GetTop50Movies()
        {
            var movies = _elastic.Search<Movie>(m => m.Index("movie")
            .Query(q => q
                .Term(t => t
                    .Field(f => f.OriginalLanguage)
                    .Value("en")
                )
            )
            .Sort(
                s => s
                .Field(f => f
                    .Field(p => p.VoteAverage)
                    .Order(SortOrder.Descending)
                )
                .Field(f => f
                    .Field(p => p.VoteCount)
                    .Order(SortOrder.Descending)
                )
            )
            .Size(50)
            );
            return movies.Documents;
        }

        public Movie GetMovieDetails(string movieId)
        {
            var movies = _elastic.Search<Movie>(m => m.Index("movie")
                .Query(q => q
                    .Term(t => t
                        .Field(f => f.Id)
                        .Value(Convert.ToInt32(movieId))
                    )
                )
            );
            return movies.Documents.FirstOrDefault();
        }

        public long GetMoviesCount()
        {
            var movies = _elastic.Count<Movie>(c => c.Index("movie"));
            return movies.Count;
        }

        public IEnumerable<ChartKeyValue> GetMoviesByDecade()
        {
            var movies = _elastic.Search<Movie>(m => m.Index("movie")
                .Aggregations(ag => ag
                    .DateRange("by_decade", dr => dr
                        .Field(p => p.ReleaseDate)
                        .Ranges(
                            r => r.Key("30's & earlier").To("1939-12-31"),
                            r => r.Key("40's").From("1940-01-01").To("1949-12-31"),
                            r => r.Key("50's").From("1950-01-01").To("1959-12-31"),
                            r => r.Key("60's").From("1960-01-01").To("1969-12-31"),
                            r => r.Key("70's").From("1970-01-01").To("1979-12-31"),
                            r => r.Key("80's").From("1980-01-01").To("1989-12-31"),
                            r => r.Key("90's").From("1990-01-01").To("1999-12-31"),
                            r => r.Key("2000's").From("2000-01-01").To("2009-12-31"),
                            r => r.Key("2010's").From("2010-01-01").To("2019-12-31"),
                            r => r.Key("2020's").From("2020-01-01").To("2029-12-31")
                        )
                    )
                )
                .Size(0)
            );
            var decades = movies.Aggregations.DateRange("by_decade");

            var buckets = new List<ChartKeyValue>();

            foreach(var dec in decades.Buckets)
            {
                var bucket = new ChartKeyValue()
                {
                    Key = dec.Key,
                    Value = dec.DocCount
                };
                buckets.Add(bucket);
            }
            return buckets;
        }

        public IEnumerable<Movie> GetTop50MoviesByRevenue()
        {
            var movies = _elastic.Search<Movie>(m => m.Index("movie")
            .Sort(
                s => s
                .Field(f => f
                    .Field(p => p.Revenue)
                    .Order(SortOrder.Descending)
                )
            )
            .Size(50)
            );
            return movies.Documents;
        }

        public IEnumerable<ChartKeyValue> GetMoviesCountByRating()
        {
            var movies = _elastic.Search<Movie>(m => m.Index("movie")
                .Aggregations(ag => ag
                    .Histogram("by_rating", dr => dr
                        .Field(p => p.VoteAverage)
                        .Interval(0.1)
                    )
                )
                .Size(0)
            );
            var ratings = movies.Aggregations.Histogram("by_rating");

            var buckets = new List<ChartKeyValue>();

            foreach (var dec in ratings.Buckets)
            {
                var bucket = new ChartKeyValue()
                {
                    Key = dec.Key.ToString("N1"),
                    Value = dec.DocCount ?? default(long)
                };
                buckets.Add(bucket);
            }
            return buckets;
        }

        public IEnumerable<MovieSearchResult> SearchMovies(MovieSearchFilter sf)
        {
            var movies = _elastic.Search<Movie>(m => m.Index("movie")
                .Query(q => q
                    .Bool(b => b
                        .Must(m => m
                            .Wildcard(w1 => w1
                                .Field("title")
                                .Value($"*{sf.MovieName ?? ""}*".ToLower())
                            ),
                            m => m
                            .Wildcard(w2 => w2
                                .Field("cast.name")
                                .Value($"*{sf.CastName ?? ""}*".ToLower())
                            ),
                            m => m
                            .Wildcard(w3 => w3
                                .Field("genres.name")
                                .Value($"*{sf.GenreName ?? ""}*".ToLower())
                            )
                        )
                    )
                )
                .Size(50)
            );

            var res = new List<MovieSearchResult>();
            foreach(var fld in movies.Documents)
            {
                var r = new MovieSearchResult()
                {
                    Id = fld.Id,
                    Title = fld.Title,
                    PosterPath = fld.PosterPath,
                    Genres = String.Join(", ", fld.Genres.Where(g => g.Name.Contains(sf.GenreName ?? "")).Select(s => s.Name).Take(3).ToArray()),
                    Cast = String.Join(", ", fld.Cast.Where(g => g.Name.ToLower().Contains(sf.CastName ?? "")).Select(s => s.Name).Take(5).ToArray()),
                    ReleaseDate = fld.ReleaseDate,
                    VoteAverage = fld.VoteAverage
                };
                
                res.Add(r);
            }
            
            return res;
        }
    }
}


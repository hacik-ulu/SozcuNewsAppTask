namespace NewsApp.WebUI.Dto
{
    public sealed class NewsAppDto
    {
        public string? Title { get; set; }
        public string? Content { get; set; }
        public string? Summary { get; set; }
        public string? NewsUrl { get; set; }
        public string? ImageUrl { get; set; }
        public string? Category { get; set; }
        public string? Author { get; set; }
        public DateTime Date { get; set; }

        [Nest.Ignore]
        public string? Id { get; set; } // Elasticsearch id alanı
    }
}

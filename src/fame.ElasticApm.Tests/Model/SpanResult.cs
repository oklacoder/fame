namespace fame.ElasticApm.Tests
{
    public class SpanResult
    {
        public Span span { get; set; }
        public Transaction transaction { get; set; }
        public Parent parent { get; set; }
    }

}

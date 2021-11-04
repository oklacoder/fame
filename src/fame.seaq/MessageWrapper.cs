using seaq;

namespace fame.seaq
{
    public partial class SeaqPlugin
    {
        public class MessageWrapper :
            BaseDocument
        {
            public object Message { get; set; }
            public string MessageType { get; set; }

            public override string Id { get; set; }
            public override string IndexName { get; set; }
            public override string Type { get; set; }

            public T GetObjectAsMessage<T>()
                where T : class, IMessage
            {
                var str = System.Text.Json.JsonSerializer.Serialize(Message);
                var obj = System.Text.Json.JsonSerializer.Deserialize<T>(str, SeaqPlugin.SerializerOptions);
                return obj;
            }

            public MessageWrapper()
            {

            }

            public MessageWrapper(
                IMessage msg)
            {
                Message = msg;
                MessageType = msg.GetType().FullName;
                Id = msg.RefId.ToString();
                IndexName = string.Join("_", GetType().Name.ToLowerInvariant(), MessageType.ToLowerInvariant());
                Type = GetType().FullName;
            }
        }
    }


}

namespace Binner.Model
{
    public class MessageState
    {
        /// <summary>
        /// Primary key
        /// </summary>
        public int MessageStateId { get; set; }

        /// <summary>
        /// The unique message id
        /// </summary>
        public Guid MessageId { get; set; }

        /// <summary>
        /// True if the message has been read by the user, false otherwise
        /// </summary>
        public DateTime? ReadDateUtc { get; set; }

        /// <summary>
        /// Creation date
        /// </summary>
        public DateTime DateCreatedUtc { get; set; }

        /// <summary>
        /// The title returrned from the server
        /// </summary>
        public string? Title { get; set; }

        /// <summary>
        /// The message returned from the server
        /// </summary>
        public string? Message { get; set; }
    }
}

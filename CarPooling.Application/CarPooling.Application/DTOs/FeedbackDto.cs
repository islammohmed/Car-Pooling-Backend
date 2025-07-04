namespace CarPooling.Application.DTOs
{
    public class CreateFeedbackDto
    {
        public string Comment { get; set; } = string.Empty;
        public int Rating { get; set; }
        public int TripId { get; set; }
        public string ReceiverId { get; set; } = string.Empty;
    }

    public class FeedbackResponseDto
    {
        public int Id { get; set; }
        public string Comment { get; set; } = string.Empty;
        public int Rating { get; set; }
        public int TripId { get; set; }
        public string SenderId { get; set; } = string.Empty;
        public string ReceiverId { get; set; } = string.Empty;
        public string SenderName { get; set; } = string.Empty;
        public string ReceiverName { get; set; } = string.Empty;
        public string TripSource { get; set; } = string.Empty;
        public string TripDestination { get; set; } = string.Empty;
        public DateTime TripStartTime { get; set; }
    }

    public class UserFeedbackSummaryDto
    {
        public string UserId { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public double AverageRating { get; set; }
        public int TotalFeedbacks { get; set; }
        public List<FeedbackResponseDto> RecentFeedbacks { get; set; } = new();
    }
} 
using CarPooling.Data;
using CarPooling.Domain.Entities;
using CarPooling.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace CarPooling.Infrastructure.Seeders
{
    internal class Seeder : ISeeder
    {
        private readonly AppDbContext _dbContext;

        public Seeder(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task Seed()
        {
            if (await _dbContext.Users.AnyAsync())
                return;

            // Create Users
            var user1 = new User
            {
                Id = "user-1",
                UserName = "driver1@example.com",
                Email = "driver1@example.com",
                FirstName = "Ahmed",
                LastName = "Ali",
                UserRole = UserRole.Driver,
                SSN = "12345678901234",
                IsVerified = true,
                ConfirmNumber = "123456",
                Gender = Gender.Male,
                DrivingLicenseImage = "license1.jpg",
                NationalIdImage = "nationalid1.jpg"
            };

            var user2 = new User
            {
                Id = "user-2",
                UserName = "rider1@example.com",
                Email = "rider1@example.com",
                FirstName = "Mona",
                LastName = "Ibrahim",
                UserRole = UserRole.Passenger,
                SSN = "98765432109876",
                IsVerified = true,
                Gender = Gender.Female,
                ConfirmNumber = "654321",
                NationalIdImage = "nationalid2.jpg"
            };

            // Additional users
            var user3 = new User
            {
                Id = "user-3",
                UserName = "driver2@example.com",
                Email = "driver2@example.com",
                FirstName = "Mohammed",
                LastName = "Hassan",
                UserRole = UserRole.Driver,
                SSN = "11122233344455",
                IsVerified = true,
                Gender = Gender.Male,
                ConfirmNumber = "789012",
                DrivingLicenseImage = "license2.jpg",
                NationalIdImage = "nationalid3.jpg"
            };

            var user4 = new User
            {
                Id = "user-4",
                UserName = "rider2@example.com",
                Email = "rider2@example.com",
                FirstName = "Sara",
                LastName = "Ahmed",
                Gender = Gender.Female,
                UserRole = UserRole.Passenger,
                SSN = "22233344455566",
                IsVerified = true,
                ConfirmNumber = "345678",
                NationalIdImage = "nationalid4.jpg"
            };

            var user5 = new User
            {
                Id = "user-5",
                UserName = "admin@example.com",
                Email = "admin@example.com",
                FirstName = "Admin",
                LastName = "User",
                Gender = Gender.Female,
                UserRole = UserRole.Admin,
                SSN = "99988877766655",
                IsVerified = true,
                ConfirmNumber = "111222",
                NationalIdImage = "nationalid5.jpg"
            };

            // Create Cars
            var car1 = new Car
            {
                Model = "Toyota Corolla",
                Color = "White",
                DriverId = user1.Id,
                PlateNumber = "ABC123"
            };

            var car2 = new Car
            {
                Model = "Honda Civic",
                Color = "Black",
                DriverId = user1.Id,
                PlateNumber = "XYZ789"
            };

            var car3 = new Car
            {
                Model = "Hyundai Elantra",
                Color = "Silver",
                DriverId = user3.Id,
                PlateNumber = "DEF456"
            };

            // Create Trips
            var trip1 = new Trip
            {
                DriverId = user1.Id,
                SourceLocation = "Cairo",
                Destination = "Alexandria",
                PricePerSeat = 150,
                AvailableSeats = 4,
                StartTime = DateTime.UtcNow.AddDays(1),
                Status = TripStatus.Confirmed,
                Created_At = DateTime.UtcNow,
                EstimatedDuration = TimeSpan.FromHours(3),
                Notes = "No smoking please",
                GenderPreference = Gender.Female,  
                TripDescription = "A comfortable ride from Cairo to Alexandria."
            };

            var trip2 = new Trip
            {
                DriverId = user1.Id,
                SourceLocation = "Cairo",
                Destination = "Giza",
                PricePerSeat = 100,
                AvailableSeats = 3,
                StartTime = DateTime.UtcNow.AddDays(2),
                Status = TripStatus.Confirmed,
                Created_At = DateTime.UtcNow,
                EstimatedDuration = TimeSpan.FromHours(2),
                Notes = "No pets allowed please",
                GenderPreference = Gender.Female,
                TripDescription = "A comfortable ride from Cairo to Giza."
            };

            var trip3 = new Trip
            {
                DriverId = user3.Id,
                SourceLocation = "Cairo",
                Destination = "Aswan",
                PricePerSeat = 500,
                AvailableSeats = 2,
                StartTime = DateTime.UtcNow.AddDays(3),
                Status = TripStatus.Pending,
                Created_At = DateTime.UtcNow.AddDays(-1),
                EstimatedDuration = TimeSpan.FromHours(10),
                Notes = "Long journey, bring snacks",
                GenderPreference = Gender.Female,
                TripDescription = "Road trip to Aswan with stops at interesting locations."
            };

            var trip4 = new Trip
            {
                DriverId = user1.Id,
                SourceLocation = "Alexandria",
                Destination = "Cairo",
                PricePerSeat = 150,
                AvailableSeats = 3,
                StartTime = DateTime.UtcNow.AddDays(5),
                Status = TripStatus.Confirmed,
                Created_At = DateTime.UtcNow,
                EstimatedDuration = TimeSpan.FromHours(3.5),
                Notes = "Return trip",
                GenderPreference = Gender.Female,
                TripDescription = "Return trip from Alexandria to Cairo."
            };

            var trip5 = new Trip
            {
                DriverId = user3.Id,
                SourceLocation = "Cairo",
                Destination = "Hurghada",
                PricePerSeat = 400,
                AvailableSeats = 4,
                StartTime = DateTime.UtcNow.AddDays(-2),
                Status = TripStatus.Completed,
                Created_At = DateTime.UtcNow.AddDays(-5),
                EstimatedDuration = TimeSpan.FromHours(6),
                Notes = "Beach trip",
                GenderPreference = Gender.Female,
                TripDescription = "Weekend getaway to Hurghada."
            };

            // Save users, cars and trips first to get IDs
            await _dbContext.Users.AddRangeAsync(user1, user2, user3, user4, user5);
            await _dbContext.Cars.AddRangeAsync(car1, car2, car3);
            await _dbContext.Trips.AddRangeAsync(trip1, trip2, trip3, trip4, trip5);
            await _dbContext.SaveChangesAsync();

            // Create Trip Participants
            var participant1 = new TripParticipant
            {
                TripId = trip1.Id,
                UserId = user2.Id,
                SeatCount = 1,
                Status = JoinStatus.Confirmed,
                JoinedAt = DateTime.UtcNow
            };

            var participant2 = new TripParticipant
            {
                TripId = trip2.Id,
                UserId = user4.Id,
                SeatCount = 2,
                Status = JoinStatus.Pending,
                JoinedAt = DateTime.UtcNow
            };

            var participant3 = new TripParticipant
            {
                TripId = trip3.Id,
                UserId = user2.Id,
                SeatCount = 1,
                Status = JoinStatus.Pending,
                JoinedAt = DateTime.UtcNow.AddDays(-1)
            };

            var participant4 = new TripParticipant
            {
                TripId = trip5.Id,
                UserId = user4.Id,
                SeatCount = 2,
                Status = JoinStatus.Cancelled,
                JoinedAt = DateTime.UtcNow.AddDays(-5)
            };

            var participant5 = new TripParticipant
            {
                TripId = trip5.Id,
                UserId = user2.Id,
                SeatCount = 1,
                Status = JoinStatus.Confirmed,
                JoinedAt = DateTime.UtcNow.AddDays(-5)
            };

            // Create Payments
            var payment1 = new Payment
            {
                TripId = trip1.Id,
                PayerId = user2.Id,
                ReceiveId = user1.Id,
                Amount = 150,
                TransactionDate = DateTime.UtcNow,
                PaymentStatus = "Completed",
                PaymentType = "Cash",
                TransactionRef = "PAY123"
            };

            var payment2 = new Payment
            {
                TripId = trip5.Id,
                PayerId = user4.Id,
                ReceiveId = user3.Id,
                Amount = 800,
                TransactionDate = DateTime.UtcNow.AddDays(-3),
                PaymentStatus = "Completed",
                PaymentType = "Online",
                TransactionRef = "PAY456"
            };

            var payment3 = new Payment
            {
                TripId = trip5.Id,
                PayerId = user2.Id,
                ReceiveId = user3.Id,
                Amount = 400,
                TransactionDate = DateTime.UtcNow.AddDays(-3),
                PaymentStatus = "Completed",
                PaymentType = "Online",
                TransactionRef = "PAY789"
            };

            // Create Feedback entries
            var feedback1 = new Feedback
            {
                TripId = trip1.Id,
                SenderId = user2.Id,
                ReceiverId = user1.Id,
                Rating = 5,
                Comment = "Great ride!"
            };

            var feedback2 = new Feedback
            {
                TripId = trip1.Id,
                SenderId = user1.Id,
                ReceiverId = user2.Id,
                Rating = 4,
                Comment = "Pleasant passenger."
            };

            var feedback3 = new Feedback
            {
                TripId = trip5.Id,
                SenderId = user4.Id,
                ReceiverId = user3.Id,
                Rating = 3,
                Comment = "Good driver but car was a bit messy."
            };

            var feedback4 = new Feedback
            {
                TripId = trip5.Id,
                SenderId = user3.Id,
                ReceiverId = user4.Id,
                Rating = 5,
                Comment = "Great passengers, very punctual."
            };

            // Create Chat messages with IsRead property
            var chat1 = new Chat
            {
                TripId = trip1.Id,
                SenderId = user2.Id,
                ReceiverId = user1.Id,
                Message = "Hi, what time exactly will we depart?",
                SentAt = DateTime.UtcNow.AddDays(-1),
                IsRead = true
            };

            var chat2 = new Chat
            {
                TripId = trip1.Id,
                SenderId = user1.Id,
                ReceiverId = user2.Id,
                Message = "We'll depart at 9:00 AM sharp.",
                SentAt = DateTime.UtcNow.AddDays(-1).AddHours(1),
                IsRead = true
            };

            var chat3 = new Chat
            {
                TripId = trip3.Id,
                SenderId = user2.Id,
                ReceiverId = user3.Id,
                Message = "Can I bring a small suitcase?",
                SentAt = DateTime.UtcNow.AddHours(-12),
                IsRead = false
            };

            // Create Delivery Requests with Weight property
            var delivery1 = new DeliveryRequest
            {
                SenderId = user2.Id,
                ReceiverPhone = "01012345678",
                SourceLocation = "Cairo",
                DropoffLocation = "Alexandria",
                ItemDescription = "Small package, 2kg",
                Weight = 2.0f,
                Price = 100,
                Status = "Pending",
                TripId = trip1.Id
            };

            var delivery2 = new DeliveryRequest
            {
                SenderId = user4.Id,
                ReceiverPhone = "01098765432",
                SourceLocation = "Cairo",
                DropoffLocation = "Hurghada",
                ItemDescription = "Documents envelope",
                Weight = 0.5f,
                Price = 150,
                Status = "Completed",
                TripId = trip5.Id
            };

            // Add document verification
            var verification1 = new DocumentVerification
            {
                UserId = user1.Id,
                AdminId = user5.Id,
                DocumentType = "Driving License",
                VerifiedAt = DateTime.UtcNow.AddDays(-8),
                VerificationStatus = VerificationStatus.Approved
            };

            var verification2 = new DocumentVerification
            {
                UserId = user3.Id,
                AdminId = user5.Id, 
                DocumentType= "Driving License",
                VerifiedAt = DateTime.UtcNow.AddDays(-5),
                VerificationStatus =VerificationStatus.Approved
            };

            // Save all related entities
            await _dbContext.TripParticipants.AddRangeAsync(participant1, participant2, participant3, participant4, participant5);
            await _dbContext.Payments.AddRangeAsync(payment1, payment2, payment3);
            await _dbContext.Feedbacks.AddRangeAsync(feedback1, feedback2, feedback3, feedback4);
            await _dbContext.Chats.AddRangeAsync(chat1, chat2, chat3);
            await _dbContext.DeliveryRequests.AddRangeAsync(delivery1, delivery2);
            await _dbContext.DocumentVerifications.AddRangeAsync(verification1, verification2);

            await _dbContext.SaveChangesAsync();
        }
    }
}
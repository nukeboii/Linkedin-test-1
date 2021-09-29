using LandonHotel.Data;
using LandonHotel.Repositories;
using LandonHotel.Services;
using Moq;
using System;
using Xunit;

namespace LandonHotel.Test
{
    public class BookingServiceTest
    {
        private Mock<IRoomsRepository> _roomRepo;
        private Mock<ICouponRepository> couponRepo;
        /// <summary>
        /// Constructor
        /// </summary>
        public BookingServiceTest()
        {
            _roomRepo = new Mock<IRoomsRepository>();
            couponRepo = new Mock<ICouponRepository>();
            _roomRepo.Setup(r => r.GetRoom(1)).Returns(new Room());
        }

        private BookingService GetService()
        {
            return new BookingService(_roomRepo.Object, couponRepo.Object);
        }
        [Fact]
        public void IsBookingValid_NonSmoker_Valid()
        {
            var service = GetService();
            bool isValid = service.IsBookingValid(1, new Booking() { IsSmoking = false });

            Assert.True(isValid);
        }
        [Fact]
        public void IsBookingValid_Smoker_Invalid()
        {
            var service = GetService();
            bool isValid = service.IsBookingValid(1, new Booking() { IsSmoking = true });

            Assert.False(isValid);
        }

        [Fact]
        public void IsBookingValid_PetsNotAllowed_Invalid()
        {
            var service = GetService();
            _roomRepo.Setup(r => r.GetRoom(1)).Returns(new Room() { ArePetsAllowed = false });
            var isValid = service.IsBookingValid(1, new Booking { HasPets = true });

            Assert.False(isValid);
        }
        [Fact]
        public void IsBookingValid_PetsAllowed_IsValid()
        {
            var service = GetService();
            _roomRepo.Setup(r => r.GetRoom(1)).Returns(new Room { ArePetsAllowed = true });

            var isValid = service.IsBookingValid(1, new Booking { HasPets = true });

            Assert.True(isValid);
        }

        [Fact]
        public void IsBookingValid_NoPetsAllowed_IsValid()
        {
            var service = GetService();
            _roomRepo.Setup(r => r.GetRoom(1)).Returns(new Room { ArePetsAllowed = true });

            var isValid = service.IsBookingValid(1, new Booking { HasPets = false });

            Assert.True(isValid);
        }

        [Fact]
        public void IsBookingValid_NoPetsNotAllowed_IsValid()
        {
            var service = GetService();
            _roomRepo.Setup(r => r.GetRoom(1)).Returns(new Room { ArePetsAllowed = false });

            var isValid = service.IsBookingValid(1, new Booking { HasPets = false });

            Assert.True(isValid);
        }

        [Theory]
        [InlineData(false, true, false)]
        [InlineData(false, false, true)]
        [InlineData(true, true, true)]
        [InlineData(true, false, true)]
        public void IsBookingValid_Pets(bool areAllowed, bool hasPets, bool result)
        {
            var service = GetService();
            _roomRepo.Setup(r => r.GetRoom(1)).Returns(new Room { ArePetsAllowed = areAllowed });

            var isValid = service.IsBookingValid(1, new Booking { HasPets = hasPets });

            Assert.Equal(isValid, result);
        }

        [Fact]
        public void IsBookingValid_GuestsLessThanCapacity_Valid()
        {
            var service = GetService();
            _roomRepo.Setup(r => r.GetRoom(1)).Returns(new Room { Capacity = 6 });

            var isValid = service.IsBookingValid(1, new Booking { NumberOfGuests = 5 });

            Assert.True(isValid);
        }

        [Fact]
        public void IsBookingValid_GuestsGreaterThanCapactiy_Invalid()
        {
            var service = GetService();
            _roomRepo.Setup(r => r.GetRoom(1)).Returns(new Room { Capacity = 10 });

            var isValid = service.IsBookingValid(1, new Booking { NumberOfGuests = 11 });

            Assert.False(isValid);
        }

        [Fact]
        public void CalculateBookingPrice_CalculatesCorrectly()
        {
            var service = GetService();

            _roomRepo.Setup(r => r.GetRoom(1)).Returns(new Room { Rate = 250 });

            var price = service.CalculateBookingPrice(new Booking { RoomId = 1, CheckInDate = DateTime.Now, CheckOutDate = DateTime.Now.AddDays(2) });

            Assert.Equal(500, price);
        }

        [Fact]
        public void CalculateBookingPrice_DiscountsCouponCode()
        {
            var service = GetService();

            _roomRepo.Setup(r => r.GetRoom(1)).Returns(new Room { Rate = 250 });
            couponRepo.Setup(r => r.GetCoupon("10OFF")).Returns(new Coupon() { PercentageDiscount = 10 });

            var price = service.CalculateBookingPrice(new Booking { RoomId = 1, CheckInDate = DateTime.Now, CheckOutDate = DateTime.Now.AddDays(2), CouponCode = "10OFF" });

            Assert.Equal(450, price);
        }


        [Fact]
        public void CalculateBookingPrice_CalculatesCorrectly_WithEmptyCouponCode()
        {
            var service = GetService();

            _roomRepo.Setup(r => r.GetRoom(1)).Returns(new Room { Rate = 250 });

            var price = service.CalculateBookingPrice(new Booking
            {
                RoomId = 1,
                CheckInDate = DateTime.Now,
                CheckOutDate = DateTime.Now.AddDays(2),
                CouponCode = ""
            });

            Assert.Equal(500, price);
        }
    }
}

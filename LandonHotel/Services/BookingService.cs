using LandonHotel.Data;
using LandonHotel.Repositories;

namespace LandonHotel.Services
{
    public class BookingService : IBookingService
    {
        private readonly IRoomsRepository _roomsRepo;
        private readonly ICouponRepository _couponRepo;
        //private readonly IBookingsRepository _bookingsRepo;
        public BookingService(IRoomsRepository roomsRepo, ICouponRepository couponRepo)
        {
            _roomsRepo = roomsRepo;
            //_bookingsRepo = bookingsRepo;
            _couponRepo = couponRepo;
        }
        public bool IsBookingValid(int roomId, Booking booking)
        {
            var guestIsSmoking = booking.IsSmoking;
            var guestBringingPets = booking.HasPets;
            var numberOfGuests = booking.NumberOfGuests;
            if (guestIsSmoking)
                return false;
            var room = _roomsRepo.GetRoom(roomId);

            if (guestBringingPets && !room.ArePetsAllowed)
            {
                return false;
            }

            if (numberOfGuests > room.Capacity)
            {
                return false;
            }
            return true;
        }
        public decimal CalculateBookingPrice(Booking booking)
        {
            var room = _roomsRepo.GetRoom(booking.RoomId);

            var numberOfNights = (booking.CheckOutDate - booking.CheckInDate).Days;
            var price = room.Rate * numberOfNights;

            if (!string.IsNullOrEmpty(booking.CouponCode))
            {
                var discount = _couponRepo.GetCoupon(booking.CouponCode).PercentageDiscount;
                price = price - (price * discount / 100);
            }

            return price;
        }
    }
}

using System;
using System.Numerics;

namespace RSA
{
    public static class BigIntegerExtensions
    {

        private static Random random = new Random();

        // Sinh số ngẫu nhiên trong khoảng [min, max)
        public static BigInteger RandomInRange(BigInteger min, BigInteger max, Random random)
        {
            if (min >= max)
                throw new ArgumentOutOfRangeException(nameof(min), "min must be less than max");

            BigInteger range = max - min;
            byte[] bytes = range.ToByteArray();
            BigInteger result;

            do
            {
                random.NextBytes(bytes); // Sinh số ngẫu nhiên vào mảng bytes
                bytes[bytes.Length - 1] &= (byte)0x7F; // Xóa bit dấu để có giá trị không âm
                result = new BigInteger(bytes); // Chuyển đổi mảng bytes thành số BigInteger
            } while (result >= range); // Nếu số sinh ra lớn hơn range, sinh lại

            return result + min;
        }

        // Tính căn bậc hai của một số BigInteger
        public static BigInteger Sqrt(this BigInteger value)
        {
            if (value < 0)
                throw new ArgumentException("Sqrt cannot be calculated for negative numbers");

            if (value == 0 || value == 1)
                return value;

            BigInteger start = 0;
            BigInteger end = value;
            BigInteger mid;

            while (start <= end)
            {
                mid = (start + end) / 2;
                BigInteger square = mid * mid;

                if (square == value)
                    return mid;

                if (square < value)
                    start = mid + 1;
                else
                    end = mid - 1;
            }

            return end;
        }
    }
}
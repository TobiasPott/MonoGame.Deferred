﻿//using Microsoft.Xna.Framework;
//using System.Diagnostics;
//using System.Globalization;
//using System.Text;

using Microsoft.Xna.Framework;
using System.Diagnostics;
using System.Text;

namespace DeferredEngine.Utilities
{

    public static class StringBuilderExtensions
    {
        // These digits are here in a static array to support hex with simple, easily-understandable code. 
        // Since A-Z don't sit next to 0-9 in the ascii table.
        private static readonly char[] ms_digits = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F' };

        private static readonly uint ms_default_decimal_places = 5; //< Matches standard .NET formatting dp's
        private static readonly char ms_default_pad_char = '0';

        //! Convert a given unsigned integer value to a string and concatenate onto the stringbuilder. Any base value allowed.
        public static StringBuilder Concat(this StringBuilder string_builder, uint uint_val, uint pad_amount, char pad_char, uint base_val)
        {
            Debug.Assert(pad_amount >= 0);
            Debug.Assert(base_val > 0 && base_val <= 16);

            // Calculate length of integer when written out
            uint length = 0;
            uint length_calc = uint_val;

            do
            {
                length_calc /= base_val;
                length++;
            }
            while (length_calc > 0);

            // Pad out space for writing.
            string_builder.Append(pad_char, (int)Math.Max(pad_amount, length));

            int strpos = string_builder.Length;

            // We're writing backwards, one character at a time.
            while (length > 0)
            {
                strpos--;

                // Lookup from static char array, to cover hex values too
                string_builder[strpos] = ms_digits[uint_val % base_val];

                uint_val /= base_val;
                length--;
            }

            return string_builder;
        }


        public static StringBuilder AppendColor(this StringBuilder string_builder, Color color)
        {
            const string r = "r:";
            const string g = " g:";
            const string b = " b:";

            string_builder.Append(r);
            string_builder.Concat(color.R);
            string_builder.Append(g);
            string_builder.Concat(color.G);
            string_builder.Append(b);
            string_builder.Concat(color.B);
            return string_builder;
        }

        public static StringBuilder AppendVector3(this StringBuilder string_builder, Vector3 v3)
        {
            const string r = "x:";
            const string g = " y:";
            const string b = " z:";

            string_builder.Append(r);
            string_builder.Concat(v3.X);
            string_builder.Append(g);
            string_builder.Concat(v3.Y);
            string_builder.Append(b);
            string_builder.Concat(v3.Z);
            return string_builder;
        }

        //! Convert a given unsigned integer value to a string and concatenate onto the stringbuilder. Assume no padding and base ten.
        public static StringBuilder Concat(this StringBuilder string_builder, uint uint_val)
        {
            string_builder.Concat(uint_val, 0, ms_default_pad_char, 10);
            return string_builder;
        }

        //! Convert a given unsigned integer value to a string and concatenate onto the stringbuilder. Assume base ten.
        public static StringBuilder Concat(this StringBuilder string_builder, uint uint_val, uint pad_amount)
        {
            string_builder.Concat(uint_val, pad_amount, ms_default_pad_char, 10);
            return string_builder;
        }

        //! Convert a given unsigned integer value to a string and concatenate onto the stringbuilder. Assume base ten.
        public static StringBuilder Concat(this StringBuilder string_builder, uint uint_val, uint pad_amount, char pad_char)
        {
            string_builder.Concat(uint_val, pad_amount, pad_char, 10);
            return string_builder;
        }

        //! Convert a given signed integer value to a string and concatenate onto the stringbuilder. Any base value allowed.
        public static StringBuilder Concat(this StringBuilder string_builder, int int_val, uint pad_amount, char pad_char, uint base_val)
        {
            Debug.Assert(pad_amount >= 0);
            Debug.Assert(base_val > 0 && base_val <= 16);

            // Deal with negative numbers
            if (int_val < 0)
            {
                string_builder.Append('-');
                uint uint_val = uint.MaxValue - ((uint)int_val) + 1; //< This is to deal with Int32.MinValue
                string_builder.Concat(uint_val, pad_amount, pad_char, base_val);
            }
            else
            {
                string_builder.Concat((uint)int_val, pad_amount, pad_char, base_val);
            }

            return string_builder;
        }

        //! Convert a given signed integer value to a string and concatenate onto the stringbuilder. Assume no padding and base ten.
        public static StringBuilder Concat(this StringBuilder string_builder, int int_val)
        {
            string_builder.Concat(int_val, 0, ms_default_pad_char, 10);
            return string_builder;
        }

        //! Convert a given signed integer value to a string and concatenate onto the stringbuilder. Assume base ten.
        public static StringBuilder Concat(this StringBuilder string_builder, int int_val, uint pad_amount)
        {
            string_builder.Concat(int_val, pad_amount, ms_default_pad_char, 10);
            return string_builder;
        }

        //! Convert a given signed integer value to a string and concatenate onto the stringbuilder. Assume base ten.
        public static StringBuilder Concat(this StringBuilder string_builder, int int_val, uint pad_amount, char pad_char)
        {
            string_builder.Concat(int_val, pad_amount, pad_char, 10);
            return string_builder;
        }

        //! Convert a given float value to a string and concatenate onto the stringbuilder
        public static StringBuilder Concat(this StringBuilder string_builder, float float_val, uint decimal_places, uint pad_amount, char pad_char)
        {
            Debug.Assert(pad_amount >= 0);

            if (float_val < 0)
                string_builder.Append('-');

            if (decimal_places == 0)
            {
                // No decimal places, just round up and print it as an int

                // Agh, Math.Floor() just works on doubles/decimals. Don't want to cast! Let's do this the old-fashioned way.
                int int_val;
                if (float_val >= 0.0f)
                {
                    // Round up
                    int_val = (int)(float_val + 0.5f);
                }
                else
                {
                    // Round down for negative numbers
                    int_val = (int)(float_val - 0.5f);
                }

                string_builder.Concat(int_val, pad_amount, pad_char, 10);
            }
            else
            {
                int int_part = (int)Math.Abs(float_val);

                // First part is easy, just cast to an integer
                string_builder.Concat(int_part, pad_amount, pad_char, 10);

                // Decimal point
                string_builder.Append('.');

                // Work out remainder we need to print after the d.p.
                float remainder = Math.Abs(float_val - int_part);

                // Multiply up to become an int that we can print
                do
                {
                    remainder *= 10;
                    decimal_places--;

                    if (remainder < 1)
                        string_builder.Concat((uint)0, 0, '0', 10);

                }
                while (decimal_places > 0);

                // Round up. It's guaranteed to be a positive number, so no extra work required here.
                remainder += 0.5f;

                // All done, print that as an int!
                string_builder.Concat((uint)remainder, 0, '0', 10);
            }
            return string_builder;
        }

        //! Convert a given float value to a string and concatenate onto the stringbuilder. Assumes five decimal places, and no padding.
        public static StringBuilder Concat(this StringBuilder string_builder, float float_val)
        {
            string_builder.Concat(float_val, ms_default_decimal_places, 0, ms_default_pad_char);
            return string_builder;
        }

        //! Convert a given float value to a string and concatenate onto the stringbuilder. Assumes no padding.
        public static StringBuilder Concat(this StringBuilder string_builder, float float_val, uint decimal_places)
        {
            string_builder.Concat(float_val, decimal_places, 0, ms_default_pad_char);
            return string_builder;
        }

        //! Convert a given float value to a string and concatenate onto the stringbuilder.
        public static StringBuilder Concat(this StringBuilder string_builder, float float_val, uint decimal_places, uint pad_amount)
        {
            string_builder.Concat(float_val, decimal_places, pad_amount, ms_default_pad_char);
            return string_builder;
        }

    }
}
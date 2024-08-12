using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;

namespace Presolver
{
    [InterpolatedStringHandler]
    public readonly struct SimpleInterpolatedStringHandler
    {
         const int DefaultBufferSize = 256;

        [ThreadStatic]
         static StringBuilder? t_buffer;

         static StringBuilder AcquireBuffer()
        {
            var buffer = t_buffer;
            if (buffer == null)
            {
                return new(DefaultBufferSize);
            }

            buffer.Clear();
            return buffer;
        }

        readonly StringBuilder _buffer;
        readonly IFormatProvider? _provider;
        readonly ICustomFormatter? _formatter;

        public SimpleInterpolatedStringHandler(int literalLength, int formattedCount)
        {
            _provider = null;
            _buffer = AcquireBuffer();
            _formatter = null;
        }

        public SimpleInterpolatedStringHandler(int literalLength, int formattedCount, IFormatProvider? provider)
        {
            _provider = provider;
            _buffer = AcquireBuffer();
            _formatter = GetCustomFormatter(provider);
        }

        public override string ToString() => _buffer.ToString();

        public string ToStringAndClear()
        {
            string result = _buffer.ToString();
            if (_buffer.Capacity <= 65536)
            {
                t_buffer = _buffer;
            }
          
            return result;
        }
        
        public void AppendTo(StringBuilder builder)
        {
            builder.Append(_buffer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AppendLiteral(string value)
        {
            _buffer.Append(value);
        }

        public void AppendFormatted(object? value, string? format = null)
        {
            if (_formatter?.Format(format, value, _provider) is string customFormatted)
            {
                _buffer.Append(customFormatted);
                return;
            }

            var result =
                value is IFormattable formattable ?
                formattable.ToString(format, _provider) :
                (value?.ToString());

            if (result is not null)
            {
                _buffer.Append(result);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AppendFormatted(string? value)
        {
            if (value is not null)
            {
                if (_formatter is null)
                {
                    _buffer.Append(value);
                }
                else if (_formatter?.Format(null, value, _provider) is string customFormatted)
                {
                    _buffer.Append(customFormatted);
                }
            }
        }

        static ICustomFormatter? GetCustomFormatter(IFormatProvider? provider)
        {
            return provider is not null && provider.GetType() != typeof(CultureInfo) ?
                provider.GetFormat(typeof(ICustomFormatter)) as ICustomFormatter : null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AppendFormatted(bool value) => _buffer.Append(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AppendFormatted(sbyte value) => _buffer.Append(value);

        public void AppendFormatted(sbyte value, string? format) => _buffer.Append(value.ToString(format));
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AppendFormatted(byte value) => _buffer.Append(value);

        public void AppendFormatted(byte value, string? format) => _buffer.Append(value.ToString(format));
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AppendFormatted(char value) => _buffer.Append(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AppendFormatted(short value) => _buffer.Append(value);

        public void AppendFormatted(short value, string? format) => _buffer.Append(value.ToString(format));
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AppendFormatted(int value) => _buffer.Append(value);

        public void AppendFormatted(int value, string? format) => _buffer.Append(value.ToString(format));
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AppendFormatted(long value) => _buffer.Append(value);

        public void AppendFormatted(long value, string? format) => _buffer.Append(value.ToString(format));
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AppendFormatted(float value) => _buffer.Append(value);

        public void AppendFormatted(float value, string? format) => _buffer.Append(value.ToString(format));
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AppendFormatted(double value) => _buffer.Append(value);

        public void AppendFormatted(double value, string? format) => _buffer.Append(value.ToString(format));
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AppendFormatted(decimal value) => _buffer.Append(value);

        public void AppendFormatted(decimal value, string? format) => _buffer.Append(value.ToString(format));
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AppendFormatted(ushort value) => _buffer.Append(value);

        public void AppendFormatted(ushort value, string? format) => _buffer.Append(value.ToString(format));
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AppendFormatted(uint value) => _buffer.Append(value);

        public void AppendFormatted(uint value, string? format) => _buffer.Append(value.ToString(format));
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AppendFormatted(ulong value) => _buffer.Append(value);

        public void AppendFormatted(ulong value, string? format) => _buffer.Append(value.ToString(format));
    }
}
using System;
using System.Collections.Generic;

namespace HexAmbientLight.Core.Services;

public class DdpPacketizer
{
    // Make configurable for unit testing and flexibility, default to protocol safe limit 480 (1440 bytes).
    internal static int MaxLedsPerPacket { get; set; } = 480;
    private const int BytesPerLed = 3; // RGB
    private const int HeaderSize = 10;
    
    // DDP Sequence should be 1-15. 0 means disable drop tracking.
    private static byte _sequence = 1;
    
    /// <summary>
    /// Splits an RGB array into valid DDP UDP packets.
    /// </summary>
    public static List<byte[]> Packetize(ReadOnlySpan<byte> rgbData)
    {
        var packets = new List<byte[]>();
        
        int totalBytes = rgbData.Length;
        if (totalBytes == 0) return packets;
        
        int currentOffset = 0;
        
        while (currentOffset < totalBytes)
        {
            int bytesToSend = Math.Min(totalBytes - currentOffset, MaxLedsPerPacket * BytesPerLed);
            bool isLast = (currentOffset + bytesToSend) >= totalBytes;
            
            byte[] packet = new byte[HeaderSize + bytesToSend];
            
            // Flags: V1 (0x40). If last packet, add PUSH flag (0x01)
            packet[0] = (byte)(0x40 | (isLast ? 0x01 : 0x00));
            // Sequence (1-15 wrapping)
            packet[1] = _sequence;
            // DataType (1 = RGB)
            packet[2] = 0x01;
            // Destination (1 = default)
            packet[3] = 0x01;
            
            // Offset in bytes (32-bit big endian)
            packet[4] = (byte)((currentOffset >> 24) & 0xFF);
            packet[5] = (byte)((currentOffset >> 16) & 0xFF);
            packet[6] = (byte)((currentOffset >> 8) & 0xFF);
            packet[7] = (byte)(currentOffset & 0xFF);
            
            // Length in bytes (16-bit big endian)
            packet[8] = (byte)((bytesToSend >> 8) & 0xFF);
            packet[9] = (byte)(bytesToSend & 0xFF);
            
            // Payload
            rgbData.Slice(currentOffset, bytesToSend).CopyTo(packet.AsSpan(HeaderSize));
            
            packets.Add(packet);
            currentOffset += bytesToSend;
        }
        
        // Increment sequence for next frame (1-15 per spec)
        _sequence++;
        if (_sequence > 15) _sequence = 1;
        
        return packets;
    }
}

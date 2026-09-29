using System;
using HexAmbientLight.Core.Services;
using Xunit;

namespace HexAmbientLight.Tests;

public class DdpPacketizerTests
{
    [Fact]
    public void Packetize_EmptyArray_ReturnsEmptyList()
    {
        var result = DdpPacketizer.Packetize(ReadOnlySpan<byte>.Empty);
        Assert.Empty(result);
    }

    [Fact]
    public void Packetize_UnderLimit_ReturnsSinglePacket()
    {
        // 10 LEDs * 3 = 30 bytes
        var data = new byte[30];
        data[0] = 255; // Red
        
        var result = DdpPacketizer.Packetize(data);
        
        Assert.Single(result);
        var packet = result[0];
        
        // 10 bytes header + 30 bytes payload
        Assert.Equal(40, packet.Length);
        
        // Push flag should be set for the last packet (0x41)
        Assert.Equal(0x41, packet[0]); 
        
        // DataType RGB
        Assert.Equal(0x01, packet[2]);
        
        // Offset 0
        Assert.Equal(0, packet[7]); 
        
        // Length 30
        Assert.Equal(30, packet[9]);
        
        // Payload content
        Assert.Equal(255, packet[10]);
    }

    [Fact]
    public void Packetize_OverLimit_SplitsCorrectly()
    {
        // 500 LEDs * 3 = 1500 bytes (Limit is 480 LEDs = 1440 bytes per packet)
        var data = new byte[1500];
        data[1499] = 255;
        
        var result = DdpPacketizer.Packetize(data);
        
        Assert.Equal(2, result.Count);
        
        var firstPacket = result[0];
        var secondPacket = result[1];
        
        // First packet checks
        Assert.Equal(1450, firstPacket.Length); // 10 + 1440
        Assert.Equal(0x40, firstPacket[0]); // No push flag
        Assert.Equal(0, firstPacket[7]); // Offset 0
        
        // Length 1440
        Assert.Equal((byte)(1440 >> 8), firstPacket[8]);
        Assert.Equal((byte)(1440 & 0xFF), firstPacket[9]);

        // Second packet checks
        Assert.Equal(70, secondPacket.Length); // 10 + 60
        Assert.Equal(0x41, secondPacket[0]); // Push flag set
        
        // Offset 1440
        Assert.Equal((byte)(1440 >> 8), secondPacket[6]);
        Assert.Equal((byte)(1440 & 0xFF), secondPacket[7]);
        
        // Length 60
        Assert.Equal(0, secondPacket[8]);
        Assert.Equal(60, secondPacket[9]);
        
        // Payload content
        Assert.Equal(255, secondPacket[69]);
    }
}

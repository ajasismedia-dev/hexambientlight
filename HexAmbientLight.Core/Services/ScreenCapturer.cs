using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Windows.Graphics.Capture;
using Windows.Graphics.DirectX;
using Windows.Graphics.DirectX.Direct3D11;
using Vortice.Direct3D11;
using Vortice.DXGI;
using Serilog;
using System.Runtime.InteropServices;
using WinRT;

namespace HexAmbientLight.Core.Services;

public class ScreenCapturer : IDisposable
{
    private ID3D11Device? _d3dDevice;
    private ID3D11DeviceContext? _d3dContext;
    private IDirect3DDevice? _winrtDevice;
    
    private GraphicsCaptureItem? _captureItem;
    private Direct3D11CaptureFramePool? _framePool;
    private GraphicsCaptureSession? _session;

    private ID3D11Texture2D? _stagingTexture;
    private int _width;
    private int _height;

    private readonly object _frameLock = new object();
    private byte[] _latestRgbaData = Array.Empty<byte>();
    private bool _isRunning;
    private int _frameCount;
    private readonly System.Diagnostics.Stopwatch _fpsStopwatch = System.Diagnostics.Stopwatch.StartNew();
    private bool _firstFrameReceived;
    public int CurrentFps { get; set; }
    
    public int Width => _width;
    public int Height => _height;

    public ScreenCapturer()
    {
        InitD3D();
    }

    private void InitD3D()
    {
        D3D11.D3D11CreateDevice(
            null,
            Vortice.Direct3D.DriverType.Hardware,
            DeviceCreationFlags.BgraSupport,
            new[] { Vortice.Direct3D.FeatureLevel.Level_11_0 },
            out ID3D11Device d3dDevice,
            out ID3D11DeviceContext d3dContext).CheckError();

        _d3dDevice = d3dDevice;
        _d3dContext = d3dContext;

        if (_d3dDevice == null) throw new InvalidOperationException("Failed to create D3D11 Device");

        var dxgiDevice = _d3dDevice.QueryInterface<IDXGIDevice>();
        
        CaptureHelper.CreateDirect3D11DeviceFromDXGIDevice(dxgiDevice.NativePointer, out IntPtr pUnknown);
        _winrtDevice = MarshalInterface<IDirect3DDevice>.FromAbi(pUnknown);
        Marshal.Release(pUnknown);
        dxgiDevice.Dispose();
    }

    private static bool _borderlessRequested = false;
    private static bool _borderlessSupported = false;
    private static bool _borderlessGranted = false;

    public async Task StartCaptureAsync(IntPtr hMonitor)
    {
        if (_isRunning) return;
        if (_winrtDevice == null) throw new InvalidOperationException("WinRT Device is not initialized.");

        try
        {
            if (!_borderlessRequested)
            {
                _borderlessRequested = true;
                _borderlessSupported = Windows.Foundation.Metadata.ApiInformation.IsMethodPresent("Windows.Graphics.Capture.GraphicsCaptureAccess", "RequestAccessAsync");
                if (_borderlessSupported)
                {
                    try
                    {
                        var access = await GraphicsCaptureAccess.RequestAccessAsync(GraphicsCaptureAccessKind.Borderless);
                        _borderlessGranted = (access == Windows.Security.Authorization.AppCapabilityAccess.AppCapabilityAccessStatus.Allowed);
                    }
                    catch (Exception ex)
                    {
                        Log.Warning(ex, "Failed to request Borderless capture access.");
                        _borderlessGranted = false;
                    }
                }
                Log.Information("BorderlessSupported: {Supported}, BorderlessAccessGranted: {Granted}", _borderlessSupported, _borderlessGranted);
            }

            _captureItem = CaptureHelper.CreateItemForMonitor(hMonitor);
            if (_captureItem == null) throw new InvalidOperationException("Failed to create CaptureItem.");
            
            _width = _captureItem.Size.Width;
            _height = _captureItem.Size.Height;
            
            CreateStagingTexture();

            _framePool = Direct3D11CaptureFramePool.CreateFreeThreaded(
                _winrtDevice,
                DirectXPixelFormat.B8G8R8A8UIntNormalized,
                1,
                _captureItem.Size);

            _framePool.FrameArrived += OnFrameArrived;

            _session = _framePool.CreateCaptureSession(_captureItem);
            _session.IsCursorCaptureEnabled = false;

            if (_borderlessGranted && Windows.Foundation.Metadata.ApiInformation.IsPropertyPresent("Windows.Graphics.Capture.GraphicsCaptureSession", "IsBorderRequired"))
            {
                _session.IsBorderRequired = false;
                Log.Information("IsBorderRequired actual value set to false.");
            }
            else
            {
                Log.Information("IsBorderRequired actual value remains true (fallback/denied).");
            }

            _session.StartCapture();

            _isRunning = true;
            Log.Information("Screen capture started for monitor. Handle: {Handle}, Resolution: {Width}x{Height}", hMonitor, _width, _height);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to start screen capture.");
            throw;
        }
    }

    private void CreateStagingTexture()
    {
        if (_d3dDevice == null) return;
        
        _stagingTexture?.Dispose();
        var desc = new Texture2DDescription
        {
            Width = (uint)_width,
            Height = (uint)_height,
            MipLevels = 1,
            ArraySize = 1,
            Format = Vortice.DXGI.Format.B8G8R8A8_UNorm,
            SampleDescription = new Vortice.DXGI.SampleDescription(1, 0),
            Usage = ResourceUsage.Staging,
            BindFlags = BindFlags.None,
            CPUAccessFlags = CpuAccessFlags.Read,
            MiscFlags = ResourceOptionFlags.None
        };
        _stagingTexture = _d3dDevice.CreateTexture2D(desc);
    }

    private void OnFrameArrived(Direct3D11CaptureFramePool sender, object args)
    {
        using var frame = sender.TryGetNextFrame();
        if (frame == null) return;

        if (_d3dContext == null || _stagingTexture == null) return;

        if (frame.ContentSize.Width != _width || frame.ContentSize.Height != _height)
        {
            _width = frame.ContentSize.Width;
            _height = frame.ContentSize.Height;
            CreateStagingTexture();
            if (_stagingTexture == null) return;
        }

        var surfaceInterop = frame.Surface.As<IDirect3DDxgiInterfaceAccess>();
        var resourceIid = typeof(ID3D11Resource).GUID;
        var resourcePtr = surfaceInterop.GetInterface(ref resourceIid);
        
        using var sourceResource = new ID3D11Resource(resourcePtr);
        
        _d3dContext.CopyResource(_stagingTexture, sourceResource);
        
        var mapped = _d3dContext.Map(_stagingTexture, 0, MapMode.Read, Vortice.Direct3D11.MapFlags.None);
        
                int bufferSize = _width * _height * 4;
        
        lock (_frameLock)
        {
            if (_latestRgbaData.Length != bufferSize)
            {
                _latestRgbaData = new byte[bufferSize];
            }

            unsafe
            {
                byte* sourcePtr = (byte*)mapped.DataPointer;
                fixed (byte* destPtr = _latestRgbaData)
                {
                    for (int y = 0; y < _height; y++)
                    {
                        Buffer.MemoryCopy(
                            sourcePtr + (y * mapped.RowPitch), 
                            destPtr + (y * _width * 4), 
                            _width * 4, 
                            _width * 4);
                    }
                }
            }
        }
        
        _d3dContext.Unmap(_stagingTexture, 0);

        _frameCount++;
        if (_fpsStopwatch.Elapsed.TotalSeconds >= 1.0)
        {
            CurrentFps = _frameCount;
            _frameCount = 0;
            _fpsStopwatch.Restart();
        }

        if (!_firstFrameReceived)
        {
            _firstFrameReceived = true;
            Log.Information("First frame received from CapturePool. Resolution: {Width}x{Height}", _width, _height);
        }
    }

            public bool TryGetLatestFrameData(byte[] buffer)
    {
        lock (_frameLock)
        {
            if (_latestRgbaData.Length == 0 || buffer.Length != _latestRgbaData.Length)
            {
                return false;
            }
            Buffer.BlockCopy(_latestRgbaData, 0, buffer, 0, _latestRgbaData.Length);
            return true;
        }
    }

    public void StopCapture()
    {
        if (!_isRunning) return;

        if (_framePool != null)
        {
            _framePool.FrameArrived -= OnFrameArrived;
            _framePool.Dispose();
            _framePool = null;
        }
        
        _session?.Dispose();
        _session = null;
        _captureItem = null;
        
        _isRunning = false;
        Log.Information("Screen capture stopped.");
    }

    public void Dispose()
    {
        StopCapture();
        _stagingTexture?.Dispose();
        _stagingTexture = null;
        _d3dContext?.Dispose();
        _d3dContext = null;
        _d3dDevice?.Dispose();
        _d3dDevice = null;
    }
}



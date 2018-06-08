/* 
 * Copyright (c) 2011-2018 FEELTeam - Maurizio Montel.
 * 
 * This file is part of the FEEL (FrontEnd - Emulator Launcher) distribution.
 *   (https://github.com/dr-prodigy/feel-frontend)
 * 
 * FEEL is free software: you can redistribute it and/or modify  
 * it under the terms of the GNU Lesser General Public License as   
 * published by the Free Software Foundation, version 3.
 *
 * FEEL is distributed in the hope that it will be useful, but 
 * WITHOUT ANY WARRANTY; without even the implied warranty of 
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU 
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License
 * along with this program. If not, see <http://www.gnu.org/licenses/>.
 *
 * doc/info/contacts: http://feelfrontend.altervista.org
 */

using System;
using System.Threading;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using DirectShowLib;

namespace feel
{
    /// <summary>
    /// Describes the state of a video player
    /// </summary>
    public enum VideoState
    {
        Playing,
        Paused,
        Stopped
    }

    /// <summary>
    /// Enables Video Playback in Microsoft XNA
    /// </summary>
    public class XNAPlayer : ISampleGrabberCB, IDisposable
    {
        #region Media Type GUIDs
        private Guid MEDIATYPE_Video = new Guid(0x73646976, 0x0000, 0x0010, 0x80, 0x00, 0x00, 0xaa, 0x00, 0x38, 0x9b, 0x71);
        private Guid MEDIASUBTYPE_RGB24 = new Guid(0xe436eb7d, 0x524f, 0x11ce, 0x9f, 0x53, 0x00, 0x20, 0xaf, 0x0b, 0xa7, 0x70);
        private Guid FORMAT_VideoInfo = new Guid(0x05589f80, 0xc356, 0x11ce, 0xbf, 0x01, 0x00, 0xaa, 0x00, 0x55, 0x59, 0x5a);
        #endregion

        #region Private Fields
        /// <summary>
        /// The Main FilterGraph Com Object
        /// </summary> 
        private FilterGraph fg = null;

        /// <summary>
        /// The GraphBuilder interface ref
        /// </summary>
        private IGraphBuilder gb = null;

        /// <summary>
        /// The MediaControl interface ref
        /// </summary>
        private IMediaControl mc = null;

        /// <summary>
        /// The MediaEvent interface ref
        /// </summary>
        private IMediaEventEx me = null;

        /// <summary>
        /// The MediaPosition interface ref
        /// </summary>
        private IMediaPosition mp = null;

        /// <summary>
        /// The MediaSeeking interface ref
        /// </summary>
        private IMediaSeeking ms = null;

        /// <summary>
        /// Thread used to update the video's Texture2D data
        /// </summary>
        private Thread updateThread;

        /// <summary>
        /// Thread used to wait untill the video is complete, then invoke the OnVideoComplete EventHandler
        /// </summary>
        private Thread waitThread;

        /// <summary>
        /// The Video File to play
        /// </summary>
        private string filename;

        /// <summary>
        /// Array to hold the raw data from the DirectShow video stream.
        /// </summary>
        private byte[] bgrData;

        /// <summary>
        /// The RGBA frame bytes used to set the data in the Texture2D Output Frame
        /// </summary>
        private byte[] videoFrameBytes;

        /// <summary>
        /// Private Video Width
        /// </summary>
        private int videoWidth = 0;

        /// <summary>
        /// Private Video Height
        /// </summary>
        private int videoHeight = 0;

        /// <summary>
        /// Private Texture2D to render video to. Created in the Video Player Constructor.
        /// </summary> 
        private Texture2D outputFrame;

        /// <summary>
        /// Average Time per Frame in milliseconds
        /// </summary>
        private long avgTimePerFrame;

        /// <summary>
        /// BitRate of the currently loaded video
        /// </summary>
        private int bitRate;

        /// <summary>
        /// Current state of the video player
        /// </summary>
        private VideoState currentState;

        /// <summary>
        /// Is Disposed?
        /// </summary>
        private bool isDisposed = false;

        /// <summary>
        /// Current time position
        /// </summary>
        private long currentPosition;

        /// <summary>
        /// Video duration
        /// </summary>
        private long videoDuration;

        /// <summary>
        /// How transparent the video frame is.
        /// Takes effect on the next frame after this is updated
        /// Max Value: 255 - Opaque
        /// Min Value: 0   - Transparent
        /// </summary>
        private byte alphaTransparency = 255;

        EventWaitHandle _wh = new AutoResetEvent(false);
        #endregion

        #region Public Properties
        /// <summary>
        /// Automatically updated video frame. Render this to the screen using a SpriteBatch.
        /// </summary>
        public Texture2D OutputFrame
        {
            get
            {
                return outputFrame;
            }
        }

        /// <summary>
        /// Width of the loaded video
        /// </summary>
        public int VideoWidth
        {
            get
            {
                return videoWidth;
            }
        }

        /// <summary>
        /// Height of the loaded video
        /// </summary>
        public int VideoHeight
        {
            get
            {
                return videoHeight;
            }
        }

        /// <summary>
        /// Gets or Sets the current position of playback in seconds
        /// </summary>
        public double CurrentPosition
        {
            get
            {
                return (double)currentPosition / 10000000;
            }
            set
            {
                if (value < 0)
                    value = 0;

                if (value > Duration)
                    value = Duration;

                DsError.ThrowExceptionForHR(mp.put_CurrentPosition(value));
                currentPosition = (long)value * 10000000;
            }
        }

        /// <summary>
        /// Returns the current position of playback, formatted as a time string (HH:MM:SS)
        /// </summary>
        public string CurrentPositionAsTimeString
        {
            get
            {
                double seconds = (double)currentPosition / 10000000;

                double minutes = seconds / 60;
                double hours = minutes / 60;

                int realHours = (int)Math.Floor(hours);
                minutes -= realHours * 60;

                int realMinutes = (int)Math.Floor(minutes);
                seconds -= realMinutes * 60;

                int realSeconds = (int)Math.Floor(seconds);

                return (realHours < 10 ? "0" + realHours.ToString() : realHours.ToString()) + ":" + (realMinutes < 10 ? "0" + realMinutes.ToString() : realMinutes.ToString()) + ":" + (realSeconds < 10 ? "0" + realSeconds.ToString() : realSeconds.ToString());
            }
        }

        /// <summary>
        /// Total duration in seconds
        /// </summary>
        public double Duration
        {
            get
            {
                return (double)videoDuration / 10000000;
            }
        }

        /// <summary>
        /// Returns the duration of the video, formatted as a time string (HH:MM:SS)
        /// </summary>
        public string DurationAsTimeString
        {
            get
            {
                double seconds = (double)videoDuration / 10000000;

                double minutes = seconds / 60;
                double hours = minutes / 60;

                int realHours = (int)Math.Floor(hours);
                minutes -= realHours * 60;

                int realMinutes = (int)Math.Floor(minutes);
                seconds -= realMinutes * 60;

                int realSeconds = (int)Math.Floor(seconds);

                return (realHours < 10 ? "0" + realHours.ToString() : realHours.ToString()) + ":" + (realMinutes < 10 ? "0" + realMinutes.ToString() : realMinutes.ToString()) + ":" + (realSeconds < 10 ? "0" + realSeconds.ToString() : realSeconds.ToString());
            }
        }

        /// <summary>
        /// Currently Loaded Video File
        /// </summary>
        public string FileName
        {
            get
            {
                return filename;
            }
        }

        /// <summary>
        /// Gets or Sets the current state of the video player
        /// </summary>
        public VideoState CurrentState
        {
            get
            {
                return currentState;
            }
            set
            {
                switch (value)
                {
                    case VideoState.Playing:
                        Play();
                        break;
                    case VideoState.Paused:
                        Pause();
                        break;
                    case VideoState.Stopped:
                        Stop();
                        break;
                }
            }
        }

        /// <summary>
        /// Event which occurs when the video stops playing once it has reached the end of the file
        /// </summary>
        public event EventHandler OnVideoComplete;

        /// <summary>
        /// Is Disposed?
        /// </summary>
        public bool IsDisposed
        {
            get
            {
                return isDisposed;
            }
        }

        /// <summary>
        /// Number of Frames Per Second in the video file.
        /// Returns -1 if this cannot be calculated.
        /// </summary>
        public int FramesPerSecond
        {
            get
            {
                if (avgTimePerFrame == 0)
                    return -1;

                float frameTime = (float)avgTimePerFrame / 10000000.0f;
                float framesPS = 1.0f / frameTime;
                return (int)Math.Round(framesPS, 0, MidpointRounding.ToEven);
            }
        }

        /// <summary>
        /// The number of milliseconds between each frame
        /// Returns -1 if this cannot be calculated
        /// </summary>
        public float MillisecondsPerFrame
        {
            get
            {
                if (avgTimePerFrame == 0)
                    return -1;

                return (float)avgTimePerFrame / 10000.0f;
            }
        }

        /// <summary>
        /// Gets or sets how transparent the video frame is.
        /// Takes effect on the next frame after this is updated
        /// Max Value: 255 - Opaque
        /// Min Value: 0   - Transparent
        /// </summary>
        public byte AlphaTransparency
        {
            get
            {
                return alphaTransparency;
            }
            set
            {
                alphaTransparency = value;
            }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new Video Player. Automatically creates the required Texture2D on the specificied GraphicsDevice.
        /// </summary>
        /// <param name="FileName">The video file to open</param>
        /// <param name="graphicsDevice">XNA Graphics Device</param>
        public XNAPlayer(Feel feel, string FileName, GraphicsDevice graphicsDevice, Action callback)
        {
            Utils.RunAsynchronously(() =>
            {
                try
                {
                    // Set video state
                    currentState = VideoState.Stopped;

                    // Store Filename
                    filename = FileName;

                    // Open DirectShow Interfaces
                    InitInterfaces();

                    // Create a SampleGrabber Filter and add it to the FilterGraph
                    SampleGrabber sg = new SampleGrabber();
                    ISampleGrabber sampleGrabber = (ISampleGrabber)sg;
                    DsError.ThrowExceptionForHR(gb.AddFilter((IBaseFilter)sg, "Grabber"));

                    // Setup Media type info for the SampleGrabber
                    AMMediaType mt = new AMMediaType();
                    mt.majorType = MEDIATYPE_Video;     // Video
                    mt.subType = MEDIASUBTYPE_RGB24;    // RGB24
                    mt.formatType = FORMAT_VideoInfo;   // VideoInfo
                    DsError.ThrowExceptionForHR(sampleGrabber.SetMediaType(mt));

                    // Construct the rest of the FilterGraph
                    DsError.ThrowExceptionForHR(gb.RenderFile(filename, null));

                    // Set SampleGrabber Properties
                    DsError.ThrowExceptionForHR(sampleGrabber.SetBufferSamples(true));
                    DsError.ThrowExceptionForHR(sampleGrabber.SetOneShot(false));
                    DsError.ThrowExceptionForHR(sampleGrabber.SetCallback((ISampleGrabberCB)this, 1));

                    // Hide Default Video Window
                    IVideoWindow pVideoWindow = (IVideoWindow)gb;
                    DsError.ThrowExceptionForHR(pVideoWindow.put_MessageDrain(IntPtr.Zero));
                    DsError.ThrowExceptionForHR(pVideoWindow.put_WindowState(WindowState.Hide));
                    DsError.ThrowExceptionForHR(pVideoWindow.put_AutoShow(OABool.False));

                    // Create AMMediaType to capture video information
                    AMMediaType MediaType = new AMMediaType();
                    DsError.ThrowExceptionForHR(sampleGrabber.GetConnectedMediaType(MediaType));
                    VideoInfoHeader pVideoHeader = new VideoInfoHeader();
                    Marshal.PtrToStructure(MediaType.formatPtr, pVideoHeader);

                    // Store video information
                    videoHeight = pVideoHeader.BmiHeader.Height;
                    videoWidth = pVideoHeader.BmiHeader.Width;
                    avgTimePerFrame = pVideoHeader.AvgTimePerFrame;
                    bitRate = pVideoHeader.BitRate;
                    DsError.ThrowExceptionForHR(ms.GetDuration(out videoDuration));

                    // Create byte arrays to hold video data
                    videoFrameBytes = new byte[(videoHeight * videoWidth) * 4]; // RGBA format (4 bytes per pixel)
                    bgrData = new byte[(videoHeight * videoWidth) * 4];         // BGR24 format (3 bytes per pixel + 1 for safety)

                    // Create Output Frame Texture2D with the height and width of the video
                    outputFrame = new Texture2D(graphicsDevice, videoWidth, videoHeight, 1, TextureUsage.None, SurfaceFormat.Color);

                    feel.RunOnUIThread(callback);
                }
                catch
                {
                    feel.ShowToast("Unable to Load or Play the video file");
                }
            }, () => { });
        }
        #endregion

        #region DirectShow Interface Management
        /// <summary>
        /// Initialises DirectShow interfaces
        /// </summary>
        private void InitInterfaces()
        {
            fg = new FilterGraph();
            gb = (IGraphBuilder)fg;
            mc = (IMediaControl)fg;
            me = (IMediaEventEx)fg;
            ms = (IMediaSeeking)fg;
            mp = (IMediaPosition)fg;
        }

        /// <summary>
        /// Closes DirectShow interfaces
        /// </summary>
        private void CloseInterfaces()
        {
            if (me != null)
            {
                DsError.ThrowExceptionForHR(mc.Stop());
                //0x00008001 = WM_GRAPHNOTIFY
                DsError.ThrowExceptionForHR(me.SetNotifyWindow(IntPtr.Zero, 0x00008001, IntPtr.Zero));
            }
            mc = null;
            me = null;
            gb = null;
            ms = null;
            mp = null;
            if (fg != null)
                Marshal.ReleaseComObject(fg);
            fg = null;
            currentState = VideoState.Stopped;
        }
        #endregion

        #region Update and Media Control
        /// <summary>
        /// Updates the Output Frame data using data from the video stream. Call this in Game.Update().
        /// </summary>
        public void Update()
        {
            // Remove the OutputFrame from the GraphicsDevice to prevent an InvalidOperationException on the SetData line.
            if (outputFrame.GraphicsDevice.Textures[0] == outputFrame)
            {
                outputFrame.GraphicsDevice.Textures[0] = null;
            }

            unsafe
            {
                // Set video data into the Output Frame
                outputFrame.SetData<byte>(videoFrameBytes);
            }

            // Update current position read-out
            DsError.ThrowExceptionForHR(ms.GetCurrentPosition(out currentPosition));
        }

        public void SetVolume(int percentage)
        {
            // convert to logarithmic scale (-10000 to 0 , i.e.: -100db to -0db)
            int volume = -10000 + (int)(10 * Math.Log(1.0d + percentage / 100.0d) * 3333.3);
            IBasicAudio iba = fg as IBasicAudio;
            if (iba != null)
                iba.put_Volume(volume);
        }

        public void SetSpeed(double rate)
        {
            if (ms != null)
                ms.SetRate(rate);
        }

        /// <summary>
        /// Starts playing the video
        /// </summary>
        public void Play()
        {
            if (currentState != VideoState.Playing && mc != null)
            {
                if (updateThread != null)
                    updateThread.Abort();

                // Create video threads
                updateThread = new Thread(new ThreadStart(UpdateBuffer));
                waitThread = new Thread(new ThreadStart(WaitForCompletion));

                // Start the FilterGraph
                DsError.ThrowExceptionForHR(mc.Run());

                // Start Threads
                updateThread.Start();
                waitThread.Start();

                // Update VideoState
                currentState = VideoState.Playing;
            }
        }

        /// <summary>
        /// Pauses the video
        /// </summary>
        public void Pause()
        {
            // End threads
            if (updateThread != null)
                updateThread.Abort();
            updateThread = null;

            if (waitThread != null)
                waitThread.Abort();
            waitThread = null;

            // Stop the FilterGraph (but remembers the current position)
            if (mc != null)
                DsError.ThrowExceptionForHR(mc.Stop());

            // Update VideoState
            currentState = VideoState.Paused;
        }

        /// <summary>
        /// Stops playing the video
        /// </summary>
        public void Stop()
        {
            // Stop the FilterGraph
            if (mc != null)
                DsError.ThrowExceptionForHR(mc.Stop());

            // Reset the current position
            if (ms != null)
                DsError.ThrowExceptionForHR(ms.SetPositions(0, AMSeekingSeekingFlags.AbsolutePositioning, 0, AMSeekingSeekingFlags.NoPositioning));

            // Update VideoState
            currentState = VideoState.Stopped;
        }

        /// <summary>
        /// Rewinds the video to the start and stops
        /// </summary>
        public void Rewind()
        {
            IMediaPosition imp = fg as IMediaPosition;
            if (imp != null)
                imp.put_CurrentPosition(0);

            // force a new play
            Stop();
        }
        #endregion

        private Object thisLock = new Object();

        #region ISampleGrabberCB Members and Helpers
        /// <summary>
        /// Required public callback from DirectShow SampleGrabber. Do not call this method.
        /// </summary>
        public int BufferCB(double SampleTime, IntPtr pBuffer, int BufferLen)
        {
            // Copy raw data into bgrData byte array
            Marshal.Copy(pBuffer, bgrData, 0, BufferLen);

            // Flag the new frame as available
            _wh.Set();

            // Return S_OK
            return 0;
        }

        /// <summary>
        /// Required public callback from DirectShow SampleGrabber. Do not call this method.
        /// </summary>
        public int SampleCB(double SampleTime, IMediaSample pSample)
        {
            // Return S_OK
            return 0;
        }

        /// <summary>
        /// Worker to copy the BGR data from the video stream into the RGBA byte array for the Output Frame.
        /// </summary>
        private void UpdateBuffer()
        {
            int waitTime = avgTimePerFrame != 0 ? (int)((float)avgTimePerFrame / 10000) : 20;

            while (true)
            {
                for (int y = 0, y2 = videoHeight - 1; y < videoHeight; y++, y2--)
                {
                    for (int x = 0; x < videoWidth; x++)
                    {
                        // XNA 4 VERSION
                        //int samplePosRGBA = (((y2 * videoWidth) + x) * 4);
                        //int samplePosBGR = ((y * videoWidth) + x) * 3 + 2;

                        //videoFrameBytes[samplePosRGBA++] = bgrData[samplePosBGR--];
                        //videoFrameBytes[samplePosRGBA++] = bgrData[samplePosBGR--];
                        //videoFrameBytes[samplePosRGBA++] = bgrData[samplePosBGR];
                        //videoFrameBytes[samplePosRGBA] = alphaTransparency;

                        int samplePosBGRA = (((y2 * videoWidth) + x) * 4);
                        int samplePosBGR = ((y * videoWidth) + x) * 3;

                        videoFrameBytes[samplePosBGRA++] = bgrData[samplePosBGR++];
                        videoFrameBytes[samplePosBGRA++] = bgrData[samplePosBGR++];
                        videoFrameBytes[samplePosBGRA++] = bgrData[samplePosBGR];
                        videoFrameBytes[samplePosBGRA] = alphaTransparency;
                    }
                }

                _wh.WaitOne();         // No more tasks - wait for a signal
            }
        }

        /// <summary>
        /// Waits for the video to finish, then calls the OnVideoComplete event
        /// </summary>
        private void WaitForCompletion()
        {
            int waitTime = avgTimePerFrame != 0 ? (int)((float)avgTimePerFrame / 10000) : 20;

            try
            {
                while (videoDuration > currentPosition)
                {
                    Thread.Sleep(waitTime);
                }

                if (OnVideoComplete != null)
                    OnVideoComplete.Invoke(this, EventArgs.Empty);
            }
            catch { }
        }
        #endregion

        #region IDisposable Members
        /// <summary>
        /// Cleans up the Video Player. Must be called when finished with the player.
        /// </summary>
        public void Dispose()
        {
            isDisposed = true;

            if (currentState != VideoState.Stopped)
                Stop();

            CloseInterfaces();

            // End Threads
            if (updateThread != null)
                updateThread.Abort();
            updateThread = null;

            if (waitThread != null)
                waitThread.Abort();
            waitThread = null;
            //outputFrame = null;
        }
        #endregion
    }
}